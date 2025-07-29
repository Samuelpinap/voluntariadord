using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Services
{
    public class AuthService : IAuthService
    {
        private readonly DbContextApplication _context;
        private readonly IJwtService _jwtService;
        private readonly IPasswordService _passwordService;

        public AuthService(DbContextApplication context, IJwtService jwtService, IPasswordService passwordService)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordService = passwordService;
        }

        public async Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest)
        {
            try
            {
                var user = await _context.Usuarios
                    .Include(u => u.Organizacion)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == loginRequest.Email.ToLower());

                if (user == null)
                {
                    return new ApiResponseDto<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Credenciales inválidas",
                        Errors = new List<string> { "Email o contraseña incorrectos" }
                    };
                }

                if (!_passwordService.VerifyPassword(loginRequest.Password, user.PasswordHash))
                {
                    return new ApiResponseDto<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Credenciales inválidas",
                        Errors = new List<string> { "Email o contraseña incorrectos" }
                    };
                }

                if (user.Estatus == UserStatus.Inactivo || user.Estatus == UserStatus.Suspendido)
                {
                    return new ApiResponseDto<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Cuenta inactiva o suspendida",
                        Errors = new List<string> { "Tu cuenta ha sido desactivada o suspendida. Contacta al administrador." }
                    };
                }

                var token = _jwtService.GenerateToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();

                var response = new LoginResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    User = MapToUserInfoDto(user)
                };

                return new ApiResponseDto<LoginResponseDto>
                {
                    Success = true,
                    Message = "Login exitoso",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<LoginResponseDto>
                {
                    Success = false,
                    Message = "Error interno del servidor",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<UserInfoDto>> RegisterVoluntarioAsync(RegisterVoluntarioDto registerDto)
        {
            try
            {
                // Validar que el email no esté en uso
                var existingUser = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == registerDto.Email.ToLower());

                if (existingUser != null)
                {
                    return new ApiResponseDto<UserInfoDto>
                    {
                        Success = false,
                        Message = "Email ya está en uso",
                        Errors = new List<string> { "Ya existe una cuenta con este email" }
                    };
                }

                // Validar fortaleza de la contraseña
                if (!_passwordService.IsPasswordStrong(registerDto.Password))
                {
                    return new ApiResponseDto<UserInfoDto>
                    {
                        Success = false,
                        Message = "Contraseña no cumple con los requisitos",
                        Errors = new List<string> { "La contraseña debe tener al menos 8 caracteres, incluir mayúsculas, minúsculas, números y símbolos" }
                    };
                }

                var user = new Usuario
                {
                    Nombre = registerDto.Nombre,
                    Apellido = registerDto.Apellido,
                    Email = registerDto.Email,
                    PasswordHash = _passwordService.HashPassword(registerDto.Password),
                    Telefono = registerDto.Telefono,
                    Direccion = registerDto.Direccion,
                    FechaNacimiento = registerDto.FechaNacimiento,
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Usuarios.Add(user);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<UserInfoDto>
                {
                    Success = true,
                    Message = "Voluntario registrado exitosamente",
                    Data = MapToUserInfoDto(user)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<UserInfoDto>
                {
                    Success = false,
                    Message = "Error al registrar usuario",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<UserInfoDto>> RegisterOrganizacionAsync(RegisterOrganizacionDto registerDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validar que el email del admin no esté en uso
                var existingUser = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == registerDto.EmailAdmin.ToLower());

                if (existingUser != null)
                {
                    return new ApiResponseDto<UserInfoDto>
                    {
                        Success = false,
                        Message = "Email del administrador ya está en uso",
                        Errors = new List<string> { "Ya existe una cuenta con este email" }
                    };
                }

                // Validar que el email de la organización no esté en uso
                var existingOrg = await _context.Organizaciones
                    .FirstOrDefaultAsync(o => o.Email.ToLower() == registerDto.EmailOrganizacion.ToLower());

                if (existingOrg != null)
                {
                    return new ApiResponseDto<UserInfoDto>
                    {
                        Success = false,
                        Message = "Email de la organización ya está en uso",
                        Errors = new List<string> { "Ya existe una organización con este email" }
                    };
                }

                // Validar fortaleza de la contraseña
                if (!_passwordService.IsPasswordStrong(registerDto.PasswordAdmin))
                {
                    return new ApiResponseDto<UserInfoDto>
                    {
                        Success = false,
                        Message = "Contraseña no cumple con los requisitos",
                        Errors = new List<string> { "La contraseña debe tener al menos 8 caracteres, incluir mayúsculas, minúsculas, números y símbolos" }
                    };
                }

                // Crear usuario administrador
                var adminUser = new Usuario
                {
                    Nombre = registerDto.NombreAdmin,
                    Apellido = registerDto.ApellidoAdmin,
                    Email = registerDto.EmailAdmin,
                    PasswordHash = _passwordService.HashPassword(registerDto.PasswordAdmin),
                    Telefono = registerDto.TelefonoAdmin,
                    FechaNacimiento = registerDto.FechaNacimientoAdmin,
                    Rol = UserRole.Organizacion,
                    Estatus = UserStatus.PendienteVerificacion,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Usuarios.Add(adminUser);
                await _context.SaveChangesAsync();

                // Crear organización
                var organizacion = new Organizacion
                {
                    Nombre = registerDto.NombreOrganizacion,
                    Descripcion = registerDto.DescripcionOrganizacion,
                    Email = registerDto.EmailOrganizacion,
                    Telefono = registerDto.TelefonoOrganizacion,
                    Direccion = registerDto.DireccionOrganizacion,
                    SitioWeb = registerDto.SitioWeb,
                    NumeroRegistro = registerDto.NumeroRegistro,
                    Estatus = OrganizacionStatus.PendienteVerificacion,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioId = adminUser.Id
                };

                _context.Organizaciones.Add(organizacion);
                await _context.SaveChangesAsync();

                adminUser.Organizacion = organizacion;

                await transaction.CommitAsync();

                return new ApiResponseDto<UserInfoDto>
                {
                    Success = true,
                    Message = "Organización registrada exitosamente. Pendiente de verificación.",
                    Data = MapToUserInfoDto(adminUser)
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponseDto<UserInfoDto>
                {
                    Success = false,
                    Message = "Error al registrar organización",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<UserInfoDto>> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _context.Usuarios
                    .Include(u => u.Organizacion)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new ApiResponseDto<UserInfoDto>
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                return new ApiResponseDto<UserInfoDto>
                {
                    Success = true,
                    Data = MapToUserInfoDto(user)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<UserInfoDto>
                {
                    Success = false,
                    Message = "Error al obtener usuario",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<bool>> ValidateEmailAsync(string email)
        {
            try
            {
                var exists = await _context.Usuarios
                    .AnyAsync(u => u.Email.ToLower() == email.ToLower());

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = !exists,
                    Message = exists ? "Email ya está en uso" : "Email disponible"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error al validar email",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _context.Usuarios.FindAsync(userId);
                if (user == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                if (!_passwordService.VerifyPassword(currentPassword, user.PasswordHash))
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Contraseña actual incorrecta"
                    };
                }

                if (!_passwordService.IsPasswordStrong(newPassword))
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "La nueva contraseña no cumple con los requisitos de seguridad"
                    };
                }

                user.PasswordHash = _passwordService.HashPassword(newPassword);
                user.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Contraseña actualizada exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Error al cambiar contraseña",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private UserInfoDto MapToUserInfoDto(Usuario user)
        {
            var userInfo = new UserInfoDto
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email,
                Telefono = user.Telefono,
                Rol = user.Rol,
                Estatus = user.Estatus
            };

            if (user.Organizacion != null)
            {
                userInfo.Organizacion = new OrganizacionInfoDto
                {
                    Id = user.Organizacion.Id,
                    Nombre = user.Organizacion.Nombre,
                    Descripcion = user.Organizacion.Descripcion,
                    Email = user.Organizacion.Email,
                    Estatus = user.Organizacion.Estatus
                };
            }

            return userInfo;
        }
    }
}
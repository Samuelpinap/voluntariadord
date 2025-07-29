using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Services
{
    public interface IAuthService
    {
        Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest);
        Task<ApiResponseDto<UserInfoDto>> RegisterVoluntarioAsync(RegisterVoluntarioDto registerDto);
        Task<ApiResponseDto<UserInfoDto>> RegisterOrganizacionAsync(RegisterOrganizacionDto registerDto);
        Task<ApiResponseDto<UserInfoDto>> GetUserByIdAsync(int userId);
        Task<ApiResponseDto<bool>> ValidateEmailAsync(string email);
        Task<ApiResponseDto<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
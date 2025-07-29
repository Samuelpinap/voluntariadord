using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using VoluntariadoConectadoRD.Models;

namespace VoluntariadoConectadoRD.Attributes
{
    public class RoleAuthorizationAttribute : Attribute, IAuthorizationFilter
    {
        private readonly UserRole[] _allowedRoles;

        public RoleAuthorizationAttribute(params UserRole[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? throw new ArgumentNullException(nameof(allowedRoles));
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Verificar si el usuario está autenticado
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Obtener el rol del usuario desde los claims
            var roleClaim = context.HttpContext.User.FindFirst(ClaimTypes.Role);
            if (roleClaim == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            // Verificar si el rol está permitido
            if (Enum.TryParse<UserRole>(roleClaim.Value, out var userRole))
            {
                if (!_allowedRoles.Contains(userRole))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
            else
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }

    // Atributos específicos para facilitar el uso
    public class VoluntarioOnlyAttribute : RoleAuthorizationAttribute
    {
        public VoluntarioOnlyAttribute() : base(UserRole.Voluntario) { }
    }

    public class OrganizacionOnlyAttribute : RoleAuthorizationAttribute
    {
        public OrganizacionOnlyAttribute() : base(UserRole.Organizacion) { }
    }

    public class AdminOnlyAttribute : RoleAuthorizationAttribute
    {
        public AdminOnlyAttribute() : base(UserRole.Administrador) { }
    }

    public class VoluntarioOrAdminAttribute : RoleAuthorizationAttribute
    {
        public VoluntarioOrAdminAttribute() : base(UserRole.Voluntario, UserRole.Administrador) { }
    }

    public class OrganizacionOrAdminAttribute : RoleAuthorizationAttribute
    {
        public OrganizacionOrAdminAttribute() : base(UserRole.Organizacion, UserRole.Administrador) { }
    }
}
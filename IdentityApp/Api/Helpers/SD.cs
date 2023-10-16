using Microsoft.AspNetCore.Authorization;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace Api.Helpers
{
    public static class SD
    {
        public const string Facebook = "facebook";
        public const string Google = "google";

        // Roles
        public const string AdminRole = "Admin";
        public const string ManagerRole = "Manager";
        public const string PlayerRole = "Player";
        public const string VipRole = "vip";

        //Policy

        public const string AdminPolicy = "AdminPolicy";
        public const string ManagerPolicy = "ManagerPolicy";
        public const string PlayerPolicy = "PlayerPolicy";
        public const string VipPolicy = "VipPolicy";
        public const string AdminOrManagerPolicy = "AdminOrManagerPolicy";
        public const string AdminAndManagerPolicy = "AdminAndManagerPolicy";
        public const string AllRolePolicy = "AllRolePolicy";
        public const string AdminEmailPolicy = "AdminEmailPolicy";
        public const string PowerSurnamePolicy = "PowerSurnamePolicy";
        public const string ClaimAdminEmail = "admin@example.com";
        public const string ClaimManagerEmail = "manager@example.com";
        public const string ClaimPowerSurname = "power";
        public const string ManagerEmailAndPowerSurnamePolicy = "ManagerEmailAndPowerSurnamePolicy";

        public static bool VIPPolicy(AuthorizationHandlerContext context)
        {
            if (context != null && context.User.IsInRole(PlayerRole) && context.User.HasClaim(c => c.Type == ClaimTypes.Email && c.Value.Contains(VipRole))){
                return true;
            }

            return false;
        }
    }
}

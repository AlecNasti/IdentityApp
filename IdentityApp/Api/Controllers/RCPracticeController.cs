using Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RCPracticeController : ControllerBase
    {
        [HttpGet("public")]
        public ActionResult Public()
        {
            return Ok("public");
        }

        #region Roles
        [Authorize(Roles = SD.AdminRole)]
        [HttpGet("admin-role")]
        public ActionResult AdminRole()
        {
            return Ok("admin");
        }

        [Authorize(Roles = SD.ManagerRole)]
        [HttpGet("manager-role")]
        public ActionResult ManagerRole()
        {
            return Ok("manager");
        }

        [Authorize(Roles = SD.PlayerRole)]
        [HttpGet("player-role")]
        public ActionResult PlayerRole()
        {
            return Ok("player");
        }

        [Authorize(Roles = SD.AdminRole + "," + SD.ManagerRole)]
        [HttpGet("admin-or-manager-role")]
        public ActionResult AdminOrManagerRole()
        {
            return Ok("admin or manager");
        }

        [Authorize(Roles = SD.AdminRole + "," + SD.PlayerRole)]
        [HttpGet("admin-or-player-role")]
        public ActionResult AdminOrPlayerRole()
        {
            return Ok("admin or player role");
        }

        #endregion

        #region Claim policy

        [Authorize(policy: SD.AdminPolicy)]
        [HttpGet("admin-policy")]
        public ActionResult AdminPolicy()
        {
            return Ok("admin policy");
        }

        [Authorize(policy: SD.ManagerPolicy)]
        [HttpGet("manager-policy")]
        public ActionResult ManagerPolicy()
        {
            return Ok("manager policy");
        }

        [Authorize(policy: SD.PlayerPolicy)]
        [HttpGet("player-policy")]
        public ActionResult PlayerPolicy()
        {
            return Ok("player policy");
        }

        [Authorize(policy: SD.AdminOrManagerPolicy)]
        [HttpGet("admin-or-manager-policy")]
        public ActionResult AdminOrManagerPolicy()
        {
            return Ok("admin or manager policy");
        }

        [Authorize(policy: SD.AdminAndManagerPolicy)]
        [HttpGet("admin-and-manager-policy")]
        public ActionResult AdminAndManagerPolicy()
        {
            return Ok("admin and manager policy");
        }

        [Authorize(policy: SD.AllRolePolicy)]
        [HttpGet("all-role-policy")]
        public ActionResult AllRolePolicy()
        {
            return Ok("all role policy");
        }
        #endregion

        #region Type claim policy

        [Authorize(policy: SD.AdminEmailPolicy)]
        [HttpGet("admin-email-policy")]
        public ActionResult AdminEmailPolicy()
        {
            return Ok("admin email policy");
        }

        [Authorize(policy: SD.PowerSurnamePolicy)]
        [HttpGet("power-surname-policy")]
        public ActionResult PowerSurnamePolicy()
        {
            return Ok("power surname policy");
        }

        [Authorize(policy: SD.ManagerEmailAndPowerSurnamePolicy)]
        [HttpGet("manager-email-and-power-surname-policy")]
        public ActionResult ManagerEmailAndPowerSurnamePolicy()
        {
            return Ok("manager and power username policy");
        }

        [Authorize(policy: SD.VipPolicy)]
        [HttpGet("vip-policy")]
        public ActionResult VipPolicy()
        {
            return Ok("vip policy");
        }

        #endregion
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.API
{
  [AllowAnonymous]
  [ApiExplorerSettings(GroupName = "v1")]
  public class SystemController : ControllerBase
  {
    [HttpGet("Health")]
    public IActionResult Health()
    {
      return Ok();
    }
  }
}

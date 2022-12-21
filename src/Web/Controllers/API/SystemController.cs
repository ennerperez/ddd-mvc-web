using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.API
{
  [ApiController]
  [Route("[controller]")]
  [ApiExplorerSettings(GroupName = "v1")]
  [AllowAnonymous]
  public class SystemController : ControllerBase
  {
    [HttpGet("Health")]
    public IActionResult Health()
    {
      return Ok();
    }
  }
}

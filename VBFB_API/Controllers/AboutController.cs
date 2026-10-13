using Microsoft.AspNetCore.Mvc;

namespace VBFB_API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AboutController : ControllerBase
    {
        // GET api/v1/about - anonymous apart from the APIKEY; used to check a deployment is up
        [HttpGet]
        public ActionResult<dynamic> Get()
        {
            return Ok(new { name = "VBFB_API", version = Globals.VersionNumber });
        }
    }
}

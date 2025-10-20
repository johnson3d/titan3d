using Microsoft.AspNetCore.Mvc;

namespace CloudWeb.Controllers
{
    //https://localhost:7000/swagger/index.html
    [ApiController]
    [Route("titan3d/cloudassets")]
    public class CloudAssets : ControllerBase
    {
        private readonly ILogger<CloudAssets> _logger;

        public CloudAssets(ILogger<CloudAssets> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetAssetHash")]
        public string GetAssetHash(string name)
        {
            return "hash";
        }
    }
}

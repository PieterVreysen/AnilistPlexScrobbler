using System.Threading.Tasks;
using AnilistPlexScrobbler.Dtos;
using AnilistPlexScrobbler.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AnilistPlexScrobbler.Controllers
{
    [ApiController]
    [Route("/")]
    public class ScrobbleController : ControllerBase
    {
        private readonly AnilistService _anilistService;
        private readonly IConfiguration _config;

        public ScrobbleController(AnilistService anilistService, IConfiguration config)
        {
            _anilistService = anilistService;
            _config = config;
        }
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("running ...");
        }

        [HttpPost("/")]
        public async Task<IActionResult> Post([FromForm]ScrobbleFile file)
        {
            var payload = JsonConvert.DeserializeObject<PlexPayload>(file.Payload);

            if (payload.Event != "media.scrobble" && payload.Event != "media.rate") return Ok();

            var animeLibraryTitle = _config.GetValue<string>("animeLibraryTitle");
            var animeMovieLibraryTitle = _config.GetValue<string>("animeMovieLibraryTitle");

            if(payload.Metadata.LibrarySectionTitle != animeLibraryTitle && payload.Metadata.LibrarySectionTitle != animeMovieLibraryTitle) return Ok();

            await _anilistService.Sync(payload);

            return Ok();
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AnilistPlexScrobbler.Dtos
{
    public class ScrobbleFile
    {
        [FromForm(Name = "payload")]
        public string Payload { get; set; }
        [FromForm(Name = "thumb")]
        public IFormFile Thumb { get; set; }
    }
}
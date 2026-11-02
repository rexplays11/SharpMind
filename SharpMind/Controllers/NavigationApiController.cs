using Microsoft.AspNetCore.Mvc;
using SharpMind.Data;

namespace SharpMind.Controllers
{
    [ApiController]
    public class NavigationApiController : ControllerBase
    {
        private readonly SharpMindContext _context;
        public NavigationApiController(SharpMindContext context)
        {
            _context = context;
        }

        [HttpGet("/api/poglavlja")]
        public IActionResult Poglavlja()
        {
            return Ok(_context.Poglavlja.ToList());
        }

        [HttpGet("/api/potpoglavlja/{poglavljeId}")]
        public IActionResult Potpoglavlja(int poglavljeId)
        {
            return Ok(
                _context.Potpoglavlja
                .Where(p => p.PoglavljeId == poglavljeId)
                .ToList()
            );
        }

        [HttpGet("/api/lekcije/{potpoglavljeId}")]
        public IActionResult Lekcije(int potpoglavljeId)
        {
            return Ok(
                _context.Lekcije
                .Where(l => l.PotpoglavljeId == potpoglavljeId)
                .ToList()
            );
        }
    }
}
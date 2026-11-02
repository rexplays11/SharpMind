
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharpMind.Controllers
{
    [Authorize]
    public class CompilerController : Controller
    {
        private readonly HttpClient _httpClient;


        private const string CLIENT_ID = "6ee30e18a7dfc14cae57080f0c1f795e";
        private const string CLIENT_SECRET = "f9ab7f1b207b8bafd2f13bb7354504d400cb7d02156684bee606e776a39e265f";

        public CompilerController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: /Compiler/Index
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Compiler/Run
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Run([FromBody] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { error = "Kod ne sme biti prazan." });

            var payload = new
            {
                clientId = CLIENT_ID,
                clientSecret = CLIENT_SECRET,
                script = code,
                language = "csharp",
                versionIndex = "4"
            };

            try
            {
                // Slanje zahteva ka eksternom kompajler API-ju (JDoodle)
                var response = await _httpClient.PostAsJsonAsync("https://api.jdoodle.com/v1/execute", payload);


                response.EnsureSuccessStatusCode();

                var resultJson = await response.Content.ReadAsStringAsync();


                using var doc = JsonDocument.Parse(resultJson);


                if (doc.RootElement.TryGetProperty("output", out var outputElement))
                {
                    var output = outputElement.GetString();


                    return Ok(new { success = true, output = output ?? "Nema output-a." });
                }

                return StatusCode(502, new { error = "Format odgovora sa servera za kompajliranje nije validan." });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { error = "Kompajler servis trenutno nije dostupan ili je odbio zahtev." });
            }
            catch (JsonException)
            {
                return StatusCode(502, new { error = "Došlo je do greške prilikom obrade rezultata kompajliranja." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Unutrašnja greška servera prilikom izvršavanja koda." });
            }
        }
    }
}
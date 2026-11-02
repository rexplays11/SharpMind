using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharpMind.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        private readonly SharpMindContext _context;
        private readonly HttpClient _httpClient;

        private const string CLIENT_ID = "6ee30e18a7dfc14cae57080f0c1f795e";
        private const string CLIENT_SECRET = "f9ab7f1b207b8bafd2f13bb7354504d400cb7d02156684bee606e776a39e265f";

        public TestController(SharpMindContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        // GET: /Test/Index
        public IActionResult Index()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var korisnik = _context.Korisnici.Find(userId);
                if (korisnik == null) return RedirectToAction("Login", "Account");

                int nivoId = korisnik.NivoId;
                int ukupnoPotrebnoPitanja = 10 + (nivoId - 1);

                var validnaPitanjaQuery = _context.Pitanja
                    .Where(p => _context.Odgovori.Any(o => o.PitanjeId == p.PitanjeId && o.OdgovorTacnost == true)
                             && _context.Odgovori.Count(o => o.PitanjeId == p.PitanjeId) >= 2);

                var pitanjaTrenutniNivo = validnaPitanjaQuery
                    .Where(p => p.NivoId == nivoId)
                    .AsEnumerable()
                    .OrderBy(x => Guid.NewGuid())
                    .Take(10)
                    .ToList();

                if (pitanjaTrenutniNivo.Count < 10)
                {
                    ViewBag.NedovoljnoPitanja = "Trenutno nema dovoljno pitanja da se uradi test za tvoj nivo. Probaj ponovo kasnije.";
                    return View("Rezultat");
                }

                var ostalaPitanja = new List<Pitanje>();
                int ostatakZaPopunu = ukupnoPotrebnoPitanja - pitanjaTrenutniNivo.Count;

                if (nivoId > 1 && ostatakZaPopunu > 0)
                {
                    ostalaPitanja = validnaPitanjaQuery
                        .Where(p => p.NivoId < nivoId && p.NivoId >= (nivoId - 2))
                        .AsEnumerable()
                        .OrderBy(x => Guid.NewGuid())
                        .Take(ostatakZaPopunu)
                        .ToList();
                }

                var konacnaListaPitanja = pitanjaTrenutniNivo.Concat(ostalaPitanja).OrderBy(x => Guid.NewGuid()).ToList();

                var testModel = new List<TestPitanjeVM>();
                foreach (var p in konacnaListaPitanja)
                {
                    var odgovori = _context.Odgovori
                        .Where(o => o.PitanjeId == p.PitanjeId)
                        .ToList();

                    testModel.Add(new TestPitanjeVM
                    {
                        Pitanje = p,
                        PonudjeniOdgovori = odgovori.OrderBy(x => Guid.NewGuid()).ToList()
                    });
                }

                return View(testModel);
            }
            catch (Exception)
            {
                ViewBag.NedovoljnoPitanja = "Došlo je do sistemske greške prilikom inicijalizacije testa. Molimo pokušajte ponovo.";
                return View("Rezultat");
            }
        }

        // POST: /Test/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(IFormCollection form)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var korisnik = _context.Korisnici.Find(userId);
                if (korisnik == null) return RedirectToAction("Login", "Account");

                int tacniOdgovori = 0;
                int ukupnoPitanja = 0;

                var kodniKlucevi = form.Keys.Where(k => k.StartsWith("pitanje_kod_")).ToList();
                foreach (var key in kodniKlucevi)
                {
                    ukupnoPitanja++;
                    string formatiraniKljuc = key.Replace("pitanje_kod_", "");
                    if (int.TryParse(formatiraniKljuc, out int pId))
                    {
                        string output = form[key].ToString().Trim();
                        var tacan = _context.Odgovori.FirstOrDefault(o => o.PitanjeId == pId && o.OdgovorTacnost == true);

                        if (tacan != null && string.Equals(output, tacan.OdgovorTekst.Trim(), StringComparison.Ordinal))
                        {
                            tacniOdgovori++;
                        }
                    }
                }

                var standardniKlucevi = form.Keys
                    .Where(k => k.StartsWith("pitanje_") && !k.Contains("_kod"))
                    .ToList();

                foreach (var key in standardniKlucevi)
                {
                    ukupnoPitanja++;
                    string pIdString = key.Replace("pitanje_", "");
                    if (int.TryParse(pIdString, out int pId))
                    {
                        var selektovaniOdgovoriIds = form[key].ToString()
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(id => int.TryParse(id, out int res) ? res : 0)
                            .Where(id => id > 0)
                            .ToList();

                        var sviTacniOdgovoriIzBazeIds = _context.Odgovori
                            .Where(o => o.PitanjeId == pId && o.OdgovorTacnost == true)
                            .Select(o => o.OdgovorId)
                            .ToList();

                        if (sviTacniOdgovoriIzBazeIds.Count > 0 &&
                            selektovaniOdgovoriIds.Count == sviTacniOdgovoriIzBazeIds.Count &&
                            selektovaniOdgovoriIds.All(id => sviTacniOdgovoriIzBazeIds.Contains(id)))
                        {
                            tacniOdgovori++;
                        }
                    }
                }

                double procenat = ukupnoPitanja > 0 ? (double)tacniOdgovori / ukupnoPitanja * 100 : 0;
                bool polozioTest = procenat >= 70;

                double osvojeniPoeniNaTestu = (procenat < 50) ? (tacniOdgovori * 2.5) : (tacniOdgovori * 5.0);

                var trenutniNivoKorisnika = _context.Nivoi.Find(korisnik.NivoId);
                int trenutniGranicni = trenutniNivoKorisnika?.GranicniPoeniNivoa ?? 0;

                var sledeciNivo = _context.Nivoi
                    .Where(n => n.GranicniPoeniNivoa > trenutniGranicni)
                    .OrderBy(n => n.GranicniPoeniNivoa)
                    .FirstOrDefault();

                int ukupnoPoenaNakonTesta = korisnik.UkupnoPoena + (int)osvojeniPoeniNaTestu;
                string poruka = "";
                bool levelUp = false;
                bool maksimalanNivoDostignut = false;

                if (sledeciNivo == null)
                {
                    maksimalanNivoDostignut = true;
                    if (polozioTest)
                    {
                        korisnik.UkupnoPoena = trenutniGranicni;
                        osvojeniPoeniNaTestu = 0; 
                        poruka = "Čestitamo! Osvojili ste maksimalan broj poena za maksimalni nivo! Uspešno ste kompletirali igru!";
                    }
                    else
                    {
                        osvojeniPoeniNaTestu = 0;
                        poruka = "Nalaziš se na poslednjem nivou, ali moraš položiti ovaj test sa najmanje 70% da bi ga zvanično završio!";
                    }
                }
                else if (ukupnoPoenaNakonTesta >= sledeciNivo.GranicniPoeniNivoa)
                {
                    // Korisnik ide na sledeći nivo, ali mora da položi test sa 70%+
                    if (polozioTest)
                    {
                        korisnik.NivoId = sledeciNivo.NivoId;
                        korisnik.UkupnoPoena = ukupnoPoenaNakonTesta;
                        levelUp = true;
                    }
                    else
                    {
                        osvojeniPoeniNaTestu = 0;
                        poruka = "Dostigao si broj poena za novi nivo, ali moraš položiti ovaj test sa najmanje 70% da bi prešao dalje!";
                    }
                }
                else
                {
                    korisnik.UkupnoPoena += (int)osvojeniPoeniNaTestu;
                }

                _context.Rezultati.Add(new Rezultat
                {
                    KorisnikId = userId,
                    NivoId = korisnik.NivoId,
                    TipAktivnosti = "TEST",
                    OsvojeniPoeni = (int)osvojeniPoeniNaTestu,
                    UkupnoPoena = ukupnoPitanja * 5,
                    DatumPolaganja = DateTime.Now
                });

                _context.SaveChanges();

                ViewBag.Poruka = poruka;
                ViewBag.Polozio = polozioTest;
                ViewBag.Osvojeno = (int)osvojeniPoeniNaTestu;
                ViewBag.Procenat = procenat;
                ViewBag.BrojPoena = tacniOdgovori;
                ViewBag.Ukupno = ukupnoPitanja;
                ViewBag.LevelUp = levelUp;
                ViewBag.MaksimalanNivo = maksimalanNivoDostignut;
                ViewBag.UkupnoPoenaKorisnika = korisnik.UkupnoPoena;
                ViewBag.NoviNivoNaziv = sledeciNivo?.NazivNivoa;

                return View("Rezultat");
            }
            catch (Exception)
            {
                ViewBag.NedovoljnoPitanja = "Kritična greška pri obradi i snimanju tvog testa. Bodovi nisu sačuvani.";
                return View("Rezultat");
            }
        }

        [HttpPost]
        [Route("api/compiler/run")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunCode()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var code = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(code))
                    return Content("Kod je prazan.", "text/plain");

                var payload = new
                {
                    clientId = CLIENT_ID,
                    clientSecret = CLIENT_SECRET,
                    script = code,
                    language = "csharp",
                    versionIndex = "4"
                };

                var response = await _httpClient.PostAsJsonAsync("https://api.jdoodle.com/v1/execute", payload);
                if (!response.IsSuccessStatusCode)
                {
                    return Content("Greška: Eksterni kompajler servis trenutno nije dostupan.", "text/plain");
                }

                var resultJson = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(resultJson);
                if (doc.RootElement.TryGetProperty("output", out var outputElement))
                {
                    var output = outputElement.GetString();
                    return Content(output ?? "Nema output-a", "text/plain");
                }

                return Content("Nije moguće pročitati povratnu vrednost izvršavanja.", "text/plain");
            }
            catch (Exception ex)
            {
                return Content("Greška pri komunikaciji sa API-jem: " + ex.Message, "text/plain");
            }
        }
    }
}
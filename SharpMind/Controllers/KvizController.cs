using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharpMind.Controllers
{

    [AllowAnonymous]
    public class KvizController : Controller
    {
        private readonly SharpMindContext _context; 
        private readonly HttpClient _httpClient;   

        // JDoodle API servisa
        private const string CLIENT_ID = "6ee30e18a7dfc14cae57080f0c1f795e";
        private const string CLIENT_SECRET = "f9ab7f1b207b8bafd2f13bb7354504d400cb7d02156684bee606e776a39e265f";


        public KvizController(SharpMindContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }


        [AllowAnonymous]
        public IActionResult Index()
        {
            try
            {

                var trenutniKorisnik = _context.Korisnici
                    .Include(k => k.Uloga)
                    .FirstOrDefault(k => k.KorisnikIme == User.Identity.Name);

                ViewBag.IsAdmin = trenutniKorisnik?.Uloga?.UlogaNaziv == "Admin";
                ViewBag.TrenutniKorisnikId = trenutniKorisnik?.KorisnikId;


                var kvizovi = _context.Kvizovi
                    .Include(k => k.Kreator)
                    .Include(k => k.KvizPitanja)
                    .ToList();

                return View(kvizovi);
            }
            catch (Exception)
            {
                ViewBag.Error = "Greška pri učitavanju kvizova iz baze podataka.";
                return View(new List<Kviz>());
            }
        }



        [AllowAnonymous]
        public IActionResult Igraj(int id)
        {
            try
            {

                var trenutniKorisnik = _context.Korisnici
                    .FirstOrDefault(k => k.KorisnikIme == User.Identity.Name);

                ViewBag.TrenutniKorisnikId = trenutniKorisnik?.KorisnikId;

                var kviz = _context.Kvizovi.Find(id);
                return kviz == null ? NotFound() : View(kviz);
            }
            catch (Exception)
            {
                return StatusCode(500, "Došlo je do greške prilikom pokretanja kviza.");
            }
        }

        public IActionResult DajPitanjeIzKviza(int kvizId, string odgovorilaIds)
        {
            try
            {
                var iskljuciIds = new List<int>();

                if (!string.IsNullOrEmpty(odgovorilaIds))
                {
                    foreach (var idStr in odgovorilaIds.Split(','))
                    {
                        if (int.TryParse(idStr.Trim(), out int parsedId))
                        {
                            iskljuciIds.Add(parsedId);
                        }
                    }
                }

                var pitanjaIds = _context.KvizPitanja
                    .Where(kp => kp.KvizId == kvizId && !iskljuciIds.Contains(kp.PitanjeId))
                    .Where(kp => _context.Odgovori.Any(o => o.PitanjeId == kp.PitanjeId && o.OdgovorTacnost == true)
                              && _context.Odgovori.Count(o => o.PitanjeId == kp.PitanjeId) >= 2)
                    .Select(kp => kp.PitanjeId)
                    .ToList();

                if (pitanjaIds.Count == 0) return Json(new { kraj = true });

                int nasumicanId = pitanjaIds[new Random().Next(pitanjaIds.Count)];

                var pitanje = _context.Pitanja.FirstOrDefault(p => p.PitanjeId == nasumicanId);
                if (pitanje == null) return Json(new { kraj = true });

                var odgovori = _context.Odgovori
                    .Where(o => o.PitanjeId == nasumicanId)
                    .Select(o => new { o.OdgovorId, o.OdgovorTekst })
                    .ToList();


                int brojTacnihOdgovora = _context.Odgovori.Count(o => o.PitanjeId == nasumicanId && o.OdgovorTacnost == true);
                bool isMultiselect = brojTacnihOdgovora > 1;

                return Json(new
                {
                    pitanjeId = pitanje.PitanjeId,
                    tekst = pitanje.PitanjeTekst,
                    blokKod = pitanje.BlokKod,
                    isMultiselect = isMultiselect, 
                    odgovori,
                    kraj = false
                });
            }
            catch (Exception)
            {
                return Json(new { error = "Sistemska greška pri dobavljanju pitanja.", kraj = true });
            }
        }


        [HttpPost]
        public IActionResult ProveriOdgovor(int pitanjeId, List<int> odgovorIds)
        {
            try
            {
                if (odgovorIds == null) odgovorIds = new List<int>();

                // Izvlačimo sve ID-jeve tačnih odgovora za ovo pitanje iz baze
                var tacniOdgovoriIzBaze = _context.Odgovori
                    .Where(o => o.PitanjeId == pitanjeId && o.OdgovorTacnost == true)
                    .Select(o => o.OdgovorId)
                    .ToList();


                bool sveTacno = tacniOdgovoriIzBaze.Count == odgovorIds.Count && !tacniOdgovoriIzBaze.Except(odgovorIds).Any();

                if (sveTacno)
                {
                    return Json(new { tacan = true });
                }

                var tacanTekstovi = _context.Odgovori
                    .Where(o => o.PitanjeId == pitanjeId && o.OdgovorTacnost == true)
                    .Select(o => o.OdgovorTekst)
                    .ToList();

                string spojenTekst = string.Join(", ", tacanTekstovi);

                return Json(new { tacan = false, tacanTekst = spojenTekst });
            }
            catch (Exception)
            {
                return Json(new { error = "Greška prilikom provere odgovora na serveru." });
            }
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RunCode([FromBody] CodeRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Kod ne sme biti prazan.");

            var payload = new { clientId = CLIENT_ID, clientSecret = CLIENT_SECRET, script = request.Code, language = "csharp", versionIndex = "4" };

            try
            {

                var response = await _httpClient.PostAsJsonAsync("https://api.jdoodle.com/v1/execute", payload);
                response.EnsureSuccessStatusCode(); 

                var resultJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(resultJson);

                if (doc.RootElement.TryGetProperty("output", out var outEl))
                {
                    return Content(outEl.GetString() ?? "Nema izlaza.");
                }

                return Content("Greška: Nije moguće pročitati izlaz kompajlera.");
            }
            catch (HttpRequestException)
            {
                return Content("Greška: Kompajler servis trenutno nije dostupan.");
            }
            catch (JsonException)
            {
                return Content("Greška pri obradi rezultata izvršavanja koda.");
            }
            catch (Exception)
            {
                return Content("Unutrašnja greška servera prilikom izvršavanja koda.");
            }
        }



        [HttpPost]
        public IActionResult SacuvajRezultat([FromBody] Rezultat rezultat, int kvizId)
        {
            if (rezultat == null) return BadRequest(new { success = false, message = "Podaci o rezultatu su prazni." });

            try
            {
                rezultat.DatumPolaganja = DateTime.Now;

                if (rezultat.TipAktivnosti?.ToUpper() == "TEST")
                {
 
                }
                else if (rezultat.TipAktivnosti?.ToUpper() == "KVIZ")
                {
                    rezultat.NivoId = null;

                    int brojPitanjaUKvizu = _context.KvizPitanja.Count(kp => kp.KvizId == kvizId);

                    rezultat.UkupnoPoena = brojPitanjaUKvizu;
                }

                _context.Rezultati.Add(rezultat);
                _context.SaveChanges();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
   
                var porukaGreske = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { success = false, message = "Sistemska greška: " + porukaGreske });
            }
        }



        // GET: Kviz/Create

        public IActionResult Create()
        {
            try
            {
                PopuniListe(); 

                var trenutni = _context.Korisnici
                    .Include(k => k.Uloga)
                    .FirstOrDefault(k => k.KorisnikIme == User.Identity.Name);

                ViewBag.TrenutniKorisnik = trenutni;
            }
            catch (Exception)
            {
                ViewBag.Error = "Sistem ne može da učita podatke iz baze.";
            }
            return View();
        }

        // POST: Kviz/Create

        [HttpPost, ValidateAntiForgeryToken] 
        public IActionResult Create(Kviz kviz, List<int> izabranaPitanja)
        {
            int izabranoBroj = izabranaPitanja?.Count ?? 0;
            //  Kviz mora imati najmanje 5 izabranih pitanja
            if (izabranoBroj < 5)
            {
                int faliJos = 5 - izabranoBroj;
                string recPitanje = faliJos == 1 ? "pitanje" : (faliJos >= 2 && faliJos <= 4) ? "pitanja" : "pitanja";
                ModelState.AddModelError("", $"Nedovoljan broj pitanja za kreiranje kviza. Izabrali ste {izabranoBroj}, potrebno je da izaberete još {faliJos} {recPitanje}.");
            }

            bool naslovPostoji = _context.Kvizovi.Any(k => k.KvizNaslov == kviz.KvizNaslov);
            if (naslovPostoji)
            {
                ModelState.AddModelError("KvizNaslov", "Kviz sa ovim naslovom već postoji. Izaberite drugi naslov.");
            }

            var trenutniKorisnik = _context.Korisnici
                .Include(k => k.Uloga)
                .FirstOrDefault(k => k.KorisnikIme == User.Identity.Name);

            if (trenutniKorisnik != null && trenutniKorisnik.Uloga != null)
            {
                if (trenutniKorisnik.Uloga.UlogaNaziv == "Profesor")
                {
                    kviz.KorisnikId = trenutniKorisnik.KorisnikId;
                }
                else if (trenutniKorisnik.Uloga.UlogaNaziv == "Admin" && kviz.KorisnikId == 0)
                {

                    kviz.KorisnikId = trenutniKorisnik.KorisnikId;
                }
            }

            try { ValidirajKviz(kviz); } catch { } 

            if (ModelState.IsValid)
            {
                try
                {

                    _context.Kvizovi.Add(kviz);
                    _context.SaveChanges();


                    if (izabranaPitanja != null && izabranaPitanja.Count > 0)
                    {
                        foreach (var pId in izabranaPitanja)
                        {
                            _context.KvizPitanja.Add(new KvizPitanje { KvizId = kviz.KvizId, PitanjeId = pId });
                        }
                        _context.SaveChanges();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Sistemska greška prilikom upisivanja kviza u bazu.");
                }
            }

            try { PopuniListe(); } catch { }
            ViewBag.TrenutniKorisnik = trenutniKorisnik;
            return View(kviz);
        }


        public IActionResult Edit(int id)
        {
            try
            {
                var kviz = _context.Kvizovi.Include(k => k.KvizPitanja).FirstOrDefault(k => k.KvizId == id);
                if (kviz == null) return NotFound();

                var trenutniKorisnik = _context.Korisnici
                    .Include(k => k.Uloga)
                    .FirstOrDefault(k => k.KorisnikIme == User.Identity.Name);

                bool isAdmin = trenutniKorisnik?.Uloga?.UlogaNaziv == "Admin";
                if (!isAdmin && kviz.KorisnikId != trenutniKorisnik?.KorisnikId)
                {
                    TempData["Greska"] = "Nemate pravo da menjate ovaj kviz jer niste njegov kreator!";
                    return RedirectToAction(nameof(Index));
                }

                PopuniListe();
                ViewBag.TrenutniKorisnik = trenutniKorisnik;
                ViewBag.SelektovanaPitanja = kviz.KvizPitanja.Select(kp => kp.PitanjeId).ToList();
                return View(kviz);
            }
            catch (Exception)
            {
                TempData["Error"] = "Greška pri učitavanju kviza za izmenu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Kviz/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Kviz kviz, List<int> izabranaPitanja)
        {
            var trenutniKorisnik = _context.Korisnici
                .Include(k => k.Uloga)
                .FirstOrDefault(k => k.KorisnikIme == User.Identity.Name);


            var postojeci = _context.Kvizovi.Include(k => k.KvizPitanja).FirstOrDefault(k => k.KvizId == id);
            if (postojeci == null) return NotFound();

            bool isAdmin = trenutniKorisnik?.Uloga?.UlogaNaziv == "Admin";
            if (!isAdmin && postojeci.KorisnikId != trenutniKorisnik?.KorisnikId)
            {
                TempData["Greska"] = "Nemate ovlašćenje da sačuvate izmene na ovom kvizu!";
                return RedirectToAction(nameof(Index));
            }

            int izabranoBroj = izabranaPitanja?.Count ?? 0;
            if (izabranoBroj < 5)
            {
                int faliJos = 5 - izabranoBroj;
                string recPitanje = faliJos == 1 ? "pitanje" : (faliJos >= 2 && faliJos <= 4) ? "pitanja" : "pitanja";
                ModelState.AddModelError("", $"Nedovoljan broj pitanja za izmenu kviza. Izabrali ste {izabranoBroj}, potrebno je da izaberete još {faliJos} {recPitanje}.");
            }

 
            bool naslovPostoji = _context.Kvizovi.Any(k => k.KvizNaslov == kviz.KvizNaslov && k.KvizId != id);
            if (naslovPostoji)
            {
                ModelState.AddModelError("KvizNaslov", "Kviz sa ovim naslovom već postoji. Izaberite drugi naslov.");
            }

            try { ValidirajKviz(kviz); } catch { }

            if (ModelState.IsValid)
            {
                try
                {

                    postojeci.KvizNaslov = kviz.KvizNaslov;
                    postojeci.TipKviza = kviz.TipKviza;
                    postojeci.VremenskoOgranicenje = kviz.VremenskoOgranicenje;
                    postojeci.BrojZivota = kviz.BrojZivota;

                    if (isAdmin)
                    {
                        postojeci.KorisnikId = kviz.KorisnikId;
                    }


                    _context.KvizPitanja.RemoveRange(postojeci.KvizPitanja);


                    if (izabranaPitanja != null)
                    {
                        foreach (var pId in izabranaPitanja)
                        {
                            _context.KvizPitanja.Add(new KvizPitanje { KvizId = postojeci.KvizId, PitanjeId = pId });
                        }
                    }

                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {

                    ModelState.AddModelError("", "Podaci o kvizu su izmenjeni u bazi od strane drugog korisnika.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Sistemska greška pri čuvanju izmena kviza.");
                }
            }

            try { PopuniListe(); } catch { }
            ViewBag.TrenutniKorisnik = trenutniKorisnik;
            ViewBag.SelektovanaPitanja = izabranaPitanja ?? new List<int>();

            return View(kviz);
        }

        // POST: Kviz/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var kviz = _context.Kvizovi.Include(k => k.KvizPitanja).FirstOrDefault(k => k.KvizId == id);
                if (kviz == null) return NotFound();

                //  Profesor briše isključivo svoje kvizove, Admin briše bilo koji kviz
                var trenutniKorisnik = _context.Korisnici
                    .Include(k => k.Uloga)
                    .FirstOrDefault(k => k.KorisnikIme == User.Identity.Name);

                bool isAdmin = trenutniKorisnik?.Uloga?.UlogaNaziv == "Admin";
                if (!isAdmin && kviz.KorisnikId != trenutniKorisnik?.KorisnikId)
                {
                    TempData["Greska"] = "Nemate pravo da obrišete ovaj kviz jer niste njegov kreator!";
                    return RedirectToAction(nameof(Index));
                }


                _context.KvizPitanja.RemoveRange(kviz.KvizPitanja);
                _context.Kvizovi.Remove(kviz);

                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
             
                TempData["Greska"] = "Kviz ne može biti obrisan jer postoje evidentirani rezultati učenika koji su ga rešavali.";
            }
            catch (Exception)
            {
                TempData["Greska"] = "Došlo je do neočekivane greške na bazi prilikom brisanja kviza.";
            }
            return RedirectToAction(nameof(Index));
        }

        private void PopuniListe()
        {
            // Iz baze povlačimo samo ona pitanja koja imaju definisan bar jedan tačan odgovor i ukupno najmanje 2 odgovora
            ViewBag.Pitanja = _context.Pitanja
                .Where(p => _context.Odgovori.Any(o => o.PitanjeId == p.PitanjeId && o.OdgovorTacnost == true)
                         && _context.Odgovori.Count(o => o.PitanjeId == p.PitanjeId) >= 2)
                .ToList();

            ViewBag.Korisnici = _context.Korisnici
                .Where(k => k.Uloga.UlogaNaziv == "Profesor" || k.Uloga.UlogaNaziv == "Admin")
                .ToList();
        }


        private void ValidirajKviz(Kviz kviz)
        {
            if (kviz.TipKviza == "vreme")
            {
                if (kviz.VremenskoOgranicenje < 30 || kviz.VremenskoOgranicenje > 1800)
                {
                    ModelState.AddModelError("VremenskoOgranicenje", "Vremensko ograničenje mora biti između 30 i 1800 sekundi.");
                }
                kviz.BrojZivota = null;
            }
            else if (kviz.TipKviza == "zivoti")
            {

                if (kviz.BrojZivota < 1 || kviz.BrojZivota > 9)
                {
                    ModelState.AddModelError("BrojZivota", "Broj života mora biti između 1 i 9.");
                }

                kviz.VremenskoOgranicenje = null;
            }
        }

        public class CodeRequest { public string Code { get; set; } }
    }
}
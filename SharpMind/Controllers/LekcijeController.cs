using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpMind.Controllers
{
    [Authorize]
    public class LekcijeController : Controller
    {
        private readonly SharpMindContext _context;

        public LekcijeController(SharpMindContext context)
        {
            _context = context;
        }

        private Korisnik GetTrenutniKorisnik()
        {
            return _context.Korisnici
                .Include(k => k.Uloga)
                .FirstOrDefault(k => k.KorisnikIme == User.Identity.Name);
        }

        private async Task<Korisnik> GetTrenutniKorisnikAsync()
        {
            return await _context.Korisnici
                .Include(k => k.Uloga)
                .FirstOrDefaultAsync(k => k.KorisnikIme == User.Identity.Name);
        }

        // INDEX METODA (Prikaz i upravljanje listom)

        [Authorize(Roles = "Admin, Profesor")]
        public IActionResult Index(string sortOrder, int page = 1, int pageSize = 25)
        {
            try
            {
                if (pageSize < 1) pageSize = 25;

                var trenutniKorisnik = GetTrenutniKorisnik();
                ViewBag.IsAdmin = trenutniKorisnik?.Uloga?.UlogaNaziv == "Admin";
                ViewBag.TrenutniKorisnikId = trenutniKorisnik?.KorisnikId;

                ViewBag.IdSortParm = sortOrder == "id_asc" ? "id_desc" : "id_asc";
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentPageSize = pageSize;

                var query = _context.Lekcije
                    .Include(l => l.Poglavlje)
                    .Include(l => l.Potpoglavlje)
                    .Include(l => l.Nivo)
                    .Include(l => l.Kreator)
                    .AsQueryable();

                switch (sortOrder)
                {
                    case "id_asc":
                        query = query.OrderBy(l => l.LekcijaId);
                        break;
                    case "id_desc":
                    default:
                        query = query.OrderByDescending(l => l.LekcijaId);
                        break;
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;

                var lekcije = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                return View(lekcije);
            }
            catch (Exception)
            {
                ViewBag.Error = "Sistemska greška pri učitavanju liste lekcija.";
                return View(new List<Lekcija>());
            }
        }

        // CREATE (POST): 
        [Authorize(Roles = "Admin, Profesor")]
        public async Task<IActionResult> Create(int? poglavljeId)
        {
            try
            {
                var ulogovaniKorisnik = await GetTrenutniKorisnikAsync();
                if (ulogovaniKorisnik == null) return RedirectToAction("Login", "Account");

                await PopuniListeLekcijaAsync();

                var model = new Lekcija();
                if (poglavljeId.HasValue)
                {
                    model.PoglavljeId = poglavljeId.Value;
                }

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "Sistem ne može da učita prateće liste iz baze za novu lekciju.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Profesor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lekcija lekcija)
        {
            ModelState.Remove(nameof(lekcija.Poglavlje));
            ModelState.Remove(nameof(lekcija.Potpoglavlje));
            ModelState.Remove(nameof(lekcija.Nivo));
            ModelState.Remove(nameof(lekcija.Kreator));
            ModelState.Remove(nameof(lekcija.KorisnikId));

            if (lekcija.LekcijaNaslov != null)
            {
                lekcija.LekcijaNaslov = lekcija.LekcijaNaslov.Trim();
            }
            if (lekcija.LekcijaTekst != null)
            {
                lekcija.LekcijaTekst = lekcija.LekcijaTekst.Trim();
            }

            if (!string.IsNullOrEmpty(lekcija.LekcijaNaslov) && await _context.Lekcije.AnyAsync(l => l.LekcijaNaslov.ToLower() == lekcija.LekcijaNaslov.ToLower()))
            {
                ModelState.AddModelError("LekcijaNaslov", "Lekcija sa ovim naslovom već postoji. Izaberite drugi naslov.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var ulogovaniKorisnik = await GetTrenutniKorisnikAsync();
                    if (ulogovaniKorisnik == null) return RedirectToAction("Login", "Account");


                    lekcija.KorisnikId = ulogovaniKorisnik.KorisnikId;

                    _context.Lekcije.Add(lekcija);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Greška u bazi podataka prilikom čuvanja lekcije.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Neočekivana greška na serveru. Lekcija nije sačuvana.");
                }
            }

            try
            {
                await PopuniListeLekcijaAsync();
            }
            catch { }

            return View(lekcija);
        }

        // GET: /Lekcije/Edit/5
        [Authorize(Roles = "Admin, Profesor")]
        public IActionResult Edit(int id)
        {
            try
            {
                var lekcija = _context.Lekcije.Find(id);
                if (lekcija == null) return NotFound();

                var trenutniKorisnik = GetTrenutniKorisnik();
                bool isAdmin = trenutniKorisnik?.Uloga?.UlogaNaziv == "Admin";


                if (!isAdmin && lekcija.KorisnikId != trenutniKorisnik?.KorisnikId)
                {
                    return Forbid();
                }

                PopuniListe();
                return View(lekcija);
            }
            catch (Exception)
            {
                TempData["Error"] = "Sistem ne može da otvori izmenu izabrane lekcije.";
                return RedirectToAction(nameof(Index));
            }
        }
        // POST: /Lekcije/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin, Profesor")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Lekcija lekcija)
        {
            ModelState.Remove("Poglavlje");
            ModelState.Remove("Potpoglavlje");
            ModelState.Remove("Nivo");
            ModelState.Remove("Kreator");

            var trenutniKorisnik = GetTrenutniKorisnik();
            var postojeca = _context.Lekcije.Find(lekcija.LekcijaId);
            if (postojeca == null) return NotFound();

            bool isAdmin = trenutniKorisnik?.Uloga?.UlogaNaziv == "Admin";


            if (!isAdmin && postojeca.KorisnikId != trenutniKorisnik?.KorisnikId)
            {
                return Forbid();
            }

            if (lekcija.LekcijaNaslov != null)
            {
                lekcija.LekcijaNaslov = lekcija.LekcijaNaslov.Trim();
            }
            if (lekcija.LekcijaTekst != null)
            {
                lekcija.LekcijaTekst = lekcija.LekcijaTekst.Trim();
            }

            if (!string.IsNullOrEmpty(lekcija.LekcijaNaslov) && _context.Lekcije.Any(l => l.LekcijaNaslov.ToLower() == lekcija.LekcijaNaslov.ToLower() && l.LekcijaId != lekcija.LekcijaId))
            {
                ModelState.AddModelError("LekcijaNaslov", "Lekcija sa ovim naslovom već postoji. Izaberite drugi naslov.");
            }

            if (ModelState.IsValid)
            {
                try
                {

                    postojeca.LekcijaNaslov = lekcija.LekcijaNaslov;
                    postojeca.LekcijaTekst = lekcija.LekcijaTekst;
                    postojeca.PoglavljeId = lekcija.PoglavljeId;
                    postojeca.PotpoglavljeId = lekcija.PotpoglavljeId;
                    postojeca.NivoId = lekcija.NivoId;

                    if (isAdmin && lekcija.KorisnikId > 0)
                    {
                        postojeca.KorisnikId = lekcija.KorisnikId;
                    }

                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Podaci su u međuvremenu izmenjeni od strane drugog korisnika.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Nije moguće snimiti izmene. Proverite vezu sa bazom.");
                }
            }

            try { PopuniListe(); } catch { }
            return View(lekcija);
        }

        // DETAILS 

        public IActionResult Details(int id)
        {
            try
            {
                var lekcija = _context.Lekcije
                    .Include(l => l.Nivo)
                    .Include(l => l.Poglavlje)
                    .Include(l => l.Potpoglavlje)
                    .Include(l => l.Kreator)
                    .FirstOrDefault(m => m.LekcijaId == id);

                if (lekcija == null) return NotFound();

                var trenutniKorisnik = GetTrenutniKorisnik();
                ViewBag.IsAdmin = trenutniKorisnik?.Uloga?.UlogaNaziv == "Admin";
                ViewBag.TrenutniKorisnikId = trenutniKorisnik?.KorisnikId;

                if (!User.IsInRole("Admin") && !User.IsInRole("Profesor"))
                {
                    int korisnikNivoId = trenutniKorisnik?.NivoId ?? 1;

                    if (lekcija.NivoId > korisnikNivoId)
                    {
                        TempData["Error"] = "Nemate pristup ovoj lekciji. Morate prvo unaprediti svoj nivo znanja.";
                        return RedirectToAction(nameof(PregledLekcija));
                    }
                }

                return View(lekcija);
            }
            catch (Exception)
            {
                TempData["Error"] = "Greška pri učitavanju detalja lekcije.";
                return RedirectToAction(nameof(PregledLekcija));
            }
        }

        // DELETE 
         
        [HttpPost]
        [Authorize(Roles = "Admin, Profesor")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var lekcija = _context.Lekcije.Find(id);
                if (lekcija == null) return NotFound();

                var trenutniKorisnik = GetTrenutniKorisnik();
                bool isAdmin = trenutniKorisnik?.Uloga?.UlogaNaziv == "Admin";

                if (!isAdmin && lekcija.KorisnikId != trenutniKorisnik?.KorisnikId)
                {
                    return Forbid();
                }

                _context.Lekcije.Remove(lekcija);
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Lekcija ne može biti obrisana jer je povezana sa drugim podacima u aplikaciji.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Došlo je do greške na serveru prilikom brisanja lekcije.";
            }
            return RedirectToAction(nameof(Index));
        }


        // OSTALE METODE

        public IActionResult Lista(int? potpoglavljeId)
        {
            try
            {
                var query = _context.Lekcije
                    .Include(l => l.Poglavlje)
                    .Include(l => l.Potpoglavlje)
                    .Include(l => l.Nivo)
                    .AsQueryable();

                if (potpoglavljeId != null)
                {
                    query = query.Where(l => l.PotpoglavljeId == potpoglavljeId);
                }

                return View(query.ToList());
            }
            catch (Exception)
            {
                return StatusCode(500, "Greška pri filtriranju lekcija.");
            }
        }

        [HttpGet]
        public IActionResult GetPoglavlja()
        {
            try
            {
                var poglavlja = _context.Poglavlja
                    .Select(p => new { p.PoglavljeId, p.PoglavljeNaziv })
                    .ToList();
                return Json(poglavlja);
            }
            catch (Exception)
            {
                return BadRequest("Nije moguće učitati poglavlja.");
            }
        }

        [HttpGet]
        public IActionResult GetPotpoglavlja(int poglavljeId)
        {
            try
            {
                var potpoglavlja = _context.Potpoglavlja
                    .Where(p => p.PoglavljeId == poglavljeId)
                    .Select(p => new { p.PotpoglavljeId, p.NazivPotpoglavlja })
                    .ToList();
                return Json(potpoglavlja);
            }
            catch (Exception)
            {
                return BadRequest("Nije moguće učitati potpoglavlja.");
            }
        }

        // GET: /Lekcije/PregledLekcija 
        public IActionResult PregledLekcija()
        {
            try
            {
                var trenutniKorisnik = GetTrenutniKorisnik();
                int korisnikNivoId = trenutniKorisnik?.NivoId ?? 1;

                ViewBag.KorisnikNivoId = korisnikNivoId;
                ViewBag.IsAdmin = trenutniKorisnik?.Uloga?.UlogaNaziv == "Admin";
                ViewBag.TrenutniKorisnikId = trenutniKorisnik?.KorisnikId;

                var struktura = _context.Poglavlja
                    .Include(p => p.Potpoglavlja)
                        .ThenInclude(sub => sub.Lekcije)
                            .ThenInclude(l => l.Nivo)
                    .ToList();

                return View(struktura);
            }
            catch (Exception)
            {
                ViewBag.Error = "Nemoguće je generisati strukturu stabla lekcija.";
                return View(new List<Poglavlje>());
            }
        }

        // GET: /Lekcije/PregledLekcijeDodaj 
        [Authorize(Roles = "Admin, Profesor")]
        public IActionResult PregledLekcijeDodaj(int potpoglavljeId, int poglavljeId)
        {
            try
            {
                var poglavlje = _context.Poglavlja.Find(poglavljeId);
                var potpoglavlje = _context.Potpoglavlja.Find(potpoglavljeId);

                ViewBag.PoglavljeNaziv = poglavlje?.PoglavljeNaziv;
                ViewBag.PotpoglavljeNaziv = potpoglavlje?.NazivPotpoglavlja;
                ViewBag.Nivoi = _context.Nivoi.ToList();

                var model = new Lekcija
                {
                    PoglavljeId = poglavljeId,
                    PotpoglavljeId = potpoglavljeId
                };

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "Sistemska greška prilikom inicijalizacije forme unutar stabla.";
                return RedirectToAction(nameof(PregledLekcija));
            }
        }

        // POST: /Lekcije/PregledLekcijeDodaj 
        [HttpPost]
        [Authorize(Roles = "Admin, Profesor")]
        [ValidateAntiForgeryToken]
        public IActionResult PregledLekcijeDodaj(Lekcija lekcija)
        {
            ModelState.Remove(nameof(lekcija.Poglavlje));
            ModelState.Remove(nameof(lekcija.Potpoglavlje));
            ModelState.Remove(nameof(lekcija.Nivo));
            ModelState.Remove(nameof(lekcija.Kreator));

            if (lekcija.LekcijaNaslov != null)
            {
                lekcija.LekcijaNaslov = lekcija.LekcijaNaslov.Trim();
            }
            if (lekcija.LekcijaTekst != null)
            {
                lekcija.LekcijaTekst = lekcija.LekcijaTekst.Trim();
            }

            if (!string.IsNullOrEmpty(lekcija.LekcijaNaslov) && _context.Lekcije.Any(l => l.LekcijaNaslov.ToLower() == lekcija.LekcijaNaslov.ToLower()))
            {
                ModelState.AddModelError("LekcijaNaslov", "Lekcija sa ovim naslovom već postoji. Izaberite drugi naslov.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var korisnik = GetTrenutniKorisnik();
                    if (korisnik == null) return RedirectToAction("Login", "Account");

                    lekcija.KorisnikId = korisnik.KorisnikId;

                    _context.Lekcije.Add(lekcija);
                    _context.SaveChanges();

                    return RedirectToAction(nameof(PregledLekcija));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Greška prilikom direktnog upisa lekcije iz stabla.");
                }
            }

            try
            {
                var poglavlje = _context.Poglavlja.Find(lekcija.PoglavljeId);
                var potpoglavlje = _context.Potpoglavlja.Find(lekcija.PotpoglavljeId);
                ViewBag.PoglavljeNaziv = poglavlje?.PoglavljeNaziv;
                ViewBag.PotpoglavljeNaziv = potpoglavlje?.NazivPotpoglavlja;
                ViewBag.Nivoi = _context.Nivoi.ToList();
            }
            catch { }
            return View(lekcija);
        }

        // GET: /Lekcije/PregledLekcijeIzmeni/5 
        [Authorize(Roles = "Admin, Profesor")]
        public IActionResult PregledLekcijeIzmeni(int id)
        {
            try
            {
                var lekcija = _context.Lekcije.Find(id);
                if (lekcija == null) return NotFound();

                var trenutniKorisnik = GetTrenutniKorisnik();
                bool isAdmin = trenutniKorisnik?.Uloga?.UlogaNaziv == "Admin";


                if (!isAdmin && lekcija.KorisnikId != trenutniKorisnik?.KorisnikId)
                {
                    return Forbid();
                }

                PopuniListe();
                return View(lekcija);
            }
            catch (Exception)
            {
                TempData["Error"] = "Sistem ne može da otvori izmenu izabrane lekcije.";
                return RedirectToAction(nameof(PregledLekcija));
            }
        }

        // POST: /Lekcije/PregledLekcijeIzmeni/5
        [HttpPost]
        [Authorize(Roles = "Admin, Profesor")]
        [ValidateAntiForgeryToken]
        public IActionResult PregledLekcijeIzmeni(Lekcija lekcija)
        {
            ModelState.Remove("Poglavlje");
            ModelState.Remove("Potpoglavlje");
            ModelState.Remove("Nivo");
            ModelState.Remove("Kreator");

            var trenutniKorisnik = GetTrenutniKorisnik();
            var postojeca = _context.Lekcije.Find(lekcija.LekcijaId);
            if (postojeca == null) return NotFound();

            bool isAdmin = trenutniKorisnik?.Uloga?.UlogaNaziv == "Admin";

            if (!isAdmin && postojeca.KorisnikId != trenutniKorisnik?.KorisnikId)
            {
                return Forbid();
            }

            if (lekcija.LekcijaNaslov != null)
            {
                lekcija.LekcijaNaslov = lekcija.LekcijaNaslov.Trim();
            }
            if (lekcija.LekcijaTekst != null)
            {
                lekcija.LekcijaTekst = lekcija.LekcijaTekst.Trim();
            }

            if (!string.IsNullOrEmpty(lekcija.LekcijaNaslov) && _context.Lekcije.Any(l => l.LekcijaNaslov.ToLower() == lekcija.LekcijaNaslov.ToLower() && l.LekcijaId != lekcija.LekcijaId))
            {
                ModelState.AddModelError("LekcijaNaslov", "Lekcija sa ovim naslovom već postoji. Izaberite drugi naslov.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    postojeca.LekcijaNaslov = lekcija.LekcijaNaslov;
                    postojeca.LekcijaTekst = lekcija.LekcijaTekst;
                    postojeca.PoglavljeId = lekcija.PoglavljeId;
                    postojeca.PotpoglavljeId = lekcija.PotpoglavljeId;
                    postojeca.NivoId = lekcija.NivoId;

                    _context.SaveChanges();

                    return RedirectToAction(nameof(PregledLekcija));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Podaci su u međuvremenu izmenjeni od strane drugog korisnika.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Nije moguće snimiti izmene. Proverite vezu sa bazom.");
                }
            }

            try { PopuniListe(); } catch { }
            return View(lekcija);
        }

        // POST: /Lekcije/PregledLekcijeObrisi/5 
        [HttpPost]
        [Authorize(Roles = "Admin, Profesor")]
        [ValidateAntiForgeryToken]
        public IActionResult PregledLekcijeObrisi(int id)
        {
            try
            {
                var lekcija = _context.Lekcije.Find(id);
                if (lekcija == null) return NotFound();

                var trenutniKorisnik = GetTrenutniKorisnik();
                bool isAdmin = trenutniKorisnik?.Uloga?.UlogaNaziv == "Admin";

                if (!isAdmin && lekcija.KorisnikId != trenutniKorisnik?.KorisnikId)
                {
                    return Forbid();
                }

                _context.Lekcije.Remove(lekcija);
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Lekcija se ne može obrisati jer je vezana za druge entitete u stablu.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Neočekivana greška na serveru prilikom brisanja lekcije.";
            }
            return RedirectToAction(nameof(PregledLekcija));
        }

        private async Task PopuniListeLekcijaAsync()
        {
            ViewBag.Poglavlja = await _context.Poglavlja.OrderBy(p => p.PoglavljeNaziv).ToListAsync();
            ViewBag.Potpoglavlja = await _context.Potpoglavlja.OrderBy(p => p.NazivPotpoglavlja).ToListAsync();
            ViewBag.Nivoi = await _context.Nivoi.ToListAsync();
            ViewBag.Korisnici = await _context.Korisnici.ToListAsync();
        }

        private void PopuniListe()
        {
            ViewBag.Poglavlja = _context.Poglavlja.OrderBy(p => p.PoglavljeNaziv).ToList();
            ViewBag.Potpoglavlja = _context.Potpoglavlja.OrderBy(p => p.NazivPotpoglavlja).ToList();
            ViewBag.Nivoi = _context.Nivoi.ToList();
            ViewBag.Korisnici = _context.Korisnici.ToList();
        }
    }
}
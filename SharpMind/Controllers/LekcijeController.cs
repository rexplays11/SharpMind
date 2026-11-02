using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpMind.Controllers
{
    [Authorize(Roles = "Admin, Profesor")] 
    public class LekcijeController : Controller
    {
        private readonly SharpMindContext _context;

        public LekcijeController(SharpMindContext context)
        {
            _context = context;
        }

        // INDEX METODA (Prikaz i upravljanje listom)

        // GET: /Lekcije
        [AllowAnonymous] 
        public IActionResult Index(string sortOrder, int page = 1, int pageSize = 25)
        {
            try
            {
                if (pageSize < 1) pageSize = 25;


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

        [Authorize(Roles = "Admin, Profesor")]
        public async Task<IActionResult> Create(int? poglavljeId)
        {
            try
            {
                var ulogovaniKorisnik = await _context.Korisnici
                    .Include(k => k.Uloga)
                    .FirstOrDefaultAsync(k => k.KorisnikIme == User.Identity.Name);

                if (ulogovaniKorisnik == null) return RedirectToAction("Login", "Nalog");

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

        // CREATE (POST): 
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
                    var username = User.Identity?.Name;
                    var ulogovaniKorisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.KorisnikIme == username);

                    if (ulogovaniKorisnik != null)
                    {
                        lekcija.KorisnikId = ulogovaniKorisnik.KorisnikId;
                    }

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
        public IActionResult Edit(int id)
        {
            try
            {
                var lekcija = _context.Lekcije.Find(id);
                if (lekcija == null) return NotFound();

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
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Lekcija lekcija)
        {
            ModelState.Remove("Poglavlje");
            ModelState.Remove("Potpoglavlje");
            ModelState.Remove("Nivo");
            ModelState.Remove("Kreator");

            if (lekcija.LekcijaNaslov != null)
            {
                lekcija.LekcijaNaslov = lekcija.LekcijaNaslov.Trim();
            }


            if (!string.IsNullOrEmpty(lekcija.LekcijaNaslov) && _context.Lekcije.Any(l => l.LekcijaNaslov.ToLower() == lekcija.LekcijaNaslov.ToLower() && l.LekcijaId != lekcija.LekcijaId))
            {
                ModelState.AddModelError("LekcijaNaslov", "Lekcija sa ovim naslovom već postoji. Izaberite drugi naslov.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lekcija);
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

        // GET: /Lekcije/Details/5
        [AllowAnonymous] 
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


                if (!User.IsInRole("Admin") && !User.IsInRole("Profesor"))
                {
                    int korisnikNivoId = 1; // Gost
                    if (User.Identity != null && User.Identity.IsAuthenticated)
                    {
                        var username = User.Identity.Name;
                        var korisnik = _context.Korisnici.FirstOrDefault(k => k.KorisnikIme == username);
                        if (korisnik != null)
                        {
                            korisnikNivoId = korisnik.NivoId;
                        }
                    }


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
                return RedirectToAction(nameof(Index));
            }
        }



        // POST: /Lekcije/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var lekcija = _context.Lekcije.Find(id);
                if (lekcija != null)
                {
                    _context.Lekcije.Remove(lekcija);
                    _context.SaveChanges();
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Lekcija ne može biti obrisan jer je povezana sa drugim podacima u aplikaciji.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Došlo je do greške na serveru prilikom brisanja lekcije.";
            }
            return RedirectToAction(nameof(Index));
        }

        // OSTALE METODE (Pomoćne i sporedne akcije)

        // GET: /Lekcije/Lista
        [AllowAnonymous]
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

        // GET: /Lekcije/GetPoglavlja
        [HttpGet]
        [AllowAnonymous]
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

        // GET: /Lekcije/GetPotpoglavlja?poglavljeId=5
        [HttpGet]
        [AllowAnonymous]
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
        [AllowAnonymous]
        public IActionResult PregledLekcija()
        {
            try
            {

                int korisnikNivoId = 1;
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var username = User.Identity.Name;
                    var korisnik = _context.Korisnici.FirstOrDefault(k => k.KorisnikIme == username);
                    if (korisnik != null)
                    {
                        korisnikNivoId = korisnik.NivoId;
                    }
                }
                ViewBag.KorisnikNivoId = korisnikNivoId;


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
                    var username = User.Identity?.Name;
                    var korisnik = _context.Korisnici.FirstOrDefault(k => k.KorisnikIme == username);

                    if (korisnik != null)
                    {
                        lekcija.KorisnikId = korisnik.KorisnikId;
                    }

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
public IActionResult PregledLekcijeIzmeni(int id)
{
    try
    {
        var lekcija = _context.Lekcije.Find(id);
        if (lekcija == null) return NotFound();

        PopuniListe();
        return View(lekcija);
    }
    catch (Exception)
    {
        TempData["Error"] = "Sistem ne može da otvori izmenu izabrane lekcije.";
        return RedirectToAction(nameof(Index));
    }
}
        // POST: /Lekcije/PregledLekcijeIzmeni/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PregledLekcijeIzmeni(Lekcija lekcija)
        {
            ModelState.Remove("Poglavlje");
            ModelState.Remove("Potpoglavlje");
            ModelState.Remove("Nivo");
            ModelState.Remove("Kreator");

            if (lekcija.LekcijaNaslov != null)
            {
                lekcija.LekcijaNaslov = lekcija.LekcijaNaslov.Trim();
            }

            // Provera da li neka druga lekcija već koristi ovaj naslov
            if (!string.IsNullOrEmpty(lekcija.LekcijaNaslov) && _context.Lekcije.Any(l => l.LekcijaNaslov.ToLower() == lekcija.LekcijaNaslov.ToLower() && l.LekcijaId != lekcija.LekcijaId))
            {
                ModelState.AddModelError("LekcijaNaslov", "Lekcija sa ovim naslovom već postoji. Izaberite drugi naslov.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lekcija);
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
        [ValidateAntiForgeryToken] 
        public IActionResult PregledLekcijeObrisi(int id)
        {
            try
            {
                var lekcija = _context.Lekcije.Find(id);
                if (lekcija != null)
                {
                    _context.Lekcije.Remove(lekcija);
                    _context.SaveChanges();
                }
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpMindApi.Controllers
{
    [Authorize]
    public class PitanjaController : Controller
    {
        private readonly SharpMindContext _context;

        public PitanjaController(SharpMindContext context) => _context = context;

        // INDEX METODA (Prikaz i upravljanje listom)

        // GET: /Pitanja
        [AllowAnonymous]
        public IActionResult Index(string sortOrder, int page = 1, int pageSize = 25)
        {
            try
            {
                if (pageSize < 1) pageSize = 25;

                ViewBag.IdSortParm = sortOrder == "id_asc" ? "id_desc" : "id_asc";
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentPageSize = pageSize;

                var query = _context.Pitanja
                    .Include(p => p.Nivo)
                    .Include(p => p.Kreator)
                    .Include(p => p.Odgovori)
                    .AsQueryable();

                switch (sortOrder)
                {
                    case "id_asc":
                        query = query.OrderBy(p => p.PitanjeId);
                        break;
                    case "id_desc":
                    default:
                        query = query.OrderByDescending(p => p.PitanjeId);
                        break;
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;

                var pitanja = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                return View(pitanja);
            }
            catch (Exception)
            {
                ViewBag.Error = "Sistemska greška pri učitavanju liste pitanja.";
                return View(new List<Pitanje>());
            }
        }

        // CREATE METODE (Kreiranje nova pitanja)

        // GET: /Pitanja/Create
        [Authorize(Roles = "Admin, Profesor")]
        public IActionResult Create()
        {
            try
            {
                PopuniListe();
            }
            catch (Exception)
            {
                ViewBag.Error = "Nije moguće učitati (nivoe/korisnike) sa servera.";
            }
            return View();
        }

        // POST: /Pitanja/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Profesor")]
        public IActionResult Create(Pitanje pitanje)
        {
            ModelState.Remove("Kreator");
            ModelState.Remove("Nivo");
            ModelState.Remove("Odgovori");

            if (pitanje != null && !string.IsNullOrWhiteSpace(pitanje.PitanjeTekst))
            {
                bool postoji = _context.Pitanja.Any(p => p.PitanjeTekst.ToLower() == pitanje.PitanjeTekst.Trim().ToLower());
                if (postoji)
                {
                    ModelState.AddModelError("PitanjeTekst", "Ovo pitanje već postoji u bazi podataka.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    pitanje.PitanjeTekst = pitanje.PitanjeTekst.Trim();
                    _context.Pitanja.Add(pitanje);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Greška pri čuvanju pitanja u bazu podataka.");
                }
            }

            try { PopuniListe(); } catch { }
            return View(pitanje);
        }

        // EDIT METODE (Izmena postojećeg pitanja)

        // GET: /Pitanja/Edit/5
        [Authorize(Roles = "Admin, Profesor")]
        public IActionResult Edit(int id)
        {
            try
            {
                var pitanje = _context.Pitanja.Find(id);
                if (pitanje == null) return NotFound();

                PopuniListe();
                return View(pitanje);
            }
            catch (Exception)
            {
                TempData["Error"] = "Greška pri otvaranju forme za izmenu pitanja.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Pitanja/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Profesor")]
        public IActionResult Edit(Pitanje pitanje)
        {
            ModelState.Remove("Kreator");
            ModelState.Remove("Nivo");
            ModelState.Remove("Odgovori");

            if (pitanje != null && !string.IsNullOrWhiteSpace(pitanje.PitanjeTekst))
            {
                bool postoji = _context.Pitanja.Any(p => p.PitanjeTekst.ToLower() == pitanje.PitanjeTekst.Trim().ToLower()
                                                              && p.PitanjeId != pitanje.PitanjeId);
                if (postoji)
                {
                    ModelState.AddModelError("PitanjeTekst", "Drugo pitanje već sadrži ovaj tekst.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    pitanje.PitanjeTekst = pitanje.PitanjeTekst.Trim();
                    _context.Pitanja.Update(pitanje);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Podaci o ovom pitanju su u međuvremenu izmenjeni u bazi.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Sistemska greška pri ažuriranju pitanja.");
                }
            }

            try { PopuniListe(); } catch { }
            return View(pitanje);
        }

        // GET: /Pitanja/Details/5
        //  Detaljan pregled jednog konkretnog pitanja. Omogućava detaljan uvid u pitanje i njegove odgovore.
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            try
            {
                var pitanje = _context.Pitanja
                    .Include(p => p.Nivo)
                    .Include(p => p.Kreator)
                    .Include(p => p.Odgovori)
                    .FirstOrDefault(p => p.PitanjeId == id);

                if (pitanje == null) return NotFound();

                return View(pitanje);
            }
            catch (Exception)
            {
                TempData["Error"] = "Greška pri učitavanju detalja o pitanju.";
                return RedirectToAction(nameof(Index));
            }
        }

        // DELETE METODA (Brisanje pitanja)
        // POST: /Pitanja/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Profesor")]
        public IActionResult Delete(int id)
        {
            try
            {
                var pitanje = _context.Pitanja
                    .Include(p => p.Odgovori)
                    .FirstOrDefault(p => p.PitanjeId == id);

                if (pitanje != null)
                {
                    if (pitanje.Odgovori != null && pitanje.Odgovori.Any())
                    {
                        _context.Odgovori.RemoveRange(pitanje.Odgovori);
                    }

                    _context.Pitanja.Remove(pitanje);

                    _context.SaveChanges();

                    TempData["Success"] = "Pitanje i svi njegovi odgovori su uspešno obrisani.";
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Pitanje se ne može obrisati jer je već zabeleženo u istoriji rešavanja testova kod korisnika.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Sistemska greška pri brisanju pitanja.";
            }

            return RedirectToAction(nameof(Index));
        }

        // OSTALE METODE (Pomoćne i sporedne akcije)

        // GET: /Pitanja/Objedinjeno
        [Authorize(Roles = "Admin, Profesor")]
        public async Task<IActionResult> Objedinjeno(string sortOrder, string pretragaNaslova, int? nivoId, int? kreatorId, int page = 1, int pageSize = 25)
        {
            try
            {
                if (pageSize < 1) pageSize = 25;
                ViewBag.IdSortParm = sortOrder == "id_asc" ? "id_desc" : "id_asc";
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentPageSize = pageSize;

                var trenutniKorisnikIme = User.Identity?.Name;
                var trenutniKorisnik = await _context.Korisnici
                    .Include(k => k.Uloga)
                    .FirstOrDefaultAsync(k => k.KorisnikIme == trenutniKorisnikIme);

                ViewBag.TrenutniKorisnikId = trenutniKorisnik?.KorisnikId;
                ViewBag.IsAdmin = trenutniKorisnik?.Uloga?.UlogaNaziv == "Admin";

                ViewBag.Nivoi = await _context.Nivoi
                    .Where(n => _context.Pitanja.Any(p => p.NivoId == n.NivoId))
                    .OrderBy(n => n.GranicniPoeniNivoa)
                    .ToListAsync();

                ViewBag.Korisnici = await _context.Korisnici
                    .Where(k => _context.Pitanja.Any(p => p.KorisnikId == k.KorisnikId))
                    .OrderBy(k => k.KorisnikIme)
                    .ToListAsync();

                var upit = _context.Pitanja
                    .Include(p => p.Kreator)
                    .Include(p => p.Nivo)
                    .Include(p => p.Odgovori)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(pretragaNaslova))
                {
                    upit = upit.Where(p => p.PitanjeTekst.Contains(pretragaNaslova));
                }

                if (nivoId.HasValue) upit = upit.Where(p => p.NivoId == nivoId.Value);
                if (kreatorId.HasValue) upit = upit.Where(p => p.KorisnikId == kreatorId.Value);

                switch (sortOrder)
                {
                    case "id_asc": upit = upit.OrderBy(p => p.PitanjeId); break;
                    case "id_desc":
                    default: upit = upit.OrderByDescending(p => p.PitanjeId); break;
                }

                var totalItems = await upit.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;

                ViewBag.TrenutnaPretraga = pretragaNaslova;
                ViewBag.TrenutniNivo = nivoId;
                ViewBag.TrenutniKreator = kreatorId;

                var validnaPitanjaIds = await _context.Odgovori
                    .GroupBy(o => o.PitanjeId)
                    .Where(g => g.Any(o => o.OdgovorTacnost == true) && g.Count() >= 2)
                    .Select(g => g.Key)
                    .ToListAsync();

                var nevalidnaUpit = _context.Pitanja
                    .Include(p => p.Kreator)
                    .Include(p => p.Odgovori)
                    .Where(p => !validnaPitanjaIds.Contains(p.PitanjeId));

                bool isAdmin = trenutniKorisnik?.Uloga?.UlogaNaziv == "Admin";
                if (!isAdmin && trenutniKorisnik != null)
                {
                    nevalidnaUpit = nevalidnaUpit.Where(p => p.KorisnikId == trenutniKorisnik.KorisnikId);
                }

                var nevalidnaPitanja = await nevalidnaUpit.ToListAsync();

                if (nevalidnaPitanja.Any())
                {
                    ViewBag.NevalidnaPitanjaList = nevalidnaPitanja;
                }

                var rezervisanaPitanja = await upit.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                return View(rezervisanaPitanja);
            }
            catch (Exception)
            {
                TempData["Greska"] = "Došlo je do greške prilikom generisanja objedinjene liste.";
                return View(new List<Pitanje>());
            }
        }

        // GET: Pitanja/NovoPitanje
        [Authorize(Roles = "Admin, Profesor")]
        public IActionResult NovoPitanje()
        {
            try
            {
                PopuniListe();
            }
            catch (Exception)
            {
                TempData["Greska"] = "Nije moguće učitati šifrarnike za kreiranje novog pitanja.";
                return RedirectToAction("Objedinjeno");
            }
            return View(new Pitanje());
        }

        // POST: Pitanja/NovoPitanje
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Profesor")]
        public IActionResult NovoPitanje(Pitanje pitanje)
        {
            ModelState.Remove("Kreator");
            ModelState.Remove("Nivo");
            ModelState.Remove("Odgovori");

            if (pitanje != null && !string.IsNullOrWhiteSpace(pitanje.PitanjeTekst))
            {
                bool postoji = _context.Pitanja.Any(p => p.PitanjeTekst.ToLower() == pitanje.PitanjeTekst.Trim().ToLower());
                if (postoji)
                {
                    ModelState.AddModelError("PitanjeTekst", "Ovo pitanje već postoji u sistemu.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    pitanje.PitanjeTekst = pitanje.PitanjeTekst.Trim();
                    _context.Pitanja.Add(pitanje);
                    _context.SaveChanges();
                    return RedirectToAction("Objedinjeno");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Sistemska greška pri snimanju novog pitanja.");
                }
            }

            try { PopuniListe(); } catch { }
            return View(pitanje);
        }

        // GET: /Pitanja/IzmenaPitanja/5
        [Authorize(Roles = "Admin, Profesor")]
        public IActionResult IzmenaPitanja(int id)
        {
            try
            {
                var pitanje = _context.Pitanja.Find(id);
                if (pitanje == null) return NotFound();

                PopuniListe();
                return View(pitanje);
            }
            catch (Exception)
            {
                TempData["Greska"] = "Nije moguće otvoriti pitanje za izmenu.";
                return RedirectToAction("Objedinjeno");
            }
        }

        // POST: /Pitanja/IzmenaPitanja
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Profesor")]
        public IActionResult IzmenaPitanja(Pitanje pitanje)
        {
            ModelState.Remove("Kreator");
            ModelState.Remove("Nivo");
            ModelState.Remove("Odgovori");

            if (pitanje != null && !string.IsNullOrWhiteSpace(pitanje.PitanjeTekst))
            {
                bool postoji = _context.Pitanja.Any(p => p.PitanjeTekst.ToLower() == pitanje.PitanjeTekst.Trim().ToLower()
                                                              && p.PitanjeId != pitanje.PitanjeId);
                if (postoji)
                {
                    ModelState.AddModelError("PitanjeTekst", "Drugo pitanje već sadrži ovaj tekst.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    pitanje.PitanjeTekst = pitanje.PitanjeTekst.Trim();
                    _context.Pitanja.Update(pitanje);
                    _context.SaveChanges();
                    return RedirectToAction("Objedinjeno");
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Neko je već izmenio ovo pitanje u bazi podataka.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Sistemska greška prilikom izmene pitanja.");
                }
            }

            try { PopuniListe(); } catch { }
            return View(pitanje);
        }

        // POST: /Pitanja/ObrisiPitanjeSaOdgovorima/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Profesor")]
        public async Task<IActionResult> ObrisiPitanjeSaOdgovorima(int id)
        {
            try
            {
                var pitanje = await _context.Pitanja
                    .Include(p => p.Odgovori)
                    .FirstOrDefaultAsync(p => p.PitanjeId == id);

                if (pitanje == null) return NotFound();

                if (pitanje.Odgovori != null && pitanje.Odgovori.Any())
                {
                    _context.Odgovori.RemoveRange(pitanje.Odgovori);
                }

                _context.Pitanja.Remove(pitanje);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Greska"] = "Pitanje se ne može obrisati sa odgovorima jer je već korišćeno u testovima učenika.";
            }
            catch (Exception)
            {
                TempData["Greska"] = "Neočekivana serverska greška pri brisanju pitanja.";
            }

            return RedirectToAction("Objedinjeno");
        }

        private void PopuniListe()
        {
            ViewBag.Nivoi = _context.Nivoi.OrderBy(n => n.NazivNivoa).ToList();

            ViewBag.Korisnici = _context.Korisnici
                                        .Include(k => k.Uloga)
                                        .OrderBy(k => k.KorisnikIme)
                                        .ToList();
        }
    }
}
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
    public class PotpoglavljaController : Controller
    {
        private readonly SharpMindContext _context;
        public PotpoglavljaController(SharpMindContext context) => _context = context;

        // INDEX METODA (Prikaz i upravljanje listom)

        // GET: /Potpoglavlja
        [AllowAnonymous]
        public IActionResult Index(int? poglavljeId, string sortOrder, int page = 1, int pageSize = 25)
        {
            try
            {
                if (pageSize < 1) pageSize = 25;

                ViewBag.IdSortParm = sortOrder == "id_asc" ? "id_desc" : "id_asc";
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentPageSize = pageSize;
                ViewBag.SelectedPoglavljeId = poglavljeId;

                var query = _context.Potpoglavlja
                    .Include(p => p.Poglavlje)
                    .AsQueryable();

                if (poglavljeId.HasValue)
                {
                    query = query.Where(p => p.PoglavljeId == poglavljeId.Value);
                    var poglavlje = _context.Poglavlja.Find(poglavljeId.Value);
                    ViewBag.ImePoglavlja = poglavlje?.PoglavljeNaziv ?? "Nepoznato";
                }
                else
                {
                    ViewBag.ImePoglavlja = "Sva Poglavlja";
                }

                switch (sortOrder)
                {
                    case "id_asc":
                        query = query.OrderBy(p => p.PotpoglavljeId);
                        break;
                    case "id_desc":
                    default:
                        query = query.OrderByDescending(p => p.PotpoglavljeId);
                        break;
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;

                var stavke = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                return View(stavke);
            }
            catch (Exception)
            {
                ViewBag.Error = "Sistemska greška pri učitavanju liste potpoglavlja.";
                return View(new List<Potpoglavlje>());
            }
        }

        // CREATE METODE (Kreiranje nove potpoglavlje)

        // GET: /Potpoglavlja/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            try
            {
                PopuniListe();
            }
            catch (Exception)
            {
                ViewBag.Error = "Nije moguće učitati spisak roditeljskih poglavlja sa servera.";
            }
            return View();
        }

        // POST: /Potpoglavlja/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(Potpoglavlje potpoglavlje)
        {
            ModelState.Remove(nameof(potpoglavlje.Poglavlje));
            ModelState.Remove(nameof(potpoglavlje.Lekcije));

            bool postoji = _context.Potpoglavlja.Any(p => p.NazivPotpoglavlja == potpoglavlje.NazivPotpoglavlja
                                                          && p.PoglavljeId == potpoglavlje.PoglavljeId);
            if (postoji)
            {
                ModelState.AddModelError("NazivPotpoglavlja", "Potpoglavlje sa ovim nazivom već postoji u izabranom poglavlju.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Potpoglavlja.Add(potpoglavlje);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Greška prilikom upisa novog potpoglavlja u bazu podataka.");
                }
            }

            try { PopuniListe(); } catch { }
            return View(potpoglavlje);
        }

        // EDIT METODE (Izmena postojeće potpoglavlja)

        // GET: /Potpoglavlja/Edit/5
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            try
            {
                var potpoglavlje = _context.Potpoglavlja.Find(id);
                if (potpoglavlje == null) return NotFound();

                PopuniListe();
                return View(potpoglavlje);
            }
            catch (Exception)
            {
                TempData["Error"] = "Greška pri otvaranju forme za izmenu potpoglavlja.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Potpoglavlja/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(Potpoglavlje potpoglavlje)
        {
            ModelState.Remove(nameof(potpoglavlje.Poglavlje));
            ModelState.Remove(nameof(potpoglavlje.Lekcije));

            bool postoji = _context.Potpoglavlja.Any(p => p.NazivPotpoglavlja == potpoglavlje.NazivPotpoglavlja
                                                          && p.PoglavljeId == potpoglavlje.PoglavljeId
                                                          && p.PotpoglavljeId != potpoglavlje.PotpoglavljeId);
            if (postoji)
            {
                ModelState.AddModelError("NazivPotpoglavlja", "Potpoglavlje sa ovim nazivom već postoji u izabranom poglavlju.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Potpoglavlja.Update(potpoglavlje);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Podaci su u međuvremenu izmenjeni od strane drugog administratora.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Sistemska greška prilikom ažuriranja potpoglavlja.");
                }
            }

            try { PopuniListe(); } catch { }
            return View(potpoglavlje);
        }

        // GET: /Potpoglavlja/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var potpoglavlje = await _context.Potpoglavlja
                    .Include(p => p.Poglavlje)
                    .FirstOrDefaultAsync(m => m.PotpoglavljeId == id);

                if (potpoglavlje == null) return NotFound();

                return View(potpoglavlje);
            }
            catch (Exception)
            {
                TempData["Error"] = "Greška pri učitavanju detalja potpoglavlja.";
                return RedirectToAction(nameof(Index));
            }
        }

        // DELETE METODA (Brisanje potpoglavlja)

        // POST: /Potpoglavlja/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            try
            {
                var pp = _context.Potpoglavlja.Find(id);
                if (pp != null)
                {
                    _context.Potpoglavlja.Remove(pp);
                    _context.SaveChanges();
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Potpoglavlje ne može biti obrisano jer sadrži lekcije unutar sebe.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Sistemska greška pri brisanju potpoglavlja.";
            }
            return RedirectToAction(nameof(Index));
        }

        // OSTALE METODE (Pomoćne i sporedne akcije)

        // GET: /Potpoglavlja/PoPoglavlju/5
        [AllowAnonymous]
        public IActionResult PoPoglavlju(int id)
        {
            try
            {
                var poglavlje = _context.Poglavlja.Find(id);
                if (poglavlje == null) return NotFound();

                var lista = _context.Potpoglavlja
                    .Where(p => p.PoglavljeId == id)
                    .Include(p => p.Poglavlje)
                    .ToList();

                ViewBag.ImePoglavlja = poglavlje.PoglavljeNaziv;
                ViewBag.Tip = "Potpoglavlja";

                return View(lista);
            }
            catch (Exception)
            {
                TempData["Error"] = "Došlo je do greške pri filtriranju potpoglavlja za izabrano poglavlje.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Potpoglavlja/PoPotpoglavlju/5
        [AllowAnonymous]
        public IActionResult PoPotpoglavlju(int id)
        {
            try
            {
                var lekcije = _context.Lekcije
                    .Include(l => l.Nivo)
                    .Include(l => l.Potpoglavlje)
                    .Where(l => l.PotpoglavljeId == id)
                    .ToList();

                if (lekcije == null || !lekcije.Any())
                {
                    ViewBag.Poruka = "Nema pronađenih lekcija za ovo potpoglavlje.";
                }

                var potpoglavlje = _context.Potpoglavlja.Find(id);
                ViewBag.Naslov = potpoglavlje?.NazivPotpoglavlja;

                return View("Lista", lekcije);
            }
            catch (Exception)
            {
                ViewBag.Error = "Greška prilikom povlačenja lekcija za odabrano potpoglavlje.";
                return View("Lista", new List<Lekcija>());
            }
        }

        private void PopuniListe()
        {
            ViewBag.Poglavlja = _context.Poglavlja.OrderBy(p => p.PoglavljeNaziv).ToList();
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpMind.Controllers
{
    [Authorize]
    public class PoglavljaController : Controller
    {
        private readonly SharpMindContext _context;

        public PoglavljaController(SharpMindContext context)
        {
            _context = context;
        }

        // INDEX METODA (Prikaz i upravljanje listom)

        // GET: /Poglavlja
        [AllowAnonymous]
        public IActionResult Index(string sortOrder, int page = 1, int pageSize = 25)
        {
            try
            {
                if (pageSize < 1) pageSize = 25;

                ViewBag.IdSortParm = sortOrder == "id_asc" ? "id_desc" : "id_asc";
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentPageSize = pageSize;

                var query = _context.Poglavlja
                    .Include(p => p.Potpoglavlja)
                    .AsQueryable();

                switch (sortOrder)
                {
                    case "id_asc":
                        query = query.OrderBy(p => p.PoglavljeId);
                        break;
                    case "id_desc":
                    default:
                        query = query.OrderByDescending(p => p.PoglavljeId);
                        break;
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;

                var poglavlja = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                return View(poglavlja);
            }
            catch (Exception)
            {
                ViewBag.Error = "Sistemska greška pri učitavanju liste poglavlja.";
                return View(new List<Poglavlje>());
            }
        }

        // CREATE METODE (Kreiranje nova poglavlja)

        // GET: /Poglavlja/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Poglavlja/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(Poglavlje poglavlje)
        {
            ModelState.Remove(nameof(poglavlje.Potpoglavlja));

            // Ne dozvoljavam kreiranje novog poglavlja sa istim imenom
            if (poglavlje != null && !string.IsNullOrWhiteSpace(poglavlje.PoglavljeNaziv))
            {
                bool postoji = _context.Poglavlja.Any(p => p.PoglavljeNaziv.ToLower() == poglavlje.PoglavljeNaziv.Trim().ToLower());
                if (postoji)
                {
                    ModelState.AddModelError("PoglavljeNaziv", "Poglavlje sa ovim nazivom već postoji u bazi podataka.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    poglavlje.PoglavljeNaziv = poglavlje.PoglavljeNaziv.Trim();
                    _context.Poglavlja.Add(poglavlje);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Nije moguće sačuvati novo poglavlje. Proverite vezu sa bazom.");
                }
            }

            return View(poglavlje);
        }

        // EDIT METODE (Izmena postojećeg poglavlja)

        // GET: /Poglavlja/Edit/5
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            try
            {
                var poglavlje = _context.Poglavlja.Find(id);
                if (poglavlje == null) return NotFound();

                return View(poglavlje);
            }
            catch (Exception)
            {
                TempData["Error"] = "Greška pri otvaranju poglavlja za izmenu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Poglavlja/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(Poglavlje poglavlje)
        {
            ModelState.Remove(nameof(poglavlje.Potpoglavlja));

            if (poglavlje != null && !string.IsNullOrWhiteSpace(poglavlje.PoglavljeNaziv))
            {
                bool postoji = _context.Poglavlja.Any(p => p.PoglavljeNaziv.ToLower() == poglavlje.PoglavljeNaziv.Trim().ToLower()
                                                              && p.PoglavljeId != poglavlje.PoglavljeId);
                if (postoji)
                {
                    ModelState.AddModelError("PoglavljeNaziv", "Drugo poglavlje već koristi ovaj naziv.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    poglavlje.PoglavljeNaziv = poglavlje.PoglavljeNaziv.Trim();
                    _context.Poglavlja.Update(poglavlje);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Podaci o ovom poglavlju su u međuvremenu izmenjeni od strane drugog korisnika.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Sistemska greška prilikom čuvanja izmena.");
                }
            }

            return View(poglavlje);
        }

        // GET: /Poglavlja/Details/5
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            try
            {
                var poglavlje = _context.Poglavlja
                    .Include(p => p.Potpoglavlja)
                    .FirstOrDefault(m => m.PoglavljeId == id);

                if (poglavlje == null) return NotFound();

                return View(poglavlje);
            }
            catch (Exception)
            {
                TempData["Error"] = "Greška pri učitavanju detalja o poglavlju.";
                return RedirectToAction(nameof(Index));
            }
        }

        // DELETE METODA (Brisanje poglavlja)

        // POST: /Poglavlja/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            try
            {
                var poglavlje = _context.Poglavlja.Find(id);
                if (poglavlje != null)
                {
                    _context.Poglavlja.Remove(poglavlje);
                    _context.SaveChanges();
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Ovo poglavlje ne može biti obrisano jer sadrži aktivna potpoglavlja ili lekcije.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Došlo je do neočekivane greške na serveru prilikom brisanja poglavlja.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
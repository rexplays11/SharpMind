using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpMindApi.Controllers
{
    [Authorize(Roles = "Admin, Profesor")] 
    public class NivoiController : Controller
    {
        private readonly SharpMindContext _context;

        public NivoiController(SharpMindContext context)
        {
            _context = context;
        }

        // INDEX METODA (Prikaz i upravljanje listom)

        // GET: /Nivoi
        [AllowAnonymous] 
        public IActionResult Index(string sortOrder, int page = 1, int pageSize = 25)
        {
            try
            {
                if (pageSize < 1) pageSize = 25;

                ViewBag.IdSortParm = sortOrder == "id_asc" ? "id_desc" : "id_asc";
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentPageSize = pageSize;

                var query = _context.Nivoi.AsQueryable();


                switch (sortOrder)
                {
                    case "id_asc":
                        query = query.OrderBy(n => n.NivoId);
                        break;
                    case "id_desc":
                    default:
                        query = query.OrderByDescending(n => n.NivoId);
                        break;
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;

                var nivoi = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                return View(nivoi);
            }
            catch (Exception)
            {
                ViewBag.Error = "Sistemska greška pri dobavljanju nivoa iz baze podataka.";
                return View(new List<Nivo>());
            }
        }

        // CREATE METODE (Kreiranje novog nivoa)

        // GET: /Nivoi/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Nivoi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Nivo nivo)
        {
            if (nivo.NazivNivoa != null)
            {
                nivo.NazivNivoa = nivo.NazivNivoa.Trim();
            }

            if (!string.IsNullOrEmpty(nivo.NazivNivoa) && _context.Nivoi.Any(n => n.NazivNivoa.ToLower() == nivo.NazivNivoa.ToLower()))
            {
                ModelState.AddModelError("NazivNivoa", "Nivo sa ovim nazivom već postoji.");
            }

            if (_context.Nivoi.Any(n => n.GranicniPoeniNivoa == nivo.GranicniPoeniNivoa))
            {
                ModelState.AddModelError("GranicniPoeniNivoa", "Granični poeni za ovaj nivo su već zauzeti.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Nivoi.Add(nivo);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Nije moguće sačuvati novi nivo. Proverite konekciju sa bazom.");
                }
            }
            return View(nivo);
        }

        // EDIT METODE (Izmena postojećeg nivoa)

        // GET: /Nivoi/Edit/5
        public IActionResult Edit(int id)
        {
            try
            {
                var nivo = _context.Nivoi.Find(id);
                if (nivo == null) return NotFound();
                return View(nivo);
            }
            catch (Exception)
            {
                TempData["Error"] = "Greška pri otvaranju nivoa za izmenu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Nivoi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Nivo nivo)
        {
            if (nivo.NazivNivoa != null)
            {
                nivo.NazivNivoa = nivo.NazivNivoa.Trim();
            }

            if (!string.IsNullOrEmpty(nivo.NazivNivoa) && _context.Nivoi.Any(n => n.NazivNivoa.ToLower() == nivo.NazivNivoa.ToLower() && n.NivoId != nivo.NivoId))
            {
                ModelState.AddModelError("NazivNivoa", "Drugi nivo već koristi ovaj naziv.");
            }

            if (_context.Nivoi.Any(n => n.GranicniPoeniNivoa == nivo.GranicniPoeniNivoa && n.NivoId != nivo.NivoId))
            {
                ModelState.AddModelError("GranicniPoeniNivoa", "Drugi nivo već koristi ove granične poene.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Nivoi.Update(nivo);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Podaci o ovom nivou su promenjeni u bazi u međuvremenu.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Sistemska greška prilikom čuvanja izmena.");
                }
            }
            return View(nivo);
        }

        // GET: /Nivoi/Details/5
        [AllowAnonymous] 
        public IActionResult Details(int id)
        {
            try
            {
                var nivo = _context.Nivoi.Find(id);
                if (nivo == null) return NotFound();
                return View(nivo);
            }
            catch (Exception)
            {
                TempData["Error"] = "Greška pri učitavanju detalja o nivou.";
                return RedirectToAction(nameof(Index));
            }
        }

        // DELETE METODA (Brisanje nivoa)

        // POST: /Nivoi/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public IActionResult Delete(int id)
        {
            try
            {
                var nivo = _context.Nivoi.Find(id);
                if (nivo != null)
                {
                    _context.Nivoi.Remove(nivo);
                    _context.SaveChanges();
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Ovaj nivo ne može biti obrisan jer postoje aktivne lekcije ili pitanja koja su vezana za njega.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Došlo je do neočekivane greške na serveru prilikom brisanja nivoa.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
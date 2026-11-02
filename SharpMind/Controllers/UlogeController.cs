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
    [Authorize(Roles = "Admin")]
    public class UlogeController : Controller
    {
        private readonly SharpMindContext _context;

        public UlogeController(SharpMindContext context)
        {
            _context = context;
        }

        // INDEX METODA (Prikaz i upravljanje listom)

        // GET: /Uloge
        public IActionResult Index(string sortOrder, int page = 1, int pageSize = 25)
        {
            try
            {

                if (pageSize < 1) pageSize = 25;

                ViewBag.IdSortParm = sortOrder == "id_asc" ? "id_desc" : "id_asc";
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentPageSize = pageSize;


                var query = _context.Uloge.AsQueryable();
                switch (sortOrder)
                {
                    case "id_asc":
                        query = query.OrderBy(u => u.UlogaId);
                        break;
                    case "id_desc":
                    default:
                        query = query.OrderByDescending(u => u.UlogaId);
                        break;
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);


                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;


                var uloge = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                return View(uloge);
            }
            catch (Exception)
            {

                ViewBag.Error = "Sistemska greška pri učitavanju liste uloga.";
                return View(new List<Uloga>());
            }
        }


        // CREATE METODE (Kreiranje nove uloge)


        // GET: /Uloge/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Uloge/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("UlogaNaziv")] Uloga uloga)
        {

            ModelState.Remove("UlogaId");

            if (ModelState.IsValid)
            {
                try
                {
                    if (_context.Uloge.Any(u => u.UlogaNaziv == uloga.UlogaNaziv))
                    {
                        ModelState.AddModelError("UlogaNaziv", "Uloga sa ovim nazivom već postoji.");
                        return View(uloga);
                    }

                    _context.Uloge.Add(uloga);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Greška prilikom upisivanja nove uloge u bazu.");
                }
            }
            return View(uloga);
        }

        // EDIT METODE (Izmena postojeće uloge)

        // GET: /Uloge/Edit/5
        public IActionResult Edit(int id)
        {
            try
            {
                var uloga = _context.Uloge.Find(id);
                if (uloga == null) return NotFound();

                return View(uloga);
            }
            catch (Exception)
            {
                TempData["Error"] = "Greška pri otvaranju uloge za izmenu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Uloge/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Uloga uloga)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (_context.Uloge.Any(u => u.UlogaNaziv == uloga.UlogaNaziv && u.UlogaId != uloga.UlogaId))
                    {
                        ModelState.AddModelError("UlogaNaziv", "Druga uloga već koristi ovaj naziv.");
                        return View(uloga);
                    }

                    _context.Uloge.Update(uloga);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Podaci o ovoj ulozi su u međuvremenu izmenjeni na serveru.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Sistemska greška prilikom ažuriranja uloge.");
                }
            }
            return View(uloga);
        }

        // DELETE METODA (Brisanje uloge)

        // POST: /Uloge/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var uloga = _context.Uloge.Find(id);
                if (uloga != null)
                {
                    bool ulogaSeKoristi = _context.Korisnici.Any(k => k.UlogaId == id);
                    if (ulogaSeKoristi)
                    {
                        TempData["Error"] = "Nije moguće obrisati ulogu jer postoje korisnici koji su joj dodeljeni.";
                        return RedirectToAction(nameof(Index));
                    }


                    _context.Uloge.Remove(uloga);
                    _context.SaveChanges();
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Uloga se ne može obrisati zbog restrikcija stranog ključa u bazi.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Sistemska greška prilikom brisanja uloge.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Uloge/Details/5
        public IActionResult Details(int id)
        {
            try
            {
                var uloga = _context.Uloge.Find(id);
                if (uloga == null) return NotFound();

                return View(uloga);
            }
            catch (Exception)
            {
                TempData["Error"] = "Greška pri učitavanju detalja o ulozi.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
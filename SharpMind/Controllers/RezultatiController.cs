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
    [Authorize]
    public class RezultatiController : Controller
    {
        private readonly SharpMindContext _context;

        public RezultatiController(SharpMindContext context)
        {
            _context = context;
        }

        // INDEX METODA (Generisanje i pokretanje rezultata)

        // GET: /Rezultati
        [Authorize(Roles = "Admin,Profesor")]
        public IActionResult Index(string sortOrder, int page = 1, int pageSize = 25)
        {
            try
            {
                if (pageSize < 1) pageSize = 25;

                ViewBag.IdSortParm = sortOrder == "id_asc" ? "id_desc" : "id_asc";
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentPageSize = pageSize;

                var query = _context.Rezultati
                    .Include(r => r.Korisnik)
                    .Include(r => r.Nivo)
                    .AsQueryable();

                switch (sortOrder)
                {
                    case "id_asc":
                        query = query.OrderBy(r => r.RezultatId);
                        break;
                    case "id_desc":
                    default:
                        query = query.OrderByDescending(r => r.RezultatId);
                        break;
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;


                var rezultati = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                return View(rezultati);
            }
            catch (Exception)
            {
                ViewBag.Error = "Sistemska greška pri učitavanju opšte liste rezultata.";
                return View(new List<Rezultat>());
            }
        }

        // CREATE METODE (Kreiranje nove rezultate)

        // GET: /Rezultati/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            try
            {
                PopuniListe();
            }
            catch (Exception)
            {
                ViewBag.Error = "Nije moguće učitati spisak korisnika i nivoa sa servera.";
            }
            return View();
        }

        // POST: /Rezultati/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(Rezultat rezultat)
        {
            ModelState.Remove("Korisnik");
            ModelState.Remove("Nivo");

            if (rezultat.OsvojeniPoeni > rezultat.UkupnoPoena)
            {
                ModelState.AddModelError("OsvojeniPoeni", "Broj osvojenih poena ne može biti veći od ukupnog broja poena.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Rezultati.Add(rezultat);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Greška prilikom upisivanja rezultata u bazu.");
                }
            }

            try { PopuniListe(); } catch { }
            return View(rezultat);
        }

        // EDIT METODE (Izmena postojeće rezultate)

        // GET: /Rezultati/Edit/5
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            try
            {
                var rezultat = _context.Rezultati.Find(id);
                if (rezultat == null) return NotFound();

                PopuniListe();
                return View(rezultat);
            }
            catch (Exception)
            {
                TempData["Error"] = "Greška pri otvaranju rezultata za izmenu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Rezultati/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(Rezultat rezultat)
        {
            ModelState.Remove("Korisnik");
            ModelState.Remove("Nivo");

            if (rezultat.OsvojeniPoeni > rezultat.UkupnoPoena)
            {
                ModelState.AddModelError("OsvojeniPoeni", "Broj osvojenih poena ne može biti veći od ukupnog broja poena.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Rezultati.Update(rezultat);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Podaci o ovom rezultatu su u međuvremenu izmenjeni na serveru.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Sistemska greška prilikom ažuriranja rezultata.");
                }
            }

            try { PopuniListe(); } catch { }
            return View(rezultat);
        }

        // GET: /Rezultati/Details/5
        [Authorize(Roles = "Admin")]
        public IActionResult Details(int id)
        {
            try
            {
                var rezultat = _context.Rezultati
                    .Include(r => r.Korisnik)
                    .Include(r => r.Nivo)
                    .FirstOrDefault(r => r.RezultatId == id);

                if (rezultat == null) return NotFound();

                if (!User.IsInRole("Admin") && !User.IsInRole("Profesor"))
                {
                    if (rezultat.Korisnik?.KorisnikIme != User.Identity.Name)
                    {
                        return Forbid();
                    }
                }

                return View(rezultat);
            }
            catch (Exception)
            {
                TempData["Error"] = "Greška pri učitavanju detalja o rezultatu.";
                return RedirectToAction(string.Format("{0}", (User.IsInRole("Admin") || User.IsInRole("Profesor")) ? "Index" : "MojPregled"));
            }
        }

        // DELETE METODA (Brisanje rezultata)

        // POST: /Rezultati/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            try
            {
                var rezultat = _context.Rezultati.Find(id);
                if (rezultat != null)
                {
                    _context.Rezultati.Remove(rezultat);
                    _context.SaveChanges();
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Rezultat se ne može obrisati jer je povezan sa drugim zapisima u bazi podataka.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Sistemska greška prilikom brisanja rezultata.";
            }
            return RedirectToAction(nameof(Index));
        }

        // OSTALE METODE (Pomoćne i sporedne akcije)

        // GET: /Rezultati/MojPregled?korisnikId=5
        public IActionResult MojPregled(int korisnikId)
        {
            try
            {
                if (!User.IsInRole("Admin") && !User.IsInRole("Profesor"))
                {
                    var trenchesKorisnik = _context.Korisnici.FirstOrDefault(k => k.KorisnikIme == User.Identity.Name);
                    if (trenchesKorisnik == null || trenchesKorisnik.KorisnikId != korisnikId)
                    {
                        return Forbid();
                    }
                }

                var rezultati = _context.Rezultati
                    .Include(r => r.Nivo)
                    .Include(r => r.Korisnik)
                    .Where(r => r.KorisnikId == korisnikId)
                    .OrderByDescending(r => r.DatumPolaganja)
                    .ToList();

                var korisnik = _context.Korisnici.Find(korisnikId);
                ViewBag.KorisnikIme = korisnik?.KorisnikIme ?? "Nepoznat korisnik";
                ViewBag.KorisnikId = korisnikId;

                return View(rezultati);
            }
            catch (Exception)
            {
                ViewBag.Error = "Došlo je do greške pri učitavanju tvojih rezultata.";
                return View(new List<Rezultat>());
            }
        }

        // GET: /Rezultati/StudentskiRezultati
        [Authorize(Roles = "Admin,Profesor")]
        public IActionResult StudentskiRezultati(string searchName, string tipAktivnosti, string sortOrder, int page = 1, int pageSize = 25)
        {
            try
            {
                if (pageSize < 1) pageSize = 25;

                var ulogovaniKorisnik = _context.Korisnici
                    .Include(k => k.Uloga)
                    .FirstOrDefault(k => k.KorisnikIme == User.Identity.Name);

                if (ulogovaniKorisnik == null)
                {
                    return RedirectToAction("Login", "Nalog");
                }

                bool isAdmin = ulogovaniKorisnik.Uloga?.UlogaNaziv == "Admin";

                var query = _context.Rezultati
                    .Include(r => r.Nivo)
                    .Include(r => r.Korisnik)
                        .ThenInclude(k => k.Profesor)
                    .AsQueryable();

                if (!isAdmin)
                {
                    query = query.Where(r => r.Korisnik.ProfesorId == ulogovaniKorisnik.KorisnikId);
                }

                if (!string.IsNullOrEmpty(searchName))
                {
                    query = query.Where(r => r.Korisnik.KorisnikIme.Contains(searchName));
                }

                if (!string.IsNullOrEmpty(tipAktivnosti))
                {
                    query = query.Where(r => r.TipAktivnosti == tipAktivnosti);
                }

                ViewBag.CurrentSort = sortOrder;

                switch (sortOrder)
                {
                    case "procenat_desc":
                        query = query.OrderByDescending(r => (double)r.OsvojeniPoeni / (r.UkupnoPoena > 0 ? r.UkupnoPoena : 1));
                        break;
                    case "procenat_asc":
                        query = query.OrderBy(r => (double)r.OsvojeniPoeni / (r.UkupnoPoena > 0 ? r.UkupnoPoena : 1));
                        break;
                    default:
                        query = query.OrderByDescending(r => r.DatumPolaganja);
                        break;
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;
                ViewBag.CurrentPageSize = pageSize;

                ViewBag.CurrentSearch = searchName;
                ViewBag.CurrentTip = tipAktivnosti;
                ViewBag.IsAdmin = isAdmin;

                var rezultati = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                return View(rezultati);
            }
            catch (Exception)
            {
                ViewBag.Error = "Greška prilikom generisanja pregleda studentskih rezultata.";
                return View(new List<Rezultat>());
            }
        }

        private void PopuniListe()
        {
            ViewBag.Korisnici = _context.Korisnici.OrderBy(k => k.KorisnikIme).ToList();
            ViewBag.Nivoi = _context.Nivoi.OrderBy(n => n.GranicniPoeniNivoa).ToList();
        }
    }
}
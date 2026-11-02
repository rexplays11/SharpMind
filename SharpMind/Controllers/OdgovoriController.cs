using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpMindApi.Controllers
{
    [Authorize(Roles = "Admin, Profesor")] 
    public class OdgovoriController : Controller
    {
        private readonly SharpMindContext _context;

        public OdgovoriController(SharpMindContext context) => _context = context;

        // INDEX METODA (Prikaz i upravljanje listom)

        // GET: /Odgovori
        [AllowAnonymous] 
        public IActionResult Index(string sortOrder, int page = 1, int pageSize = 25)
        {
            try
            {
                if (pageSize < 1) pageSize = 25;

                ViewBag.IdSortParm = sortOrder == "id_asc" ? "id_desc" : "id_asc";
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentPageSize = pageSize;

                var query = _context.Odgovori
                    .Include(o => o.Pitanje)
                    .AsQueryable();

                switch (sortOrder)
                {
                    case "id_asc":
                        query = query.OrderBy(o => o.OdgovorId);
                        break;
                    default:
                        query = query.OrderByDescending(o => o.OdgovorId);
                        break;
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;

                var odgovori = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                return View(odgovori);
            }
            catch (Exception)
            {
                ViewBag.Error = "Sistemska greška pri učitavanju odgovora.";
                return View(new List<Odgovor>());
            }
        }

        // CREATE METODE (Kreiranje novog odgovora)

        // GET: /Odgovori/Create
        public IActionResult Create(int? pitanjeId)
        {
            PopuniListe();

            if (pitanjeId.HasValue)
            {
                var model = new Odgovor { PitanjeId = pitanjeId.Value };
                return View(model);
            }

            return View();
        }

        // POST: /Odgovori/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Odgovor odgovor)
        {
            ModelState.Remove("Pitanje");

            if (odgovor.OdgovorTekst != null)
            {
                odgovor.OdgovorTekst = odgovor.OdgovorTekst.Trim();
            }

            var duplikat = _context.Odgovori.Any(o => o.PitanjeId == odgovor.PitanjeId && o.OdgovorTekst.Trim() == odgovor.OdgovorTekst);
            if (duplikat)
            {
                ModelState.AddModelError("OdgovorTekst", "Ovaj odgovor već postoji za izabrano pitanje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Odgovori.Add(odgovor);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Greška prilikom upisa odgovora u bazu podataka.");
                }
            }

            PopuniListe();
            return View(odgovor);
        }

        // EDIT METODE (Izmena postojećeg odgovora)

        // GET: /Odgovori/Edit/5
        public IActionResult Edit(int id)
        {
            try
            {
                var odgovor = _context.Odgovori.Find(id);
                if (odgovor == null) return NotFound();

                PopuniListe();
                return View(odgovor);
            }
            catch (Exception)
            {
                TempData["Error"] = "Sistem ne može da učita formu za izmenu odgovora.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Odgovori/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Odgovor odgovor)
        {
            ModelState.Remove("Pitanje");

            if (odgovor.OdgovorTekst != null)
            {
                odgovor.OdgovorTekst = odgovor.OdgovorTekst.Trim();
            }

            var duplikat = _context.Odgovori.Any(o => o.PitanjeId == odgovor.PitanjeId && o.OdgovorTekst.Trim() == odgovor.OdgovorTekst && o.OdgovorId != odgovor.OdgovorId);
            if (duplikat)
            {
                ModelState.AddModelError("OdgovorTekst", "Identifikovan je duplikat! Ovaj tekst odgovora već postoji za izabrano pitanje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(odgovor);
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Podaci su u međuvremenu izmenjeni od strane drugog korisnika.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Sistemska greška pri ažuriranju odgovora.");
                }
            }

            PopuniListe();
            return View(odgovor);
        }

        // GET: /Odgovori/Details/5
        [AllowAnonymous] 
        public IActionResult Details(int id)
        {
            try
            {
                var odgovor = _context.Odgovori
                    .Include(o => o.Pitanje)
                    .FirstOrDefault(m => m.OdgovorId == id);

                if (odgovor == null) return NotFound();

                return View(odgovor);
            }
            catch (Exception)
            {
                TempData["Error"] = "Greška pri učitavanju detalja odgovora.";
                return RedirectToAction(nameof(Index));
            }
        }


        // POST: /Odgovori/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var o = _context.Odgovori.Find(id);
                if (o != null)
                {
                    _context.Odgovori.Remove(o);
                    _context.SaveChanges();
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Odgovor ne može biti obrisan jer je povezan sa istorijom polaganja kvizova.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Sistemska greška pri brisanju odgovora.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Odgovori/Izmena/5
        public IActionResult Izmena(int id)
        {
            try
            {
                var odg = _context.Odgovori
                    .Include(o => o.Pitanje)
                    .FirstOrDefault(o => o.OdgovorId == id);

                if (odg == null) return NotFound();

                ViewBag.TekstPitanja = odg.Pitanje?.PitanjeTekst ?? "Nepoznato pitanje (Pitanje je obrisano)";

                PopuniListe();
                return View("IzmenaOdgovora", odg);
            }
            catch (Exception ex)
            {
                TempData["Greska"] = "Sistemska greška pri učitavanju (GET Izmena): " + ex.Message;
                return RedirectToAction("Objedinjeno", "Pitanja");
            }
        }

        // POST: /Odgovori/Izmena/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Izmena(Odgovor odg)
        {
            ModelState.Remove("Pitanje");

            if (odg.OdgovorTekst != null)
            {
                odg.OdgovorTekst = odg.OdgovorTekst.Trim();
            }

            var duplikat = _context.Odgovori.Any(o => o.PitanjeId == odg.PitanjeId && o.OdgovorTekst.Trim() == odg.OdgovorTekst && o.OdgovorId != odg.OdgovorId);
            if (duplikat)
            {
                ModelState.AddModelError("OdgovorTekst", "Ovaj tekst odgovora već postoji za izabrano pitanje.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(odg);
                    _context.SaveChanges();
                    return RedirectToAction("Objedinjeno", "Pitanja");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Greška pri čuvanju u bazu (POST Izmena): " + ex.Message);
                }
            }

            var pitanje = _context.Pitanja.Find(odg.PitanjeId);
            ViewBag.TekstPitanja = pitanje?.PitanjeTekst ?? "Nepoznato pitanje";

            PopuniListe();
            return View("IzmenaOdgovora", odg);
        }

        // GET: /Odgovori/NoviOdgovor
        public IActionResult NoviOdgovor(int? pitanjeId)
        {
            try
            {
                PopuniListe();
                var model = new Odgovor();

                if (pitanjeId.HasValue)
                {
                    model.PitanjeId = pitanjeId.Value;
                    var pitanje = _context.Pitanja.Find(pitanjeId.Value);
                    ViewBag.TekstPitanja = pitanje?.PitanjeTekst ?? "Nepoznato pitanje";
                }
                else
                {
                    ViewBag.TekstPitanja = "Pitanje nije prosleđeno.";
                }

                return View(model);
            }
            catch (Exception)
            {
                TempData["Greska"] = "Nije moguće otvoriti formu za novi odgovor.";
                return RedirectToAction("Objedinjeno", "Pitanja");
            }
        }

        // POST: /Odgovori/NoviOdgovor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NoviOdgovor(Odgovor odgovor)
        {
            ModelState.Remove("Pitanje");

            if (odgovor.OdgovorTekst != null)
            {
                odgovor.OdgovorTekst = odgovor.OdgovorTekst.Trim();
            }

            try
            {
                var brojPostojecih = _context.Odgovori.Count(o => o.PitanjeId == odgovor.PitanjeId);
                if (brojPostojecih >= 4)
                {
                    TempData["Greska"] = "Nije moguće dodati odgovor. Pitanje već ima maksimalno 4 odgovora!";
                    return RedirectToAction("Objedinjeno", "Pitanja");
                }

                var duplikat = _context.Odgovori.Any(o => o.PitanjeId == odgovor.PitanjeId && o.OdgovorTekst.Trim() == odgovor.OdgovorTekst);
                if (duplikat)
                {
                    ModelState.AddModelError("OdgovorTekst", "Ovaj tekst odgovora već postoji unutar ovog pitanja.");
                }

                if (ModelState.IsValid)
                {
                    _context.Odgovori.Add(odgovor);
                    _context.SaveChanges();
                    return RedirectToAction("Objedinjeno", "Pitanja");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Sistemska greška prilikom validacije ili čuvanja novog odgovora.");
            }

            var ptr = _context.Pitanja.Find(odgovor.PitanjeId);
            ViewBag.TekstPitanja = ptr?.PitanjeTekst ?? "Nepoznato pitanje";

            PopuniListe();
            return View(odgovor);
        }

        // GET: /Odgovori/IzmenaOdgovora/5
        public IActionResult IzmenaOdgovora(int id)
        {
            try
            {
                var odgovor = _context.Odgovori.Find(id);
                if (odgovor == null) return NotFound();

                var pitanje = _context.Pitanja.Find(odgovor.PitanjeId);
                ViewBag.TekstPitanja = pitanje?.PitanjeTekst ?? "Nepoznato pitanje";

                return View(odgovor);
            }
            catch (Exception)
            {
                TempData["Greska"] = "Nije moguće otvoriti odgovor za izmenu.";
                return RedirectToAction("Objedinjeno", "Pitanja");
            }
        }

        // POST: /Odgovori/IzmenaOdgovora
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IzmenaOdgovora(Odgovor odgovor)
        {
            ModelState.Remove("Pitanje");

            if (odgovor != null && !string.IsNullOrWhiteSpace(odgovor.OdgovorTekst))
            {
                odgovor.OdgovorTekst = odgovor.OdgovorTekst.Trim();

                bool postoji = _context.Odgovori.Any(o => o.PitanjeId == odgovor.PitanjeId
                                                              && o.OdgovorTekst.ToLower() == odgovor.OdgovorTekst.ToLower()
                                                              && o.OdgovorId != odgovor.OdgovorId);
                if (postoji)
                {
                    ModelState.AddModelError("OdgovorTekst", "Drugi odgovor za ovo pitanje već sadrži ovaj tekst.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Odgovori.Update(odgovor);
                    _context.SaveChanges();
                    return RedirectToAction("Objedinjeno", "Pitanja");
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Neko je već izmenio ovaj odgovor u bazi podataka.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Sistemska greška prilikom izmene odgovora.");
                }
            }

            try
            {
                int stvarniPitanjeId = odgovor.PitanjeId;
                if (stvarniPitanjeId == 0 && odgovor.OdgovorId != 0)
                {
                    var originalniOdgovor = _context.Odgovori.AsNoTracking().FirstOrDefault(o => o.OdgovorId == odgovor.OdgovorId);
                    if (originalniOdgovor != null)
                    {
                        stvarniPitanjeId = originalniOdgovor.PitanjeId;
                        odgovor.PitanjeId = stvarniPitanjeId;
                    }
                }

                var pitanje = _context.Pitanja.Find(stvarniPitanjeId);
                ViewBag.TekstPitanja = pitanje?.PitanjeTekst ?? "Nepoznato pitanje";
            }
            catch { }

            return View(odgovor);
        }

        // POST: /Odgovori/ObrisiOdgovor/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ObrisiOdgovor(int id)
        {
            try
            {
                var o = _context.Odgovori.Find(id);
                if (o != null)
                {
                    _context.Odgovori.Remove(o);
                    _context.SaveChanges();
                }
            }
            catch (DbUpdateException)
            {
                TempData["Greska"] = "Odgovor ne može biti obrisan jer je povezan sa postojećim rezultatima kvizova.";
            }
            catch (Exception)
            {
                TempData["Greska"] = "Neočekivana greška na bazi podataka prilikom brisanja odgovora.";
            }
            return RedirectToAction("Objedinjeno", "Pitanja");
        }

        private void PopuniListe()
        {
            ViewBag.Pitanja = _context.Pitanja.OrderBy(p => p.PitanjeTekst).ToList();
        }
    }
}
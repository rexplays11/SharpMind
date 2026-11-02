using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpMindApi.Controllers
{
    [Authorize]
    public class KorisniciController : Controller
    {
        private readonly SharpMindContext _context;

        public KorisniciController(SharpMindContext context)
        {
            _context = context;
        }


        // INDEX: Prikaz i upravljanje listom korisnika 


        [Authorize(Roles = "Admin, Profesor")]
        public async Task<IActionResult> Index(string sortOrder, int page = 1, int pageSize = 25)
        {
            try
            {
                var ulogovaniKorisnik = await _context.Korisnici
                    .Include(k => k.Uloga)
                    .FirstOrDefaultAsync(k => k.KorisnikIme == User.Identity.Name);

                if (ulogovaniKorisnik == null) return RedirectToAction("Login", "Account");

                ViewBag.IdSortParm = sortOrder == "id_asc" ? "id_desc" : "id_asc";
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentPageSize = pageSize;

               
                var query = _context.Korisnici
                    .Include(k => k.Uloga)
                    .Include(k => k.Nivo)
                    .Include(k => k.Profesor)
                    .AsQueryable();

                query = sortOrder == "id_asc"
                    ? query.OrderBy(k => k.KorisnikId)
                    : query.OrderByDescending(k => k.KorisnikId);

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;

                var korisnici = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return View(korisnici);
            }
            catch (Exception)
            {
                ViewBag.Error = "Došlo je do greške prilikom učitavanja liste korisnika.";
                return View(new List<Korisnik>());
            }
        }




        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            try
            {
                await PopuniListeAsync();
            }
            catch (Exception)
            {
                ViewBag.Error = "Sistem ne može da učita prateće liste iz baze.";
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Korisnik korisnik)
        {
            UkloniNavigacionePropertijeIzModela();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Korisnici.Add(korisnik);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Greška u bazi podataka. Moguće je da korisničko ime već postoji.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Neočekivana greška na serveru. Korisnik nije sačuvan.");
                }
            }

            await OcistiIPopuniListeBezbednoAsync();
            return View(korisnik);
        }


        // EDIT: Izmena korisnika (Admin ili Profesor za sebe/svoje studente)


        [Authorize(Roles = "Admin, Profesor")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var ulogovaniKorisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.KorisnikIme == User.Identity.Name);
                if (ulogovaniKorisnik == null) return Forbid();

                var korisnik = await _context.Korisnici.FindAsync(id);
                if (korisnik == null) return NotFound();


                bool isProfesorOnly = User.IsInRole("Profesor") && !User.IsInRole("Admin");
                if (isProfesorOnly && korisnik.KorisnikId != ulogovaniKorisnik.KorisnikId && korisnik.ProfesorId != ulogovaniKorisnik.KorisnikId)
                {
                    return Forbid();
                }

                ViewBag.IsProfesorOnly = isProfesorOnly;
                await PopuniListeAsync();
                return View(korisnik);
            }
            catch (Exception)
            {
                ViewBag.Error = "Greška pri pronalaženju korisnika.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Profesor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Korisnik model)
        {
            if (id != model.KorisnikId) return BadRequest();

            var ulogovaniKorisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.KorisnikIme == User.Identity.Name);
            if (ulogovaniKorisnik == null) return Forbid();

            var korisnikIzBaze = await _context.Korisnici.FindAsync(id);
            if (korisnikIzBaze == null) return NotFound();

            bool isProfesorOnly = User.IsInRole("Profesor") && !User.IsInRole("Admin");

            if (isProfesorOnly && korisnikIzBaze.KorisnikId != ulogovaniKorisnik.KorisnikId && korisnikIzBaze.ProfesorId != ulogovaniKorisnik.KorisnikId)
            {
                return Forbid();
            }

            UkloniNavigacionePropertijeIzModela();

            if (isProfesorOnly)
            {

                ModelState.Remove(nameof(Korisnik.KorisnikDatumRegistracije));
                ModelState.Remove(nameof(Korisnik.UlogaId));
                ModelState.Remove(nameof(Korisnik.ProfesorId));
                ModelState.Remove(nameof(Korisnik.NivoId));
                ModelState.Remove(nameof(Korisnik.UkupnoPoena));

                korisnikIzBaze.KorisnikIme = model.KorisnikIme;
                if (!string.IsNullOrWhiteSpace(model.KorisnikLozinka))
                {
                    korisnikIzBaze.KorisnikLozinka = model.KorisnikLozinka;
                }

            }
            else
            {

                korisnikIzBaze.KorisnikIme = model.KorisnikIme;
                if (!string.IsNullOrWhiteSpace(model.KorisnikLozinka))
                {
                    korisnikIzBaze.KorisnikLozinka = model.KorisnikLozinka;
                }
                korisnikIzBaze.UlogaId = model.UlogaId;
                korisnikIzBaze.ProfesorId = model.ProfesorId;
                korisnikIzBaze.NivoId = model.NivoId;
                korisnikIzBaze.UkupnoPoena = model.UkupnoPoena;
                korisnikIzBaze.KorisnikDatumRegistracije = model.KorisnikDatumRegistracije;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(korisnikIzBaze);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await KorisnikPostojiAsync(korisnikIzBaze.KorisnikId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Korisnik je u međuvremenu izmenjen od strane drugog procesa. Osvežite stranicu.");
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Greška pri čuvanju izmena u bazi podataka.");
                }
            }

            ViewBag.IsProfesorOnly = isProfesorOnly;
            await OcistiIPopuniListeBezbednoAsync();
            return View(korisnikIzBaze);
        }


        // DETAILS: Detaljan pregled korisnika (Samo Admin)


         [Authorize(Roles = "Admin, Profesor")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var korisnik = await _context.Korisnici
                    .Include(k => k.Uloga)
                    .Include(k => k.Nivo)
                    .Include(k => k.Profesor)
                    .FirstOrDefaultAsync(k => k.KorisnikId == id);

                if (korisnik == null) return NotFound();

                return View(korisnik);
            }
            catch (Exception)
            {
                ViewBag.Error = "Došlo je do greške pri dobavljanju detalja o korisniku.";
                return RedirectToAction(nameof(Index));
            }
        }


        // DELETE: Brisanje korisnika (Samo Admin)


        [HttpPost]
        [Authorize(Roles = "Admin, Profesor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var korisnik = await _context.Korisnici.FindAsync(id);
                if (korisnik == null) return NotFound();

                var korisnikoviKvizovi = await _context.Kvizovi
                    .Where(k => k.KorisnikId == id)
                    .ToListAsync();

                foreach (var kviz in korisnikoviKvizovi)
                {
                    kviz.KorisnikId = null;
                }

                _context.Korisnici.Remove(korisnik);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Korisnik ne može biti obrisan zbog preostalih povezanih podataka u bazi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["Error"] = "Sistemska greška prilikom brisanja korisnika.";
                return RedirectToAction(nameof(Index));
            }
        }


        // PROFIL: Prikaz sopstvenog profila

        public async Task<IActionResult> Profil()
        {
            try
            {
                var korisnikIme = User.Identity?.Name;
                var korisnik = await _context.Korisnici
                    .Include(k => k.Nivo)
                    .Include(k => k.Uloga)
                    .FirstOrDefaultAsync(k => k.KorisnikIme == korisnikIme);

                if (korisnik == null) return NotFound();

                int donjaGranica = korisnik.Nivo?.GranicniPoeniNivoa ?? 0;

                var sledeciNivo = await _context.Nivoi
                    .Where(n => n.GranicniPoeniNivoa > korisnik.UkupnoPoena)
                    .OrderBy(n => n.GranicniPoeniNivoa)
                    .FirstOrDefaultAsync();

                double procenat = 0;
                int gornjaGranica = donjaGranica;
                string sledeciNivoNaziv = "Maksimalan Nivo";
                bool jeMaksimalanNivo = false;

                if (sledeciNivo != null)
                {
                    gornjaGranica = sledeciNivo.GranicniPoeniNivoa;
                    sledeciNivoNaziv = sledeciNivo.NazivNivoa;

                    double ukupnoPotrebnoURangu = gornjaGranica - donjaGranica;
                    double ostvarenoURangu = korisnik.UkupnoPoena - donjaGranica;

                    if (ukupnoPotrebnoURangu > 0)
                    {
                        if (ostvarenoURangu < 0) ostvarenoURangu = 0;
                        procenat = (ostvarenoURangu / ukupnoPotrebnoURangu) * 100;
                    }
                    else
                    {
                        procenat = ((double)korisnik.UkupnoPoena / gornjaGranica) * 100;
                    }
                }
                else
                {
                    jeMaksimalanNivo = true;
                    procenat = 100;
                }

                ViewBag.SledeciNivoNaziv = sledeciNivoNaziv;
                ViewBag.MaxPoenaZaSledeci = gornjaGranica;
                ViewBag.ProcenatProgresa = Math.Clamp(procenat, 0, 100);
                ViewBag.JeMaksimalanNivo = jeMaksimalanNivo;

                ViewBag.Uloge = await _context.Uloge.ToListAsync();
                ViewBag.Nivoi = await _context.Nivoi.ToListAsync();
                ViewBag.SviKorisnici = await _context.Korisnici.ToListAsync();

                return View(korisnik);
            }
            catch (Exception)
            {
                ViewBag.Error = "Došlo je do greške prilikom generisanja profila.";
                return View(new Korisnik());
            }
        }


        // REGISTRACIJA: Dodavanje studenata (Profesor i Admin)


        [Authorize(Roles = "Admin, Profesor")]
        public async Task<IActionResult> Registracija()
        {
            try
            {
                var ulogovaniKorisnik = await _context.Korisnici
                    .Include(k => k.Uloga)
                    .FirstOrDefaultAsync(k => k.KorisnikIme == User.Identity.Name);

                if (ulogovaniKorisnik == null) return RedirectToAction("Login", "Nalog");

                bool isAdmin = ulogovaniKorisnik.Uloga?.UlogaNaziv == "Admin";
                ViewBag.IsAdmin = isAdmin;
                ViewBag.TrenutniProfesorId = ulogovaniKorisnik.KorisnikId;

                ViewBag.Uloge = isAdmin
                    ? await _context.Uloge.ToListAsync()
                    : await _context.Uloge.Where(u => u.UlogaNaziv == "Student").ToListAsync();

                ViewBag.Nivoi = await _context.Nivoi.ToListAsync();
                ViewBag.Profesori = await _context.Korisnici
                    .Include(k => k.Uloga)
                    .Where(k => k.Uloga.UlogaNaziv == "Profesor")
                    .ToListAsync();

                return View();
            }
            catch (Exception)
            {
                ViewBag.Error = "Došlo je do sistemske greške pri učitavanju forme za registraciju.";
                return View();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Profesor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registracija(Korisnik korisnik)
        {
            var ulogovaniKorisnik = await _context.Korisnici
                .Include(k => k.Uloga)
                .FirstOrDefaultAsync(k => k.KorisnikIme == User.Identity.Name);

            if (ulogovaniKorisnik == null) return RedirectToAction("Login", "Nalog");

            bool isAdmin = ulogovaniKorisnik.Uloga?.UlogaNaziv == "Admin";

            try
            {
                if (!isAdmin)
                {
                    var studentUloga = await _context.Uloge.FirstOrDefaultAsync(u => u.UlogaNaziv == "Student");
                    if (studentUloga != null) korisnik.UlogaId = studentUloga.UlogaId;

                    korisnik.KorisnikDatumRegistracije = DateTime.Now;
                    korisnik.ProfesorId = ulogovaniKorisnik.KorisnikId;
                    ModelState.Remove(nameof(korisnik.KorisnikDatumRegistracije));
                }

                UkloniNavigacionePropertijeIzModela();

                if (await _context.Korisnici.AnyAsync(k => k.KorisnikIme == korisnik.KorisnikIme))
                {
                    ModelState.AddModelError("KorisnikIme", "Ovo korisničko ime je već zauzeto. Izaberite drugo.");
                }

                var sviNivoiPoredjani = await _context.Nivoi.OrderBy(n => n.GranicniPoeniNivoa).ToListAsync();
                var nivoIzBaze = sviNivoiPoredjani.FirstOrDefault(n => n.NivoId == korisnik.NivoId);

                if (nivoIzBaze != null)
                {
                    if (korisnik.UkupnoPoena > nivoIzBaze.GranicniPoeniNivoa)
                    {
                        ModelState.AddModelError("UkupnoPoena", $"Maksimalno dozvoljeno za nivo '{nivoIzBaze.NazivNivoa}' je {nivoIzBaze.GranicniPoeniNivoa} poena.");
                    }

                    int trenutniIndeks = sviNivoiPoredjani.IndexOf(nivoIzBaze);
                    if (trenutniIndeks > 0)
                    {
                        var prethodniNivo = sviNivoiPoredjani[trenutniIndeks - 1];
                        int minimalnoPotrebno = prethodniNivo.GranicniPoeniNivoa + 1;

                        if (korisnik.UkupnoPoena < minimalnoPotrebno)
                        {
                            ModelState.AddModelError("UkupnoPoena", $"Za nivo '{nivoIzBaze.NazivNivoa}' potrebno je najmanje {minimalnoPotrebno} poena.");
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    if (korisnik.KorisnikDatumRegistracije == default)
                    {
                        korisnik.KorisnikDatumRegistracije = DateTime.Now;
                    }

                    _context.Korisnici.Add(korisnik);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Sistemska greška prilikom registracije.");
            }

            ViewBag.IsAdmin = isAdmin;
            ViewBag.TrenutniProfesorId = ulogovaniKorisnik.KorisnikId;

            try
            {
                ViewBag.Uloge = isAdmin
                    ? await _context.Uloge.ToListAsync()
                    : await _context.Uloge.Where(u => u.UlogaNaziv == "Student").ToListAsync();
                ViewBag.Nivoi = await _context.Nivoi.ToListAsync();
                ViewBag.Profesori = await _context.Korisnici.Include(k => k.Uloga).Where(k => k.Uloga.UlogaNaziv == "Profesor").ToListAsync();
            }
            catch { }

            return View(korisnik);
        }

        [Authorize(Roles = "Admin, Profesor")]
        public async Task<IActionResult> ListaStudenata(string sortOrder, int page = 1, int pageSize = 25)
        {
            try
            {
                var ulogovaniKorisnik = await _context.Korisnici
                    .Include(k => k.Uloga)
                    .FirstOrDefaultAsync(k => k.KorisnikIme == User.Identity.Name);

                if (ulogovaniKorisnik == null) return RedirectToAction("Login", "Account");

                ViewBag.IdSortParm = sortOrder == "id_asc" ? "id_desc" : "id_asc";
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentPageSize = pageSize;

                var query = _context.Korisnici
                    .Include(k => k.Uloga)
                    .Include(k => k.Nivo)
                    .Include(k => k.Profesor)
                    .Where(k => k.Uloga.UlogaNaziv == "Student") 
                    .AsQueryable();

               
                query = query.Where(k => k.ProfesorId == ulogovaniKorisnik.KorisnikId);

                query = sortOrder == "id_asc"
                    ? query.OrderBy(k => k.KorisnikId)
                    : query.OrderByDescending(k => k.KorisnikId);

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;

                var studenti = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return View(studenti);
            }
            catch (Exception)
            {
                ViewBag.Error = "Došlo je do greške prilikom učitavanja liste studenata.";
                return View(new List<Korisnik>());
            }
        }


        // GET: Korisnici/Azuriranje/5
        [HttpGet]
        public async Task<IActionResult> Azuriranje(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var korisnik = await _context.Korisnici.FindAsync(id);
            if (korisnik == null)
            {
                return NotFound();
            }

            ViewBag.Uloge = await _context.Uloge.ToListAsync();
            ViewBag.Nivoi = await _context.Nivoi.ToListAsync();
            ViewBag.Profesori = await _context.Korisnici
                .Include(k => k.Uloga)
                .Where(k => k.Uloga.UlogaNaziv == "Profesor" && k.KorisnikId != id)
                .ToListAsync();

            return View(korisnik);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Azuriranje(Korisnik model)
        {
            var korisnikIzBaze = await _context.Korisnici.FindAsync(model.KorisnikId);
            if (korisnikIzBaze == null)
            {
                return NotFound();
            }

            korisnikIzBaze.KorisnikIme = model.KorisnikIme;
            if (!string.IsNullOrWhiteSpace(model.KorisnikLozinka))
            {
                korisnikIzBaze.KorisnikLozinka = model.KorisnikLozinka;
            }

            if (User.IsInRole("Profesor") || User.IsInRole("Admin"))
            {
                korisnikIzBaze.NivoId = model.NivoId;
                korisnikIzBaze.UkupnoPoena = model.UkupnoPoena;
            }
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                korisnikIzBaze.UlogaId = model.UlogaId;
                korisnikIzBaze.ProfesorId = model.ProfesorId;
            }

            try
            {
                _context.Update(korisnikIzBaze);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {

                ViewBag.Uloge = await _context.Uloge.ToListAsync();
                ViewBag.Nivoi = await _context.Nivoi.ToListAsync();
                ViewBag.Profesori = await _context.Korisnici
                    .Include(k => k.Uloga)
                    .Where(k => k.Uloga.UlogaNaziv == "Profesor" && k.KorisnikId != model.KorisnikId)
                    .ToListAsync();

                return View(model);
            }
        }


        [Authorize(Roles = "Admin, Profesor")]
        [HttpGet]
        public async Task<IActionResult> StudentEdit(int? id)
        {
            if (id == null) return NotFound();

            var korisnik = await _context.Korisnici
                .Include(k => k.Uloga)
                .Include(k => k.Nivo)
                .Include(k => k.Profesor)
                .FirstOrDefaultAsync(k => k.KorisnikId == id);

            if (korisnik == null || korisnik.Uloga?.UlogaNaziv != "Student")
            {
                return NotFound();
            }

            var ulogovaniKorisnik = await _context.Korisnici
                .Include(k => k.Uloga)
                .FirstOrDefaultAsync(k => k.KorisnikIme == User.Identity.Name);

            if (ulogovaniKorisnik == null) return RedirectToAction("Login", "Nalog");

            bool isAdmin = ulogovaniKorisnik.Uloga?.UlogaNaziv == "Admin";

            if (!isAdmin && korisnik.ProfesorId != ulogovaniKorisnik.KorisnikId)
            {
                return Forbid();
            }

            ViewBag.Uloge = await _context.Uloge.ToListAsync();
            ViewBag.Nivoi = await _context.Nivoi.ToListAsync();
            ViewBag.Profesori = await _context.Korisnici
                .Include(k => k.Uloga)
                .Where(k => k.Uloga.UlogaNaziv == "Profesor")
                .ToListAsync();

            return View(korisnik);
        }
        [HttpPost]
        [Authorize(Roles = "Admin, Profesor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StudentEdit(int id, Korisnik model)
        {
            if (id != model.KorisnikId) return BadRequest();

            var korisnikIzBaze = await _context.Korisnici
                .Include(k => k.Uloga)
                .Include(k => k.Nivo)
                .FirstOrDefaultAsync(k => k.KorisnikId == id);

            if (korisnikIzBaze == null || korisnikIzBaze.Uloga?.UlogaNaziv != "Student")
            {
                return NotFound();
            }

            var ulogovaniKorisnik = await _context.Korisnici
                .Include(k => k.Uloga)
                .FirstOrDefaultAsync(k => k.KorisnikIme == User.Identity.Name);

            if (ulogovaniKorisnik == null) return RedirectToAction("Login", "Nalog");

            bool isAdmin = ulogovaniKorisnik.Uloga?.UlogaNaziv == "Admin";

            if (!isAdmin && korisnikIzBaze.ProfesorId != ulogovaniKorisnik.KorisnikId)
            {
                return Forbid();
            }

            UkloniNavigacionePropertijeIzModela();

            korisnikIzBaze.KorisnikIme = model.KorisnikIme;
            if (!string.IsNullOrWhiteSpace(model.KorisnikLozinka))
            {
                korisnikIzBaze.KorisnikLozinka = model.KorisnikLozinka;
            }

            korisnikIzBaze.NivoId = model.NivoId;
            korisnikIzBaze.UkupnoPoena = model.UkupnoPoena;

            if (isAdmin)
            {
                korisnikIzBaze.UlogaId = model.UlogaId;
                korisnikIzBaze.ProfesorId = model.ProfesorId;
            }

            var izabraniNivo = await _context.Nivoi.FindAsync(model.NivoId);
            if (izabraniNivo != null)
            {
                if (model.UkupnoPoena > izabraniNivo.GranicniPoeniNivoa)
                {
                    ModelState.AddModelError("UkupnoPoena", $"Maksimalno dozvoljeno poena za nivo '{izabraniNivo.NazivNivoa}' je {izabraniNivo.GranicniPoeniNivoa}.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(korisnikIzBaze);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ListaStudenata));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await KorisnikPostojiAsync(korisnikIzBaze.KorisnikId))
                    {
                        return NotFound();
                    }
                    ModelState.AddModelError("", "Došlo je do konflikta pri izmeni. Osvežite stranicu.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Greška pri čuvanju izmena u bazi podataka.");
                }
            }


            ViewBag.Uloge = await _context.Uloge.ToListAsync();
            ViewBag.Nivoi = await _context.Nivoi.ToListAsync();
            ViewBag.Profesori = await _context.Korisnici
                .Include(k => k.Uloga)
                .Where(k => k.Uloga.UlogaNaziv == "Profesor")
                .ToListAsync();

            return View(model);
        }


        [Authorize(Roles = "Admin, Profesor")]
        [HttpGet]
        public async Task<IActionResult> StudentDetails(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var student = await _context.Korisnici
                    .Include(k => k.Uloga)
                    .Include(k => k.Nivo)
                    .Include(k => k.Profesor)
                    .FirstOrDefaultAsync(k => k.KorisnikId == id && k.Uloga.UlogaNaziv == "Student");

                if (student == null) return NotFound();

                var ulogovaniKorisnik = await _context.Korisnici
                    .Include(k => k.Uloga)
                    .FirstOrDefaultAsync(k => k.KorisnikIme == User.Identity.Name);

                if (ulogovaniKorisnik != null && ulogovaniKorisnik.Uloga?.UlogaNaziv == "Profesor")
                {
                    if (student.ProfesorId != ulogovaniKorisnik.KorisnikId)
                    {
                        return Forbid();
                    }
                }

                return View(student);
            }
            catch (Exception)
            {
                TempData["Error"] = "Došlo je do greške pri dobavljanju detalja o studentu.";
                return RedirectToAction(nameof(ListaStudenata));
            }
        }


        [HttpPost]
        [Authorize(Roles = "Admin, Profesor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StudentDelete(int id)
        {
            try
            {
                var student = await _context.Korisnici
                    .Include(k => k.Uloga)
                    .FirstOrDefaultAsync(k => k.KorisnikId == id);

                if (student == null || student.Uloga?.UlogaNaziv == null || student.Uloga.UlogaNaziv != "Student")
                {
                    return NotFound();
                }

                var ulogovaniKorisnik = await _context.Korisnici
                    .Include(k => k.Uloga)
                    .FirstOrDefaultAsync(k => k.KorisnikIme == User.Identity.Name);

                if (ulogovaniKorisnik != null && ulogovaniKorisnik.Uloga?.UlogaNaziv == "Profesor")
                {
                    if (student.ProfesorId != ulogovaniKorisnik.KorisnikId)
                    {
                        return Forbid();
                    }
                }


                var korisnikoviKvizovi = await _context.Kvizovi
                    .Where(k => k.KorisnikId == id)
                    .ToListAsync();

                foreach (var kviz in korisnikoviKvizovi)
                {
                    kviz.KorisnikId = null;
                }

                _context.Korisnici.Remove(student);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ListaStudenata));
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Student ne može biti obrisan zbog preostalih povezanih podataka u bazi.";
                return RedirectToAction(nameof(ListaStudenata));
            }
            catch (Exception)
            {
                TempData["Error"] = "Sistemska greška prilikom brisanja studenta.";
                return RedirectToAction(nameof(ListaStudenata));
            }
        }


        // POMOĆNE METODE

        private async Task PopuniListeAsync()
        {
            ViewBag.Uloge = await _context.Uloge.ToListAsync();
            ViewBag.Nivoi = await _context.Nivoi.ToListAsync();
            ViewBag.Profesori = await _context.Korisnici.ToListAsync();
        }

        private async Task OcistiIPopuniListeBezbednoAsync()
        {
            try { await PopuniListeAsync(); } catch { }
        }

        private void UkloniNavigacionePropertijeIzModela()
        {
            ModelState.Remove(nameof(Korisnik.Profesor));
            ModelState.Remove(nameof(Korisnik.Nivo));
            ModelState.Remove(nameof(Korisnik.Uloga));
        }

        private async Task<bool> KorisnikPostojiAsync(int id)
        {
            return await _context.Korisnici.AnyAsync(e => e.KorisnikId == id);
        }
    }
}
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SharpMind.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SharpMindContext _context;

        public AccountController(SharpMindContext context)
        {
            _context = context;
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {
                var user = _context.Korisnici
                    .Include(u => u.Uloga)
                    .FirstOrDefault(u => u.KorisnikIme == username && u.KorisnikLozinka == password);

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.KorisnikIme),
                        new Claim(ClaimTypes.NameIdentifier, user.KorisnikId.ToString()),
                        new Claim(ClaimTypes.Role, user.Uloga?.UlogaNaziv ?? "Student")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "Neispravno ime ili lozinka.";
            }
            catch (Exception ex)
            {

                ViewBag.Error = "Došlo je do greške na serveru prilikom prijave. Molimo pokušajte ponovo.";
            }

            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string KorisnikIme, string KorisnikLozinka)
        {
            if (string.IsNullOrEmpty(KorisnikIme) || string.IsNullOrEmpty(KorisnikLozinka))
            {
                ViewBag.Error = "Sva polja su obavezna.";
                return View();
            }

            try
            {
                if (_context.Korisnici.Any(u => u.KorisnikIme == KorisnikIme))
                {
                    ViewBag.Error = "Korisničko ime je zauzeto.";
                    return View();
                }

                var noviKorisnik = new Korisnik
                {
                    KorisnikIme = KorisnikIme,
                    KorisnikLozinka = KorisnikLozinka,
                    KorisnikDatumRegistracije = DateTime.Now,
                    UlogaId = 2,     
                    NivoId = 1,       
                    UkupnoPoena = 0,  
                    ProfesorId = null 
                };

                _context.Korisnici.Add(noviKorisnik);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }
            catch (DbUpdateException ex)
            {
                ViewBag.Error = "Sistemska greška pri čuvanju podataka. Proverite da li su unosi ispravni.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Došlo je do greške na serveru. Registracija trenutno nije moguća.";
            }

            return View();
        }


        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception)
            {
            }

            return RedirectToAction("Login");
        }
    }
}
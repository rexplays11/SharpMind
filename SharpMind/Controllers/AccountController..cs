using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
                var user = await _context.Korisnici
                    .Include(u => u.Uloga)
                    .FirstOrDefaultAsync(u => u.KorisnikIme == username);

                if (user != null && !string.IsNullOrEmpty(password))
                {
                    var passwordHasher = new PasswordHasher<Korisnik>();
                    bool lozinkaIspravna = false;

                    try
                    {
                        var result = passwordHasher.VerifyHashedPassword(user, user.KorisnikLozinka, password);
                        if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
                        {
                            lozinkaIspravna = true;
                        }
                    }
                    catch (FormatException)
                    {

                        if (user.KorisnikLozinka == password)
                        {
                            lozinkaIspravna = true;
                            user.KorisnikLozinka = passwordHasher.HashPassword(user, password);
                            _context.Update(user);
                            await _context.SaveChangesAsync();
                        }
                    }

                    if (lozinkaIspravna)
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
                }

                ViewBag.Error = "Neispravno ime ili lozinka.";
            }
            catch (Exception)
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
        public async Task<IActionResult> Register(string KorisnikIme, string KorisnikLozinka)
        {
            if (string.IsNullOrWhiteSpace(KorisnikIme) || string.IsNullOrWhiteSpace(KorisnikLozinka))
            {
                ViewBag.Error = "Sva polja su obavezna.";
                return View();
            }

            try
            {
                if (await _context.Korisnici.AnyAsync(u => u.KorisnikIme == KorisnikIme))
                {
                    ViewBag.Error = "Korisničko ime je zauzeto.";
                    return View();
                }

                var noviKorisnik = new Korisnik
                {
                    KorisnikIme = KorisnikIme.Trim(),
                    KorisnikDatumRegistracije = DateTime.Now,
                    UlogaId = 3,
                    NivoId = 1,
                    UkupnoPoena = 0,
                    ProfesorId = null
                };

                var passwordHasher = new PasswordHasher<Korisnik>();
                noviKorisnik.KorisnikLozinka = passwordHasher.HashPassword(noviKorisnik, KorisnikLozinka);

                _context.Korisnici.Add(noviKorisnik);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }
            catch (DbUpdateException)
            {
                ViewBag.Error = "Sistemska greška pri čuvanju podataka. Proverite da li su unosi ispravni.";
            }
            catch (Exception)
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

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            string refererUrl = Request.Headers["Referer"].ToString();
            TempData["Error"] = "Nemate prava pristupa toj stranici.";

            if (!string.IsNullOrEmpty(refererUrl))
            {
                return Redirect(refererUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
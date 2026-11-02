using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using SharpMind.Controllers;
using SharpMind.Data;
using SharpMind.Models;

namespace SharpMind.Tests
{
    public class RezultatiControllerTests : IDisposable
    {
        private readonly SharpMindContext _context;
        private readonly RezultatiController _controller;

        public RezultatiControllerTests()
        {
            var options = new DbContextOptionsBuilder<SharpMindContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SharpMindContext(options);
            SeedDatabase();

            _controller = new RezultatiController(_context);
            SetUser("AdminUser", "Admin");
        }

        private void SetUser(string username, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        private void SeedDatabase()
        {
            var ulogaAdmin = new Uloga { UlogaId = 1, UlogaNaziv = "Admin" };
            var ulogaProfesor = new Uloga { UlogaId = 2, UlogaNaziv = "Profesor" };
            var ulogaStudent = new Uloga { UlogaId = 3, UlogaNaziv = "Student" };
            _context.Uloge.AddRange(ulogaAdmin, ulogaProfesor, ulogaStudent);

            var adminKorisnik = new Korisnik { KorisnikId = 1, KorisnikIme = "AdminUser", UlogaId = 1 };
            var studentKorisnik = new Korisnik { KorisnikId = 2, KorisnikIme = "StudentUser", UlogaId = 3 };
            _context.Korisnici.AddRange(adminKorisnik, studentKorisnik);

            var nivo = new Nivo { NivoId = 1, NazivNivoa = "Početni", GranicniPoeniNivoa = 0 };
            _context.Nivoi.Add(nivo);

            var rezultat = new Rezultat
            {
                RezultatId = 1,
                KorisnikId = 2,
                NivoId = 1,
                OsvojeniPoeni = 8,
                UkupnoPoena = 10,
                TipAktivnosti = "TEST",
                DatumPolaganja = DateTime.Now
            };
            _context.Rezultati.Add(rezultat);

            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void Index_AdminPristup_VracaViewResultSaRezultatima()
        {
            var result = _controller.Index(null, 1, 25);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Rezultat>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public void Create_Get_VracaViewResult()
        {
            var result = _controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Post_ValidanRezultat_UpisujeUBazuI_VracaRedirect()
        {
            var noviRezultat = new Rezultat
            {
                KorisnikId = 2,
                NivoId = 1,
                OsvojeniPoeni = 5,
                UkupnoPoena = 10,
                TipAktivnosti = "KVIZ"
            };

            var result = _controller.Create(noviRezultat);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(2, _context.Rezultati.Count());
        }

        [Fact]
        public void Create_Post_NevalidniPoeni_DodajeModelError_I_VracaView()
        {
            var nevalidanRezultat = new Rezultat
            {
                KorisnikId = 2,
                NivoId = 1,
                OsvojeniPoeni = 15, 
                UkupnoPoena = 10,
                TipAktivnosti = "KVIZ"
            };

            var result = _controller.Create(nevalidanRezultat);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public void Edit_Get_PostojeciId_VracaViewResult()
        {
            var result = _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Rezultat>(viewResult.Model);
            Assert.Equal(1, model.RezultatId);
        }

        [Fact]
        public void Edit_Get_NepostojeciId_VracaNotFound()
        {
            var result = _controller.Edit(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Details_PostojeciId_AdminPristup_VracaViewResult()
        {
            var result = _controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Rezultat>(viewResult.Model);
            Assert.Equal(1, model.RezultatId);
        }

        [Fact]
        public void Delete_PostojeciId_BriseRezultatI_VracaRedirect()
        {
            var result = _controller.Delete(1);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Empty(_context.Rezultati);
        }

        [Fact]
        public void MojPregled_VlasnikPristup_VracaViewResult()
        {
            SetUser("StudentUser", "Student");

            var result = _controller.MojPregled(2);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Rezultat>>(viewResult.Model);
            Assert.Single(model);
        }
    }
}
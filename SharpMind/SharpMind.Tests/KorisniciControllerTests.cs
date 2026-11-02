using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SharpMindApi.Controllers; 
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SharpMind.Tests
{
    public class KorisniciControllerTests : IDisposable
    {
        private readonly SharpMindContext _context;
        private readonly KorisniciController _controller;
        private readonly Mock<HttpContext> _httpContextMock;

        public KorisniciControllerTests()
        {
            var options = new DbContextOptionsBuilder<SharpMindContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SharpMindContext(options);
            _context.Database.EnsureCreated();

            _controller = new KorisniciController(_context);

            _httpContextMock = new Mock<HttpContext>();

            PostaviUlogovanogKorisnika("test_profesor");

            var tempDataMock = new Mock<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionary>();
            _controller.TempData = tempDataMock.Object;
        }

        private void PostaviUlogovanogKorisnika(string username)
        {
            var identity = new GenericIdentity(username);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _httpContextMock.Setup(h => h.User).Returns(claimsPrincipal);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContextMock.Object
            };
        }

        #region INDEX TESTOVI

        [Fact]
        public void Index_VracaViewResultSaListomKorisnika()
        {
            _context.Korisnici.Add(new Korisnik { KorisnikId = 1, KorisnikIme = "user1", KorisnikLozinka = "123" });
            _context.Korisnici.Add(new Korisnik { KorisnikId = 2, KorisnikIme = "user2", KorisnikLozinka = "123" });
            _context.SaveChanges();

            var result = _controller.Index("id_asc", page: 1, pageSize: 25);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Korisnik>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        #endregion

        #region CREATE TESTOVI

        [Fact]
        public void Create_Post_UspjesnoKreiranje_PreusmjeravaNaIndex()
        {
            var noviKorisnik = new Korisnik { KorisnikId = 10, KorisnikIme = "novi", KorisnikLozinka = "sifra" };

            var result = _controller.Create(noviKorisnik);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);

            var korisnikUBazi = _context.Korisnici.Find(10);
            Assert.NotNull(korisnikUBazi);
            Assert.Equal("novi", korisnikUBazi.KorisnikIme);
        }

        #endregion

        #region EDIT TESTOVI

        [Fact]
        public void Edit_Get_KorisnikNePostoji_VracaNotFound()
        {
            var result = _controller.Edit(999); 

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Post_RazlicitIdUParamentruIModelu_VracaBadRequest()
        {
            var korisnik = new Korisnik { KorisnikId = 5, KorisnikIme = "izmjena" };

            var result = _controller.Edit(1, korisnik); 
            Assert.IsType<BadRequestResult>(result);
        }

        #endregion

        #region DELETE TESTOVI (Kaskadna logika)

        [Fact]
        public async Task Delete_UspjesnoBriseKorisnika_I_SkidaVlasnistvoSaKvizova()
        {
            var korisnik = new Korisnik { KorisnikId = 12, KorisnikIme = "brisi_me", KorisnikLozinka = "123" };
            var kviz1 = new Kviz { KvizId = 101, KvizNaslov = "C# Osnove", KorisnikId = 12 };
            var kviz2 = new Kviz { KvizId = 102, KvizNaslov = "C# Napredni", KorisnikId = 12 };

            _context.Korisnici.Add(korisnik);
            _context.Kvizovi.AddRange(kviz1, kviz2);
            _context.SaveChanges();

            var result = await _controller.Delete(12);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);

            var obrisaniKorisnik = await _context.Korisnici.FindAsync(12);
            Assert.Null(obrisaniKorisnik);

            var azuriraniKviz1 = await _context.Kvizovi.FindAsync(101);
            var azuriraniKviz2 = await _context.Kvizovi.FindAsync(102);

            Assert.NotNull(azuriraniKviz1);
            Assert.Null(azuriraniKviz1.KorisnikId); 
            Assert.NotNull(azuriraniKviz2);
            Assert.Null(azuriraniKviz2.KorisnikId);
        }

        #endregion

        #region PROFIL TESTOVI (Gamifikacija i Progres bar)

        [Fact]
        public void Profil_IzracunavaIspravanProcenatProgresaZaSledeciNivo()
        {
            var nivo1 = new Nivo { NivoId = 1, NazivNivoa = "Početnik", GranicniPoeniNivoa = 100 };
            var nivo2 = new Nivo { NivoId = 2, NazivNivoa = "Napredni", GranicniPoeniNivoa = 300 };

            var korisnik = new Korisnik
            {
                KorisnikId = 1,
                KorisnikIme = "progres_user",
                KorisnikLozinka = "123",
                UkupnoPoena = 150,
                NivoId = 1,
                Nivo = nivo1
            };

            _context.Nivoi.AddRange(nivo1, nivo2);
            _context.Korisnici.Add(korisnik);
            _context.SaveChanges();

            PostaviUlogovanogKorisnika("progres_user");

            var result = _controller.Profil();

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(25.0, _controller.ViewBag.ProcenatProgresa);
            Assert.Equal("Napredni", _controller.ViewBag.SledeciNivoNaziv);
            Assert.Equal(300, _controller.ViewBag.MaxPoenaZaSledeci);
            Assert.False(_controller.ViewBag.JeMaksimalanNivo);
        }

        #endregion

        #region STUDENTI TESTOVI

        

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller.Dispose();
        }
    }
}
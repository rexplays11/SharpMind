using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using SharpMind.Controllers;
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Xunit;

namespace SharpMind.Tests
{
    public class LekcijeControllerTests : IDisposable
    {
        private readonly SharpMindContext _context;
        private readonly LekcijeController _controller;
        private readonly Mock<ITempDataDictionary> _tempDataMock;

        public LekcijeControllerTests()
        {
            var options = new DbContextOptionsBuilder<SharpMindContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SharpMindContext(options);
            _context.Database.EnsureCreated();

            _tempDataMock = new Mock<ITempDataDictionary>();

            _controller = new LekcijeController(_context)
            {
                TempData = _tempDataMock.Object
            };
        }

        #region TESTOVI ZA INDEX (SORTIRANJE I PAGINACIJA)

        [Fact]
        public void Index_Paginacija_VracaTacanBrojLekcijaISetujeViewBag()
        {
            for (int i = 1; i <= 30; i++)
            {
                _context.Lekcije.Add(new Lekcija { LekcijaId = i, LekcijaNaslov = $"Lekcija {i}" });
            }
            _context.SaveChanges();

            var result = _controller.Index(sortOrder: "id_asc", page: 2, pageSize: 10);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Lekcija>>(viewResult.Model);

            Assert.Equal(10, model.Count);
            Assert.Equal(11, model.First().LekcijaId); 

            Assert.Equal(2, _controller.ViewBag.CurrentPage);
            Assert.Equal(3, _controller.ViewBag.TotalPages);
            Assert.Equal(30, _controller.ViewBag.TotalItems);
        }

        [Fact]
        public void Index_ProsledjenaStranicaVanOpsega_KoregujeStranicuNaGranicneVrednosti()
        {
            _context.Lekcije.Add(new Lekcija { LekcijaId = 1, LekcijaNaslov = "Test" });
            _context.SaveChanges();

            var result = _controller.Index(sortOrder: "id_asc", page: 999, pageSize: 10);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(1, _controller.ViewBag.CurrentPage); 
        }

        #endregion

        #region TESTOVI ZA PREGLED LEKCIJA I STABLO

        [Fact]
        public void PregledLekcija_VracaKompletnuStrukturuStabla()
        {
            var poglavlje = new Poglavlje { PoglavljeId = 1, PoglavljeNaziv = "Uvod u C#" };
            var potpoglavlje = new Potpoglavlje { PotpoglavljeId = 10, NazivPotpoglavlja = "Sintaksa", PoglavljeId = 1 };

            _context.Poglavlja.Add(poglavlje);
            _context.Potpoglavlja.Add(potpoglavlje);
            _context.Lekcije.Add(new Lekcija { LekcijaId = 5, LekcijaNaslov = "Tipovi podataka", PoglavljeId = 1, PotpoglavljeId = 10 });
            _context.SaveChanges();

            var result = _controller.PregledLekcija();

            var viewResult = Assert.IsType<ViewResult>(result);
            var struktura = Assert.IsAssignableFrom<List<Poglavlje>>(viewResult.Model);

            Assert.Single(struktura);
            Assert.Equal("Uvod u C#", struktura.First().PoglavljeNaziv);
            Assert.Single(struktura.First().Potpoglavlja);
        }

        #endregion

        #region TESTOVI ZA MOCKOVANJE KORISNIČKOG IDENTITETA (HttpContext)

        [Fact]
        public void PregledLekcijeDodaj_Post_UspesnoPrepoznajeUlogovanogKorisnikaISanjuUBazu()
        {
            var testUsername = "profesor_nikola";
            _context.Korisnici.Add(new Korisnik { KorisnikId = 44, KorisnikIme = testUsername });
            _context.SaveChanges();

            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(i => i.Name).Returns(testUsername);

            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(p => p.Identity).Returns(mockIdentity.Object);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.User).Returns(mockPrincipal.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var novaLekcija = new Lekcija
            {
                LekcijaId = 100,
                LekcijaNaslov = "Novi entiteti",
                LekcijaTekst = "   Tekst sa razmacima   "
            };

            var result = _controller.PregledLekcijeDodaj(novaLekcija);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("PregledLekcija", redirectResult.ActionName);

            var upisanaLekcija = _context.Lekcije.Find(100);
            Assert.NotNull(upisanaLekcija);
            Assert.Equal("Tekst sa razmacima", upisanaLekcija.LekcijaTekst); 
            Assert.Equal(44, upisanaLekcija.KorisnikId); 
        }

        #endregion

        #region TESTOVI ZA CRUD OPERACIJE

        [Fact]
        public void Create_Post_ValidanModel_RedirectujeNaIndex()
        {
            var lekcija = new Lekcija { LekcijaId = 1, LekcijaNaslov = "Validna lekcija" };

            var result = _controller.Create(lekcija);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.NotNull(_context.Lekcije.Find(1));
        }



        [Fact]
        public void Delete_PostojecaLekcija_UspesnoUklanjaIzBaze()
        {
            _context.Lekcije.Add(new Lekcija { LekcijaId = 10, LekcijaNaslov = "Za brisanje" });
            _context.SaveChanges();

            var result = _controller.Delete(10);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var obrisana = _context.Lekcije.Find(10);
            Assert.Null(obrisana);
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller.Dispose();
        }
    }
}
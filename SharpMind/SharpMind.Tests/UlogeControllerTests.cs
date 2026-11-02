using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMindApi.Controllers;
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SharpMind.Tests
{
    public class UlogeControllerTests : IDisposable
    {
        private readonly SharpMindContext _context;
        private readonly UlogeController _controller;
        private readonly Mock<ITempDataDictionary> _tempDataMock;

        public UlogeControllerTests()
        {

            var options = new DbContextOptionsBuilder<SharpMindContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SharpMindContext(options);
            _context.Database.EnsureCreated();

            _tempDataMock = new Mock<ITempDataDictionary>();

            _controller = new UlogeController(_context)
            {
                TempData = _tempDataMock.Object
            };
        }

        #region TESTOVI ZA INDEX METODU (PAGINACIJA I SORTIRANJE ULOGA)

        [Fact]
        public void Index_UspjesnoUcitavanje_PrimjenjujePaginacijuIVracaTacneMetapodatke()
        {
            _context.Uloge.AddRange(
                new Uloga { UlogaId = 1, UlogaNaziv = "Admin" },
                new Uloga { UlogaId = 2, UlogaNaziv = "Profesor" },
                new Uloga { UlogaId = 3, UlogaNaziv = "Student" }
            );
            _context.SaveChanges();

            var result = _controller.Index(sortOrder: "id_asc", page: 1, pageSize: 2);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Uloga>>(viewResult.Model);

            Assert.Equal(2, model.Count); 
            Assert.Equal(1, model[0].UlogaId); 
            Assert.Equal(1, _controller.ViewBag.CurrentPage);
            Assert.Equal(2, _controller.ViewBag.TotalPages);
            Assert.Equal(3, _controller.ViewBag.TotalItems);
        }

        #endregion

        #region TESTOVI ZA KREIRANJE ULOGE I VALIDACIJU JEDINSTVENOSTI

        [Fact]
        public void Create_Post_NoviNazivUloge_UspjesnoZapisujeUBazuIDelegriseNaIndex()
        {
            var novaUloga = new Uloga { UlogaId = 10, UlogaNaziv = "Moderator" };

            var result = _controller.Create(novaUloga);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
            Assert.NotNull(_context.Uloge.Find(10));
        }

        [Fact]
        public void Create_Post_NazivUlogeVecPostoji_BlokiraUpisIDodajeGreskuUModelState()
        {
            _context.Uloge.Add(new Uloga { UlogaId = 1, UlogaNaziv = "Admin" });
            _context.SaveChanges();

            var dupliranaUloga = new Uloga { UlogaId = 2, UlogaNaziv = "Admin" };

            var result = _controller.Create(dupliranaUloga);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("NazivUloge"));
            Assert.Equal("Uloga sa ovim nazivom već postoji.", _controller.ModelState["NazivUloge"].Errors[0].ErrorMessage);

            Assert.Equal(1, _context.Uloge.Count());
        }

        #endregion

        #region TESTOVI ZA SIGURNOSNE RESTRIKCIJE BRISANJA (REFERENCIJALNI INTEGRITET)

        [Fact]
        public void Delete_UlogaSeAktivnoKoristi_SprecavaBrisanjeIPostavljaTempDataGresku()
        {
            var uloga = new Uloga { UlogaId = 5, UlogaNaziv = "Student" };
            var korisnik = new Korisnik { KorisnikId = 1, KorisnikIme = "Luka", UlogaId = 5 }; 

            _context.Uloge.Add(uloga);
            _context.Korisnici.Add(korisnik);
            _context.SaveChanges();

            var result = _controller.Delete(id: 5);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);

            Assert.NotNull(_context.Uloge.Find(5));

            _tempDataMock.VerifySet(t => t["Error"] = "Nije moguće obrisati ulogu jer postoje korisnici koji su joj dodeljeni.", Times.Once());
        }

        [Fact]
        public void Delete_UlogaNijeDodeljenaKorisnicima_UspjesnoBriseZapisIzBaze()
        {
            var uloga = new Uloga { UlogaId = 8, UlogaNaziv = "PrivremenaUloga" };
            _context.Uloge.Add(uloga);
            _context.SaveChanges();

            var result = _controller.Delete(id: 8);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Null(_context.Uloge.Find(8)); 
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
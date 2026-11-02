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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SharpMind.Tests
{
    public class PotpoglavljaControllerTests : IDisposable
    {
        private readonly SharpMindContext _context;
        private readonly PotpoglavljaController _controller;
        private readonly Mock<ITempDataDictionary> _tempDataMock;

        public PotpoglavljaControllerTests()
        {
            var options = new DbContextOptionsBuilder<SharpMindContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SharpMindContext(options);
            _context.Database.EnsureCreated();

            _tempDataMock = new Mock<ITempDataDictionary>();

            _controller = new PotpoglavljaController(_context)
            {
                TempData = _tempDataMock.Object
            };
        }

        #region TESTOVI ZA INDEX METODU (FILTRIRANJE I DINAMIČKI VIEWBUG)

        [Fact]
        public void Index_SaProsledjenimPoglavljeId_UspjesnoFiltriraStavkeIPostavljaImePoglavlja()
        {
            var poglavlje = new Poglavlje { PoglavljeId = 5, PoglavljeNaziv = "Baze Podataka" };
            var pp1 = new Potpoglavlje { PotpoglavljeId = 1, NazivPotpoglavlja = "SQL Upiti", PoglavljeId = 5 };
            var pp2 = new Potpoglavlje { PotpoglavljeId = 2, NazivPotpoglavlja = "NoSQL Uvod", PoglavljeId = 6 };

            _context.Poglavlja.Add(poglavlje);
            _context.Potpoglavlja.AddRange(pp1, pp2);
            _context.SaveChanges();

            var result = _controller.Index(poglavljeId: 5, sortOrder: "id_asc", page: 1, pageSize: 10);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Potpoglavlje>>(viewResult.Model);

            Assert.Single(model);
            Assert.Equal("SQL Upiti", model.First().NazivPotpoglavlja);
            Assert.Equal("Baze Podataka", _controller.ViewBag.ImePoglavlja);
            Assert.Equal(5, _controller.ViewBag.SelectedPoglavljeId);
        }

        [Fact]
        public void Index_BezPoglavljeId_VracaSveStavkeSaZabranomPrekoracenjaPaginacije()
        {
            var pp1 = new Potpoglavlje { PotpoglavljeId = 1, NazivPotpoglavlja = "PP1" };
            var pp2 = new Potpoglavlje { PotpoglavljeId = 2, NazivPotpoglavlja = "PP2" };
            _context.Potpoglavlja.AddRange(pp1, pp2);
            _context.SaveChanges();

            var result = _controller.Index(poglavljeId: null, sortOrder: "id_desc", page: 5, pageSize: 25);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Potpoglavlje>>(viewResult.Model);

            Assert.Equal(2, model.Count);
            Assert.Equal("Sva Poglavlja", _controller.ViewBag.ImePoglavlja);
            Assert.Equal(1, _controller.ViewBag.CurrentPage); 
        }

        #endregion

        #region TESTOVI ZA PO POGLAVLJU I PO POTPOGLAVLJU (ALTERNATIVNI POGLEDI)

        [Fact]
        public void PoPoglavlju_NepostojeciId_VracaNotFoundResult()
        {
            var result = _controller.PoPoglavlju(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void PoPotpoglavlju_KadaNemaLekcija_UspjesnoZapisujePorukuUViewBagIVracaPogledLista()
        {
            var potpoglavlje = new Potpoglavlje { PotpoglavljeId = 12, NazivPotpoglavlja = "Napredni C#" };
            _context.Potpoglavlja.Add(potpoglavlje);
            _context.SaveChanges();

            var result = _controller.PoPotpoglavlju(12);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Lista", viewResult.ViewName); 

            var model = Assert.IsAssignableFrom<List<Lekcija>>(viewResult.Model);
            Assert.Empty(model);
            Assert.Equal("Nema pronađenih lekcija za ovo potpoglavlje.", _controller.ViewBag.Poruka);
            Assert.Equal("Napredni C#", _controller.ViewBag.Naslov);
        }

        [Fact]
        public void PoPotpoglavlju_SaPostojecimLekcijama_VracaPopunjenModelUViewLista()
        {
            var potpoglavlje = new Potpoglavlje { PotpoglavljeId = 15, NazivPotpoglavlja = "Asinhrono programiranje" };
            var nivo = new Nivo { NivoId = 1, NazivNivoa = "Srednji" };
            var lekcija = new Lekcija { LekcijaId = 1, LekcijaNaslov = "Tasks i async/await", PotpoglavljeId = 15, NivoId = 1 };

            _context.Potpoglavlja.Add(potpoglavlje);
            _context.Nivoi.Add(nivo);
            _context.Lekcije.Add(lekcija);
            _context.SaveChanges();

            var result = _controller.PoPotpoglavlju(15);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Lista", viewResult.ViewName);

            var model = Assert.IsAssignableFrom<List<Lekcija>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Tasks i async/await", model.First().LekcijaNaslov);
        }

        #endregion

       

        [Fact]
        public async Task Details_ProsledjenNullId_VracaNotFound()
        {
            var result = await _controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_PostojeciId_VracaPogledSaUkljucenimPoglavljem()
        {
            var poglavlje = new Poglavlje { PoglavljeId = 1, PoglavljeNaziv = "Uvod" };
            var potpoglavlje = new Potpoglavlje { PotpoglavljeId = 100, NazivPotpoglavlja = "Uvodna lekcija", PoglavljeId = 1 };

            _context.Poglavlja.Add(poglavlje);
            _context.Potpoglavlja.Add(potpoglavlje);
            await _context.SaveChangesAsync();

            var result = await _controller.Details(100);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Potpoglavlje>(viewResult.Model);

            Assert.Equal(100, model.PotpoglavljeId);
            Assert.NotNull(model.Poglavlje);
            Assert.Equal("Uvod", model.Poglavlje.PoglavljeNaziv);
        }
       
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller.Dispose();
        }
    }
}
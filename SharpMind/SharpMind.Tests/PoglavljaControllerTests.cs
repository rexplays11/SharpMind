using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMind.Controllers;
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SharpMind.Tests
{
    public class PoglavljaControllerTests : IDisposable
    {
        private readonly SharpMindContext _context;
        private readonly PoglavljaController _controller;
        private readonly Mock<ITempDataDictionary> _tempDataMock;

        public PoglavljaControllerTests()
        {
            var options = new DbContextOptionsBuilder<SharpMindContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SharpMindContext(options);
            _context.Database.EnsureCreated();

            _tempDataMock = new Mock<ITempDataDictionary>();

            _controller = new PoglavljaController(_context)
            {
                TempData = _tempDataMock.Object
            };
        }

        #region TESTOVI ZA METODU INDEX (PAGINACIJA, SORTIRANJE I EAGER LOADING)

        [Fact]
        public void Index_UspjesnoUcitavanje_UkljucujePotpoglavljaIVracaTacanBrojStranica()
        {

            var poglavlje = new Poglavlje { PoglavljeId = 1, PoglavljeNaziv = "Osnove C# programiranja" };
            var potpoglavlje = new Potpoglavlje { PotpoglavljeId = 10, PoglavljeId = 1, NazivPotpoglavlja = "Sintaksa i varijable" };

            _context.Poglavlja.Add(poglavlje);
            _context.Potpoglavlja.Add(potpoglavlje);
            _context.SaveChanges();

            var result = _controller.Index(sortOrder: "id_asc", page: 1, pageSize: 5);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Poglavlje>>(viewResult.Model);

            Assert.Single(model);
            Assert.NotNull(model.First().Potpoglavlja);
            Assert.Single(model.First().Potpoglavlja);

            Assert.Equal(1, _controller.ViewBag.CurrentPage);
            Assert.Equal(1, _controller.ViewBag.TotalItems);
            Assert.Equal(1, _controller.ViewBag.TotalPages);
        }

        [Fact]
        public void Index_PraznaBaza_HvataIzuzetakIVracaPraznuListuSaPorukom()
        {

            var result = _controller.Index(sortOrder: "id_desc", page: 1, pageSize: -10);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Poglavlje>>(viewResult.Model);
            Assert.Empty(model);
            Assert.Equal(25, _controller.ViewBag.CurrentPageSize);
        }

        #endregion

        #region TESTOVI ZA KREIRANJE I VALIDACIJU MODELA

        [Fact]
        public void Create_Post_ValidanModel_UspjesnoZapisujeUBazuIRedirektuje()
        {
            var novoPoglavlje = new Poglavlje { PoglavljeId = 7, PoglavljeNaziv = "Objektno orijentisano programiranje" };

            var result = _controller.Create(novoPoglavlje);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);

            var uBazi = _context.Poglavlja.Find(7);
            Assert.NotNull(uBazi);
            Assert.Equal("Objedinjeno", uBazi.Potpoglavlja == null || !uBazi.Potpoglavlja.Any() ? "Objedinjeno" : "Sadrzi");
        }

        [Fact]
        public void Create_Post_NevalidanModel_NeUpisujeUBazuIVracaIstiPogled()
        {
            var nevalidnoPoglavlje = new Poglavlje { PoglavljeId = 8, PoglavljeNaziv = "" }; 
            _controller.ModelState.AddModelError("NazivPoglavlja", "Naziv poglavlja je obavezan polje.");

            var result = _controller.Create(nevalidnoPoglavlje);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Poglavlje>(viewResult.Model);
            Assert.Equal(8, model.PoglavljeId);

            Assert.Null(_context.Poglavlja.Find(8));
        }

        #endregion

        #region TESTOVI ZA IZMJENU I KONKURENTNOST

        [Fact]
        public void Edit_Get_NepostojeciId_VracaNotFoundResult()
        {
            var result = _controller.Edit(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Post_DbUpdateConcurrencyException_GuraGreskuUModelState()
        {
            var poglavlje = new Poglavlje { PoglavljeId = 3, PoglavljeNaziv = "Konkurentna izmena" };

            var mockContext = new Mock<SharpMindContext>(new DbContextOptions<SharpMindContext>());
            mockContext.Setup(c => c.SaveChanges()).Throws<DbUpdateConcurrencyException>();

            var lokalniController = new PoglavljaController(mockContext.Object);

            var result = lokalniController.Edit(poglavlje);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(lokalniController.ModelState.IsValid);
            Assert.True(lokalniController.ModelState.ContainsKey(string.Empty));
        }

        #endregion

     

        [Fact]
        public void Delete_UspjesnoBrisanjeSlobodnogPoglavlja_UklanjaEntitetIzBaze()
        {
            var poglavlje = new Poglavlje { PoglavljeId = 15, PoglavljeNaziv = "Poglavlje bez podveza" };
            _context.Poglavlja.Add(poglavlje);
            _context.SaveChanges();

            var result = _controller.Delete(15);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);

            var uBazi = _context.Poglavlja.Find(15);
            Assert.Null(uBazi);
        }
     
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller.Dispose();
        }
    }
}
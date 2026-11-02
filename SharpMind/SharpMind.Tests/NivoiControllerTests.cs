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
    public class NivoiControllerTests : IDisposable
    {
        private readonly SharpMindContext _context;
        private readonly NivoiController _controller;
        private readonly Mock<ITempDataDictionary> _tempDataMock;

        public NivoiControllerTests()
        {
            var options = new DbContextOptionsBuilder<SharpMindContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SharpMindContext(options);
            _context.Database.EnsureCreated();

            _tempDataMock = new Mock<ITempDataDictionary>();

            _controller = new NivoiController(_context)
            {
                TempData = _tempDataMock.Object
            };
        }

        #region TESTOVI ZA INDEX (PAGINACIJA I SORTIRANJE)

        [Fact]
        public void Index_UspjesnaPaginacija_VracaTacanBrojZapisaIPostavljaViewBag()
        {
            for (int i = 1; i <= 5; i++)
            {
                _context.Nivoi.Add(new Nivo { NivoId = i, NazivNivoa = $"Nivo {i}" });
            }
            _context.SaveChanges();

            var result = _controller.Index(sortOrder: "id_asc", page: 2, pageSize: 2);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Nivo>>(viewResult.Model);

            Assert.Equal(2, model.Count);
            Assert.Equal(3, model.First().NivoId); 

            Assert.Equal(2, _controller.ViewBag.CurrentPage);
            Assert.Equal(3, _controller.ViewBag.TotalPages);
            Assert.Equal(5, _controller.ViewBag.TotalItems);
        }

        [Fact]
        public void Index_NegativanBrojStranice_AutomatskiKoregujeNaPrvuStranicu()
        {
            _context.Nivoi.Add(new Nivo { NivoId = 1, NazivNivoa = "Početni" });
            _context.SaveChanges();

            var result = _controller.Index(sortOrder: "id_asc", page: -5, pageSize: 10);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(1, _controller.ViewBag.CurrentPage); 
        }

        #endregion

        #region TESTOVI ZA CRUD OPERACIJE (CREATE, EDIT, DETAILS)

        [Fact]
        public void Create_Post_ValidanModel_UspjesnoUpisujeUBazuIRedirektuje()
        {
            var noviNivo = new Nivo { NivoId = 10, NazivNivoa = "Napredni C#" };

            var result = _controller.Create(noviNivo);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var uBazi = _context.Nivoi.Find(10);
            Assert.NotNull(uBazi);
            Assert.Equal("Napredni C#", uBazi.NazivNivoa);
        }

        [Fact]
        public void Create_Post_NevalidanModelState_VracaIstiViewBezUpisaUBazu()
        {
            var nevalidanNivo = new Nivo { NivoId = 11, NazivNivoa = "" };
            _controller.ModelState.AddModelError("NivoNaziv", "Naziv nivoa je obavezan polje.");

            var result = _controller.Create(nevalidanNivo);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Nivo>(viewResult.Model);
            Assert.Equal(11, model.NivoId);

            Assert.Null(_context.Nivoi.Find(11));
        }

        [Fact]
        public void Edit_Get_NepostojeciId_VracaNotFoundRezultat()
        {
            var result = _controller.Edit(999); 

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region TESTOVI ZA IZUZETKE (CONCURRENCY I FOREIGN KEY)

        [Fact]
        public void Edit_Post_SimulacijaKonkurentnosti_HvataDbUpdateConcurrencyExceptionIDodajeModelGrešku()
        {
            var nivo = new Nivo { NivoId = 1, NazivNivoa = "Originalni Naziv" };
            _context.Nivoi.Add(nivo);
            _context.SaveChanges();

            var mockContext = new Mock<SharpMindContext>(new DbContextOptions<SharpMindContext>());
            mockContext.Setup(c => c.Update(It.IsAny<Nivo>()));
            mockContext.Setup(c => c.SaveChanges()).Throws<DbUpdateConcurrencyException>();

            var lokalniController = new NivoiController(mockContext.Object);

            var result = lokalniController.Edit(nivo);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(lokalniController.ModelState.IsValid);

            var error = lokalniController.ModelState[""].Errors.First();
            Assert.Contains("Podaci o ovom nivou su promenjeni u bazi", error.ErrorMessage);
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
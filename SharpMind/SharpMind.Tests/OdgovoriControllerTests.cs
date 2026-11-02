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
    public class OdgovoriControllerTests : IDisposable
    {
        private readonly SharpMindContext _context;
        private readonly OdgovoriController _controller;
        private readonly Mock<ITempDataDictionary> _tempDataMock;

        public OdgovoriControllerTests()
        {
            var options = new DbContextOptionsBuilder<SharpMindContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SharpMindContext(options);
            _context.Database.EnsureCreated();

            _tempDataMock = new Mock<ITempDataDictionary>();

            _controller = new OdgovoriController(_context)
            {
                TempData = _tempDataMock.Object
            };
        }

        #region TESTOVI ZA PAGINACIJU I UKLJUČIVANJE PODATAKA (EAGER LOADING)

        [Fact]
        public void Index_UspjesnoUcitavanje_UkljucujePovezanoPitanjeIVracaModel()
        {
            var pitanje = new Pitanje { PitanjeId = 1, PitanjeTekst = "Šta je C#?" };
            var odgovor = new Odgovor
            {
                OdgovorId = 1,
                OdgovorTekst = "Programski jezik",
                PitanjeId = 1,
                Pitanje = pitanje,
                OdgovorTacnost = true
            };

            _context.Pitanja.Add(pitanje);
            _context.Odgovori.Add(odgovor);
            _context.SaveChanges();

            var result = _controller.Index(sortOrder: "id_asc", page: 1, pageSize: 10);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Odgovor>>(viewResult.Model);

            Assert.Single(model);
            Assert.NotNull(model.First().Pitanje);
            Assert.Equal("Šta je C#?", model.First().Pitanje.PitanjeTekst);
        }

        #endregion

     
        [Fact]
        public void NoviOdgovor_Post_ValidanUnosUnutarLimita_UspjesnoZapisujeUBazu()
        {
            int testPitanjeId = 12;
            _context.Odgovori.Add(new Odgovor { OdgovorId = 1, PitanjeId = testPitanjeId, OdgovorTekst = "Prvi ponuđeni odgovor", OdgovorTacnost = false });
            _context.SaveChanges();

            var drugiOdgovor = new Odgovor { OdgovorId = 2, PitanjeId = testPitanjeId, OdgovorTekst = "Drugi ponuđeni (tačan) odgovor", OdgovorTacnost = true };

            var result = _controller.NoviOdgovor(drugiOdgovor);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Objedinjeno", redirectResult.ActionName);

            Assert.Equal(2, _context.Odgovori.Count(o => o.PitanjeId == testPitanjeId));
        }

    
        #region TESTOVI ZA IZMJENU I ALTERNATIVNE RUTE

        [Fact]
        public void Izmena_Get_NepostojeciId_VracaNotFound()
        {
            var result = _controller.Izmena(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Izmena_Post_ValidanModel_UspjesnoMjenjaPodatkeIRedirektujeNaPitanja()
        {
            var stariOdgovor = new Odgovor { OdgovorId = 50, OdgovorTekst = "Stari tekst", PitanjeId = 1, OdgovorTacnost = false };
            _context.Odgovori.Add(stariOdgovor);
            _context.SaveChanges();

            _context.Entry(stariOdgovor).State = EntityState.Detached;

            var izmjenjeniOdgovor = new Odgovor { OdgovorId = 50, OdgovorTekst = "Ažurirani tekst", PitanjeId = 1, OdgovorTacnost = true };

            var result = _controller.Izmena(izmjenjeniOdgovor);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Objedinjeno", redirectResult.ActionName);
            Assert.Equal("Pitanja", redirectResult.ControllerName);

            var uBazi = _context.Odgovori.Find(50);
            Assert.NotNull(uBazi);
            Assert.Equal("Ažurirani tekst", uBazi.OdgovorTekst);
            Assert.True(uBazi.OdgovorTacnost);
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
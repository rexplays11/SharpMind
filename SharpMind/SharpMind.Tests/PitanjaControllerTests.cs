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
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SharpMind.Tests
{
    public class PitanjaControllerTests : IDisposable
    {
        private readonly SharpMindContext _context;
        private readonly PitanjaController _controller;
        private readonly Mock<ITempDataDictionary> _tempDataMock;

        public PitanjaControllerTests()
        {
            var options = new DbContextOptionsBuilder<SharpMindContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SharpMindContext(options);
            _context.Database.EnsureCreated();

            _tempDataMock = new Mock<ITempDataDictionary>();

            _controller = new PitanjaController(_context)
            {
                TempData = _tempDataMock.Object
            };

            var username = "profesor_test";
            var identity = new GenericIdentity(username);
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        #region TESTOVI ZA METODU OBJEDINJENO (FILTRIRANJE I VALIDACIJA)

        [Fact]
        public async Task Objedinjeno_DetekcijaNevalidnihPitanja_UspjesnoPrepoznajeIIzdvajaNevalidnoPitanje()
        {

            var uloga = new Uloga { UlogaId = 1, UlogaNaziv = "Profesor" };
            var korisnik = new Korisnik { KorisnikId = 1, KorisnikIme = "profesor_test", Uloga = uloga };
            var nivo = new Nivo { NivoId = 1, NazivNivoa = "Osnovni", GranicniPoeniNivoa = 50 };

            _context.Uloge.Add(uloga);
            _context.Korisnici.Add(korisnik);
            _context.Nivoi.Add(nivo);

            var pValidno = new Pitanje { PitanjeId = 10, PitanjeTekst = "Validno pitanje?", NivoId = 1, KorisnikId = 1 };
            _context.Odgovori.Add(new Odgovor { OdgovorId = 1, PitanjeId = 10, OdgovorTekst = "Tačno", OdgovorTacnost = true });
            _context.Odgovori.Add(new Odgovor { OdgovorId = 2, PitanjeId = 10, OdgovorTekst = "Netačno", OdgovorTacnost = false });

            var pNevalidno = new Pitanje { PitanjeId = 20, PitanjeTekst = "Nevalidno pitanje?", NivoId = 1, KorisnikId = 1 };
            _context.Odgovori.Add(new Odgovor { OdgovorId = 3, PitanjeId = 20, OdgovorTekst = "Sve je netačno", OdgovorTacnost = false });

            _context.Pitanja.AddRange(pValidno, pNevalidno);
            await _context.SaveChangesAsync();

            var result = await _controller.Objedinjeno(sortOrder: "id_asc", pretragaNaslova: null, nivoId: null, kreatorId: null);

            var viewResult = Assert.IsType<ViewResult>(result);

            var nevalidnaLista = _controller.ViewBag.NevalidnaPitanjaList as List<Pitanje>;
            Assert.NotNull(nevalidnaLista);
            Assert.Single(nevalidnaLista);
            Assert.Equal(20, nevalidnaLista.First().PitanjeId); 
        }

        [Fact]
        public async Task Objedinjeno_FiltriranjePoTekstu_VracaSamoPodudarneRezultate()
        {
            var p1 = new Pitanje { PitanjeId = 1, PitanjeTekst = "Uvod u C# programiranje" };
            var p2 = new Pitanje { PitanjeId = 2, PitanjeTekst = "Napredno objektno orijentisano programiranje" };
            _context.Pitanja.AddRange(p1, p2);
            await _context.SaveChangesAsync();

            var result = await _controller.Objedinjeno(sortOrder: "id_asc", pretragaNaslova: "Uvod", nivoId: null, kreatorId: null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Pitanje>>(viewResult.Model);

            Assert.Single(model);
            Assert.Equal("Uvod u C# programiranje", model.First().PitanjeTekst);
        }

        #endregion

        #region TESTOVI ZA KASKADNO BRISANJE

        [Fact]
        public async Task ObrisiPitanjeSaOdgovorima_Post_UspjesnoBriseIPitanjeIPovezaneOdgovore()
        {
            var pitanje = new Pitanje { PitanjeId = 100, PitanjeTekst = "Pitanje za brisanje" };
            var o1 = new Odgovor { OdgovorId = 10, PitanjeId = 100, OdgovorTekst = "O1" };
            var o2 = new Odgovor { OdgovorId = 11, PitanjeId = 100, OdgovorTekst = "O2" };

            _context.Pitanja.Add(pitanje);
            _context.Odgovori.AddRange(o1, o2);
            await _context.SaveChangesAsync();

            var result = await _controller.ObrisiPitanjeSaOdgovorima(100);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Objedinjeno", redirectResult.ActionName);

            var pitanjeUBazi = await _context.Pitanja.FindAsync(100);
            Assert.Null(pitanjeUBazi);

            var odgovoriUBazi = await _context.Odgovori.Where(o => o.PitanjeId == 100).ToListAsync();
            Assert.Empty(odgovoriUBazi);
        }

        [Fact]
        public async Task ObrisiPitanjeSaOdgovorima_NepostojeciId_VracaNotFound()
        {
            var result = await _controller.ObrisiPitanjeSaOdgovorima(999);

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region TESTOVI ZA KONKURENTNOST I VALIDACIJU MODELA

        [Fact]
        public void IzmenaPitanja_Post_NevalidanModel_VracaIstiPogledSaModelom()
        {
            var pitanje = new Pitanje { PitanjeId = 5, PitanjeTekst = "" }; 
            _controller.ModelState.AddModelError("PitanjeTekst", "Polje je obavezno");

            var result = _controller.IzmenaPitanja(pitanje);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Pitanje>(viewResult.Model);
            Assert.Equal(5, model.PitanjeId);
        }

        [Fact]
        public void Edit_Post_DbUpdateConcurrencyException_GuraGreskuUModelState()
        {
            var pitanje = new Pitanje { PitanjeId = 1, PitanjeTekst = "Test" };

            var mockContext = new Mock<SharpMindContext>(new DbContextOptions<SharpMindContext>());
            mockContext.Setup(c => c.SaveChanges()).Throws<DbUpdateConcurrencyException>();

            var lokalniController = new PitanjaController(mockContext.Object);

            var result = lokalniController.Edit(pitanje);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(lokalniController.ModelState.IsValid);
            Assert.True(lokalniController.ModelState.ContainsKey(string.Empty)); 
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using SharpMind.Controllers;
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SharpMind.Tests
{
    public class TestControllerTests : IDisposable
    {
        private readonly SharpMindContext _context;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly TestController _controller;

        public TestControllerTests()
        {

            var options = new DbContextOptionsBuilder<SharpMindContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new SharpMindContext(options);
            _context.Database.EnsureCreated();

            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            _controller = new TestController(_context, _httpClient);

            PostaviKorisnikaUContext(userId: 1);
        }

        private void PostaviKorisnikaUContext(int userId)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        #region TESTOVI ZA COMPILER API (RUNCODE)

        [Fact]
        public async Task RunCode_PrazanKod_VracaPorukuDaJeKodPrazan()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(""));
            _controller.Request.Body = stream;

            var result = await _controller.RunCode();

            var contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal("Kod je prazan.", contentResult.Content);
            Assert.Equal("text/plain", contentResult.ContentType);
        }

        [Fact]
        public async Task RunCode_UspjesanPozivSpoljnogServisa_VracaOutputKompajlera()
        {
            var kod = "Console.WriteLine(\"Zdravo svete!\");";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(kod));
            _controller.Request.Body = stream;

            var jsonResponse = "{\"output\":\"Zdravo svete!\\n\",\"statusCode\":200}";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            var result = await _controller.RunCode();

            var contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal("Zdravo svete!\n", contentResult.Content);
        }

        #endregion

        #region TESTOVI ZA GENERISANJE TESTA (INDEX)

        [Fact]
        public void Index_NedovoljnoPitanjaUBazi_VracaPogledRezultatSaPorukomGreske()
        {

            var korisnik = new Korisnik { KorisnikId = 1, NivoId = 1 };
            _context.Korisnici.Add(korisnik);
            _context.SaveChanges();

            var result = _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Rezultat", viewResult.ViewName);
            Assert.Contains("Trenutno nema dovoljno pitanja", _controller.ViewBag.NedovoljnoPitanja);
        }

        [Fact]
        public void Index_DovoljnoValidnihPitanja_GeneriseTestSaIzmjesanimOdgovorima()
        {
            var korisnik = new Korisnik { KorisnikId = 1, NivoId = 1 };
            _context.Korisnici.Add(korisnik);

            for (int i = 1; i <= 10; i++)
            {
                _context.Pitanja.Add(new Pitanje { PitanjeId = i, NivoId = 1, PitanjeTekst = $"Pitanje {i}" });
                _context.Odgovori.Add(new Odgovor { OdgovorId = i * 2, PitanjeId = i, OdgovorTekst = "T", OdgovorTacnost = true });
                _context.Odgovori.Add(new Odgovor { OdgovorId = (i * 2) + 1, PitanjeId = i, OdgovorTekst = "N", OdgovorTacnost = false });
            }
            _context.SaveChanges();

            var result = _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<TestPitanjeVM>>(viewResult.Model);
            Assert.Equal(10, model.Count);
            Assert.NotNull(model.First().PonudjeniOdgovori);
        }

        #endregion

        #region TESTOVI ZA SUBMIT I LEVEL-UP SISTEM

        [Fact]
        public void Submit_PolozioSaPreko70iIspunioUslovZaNoviNivo_UspjesnoIzvrsavaLevelUp()
        {
            var korisnik = new Korisnik { KorisnikId = 1, NivoId = 1, UkupnoPoena = 80 };
            _context.Korisnici.Add(korisnik);

            var nivo1 = new Nivo { NivoId = 1, NazivNivoa = "Početni", GranicniPoeniNivoa = 0 };
            var nivo2 = new Nivo { NivoId = 2, NazivNivoa = "Srednji", GranicniPoeniNivoa = 100 };

            _context.Nivoi.AddRange(nivo1, nivo2);
            _context.Pitanja.Add(new Pitanje { PitanjeId = 99, NivoId = 1 });
            _context.Odgovori.Add(new Odgovor { OdgovorId = 500, PitanjeId = 99, OdgovorTacnost = true });
            _context.SaveChanges();


            var formMockData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "pitanje_99", "500" }
            };
            var formCollection = new FormCollection(formMockData);


            var result = _controller.Submit(formCollection);


            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Rezultat", viewResult.ViewName);


            var azuriraniKorisnik = _context.Korisnici.Find(1);
            Assert.Equal(2, azuriraniKorisnik.NivoId); 
            Assert.Equal(85, azuriraniKorisnik.UkupnoPoena); 


            Assert.True(_controller.ViewBag.Polozio);
            Assert.True(_controller.ViewBag.LevelUp);
            Assert.Equal("Srednji", _controller.ViewBag.NoviNivoNaziv);

            Assert.Single(_context.Rezultati.Where(r => r.KorisnikId == 1 && r.OsvojeniPoeni == 5));
        }

        [Fact]
        public void Submit_DovoljnoBodovaAliNijePolozioTest_BlokiraLevelUpIPonistavaBodove()
        {

            var korisnik = new Korisnik { KorisnikId = 1, NivoId = 1, UkupnoPoena = 98 };
            _context.Korisnici.Add(korisnik);

            var nivo1 = new Nivo { NivoId = 1, GranicniPoeniNivoa = 0 };
            var nivo2 = new Nivo { NivoId = 2, NazivNivoa = "Srednji", GranicniPoeniNivoa = 100 };
            _context.Nivoi.AddRange(nivo1, nivo2);


            _context.Pitanja.AddRange(new Pitanje { PitanjeId = 10 }, new Pitanje { PitanjeId = 11 });
            _context.Odgovori.Add(new Odgovor { OdgovorId = 100, PitanjeId = 10, OdgovorTacnost = true });
            _context.Odgovori.Add(new Odgovor { OdgovorId = 101, PitanjeId = 11, OdgovorTacnost = false }); 
            _context.SaveChanges();

            var formMockData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "pitanje_10", "100" },
                { "pitanje_11", "101" }
            };
            var formCollection = new FormCollection(formMockData);

            var result = _controller.Submit(formCollection);

            var viewResult = Assert.IsType<ViewResult>(result);

            var proveraKorisnika = _context.Korisnici.Find(1);
            Assert.Equal(1, proveraKorisnika.NivoId); 
            Assert.Equal(98, proveraKorisnika.UkupnoPoena); 
            Assert.Contains("moraš položiti ovaj test sa najmanje 70%", _controller.ViewBag.Poruka);
            Assert.False(_controller.ViewBag.LevelUp);
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _httpClient.Dispose();
            _controller.Dispose();
        }
    }
}
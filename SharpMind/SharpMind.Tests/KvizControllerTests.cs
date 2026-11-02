using Xunit;
using Moq;
using Moq.Protected;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using SharpMind.Controllers;
using SharpMind.Data;
using SharpMind.Models;

namespace SharpMind.Tests
{
    public class KvizControllerTests : IDisposable
    {
        private readonly SharpMindContext _context;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly KvizController _controller;

        public KvizControllerTests()
        {
            // 1. In-Memory baza za testiranje Entity Framework upita
            var options = new DbContextOptionsBuilder<SharpMindContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SharpMindContext(options);
            SeedDatabase();

            // 2. Mockovanje HTTP klijenta za JDoodle API pozive
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_handlerMock.Object);

            // 3. Inicijalizacija kontrolera
            _controller = new KvizController(_context, _httpClient);

            // Postavljamo lažnog ulogovanog korisnika (Admin)
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
            // Dodajemo uloge
            var adminUloga = new Uloga { UlogaId = 1, UlogaNaziv = "Admin" };
            var profesorUloga = new Uloga { UlogaId = 2, UlogaNaziv = "Profesor" };
            _context.Uloge.AddRange(adminUloga, profesorUloga);

            // Dodajemo korisnike
            var adminKorisnik = new Korisnik { KorisnikId = 1, KorisnikIme = "AdminUser", UlogaId = 1 };
            _context.Korisnici.Add(adminKorisnik);

            // Dodajemo test kviz
            var kviz = new Kviz { KvizId = 1, KvizNaslov = "C# Osnove", TipKviza = "vreme", VremenskoOgranicenje = 300, KorisnikId = 1 };
            _context.Kvizovi.Add(kviz);

            // Dodajemo pitanja i odgovore
            var pitanje1 = new Pitanje { PitanjeId = 1, PitanjeTekst = "Šta je C#?", PitanjeTip = "Tekst", NivoId = 1 };
            var odgovor1 = new Odgovor { OdgovorId = 1, PitanjeId = 1, OdgovorTekst = "Programski jezik", OdgovorTacnost = true };
            var odgovor2 = new Odgovor { OdgovorId = 2, PitanjeId = 1, OdgovorTekst = "Baza podataka", OdgovorTacnost = false };

            _context.Pitanja.Add(pitanje1);
            _context.Odgovori.AddRange(odgovor1, odgovor2);
            _context.KvizPitanja.Add(new KvizPitanje { KvizId = 1, PitanjeId = 1 });

            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _httpClient.Dispose();
        }

        [Fact]
        public void Index_VracaViewResultSaKvizovima()
        {
            var result = _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Kviz>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public void Igraj_PostojeciKvizId_VracaViewResult()
        {
            var result = _controller.Igraj(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Kviz>(viewResult.Model);
            Assert.Equal(1, model.KvizId);
        }

        [Fact]
        public void Igraj_NepostojeciKvizId_VracaNotFound()
        {
            var result = _controller.Igraj(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void ProveriOdgovor_TacniOdgovori_VracaJsonSaTacanTrue()
        {
            var izabraniIds = new List<int> { 1 }; // Id 1 je tačan odgovor za pitanje 1

            var result = _controller.ProveriOdgovor(1, izabraniIds);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);

            Assert.True(doc.RootElement.GetProperty("tacan").GetBoolean());
        }

        [Fact]
        public void ProveriOdgovor_NetacniOdgovori_VracaJsonSaTacanFalse()
        {
            var izabraniIds = new List<int> { 2 }; // Id 2 je netačan odgovor

            var result = _controller.ProveriOdgovor(1, izabraniIds);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            using var doc = JsonDocument.Parse(json);

            Assert.False(doc.RootElement.GetProperty("tacan").GetBoolean());
            Assert.Equal("Programski jezik", doc.RootElement.GetProperty("tacanTekst").GetString());
        }

        [Fact]
        public async Task RunCode_UspesnoIzvrsavanje_VracaContentSaOutputom()
        {
            var request = new KvizController.CodeRequest { Code = "Console.WriteLine(\"Test\");" };

            var lažniOdgovor = new { output = "Test\n" };
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(lažniOdgovor), System.Text.Encoding.UTF8, "application/json")
            };

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponse);

            var result = await _controller.RunCode(request);

            var contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal("Test\n", contentResult.Content);
        }

        [Fact]
        public async Task RunCode_PrazanKod_VracaBadRequest()
        {
            var request = new KvizController.CodeRequest { Code = "" };

            var result = await _controller.RunCode(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Kod ne sme biti prazan.", badRequestResult.Value);
        }

        [Fact]
        public void Delete_PostojeciKviz_BrisseI_VracaRedirect()
        {
            var result = _controller.Delete(1);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Empty(_context.Kvizovi);
        }
    }
}
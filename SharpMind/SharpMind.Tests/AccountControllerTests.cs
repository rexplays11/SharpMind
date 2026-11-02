using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.CodeAnalysis.Elfie.Model.Structures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using SharpMind.Controllers;
using SharpMind.Data;
using SharpMind.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using Xunit;

namespace SharpMind.Tests
{
    public class AccountControllerTests : IDisposable
    {
        private readonly SharpMindContext _context;
        private readonly AccountController _controller;
        private readonly Mock<IServiceProvider> _serviceProviderMock;

        public AccountControllerTests()
        {
            var options = new DbContextOptionsBuilder<SharpMindContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SharpMindContext(options);
            _context.Database.EnsureCreated();

            _controller = new AccountController(_context);

            var httpContextMock = new Mock<HttpContext>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            var authServiceMock = new Mock<IAuthenticationService>();

            authServiceMock
                .Setup(a => a.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            authServiceMock
                .Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var tempDataFactoryMock = new Mock<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory>();
            var tempDataMock = new Mock<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionary>();
            tempDataFactoryMock.Setup(f => f.GetTempData(It.IsAny<HttpContext>())).Returns(tempDataMock.Object);


            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperFactoryMock.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelperMock.Object);
        

            _serviceProviderMock.Setup(s => s.GetService(typeof(IAuthenticationService))).Returns(authServiceMock.Object);
            _serviceProviderMock.Setup(s => s.GetService(typeof(Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory))).Returns(tempDataFactoryMock.Object);
            _serviceProviderMock.Setup(s => s.GetService(typeof(IUrlHelperFactory))).Returns(urlHelperFactoryMock.Object);

            httpContextMock.Setup(h => h.RequestServices).Returns(_serviceProviderMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            _controller.TempData = tempDataMock.Object;
        }

        #region TESTOVI ZA LOGIN (PRIJAVA)

        [Fact]
        public void Login_Get_VracaViewResult()
        {
            var result = _controller.Login();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Login_Post_UspesnaPrijava_PreusmeravaNaHomeIndex()
        {

            var uloga = new Uloga { UlogaId = 2, UlogaNaziv = "Student" };
            var korisnik = new Korisnik
            {
                KorisnikId = 1,
                KorisnikIme = "nikola",
                KorisnikLozinka = "sifra123",
                UlogaId = 2,
                Uloga = uloga
            };

            _context.Uloge.Add(uloga);
            _context.Korisnici.Add(korisnik);
            _context.SaveChanges();

            var result = await _controller.Login("nikola", "sifra123");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_Post_PogresnaLozinka_VracaViewSaGreskom()
        {
            var korisnik = new Korisnik { KorisnikId = 1, KorisnikIme = "nikola", KorisnikLozinka = "sifra123" };
            _context.Korisnici.Add(korisnik);
            _context.SaveChanges();

            var result = await _controller.Login("nikola", "pogresnaSifra");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Neispravno ime ili lozinka.", _controller.ViewBag.Error);
        }

        #endregion

        #region TESTOVI ZA REGISTER (REGISTRACIJA)

        [Fact]
        public void Register_Get_VracaViewResult()
        {
            var result = _controller.Register();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Register_Post_PraznaPolja_VracaViewSaGreskom()
        {
            var result = _controller.Register("", "sifra123");
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Sva polja su obavezna.", _controller.ViewBag.Error);
        }

        [Fact]
        public void Register_Post_ZauzetoKorisnickoIme_VracaViewSaGreskom()
        {
            _context.Korisnici.Add(new Korisnik { KorisnikId = 1, KorisnikIme = "marko", KorisnikLozinka = "111" });
            _context.SaveChanges();

            var result = _controller.Register("marko", "novaSifra");

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Korisničko ime je zauzeto.", _controller.ViewBag.Error);
        }

        [Fact]
        public void Register_Post_UspesnaRegistracija_UpisujeUBazu_I_PreusmeravaNaLogin()
        {

            if (!_context.Uloge.Any(u => u.UlogaId == 2))
            {
                _context.Uloge.Add(new Uloga { UlogaId = 2, UlogaNaziv = "Student" });
            }

            if (!_context.Nivoi.Any(n => n.NivoId == 1))
            {
                _context.Nivoi.Add(new Nivo { NivoId = 1, NazivNivoa = "Početnik" });
            }

            _context.SaveChanges();

            var result = _controller.Register("novistudent", "student123");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);

            var provera = _context.Korisnici.FirstOrDefault(k => k.KorisnikIme == "novistudent");
            Assert.NotNull(provera);
            Assert.Equal("student123", provera.KorisnikLozinka);
            Assert.Equal(2, provera.UlogaId);
            Assert.Equal(1, provera.NivoId);
        }

        #region TESTOVI ZA LOGOUT (ODJAVA)
        #endregion
        [Fact]
        public async Task Logout_UspesnoOdjavljujeKorisnika_I_PreusmeravaNaLogin()
        {
            // Act
            var result = await _controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
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


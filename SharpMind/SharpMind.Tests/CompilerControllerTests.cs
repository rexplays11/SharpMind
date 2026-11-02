using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using SharpMind.Controllers;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Moq.Protected;

namespace SharpMind.Tests
{
    public class CompilerControllerTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly CompilerController _controller;

        public CompilerControllerTests()
        {

            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            _httpClient = new HttpClient(_handlerMock.Object);

            _controller = new CompilerController(_httpClient);
        }

        [Fact]
        public void Index_VracaViewResult()
        {
            var result = _controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Run_PrazanKod_VracaBadRequestSaGreskom()
        {

            var result = await _controller.Run(""); 


            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            var json = JsonSerializer.Serialize(badRequestResult.Value);
            using var doc = JsonDocument.Parse(json);
            Assert.Equal("Kod ne sme biti prazan.", doc.RootElement.GetProperty("error").GetString());
        }

        [Fact]
        public async Task Run_UspesnoKompajliranje_VracaOkResultSaOutputom()
        {
            string poslatiKod = "System.Console.WriteLine(\"Zdravo!\");";

            var lažniJdoodleOdgovor = new { output = "Zdravo!\n" };
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(lažniJdoodleOdgovor), System.Text.Encoding.UTF8, "application/json")
            };


            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponse);

            var result = await _controller.Run(poslatiKod);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var json = JsonSerializer.Serialize(okResult.Value);
            using var doc = JsonDocument.Parse(json);

            Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
            Assert.Equal("Zdravo!\n", doc.RootElement.GetProperty("output").GetString());
        }

        [Fact]
        public async Task Run_EksterniServerVratiGresku_HvataHttpRequestException_I_VracaStatus502()
        {
            string poslatiKod = "System.Console.WriteLine();";


            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadGateway
            };

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponse);


            var result = await _controller.Run(poslatiKod);


            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(502, statusCodeResult.StatusCode);

            var json = JsonSerializer.Serialize(statusCodeResult.Value);
            using var doc = JsonDocument.Parse(json);
            Assert.Equal("Kompajler servis trenutno nije dostupan ili je odbio zahtev.", doc.RootElement.GetProperty("error").GetString());
        }

        [Fact]
        public async Task Run_ServerVratiLosFormatOdgovora_VracaStatus502()
        {

            string poslatiKod = "System.Console.WriteLine();";


            var losFormatOdgovora = new { pogresnoSvojstvo = "nebitno" };
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(losFormatOdgovora), System.Text.Encoding.UTF8, "application/json")
            };

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponse);

            var result = await _controller.Run(poslatiKod);


            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(502, statusCodeResult.StatusCode);

            var json = JsonSerializer.Serialize(statusCodeResult.Value);
            using var doc = JsonDocument.Parse(json);
            Assert.Equal("Format odgovora sa servera za kompajliranje nije validan.", doc.RootElement.GetProperty("error").GetString());
        }
    }
}
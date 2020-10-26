using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnitTestingWebAPI.API.HeaderAppenderHandler;
using UnitTestingWebAPI.API.MessageHandlers;

namespace UnitTestingWebAPI.Tests
{
    [TestFixture]
    public class MessageHandlerTests
    {
        #region Variables
        private EndRequestHandler endRequestHandler;
        private HeaderAppenderHandler headerAppenderHandler;
        #endregion

        #region Setup
        [SetUp]
        public void Setup()
        {
            endRequestHandler = new EndRequestHandler();
            headerAppenderHandler = new HeaderAppenderHandler()
            {
                InnerHandler = endRequestHandler
            };
        }
        #endregion

        [Test]
        public async Task ShouldAppendCustomHeader()
        {
            var invoker = new HttpMessageInvoker(headerAppenderHandler);
            var result = await invoker.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost/api/test/")),
                CancellationToken.None
            );

            Assert.That(result.Headers.Contains("X-WebAPI-Header"), Is.True);
            Assert.That(result.Content.ReadAsStringAsync().Result,
                Is.EqualTo("Unit testing message handlers!")
            );
        }
    }
}

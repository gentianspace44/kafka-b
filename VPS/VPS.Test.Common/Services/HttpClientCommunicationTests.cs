using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;
using System.Threading;
using VPS.API.Common;
using VPS.Domain.Models.Enums;
using VPS.Helpers.Logging;

namespace VPS.Test.Common.Services
{
    public class HttpClientCommunicationTests
    {
        [Fact]
        public async Task SendRequestAsync_Should_Send_Get_Request()
        {
            // Arrange
            var httpClientWrapper = Substitute.For<IHttpClientWrapper>();
            var logger = Substitute.For<ILoggerAdapter<HttpClientCommunication>>();
            var communication = new HttpClientCommunication(httpClientWrapper, logger);

            var url = "https://example.com/api";
            var httpMethod = Domain.Models.Enums.HttpMethod.GET;

            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            httpClientWrapper.GetAsync(url).Returns(expectedResponse);

            // Act
            var response = await communication.SendRequestAsync(url, httpMethod);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendRequestAsync_Should_Send_Post_Request_With_Content()
        {
            // Arrange
            var httpClientWrapper = Substitute.For<IHttpClientWrapper>();
            var logger = Substitute.For<ILoggerAdapter<HttpClientCommunication>>();
            var communication = new HttpClientCommunication(httpClientWrapper, logger);

            var url = "https://example.com/api";
            var httpMethod = Domain.Models.Enums.HttpMethod.POST;
            var content = "Request Content";
            var contentType = "application/json";

            var expectedResponse = new HttpResponseMessage(HttpStatusCode.Created);

            httpClientWrapper.PostAsync(url, Arg.Any<HttpContent>(),default).Returns(expectedResponse);

            // Act
            var response = await communication.SendRequestAsync(url, httpMethod, content, contentType);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task SendRequestAsync_Should_Log_Exception()
        {
            // Arrange
            var httpClientWrapper = Substitute.For<IHttpClientWrapper>();
            var logger = Substitute.For<ILoggerAdapter<HttpClientCommunication>>();
            var communication = new HttpClientCommunication(httpClientWrapper, logger);

            var url = "https://example.com/api";
            var httpMethod = Domain.Models.Enums.HttpMethod.GET;

            var expectedException = new HttpRequestException("Request failed");

            httpClientWrapper.GetAsync(url).Throws(expectedException);

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => communication.SendRequestAsync(url, httpMethod));
        }

        [Fact]
        public async Task SendRequestAsync_Should_Send_Post_Request_With_Content_And_Timeout()
        {
            // Arrange
            var httpClientWrapper = Substitute.For<IHttpClientWrapper>();
            var logger = Substitute.For<ILoggerAdapter<HttpClientCommunication>>();
            var communication = new HttpClientCommunication(httpClientWrapper, logger);

            var url = "https://example.com/api";
            var httpMethod = Domain.Models.Enums.HttpMethod.POST;
            var content = "Request Content";
            var contentType = "application/json";
            var timeout = new TimeSpan(0, 0, 30);
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.Created);

            httpClientWrapper.PostAsync(url, Arg.Any<HttpContent>(), Arg.Any<TimeSpan>()).Returns(expectedResponse);

            // Act
            var response = await communication.SendRequestAsync(url, httpMethod, content, contentType, null, CharsetEncoding.UTF8, timeout);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }
}

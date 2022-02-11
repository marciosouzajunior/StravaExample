using Moq;
using NUnit.Framework;
using StravaExample.Services;
using StravaExample.Services.Impl;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace StravaExampleTest
{
    [TestFixture]
    public class StravaServiceTest
    {

        Mock<IProperties> propertiesMock;
        Mock<IDatabase> databaseMock;
        Mock<IHttpHandler> httpHandlerMock;
        StravaService stravaService;

        string accessToken;
        string refreshToken;
        string validTokenResponse;
        string expiredTokenResponse;

        [SetUp]
        public void Setup()
        {

            propertiesMock = new Mock<IProperties>();
            databaseMock = new Mock<IDatabase>();
            httpHandlerMock = new Mock<IHttpHandler>();

            accessToken = "9a25db04e7ac69cdff7475dee307fe8992be1847";
            refreshToken = "88acaf6706b3b5a7a5423c830f25d508c18add7b";

            validTokenResponse = ""
                + "{\"expires_at\":" + DateTimeUtil.ConvertDateTimeToEpoch(DateTime.Now.AddDays(1)) + ","
                + "\"refresh_token\":\"" + refreshToken + "\","
                + "\"access_token\":\"" + accessToken + "\"}";

            expiredTokenResponse = ""
                + "{\"expires_at\":" + DateTimeUtil.ConvertDateTimeToEpoch(DateTime.Now.AddDays(-1)) + ","
                + "\"refresh_token\":\"" + refreshToken + "\","
                + "\"access_token\":\"" + accessToken + "\"}";

            stravaService = new StravaService(
                propertiesMock.Object,
                databaseMock.Object,
                httpHandlerMock.Object);

        }

        [Test]
        public void ShouldReturnFalse_WhenCallCheckScopeWithPartialScope()
        {
            string partialScope = "activity:write";
            bool result = stravaService.CheckScope(partialScope);
            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldReturnTrue_WhenCallCheckScopeWithAllScopes()
        {
            string allScopes = "activity:read_all,activity:write";
            bool result = stravaService.CheckScope(allScopes);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task ShouldRemoveProperties_WhenCallDisconnect()
        {
            HttpResponseMessage httpResponseOK = new HttpResponseMessage(HttpStatusCode.OK);
            propertiesMock.Setup(properties => properties.Contains("stravaToken")).Returns(true);
            propertiesMock.Setup(properties => properties.Get("stravaToken")).Returns(validTokenResponse);
            httpHandlerMock.Setup(httpHandler => httpHandler.PostAsync(It.IsAny<string>(), null)).Returns(Task.FromResult(httpResponseOK));

            await stravaService.Disconnect();

            propertiesMock.Verify(properties => properties.Remove("stravaToken"), Times.Once);
            propertiesMock.Verify(properties => properties.Remove("stravaSyncDate"), Times.Once);
        }

        [Test]
        public void ShouldReturnTrue_WhenCallIsConnectedAndHasToken()
        {
            propertiesMock.Setup(properties => properties.Contains("stravaToken")).Returns(true);
            bool result = stravaService.IsConnected();
            Assert.IsTrue(result);
        }

        [Test]
        public async Task ShouldReturnSavedToken_WhenCallGetAccessTokenAndTokenIsValid()
        {
            HttpResponseMessage httpResponseOK = new HttpResponseMessage(HttpStatusCode.OK);
            propertiesMock.Setup(properties => properties.Contains("stravaToken")).Returns(true);
            propertiesMock.Setup(properties => properties.Get("stravaToken")).Returns(validTokenResponse);

            string result = await stravaService.GetAccessToken();

            Assert.AreEqual(accessToken, result);
        }

        [Test]
        public async Task ShouldRefreshToken_WhenCallGetAccessTokenAndTokenIsExpired()
        {
            HttpResponseMessage httpResponseOK = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponseOK.Content = new StringContent(validTokenResponse);
            propertiesMock.Setup(properties => properties.Contains("stravaToken")).Returns(true);
            propertiesMock.Setup(properties => properties.Get("stravaToken")).Returns(expiredTokenResponse);
            httpHandlerMock.Setup(httpHandler => httpHandler.PostAsync(It.IsAny<string>(), It.IsAny<StringContent>())).Returns(Task.FromResult(httpResponseOK));

            string result = await stravaService.GetAccessToken();

            Assert.AreEqual(accessToken, result);
            propertiesMock.Verify(properties => properties.Save("stravaToken", validTokenResponse), Times.Once);
        }

        [Test]
        public void ShouldReturnSavedSyncDate_WhenCallGetSyncDate()
        {
            long syncDateMock = DateTimeUtil.ConvertDateTimeToEpoch(DateTime.Now);
            propertiesMock.Setup(properties => properties.Contains("stravaSyncDate")).Returns(true);
            propertiesMock.Setup(properties => properties.Get("stravaSyncDate")).Returns(syncDateMock);

            long result = stravaService.GetSyncDate();

            Assert.AreEqual(syncDateMock, result);
        }

        [Test]
        public void ShouldInitializeAndSaveSyncDate_WhenCallGetSyncDate()
        {
            propertiesMock.Setup(properties => properties.Contains("stravaSyncDate")).Returns(false);

            long result = stravaService.GetSyncDate();

            Assert.AreEqual(DateTime.Now.ToString("d"), DateTimeUtil.ConvertEpochToDateTime(result).ToString("d"));
            propertiesMock.Verify(properties => properties.Save("stravaSyncDate", result), Times.Once);
        }

        [Test]
        public void ShouldSaveSyncDate_WhenCallUpdateSyncDate()
        {
            long result = stravaService.UpdateSyncDate();

            Assert.AreEqual(DateTime.Now.ToString("d"), DateTimeUtil.ConvertEpochToDateTime(result).ToString("d"));
            propertiesMock.Verify(properties => properties.Save("stravaSyncDate", result), Times.Once);
        }

    }
}

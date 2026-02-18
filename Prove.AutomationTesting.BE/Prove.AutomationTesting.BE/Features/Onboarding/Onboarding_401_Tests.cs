using System.Net;
using System.Net.Http.Headers;
using NUnit.Framework;
using ApiTests.Infrastructure;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiTests.Features.Onboarding;

[TestFixture]
public class Onboarding_401_Tests : OnboardingApiTestBase
{
    [Test]
    public async Task Should_Return_401_On_Expired_Token()
    {
        var expired = Settings.TestCases.Auth.ExpiredToken;

        Assert.That(expired, Is.Not.Null.And.Not.Empty,
            "Manca TestCases:Auth:ExpiredToken in appsettings.TestCases.json");

        var request = new HttpRequestMessage(HttpMethod.Get, "/onboarding/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", expired);

        HttpResponseMessage? response = null;

        try
        {
            response = await HttpClient.SendAsync(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
        finally
        {
            // Se fallisce, salva request/response nei risultati (utile in locale e in DevOps)
            await TestArtifacts.AttachHttpOnFailureAsync(
                $"{TestContext.CurrentContext.Test.Name}_http",
                request,
                response);
        }
    }
}

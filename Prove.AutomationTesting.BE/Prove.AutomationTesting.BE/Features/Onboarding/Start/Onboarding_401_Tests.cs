using Allure.NUnit;
using ApiTests.Features.Onboarding;
using ApiTests.Infrastructure;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Prove.AutomationTesting.BE.Features.Onboarding.Start;

[AllureNUnit]
[TestFixture]
public class Onboarding_401_Tests : OnboardingApiTestBase
{
    [Test]
    [Property("PBI", "12346")]
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

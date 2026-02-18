using NUnit.Framework;
using System.Threading.Tasks;

namespace ApiTests.Features.Onboarding;

[TestFixture]
public class Onboarding_200_Tests : OnboardingApiTestBase
{
    [Test]
    public async Task Should_Return_200_On_Valid_Request()
    {
        // TODO: endpoint/payload reali.
        // var request = new HttpRequestMessage(HttpMethod.Post, "/onboarding/start") { ... };
        // var response = await HttpClient.SendAsync(request);
        // Assert.That((int)response.StatusCode, Is.EqualTo(200));

        await Task.CompletedTask;
        Assert.Fail("Stub: implementa request reale e assert.");
    }
}

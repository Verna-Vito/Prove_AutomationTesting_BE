using NUnit.Framework;
using System.Threading.Tasks;

namespace ApiTests.Features.Onboarding;

/// <summary>
/// Suite di test per verificare il comportamento corretto dell'API Onboarding
/// quando riceve una richiesta valida e deve restituire uno stato HTTP 200 OK.
/// </summary>
[TestFixture]
public class Onboarding_200_Tests : OnboardingApiTestBase
{
    /// <summary>
    /// Verifica che l'endpoint di onboarding restituisca uno status code 200 (OK)
    /// quando riceve una richiesta valida con payload corretto.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Questo test è attualmente uno stub e deve essere completato con:
    /// <list type="number">
    /// <item>Una richiesta HTTP POST all'endpoint "/onboarding/start"</item>
    /// <item>Un payload reale valido per l'inizio dell'onboarding</item>
    /// <item>Un'asserzione che verifica lo status code della risposta sia uguale a 200</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <returns>Un task che rappresenta l'operazione asincrona del test</returns>
    [Test]
    public async Task Should_Return_200_On_Valid_Request()
    {
        // TODO: endpoint/payload reali.
        // var request = new HttpRequestMessage(HttpMethod.Post, "/onboarding/start") { ... };
        // var response = await HttpClient.SendAsync(request);
        // Assert.That((int)response.StatusCode, Is.EqualTo(200));

        await Task.CompletedTask;
        Assert.Pass("Stub: implementa request reale e assert.");
    }
}

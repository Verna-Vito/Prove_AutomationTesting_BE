// ESEMPIO DI UTILIZZO - ApiHelper nelle fixture di test
// ========================================================
// Questo file mostra come integrare l'ApiHelper nei test Onboarding

using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using ApiTests.Infrastructure.Utilities;
using ApiTests.Features.Onboarding;

namespace Prove.AutomationTesting.BE.Features.Onboarding.Examples;

/// <summary>
/// Esempio di come usare ApiHelper e ResponseValidator nei test.
/// NOTA: questo file è solo documentativo. Elimina o adatta secondo le tue necessità.
/// </summary>
[TestFixture]
public class ApiHelper_UsageExamples : OnboardingApiTestBase
{
    private ApiHelper _apiHelper = null!;

    protected override void AfterOnboardingSetUp()
    {
        // Inizializza l'ApiHelper con HttpClient e Settings
        _apiHelper = new ApiHelper(HttpClient, Settings);
    }

    // ===== ESEMPIO 1: GET semplice con validazione =====
    [Test]
    [Ignore("Questo è un esempio - adatta con endpoint reale")]
    public async Task Example_GetWithValidation()
    {
        // Esegui GET e valida
        var response = await _apiHelper.GetAsync("/api/users/me");
        
        response.Validate()
            .AssertIsSuccess()
            .AssertIsJsonContent()
            .AssertHasHeader("X-Request-Id");
    }

    // ===== ESEMPIO 2: POST con serializzazione e deserializzazione =====
    [Test]
    [Ignore("Questo è un esempio - adatta con endpoint reale")]
    public async Task Example_PostWithBodyAndResponse()
    {
        var requestBody = new
        {
            email = "test@example.com",
            deviceId = DeviceId,
            acceptLanguage = AcceptLanguage
        };

        // Esegui POST e deserializza la risposta
        var result = await _apiHelper.PostAsync<object, dynamic>(
            "/onboarding/start", 
            requestBody);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.id, Is.Not.Null.And.Not.Empty);
    }

    // ===== ESEMPIO 3: Validazione dettagliata di status code =====
    [Test]
    [Ignore("Questo è un esempio - adatta con endpoint reale")]
    public async Task Example_ValidateStatusCode()
    {
        var response = await _apiHelper.PostAsync("/onboarding/invalid", new { });

        response.Validate()
            .AssertStatusCode(HttpStatusCode.BadRequest);
    }

    // ===== ESEMPIO 4: Validazione con uno tra più status code =====
    [Test]
    [Ignore("Questo è un esempio - adatta con endpoint reale")]
    public async Task Example_ValidateMultipleStatusCodes()
    {
        var response = await _apiHelper.GetAsync("/api/non-existent");

        response.Validate()
            .AssertStatusCodeIsOneOf(
                HttpStatusCode.NotFound,
                HttpStatusCode.Unauthorized);
    }

    // ===== ESEMPIO 5: Retry automatico =====
    [Test]
    [Ignore("Questo è un esempio - adatta con endpoint reale")]
    public async Task Example_GetWithRetry()
    {
        var response = await _apiHelper.GetWithRetryAsync(
            "/api/endpoint",
            maxRetries: 3);

        response.Validate().AssertIsSuccess();
    }

    // ===== ESEMPIO 6: POST con retry e deserializzazione =====
    [Test]
    [Ignore("Questo è un esempio - adatta con endpoint reale")]
    public async Task Example_PostWithRetryAndDeserialization()
    {
        var result = await _apiHelper.PostWithRetryAsync<object, dynamic>(
            "/onboarding/start",
            new { email = "test@example.com" },
            maxRetries: 3);

        Assert.That(result.id, Is.Not.Null);
    }

    // ===== ESEMPIO 7: Validazione header personalizzati =====
    [Test]
    [Ignore("Questo è un esempio - adatta con endpoint reale")]
    public async Task Example_ValidateCustomHeaders()
    {
        var response = await _apiHelper.GetAsync("/api/users/me");

        var requestIdHeader = response.Validate()
            .AssertHasHeader("X-Request-Id")
            .GetHeaderValue("X-Request-Id");

        Assert.That(requestIdHeader, Is.Not.Empty);
    }

    // ===== ESEMPIO 8: Pattern fluente completo =====
    [Test]
    [Ignore("Questo è un esempio - adatta con endpoint reale")]
    public async Task Example_FullFluentPattern()
    {
        var response = await _apiHelper.PostAsync(
            "/onboarding/start",
            new { email = "test@example.com", deviceId = DeviceId });

        // Valida tutto in una catena fluente
        var validator = response.Validate()
            .AssertStatusCode(HttpStatusCode.Created)
            .AssertIsJsonContent()
            .AssertHasHeader("Location");

        // Usa la risposta per ulteriori operazioni
        var actualResponse = validator.GetResponse();
        var content = await actualResponse.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("id"));
    }
}

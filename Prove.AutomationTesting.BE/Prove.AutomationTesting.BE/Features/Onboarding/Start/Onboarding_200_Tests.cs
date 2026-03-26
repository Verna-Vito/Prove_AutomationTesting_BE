using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Prove.AutomationTesting.BE.Features.Onboarding.Start;

[AllureNUnit]
[AllureSuite("Onboarding /start")]
[AllureEpic("BE E2E")]
[AllureFeature("Onboarding")]
[AllureStory("Start")]
[AllureLink("PBI 14", "https://github.com/ESSE4/backend/issues/14")]
[AllureTag("pbi:14", "onboarding", "start")]
[TestFixture]
[Category("Validation Test")]
public class Onboarding_200_Tests
{
    // --- MOCK: finto client BE (in reale sarebbe HttpClient)
    private FakeOnboardingApiClient _api = null!;

    [SetUp]
    public void SetUp()
    {
        _api = new FakeOnboardingApiClient();
    }

    [Test]
    [AllureIssue("5")] // se vuoi usare template {issue}; qui la PBI 14
    [AllureSeverity(SeverityLevel.critical)]
    public async Task Start_should_return_200_when_payload_is_valid()
    {
        // Arrange
        var request = new StartOnboardingRequest(
            email: "vito@example.com",
            country: "IT",
            consent: true
        );

        // Act
        var result = await AllureApi.Step("POST /onboarding/start con payload valido", async () =>
        {
            LogRequest("/onboarding/start", request);
            var res = await _api.StartAsync(request);
            LogResponse(res);
            return res;
        });

        // Assert
        AllureApi.Step("Assert status 200 + body", () =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Body, Does.Contain("started"));
            Assert.Ignore("Proviamolo qui!");
        });
    }

    [Test]
    [AllureIssue("14")]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task Start_should_return_400_when_email_is_missing()
    {
        // Arrange (mock dati invalidi)
        var request = new StartOnboardingRequest(
            email: "",          // email mancante
            country: "IT",
            consent: true
        );

        // Act
        var result = await AllureApi.Step("POST /onboarding/start con email vuota", async () =>
        {
            LogRequest("/onboarding/start", request);
            var res = await _api.StartAsync(request);
            LogResponse(res);
            return res;
        });

        // Assert
        AllureApi.Step("Assert status 400 + messaggio errore", () =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(400));
            Assert.That(result.Body, Does.Contain("email"));
        });
    }

    [Test]
    [AllureIssue("14")]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task Start_should_return_409_when_user_already_started_onboarding()
    {
        // Arrange (mock scenario: utente già presente)
        var request = new StartOnboardingRequest(
            email: "already@started.com",
            country: "IT",
            consent: true
        );

        // Act
        var result = await AllureApi.Step("POST /onboarding/start per utente già in onboarding", async () =>
        {
            LogRequest("/onboarding/start", request);
            var res = await _api.StartAsync(request);
            LogResponse(res);
            return res;
        });

        // Assert
        AllureApi.Step("Assert status 409", () =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(409));
            Assert.That(result.Body, Does.Contain("already"));
        });
    }

    [Test]
    [AllureIssue("14")]
    [AllureSeverity(SeverityLevel.normal)]
    public async Task Start_should_return_409_when_user_already_started_onboarding_2()
    {
        // Arrange (mock scenario: utente già presente)
        var request = new StartOnboardingRequest(
            email: "already@started.com",
            country: "IT",
            consent: true
        );

        // Act
        var result = await AllureApi.Step("POST /onboarding/start per utente già in onboarding", async () =>
        {
            LogRequest("/onboarding/start", request);
            var res = await _api.StartAsync(request);
            LogResponse(res);
            return res;
        });

        // Assert
        AllureApi.Step("Assert status 409", () =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(409));
            Assert.That(result.Body, Does.Contain("already"));
        });
    }

    [Test]
    [AllureIssue("14")]
    [AllureSeverity(SeverityLevel.critical)]
    public async Task Start_should_fail_example_to_show_allure_attachments()
    {
        // Questo test è volutamente FAIL per farti vedere come appaiono log e allegati in report.

        var request = new StartOnboardingRequest(
            email: "vito@example.com",
            country: "IT",
            consent: true
        );

        var result = await AllureApi.Step("POST /onboarding/start (FAIL dimostrativo)", async () =>
        {
            LogRequest("/onboarding/start", request);

            // mock: la fake API ritorna 200, ma noi asseriamo result.StatusCode per forzare il fail
            var res = await _api.StartAsync(request);

            LogResponse(res);
            AddTextLog("Nota", "Questo test deve fallire per mostrare il report Allure con attachment.");
            return res;
        });

        AllureApi.Step("Assert volutamente errata (result.StatusCode)", () =>
        {
            Assert.That(400, Is.EqualTo(result.StatusCode), "Fail voluto: il mock torna 200.");
        });
    }

    // ---------------------------
    // Helpers per log su Allure
    // ---------------------------

    private static void LogRequest(string path, object payload)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });

        AllureApi.AddAttachment(
            name: $"REQUEST {path}",
            type: "application/json",
            content: Encoding.UTF8.GetBytes(json)
        );

        AddTextLog("LOG", $"Invocato endpoint: {path}\nTimestamp: {DateTime.UtcNow:O}");
    }

    private static void LogResponse(FakeApiResponse response)
    {
        AllureApi.AddAttachment(
            name: $"RESPONSE status={response.StatusCode}",
            type: "application/json",
            content: Encoding.UTF8.GetBytes(response.Body)
        );

        AddTextLog("LOG", $"Ricevuto status: {response.StatusCode}");
    }

    private static void AddTextLog(string title, string message)
    {
        AllureApi.AddAttachment(
            name: title,
            type: "text/plain",
            content: Encoding.UTF8.GetBytes(message)
        );
    }

    // ---------------------------
    // Mock types
    // ---------------------------

    private sealed class FakeOnboardingApiClient
    {
        // Simula “database” in memoria
        private readonly HashSet<string> _alreadyStarted = new(StringComparer.OrdinalIgnoreCase)
        {
            "already@started.com"
        };

        public Task<FakeApiResponse> StartAsync(StartOnboardingRequest request)
        {
            // Validazioni mock
            if (string.IsNullOrWhiteSpace(request.email))
            {
                return Task.FromResult(new FakeApiResponse(
                    400,
                    JsonSerializer.Serialize(new { error = "email is required" })
                ));
            }

            if (!request.consent)
            {
                return Task.FromResult(new FakeApiResponse(
                    400,
                    JsonSerializer.Serialize(new { error = "consent must be true" })
                ));
            }

            if (_alreadyStarted.Contains(request.email))
            {
                return Task.FromResult(new FakeApiResponse(
                    409,
                    JsonSerializer.Serialize(new { error = "already started" })
                ));
            }

            // OK
            return Task.FromResult(new FakeApiResponse(
                200,
                JsonSerializer.Serialize(new { status = "started", email = request.email })
            ));
        }
    }

    private sealed record StartOnboardingRequest(string email, string country, bool consent);

    private sealed record FakeApiResponse(int StatusCode, string Body);
}

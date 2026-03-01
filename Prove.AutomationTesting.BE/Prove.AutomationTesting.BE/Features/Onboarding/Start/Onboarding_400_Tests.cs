using Allure.Net.Commons;
using Allure.NUnit;
using Allure.NUnit.Attributes;
using ApiTests.Features.Onboarding;
using ApiTests.Infrastructure;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Prove.AutomationTesting.BE.Features.Onboarding.Start;

[AllureNUnit]
[AllureSuite("Onboarding /start")]
[AllureEpic("BE Proviamo")]
[AllureFeature("Registrazione")]
[AllureStory("Start Onboarding")]
[AllureLink("PBI 14", "https://github.com/ESSE4/backend/issues/14")]
[AllureTag("pbi:14", "onboarding", "start")]
[TestFixture]
public class Onboarding_400_Tests : OnboardingApiTestBase
{
    // --- MOCK: finto client BE (in reale sarebbe HttpClient)
    private FakeOnboardingApiClient _api = null!;

    [Test]
    public async Task Should_Return_400_On_NoToken_Passed()
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
            return Task.FromResult(new FakeApiResponse(
                400,
                JsonSerializer.Serialize(new { status = "started", request.email })
            ));
        }
    }

    private sealed record StartOnboardingRequest(string email, string country, bool consent);

    private sealed record FakeApiResponse(int StatusCode, string Body);
}

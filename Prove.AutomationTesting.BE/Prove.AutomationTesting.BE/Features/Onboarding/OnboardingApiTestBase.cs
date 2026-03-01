using Allure.NUnit;
using Allure.NUnit.Attributes;
using ApiTests.Infrastructure;
using NUnit.Framework;
using System;
using System.Net.Http.Headers;

namespace ApiTests.Features.Onboarding;

/// <summary>
/// Base specifica per onboarding.
/// Qui: Accept-Language, DeviceId, eventuali header comuni onboarding.
/// </summary>
public abstract class OnboardingApiTestBase : ApiTestBase
{
    protected string DeviceId = null!;
    protected string AcceptLanguage = null!;

    protected override void AfterApiSetUp()
    {
        AcceptLanguage = Settings.Api.AcceptLanguage;

        // DeviceId: riuso se presente nello state; altrimenti default config; altrimenti genero.
        DeviceId = RunState.Onboarding.DeviceId
                   ?? Settings.Onboarding.DefaultDeviceId
                   ?? Guid.NewGuid().ToString("N");

        // Header comuni
        HttpClient.DefaultRequestHeaders.Accept.Clear();
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        HttpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(AcceptLanguage));

        // Persisto DeviceId se serve (UpdateAsync è safe: lock + atomic write)
        StateStore.UpdateAsync(s => s.Onboarding.DeviceId = DeviceId).GetAwaiter().GetResult();

        AfterOnboardingSetUp();
    }

    /// <summary>
    /// Hook ulteriore per le classi foglia (es. 200 vs 401).
    /// </summary>
    protected virtual void AfterOnboardingSetUp() { }
}

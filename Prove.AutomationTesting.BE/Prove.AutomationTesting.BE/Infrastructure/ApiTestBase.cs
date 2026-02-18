using NUnit.Framework;
using ApiTests.Infrastructure.State;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace ApiTests.Infrastructure;

/// <summary>
/// Base comune a tutte le fixture API.
/// Contiene SOLO cose veramente trasversali:
/// - Settings (appsettings)
/// - HttpClient
/// - Conn string
/// - StateStore (RunState.json) persistente
/// - Hook AfterApiSetUp per estensioni
/// </summary>
public abstract class ApiTestBase
{
    protected TestSettings Settings = null!;
    protected HttpClient HttpClient = null!;
    protected string DbConnectionString = null!;

    protected JsonFileStateStore StateStore = null!;
    protected RunState RunState = null!;

    [SetUp]
    public async Task SetUp()
    {
        Settings = TestConfigLoader.Load();
        DbConnectionString = Settings.Database.ConnectionString;

        HttpClient = new HttpClient
        {
            BaseAddress = new Uri(Settings.Api.BaseUrl),
            Timeout = TimeSpan.FromSeconds(Settings.Api.TimeoutSeconds)
        };

        // State persistente su 1 file unico (non versionato)
        StateStore = new JsonFileStateStore("./.teststate/RunState.json");
        RunState = await StateStore.LoadAsync();

        // Allinea info ambiente nello state (utile per debug e guardrail)
        await StateStore.UpdateAsync(s =>
        {
            s.Env = Settings.Env;
            s.BaseUrl = Settings.Api.BaseUrl;
        });

        AfterApiSetUp();
    }

    /// <summary>
    /// Hook per inizializzazioni specifiche di una famiglia di test.
    /// Evita duplicazioni di [SetUp] in derived.
    /// </summary>
    protected virtual void AfterApiSetUp() { }

    [TearDown]
    public void TearDown()
    {
        HttpClient.Dispose();
    }
}

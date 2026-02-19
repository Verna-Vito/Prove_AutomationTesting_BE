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
    /// <summary>
    /// Configurazione dell'applicazione caricata da appsettings.
    /// Contiene i parametri di connessione, URL base dell'API e timeout.
    /// </summary>
    protected TestSettings Settings = null!;

    /// <summary>
    /// Client HTTP preconfigurato con URL base e timeout derivati da <see cref="Settings"/>.
    /// Utilizzato per eseguire richieste HTTP verso l'API sotto test.
    /// </summary>
    protected HttpClient HttpClient = null!;

    /// <summary>
    /// Stringa di connessione al database estratta da <see cref="Settings"/>.
    /// Disponibile per i test che necessitano di accesso diretto al database.
    /// </summary>
    protected string DbConnectionString = null!;

    /// <summary>
    /// Store persistente che gestisce il caricamento e la persistenza dello stato dei test su file JSON.
    /// Path: ./.teststate/RunState.json (non versionato).
    /// </summary>
    protected JsonFileStateStore StateStore = null!;

    /// <summary>
    /// Stato corrente dei test caricato dallo <see cref="StateStore"/>.
    /// Contiene informazioni condivise tra test (es. token, ID, ambiente) e metadati di esecuzione.
    /// </summary>
    protected RunState RunState = null!;

    /// <summary>
    /// Inizializzazione della fixture API eseguita prima di ogni test.
    /// Inizializza: settings, HttpClient, connection string, state store e ambiente.
    /// Invoca <see cref="AfterApiSetUp()"/> al termine per estensioni specifiche.
    /// </summary>
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
    /// Eseguito al termine di <see cref="SetUp()"/> per ogni test.
    /// </summary>
    protected virtual void AfterApiSetUp() { }

    /// <summary>
    /// Pulizia della fixture API eseguita dopo ogni test.
    /// Rilascia le risorse dell'<see cref="HttpClient"/>.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        HttpClient.Dispose();
    }
}

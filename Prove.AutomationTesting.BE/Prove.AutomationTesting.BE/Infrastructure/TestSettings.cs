namespace ApiTests.Infrastructure;

/// <summary>
/// Mappa strongly-typed della configurazione.
/// - appsettings.json: configurazione (input)
/// - appsettings.TestCases.json: fixtures di test (input)
///
/// Obiettivo: evitare stringhe tipo "Api:BaseUrl" sparse nei test.
/// </summary>
public sealed class TestSettings
{
    public string Env { get; set; } = "Local";
    public ApiSettings Api { get; set; } = new();
    public DatabaseSettings Database { get; set; } = new();
    public OnboardingSettings Onboarding { get; set; } = new();
    public TestCasesSettings TestCases { get; set; } = new();
}

public sealed class ApiSettings
{
    public string BaseUrl { get; set; } = "";
    public string AcceptLanguage { get; set; } = "it-IT";
    public int TimeoutSeconds { get; set; } = 30;
}

public sealed class DatabaseSettings
{
    public string ConnectionString { get; set; } = "";
}

public sealed class OnboardingSettings
{
    public string DefaultDeviceId { get; set; } = "";
}

public sealed class TestCasesSettings
{
    public AuthTestCases Auth { get; set; } = new();
}

public sealed class AuthTestCases
{
    /// <summary>
    /// Token realisticamente valido come "forma" nel tuo ambiente ma già scaduto.
    /// Usalo per testare 401 in modo deterministico senza aspettare scadenze.
    /// </summary>
    public string ExpiredToken { get; set; } = "";

    public string MalformedToken { get; set; } = "";
    public string EmptyToken { get; set; } = "";
}

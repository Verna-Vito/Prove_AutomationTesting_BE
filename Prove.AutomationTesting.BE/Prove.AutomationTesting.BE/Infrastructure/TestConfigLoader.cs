using Microsoft.Extensions.Configuration;
using System;

namespace ApiTests.Infrastructure;

/// <summary>
/// Caricamento config cross-platform.
/// Usa AppContext.BaseDirectory perché in test i file vengono copiati lì.
/// </summary>
public static class TestConfigLoader
{
    public static TestSettings Load()
    {
        var cfg = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.TestCases.json", optional: false, reloadOnChange: false)
            .Build();

        return cfg.Get<TestSettings>() ?? new TestSettings();
    }
}

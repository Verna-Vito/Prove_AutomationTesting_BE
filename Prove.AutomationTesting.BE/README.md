# Struttura files

```
tests/
  ApiTests/
    ApiTests.csproj
    appsettings.json
    appsettings.TestCases.json
    .gitignore
    azure-pipelines.yml   (quando lo porterete su DevOps)

    Infrastructure/
      TestSettings.cs
      TestConfigLoader.cs
      ApiTestBase.cs
      TestArtifacts.cs

      State/
        RunStateModels.cs
        FileLock.cs
        JsonFileStateStore.cs

    Features/
      Onboarding/
        OnboardingApiTestBase.cs
        Onboarding_200_Tests.cs
        Onboarding_401_Tests.cs
```
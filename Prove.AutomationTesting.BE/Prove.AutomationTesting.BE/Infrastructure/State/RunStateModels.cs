using System;

namespace ApiTests.Infrastructure.State;

/// <summary>
/// Dati persistenti prodotti/consumati dai test.
/// </summary>
public sealed class RunState
{
    public string? Env { get; set; }
    public string? BaseUrl { get; set; }

    public string UpdatedAtUtc { get; set; } = DateTime.UtcNow.ToString("O");

    public OnboardingState Onboarding { get; set; } = new();
}

public sealed class OnboardingState
{
    public string? DeviceId { get; set; }
    public string? SessionId { get; set; }
    public string? UserId { get; set; }

    public TokenState Token { get; set; } = new();
}

public sealed class TokenState
{
    public string? Value { get; set; }
}

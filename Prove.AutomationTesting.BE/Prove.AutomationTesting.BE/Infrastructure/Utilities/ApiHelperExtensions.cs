using System.Net.Http;

namespace ApiTests.Infrastructure.Utilities;

/// <summary>
/// Extension methods per semplificare l'uso delle utility nel contesto dei test.
/// Permette di usare il pattern fluent direttamente su HttpResponseMessage.
/// </summary>
public static class ApiHelperExtensions
{
    /// <summary>
    /// Restituisce un ResponseValidator per la validazione fluent della risposta.
    /// </summary>
    /// <example>
    /// var response = await httpClient.GetAsync("/api/endpoint");
    /// response.Validate()
    ///     .AssertIsSuccess()
    ///     .AssertIsJsonContent()
    ///     .AssertHasHeader("X-Request-Id");
    /// </example>
    public static ResponseValidator Validate(this HttpResponseMessage response)
    {
        return new ResponseValidator(response);
    }
}

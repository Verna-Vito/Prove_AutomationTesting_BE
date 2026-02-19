using System;
using System.Net;
using System.Net.Http;

namespace ApiTests.Infrastructure.Utilities;

/// <summary>
/// Utility per la validazione delle risposte HTTP nei test.
/// Centralizza controlli comuni sugli status code, header e contenuto.
/// </summary>
public class ResponseValidator
{
    private readonly HttpResponseMessage _response;

    public ResponseValidator(HttpResponseMessage response)
    {
        _response = response ?? throw new ArgumentNullException(nameof(response));
    }

    /// <summary>
    /// Valida che la risposta sia di successo (status 2xx).
    /// </summary>
    /// <exception cref="InvalidOperationException">Se lo status code non è di successo</exception>
    public ResponseValidator AssertIsSuccess()
    {
        if (!_response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Risposta attesa con successo (2xx), ricevuto: {(int)_response.StatusCode} {_response.StatusCode}");
        }
        return this;
    }

    /// <summary>
    /// Valida che lo status code sia esattamente quello specificato.
    /// </summary>
    public ResponseValidator AssertStatusCode(HttpStatusCode expectedCode)
    {
        if (_response.StatusCode != expectedCode)
        {
            throw new InvalidOperationException(
                $"Status code atteso: {(int)expectedCode} {expectedCode}, ricevuto: {(int)_response.StatusCode} {_response.StatusCode}");
        }
        return this;
    }

    /// <summary>
    /// Valida che lo status code sia uno tra quelli specificati.
    /// </summary>
    public ResponseValidator AssertStatusCodeIsOneOf(params HttpStatusCode[] expectedCodes)
    {
        if (Array.IndexOf(expectedCodes, _response.StatusCode) == -1)
        {
            throw new InvalidOperationException(
                $"Status code ricevuto {(int)_response.StatusCode} non è tra i valori attesi: {string.Join(", ", expectedCodes)}");
        }
        return this;
    }

    /// <summary>
    /// Valida che la risposta contenga l'header specificato.
    /// </summary>
    public ResponseValidator AssertHasHeader(string headerName)
    {
        if (!_response.Headers.Contains(headerName) && !_response.Content.Headers.Contains(headerName))
        {
            throw new InvalidOperationException($"Header '{headerName}' non trovato nella risposta");
        }
        return this;
    }

    /// <summary>
    /// Valida che l'header abbia il valore specificato.
    /// </summary>
    public ResponseValidator AssertHeaderValue(string headerName, string expectedValue)
    {
        AssertHasHeader(headerName);

        var headerValue = GetHeaderValue(headerName);
        if (headerValue != expectedValue)
        {
            throw new InvalidOperationException(
                $"Header '{headerName}' ha valore '{headerValue}', atteso: '{expectedValue}'");
        }
        return this;
    }

    /// <summary>
    /// Valida che la risposta contenga un Content-Type specifico.
    /// </summary>
    public ResponseValidator AssertContentType(string expectedContentType)
    {
        var contentType = _response.Content.Headers.ContentType?.MediaType ?? "";
        if (!contentType.Contains(expectedContentType))
        {
            throw new InvalidOperationException(
                $"Content-Type atteso: '{expectedContentType}', ricevuto: '{contentType}'");
        }
        return this;
    }

    /// <summary>
    /// Valida che il content sia JSON (application/json).
    /// </summary>
    public ResponseValidator AssertIsJsonContent()
    {
        return AssertContentType("application/json");
    }

    /// <summary>
    /// Restituisce il valore dell'header specificato per ulteriori validazioni.
    /// </summary>
    public string GetHeaderValue(string headerName)
    {
        if (_response.Headers.TryGetValues(headerName, out var values))
        {
            return string.Join(", ", values);
        }

        if (_response.Content.Headers.TryGetValues(headerName, out var contentValues))
        {
            return string.Join(", ", contentValues);
        }

        throw new InvalidOperationException($"Header '{headerName}' non trovato");
    }

    /// <summary>
    /// Restituisce la risposta per continuare con ulteriori operazioni.
    /// </summary>
    public HttpResponseMessage GetResponse()
    {
        return _response;
    }
}

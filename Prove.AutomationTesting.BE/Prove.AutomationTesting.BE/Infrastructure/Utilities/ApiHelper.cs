using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiTests.Infrastructure.Utilities;

/// <summary>
/// Helper utility per semplificare le operazioni HTTP comuni nei test API.
/// Centralizza la logica di request/response e riduce la duplicazione nei test.
/// </summary>
public class ApiHelper
{
    private readonly HttpClient _httpClient;
    private readonly TestSettings _settings;

    public ApiHelper(HttpClient httpClient, TestSettings settings)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Esegue una richiesta GET all'endpoint specificato.
    /// </summary>
    /// <param name="endpoint">L'endpoint relativo (es: "/onboarding/start")</param>
    /// <returns>La risposta HTTP</returns>
    public async Task<HttpResponseMessage> GetAsync(string endpoint)
    {
        ValidateEndpoint(endpoint);
        return await _httpClient.GetAsync(endpoint);
    }

    /// <summary>
    /// Esegue una richiesta GET e deserializza la risposta nel tipo specificato.
    /// </summary>
    /// <typeparam name="T">Tipo della risposta atteso</typeparam>
    /// <param name="endpoint">L'endpoint relativo</param>
    /// <returns>L'oggetto deserializzato dalla risposta</returns>
    public async Task<T> GetAsync<T>(string endpoint) where T : class
    {
        var response = await GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await DeserializeAsync<T>(response);
    }

    /// <summary>
    /// Esegue una richiesta POST con body serializzato come JSON.
    /// </summary>
    /// <typeparam name="TRequest">Tipo del body richiesto</typeparam>
    /// <param name="endpoint">L'endpoint relativo</param>
    /// <param name="body">L'oggetto da serializzare nel body</param>
    /// <returns>La risposta HTTP</returns>
    public async Task<HttpResponseMessage> PostAsync<TRequest>(string endpoint, TRequest body) 
        where TRequest : class
    {
        ValidateEndpoint(endpoint);
        var content = SerializeToJson(body);
        return await _httpClient.PostAsync(endpoint, content);
    }

    /// <summary>
    /// Esegue una richiesta POST e deserializza la risposta nel tipo specificato.
    /// </summary>
    /// <typeparam name="TRequest">Tipo del body richiesto</typeparam>
    /// <typeparam name="TResponse">Tipo della risposta atteso</typeparam>
    /// <param name="endpoint">L'endpoint relativo</param>
    /// <param name="body">L'oggetto da serializzare nel body</param>
    /// <returns>L'oggetto deserializzato dalla risposta</returns>
    public async Task<T> PostAsync<TRequest, T>(string endpoint, TRequest body) 
        where TRequest : class 
        where T : class
    {
        var response = await PostAsync(endpoint, body);
        response.EnsureSuccessStatusCode();
        return await DeserializeAsync<T>(response);
    }

    /// <summary>
    /// Esegue una richiesta PUT con body serializzato come JSON.
    /// </summary>
    public async Task<HttpResponseMessage> PutAsync<TRequest>(string endpoint, TRequest body) 
        where TRequest : class
    {
        ValidateEndpoint(endpoint);
        var content = SerializeToJson(body);
        return await _httpClient.PutAsync(endpoint, content);
    }

    /// <summary>
    /// Esegue una richiesta PUT e deserializza la risposta nel tipo specificato.
    /// </summary>
    public async Task<T> PutAsync<TRequest, T>(string endpoint, TRequest body) 
        where TRequest : class 
        where T : class
    {
        var response = await PutAsync(endpoint, body);
        response.EnsureSuccessStatusCode();
        return await DeserializeAsync<T>(response);
    }

    /// <summary>
    /// Esegue una richiesta DELETE all'endpoint specificato.
    /// </summary>
    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        ValidateEndpoint(endpoint);
        return await _httpClient.DeleteAsync(endpoint);
    }

    /// <summary>
    /// Esegue una richiesta GET con retry automatico in caso di errore temporaneo.
    /// Utilizza exponential backoff per i retry.
    /// </summary>
    /// <param name="endpoint">L'endpoint relativo</param>
    /// <param name="maxRetries">Numero massimo di tentativi (default: 3)</param>
    /// <returns>La risposta HTTP al primo tentativo riuscito</returns>
    public async Task<HttpResponseMessage> GetWithRetryAsync(string endpoint, int maxRetries = 3)
    {
        return await ExecuteWithRetryAsync(() => GetAsync(endpoint), maxRetries, endpoint);
    }

    /// <summary>
    /// Esegue una richiesta GET con retry e deserializza il risultato.
    /// </summary>
    public async Task<T> GetWithRetryAsync<T>(string endpoint, int maxRetries = 3) where T : class
    {
        var response = await GetWithRetryAsync(endpoint, maxRetries);
        response.EnsureSuccessStatusCode();
        return await DeserializeAsync<T>(response);
    }

    /// <summary>
    /// Esegue una richiesta POST con retry automatico in caso di errore temporaneo.
    /// </summary>
    public async Task<HttpResponseMessage> PostWithRetryAsync<TRequest>(
        string endpoint, 
        TRequest body, 
        int maxRetries = 3) 
        where TRequest : class
    {
        return await ExecuteWithRetryAsync(
            () => PostAsync(endpoint, body), 
            maxRetries, 
            endpoint);
    }

    /// <summary>
    /// Esegue una richiesta POST con retry e deserializza il risultato.
    /// </summary>
    public async Task<T> PostWithRetryAsync<TRequest, T>(
        string endpoint, 
        TRequest body, 
        int maxRetries = 3) 
        where TRequest : class 
        where T : class
    {
        var response = await PostWithRetryAsync(endpoint, body, maxRetries);
        response.EnsureSuccessStatusCode();
        return await DeserializeAsync<T>(response);
    }

    /// <summary>
    /// Serializza un oggetto a JSON con le opzioni di default.
    /// </summary>
    public static string Serialize<T>(T obj) where T : class
    {
        return JsonSerializer.Serialize(obj, GetDefaultJsonOptions());
    }

    /// <summary>
    /// Deserializza una stringa JSON nel tipo specificato.
    /// </summary>
    public static T Deserialize<T>(string json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON non può essere vuoto", nameof(json));

        return JsonSerializer.Deserialize<T>(json, GetDefaultJsonOptions()) 
            ?? throw new InvalidOperationException($"Deserializzazione di {typeof(T).Name} ha restituito null");
    }

    // ==================== PRIVATE METHODS ====================

    private static StringContent SerializeToJson<T>(T obj) where T : class
    {
        var json = Serialize(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static async Task<T> DeserializeAsync<T>(HttpResponseMessage response) where T : class
    {
        var content = await response.Content.ReadAsStringAsync();
        return Deserialize<T>(content);
    }

    private static void ValidateEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint non può essere vuoto", nameof(endpoint));
    }

    private static async Task<HttpResponseMessage> ExecuteWithRetryAsync(
        Func<Task<HttpResponseMessage>> operation,
        int maxRetries,
        string operationName)
    {
        if (maxRetries < 1)
            throw new ArgumentException("maxRetries deve essere almeno 1", nameof(maxRetries));

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await operation();
                return response;
            }
            catch (HttpRequestException) when (attempt < maxRetries)
            {
                // Exponential backoff: 1s, 2s, 4s, ...
                var delayMs = (int)Math.Pow(2, attempt - 1) * 1000;
                await Task.Delay(delayMs);
            }
        }

        // Se arriviamo qui, tutti i retry sono falliti
        throw new InvalidOperationException(
            $"Operazione '{operationName}' fallita dopo {maxRetries} tentativi");
    }

    private static JsonSerializerOptions GetDefaultJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }
}

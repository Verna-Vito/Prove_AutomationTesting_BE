# 🛠️ ApiHelper Utility - Guida Completa

## 📋 Sommario

L'**ApiHelper** è una utility centralizzata per semplificare le operazioni HTTP nei test API. Riduce la duplicazione di codice e fornisce un'interfaccia fluente per request/response.

---

## 📁 Struttura File

```
Infrastructure/Utilities/
├── ApiHelper.cs                 # Utility principale per richieste HTTP
├── ResponseValidator.cs         # Validazione fluente delle risposte
└── ApiHelperExtensions.cs       # Extension methods per HttpResponseMessage
```

---

## 🚀 Quick Start

### 1. **Inizializzare l'ApiHelper**

Nella tua classe di test che eredita da `OnboardingApiTestBase`:

```csharp
private ApiHelper _apiHelper = null!;

protected override void AfterOnboardingSetUp()
{
    _apiHelper = new ApiHelper(HttpClient, Settings);
}
```

### 2. **Usare ApiHelper per richieste semplici**

```csharp
// GET semplice
var response = await _apiHelper.GetAsync("/api/users/me");

// POST con body
var response = await _apiHelper.PostAsync("/api/users", new { name = "Test" });

// GET con deserializzazione
var user = await _apiHelper.GetAsync<User>("/api/users/123");

// POST con body e risposta tipizzata
var result = await _apiHelper.PostAsync<UserRequest, UserResponse>(
    "/api/users", 
    new UserRequest { name = "Test" });
```

### 3. **Validare le risposte**

```csharp
// Pattern fluente
var response = await _apiHelper.GetAsync("/api/users/me");

response.Validate()
    .AssertIsSuccess()
    .AssertIsJsonContent()
    .AssertHasHeader("X-Request-Id");
```

---

## 📚 API Disponibili

### **ApiHelper - Metodi Principali**

#### **GET Requests**

```csharp
// GET semplice → HttpResponseMessage
public async Task<HttpResponseMessage> GetAsync(string endpoint)

// GET con deserializzazione → T
public async Task<T> GetAsync<T>(string endpoint) where T : class

// GET con retry (max 3 tentativi per default)
public async Task<HttpResponseMessage> GetWithRetryAsync(string endpoint, int maxRetries = 3)

// GET con retry e deserializzazione
public async Task<T> GetWithRetryAsync<T>(string endpoint, int maxRetries = 3) where T : class
```

#### **POST Requests**

```csharp
// POST con body → HttpResponseMessage
public async Task<HttpResponseMessage> PostAsync<TRequest>(string endpoint, TRequest body) 
    where TRequest : class

// POST con body e deserializzazione della risposta → T
public async Task<T> PostAsync<TRequest, T>(string endpoint, TRequest body) 
    where TRequest : class where T : class

// POST con retry
public async Task<HttpResponseMessage> PostWithRetryAsync<TRequest>(
    string endpoint, TRequest body, int maxRetries = 3) where TRequest : class

// POST con retry e deserializzazione
public async Task<T> PostWithRetryAsync<TRequest, T>(
    string endpoint, TRequest body, int maxRetries = 3) 
    where TRequest : class where T : class
```

#### **PUT Requests**

```csharp
public async Task<HttpResponseMessage> PutAsync<TRequest>(string endpoint, TRequest body)
public async Task<T> PutAsync<TRequest, T>(string endpoint, TRequest body)
```

#### **DELETE Requests**

```csharp
public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
```

#### **Utility Statici**

```csharp
// Serializzare oggetto a JSON
public static string Serialize<T>(T obj) where T : class

// Deserializzare JSON a oggetto
public static T Deserialize<T>(string json) where T : class
```

---

### **ResponseValidator - Metodi di Validazione**

Tutti i metodi supportano il **pattern fluente** (return `this`):

```csharp
// Validare che la risposta sia di successo (2xx)
.AssertIsSuccess()

// Validare status code esatto
.AssertStatusCode(HttpStatusCode.OK)

// Validare uno tra più status code
.AssertStatusCodeIsOneOf(
    HttpStatusCode.OK, 
    HttpStatusCode.Created)

// Validare presenza di header
.AssertHasHeader("X-Request-Id")

// Validare valore di header
.AssertHeaderValue("Content-Type", "application/json")

// Validare Content-Type
.AssertContentType("application/json")

// Validare che sia JSON
.AssertIsJsonContent()

// Ottenere il valore di un header (per ulteriori verifiche)
.GetHeaderValue("X-Request-Id")

// Ottenere la risposta originale (per operazioni ulteriori)
.GetResponse()
```

---

## 💡 Pattern Consigliati

### **Pattern 1: GET semplice con validazione**

```csharp
[Test]
public async Task Should_Get_User_Successfully()
{
    var user = await _apiHelper.GetAsync<User>("/api/users/123");
    
    Assert.That(user.Id, Is.EqualTo("123"));
    Assert.That(user.Email, Is.Not.Empty);
}
```

### **Pattern 2: POST con corpo tipizzato**

```csharp
[Test]
public async Task Should_Create_User()
{
    var request = new UserCreateRequest
    {
        Email = "test@example.com",
        Name = "Test User"
    };

    var response = await _apiHelper.PostAsync<UserCreateRequest, UserResponse>(
        "/api/users",
        request);

    Assert.That(response.Id, Is.Not.Null);
}
```

### **Pattern 3: Validazione dettagliata di errore**

```csharp
[Test]
public async Task Should_Return_400_On_Invalid_Email()
{
    var response = await _apiHelper.PostAsync(
        "/api/users",
        new { email = "invalid-email" });

    response.Validate()
        .AssertStatusCode(HttpStatusCode.BadRequest)
        .AssertIsJsonContent();
}
```

### **Pattern 4: Retry automatico per operazioni flaky**

```csharp
[Test]
public async Task Should_Eventually_Get_User_With_Retry()
{
    var user = await _apiHelper.GetWithRetryAsync<User>(
        "/api/users/123",
        maxRetries: 5);

    Assert.That(user, Is.Not.Null);
}
```

### **Pattern 5: Catena fluente completa**

```csharp
[Test]
public async Task Should_Validate_Complete_Response()
{
    var response = await _apiHelper.PostAsync(
        "/api/users",
        new { email = "test@example.com" });

    var validator = response.Validate()
        .AssertStatusCode(HttpStatusCode.Created)
        .AssertIsJsonContent()
        .AssertHasHeader("Location");

    // Usa la risposta per ulteriori operazioni
    var content = await validator.GetResponse().Content.ReadAsStringAsync();
    Assert.That(content, Does.Contain("id"));
}
```

### **Pattern 6: Combinare con TestDataBuilder (per test Onboarding)**

```csharp
[Test]
public async Task Should_Register_Valid_User()
{
    // Usa il builder per creare dati consistenti
    var user = TestUserFactory.CreateValidUser();
    
    // Invia con ApiHelper
    var response = await _apiHelper.PostAsync<UserRequest, UserResponse>(
        "/onboarding/register",
        user);

    // Valida risposta
    response.Validate()
        .AssertStatusCode(HttpStatusCode.Created)
        .AssertIsJsonContent();

    Assert.That(response.UserId, Is.Not.Empty);
}
```

---

## 🔧 Configurazione

L'ApiHelper utilizza le impostazioni da `TestSettings`:

```json
{
  "Api": {
    "BaseUrl": "https://qa.api.example.com",
    "AcceptLanguage": "it-IT",
    "TimeoutSeconds": 30
  }
}
```

- **BaseUrl**: viene usato come base per tutti gli endpoint
- **AcceptLanguage**: header comune aggiunto in `OnboardingApiTestBase`
- **TimeoutSeconds**: timeout del client HTTP

---

## ❌ Errori Comuni e Soluzioni

### **Errore: "Endpoint non può essere vuoto"**
```csharp
❌ SBAGLIATO:
var response = await _apiHelper.GetAsync("");

✅ CORRETTO:
var response = await _apiHelper.GetAsync("/api/users");
```

### **Errore: "JSON non può essere vuoto"**
```csharp
❌ SBAGLIATO (risposta senza body):
var result = await _apiHelper.GetAsync<User>("/api/delete/123");

✅ CORRETTO (usa GetAsync senza deserializzazione):
var response = await _apiHelper.DeleteAsync("/api/delete/123");
response.Validate().AssertIsSuccess();
```

### **Errore: "Status code atteso... ricevuto..."**
```csharp
❌ SBAGLIATO (assumi success):
var user = await _apiHelper.GetAsync<User>("/api/invalid");

✅ CORRETTO (valida lo status):
var response = await _apiHelper.GetAsync("/api/invalid");
response.Validate().AssertStatusCode(HttpStatusCode.NotFound);
```

---

## 📊 Esempio Completo: Test Onboarding

```csharp
[TestFixture]
public class Onboarding_Complete_Example : OnboardingApiTestBase
{
    private ApiHelper _apiHelper = null!;

    protected override void AfterOnboardingSetUp()
    {
        _apiHelper = new ApiHelper(HttpClient, Settings);
    }

    [Test]
    public async Task Should_Register_User_And_Verify_Data()
    {
        // 1. Prepara dati
        var request = new OnboardingRequest
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            DeviceId = DeviceId,
            AcceptLanguage = AcceptLanguage
        };

        // 2. Esegui richiesta
        var response = await _apiHelper.PostAsync<OnboardingRequest, OnboardingResponse>(
            "/onboarding/register",
            request);

        // 3. Valida risposta
        response.Validate()
            .AssertIsJsonContent()
            .AssertHasHeader("X-Request-Id");

        // 4. Verifica dati
        Assert.That(response.UserId, Is.Not.Empty);
        Assert.That(response.RegistrationDate, Is.LessThanOrEqualTo(DateTime.UtcNow));

        // 5. Persisti nello state per test successivi
        await StateStore.UpdateAsync(s => 
            s.Onboarding.UserId = response.UserId);
    }

    [Test]
    public async Task Should_Return_400_On_Invalid_Email()
    {
        var response = await _apiHelper.PostAsync(
            "/onboarding/register",
            new { email = "not-an-email", deviceId = DeviceId });

        response.Validate()
            .AssertStatusCode(HttpStatusCode.BadRequest)
            .AssertIsJsonContent();
    }
}
```

---

## 🚦 Best Practices

✅ **DO:**
- Usa `ApiHelper` per tutte le operazioni HTTP
- Valida sempre le risposte con `ResponseValidator`
- Sfrutta il pattern fluente per readabilità
- Usa retry per operazioni potenzialmente flaky
- Centralizza la creazione di dati di test

❌ **DON'T:**
- Non usare direttamente `HttpClient` nei test (usa `ApiHelper`)
- Non ignorare i validation errors (cattura le exception)
- Non hard-coded gli endpoint (usa costanti o config)
- Non mescolare logica di validazione con logica di test

---

## 📞 Supporto

Per dubbi sull'utilizzo dell'ApiHelper, consulta:
1. File di esempio: `Features/Onboarding/Examples/ApiHelper_UsageExamples.cs`
2. Test esistenti nel progetto
3. Commenti XML in `ApiHelper.cs` e `ResponseValidator.cs`

---

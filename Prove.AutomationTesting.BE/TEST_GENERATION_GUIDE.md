# Guida alla Generazione Automatica di Test API - Prompt per LLM Agentico

## Introduzione

Questo documento definisce il framework per generare test API in **Prove.AutomationTesting.BE**.

**Input per LLM:**
1. Questo documento come system prompt
2. Definizione feature (nome, endpoint base)
3. Array di test case in formato YAML

**Output atteso:** Test script C# pronti all'uso (.NET 8, NUnit 4.x)

---

## Struttura Progetto

```
Features/{FeatureName}/
├── {FeatureName}ApiTestBase.cs        # Base class feature-specific
├── {FeatureName}_200_Tests.cs         # Test successo (2xx)
├── {FeatureName}_400_Tests.cs         # Test validazione (4xx)
├── {FeatureName}_401_Tests.cs         # Test autenticazione
├── {FeatureName}_403_Tests.cs         # Test autorizzazione
└── Examples/
    └── {FeatureName}_UsageExamples.cs

Infrastructure/
├── ApiTestBase.cs              # Base per tutti test
├── Utilities/
│   ├── ApiHelper.cs            # HTTP operations
│   └── ResponseValidator.cs    # Validazione risposte
└── State/
    └── JsonFileStateStore.cs   # Persistenza stato
```

---

## Pattern Test Base Class

```csharp
namespace ApiTests.Features.{FeatureName};

public abstract class {FeatureName}ApiTestBase : ApiTestBase
{
    protected string DefaultResourceId => Settings.{FeatureName}.DefaultResourceId;

    protected override void AfterApiSetUp()
    {
        HttpClient.DefaultRequestHeaders.Add("X-Resource-Id", DefaultResourceId);
    }
}
```

## Pattern Test Suite

```csharp
namespace ApiTests.Features.{FeatureName};

[TestFixture]
public class {FeatureName}_{StatusCode}_Tests : {FeatureName}ApiTestBase
{
    [Test]
    [Description("{TestDescription}")]
    public async Task {TestMethodName}()
    {
        // Arrange
        var apiHelper = new ApiHelper(HttpClient, Settings);
        var request = new { /* payload */ };

        // Act
        var response = await apiHelper.{Method}(
            "{Endpoint}",
            request
        );

        // Assert
        new ResponseValidator(response)
            .AssertStatusCode(HttpStatusCode.{Status})
            .AssertContentType("application/json");
    }
}
```

---

## Mapping Test Case → Test Script

### Formato Test Case YAML

```yaml
feature: Onboarding
testName: Should_Return_200_With_Valid_Request
endpoint: /api/onboarding/start
method: POST
statusCode: 200
description: "Verifica accettazione richiesta onboarding valida"
request:
  deviceId: device-001
  email: user@example.com
  language: it-IT
expectedResponse:
  sessionId: string
  status: ACTIVE
  expiresAt: ISO8601
assertions:
  - StatusCode equals 200
  - ContentType equals application/json
  - Response.SessionId is not null
  - Response.Status equals ACTIVE
stateManagement: SAVE_SESSION_ID
```

### Mapping 1:1: YAML → C#

| YAML | C# | Mapping |
|------|-----|---------|
| `feature: Onboarding` | Class name: `Onboarding_200_Tests` | Nome file + status code |
| `testName: Should_Return_200_With_Valid_Request` | `public async Task Should_Return_200_With_Valid_Request()` | Nome metodo diretto |
| `endpoint: /api/onboarding/start` | `apiHelper.PostAsync("/api/onboarding/start", ...)` | Endpoint nel metodo |
| `method: POST` | `apiHelper.PostAsync<T, T>()` | Determina ApiHelper method |
| `statusCode: 200` | `HttpStatusCode.OK` | Validazione nel test |
| `request: {...}` | `var request = new { deviceId = ... }` | Arrange section |
| `expectedResponse: {...}` | `Assert.That(response.sessionId, ...)` | Assert section |
| `assertions: [...]` | `.AssertStatusCode(...).AssertContentType(...)` | ResponseValidator + NUnit Assert |
| `stateManagement: SAVE_SESSION_ID` | `RunState.Onboarding.SessionId = ...` | State handling |

### Esempio: Trasformazione Completa

**Input YAML:**
```yaml
feature: Onboarding
testName: Should_Return_401_With_Expired_Token
endpoint: /api/onboarding/start
method: POST
statusCode: 401
description: "Valida rifiuto con token scaduto"
request: null
assertions:
  - StatusCode equals 401
stateManagement: NONE
```

**Output C#:**
```csharp
[Test]
[Description("Valida rifiuto con token scaduto")]
public async Task Should_Return_401_With_Expired_Token()
{
    // Arrange
    var request = new HttpRequestMessage(HttpMethod.Post, "/api/onboarding/start")
    {
        Headers = 
        {
            Authorization = new AuthenticationHeaderValue(
                "Bearer", 
                Settings.TestCases.Auth.ExpiredToken
            )
        }
    };

    // Act
    var response = await HttpClient.SendAsync(request);

    // Assert
    new ResponseValidator(response)
        .AssertStatusCode(HttpStatusCode.Unauthorized);
}
```

---

## Pattern Comuni

### GET (Lettura)
```csharp
var response = await apiHelper.GetAsync<ResponseDto>("/api/resource/{id}");
new ResponseValidator(response).AssertStatusCode(HttpStatusCode.OK);
Assert.That(response.Id, Is.EqualTo(expectedId));
```

### POST (Creazione)
```csharp
var response = await apiHelper.PostAsync<CreateDto, ResponseDto>("/api/resource", request);
new ResponseValidator(response).AssertStatusCode(HttpStatusCode.Created);
```

### PUT (Aggiornamento)
```csharp
var response = await apiHelper.PutAsync<UpdateDto, ResponseDto>("/api/resource/{id}", request);
new ResponseValidator(response).AssertStatusCode(HttpStatusCode.OK);
```

### DELETE (Cancellazione)
```csharp
var response = await apiHelper.DeleteAsync("/api/resource/{id}");
new ResponseValidator(response).AssertStatusCode(HttpStatusCode.NoContent);
```

### Autenticazione (401)
```csharp
var request = new HttpRequestMessage(HttpMethod.Post, "/api/resource")
{
    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", Settings.TestCases.Auth.ExpiredToken) }
};
var response = await HttpClient.SendAsync(request);
new ResponseValidator(response).AssertStatusCode(HttpStatusCode.Unauthorized);
```

### Validazione (400)
```csharp
var response = await apiHelper.PostAsync<RequestDto>("/api/resource", invalidRequest);
new ResponseValidator(response).AssertStatusCode(HttpStatusCode.BadRequest);
var errorContent = await response.Content.ReadAsStringAsync();
Assert.That(errorContent, Does.Contain("error_field").IgnoreCase);
```

### State Management
```csharp
// Carica
await StateStore.LoadAsync();
var resourceId = RunState.{Feature}.ResourceId ?? throw new InvalidOperationException("Missing state");

// Salva
RunState.{Feature}.ResourceId = response.Id;
await StateStore.SaveAsync(RunState);
```

---

## Definizione Test Case YAML

```yaml
feature: string                    # Nome della feature (Onboarding, Payment, etc)
testName: string                   # Nome metodo test (Should_Return_200_...)
endpoint: string                   # Path API (/api/feature/resource)
method: string                     # HTTP method (GET, POST, PUT, DELETE)
statusCode: int                    # Status code atteso (200, 400, 401, etc)
description: string                # Descrizione scenario
request: object                    # Body richiesta (null per GET/DELETE)
expectedResponse: object           # Struttura risposta attesa
assertions:                        # Liste asserzioni
  - string                         # Es: "StatusCode equals 200"
preconditions: []                  # Setup prerequisites (vuoto per default)
stateManagement: string            # NONE, LOAD, SAVE_*, LOAD_AND_SAVE_*
```

---

## Test Cases Examples

### 1. Success Case (200)

```yaml
feature: Onboarding
testName: Should_Return_200_With_Valid_Request
endpoint: /api/onboarding/start
method: POST
statusCode: 200
description: "Accetta richiesta onboarding valida"
request:
  deviceId: device-001
  email: user@example.com
  acceptedTerms: true
expectedResponse:
  sessionId: string
  status: PENDING_VERIFICATION
  createdAt: ISO8601
  expiresAt: ISO8601
assertions:
  - StatusCode equals 200
  - ContentType equals application/json
  - Response.SessionId is not null and not empty
  - Response.Status equals PENDING_VERIFICATION
  - Response.ExpiresAt is greater than CreatedAt
preconditions: []
stateManagement: SAVE_SESSION_ID
```

### 2. Created Case (201)

```yaml
feature: Payment
testName: Should_Return_201_When_Payment_Created
endpoint: /api/payment/transactions
method: POST
statusCode: 201
description: "Crea una nuova transazione di pagamento"
request:
  amount: 99.99
  currency: EUR
  description: "Acquisto prodotto"
expectedResponse:
  id: string
  status: PENDING
  createdAt: ISO8601
assertions:
  - StatusCode equals 201
  - Response.Id is not null
  - Response.Status equals PENDING
preconditions: []
stateManagement: SAVE_PAYMENT_ID
```

### 3. No Content Case (204)

```yaml
feature: Shipping
testName: Should_Return_204_On_Delete
endpoint: /api/shipping/orders/{orderId}
method: DELETE
statusCode: 204
description: "Cancella un ordine di spedizione"
request: null
expectedResponse: null
assertions:
  - StatusCode equals 204
preconditions:
  - orderId exists from previous test
stateManagement: LOAD_ORDER_ID
```

### 4. Bad Request Case (400)

```yaml
feature: Onboarding
testName: Should_Return_400_With_Invalid_Email
endpoint: /api/onboarding/start
method: POST
statusCode: 400
description: "Rifiuta email non valida"
request:
  deviceId: device-001
  email: invalid-email-format
  acceptedTerms: true
expectedResponse:
  error: string
  message: string
assertions:
  - StatusCode equals 400
  - ContentType equals application/json
  - Response.Error contains email
  - Response.Message is not null
preconditions: []
stateManagement: NONE
```

### 5. Unauthorized Case (401)

```yaml
feature: Onboarding
testName: Should_Return_401_With_Expired_Token
endpoint: /api/onboarding/verify
method: POST
statusCode: 401
description: "Rifiuta token scaduto"
request: null
expectedResponse:
  error: string
assertions:
  - StatusCode equals 401
preconditions: []
stateManagement: NONE
auth_token: ExpiredToken
```

### 6. Forbidden Case (403)

```yaml
feature: Payment
testName: Should_Return_403_When_User_Not_Admin
endpoint: /api/payment/admin/settings
method: PUT
statusCode: 403
description: "Nega accesso non-admin alle impostazioni admin"
request:
  setting: daily_limit
  value: 5000
expectedResponse:
  error: string
assertions:
  - StatusCode equals 403
  - Response.Error contains permission
preconditions: []
stateManagement: NONE
auth_token: RestrictedUserToken
```

### 7. Validation Error Case (422)

```yaml
feature: Payment
testName: Should_Return_422_With_Insufficient_Funds
endpoint: /api/payment/charge
method: POST
statusCode: 422
description: "Rifiuta pagamento per fondi insufficienti"
request:
  accountId: acc-123
  amount: 10000
  currency: EUR
expectedResponse:
  error: INSUFFICIENT_FUNDS
  availableBalance: number
assertions:
  - StatusCode equals 422
  - Response.Error equals INSUFFICIENT_FUNDS
  - Response.AvailableBalance is less than requested amount
preconditions: []
stateManagement: NONE
```

### 8. Not Found Case (404)

```yaml
feature: Shipping
testName: Should_Return_404_When_Order_Not_Found
endpoint: /api/shipping/orders/nonexistent-id
method: GET
statusCode: 404
description: "Restituisce 404 per ordine non trovato"
request: null
expectedResponse:
  error: string
assertions:
  - StatusCode equals 404
  - Response.Error contains not found
preconditions: []
stateManagement: NONE
```

### 9. Conflict Case (409)

```yaml
feature: Onboarding
testName: Should_Return_409_When_Device_Already_Onboarded
endpoint: /api/onboarding/start
method: POST
statusCode: 409
description: "Rifiuta onboarding se device già registrato"
request:
  deviceId: device-already-registered
  email: user@example.com
  acceptedTerms: true
expectedResponse:
  error: CONFLICT
  message: string
assertions:
  - StatusCode equals 409
  - Response.Error equals CONFLICT
preconditions: []
stateManagement: NONE
```

### 10. Server Error Case (500)

```yaml
feature: Payment
testName: Should_Return_500_On_Internal_Error
endpoint: /api/payment/charge
method: POST
statusCode: 500
description: "Gestisce errore interno del server"
request:
  amount: 99.99
  currency: EUR
expectedResponse:
  error: string
  requestId: string
assertions:
  - StatusCode equals 500
  - Response.RequestId is not null
preconditions: []
stateManagement: NONE
```

---

## File YAML Completo (Più Test Cases)

```yaml
# Feature: Onboarding
# Contiene test per diversi scenari

testCases:
  - feature: Onboarding
    testName: Should_Return_200_With_Valid_Request
    endpoint: /api/onboarding/start
    method: POST
    statusCode: 200
    description: "Accetta richiesta onboarding valida"
    request:
      deviceId: device-001
      email: user@example.com
      acceptedTerms: true
    expectedResponse:
      sessionId: string
      status: PENDING_VERIFICATION
    assertions:
      - StatusCode equals 200
      - Response.SessionId is not null
    stateManagement: SAVE_SESSION_ID

  - feature: Onboarding
    testName: Should_Return_400_With_Invalid_Email
    endpoint: /api/onboarding/start
    method: POST
    statusCode: 400
    description: "Rifiuta email non valida"
    request:
      deviceId: device-001
      email: invalid-email
      acceptedTerms: true
    expectedResponse:
      error: string
    assertions:
      - StatusCode equals 400
      - Response.Error contains email
    stateManagement: NONE

  - feature: Onboarding
    testName: Should_Return_401_With_Expired_Token
    endpoint: /api/onboarding/verify
    method: POST
    statusCode: 401
    description: "Rifiuta token scaduto"
    request: null
    expectedResponse:
      error: string
    assertions:
      - StatusCode equals 401
    stateManagement: NONE
    auth_token: ExpiredToken

  - feature: Onboarding
    testName: Should_Return_200_Verify_Session
    endpoint: /api/onboarding/verify
    method: POST
    statusCode: 200
    description: "Verifica sessione onboarding"
    request:
      sessionId: string
      code: "123456"
    expectedResponse:
      userId: string
      token: string
    assertions:
      - StatusCode equals 200
      - Response.UserId is not null
      - Response.Token is not null
    preconditions:
      - sessionId from previous test
    stateManagement: LOAD_AND_SAVE_USER_ID
```

---

## Input/Output Esempio

**Input YAML (Onboarding):**
```yaml
feature: Onboarding
testName: Should_Return_200_On_Valid_Request
endpoint: /api/onboarding/start
method: POST
statusCode: 200
description: "Accetta richiesta onboarding valida"
request:
  deviceId: device-001
  email: user@example.com
  acceptedTerms: true
expectedResponse:
  sessionId: string
  status: PENDING_VERIFICATION
assertions:
  - StatusCode equals 200
  - Response.SessionId is not null
stateManagement: SAVE_SESSION_ID
```

**Output C# Atteso:**
```csharp
[Test]
[Description("Accetta richiesta onboarding valida")]
public async Task Should_Return_200_On_Valid_Request()
{
    var apiHelper = new ApiHelper(HttpClient, Settings);
    var validRequest = new
    {
        deviceId = "device-001",
        email = "user@example.com",
        acceptedTerms = true
    };

    var response = await apiHelper.PostAsync<dynamic, dynamic>(
        "/api/onboarding/start",
        validRequest
    );

    new ResponseValidator(response)
        .AssertStatusCode(HttpStatusCode.OK);

    var content = await response.Content.ReadAsAsync<dynamic>();
    Assert.That(content.sessionId, Is.Not.Null.And.Not.Empty);

    RunState.Onboarding.SessionId = content.sessionId;
    await StateStore.SaveAsync(RunState);
}
```

---

## Istruzioni per LLM

**Input:**
1. Questo documento come system prompt
2. Feature name (es: Onboarding)
3. Array di test case YAML

**Output per ogni test case:**
1. File: `{FeatureName}_{StatusCode}_Tests.cs`
2. Metodo test seguendo pattern Arrange/Act/Assert
3. Se non esiste: classe base `{FeatureName}ApiTestBase.cs`

**Qualità richiesta:**
- Compila senza errori (.NET 8, C# 12, NUnit 4.x)
- Async/await corretto
- Nomi seguono convenzioni
- Valida tutti gli assertion YAML
- State management implementato
- Pattern Arrange/Act/Assert

**Convenzioni C#:**
- PascalCase classi/metodi, camelCase variabili
- `$"..."` per string interpolation
- `??`, `?.`, `is not null` per null handling
- 4 spazi indentazione, no tabs

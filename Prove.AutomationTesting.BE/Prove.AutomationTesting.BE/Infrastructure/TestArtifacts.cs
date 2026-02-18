using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ApiTests.Infrastructure;

/// <summary>
/// Utility per persistere artefatti dei test (request/response, payload, ecc.)
/// - In locale: li trovi nel WorkDirectory del runner
/// - In DevOps: vengono allegati ai risultati (se il runner li supporta) e/o pubblicati come artifact
///
/// Consiglio: allega SOLO su failure per non generare tonnellate di file.
/// </summary>
public static class TestArtifacts
{
    public static void AttachTextOnFailure(string fileName, string content)
    {
        var status = TestContext.CurrentContext.Result.Outcome.Status;
        if (status != NUnit.Framework.Interfaces.TestStatus.Failed)
            return;

        var dir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Artifacts");
        Directory.CreateDirectory(dir);

        var safe = string.Concat(fileName.Split(Path.GetInvalidFileNameChars()));
        var path = Path.Combine(dir, $"{safe}.txt");

        File.WriteAllText(path, content, Encoding.UTF8);

        // Allegato al test result (quando supportato)
        TestContext.AddTestAttachment(path);
    }

    public static async Task AttachHttpOnFailureAsync(
        string name,
        HttpRequestMessage request,
        HttpResponseMessage? response)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== REQUEST ===");
        sb.AppendLine($"{request.Method} {request.RequestUri}");
        foreach (var h in request.Headers)
            sb.AppendLine($"{h.Key}: {string.Join(",", h.Value)}");

        if (request.Content != null)
        {
            sb.AppendLine();
            sb.AppendLine("--- Request Body ---");
            sb.AppendLine(await request.Content.ReadAsStringAsync());
        }

        sb.AppendLine();
        sb.AppendLine("=== RESPONSE ===");

        if (response == null)
        {
            sb.AppendLine("Response: <null>");
            AttachTextOnFailure(name, sb.ToString());
            return;
        }

        sb.AppendLine($"Status: {(int)response.StatusCode} ({response.StatusCode})");
        foreach (var h in response.Headers)
            sb.AppendLine($"{h.Key}: {string.Join(",", h.Value)}");

        if (response.Content != null)
        {
            sb.AppendLine();
            sb.AppendLine("--- Response Body ---");
            sb.AppendLine(await response.Content.ReadAsStringAsync());
        }

        AttachTextOnFailure(name, sb.ToString());
    }
}

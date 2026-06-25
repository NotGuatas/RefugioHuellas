using Amazon;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using System.Text;
using System.Text.Json;

namespace RefugioHuellas.Services;

public class EscaladaECService
{
    private readonly HttpClient _httpClient;
    private readonly IAmazonKeyManagementService _kmsClient;
    private readonly string _kmsKeyId;
    private const string EscaladaECUrl = "https://escalad-ec.vercel.app";

    public EscaladaECService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _kmsKeyId = config["AWS_KMS_KEY_ID"]!;

        _kmsClient = new AmazonKeyManagementServiceClient(
            config["AWS_ACCESS_KEY_ID"],
            config["AWS_SECRET_ACCESS_KEY"],
            RegionEndpoint.GetBySystemName(config["AWS_REGION"] ?? "us-east-2")
        );
    }

    public async Task<string> EnviarDatosCifradosAsync(object datos, string accessToken)
    {
        var json = JsonSerializer.Serialize(datos);
        var plainBytes = Encoding.UTF8.GetBytes(json);

        var encryptResult = await _kmsClient.EncryptAsync(new EncryptRequest
        {
            KeyId = _kmsKeyId,
            Plaintext = new MemoryStream(plainBytes)
        });

        var encryptedBase64 = Convert.ToBase64String(encryptResult.CiphertextBlob.ToArray());

        var content = new StringContent(
            JsonSerializer.Serialize(new { data = encryptedBase64 }),
            Encoding.UTF8,
            "application/json"
        );

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{EscaladaECUrl}/api/refugio")
        {
            Content = content,
            Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken) }
        };

        var response = await _httpClient.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }
}

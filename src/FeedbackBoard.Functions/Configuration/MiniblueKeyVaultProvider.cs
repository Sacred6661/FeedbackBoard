using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace FeedbackBoard.Functions.Configuration;

// this is for local dev azure

public class MiniblueKeyVaultProvider : ConfigurationProvider
{
    private readonly string _baseUrl;
    private readonly string _vaultName;
    private readonly HttpClient _httpClient;

    public MiniblueKeyVaultProvider(string baseUrl, string vaultName)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _vaultName = vaultName;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    public override void Load()
    {
        if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_vaultName))
            return;

        try
        {
            // not the best solution, maybe should be refactored later
            LoadSecretsAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not load from miniblue Key Vault: {ex.Message}");
        }
    }

    private async Task LoadSecretsAsync()
    {
        var listUrl = $"{_baseUrl}/keyvault/{_vaultName}/secrets?api-version=7.4";
        var listResponse = await _httpClient.GetAsync(listUrl);

        if (!listResponse.IsSuccessStatusCode) return;

        var listJson = await listResponse.Content.ReadAsStringAsync();
        var secretsList = JsonSerializer.Deserialize<MiniblueSecretsList>(listJson);

        if (secretsList?.Value == null) return;

        Console.WriteLine($"Loading {secretsList.Value.Count} secrets from Key Vault '{_vaultName}'");

        foreach (var secretRef in secretsList.Value)
        {
            var secretName = ExtractSecretName(secretRef.Id);
            if (string.IsNullOrEmpty(secretName)) continue;

            var secretUrl = $"{_baseUrl}/keyvault/{_vaultName}/secrets/{secretName}?api-version=7.4";
            var secretResponse = await _httpClient.GetAsync(secretUrl);
            if (!secretResponse.IsSuccessStatusCode) continue;

            var secretJson = await secretResponse.Content.ReadAsStringAsync();
            var secret = JsonSerializer.Deserialize<MiniblueSecret>(secretJson);

            if (!string.IsNullOrEmpty(secret?.Value))
            {
                var configKey = secretName.Replace("--", ":");
                Data[configKey] = secret.Value;
            }
        }
    }

    private static string? ExtractSecretName(string? id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return id.Split('/').LastOrDefault();
    }

    private class MiniblueSecretsList
    {
        [System.Text.Json.Serialization.JsonPropertyName("value")]
        public List<MiniblueSecretRef>? Value { get; set; }
    }

    private class MiniblueSecretRef
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    private class MiniblueSecret
    {
        [System.Text.Json.Serialization.JsonPropertyName("value")]
        public string? Value { get; set; }
    }
}

public class MiniblueKeyVaultConfigurationSource : IConfigurationSource
{
    private readonly string _baseUrl;
    private readonly string _vaultName;

    public MiniblueKeyVaultConfigurationSource(string baseUrl, string vaultName)
    {
        _baseUrl = baseUrl;
        _vaultName = vaultName;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new MiniblueKeyVaultProvider(_baseUrl, _vaultName);
}

public static class MiniblueKeyVaultExtensions
{
    public static IConfigurationBuilder AddMiniblueKeyVault(
        this IConfigurationBuilder builder,
        string baseUrl,
        string vaultName)
        => builder.Add(new MiniblueKeyVaultConfigurationSource(baseUrl, vaultName));
}
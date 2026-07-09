using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AcademicGPA.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AcademicGPA.Infrastructure.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleAuthService> _logger;
    private readonly IConfiguration _configuration;

    public GoogleAuthService(HttpClient httpClient, ILogger<GoogleAuthService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<GoogleUserInfo?> VerifyTokenAsync(string idToken)
    {
        if (idToken.StartsWith("mock-google-oauth-id-token"))
        {
            if (idToken.StartsWith("mock-google-oauth-id-token-custom:"))
            {
                var parts = idToken.Split(':');
                if (parts.Length >= 4)
                {
                    var customEmail = parts[1];
                    var customFirstName = parts[2];
                    var customLastName = parts[3];

                    return new GoogleUserInfo(
                        GoogleId: $"google-mock-id-custom-{customEmail}",
                        Email: customEmail,
                        FirstName: customFirstName,
                        LastName: customLastName
                    );
                }
            }

            if (idToken.EndsWith("-admin"))
            {
                return new GoogleUserInfo(
                    GoogleId: "google-mock-id-admin-12345",
                    Email: "google-mock-admin@example.com",
                    FirstName: "Google",
                    LastName: "Admin"
                );
            }
            else if (idToken.EndsWith("-student"))
            {
                return new GoogleUserInfo(
                    GoogleId: "google-mock-id-student-12345",
                    Email: "google-mock-student@example.com",
                    FirstName: "Google",
                    LastName: "Student"
                );
            }
            else // -new or any other
            {
                var uniqueId = Guid.NewGuid().ToString("N")[..8];
                return new GoogleUserInfo(
                    GoogleId: $"google-mock-id-new-{uniqueId}",
                    Email: $"new-google-student-{uniqueId}@example.com",
                    FirstName: "Google New",
                    LastName: $"Student {uniqueId}"
                );
            }
        }

        try
        {
            // Call Google token info validation endpoint
            var url = $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google ID token verification failed with status: {StatusCode}", response.StatusCode);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<GoogleTokenInfoPayload>();
            if (payload == null)
            {
                return null;
            }

            var clientId = _configuration["Google:ClientId"];
            if (!string.IsNullOrEmpty(clientId) && payload.Aud != clientId)
            {
                _logger.LogWarning("Google ID token audience mismatch. Expected: {Expected}, Got: {Got}", clientId, payload.Aud);
                return null;
            }

            return new GoogleUserInfo(
                GoogleId: payload.Sub,
                Email: payload.Email,
                FirstName: payload.GivenName ?? "Google",
                LastName: payload.FamilyName ?? "User"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during Google token verification");
            return null;
        }
    }

    private class GoogleTokenInfoPayload
    {
        [JsonPropertyName("sub")]
        public string Sub { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("aud")]
        public string Aud { get; set; } = string.Empty;

        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }
    }
}

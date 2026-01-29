using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BookManager.ViewModels.Models;

namespace BookManager.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        private readonly UserVM _currentUser;

        public ApiService(HttpClient httpClient, UserVM currentUser)
        {
            _httpClient = httpClient;
            _currentUser = currentUser;
        }

        public async Task<HttpResponseMessage> SignUpAsync(MultipartFormDataContent content)
        {
            return await _httpClient.PostAsync("/api/users/signup", content);
        }

        public async Task<HttpResponseMessage> EmailLoginAsync(string email, string password)
        {
            var payload = new { Email = email, Password = password };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/users/email_login", content);
            if (!response.IsSuccessStatusCode) return response;

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            var token = root.GetProperty("token").GetString();
            await SecureStorage.SetAsync("jwt_token", token);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var userElem = root.GetProperty("user");
            _currentUser.PublicUser.Id = userElem.GetProperty("id").GetInt32();
            _currentUser.EmailAddress = userElem.GetProperty("emailAddress").GetString();
            _currentUser.PublicUser.Username = userElem.GetProperty("username").GetString();
            _currentUser.PublicUser.ProfilePicture = userElem.GetProperty("profilePicture").GetString();
            _currentUser.CreatedAt = userElem.GetProperty("createdAt").GetDateTime();

            if (userElem.TryGetProperty("role", out var roleElem) && roleElem.ValueKind == JsonValueKind.Number)
            {
                _currentUser.Role = roleElem.GetInt32() == 1 ? UserRole.Admin : UserRole.User;
            }
            else
            {
                _currentUser.Role = UserRole.User;
            }

            if (userElem.TryGetProperty("currentRestriction", out var restrictionElem) && restrictionElem.ValueKind != JsonValueKind.Null)
            {
                _currentUser.PublicUser.CurrentRestriction = new RestrictionVM
                {
                    Id = restrictionElem.GetProperty("id").GetInt32(),
                    StartDate = restrictionElem.GetProperty("startDate").GetDateTime(),
                    EndDate = restrictionElem.TryGetProperty("endDate", out var end) && end.ValueKind != JsonValueKind.Null
                        ? end.GetDateTime()
                        : null,
                    Reason = restrictionElem.GetProperty("reason").GetString(),
                    User = _currentUser.PublicUser
                };
            }

            return response;
        }

        public async Task<bool> TokenLoginAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("jwt_token");
                if (string.IsNullOrWhiteSpace(token))
                    return false;

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("/api/users/me");
                if (!response.IsSuccessStatusCode)
                {
                    SecureStorage.Remove("jwt_token");
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var userElem = doc.RootElement.GetProperty("user");

                _currentUser.PublicUser.Id = userElem.GetProperty("id").GetInt32();
                _currentUser.EmailAddress = userElem.GetProperty("emailAddress").GetString();
                _currentUser.PublicUser.Username = userElem.GetProperty("username").GetString();
                _currentUser.PublicUser.ProfilePicture = userElem.GetProperty("profilePicture").GetString();
                _currentUser.CreatedAt = userElem.GetProperty("createdAt").GetDateTime();

                if (userElem.TryGetProperty("role", out var roleElem) && roleElem.ValueKind == JsonValueKind.Number)
                {
                    _currentUser.Role = roleElem.GetInt32() == 1 ? UserRole.Admin : UserRole.User;
                }
                else
                {
                    _currentUser.Role = UserRole.User;
                }

                if (userElem.TryGetProperty("currentRestriction", out var restrictionElem) && restrictionElem.ValueKind != JsonValueKind.Null)
                {
                    _currentUser.PublicUser.CurrentRestriction = new RestrictionVM
                    {
                        Id = restrictionElem.GetProperty("id").GetInt32(),
                        StartDate = restrictionElem.GetProperty("startDate").GetDateTime(),
                        EndDate = restrictionElem.TryGetProperty("endDate", out var end) && end.ValueKind != JsonValueKind.Null
                            ? end.GetDateTime()
                            : null,
                        Reason = restrictionElem.GetProperty("reason").GetString(),
                        User = _currentUser.PublicUser
                    };
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenLogin failed: {ex}");
                return false;
            }
        }
    }

}

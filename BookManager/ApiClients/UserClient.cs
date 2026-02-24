using BookManager.Authentication;
using BookManager.ViewModels.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings;
using System.Text.Json;

namespace BookManager.ApiClients
{
    public class UserClient
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenStore _tokenStore;

        private readonly UserVM _user;

        public UserClient(HttpClient httpClient, UserVM currentUser, ITokenStore tokenStore)
        {
            _httpClient = httpClient;
            _user = currentUser;
            _tokenStore = tokenStore;
        }

        public async Task<AuthResult> SignUpAsync(string username, string email, string password, string imagePath)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(username), "Username");
            content.Add(new StringContent(email), "EmailAddress");
            content.Add(new StringContent(password), "Password");

            var stream = File.OpenRead(imagePath);
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            content.Add(streamContent, "ProfilePicture", Path.GetFileName(imagePath));

            var response = await _httpClient.PostAsync("/api/users/signup", content);

            if (!response.IsSuccessStatusCode)
            {
                return new AuthResult(false, await ApiErrorParser.ParseAsync(response));
            }

            return new AuthResult(true, null);
        }

        public async Task<AuthResult> EmailLoginAsync(string email, string password)
        {
            var payload = new { Email = email, Password = password };
            var json = JsonSerializer.Serialize(payload);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/users/email_login", content);

            if (!response.IsSuccessStatusCode)
            {
                return new AuthResult(false, await ApiErrorParser.ParseAsync(response));
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            var token = root.GetProperty("token").GetString();

            _user.FromJson(root.GetProperty("user"));

            await _tokenStore.SetAccessTokenAsync(token);

            return new AuthResult(true, null);
        }

        public async Task<AuthResult> TokenLoginAsync()
        {
            var token = await _tokenStore.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
                return new AuthResult(false, "No token stored");

            var response = await _httpClient.GetAsync("/api/users/me");

            if (!response.IsSuccessStatusCode)
            {
                _tokenStore.Clear();
                return new AuthResult(false, await ApiErrorParser.ParseAsync(response));
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            _user.FromJson(doc.RootElement.GetProperty("user"));

            return new AuthResult(true, null);
        }
    
    }
}

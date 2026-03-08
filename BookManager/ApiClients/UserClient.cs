using BookManager.Authentication;
using BookManager.Models.User;
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
        public event Func<Task>? OnLogout;

        public UserClient(HttpClient httpClient, UserVM currentUser, ITokenStore tokenStore)
        {
            _httpClient = httpClient;
            _user = currentUser;
            _tokenStore = tokenStore;
        }
        public async Task Logout()
        {
            _user.EmailAddress = null;
            _user.CreatedAt = default(DateTime);
            _user.PublicUser.Id = 0;
            _user.PublicUser.ProfilePicture = null;
            _user.PublicUser.ProfilePictureSource = null;
            _user.PublicUser.Username = null; 
            _user.Role = UserRole.User;

            _user.Restriction = new RestrictionVM();

            _tokenStore?.Clear();
            if (OnLogout != null)
            {
                foreach (var handler in OnLogout.GetInvocationList())
                {
                    if (handler is Func<Task> asyncHandler)
                    {
                        try
                        {
                            await asyncHandler();
                        }
                        catch (Exception ex)
                        {
                            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
                        }
                    }
                }
            }
        }

        public async Task<RequestResult> SignUpAsync(string username, string email, string password, string imagePath)
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
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));
            }

            return new RequestResult(true, null);
        }

        public async Task<RequestResult> EmailLoginAsync(string email, string password)
        {
            var payload = new { Email = email, Password = password };
            var json = JsonSerializer.Serialize(payload);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/users/email_login", content);

            if (!response.IsSuccessStatusCode)
            {
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response, false));
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            var token = root.GetProperty("token").GetString();

            _user.FromJson(root.GetProperty("user"));

            await _tokenStore.SetAccessTokenAsync(token);

            return new RequestResult(true, null);
        }

        public async Task<RequestResult> TokenLoginAsync()
        {
            var token = await _tokenStore.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
                return new RequestResult(false, "No token stored");

            var response = await _httpClient.GetAsync("/api/users/me");

            if (!response.IsSuccessStatusCode)
            {
                _tokenStore.Clear();
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response, false));
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            _user.FromJson(doc.RootElement.GetProperty("user"));

            return new RequestResult(true, null);
        }

        public async Task<RequestResult> UpdateUsernameEmailAsync(string username, string emailAddress)
        {
            var payload = new
            {
                Username = username,
                EmailAddress = emailAddress
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync("/api/users/me", content);

            if (!response.IsSuccessStatusCode)
            {
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));
            }

            if (!string.IsNullOrWhiteSpace(username))
            {
                _user.PublicUser.Username = username;
            }

            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                _user.EmailAddress = emailAddress;
            }

            return new RequestResult(true, null);
        }

        public async Task<RequestResult> UpdatePasswordAsync(string currentPassword, string newPassword)
        {
            var payload = new
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync("/api/users/me/password", content);

            if (!response.IsSuccessStatusCode)
            {
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));
            }

            return new RequestResult(true, null);
        }

        public async Task<RequestResult> UpdateProfilePictureAsync(string imagePath)
        {
            using var content = new MultipartFormDataContent();
            using var stream = File.OpenRead(imagePath);
            using var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            content.Add(streamContent, "picture", Path.GetFileName(imagePath));

            var response = await _httpClient.PutAsync("/api/users/me/profile-picture", content);

            if (!response.IsSuccessStatusCode)
            {
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var newPath = doc.RootElement.GetProperty("profilePicture").GetString();

            _user.PublicUser.ProfilePicture = newPath;

            return new RequestResult(true, null);
        }

        public ImageSource GetProfilePicture(string path)
        {
            return ImageSource.FromUri(new Uri($"{path}"));
        }

        public async Task<RequestResult> DeleteUserAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));
            }

            return new RequestResult(true, null);
        }

        public async Task<RequestResult> DeleteMyselfAsync()
        {
            var response = await _httpClient.DeleteAsync($"/api/users");

            if (!response.IsSuccessStatusCode)
            {
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));
            }

            return new RequestResult(true, null);
        }

        public async Task<RequestResult> GetMyPendingRestrictionsAsync()
        {
            var url = "/api/users/comment-restrictions/me";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));
            }

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("restriction", out var restrictionJson))
            {
                if (restrictionJson.ValueKind != JsonValueKind.Null)
                {
                    _user.Restriction.FromJson(restrictionJson);
                }
                else
                {
                    _user.Restriction.Id = 0;
                    _user.Restriction.EndDate = null;
                    _user.Restriction.StartDate = null;
                    _user.Restriction.Reason = null;
                }
            }

            return new RequestResult(true, null);
        }

        public async Task<RequestResult> CreateCommentRestrictionAsync(int userId, DateTime? endDate, string reason)
        {
            var payload = new
            {
                EndDate = endDate,
                Reason = reason
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"/api/users/{userId}/comment-restriction", content);

            if (!response.IsSuccessStatusCode)
            {
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));
            }

            return new RequestResult(true, null);
        }

        public async Task<(List<RestrictionVM>, RequestResult, DateTime? cursorDate, int? cursorKey)>
            GetCommentRestrictionsAsync( int count, RestrictionFilter filter, DateTime? cursorDate, int? cursorKey)
        {
            var queryParams = new List<string>
            {
                $"count={count}",
                $"filter={filter}"
            };

            if (cursorDate.HasValue)
                queryParams.Add($"cursorDate={Uri.EscapeDataString(cursorDate.Value.ToString("o"))}");

            if (cursorKey.HasValue)
                queryParams.Add($"cursorKey={cursorKey.Value}");

            var url = "/api/users/comment-restrictions?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return (null, new RequestResult(false, await ApiErrorParser.ParseAsync(response)), null, null);
            }

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var restrictions = new List<RestrictionVM>();

            foreach (var element in root.GetProperty("restrictions").EnumerateArray())
            {
                var vm = new RestrictionVM();
                vm.FromJson(element);
                restrictions.Add(vm);
            }

            DateTime? nextCursorDate = null;
            int? nextCursorKey = null;

            if (root.TryGetProperty("cursorDate", out var cursorDateProp) &&
                cursorDateProp.ValueKind != JsonValueKind.Null)
            {
                nextCursorDate = cursorDateProp.GetDateTime();
            }

            if (root.TryGetProperty("cursorId", out var cursorIdProp) &&
                cursorIdProp.ValueKind != JsonValueKind.Null)
            {
                nextCursorKey = cursorIdProp.GetInt32();
            }

            return (restrictions, new RequestResult(true, null), nextCursorDate, nextCursorKey );
        }

        public async Task<RequestResult> EndCommentRestrictionAsync(int restrictionId)
        {
            var response = await _httpClient.PutAsync($"/api/users/comment-restriction/{restrictionId}/end", null);

            if (!response.IsSuccessStatusCode)
            {
                return new RequestResult(false, await ApiErrorParser.ParseAsync(response));
            }

            return new RequestResult(true, null);
        }
    }
}

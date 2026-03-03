using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BookManager.ApiClients
{
    public static class ApiErrorParser
    {
        private static Func<Task>? _onLogout;

        public static void Initialize(Func<Task> onLogout)
        {
            _onLogout = onLogout;
        }

        public static async Task<string> ParseAsync(HttpResponseMessage response, bool handleUnauthorized = true)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && handleUnauthorized)
            {
                if (_onLogout != null)
                {
                    await _onLogout();
                }

                throw new UnauthorizedAccessException("Session expired. Logging out.");
            }

            if (string.IsNullOrWhiteSpace(content))
                return $"Server returned {response.StatusCode}";

            try
            {
                using var doc = JsonDocument.Parse(content);

                if (doc.RootElement.TryGetProperty("error", out var errorProp))
                    return errorProp.GetString() ?? "Unknown error";

                return content;
            }
            catch
            {
                return content;
            }
        }
    }
}

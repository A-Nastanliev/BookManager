using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BookManager.ApiClients
{
    public static class ApiErrorParser
    {
        public static async Task<string> ParseAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

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

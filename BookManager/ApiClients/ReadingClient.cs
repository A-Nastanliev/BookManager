using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager.ApiClients
{
    public class ReadingClient
    {
        readonly HttpClient _httpClient;

        public ReadingClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
    }
}

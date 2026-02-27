using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager.ApiClients
{
    public record RequestResult(
        bool Success,
        string? Error
    );
}

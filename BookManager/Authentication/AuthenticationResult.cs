using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager.Authentication
{
    public record AuthResult(
        bool Success,
        string? Error
    );
}

using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager.Authentication
{
    public static class EmailValidator
    {
        public static bool IsValid(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}

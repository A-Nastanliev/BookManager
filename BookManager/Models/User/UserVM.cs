using BookManager.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BookManager.ViewModels.Models
{
    public partial class UserVM : ObservableObject, IJsonParseable 
    {
        [ObservableProperty]
        private string emailAddress;

        [ObservableProperty]
        private UserRole role;

        [ObservableProperty]
        private DateTime createdAt;

        [ObservableProperty]
        private PublicUserVM publicUser;

        [ObservableProperty]
        private RestrictionVM restriction;

        public UserVM() 
        {
            publicUser = new PublicUserVM();
        }

        public void FromJson(JsonElement json)
        {
            EmailAddress = json.GetProperty("emailAddress").GetString()!;
            CreatedAt = json.GetProperty("createdAt").GetDateTime();
            Role = (UserRole)json.GetProperty("role").GetInt32();

            PublicUser.FromJson(json);

            if (json.TryGetProperty("currentRestriction", out var restriction)
                && restriction.ValueKind != JsonValueKind.Null)
            {
                Restriction ??= new RestrictionVM(PublicUser);
                Restriction.FromJson(restriction);
            }

        }
    }
}

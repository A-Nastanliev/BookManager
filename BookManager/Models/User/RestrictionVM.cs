using BookManager.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BookManager.Models.User
{
    public partial class RestrictionVM : ObservableObject, IJsonParseable
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private DateTime? startDate;

        [ObservableProperty]
        private DateTime? endDate;

        [ObservableProperty]
        private string reason;

        [ObservableProperty]
        private PublicUserVM publicUser;

        public RestrictionVM() { }

        public RestrictionVM(PublicUserVM user)
        {
            PublicUser = user;   
        }


        public void FromJson(JsonElement json)
        {
            Id = json.GetProperty("id").GetInt32();
            StartDate = json.GetProperty("startDate").GetDateTime();
            EndDate = json.TryGetProperty("endDate", out var end) && end.ValueKind != JsonValueKind.Null ? end.GetDateTime() : null;
            Reason = json.GetProperty("reason").GetString()!;

            if (json.TryGetProperty("user", out var publicUserJson))
            {
                PublicUser ??= new PublicUserVM();
                PublicUser.FromJson(publicUserJson);
            }
        }
    }
}

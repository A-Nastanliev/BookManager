using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json; 

namespace BookManager.Models.Book
{
    public partial class AuthorVM : ObservableObject, IJsonParseable, ICopyable<AuthorVM>
    {
        [ObservableProperty]
        int id;

        [ObservableProperty]
        string name;

        [ObservableProperty]
        string biography;

        [ObservableProperty]
        DateTime? birthDate;

        public void CopyFrom(AuthorVM original)
        {
           Id = original.Id;
           Name = original.Name;
           Biography = original.Biography;
           BirthDate = original.BirthDate;
        }

        public void FromJson(JsonElement json)
        {
            Id = json.GetProperty("id").GetInt32();
            Name = json.GetProperty("name").GetString()!;

            if (json.TryGetProperty("biography", out var bio) && bio.ValueKind != JsonValueKind.Null)
                Biography = bio.GetString();

            if (json.TryGetProperty("birthDate", out var birth) && birth.ValueKind != JsonValueKind.Null)
                BirthDate = birth.GetDateTime();
        }
    }

}

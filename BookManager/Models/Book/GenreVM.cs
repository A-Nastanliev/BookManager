using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BookManager.Models.Book
{
    public partial class GenreVM : ObservableObject, IJsonParseable, ICopyable<GenreVM>
    {
        [ObservableProperty]
        int id;

        [ObservableProperty]
        string name;

        [ObservableProperty]
        string description;

        public void CopyFrom(GenreVM original)
        {
            Id = original.Id;
            Name = original.Name;
            Description = original.Description;
        }

        public void FromJson(JsonElement json)
        {
            Id = json.GetProperty("id").GetInt32();
            Name = json.GetProperty("name").GetString()!;

            if (json.TryGetProperty("description", out var desc) && desc.ValueKind != JsonValueKind.Null)
                Description = desc.GetString();
        }
    }

}

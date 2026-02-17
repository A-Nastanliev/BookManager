using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BookManager.Models.Book
{
    public partial class PublisherVM : ObservableObject, IJsonParseable, ICopyable<PublisherVM>
    {
        [ObservableProperty]
        int id;

        [ObservableProperty]
        string name;

        [ObservableProperty]
        string description;

        [ObservableProperty]
        string website;

        public void CopyFrom(PublisherVM original)
        {
            Id = original.Id;
            Name = original.Name;
            Description = original.Description;
            Website = original.Website;
        }

        public void FromJson(JsonElement json)
        {
            Id = json.GetProperty("id").GetInt32();
            Name = json.GetProperty("name").GetString()!;

            if (json.TryGetProperty("description", out var desc) && desc.ValueKind != JsonValueKind.Null)
                Description = desc.GetString();

            if (json.TryGetProperty("website", out var web) && web.ValueKind != JsonValueKind.Null)
                Website = web.GetString();
        }
    }

}

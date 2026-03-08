using BookManager.Models.User;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BookManager.Models.Reading
{
    public partial class BookRequestVM : ObservableObject, IJsonParseable
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string isbn;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private BookRequestStatus status;

        [ObservableProperty]
        private DateTime dateSent;

        [ObservableProperty]
        private DateTime? dateActioned;

        [ObservableProperty]
        private PublicUserVM sender;

        [ObservableProperty]
        private PublicUserVM actionedBy;

        public BookRequestVM()
        {
            Sender = new PublicUserVM();
            ActionedBy = new PublicUserVM();
        }

        public BookRequestVM(PublicUserVM publicUser)
        {
            Sender = publicUser;
            ActionedBy = new PublicUserVM();
        }


        public void FromJson(JsonElement json)
        {
            Id = json.GetProperty("id").GetInt32();
            Isbn = json.GetProperty("isbn").GetString()!;
            Title = json.GetProperty("title").GetString()!;
            Status = (BookRequestStatus)json.GetProperty("status").GetInt32();
            DateSent = json.GetProperty("dateSent").GetDateTime();
            DateActioned = json.TryGetProperty("dateActioned", out var end) && end.ValueKind != JsonValueKind.Null ? end.GetDateTime() : null;

            if (json.TryGetProperty("sender", out var senderJson) && senderJson.ValueKind == JsonValueKind.Object
                && senderJson.EnumerateObject().Any())
            {
                Sender ??= new PublicUserVM();
                Sender.FromJson(senderJson);
            }

            if (json.TryGetProperty("actionedBy", out var actionJson) && actionJson.ValueKind == JsonValueKind.Object
                && actionJson.EnumerateObject().Any())
            {
                ActionedBy ??= new PublicUserVM();
                ActionedBy.FromJson(actionJson);
            }
        }
    }

}
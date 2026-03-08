using BookManager.Models.User;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BookManager.Models.Reading
{
    public partial class CommentVM : ObservableObject, IJsonParseable
    {
        [ObservableProperty]
        int id;

        [ObservableProperty]
        int userId;

        [ObservableProperty]
        string comment;

        [ObservableProperty]
        int userPageProgress;

        [ObservableProperty]
        DateTime dateTime;

        [ObservableProperty]
        PublicUserVM publicUser;

        public CommentVM()  { }

        public CommentVM(PublicUserVM publicUser)
        {
            PublicUser = publicUser;
        }

        public void FromJson(JsonElement json)
        {
            Id = json.GetProperty("id").GetInt32();
            UserId = json.GetProperty("userId").GetInt32();
            Comment = json.GetProperty("comment").GetString();
            UserPageProgress = json.GetProperty("userPageProgress").GetInt32();
            DateTime = json.GetProperty("dateTime").GetDateTime();

            if (json.TryGetProperty("user", out var userJson))
            {
                PublicUser ??= new PublicUserVM();
                PublicUser.FromJson(userJson);
            }
        }
    }
}

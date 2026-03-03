using BookManager.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace BookManager.Models.User
{
    public partial class PublicUserVM : ObservableObject, IJsonParseable
    {
        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string profilePicture;

        [ObservableProperty]
        private ImageSource profilePictureSource;

        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private RestrictionVM currentRestriction;

        public PublicUserVM() 
        {
        }

        public PublicUserVM(int id, string username,  string profilePicture, RestrictionVM currentRestriction)
        {
            Id = id;
            Username = username;
            ProfilePicture = profilePicture;
            CurrentRestriction = currentRestriction;
        }

        public void FromJson(JsonElement json)
        {
            Id = json.GetProperty("id").GetInt32();
            Username = json.GetProperty("username").GetString()!;
            ProfilePicture = json.GetProperty("profilePicture").GetString();

            if (!string.IsNullOrWhiteSpace(ProfilePicture))
            {
                try
                {
                    ProfilePictureSource = ImageSource.FromUri(new Uri(ProfilePicture));
                }
                catch
                {
                    ProfilePictureSource = null;
                }
            }
            else
            {
                ProfilePictureSource = null;
            }
        }
    }
}

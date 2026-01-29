using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager.ViewModels.Models
{
    public partial class UserVM : ObservableObject
    {
        [ObservableProperty]
        private string emailAddress;

        [ObservableProperty]
        private UserRole role;

        [ObservableProperty]
        private DateTime createdAt;

        [ObservableProperty]
        private PublicUserVM publicUser;

        public UserVM() 
        {
            publicUser = new PublicUserVM();
        }

        public UserVM(int id, string emailAddress, string username, string profilePicture, UserRole role, DateTime createdAt, RestrictionVM currentRestriction)
        {
            publicUser.Id = id;
            Role = role;
            CreatedAt = createdAt;
            publicUser.CurrentRestriction = currentRestriction;
            publicUser.Username = username;
            publicUser.ProfilePicture = profilePicture;
        }
    }
}

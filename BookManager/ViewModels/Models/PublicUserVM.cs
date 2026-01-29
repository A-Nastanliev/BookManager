using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager.ViewModels.Models
{
    public partial class PublicUserVM : ObservableObject
    {
        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string profilePicture;

        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private RestrictionVM currentRestriction;

        public PublicUserVM() 
        {
            CurrentRestriction = new RestrictionVM();
        }

        public PublicUserVM(int id, string username,  string profilePicture, RestrictionVM currentRestriction)
        {
            Id = id;
            Username = username;
            ProfilePicture = profilePicture;
            CurrentRestriction = currentRestriction;
        }
    }
}

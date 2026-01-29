using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager.ViewModels.Models
{
    public partial class RestrictionVM : ObservableObject
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
        private PublicUserVM user;

        public RestrictionVM() { }
    }
}

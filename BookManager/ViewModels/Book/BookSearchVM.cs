using BookManager.ViewModels.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager.ViewModels.Book
{
    public partial class BookSearchVM : ObservableObject
    {
        [ObservableProperty]
        UserVM user;

        public BookSearchVM(UserVM user)
        {
            User = user;
        }
    }
}

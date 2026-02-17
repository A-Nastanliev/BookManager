using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager.ViewModels
{
    public abstract partial class PagedLoadingVM : ObservableObject
    {
        [ObservableProperty]
        bool loading;

        protected DateTime? CursorDate { get; set; }
        protected int BatchSize { get; set; } = 16;
        protected int? CursorId { get; set; }
        protected bool CanLoadMore { get; set; } = true;

        protected bool CanStartLoading()
            => !Loading && CanLoadMore;

        protected void BeginLoading()
            => Loading = true;

        protected void EndLoading(int itemsLoaded, DateTime? cursorDate, int? cursorId)
        {
            Loading = false;

            if (itemsLoaded < BatchSize)
                CanLoadMore = false;

            CursorDate = cursorDate;
            CursorId = cursorId;

        }

        public abstract Task Load();
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookManager.ViewModels
{
    public partial class PagedLoadingVM : ObservableObject
    {
        [ObservableProperty]
        bool loading;

        [ObservableProperty]
        bool isRefreshing;

        public DateTime? CursorDate { get; set; }
        public int BatchSize { get; set; } = 16;
        public int? CursorId { get; set; }
        protected bool CanLoadMore { get; set; } = true;

        protected CancellationTokenSource? _searchCts;

        public bool CanStartLoading()
            => !Loading && CanLoadMore;

        public void BeginLoading()
            => Loading = true;

        public void EndLoading(int itemsLoaded, DateTime? cursorDate, int? cursorId)
        {
            Loading = false;

            if (itemsLoaded < BatchSize)
                CanLoadMore = false;

            CursorDate = cursorDate;
            CursorId = cursorId;

        }

        public virtual Task Load() => Task.CompletedTask;

        public virtual Task Refresh() => Task.CompletedTask;
    }
}

using BookManager.ApiClients;
using BookManager.Models.User;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BookManager.ViewModels.Settings
{
    public partial class RestrictionsVM : PagedLoadingVM
    {
        [ObservableProperty]
        int selectedSegment;

        [ObservableProperty]
        ObservableCollection<RestrictionVM> restrictions = new();

        [ObservableProperty]
        RestrictionVM selectedRestriction;

        [ObservableProperty]
        bool canEndRestriction;

        [ObservableProperty]
        UserVM user;

        public Func<Task> OpenBottomSheet;

        public Func<Task> CloseBottomSheet;

        UserClient _userClient;

        public RestrictionsVM(UserClient userClient, UserVM userVM)
        {
            _userClient = userClient;
            User = userVM;
        }

        [RelayCommand]
        public override async Task Load()
        {
            if (!CanStartLoading()) return;

            BeginLoading();

            try
            {
                var (restrictions, result, cursorDate, cursorId) = 
                    await _userClient.GetCommentRestrictionsAsync(BatchSize, (RestrictionFilter)SelectedSegment, CursorDate, CursorId);

                if (!result.Success)
                {
                    EndLoading(BatchSize, CursorDate, CursorId);
                    await Shell.Current.DisplayAlertAsync("Error", $"{result.Error}", "OK");                
                    return;
                }

                foreach (var r in restrictions)
                {
                    Restrictions.Add(r);
                }

                if (restrictions.Any())
                {
                    EndLoading(restrictions.Count, Restrictions.Last().StartDate, Restrictions.Last().Id);
                    return;
                }

                EndLoading(0, null, null);
            }
            catch (Exception ex)
            {
                Loading = false;
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task EndCommentRestriction()
        {
            if (SelectedRestriction == null) return;

            bool confirm = await Shell.Current.DisplayAlertAsync("Confirm",
                $"Are you sure you want to end {SelectedRestriction.PublicUser.Username}'s restriction.", "Yes", "No");

            if (!confirm)
            {
                SelectedRestriction = null;
                return;
            }

            try
            {
                var result = await _userClient.EndCommentRestrictionAsync(SelectedRestriction.Id);
                if (result.Success)
                {
                    CanEndRestriction = false;
                    await CloseBottomSheet?.Invoke();
                    Restrictions.Remove(SelectedRestriction);
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Error", $"{result.Error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        partial void OnSelectedSegmentChanged(int oldValue, int newValue)
        {
            Restrictions.Clear();
            CanLoadMore = true;
            Loading = false;
            CursorId = null;
            CursorDate = null;
            CanEndRestriction = false;
            LoadCommand.Execute(null);
        }

        public override Task Refresh()
        {
            throw new NotImplementedException();
        }

        public async Task OnAppearingAsync()
        {
            await Load();
        }

        [RelayCommand]
        public async Task SelectRestriction(RestrictionVM restriction)
        {
            if (SelectedRestriction == restriction)
            {
                SelectedRestriction = null;
            }
            else
            {
                SelectedRestriction = restriction;
                CanEndRestriction = (restriction.EndDate > DateTime.UtcNow || restriction.EndDate == null);
                await OpenBottomSheet?.Invoke();
            }
        }
    }
}


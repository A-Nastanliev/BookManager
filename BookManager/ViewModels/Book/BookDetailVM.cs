using BookManager.ApiClients;
using BookManager.Models.Book;
using BookManager.Models.Reading;
using BookManager.Models.User;
using BookManager.Views.Book;
using CommunityToolkit.Maui.Alerts;
using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Collections.ObjectModel;

namespace BookManager.ViewModels.Book
{
    public partial class BookDetailVM : ObservableObject,IQueryAttributable
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PageProgress))]
        BookVM book = new();

        [ObservableProperty]
        ObservableCollection<ReadingLogVM> logs = new();

        [ObservableProperty]
        ReadingLogVM newLog = new();

        readonly PagedLoadingVM logPagedVM = new();

        [ObservableProperty]
        UserBookStatus status;

        [ObservableProperty]
        ObservableCollection<CommentVM> comments = new();

        [ObservableProperty]
        CommentVM selectedComment = new();

        [ObservableProperty]
        CommentVM newComment;

        readonly PagedLoadingVM commentPagedVM = new();

        [ObservableProperty]
        bool canPunishUser;

        public ObservableCollection<KeyValuePair<string, TimeSpan?>> RestrictionOptions { get; } = new ObservableCollection<KeyValuePair<string, TimeSpan?>>
            {
                new("Restrict for 1 hour", TimeSpan.FromHours(1)),
                new("Restrict for 2 hours", TimeSpan.FromHours(2)),
                new("Restrict for 8 hours", TimeSpan.FromHours(8)),
                new("Restrict for 1 day", TimeSpan.FromDays(1)),
                new("Restrict for 2 days", TimeSpan.FromDays(2)),
                new("Restrict for 1 week", TimeSpan.FromDays(7)),
                new("Undefined", null)
            };

        [ObservableProperty]
        KeyValuePair<string, TimeSpan?> selectedRestrictionOption;

        [ObservableProperty]
        string restrictionReason;

        [ObservableProperty]
        bool canDeleteComment;

        public Func<Task> OnSelectComment;
        public Func<Task> OnDeselectComment;

        [ObservableProperty]
        int selectedTabIndex;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PageProgress))]
        int readPages;

        [ObservableProperty]
        byte? myRating;

        [ObservableProperty]
        int ratingCount;

        [ObservableProperty]
        double averageRating;

        bool _canManageRating;

        public double PageProgress => (Book?.TotalPages ?? 0) > 0 ? (double)ReadPages / (Book.TotalPages ?? 1) : 0;

        readonly ReadingClient _readingClient;
        readonly BookClient _bookClient;
        readonly UserClient _userClient;
        readonly UserVM _userVM;

        public BookDetailVM(ReadingClient readingClient, BookClient bookClient, UserClient userClient, UserVM user)
        {
            _readingClient = readingClient;
            _bookClient = bookClient;
            _userClient = userClient;
            _userVM = user;
            NewComment = new CommentVM(user.PublicUser);
        }

        [RelayCommand]
        public async Task Load()
        {
            try 
            {
                (Status, ReadPages, MyRating, RatingCount, AverageRating) = await _readingClient.GetUserBookDetailsAsync(Book.Id);
                await _userClient.GetMyPendingRestrictionsAsync();
                _canManageRating = true;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task ClearRating()
        {
            if(MyRating != null && MyRating != 0)
            {

                try
                {
                    var result = await _readingClient.DeleteBookRatingAsync(Book.Id);
                    if (!result.Success)
                    {
                        await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                        return;
                    }
                }
                catch (Exception ex) 
                {
                    await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
                }

                double totalRating = AverageRating * RatingCount;
                totalRating -= (double)MyRating;
                RatingCount -= 1;
                if (RatingCount == 0)
                {
                    AverageRating = 0;
                }
                else
                {
                    AverageRating = totalRating / RatingCount;
                }
                MyRating = 0;
            }
        }

        partial void OnMyRatingChanged(byte? oldValue, byte? newValue)
        {
            if (_canManageRating)
            {
                if (newValue != 0 && newValue != null && (oldValue == 0 || oldValue == null))
                    CreateRatingCommand.Execute(null);
                else if (newValue != 0 && newValue != null && oldValue != 0 && oldValue != null)
                    UpdateRatingCommand.Execute(oldValue);
            }
        }

        [RelayCommand]
        private async Task CreateRating()
        {
            try
            {
                var result = await _readingClient.CreateBookRatingAsync(Book.Id, MyRating.Value);
                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }

            double totalRating = AverageRating * RatingCount;
            totalRating += (double)MyRating;
            RatingCount += 1;
            AverageRating = totalRating / RatingCount;
        }

        [RelayCommand]
        private async Task UpdateRating(byte oldRating)
        {
            try
            {
                var result = await _readingClient.UpdateBookRatingAsync(Book.Id, MyRating.Value);
                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }

            double totalRating = AverageRating * RatingCount;
            totalRating += (double)MyRating - (double)oldRating;
            AverageRating = totalRating / RatingCount;
        }

        [RelayCommand]
        public async Task LoadLogs()
        {
            if (!logPagedVM.CanStartLoading())
                return;


            logPagedVM.BeginLoading();

            try
            {
                var (logs, cursorDate, cursorKey) = await _readingClient.GetNextReadingLogsAsync
                    (Book.Id, logPagedVM.BatchSize , logPagedVM.CursorDate, logPagedVM.CursorId);
                if (logs.Any())
                {
                    foreach(var l in logs)
                    {
                        Logs.Add(l);
                    }               
                    logPagedVM.EndLoading(logs.Count, cursorDate, cursorKey);
                    return;
                }

                logPagedVM.EndLoading(0, null, null);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task LoadComments()
        {
           if (!commentPagedVM.CanStartLoading())
                return;


            commentPagedVM.BeginLoading();

            try
            {
                var (comments, cursorDate, cursorKey) = await _readingClient.GetNextBookCommentsAsync
                    (Book.Id, commentPagedVM.BatchSize, commentPagedVM.CursorDate, commentPagedVM.CursorId);
                if (comments.Any())
                {
                    foreach (var c in comments)
                    {
                        Comments.Add(c);
                    }
                    commentPagedVM.EndLoading(comments.Count, cursorDate, cursorKey);
                    return;
                }

                commentPagedVM.EndLoading(0, null, null);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task CreateComment()
        {
            if ((_userVM.Restriction.EndDate > DateTime.UtcNow || _userVM.Restriction.EndDate == null )&& _userVM.Restriction.Id >0)
            {
                string endTime = "";
                if(_userVM.Restriction.EndDate > DateTime.Now)
                {
                    endTime = "on " + _userVM.Restriction.EndDate.Value.ToLocalTime().ToString("HH:mm d MMMM yyyy");
                }
                else
                {
                    endTime = "when an administrator lift it up";
                }

                await Shell.Current.DisplayAlertAsync("You have restriction", 
                    $"Your restriction end's {endTime}", "OK");
                return;
            }

            if (!string.IsNullOrWhiteSpace(NewComment.Comment))
            {
                NewComment.Comment = NewComment.Comment.Trim();
                NewComment.Comment = Regex.Replace(NewComment.Comment, @"^[ \t]+$[\r\n]*", "", RegexOptions.Multiline);
                NewComment.Comment = Regex.Replace(NewComment.Comment, @"(\r?\n){2,}", "\n");
                NewComment.Comment = Regex.Replace(NewComment.Comment, @" {2,}", " ");
            }

            if (NewComment.Comment?.Length < 4 || NewComment.Comment?.Length > 500 || string.IsNullOrWhiteSpace(NewComment.Comment))
            {
                await Shell.Current.DisplayAlertAsync("Invalid input", "Comments have to be between 4 and 500 characters ", "OK");
                return;
            }

            try
            {
                var result = await _readingClient.CreateBookCommentAsync(Book.Id, NewComment );
                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }
                NewComment.UserPageProgress = ReadPages;
                Comments.Insert(0, NewComment);
                NewComment = new CommentVM(_userVM.PublicUser);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task DeleteComment()
        {
            bool confirm = await Shell.Current.DisplayAlertAsync($"Delete comment by {SelectedComment.PublicUser.Username}",
                "Are you sure you want to delete this comment?", "Yes", "No");

            if (!confirm)
                return;
            try
            {
                var result = await _readingClient.DeleteBookCommentAsync(SelectedComment.Id);

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }

                await OnDeselectComment?.Invoke();
                Comments.Remove(SelectedComment);
                SelectedComment = null;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }

        }

        [RelayCommand]
        public async Task SelectComment(CommentVM commentVM)
        {           
            SelectedComment = commentVM;
            CanPunishUser = _userVM.Role == UserRole.Admin && commentVM.PublicUser.Id != _userVM.PublicUser.Id;
            CanDeleteComment = commentVM.UserId == _userVM.PublicUser.Id || _userVM.Role == UserRole.Admin;
            await OnSelectComment?.Invoke();
        }

        [RelayCommand]
        public async Task DeleteUserFromComment()
        {
            bool confirm = await Shell.Current.DisplayAlertAsync($"Delete {SelectedComment.PublicUser.Username}",
              "Are you sure you want to delete this user?", "Yes", "No");

            if (!confirm)
                return;

            confirm = await Shell.Current.DisplayAlertAsync($"Confirm delete again {SelectedComment.PublicUser.Username}",
                "Are you sure you want to delete this user?", "Yes", "No");

            if (!confirm)
                return;

            try
            {
                int userId = SelectedComment.PublicUser.Id;
                var result = await _userClient.DeleteUserAsync(userId);

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }

                await OnDeselectComment?.Invoke();
                SelectedComment = null;

                var toRemove = Comments.Where(c => c.PublicUser.Id == userId).ToList();
                foreach (var c in toRemove)
                {
                    Comments.Remove(c);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task RestrictUser()
        {
            try
            {
                TimeSpan? duration = SelectedRestrictionOption.Value;
                DateTime? endDate = duration.HasValue ? DateTime.UtcNow.Add(duration.Value) : null;

                if (!string.IsNullOrWhiteSpace(RestrictionReason))
                {
                    RestrictionReason = RestrictionReason.Trim();
                    RestrictionReason = Regex.Replace(RestrictionReason, @"^[ \t]+$[\r\n]*", "", RegexOptions.Multiline);
                    RestrictionReason = Regex.Replace(RestrictionReason, @"(\r?\n){2,}", "\n");
                    RestrictionReason = Regex.Replace(RestrictionReason, @" {2,}", " ");
                }
                var result = await _userClient.CreateCommentRestrictionAsync(SelectedComment.UserId, endDate, RestrictionReason);

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }

                RestrictionReason = null;
                _ = Toast.Make($"{SelectedComment.PublicUser.Username} restricted").Show();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }

        }

        [RelayCommand]
        public async Task Edit()
        {
            await Shell.Current.GoToAsync(nameof(BookFormPage), new Dictionary<string, object> { [nameof(BookFormVM.Book)] = Book });
        }

        [RelayCommand]
        public async Task Delete() 
        {
            bool confirm = await Shell.Current.DisplayAlertAsync($"Delete {Book.Title}",
                "Are you sure you want to delete this book?", "Yes", "No");

            if (!confirm)
                return;

            try
            {
                var result = await _bookClient.DeleteBookAsync(Book.Id);

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }

                _ = Toast.Make($"{Book.Title} deleted").Show();
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<(BookVM, UserBookStatus)>((Book, UserBookStatus.None)), Messages.UserBookStatusChanged);
                await Shell.Current.GoToAsync("..", new Dictionary<string, object> { ["deletedBookId"] = Book.Id });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task ManageUserBook()
        {
            try
            {
                RequestResult result = new RequestResult(false, "error");
                if (Status == UserBookStatus.None) 
                {
                   result  = await _readingClient.WishlistBookAsync(Book.Id); 
                }
                else if(Status == UserBookStatus.Wishlisted)
                {
                    result = await _readingClient.DeleteUserBookAsync(Book.Id);
                }

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }

                if (Status == UserBookStatus.None)
                {
                    Status = UserBookStatus.Wishlisted;
                    _ = Toast.Make($"{Book.Title} wishlisted").Show();
                }
                else if (Status == UserBookStatus.Wishlisted)
                {
                    Status = UserBookStatus.None;
                    _ = Toast.Make($"{Book.Title} removed from wishlist").Show();
                }
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<(BookVM, UserBookStatus)>((Book, Status)), Messages.UserBookStatusChanged);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task ClearUserBook()
        {
            bool confirm = await Shell.Current.DisplayAlertAsync($"Delete reading data for {Book.Title}",
               "Are you sure you want to delete your logs?", "Yes", "No");

            if (!confirm)
                return;

            try
            {
                var result = await _readingClient.DeleteUserBookAsync(Book.Id);

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }

                Logs.Clear();
                _ = Toast.Make($"{Book.Title}'s data cleared").Show();
                Status = UserBookStatus.None;
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<(BookVM, UserBookStatus)>((Book, Status)), Messages.UserBookStatusChanged);
                ReadPages = 0;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task CreateLog()
        {
            if(NewLog.StartingPage == null || NewLog.EndingPage == null || NewLog.StartingPage <=0 || NewLog.EndingPage < NewLog.StartingPage
                || NewLog.EndingPage > Book.TotalPages)
            {
                await Shell.Current.DisplayAlertAsync("Invalid input",
                    "To create a log you need start and end page. Start page can't be smaller than 1. " +
                    "End page can't be smaller than start page or bigger than the total pages of the book. "
                    , "OK");
                return;
            }

            try
            {
                var result = await _readingClient.CreateReadingLogAsync(Book.Id,NewLog);

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }

                Logs.Insert(0, NewLog);
                ReadPages += NewLog.PagesRead;
                if(ReadPages == Book.TotalPages)
                {
                    Status = UserBookStatus.Finished;
                }
                else
                {
                    Status = UserBookStatus.Reading;
                }
                NewLog = new ReadingLogVM();
                WeakReferenceMessenger.Default.Send(new ValueChangedMessage<(BookVM, UserBookStatus)>((Book, Status)), Messages.UserBookStatusChanged);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task DeleteReadingLog(ReadingLogVM log)
        {

            bool confirm = await Shell.Current.DisplayAlertAsync($"Delete reading log",
               $"Are you sure you want to your log from {log.Date.ToLocalTime().ToString("HH:mm d MMMM yyyy")}, start page: {log.StartingPage.ToString()} " +
               $", end page: {log.EndingPage.ToString()} ?", "Yes", "No");

            if (!confirm)
                return;
            try
            {
                var result = await _readingClient.DeleteReadingLogAsync(Book.Id, log.Id);

                if (!result.Success)
                {
                    await Shell.Current.DisplayAlertAsync("Error", result.Error, "OK");
                    return;
                }

                Logs.Remove(log);
                _ = Toast.Make($"Log from {log.Date.ToLocalTime().ToString("HH:mm d MMMM yyyy")} deleted").Show();
                ReadPages -= log.PagesRead;
                if(ReadPages == 0)
                {
                    Status = UserBookStatus.Wishlisted;
                    WeakReferenceMessenger.Default.Send(new ValueChangedMessage<(BookVM, UserBookStatus)>((Book, Status)), Messages.UserBookStatusChanged);
                }
                else if((ReadPages + log.PagesRead) == Book.TotalPages)
                {
                    Status = UserBookStatus.Reading;
                    WeakReferenceMessenger.Default.Send(new ValueChangedMessage<(BookVM, UserBookStatus)>((Book, Status)), Messages.UserBookStatusChanged);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }

        }

       [RelayCommand]
        public async Task GenreTapped()
        {
            if (Book.Genre.Id == 0)
                return;

            await Shell.Current.GoToAsync(nameof(BookAttributePage), new Dictionary<string, object> { [nameof(BookAttributeVM.Genre)] = Book.Genre });
        }

        [RelayCommand]
        public async Task AuthorTapped()
        {
            if (Book.Author.Id == 0)
                return;

            await Shell.Current.GoToAsync(nameof(BookAttributePage), new Dictionary<string, object> { [nameof(BookAttributeVM.Author)] = Book.Author });
        }

        [RelayCommand]
        public async Task PublisherTapped()
        {
            if(Book.Publisher.Id == 0)
                return;

            await Shell.Current.GoToAsync(nameof(BookAttributePage), new Dictionary<string, object> { [nameof(BookAttributeVM.Publisher)] = Book.Publisher });
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue($"{nameof(Book)}", out var obj) && obj is BookVM book)
            {
                Book.CopyFrom(book);
                await Load();
                await LoadLogs();
                await LoadComments();
                query.Remove($"{nameof(Book)}");
            }
        }
    }
}

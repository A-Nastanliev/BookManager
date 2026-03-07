using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace BookManager.Models.Reading
{
    public partial class ReadingLogVM : ObservableObject, IJsonParseable
    {
        [ObservableProperty]
        int id;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PagesRead))]
        int? startingPage;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PagesRead))]
        int? endingPage;

        [ObservableProperty]
        DateTime date;

        public int PagesRead => (EndingPage ?? 0) - (StartingPage ?? 0) + 1;

        public ReadingLogVM() { }

        public void FromJson(JsonElement json)
        {
            Id = json.GetProperty("id").GetInt32();
            StartingPage = json.GetProperty("startingPage").GetInt32();
            EndingPage = json.GetProperty("endingPage").GetInt32();
            Date = json.GetProperty("date").GetDateTime();
        }
    }
}

using System.Text.Json;

namespace BookManager.Models
{
    public interface IJsonParseable
    {
        void FromJson(JsonElement json);
    }
}

namespace BookManager.Authentication
{
    public interface ITokenStore
    {
        Task<string?> GetAccessTokenAsync();
        Task SetAccessTokenAsync(string? token);
        void Clear();
    }
}

namespace BusinessLayer
{
	public interface IRepository<T, K> where T : class where K : struct
    {
		Task<bool> CreateAsync(T obj);
		Task<List<T>> ReadAllAsync();
		Task<T> ReadAsync(K obj);
		Task<(List<T>, DateTime? cursorDate, K? cursorKey)> ReadNextAsync(int count, DateTime? cursorDate, K? cursorKey);
		Task UpdateAsync(T OBJ);
		Task<bool> DeleteAsync(T obj);

	}
}

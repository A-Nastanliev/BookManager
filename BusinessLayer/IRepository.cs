namespace BusinessLayer
{
	public interface IRepository<T, K> where T : class
	{
		Task<bool> CreateAsync(T obj);
		Task<List<T>> ReadAllAsync();
		Task<T> ReadAsync(K obj);
		Task<List<T>> ReadNextAsync(int count, int loaded);
		Task<bool> UpdateAsync(T OBJ);
		Task<bool> DeleteAsync(K obj);

	}
}

namespace BusinessLayer
{
	public interface IRepository<T, K> where T : class
	{
		Task CreateAsync(T obj);
		Task<List<T>> ReadAllAsync();
		Task<T> ReadAsync(K obj);
		Task UpdateAsync(T OBJ);
		Task DeleteAsync(K obj);

	}
}

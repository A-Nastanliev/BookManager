namespace BusinessLayer
{
	public abstract class AbstractRepository<T, K> : IRepository<T, K> where T : class where K : struct
    {
		protected readonly BookManagerContext _context;

		public AbstractRepository(BookManagerContext context)
		{
			_context = context;
		}

		public virtual async Task<bool> CreateAsync(T obj)
		{
			_context.Set<T>().Add(obj);
			return await _context.SaveChangesAsync() > 0;
		}

		public virtual async Task<List<T>> ReadAllAsync()
		{
			return await _context.Set<T>().AsNoTracking().ToListAsync();
		}

		public virtual async Task<T> ReadAsync(K id)
		{
			return await _context.Set<T>().FindAsync(id);
		}

		public virtual async Task UpdateAsync(T obj)
		{
			var keyProperty = _context.Model.FindEntityType(typeof(T))
								  .FindPrimaryKey()
								  .Properties
								  .FirstOrDefault();

			var keyValue = (K)keyProperty.PropertyInfo.GetValue(obj);

			var existingEntity = await _context.Set<T>().FindAsync(keyValue);

			_context.Entry(existingEntity).CurrentValues.SetValues(obj);

			await _context.SaveChangesAsync() ;
		}

		public virtual async Task<bool> DeleteAsync(T entity)
		{
			var entityType = _context.Model.FindEntityType(typeof(T));
			var keyProperties = entityType.FindPrimaryKey().Properties;

			var keyValues = keyProperties
				.Select(p => p.PropertyInfo.GetValue(entity))
				.ToArray();

			var existingEntity = await _context.Set<T>().FindAsync(keyValues);

			if (existingEntity == null)
				return false;

			_context.Set<T>().Remove(existingEntity);
			return await _context.SaveChangesAsync() > 0;
		}

		public abstract Task<(List<T>, DateTime? cursorDate, K? cursorKey)> ReadNextAsync(int count, DateTime? cursorDate, K? cursorKey);
    }
}

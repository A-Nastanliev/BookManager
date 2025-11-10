namespace BusinessLayer
{
	public abstract class GenericDbService<T, K> : IRepository<T, K> where T : class
	{
        protected readonly BookManagerContext _context;

		public GenericDbService(BookManagerContext context)
        {
            _context = context;
        }

        public virtual async Task CreateAsync(T obj)
        {
            _context.Set<T>().Add(obj);
            await _context.SaveChangesAsync();
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

            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(K id)
        {
            var entity = await ReadAsync(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

    }
}

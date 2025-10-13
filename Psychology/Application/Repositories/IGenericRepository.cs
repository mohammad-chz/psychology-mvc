using System.Linq.Expressions;

namespace Psychology.Application.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(long id, CancellationToken ct = default);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
                                     Func<IQueryable<T>, IQueryable<T>>? include = null,
                                     bool asNoTracking = true,
                                     CancellationToken ct = default);

        Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null,
                                            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                            Func<IQueryable<T>, IQueryable<T>>? include = null,
                                            bool asNoTracking = true,
                                            CancellationToken ct = default);

        Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            bool asNoTracking = true,
            CancellationToken ct = default);

        Task AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Remove(T entity);            // respects soft-delete via DbContext override
        void RemoveRange(IEnumerable<T> entities);

        IQueryable<T> Query();            // advanced scenarios (compositions)
    }
}

using Microsoft.EntityFrameworkCore;
using Psychology.Application.Interfaces;
using Psychology.Infrastructure.Persistence;
using System.Linq;
using System.Linq.Expressions;

namespace Psychology.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _ctx;
        protected readonly DbSet<T> _db;

        public GenericRepository(AppDbContext ctx)
        {
            _ctx = ctx;
            _db = ctx.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            // Works when the key name is "Id" (long). If composite keys, override in specific repo.
            return await _db.FindAsync([id], ct);
        }

        public virtual async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            IQueryable<T> q = _db;
            if (include is not null) q = include(q);
            if (asNoTracking) q = q.AsNoTracking();

            return await q.FirstOrDefaultAsync(predicate, ct);
        }

        public virtual async Task<IReadOnlyList<TResult>> GetListAsync<TResult>(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Expression<Func<T, TResult>>? selector = null,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            IQueryable<T> q = _db;
            
            if (predicate != null)
                q = q.Where(predicate);

            if (include != null)
                q = include(q);

            if (orderBy != null)
                q = orderBy(q);

            if (asNoTracking)
                q = q.AsNoTracking();

            if (selector != null)
                return await q.Select(selector).ToListAsync(ct);

            // fallback if no projection given
            return (IReadOnlyList<TResult>)await q.Cast<TResult>().ToListAsync(ct);
        }


        public virtual async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            bool asNoTracking = true,
            CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<T> q = _db;
            if (predicate != null) q = q.Where(predicate);
            if (include != null) q = include(q);

            var total = await q.CountAsync(ct);

            if (orderBy != null) q = orderBy(q);
            if (asNoTracking) q = q.AsNoTracking();

            var items = await q.Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(ct);

            return (items, total);
        }

        public virtual async Task AddAsync(T entity, CancellationToken ct = default)
            => await _db.AddAsync(entity, ct);

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
            => await _db.AddRangeAsync(entities, ct);

        public virtual void Update(T entity) => _db.Update(entity);
        public virtual void UpdateRange(IEnumerable<T> entities) => _db.UpdateRange(entities);
        public virtual void Remove(T entity) => _db.Remove(entity);
        public virtual void RemoveRange(IEnumerable<T> entities) => _db.RemoveRange(entities);

        public IQueryable<T> Query() => _db.AsQueryable();
    }
}

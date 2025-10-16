using Psychology.Application.Interfaces;
using Psychology.Domain.Entities;
using Psychology.Infrastructure.Persistence;

namespace Psychology.Infrastructure.Repositories
{
    public sealed class ErrorLogRepository : GenericRepository<ErrorLog>, IErrorLogRepository
    {
        public ErrorLogRepository(AppDbContext context) : base(context) { }
    }
}

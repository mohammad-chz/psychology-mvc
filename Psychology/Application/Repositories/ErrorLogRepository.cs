using Psychology.Application.Interfaces;
using Psychology.Domain.Entities;
using Psychology.Infrastructure.Persistence;
using Psychology.Infrastructure.Repositories;

namespace Psychology.Application.Repositories
{
    public sealed class ErrorLogRepository : GenericRepository<ErrorLog>, IErrorLogRepository
    {
        public ErrorLogRepository(AppDbContext context) : base(context) { }
    }
}

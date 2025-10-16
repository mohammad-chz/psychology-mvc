using Psychology.Application.Interfaces;
using Psychology.Domain.Entities;
using Psychology.Infrastructure.Persistence;

namespace Psychology.Infrastructure.Repositories
{
    public class PhonePrefixesRepository : GenericRepository<PhonePrefix>, IPhonePrefixesRepository
    {
        public PhonePrefixesRepository(AppDbContext db) : base(db) { }
    }
}

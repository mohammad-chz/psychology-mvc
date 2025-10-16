using Psychology.Application.Interfaces;
using Psychology.Domain.Entities;
using Psychology.Infrastructure.Persistence;

namespace Psychology.Infrastructure.Repositories
{
    public class PreConsultationRepository : GenericRepository<PreConsultation>, IPreConsultationRepository
    {
        public PreConsultationRepository(AppDbContext db) : base(db) { }
    }
}

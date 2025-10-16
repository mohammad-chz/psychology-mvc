using System.Data;

namespace Psychology.Application.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        // Repositories
        IPreConsultationRepository PreConsultation { get; }
        IPhonePrefixesRepository PhonePrefixes { get; }
        // Save
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        // Transactions (optional but recommended when multiple aggregates change together)
        Task BeginTransactionAsync(IsolationLevel isolation = IsolationLevel.ReadCommitted, CancellationToken ct = default);
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);
    }
}

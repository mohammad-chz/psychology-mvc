using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Psychology.Application.Interfaces;
using Psychology.Infrastructure.Persistence;
using System.Data;

namespace Psychology.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _ctx;
        private IDbContextTransaction? _tx;

        public IPreConsultationRepository PreConsultation { get; }
        public IPhonePrefixesRepository PhonePrefixes { get; }

        public UnitOfWork(
            AppDbContext ctx,
            IPreConsultationRepository preConsultation,
            IPhonePrefixesRepository phonePrefixes)
        {
            _ctx = ctx;
            PreConsultation = preConsultation;
            PhonePrefixes = phonePrefixes;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _ctx.SaveChangesAsync(ct);

        public async Task BeginTransactionAsync(IsolationLevel isolation = IsolationLevel.ReadCommitted, CancellationToken ct = default)
        {
            if (_tx is not null) return;
            _tx = await _ctx.Database.BeginTransactionAsync(isolation, ct);
        }

        public async Task CommitAsync(CancellationToken ct = default)
        {
            if (_tx is null) return;
            await _tx.CommitAsync(ct);
            await _tx.DisposeAsync();
            _tx = null;
        }

        public async Task RollbackAsync(CancellationToken ct = default)
        {
            if (_tx is null) return;
            await _tx.RollbackAsync(ct);
            await _tx.DisposeAsync();
            _tx = null;
        }

        public async ValueTask DisposeAsync()
        {
            if (_tx is not null)
            {
                await _tx.DisposeAsync();
                _tx = null;
            }
        }
    }
}

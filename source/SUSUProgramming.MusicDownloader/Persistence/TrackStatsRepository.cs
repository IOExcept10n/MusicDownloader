using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SUSUProgramming.MusicDownloader.Persistence
{
    public interface ITrackStatsRepository
    {
        Task<TrackStatsModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<TrackStatsModel?> GetByArtistAndTitleAsync(string artistString, string titleString, CancellationToken ct = default);
        Task<TrackStatsModel> GetOrCreateByArtistAndTitleAsync(string artistString, string titleString, Func<TrackStatsModel>? factory = null, CancellationToken ct = default);
        Task<IReadOnlyList<TrackStatsModel>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(TrackStatsModel entity, CancellationToken ct = default);
        Task<bool> UpdateAsync(TrackStatsModel entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<TrackStatsModel>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
        Task SaveAsync(CancellationToken ct = default);
    }

    public class TrackStatsRepository : ITrackStatsRepository
    {
        const int MaxRetries = 3;

        private readonly TracksDbContext _db;
        private static readonly SemaphoreSlim _writeSemaphore = new(1, 1);

        public TrackStatsRepository(TracksDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        private DbSet<TrackStatsModel> Set => _db.TrackStats;

        public async Task<TrackStatsModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await Set.FindAsync(new object[] { id }, ct).ConfigureAwait(false);
        }

        public async Task<TrackStatsModel?> GetByArtistAndTitleAsync(string artistString, string titleString, CancellationToken ct = default)
        {
            if (artistString is null) throw new ArgumentNullException(nameof(artistString));
            if (titleString is null) throw new ArgumentNullException(nameof(titleString));

            return await Set
                .AsNoTracking()
                .Where(t => t.ArtistString == artistString && t.TitleString == titleString)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Returns an existing TrackStatsModel matching artist and title, or creates, saves, and returns a new one.
        /// The optional factory can populate defaults for a new entity.
        /// Uses a simple transactional approach to avoid races in a single DB instance.
        /// For high-concurrency environments across multiple app instances consider a distributed lock or unique DB constraint.
        /// </summary>
        public async Task<TrackStatsModel> GetOrCreateByArtistAndTitleAsync(string artistString, string titleString, Func<TrackStatsModel>? factory = null, CancellationToken ct = default)
        {
            if (artistString is null) throw new ArgumentNullException(nameof(artistString));
            if (titleString is null) throw new ArgumentNullException(nameof(titleString));

            // Attempt fast read first
            var existing = await Set
                .Where(t => t.ArtistString == artistString && t.TitleString == titleString)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (existing != null) return existing;

            // Not found — create and save within a transaction to reduce race window
            var newEntity = factory?.Invoke() ?? new TrackStatsModel
            {
                Id = Guid.NewGuid(),
                ArtistString = artistString,
                TitleString = titleString,
                ListenCount = null,
                TrackPath = null,
            };

            // Ensure keys/fields are consistent with inputs
            newEntity.ArtistString = artistString;
            newEntity.TitleString = titleString;
            if (newEntity.Id == Guid.Empty) newEntity.Id = Guid.NewGuid();

            // Try insert; if unique constraint exists at DB level, handle conflict by re-querying
            using var tx = await _db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
            try
            {
                await Set.AddAsync(newEntity, ct).ConfigureAwait(false);
                await _db.SaveChangesAsync(ct).ConfigureAwait(false);
                await tx.CommitAsync(ct).ConfigureAwait(false);
                return newEntity;
            }
            catch (DbUpdateException)
            {
                // Likely a race caused another instance to insert — rollback and return the existing row
                await tx.RollbackAsync(ct).ConfigureAwait(false);

                var recheck = await Set
                    .Where(t => t.ArtistString == artistString && t.TitleString == titleString)
                    .FirstOrDefaultAsync(ct)
                    .ConfigureAwait(false);

                if (recheck != null) return recheck;

                // If still missing, rethrow to surface unexpected error
                throw;
            }
        }

        public async Task<IReadOnlyList<TrackStatsModel>> GetAllAsync(CancellationToken ct = default)
        {
            return await Set.AsNoTracking().ToListAsync(ct).ConfigureAwait(false);
        }

        public async Task AddAsync(TrackStatsModel entity, CancellationToken ct = default)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));
            if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();

            await Set.AddAsync(entity, ct).ConfigureAwait(false);
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }

        public async Task<bool> UpdateAsync(TrackStatsModel entity, CancellationToken ct = default)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));
            if (entity.Id == Guid.Empty) throw new ArgumentException("Id must be set for update.", nameof(entity));

            await _writeSemaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                var existing = await Set.FindAsync(new object[] { entity.Id }, ct).ConfigureAwait(false);
                if (existing == null) return false;

                existing.ArtistString = entity.ArtistString;
                existing.TitleString = entity.TitleString;
                existing.ListenCount = entity.ListenCount;
                existing.TrackPath = entity.TrackPath;

                // Retry loop for transient SQLITE_BUSY or write contention
                for (int attempt = 0; attempt < MaxRetries; attempt++)
                {
                    try
                    {
                        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
                        return true;
                    }
                    catch (DbUpdateException ex) when (IsSqliteBusy(ex) && attempt < MaxRetries - 1)
                    {
                        // exponential backoff
                        await Task.Delay((int)Math.Pow(2, attempt) * 100, ct).ConfigureAwait(false);

                        // re-query to ensure state is fresh before retrying
                        _db.ChangeTracker.Clear();
                        existing = await Set.FindAsync(new object[] { entity.Id }, ct).ConfigureAwait(false);
                        if (existing == null) return false;
                        existing.ArtistString = entity.ArtistString;
                        existing.TitleString = entity.TitleString;
                        existing.ListenCount = entity.ListenCount;
                        existing.TrackPath = entity.TrackPath;
                        continue;
                    }
                }

                return false;
            }
            finally
            {
                _writeSemaphore.Release();
            }
        }

        public async Task SaveAsync(CancellationToken ct = default) => await _db.SaveChangesAsync(ct);

        private static bool IsSqliteBusy(Exception ex)
        {
            // Best-effort detection: check inner exceptions/messages for SQLITE_BUSY
            while (ex != null)
            {
                if (ex.Message?.IndexOf("SQLITE_BUSY", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                ex = ex.InnerException!;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var existing = await Set.FindAsync(new object[] { id }, ct).ConfigureAwait(false);
            if (existing == null) return false;

            Set.Remove(existing);
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty) return false;
            return await Set.AsNoTracking().AnyAsync(x => x.Id == id, ct).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<TrackStatsModel>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            return await Set
                .AsNoTracking()
                .OrderBy(t => t.TitleString)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }
    }

}

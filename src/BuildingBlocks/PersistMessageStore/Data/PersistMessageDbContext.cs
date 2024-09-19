using BuildingBlocks.Core.Model;
using BuildingBlocks.PersistMessageStore.Data.Configurations;
using BuildingBlocks.PersistMessageStore.Interfaces;
using BuildingBlocks.PersistMessageStore.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.PersistMessageStore.Data
{
    public class PersistMessageDbContext
        (ILogger<PersistMessageDbContext> _logger)
        : DbContext, IPersistMessageDbContext
    {
        public DbSet<PersistMessage> PersistMessages => Set<PersistMessage>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new PersistMessageConfiguration());
            base.OnModelCreating(builder);
        }

        public Task ExecuteTransactionalAsync(CancellationToken cancellationToken = default)
        {
            var stragety = Database.CreateExecutionStrategy();

            return stragety.ExecuteAsync(async () =>
            {
                await using var transaction = await Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    await SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while saving changes, trying for rollback transaction");
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSaving();

            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Resolving concurrency conflicts.
                // Refs: https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations#resolving-concurrency-conflicts
                foreach (var entry in ex.Entries)
                {
                    var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

                    if (databaseValues == null)
                    {
                        _logger.LogError("The record no longer exists in the database, The record has been deleted by another user.");
                        throw;
                    }

                    // Refresh the original values to bypass next concurrency check
                    entry.OriginalValues.SetValues(databaseValues);
                }

                return await base.SaveChangesAsync(cancellationToken);
            }
        }

        public void CreatePersistMessageTable()
        {
            if (Database.GetPendingMigrations().Any())
            {
                throw new InvalidOperationException("Cannot create table if there are pending migrations.");
            }

            Database.ExecuteSqlRaw($@"
                CREATE TABLE IF NOT EXISTS persist_message (
                    id uuid NOT NULL,
                    data_type TEXT,
                    data TEXT,
                    created_at timestamp with time zone NOT NULL,
                    retry_count INTEGER NOT NULL DEFAULT 1,
                    message_status TEXT NOT NULL DEFAULT 'IN_PROGRESSING'::TEXT,
                    message_delivery_type TEXT NOT NULL DEFAULT 'OUTBOX'::TEXT,
                    version BIGINT NOT NULL,
                    CONSTRAINT PK_PERSIST_MESSAGE PRIMARY KEY (id)
                )");
        }

        private void OnBeforeSaving()
        {
            try
            {
                var entries = ChangeTracker.Entries<IVersion>();

                foreach (var entry in entries)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.Entity.Version = 1;
                            break;
                        case EntityState.Modified:
                            entry.Entity.Version++;
                            break;
                        case EntityState.Deleted:
                            entry.Entity.Version++;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving changes, trying for find version of record", ex);
            }
        }
    }
}

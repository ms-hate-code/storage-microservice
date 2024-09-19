using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace BuildingBlocks.Mongo
{
    public class MongoDbContext : IMongoDbContext
    {
        public IClientSessionHandle? Session { get; set; }
        public IMongoDatabase Database { get; }
        public IMongoClient MongoClient { get; }
        protected readonly IList<Func<Task>> _commands;
        private readonly ILogger<MongoDbContext> _logger;

        public MongoDbContext(IOptions<MongoOptions> options, ILogger<MongoDbContext> logger)
        {
            RegisterConventions();
            var mongoOptions = options.Value;
            MongoClient = new MongoClient(mongoOptions.ConnectionString);
            Database = MongoClient.GetDatabase(mongoOptions.DatabaseName);
            _commands = [];
            _logger = logger;
        }

        private static void RegisterConventions()
        {
            ConventionRegistry.Register(
                "conventions",
                new ConventionPack
                {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
                new IgnoreIfDefaultConvention(false)
                }, _ => true);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var result = _commands.Count;

            using (Session = await MongoClient.StartSessionAsync(cancellationToken: cancellationToken))
            {
                Session.StartTransaction();

                try
                {
                    var commandTasks = _commands.Select(c => c());

                    await Task.WhenAll(commandTasks);

                    await Session.CommitTransactionAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await Session.AbortTransactionAsync(cancellationToken);
                    _commands.Clear();
                    _logger.LogError(ex, "Error on save changes");
                    throw;
                }
            }

            _commands.Clear();
            return result;
        }

        public IMongoCollection<T> GetCollections<T>(string? name = null)
        {
            return Database.GetCollection<T>(name ?? typeof(T).Name);
        }

        public void AddCommand(Func<Task> func)
        {
            _commands.Add(func);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            Session = await MongoClient.StartSessionAsync(cancellationToken: cancellationToken);
            Session.StartTransaction();
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (Session is { IsInTransaction: true })
                await Session.CommitTransactionAsync(cancellationToken);

            Session?.Dispose();
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            await Session?.AbortTransactionAsync(cancellationToken)!;
        }
    }
}

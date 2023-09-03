using Microsoft.Extensions.Logging.Abstractions;
using NATS.Client.Core;
using System.Collections.Concurrent;

namespace RinhaDeBackend.Services
{
    public class PeopleSyncService : BackgroundService
    {
        private readonly ConcurrentDictionary<string, byte> _peopleByApelidoCache;
        private readonly ConcurrentDictionary<Guid, Pessoa> _peopleByIdCache;
        private readonly ConcurrentDictionary<string, Pessoa> _termoPessoaCache;
        private readonly ILogger<PeopleSyncService> _logger;

        public PeopleSyncService(
            ConcurrentDictionary<string, byte> peopleByApelidoCache,
            ConcurrentDictionary<Guid, Pessoa> peopleByIdCache,
            ConcurrentDictionary<string, Pessoa> termoPessoaCache,
            ILogger<PeopleSyncService> logger)
        {
            _peopleByApelidoCache = peopleByApelidoCache;
            _peopleByIdCache = peopleByIdCache;
            _termoPessoaCache = termoPessoaCache;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var thread = new Thread(async () =>
            {

                var natsOptions = NatsOptions.Default with
                {
                    Url = "nats:4222",
                    LoggerFactory = NullLoggerFactory.Instance,
                    ObjectPoolSize = 50000
                };

                var natsConnection = new NatsConnection(natsOptions);
                await natsConnection.ConnectAsync();

                await using var sub = await natsConnection.SubscribeAsync<Pessoa>("Creation", cancellationToken: stoppingToken);
                await foreach (var msg in sub.Msgs.ReadAllAsync(stoppingToken))
                {
                    var person = msg.Data;
                    var buscaStackValue = person.Stack == null ? "" : string.Join("", person.Stack.Select(s => s.ToString()));
                    var buscaValue = $"{person.Apelido}{person.Nome}{buscaStackValue}" ?? "";

                    _peopleByApelidoCache.TryAdd(person.Apelido, default);
                    _peopleByIdCache.TryAdd(person.Id.Value, person);
                    _termoPessoaCache.TryAdd(buscaValue, person);
                }
            })
            {
                IsBackground = true
            };
            thread.Start();
        }
    }
}


using Npgsql;
using System.Threading.Channels;

namespace RinhaDeBackend.Services
{
    public class PersonInsertService : BackgroundService
    {
        private readonly NpgsqlConnection _conn;
        private readonly Channel<Pessoa> _createPersonChannel;
        private readonly ILogger<PersonInsertService> _logger;

        public PersonInsertService(
            NpgsqlConnection conn,
            Channel<Pessoa> createPersonChannel,
            ILogger<PersonInsertService> logger)
        {
            _conn = conn ?? throw new ArgumentNullException(nameof(conn));
            _createPersonChannel = createPersonChannel ?? throw new ArgumentNullException(nameof(createPersonChannel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            bool connected = false;

            while (!connected)
            {
                try
                {
                    await _conn.OpenAsync();
                    connected = true;
                    _logger.LogInformation("connected to postgres!!! yey");
                }
                catch (NpgsqlException)
                {
                    _logger.LogWarning("retrying connection to postgres");
                    await Task.Delay(1_000);
                }
            }


            var people = new List<Pessoa>();

            while (!stoppingToken.IsCancellationRequested)
            {
                await foreach (var person in _createPersonChannel.Reader.ReadAllAsync())
                {
                    people.Add(person);

                    if (people.Count < 100)
                    {
                        continue;
                    }

                    try
                    {
                        var batch = _conn.CreateBatch();
                        var batchCommands = new List<NpgsqlBatchCommand>();
                        _logger.LogInformation("Starting to write in db");

                        foreach (var p in people)
                        {
                            var batchCmd = new NpgsqlBatchCommand("""
                                insert into pessoas
                                (id, nome, apelido, nascimento, stack)
                                values ($1, $2, $3, $4, $5);
                            """);

                            batchCmd.Parameters.AddWithValue(p.Id);
                            batchCmd.Parameters.AddWithValue(p.Nome);
                            batchCmd.Parameters.AddWithValue(p.Apelido);
                            batchCmd.Parameters.AddWithValue(p.Nascimento.Value);
                            var stack = p.Stack is not null && p.Stack.Any() ? string.Join(", ", p.Stack) : "";
                            batchCmd.Parameters.AddWithValue(stack);

                            batch.BatchCommands.Add(batchCmd);
                        }

                        await batch.ExecuteNonQueryAsync();
                        _logger.LogInformation("Insertion completed!");

                        people.Clear();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "erro no worker :)");
                    }
                }
            }
            await _conn.CloseAsync();
            await _conn.DisposeAsync();
        }
    }
}

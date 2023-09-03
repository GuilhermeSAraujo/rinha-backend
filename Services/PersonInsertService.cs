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

                    if (people.Count < 10)
                    {
                        continue;
                    }

                    try
                    {
                        using var writer = _conn.BeginBinaryImport("COPY pessoas (id, nome, apelido, nascimento, stack) FROM STDIN (FORMAT BINARY)");
                        _logger.LogInformation("Starting to write in db");

                        foreach (var p in people)
                        {
                            writer.StartRow();

                            writer.Write((Guid)p.Id);

                            writer.Write(p.Nome);

                            writer.Write(p.Apelido);

                            writer.Write(p.Nascimento.Value);

                            var stack = p.Stack is not null && p.Stack.Any() ? string.Join(", ", p.Stack) : "";
                            writer.Write(stack);
                        }

                        writer.Complete();
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

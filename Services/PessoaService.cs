using Npgsql;
using NpgsqlTypes;
using System.Collections.Concurrent;

namespace RinhaDeBackend.Services
{
    public class PessoaService : IPessoaService
    {
        private readonly NpgsqlConnection _conn;
        private readonly ILogger<PessoaService> _logger;
        public PessoaService(NpgsqlConnection conn, ILogger<PessoaService> logger)
        {
            _conn = conn;
            _logger = logger;
        }

        public async Task CriarPessoa(ConcurrentQueue<Pessoa> waitingForCreation)
        {
            bool connected = false;

            while (!connected)
            {
                try
                {
                    await _conn.OpenAsync();
                    connected = true;
                }
                catch (NpgsqlException)
                {
                    _logger.LogWarning("retrying connection to postgres");
                    await Task.Delay(1_000);
                }
            }

            using var writer = _conn.BeginBinaryImport("COPY pessoas (id, nome, apelido, nascimento, stack) FROM STDIN (FORMAT BINARY)");
            _logger.LogInformation("Starting to write in db");
            while (waitingForCreation.TryDequeue(out var person))
            {

                writer.StartRow();

                writer.Write((Guid)person.Id);

                writer.Write(person.Nome);

                writer.Write(person.Apelido);

                writer.Write(person.Nascimento.Value);

                var stack = person.Stack is not null && person.Stack.Any() ? string.Join(", ", person.Stack) : "";
                writer.Write(stack);
            }

            writer.Complete();
            _logger.LogInformation("Transaction completed!");

        }

        public async Task<Pessoa?> BuscarPessoa(Guid id)
        {
            bool connected = false;

            while (!connected)
            {
                try
                {
                    await _conn.OpenAsync();
                    connected = true;
                }
                catch (NpgsqlException)
                {
                    _logger.LogWarning("retrying connection to postgres");
                    await Task.Delay(1_000);
                }
            }
            using var cmd = new NpgsqlCommand("SELECT id, nome, apelido, nascimento, stack from pessoas where id = $1", _conn);

            cmd.Parameters.AddWithValue(id);

            using var reader = await cmd.ExecuteReaderAsync();

            Pessoa? pessoa = null;
            if (await reader.ReadAsync())
            {
                pessoa = new Pessoa
                {
                    Id = reader.GetGuid(0),         // Assuming Id is in the first column (index 0)
                    Nome = reader.GetString(1),      // Assuming Nome is in the second column (index 1)
                    Apelido = reader.GetString(2),   // Assuming Apelido is in the third column (index 2)
                    Nascimento = DateOnly.FromDateTime(reader.GetDateTime(3)), // Assuming Nascimento is in the fourth column (index 3)
                    Stack = reader.GetString(4).Split(',').Select(element => element.Trim())      // Assuming Stack is in the fifth column (index 4)
                };

                return pessoa;
            }

            return pessoa;
        }

        public async Task<IEnumerable<Pessoa>> BuscarTermo(string termo)
        {
            bool connected = false;

            while (!connected)
            {
                try
                {
                    await _conn.OpenAsync();
                    connected = true;
                }
                catch (NpgsqlException)
                {
                    _logger.LogWarning("retrying connection to postgres");
                    await Task.Delay(1_000);
                }
            }
            using var cmd = new NpgsqlCommand("SELECT id, nome, apelido, nascimento, stack from pessoas where termo LIKE @termo LIMIT 50", _conn);
            cmd.Parameters.AddWithValue("@termo", "%" + termo + "%");

            //cmd.Parameters.AddWithValue(termo); 

            using var reader = await cmd.ExecuteReaderAsync();

            List<Pessoa> pessoas = new();

            if (await reader.ReadAsync())
            {
                var p = new Pessoa
                {
                    Id = reader.GetGuid(0),         // Assuming Id is in the first column (index 0)
                    Apelido = reader.GetString(1),   // Assuming Apelido is in the third column (index 2)
                    Nome = reader.GetString(2),      // Assuming Nome is in the second column (index 1)
                    Nascimento = DateOnly.FromDateTime(reader.GetDateTime(3)), // Assuming Nascimento is in the fourth column (index 3)
                    Stack = reader.GetString(4).Split(',').Select(element => element.Trim())      // Assuming Stack is in the fifth column (index 4)
                };

                pessoas.Add(p);
            }

            return pessoas;
        }
    }
}

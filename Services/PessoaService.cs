using Npgsql;

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

        public async Task<Pessoa> CriarPessoa(Pessoa pessoa)
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
            var cmd = new NpgsqlCommand("INSERT INTO pessoas (id, nome, apelido, nascimento, stack) VALUES ($1, $2, $3, $4, $5)", _conn);

            cmd.Parameters.AddWithValue(pessoa.Id);
            cmd.Parameters.AddWithValue(pessoa.Nome);
            cmd.Parameters.AddWithValue(pessoa.Apelido);
            cmd.Parameters.AddWithValue(pessoa.Nascimento);
            cmd.Parameters.AddWithValue(pessoa.Stack != null && pessoa.Stack.Count() > 0 ? string.Join(",", pessoa.Stack) : "");

            await cmd.ExecuteNonQueryAsync();

            return pessoa;
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
                    _logger.LogInformation("connected to postgres!!! yey");
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
                    Apelido = reader.GetString(1),   // Assuming Apelido is in the third column (index 2)
                    Nome = reader.GetString(2),      // Assuming Nome is in the second column (index 1)
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
                    _logger.LogInformation("connected to postgres!!! yey");
                }
                catch (NpgsqlException)
                {
                    _logger.LogWarning("retrying connection to postgres");
                    await Task.Delay(1_000);
                }
            }
            using var cmd = new NpgsqlCommand("SELECT id, nome, apelido, nascimento, stack from pessoas where termo ILIKE @termo", _conn);
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

using Npgsql;
using RinhaDeBackend.Models;

namespace RinhaDeBackend.UseCases.CriarPessoa
{
    public class CriarPessoa : ICriarPessoa
    {
        private readonly NpgsqlConnection _conn;

        public CriarPessoa(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        public async Task Execute(Pessoa pessoa)
        {
            await _conn.OpenAsync();
            await using var cmd = _conn.CreateCommand();

            var sql = $"INSERT INTO Person (apelido, nome, nascimento, stack) VALUES (${pessoa.Apelido}, ${pessoa.Nome}, ${pessoa.DataNascimento});";

            cmd.CommandText = sql;
            var r = await cmd.ExecuteScalarAsync();

            await _conn.CloseAsync();


            //using var cmd = new NpgsqlCommand(sql, _conn);

            //var response = cmd.ExecuteScalar().ToString();

            //Console.WriteLine(response);
        }
    }
}

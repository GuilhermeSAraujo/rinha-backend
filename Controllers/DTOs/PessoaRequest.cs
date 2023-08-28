using RinhaDeBackend.Models;

namespace RinhaDeBackend.Controllers.DTOs
{
    public class PessoaRequest
    {
        public string Nome { get; set; }
        public string Apelido { get; set; }
        public string Nascimento   { get; set; }
        public IEnumerable<string> Stacks { get; set; }

        public static Pessoa ParsePessoa(PessoaRequest p)
        {
            return new Pessoa
            {
                Nome = p.Nome,
                Apelido = p.Apelido,
                DataNascimento = p.Nascimento,
                Stacks = p.Stacks
            };
        }
    }
}

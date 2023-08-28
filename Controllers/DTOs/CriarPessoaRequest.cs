using RinhaDeBackend.Models;

namespace RinhaDeBackend.Controllers.DTOs
{
    public class CriarPessoaRequest
    {
        public string Nome { get; set; }
        public string Apelido { get; set; }
        public string Nascimento   { get; set; }
        public IEnumerable<string> Stacks { get; set; } = Enumerable.Empty<string>();

        public static Pessoa ParsePessoa(CriarPessoaRequest p)
        {
            return new Pessoa
            {
                Nome = p.Nome,
                Apelido = p.Apelido,
                DataNascimento = p.Nascimento,
                Stacks = p.Stacks.Any() ? string.Join(", ", p.Stacks) : null
            };
        }

        public bool ValidarRequest()
        {
            if(string.IsNullOrEmpty(this.Nome)) return false;
            if(string.IsNullOrEmpty(this.Apelido)) return false;
            if(string.IsNullOrEmpty(this.Nascimento)) return false;
            return true;
        }
    }
}

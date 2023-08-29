using RinhaDeBackend.Models;

namespace RinhaDeBackend.Controllers.DTOs
{
    public class CriarPessoaRequest
    {
        public string Nome { get; set; }
        public string Apelido { get; set; }
        public string Nascimento   { get; set; }
        public IEnumerable<string> Stack { get; set; } = Enumerable.Empty<string>();

        public static Pessoa ParsePessoa(CriarPessoaRequest p)
        {
            return new Pessoa
            {
                Nome = p.Nome,
                Apelido = p.Apelido,
                DataNascimento = p.Nascimento,
                Stacks = p.Stack.Any() ? string.Join(", ", p.Stack) : null
            };
        }

        public bool ValidarRequest()
        {
            if(string.IsNullOrEmpty(this.Nome) || this.Nome.Any(char.IsDigit)) return false;
            if(string.IsNullOrEmpty(this.Apelido) || this.Apelido.Any(char.IsDigit)) return false;
            if(string.IsNullOrEmpty(this.Nascimento) || this.Nascimento.Any(char.IsAsciiLetter)) return false;
            return true;
        }
    }
}

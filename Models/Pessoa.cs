namespace RinhaDeBackend.Models
{
    public class Pessoa
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Apelido { get; set; }
        public string DataNascimento { get; set; }
        public string? Stacks { get; set; }
        public string Termo { get; set; }
    }
}

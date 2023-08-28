namespace RinhaDeBackend.Models
{
    public class Pessoa
    {
        //private string _nome { get; set; }
        //private string _apelido { get; set; }
        //private DateTime _dataNascimento { get; set; }
        //private IEnumerable<string> _stacks { get; set; }
        public string Nome { get; set; }
        public string Apelido { get; set; }
        public string DataNascimento { get; set; }
        public IEnumerable<string> Stacks { get; set; }

        //public Pessoa(
        //    string nome,
        //    string apelido,
        //    string dataNascimento,
        //    IEnumerable<string> stacks)
        //{
        //    _nome = nome;
        //    _apelido = apelido;
        //}
    }
}

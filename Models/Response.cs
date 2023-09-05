using System.Text.Json.Serialization;

namespace RinhaDeBackend
{

    public class Pessoa
    {
        public Guid? Id { get; set; }

        public string? Apelido { get; set; }

        public string? Nome { get; set; }

        public DateOnly? Nascimento { get; set; }

        public IEnumerable<string>? Stack { get; set; }

        public override string ToString()
        {
            string s = string.Empty;
            if (this.Nome is not null)
                s += this.Nome + " | ";

            if (this.Apelido is not null)
                s += this.Apelido + " | ";

            if (this.Nascimento is not null)
                s += this.Nascimento + " | ";

            if (this.Stack is not null)
                s += string.Join(", ", this.Stack);

            return s;
        }

        internal static bool HasInvalidBody(Pessoa pessoa)
        {
            var atributosInvalidos = !pessoa.Nascimento.HasValue
                                || string.IsNullOrEmpty(pessoa.Nome)
                                || pessoa.Nome.Length > 100
                                || string.IsNullOrEmpty(pessoa.Apelido)
                                || pessoa.Apelido.Length > 32;

            if (atributosInvalidos)
                return false;

            foreach (var item in pessoa.Stack ?? Enumerable.Empty<string>())
                if (item.Length > 32 || item.Length == 0)
                    return false;

            return true;
        }

        internal static bool IsBadRequest(Pessoa pessoa)
        {
            var badRequest = false;

            var stacksAsString = pessoa.Stack is not null && pessoa.Stack.Any() ? string.Join("", pessoa.Stack) : "";
            if (pessoa.Apelido.Length > 32 ||
                pessoa.Nome.Length > 100 ||
                pessoa.Nome.Any(char.IsDigit) ||
                stacksAsString.Any(char.IsDigit))
            {
                badRequest = true;
                if (badRequest)
                {
                    return badRequest;
                }
            }

            if (pessoa.Stack != null && pessoa.Stack.Any())
            {
                var isStackTooLong = pessoa.Stack?.Where(s => s.Length > 32);
                if (isStackTooLong != null && isStackTooLong.Any())
                    badRequest = true;
            }

            return badRequest;
        }
    }

    public abstract class Response
    {
        public string? Erro { get; set; }
    }

    public class ResponseBusca
        : Response
    {
        public const string RespostaErroString = "{\"Resultados\":[],\"Erro\":\"\\u0027t\\u0027 n\\uFFFDo informado\"}";
        public IEnumerable<Pessoa> Resultados { get; set; } = new List<Pessoa>();
    }

    public class ResponseCriacao
        : Response
    {
        public const string ResponseAfeString = "{\"Pessoa\":null,\"Erro\":\"afe...\"}";
        public const string DuplicatedResultString = "{\"Pessoa\":null,\"Erro\":\"esse apelido j\\uFFFD existe\"}";
        public Pessoa? Pessoa { get; set; }
    }

    public class ResponseConsulta
        : Response
    {
        public const string RespostaErroString = "{\"pessoa\":null,\"erro\":\"Oops\"}";
        public object? Pessoa { get; set; }
    }

    [JsonSerializable(typeof(Pessoa))]
    public partial class PersonContext : JsonSerializerContext { }
    [JsonSerializable(typeof(ResponseBusca))]
    public partial class ResponseBuscaContext : JsonSerializerContext { }
    [JsonSerializable(typeof(ResponseCriacao))]
    public partial class ResponseCriacaoContext : JsonSerializerContext { }
    [JsonSerializable(typeof(ResponseConsulta))]
    public partial class ResponseConsultaContext : JsonSerializerContext { }
}

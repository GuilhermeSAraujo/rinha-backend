using RinhaDeBackend.Models;

namespace RinhaDeBackend.UseCases.CriarPessoa
{
    public interface ICriarPessoa
    {
        Task Execute(Pessoa pessoa);
    }
}

using System.Collections.Concurrent;

namespace RinhaDeBackend.Services
{
    public interface IPessoaService
    {
        Task CriarPessoa(ConcurrentQueue<Pessoa> waitingForCreation);
        Task<Pessoa?> BuscarPessoa(Guid id);
        Task<IEnumerable<Pessoa>> BuscarTermo(string termo);
        //Task<int> ContarPessoas();
    }
}

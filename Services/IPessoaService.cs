using RinhaDeBackend.Controllers.DTOs;
using RinhaDeBackend.Models;

namespace RinhaDeBackend.Services
{
    public interface IPessoaService
    {
        Task<Pessoa> BuscarPessoa(Guid id);
        Task<IEnumerable<Pessoa>> BuscarTermo(string termo);
        Task<Pessoa> CriarPessoa(CriarPessoaRequest pessoa);
        Task<int> ContarPessoas();
    }
}

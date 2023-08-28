using RinhaDeBackend.Controllers.DTOs;
using RinhaDeBackend.Models;

namespace RinhaDeBackend.Services
{
    public interface IPessoaService
    {
        Task<IEnumerable<Pessoa>> ListarPessoas();
        Task<Pessoa> BuscarPessoa(Guid id);
        Task<Pessoa> CriarPessoa(CriarPessoaRequest pessoa);
    }
}

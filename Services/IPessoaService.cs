namespace RinhaDeBackend.Services
{
    public interface IPessoaService
    {
        Task<Pessoa> CriarPessoa(Pessoa pessoa);
        Task<Pessoa?> BuscarPessoa(Guid id);
        Task<IEnumerable<Pessoa>> BuscarTermo(string termo);
        //Task<int> ContarPessoas();
    }
}

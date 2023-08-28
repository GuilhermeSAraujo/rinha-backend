using Microsoft.EntityFrameworkCore;
using RinhaDeBackend.Controllers.DTOs;
using RinhaDeBackend.Data;
using RinhaDeBackend.Models;

namespace RinhaDeBackend.Services
{
    public class PessoaService : IPessoaService
    {
        private readonly DataContext _context;
        public PessoaService(DataContext context)
        {
            _context = context;
        }

        public async Task<Pessoa> BuscarPessoa(Guid id)
        {
            return await _context.Pessoa.FindAsync(id);
        }

        public async Task<Pessoa> CriarPessoa(CriarPessoaRequest pessoa)
        {
            var p = await _context.Pessoa.AddAsync(CriarPessoaRequest.ParsePessoa(pessoa));
            await _context.SaveChangesAsync();
            return p.Entity;
        }

        public async Task<IEnumerable<Pessoa>> ListarPessoas()
        {
            return await _context.Pessoa.ToListAsync();
        }
    }
}

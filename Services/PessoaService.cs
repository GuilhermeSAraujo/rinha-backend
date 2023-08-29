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

        public async Task<Pessoa> CriarPessoa(CriarPessoaRequest pessoa)
        {
            var p = await _context.Pessoa.AddAsync(CriarPessoaRequest.ParsePessoa(pessoa));
            await _context.SaveChangesAsync();
            return p.Entity;
        }

        public async Task<Pessoa> BuscarPessoa(Guid id)
        {
             return await _context.Pessoa.FindAsync(id);
        }

        public async Task<IEnumerable<Pessoa>> BuscarTermo(string termo)
        {
            var termos = await _context.Pessoa.ToListAsync();
            return termos.Where(p =>
                (!string.IsNullOrEmpty(p.Stacks) && p.Stacks.Contains(termo)) ||
                p.Nome.Contains(termo) ||
                p.Apelido.Contains(termo)
            );
        }

        public async Task<int> ContarPessoas()
        {
            var r = await _context.Pessoa.FromSql($"SELECT COUNT(Id) FROM Pessoa").ToListAsync();
            return 100;   
        }
    }
}

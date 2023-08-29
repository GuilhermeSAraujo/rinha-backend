using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RinhaDeBackend.Controllers.DTOs;
using RinhaDeBackend.Services;

namespace RinhaDeBackend.Controllers
{
    [Route("pessoas")]
    [ApiController]
    public class PessoasController : ControllerBase
    {
        private readonly IPessoaService _pessoaService;
        private IMemoryCache _memoryCache;
        private readonly List<string> UsuarioCriado = new List<string>();

        public PessoasController(IPessoaService pessoaService, IMemoryCache memoryCache)
        {
            _pessoaService = pessoaService;
            _memoryCache = memoryCache;
        }

        [HttpPost]
        public async Task<IActionResult> CriarPessoa([FromBody] CriarPessoaRequest pessoa)
        {
            var pessoaValida = pessoa.ValidarRequest();

            if (pessoaValida && UsuarioCriado.Contains(pessoa.Nome))
            {
                return UnprocessableEntity();
            }

            if (pessoaValida)
            {
                var pessoaCriada = await _pessoaService.CriarPessoa(pessoa);
                UsuarioCriado.Add(pessoaCriada.Nome);
                return Created($"/pessoas/{pessoaCriada.Id}", pessoaCriada);
            }

            return UnprocessableEntity();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPessoa([FromRoute] Guid id)
        {
            var pessoa = await _memoryCache.GetOrCreate(id, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

               return await _pessoaService.BuscarPessoa(id);
            });

            if (pessoa is null)
                return NotFound();

            return Ok(pessoa);
        }

        [HttpGet]
        public async Task<IActionResult> ListarPorTermo([FromQuery] string? t)
        {
            if (string.IsNullOrEmpty(t))
            {
                return BadRequest();
            }

            var pessoas = await _memoryCache.GetOrCreate(t, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                return await _pessoaService.BuscarTermo(t);
            });

            return Ok(pessoas);

        }

        [HttpGet("contagem-pessoas")]
        public async Task<IActionResult> ContarPessoas()
        {
            return Ok(await _pessoaService.ContarPessoas());
        }
    }
}

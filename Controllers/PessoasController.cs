using Microsoft.AspNetCore.Mvc;
using RinhaDeBackend.Controllers.DTOs;
using RinhaDeBackend.Services;

namespace RinhaDeBackend.Controllers
{
    [Route("pessoas")]
    [ApiController]
    public class PessoasController : ControllerBase
    {
        private readonly IPessoaService _pessoaService;

        public PessoasController(IPessoaService pessoaService)
        {
            _pessoaService = pessoaService;
        }

        [HttpPost]
        public async Task<IActionResult> CriarPessoa([FromBody] CriarPessoaRequest pessoa)
        {
            var pessoaValida = pessoa.ValidarRequest();

            if (pessoaValida)
            {
                var pessoaCriada = await _pessoaService.CriarPessoa(pessoa);
                return Created($"/pessoas/{pessoaCriada.Id}", pessoaCriada);
            }

            return UnprocessableEntity();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPessoa([FromRoute] Guid id)
        {
            var pessoas = await _pessoaService.BuscarPessoa(id);

            if (pessoas is null)
                return NotFound();

            return Ok(pessoas);
        }

        [HttpGet]
        public async Task<IActionResult> ListarPorTermo([FromQuery] string? t)
        {
            if (!string.IsNullOrEmpty(t))
            {
                var pessoas = await _pessoaService.BuscarTermo(t);
                return Ok(pessoas);
            }

            return BadRequest();
        }

        [HttpGet("contagem-pessoas")]
        public async Task<IActionResult> ContarPessoas()
        {
            return Ok(await _pessoaService.ContarPessoas());
        }
    }
}

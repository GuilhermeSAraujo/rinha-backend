using Microsoft.AspNetCore.Mvc;
using RinhaDeBackend.Controllers.DTOs;
using RinhaDeBackend.Services;

namespace RinhaDeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PessoasController : ControllerBase
    {
        private readonly IPessoaService _pessoaService;

        public PessoasController(IPessoaService pessoaService)
        {
            _pessoaService = pessoaService;
        }

        [HttpGet]
        public async Task<IActionResult> ListarPessoas()
        {
            var pessoas = await _pessoaService.ListarPessoas();
            return Ok(pessoas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPessoa([FromRoute] Guid id)
        {
            var pessoas = await _pessoaService.BuscarPessoa(id);
            return Ok(pessoas);
        }

        [HttpPost]
        public async Task<IActionResult> CriarPessoa([FromBody] CriarPessoaRequest pessoa)
        {
            var p = await _pessoaService.CriarPessoa(pessoa);

            return Created($"/pessoas/{p.Id}", p);
        }
    }
}

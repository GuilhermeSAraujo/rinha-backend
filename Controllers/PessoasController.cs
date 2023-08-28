using Microsoft.AspNetCore.Mvc;
using RinhaDeBackend.Controllers.DTOs;
using RinhaDeBackend.UseCases.CriarPessoa;

namespace RinhaDeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PessoasController : ControllerBase
    {
        private readonly ICriarPessoa _criarPessoa;

        public PessoasController(ICriarPessoa criarPessoa)
        {
            _criarPessoa = criarPessoa;
        }

        [HttpPost]
        public async Task<IActionResult> CriarPessoa([FromBody] PessoaRequest pessoa)
        {
            await _criarPessoa.Execute(PessoaRequest.ParsePessoa(pessoa));
            return Ok();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Signa.Library.Core.Aspnet.Domain.Models;
using Signa.TemplateCore.Api.Business;
using Signa.TemplateCore.Api.Domain.Models;
using System.Collections.Generic;

namespace Signa.TemplateCore.Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Authorize("Bearer")]
    [AllowAnonymous]
    public class PessoaController : Controller
    {
        private readonly PessoaBL _pessoaBLL;

        public PessoaController(PessoaBL pessoaBLL)
        {
            _pessoaBLL = pessoaBLL;
        }

        [HttpPost]
        [Route("pessoa")]
        [ProducesResponseType(type: typeof(PessoaModel), statusCode: 200)]
        public IActionResult Insert(PessoaModel pessoa) => Ok(_pessoaBLL.Insert(pessoa));

        [HttpGet]
        [Route("pessoa/{id}")]
        [ProducesResponseType(type: typeof(PessoaModel), statusCode: 200)]
        public IActionResult GetById(int id) => Ok(_pessoaBLL.GetById(id));

        [HttpGet]
        [Route("pessoa")]
        [ProducesResponseType(type: typeof(IEnumerable<PessoaModel>), statusCode: 200)]
        public IActionResult Get() => Ok(_pessoaBLL.Get());

        [HttpDelete]
        [Route("pessoa/{id}")]
        [ProducesResponseType(type: typeof(MessageReturnModel), statusCode: 200)]
        public IActionResult DeletePessoa(int id)
        {
            _pessoaBLL.Delete(id);
            return Ok(new MessageReturnModel("Pessoa excluída com sucesso"));
        }
    }
}
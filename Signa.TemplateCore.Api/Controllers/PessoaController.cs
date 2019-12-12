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

        /// <summary>
        /// Grava ou atualiza dados de Pessoa
        /// </summary>
        [HttpPost]
        [Route("pessoa")]
        public ActionResult<PessoaModel> Insert(PessoaModel pessoa) => Ok(_pessoaBLL.Insert(pessoa));

        /// <summary>
        /// Busca uma pessoa através do ID
        /// </summary>
        /// <param name="id">ID da pessoa</param>
        [HttpGet]
        [Route("pessoa/{id}")]
        public ActionResult<PessoaModel> GetById(int id) => Ok(_pessoaBLL.GetById(id));

        /// <summary>
        /// Busca todas as pessoas na base de dados
        /// </summary>
        [HttpGet]
        [Route("pessoa")]
        public ActionResult<IEnumerable<PessoaModel>> Get() => Ok(_pessoaBLL.Get());

        /// <summary>
        /// Exclui uma pessoa através do ID
        /// </summary>
        /// <param name="id">ID da pessoa</param>
        [HttpDelete]
        [Route("pessoa/{id}")]
        public ActionResult<MessageReturnModel> DeletePessoa(int id)
        {
            _pessoaBLL.Delete(id);
            return Ok(new MessageReturnModel("Pessoa excluída com sucesso"));
        }
    }
}
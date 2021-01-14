using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Signa.Library.Core.Aspnet.Domain.Models;
using Signa.FinalTest.Api.Business;
using Signa.FinalTest.Api.Domain.Models;
using System.Collections.Generic;

namespace Signa.FinalTest.Api.Controllers
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
        /// <remarks>
        /// Exemplo gravação:
        ///
        ///     POST /pessoa
        ///     {
        ///        "nome": "Teste",
        ///        "nomeFantasia": "Teste",
        ///        "cnpjCpf": "000.000.000-00",
        ///        "email": "teste@teste.com.br",
        ///        "dataNascimento": "2019-12-12T14:03:28.166Z"
        ///     }
        ///
        /// Exemplo atualização:
        ///
        ///     POST /pessoa
        ///     {
        ///        "id": "1",
        ///        "nome": "Teste",
        ///        "nomeFantasia": "Teste",
        ///        "cnpjCpf": "000.000.000-00",
        ///        "email": "teste@teste.com.br",
        ///        "dataNascimento": "2019-12-12T14:03:28.166Z"
        ///     }
        ///
        /// </remarks>
        /// <returns>Dados da pessoa incluída ou atualizada</returns>
        /// <response code="200">Pessoa criada ou atualizada</response>
        /// <response code="404">Erro na inserção ou atualização, pessoa não encontrada na busca para retornar os dados</response>
        //[HttpPost]
        //[Route("pessoa")]
        //public ActionResult<PessoaModel> Insert(PessoaModel pessoa) => Ok(_pessoaBLL.Insert(pessoa));

        /// <summary>
        /// Busca uma pessoa através do ID
        /// </summary>
        /// <param name="id">ID da pessoa</param>
        /// <returns>Dados da pessoa</returns>
        /// <response code="200">Pessoa cadastrada na base de dados</response>
        /// <response code="404">Pessoa não encontrada na busca</response>
        //[HttpGet]
        //[Route("pessoa/{id}")]
        //public ActionResult<PessoaModel> GetById(int id) => Ok(_pessoaBLL.GetById(id));

        /// <summary>
        /// Busca todas as pessoas na base de dados
        /// </summary>
        /// <response code="200">Pessoas cadastradas na base de dados</response>
        /// <response code="404">Nenhuma pessoa encontrada na base de dados</response>
        //[HttpGet]
        //[Route("pessoa")]
        //public ActionResult<IEnumerable<PessoaModel>> Get() => Ok(_pessoaBLL.Get());

        /// <summary>
        /// Exclui uma pessoa através do ID
        /// </summary>
        /// <param name="id">ID da pessoa</param>
        /// <response code="200">Mensagem de pessoa excluída com sucesso</response>
        //[HttpDelete]
        //[Route("pessoa/{id}")]
        //public ActionResult<MessageReturnModel> DeletePessoa(int id)
        //{
        //    _pessoaBLL.Delete(id);
        //    return Ok(new MessageReturnModel("Pessoa excluída com sucesso"));
        //}
    }
}
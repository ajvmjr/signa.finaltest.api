using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Signa.FinalTest.Api.Business;
using Signa.FinalTest.Api.Domain.Models.Request;
using Signa.FinalTest.Api.Domain.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Signa.FinalTest.Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Authorize("Bearer")]
    [AllowAnonymous]
    public class RepositoryController : ControllerBase
    {
        private readonly RepositoryBL _repositoryBL;

        public RepositoryController(RepositoryBL repositoryBL)
        {
            _repositoryBL = repositoryBL;

        }

        /// <summary>
        /// Lista todos os repositórios.
        /// </summary>
        /// <param name="q"></param>
        /// <returns>Lista dos repositórios encontrados.</returns>
        /// <response code="200">Repositórios encontrados.</response>
        /// <response code="404">Mensagem de erro ao tentar obter os repositórios.</response>
        [HttpGet]
        [ProducesResponseType(typeof(RepositoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [Route("get/all/repositories")]
        public ActionResult<IEnumerable<RepositoryResponse>> GetAllRepositories()
        {
            var response = _repositoryBL.GetAllRepositories();

            if(response != null)
            {
                return Ok(response);
            }
            else
            {
                return NotFound(new { message = "Nenhum repositório foi encontrado." });
            }
        }

        /// <summary>
        /// Busca o repositório através de uma query.
        /// </summary>
        /// <param name="q"></param>
        /// <returns>Lista dos repositórios encontrados.</returns>
        /// <response code="200">Repositórios encontrados.</response>
        /// <response code="404">Mensagem de erro ao tentar obter os repositórios.</response>
        [HttpGet]
        [ProducesResponseType(typeof(RepositoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [Route("get/repository/{q?}")]
        public ActionResult<IEnumerable<RepositoryResponse>> GetRepositoriesByQuery(string? q)
        {
            if(q == "" || q == null)
            {
                var standardResponse = _repositoryBL.GetAllRepositories();
                if(standardResponse != null)
                {
                    return Ok(standardResponse);
                }
                else
                {
                    return NotFound(new { message = "Nenhum repositório foi encontrado." });
                }
            }

            var response = _repositoryBL.GetRepositoriesByQuery(q);
            if(response != null)
            {
                return Ok(response);
            }
            else
            {
                return NotFound(new { message = "Nenhum repositório foi encontrado." });
            }
        }

        /// <summary>
        /// Busca os repositórios starred de determinado usuário.
        /// </summary>
        /// <param name="q"></param>
        /// <returns>Lista dos repositórios starred.</returns>
        /// <response code="200">Número com confirmação de quantos repositórios foram adicionados no banco.</response>
        /// <response code="404">Mensagem de erro ao tentar obter os repositórios.</response>
        [HttpGet]
        [ProducesResponseType(typeof(RepositoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HttpResponseMessage), StatusCodes.Status400BadRequest)]
        [Route("get/starred/{q}")]
         async public Task<ActionResult<List<RepositoryResponse>>> GetRepositories(string q)
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");

            HttpResponseMessage response = await client.GetAsync($"users/{q}/starred");
            
            if(response.IsSuccessStatusCode)
            {
                var repositories = await response.Content.ReadAsAsync<List<RepositoryRequest>>();
                var affectedRows = _repositoryBL.InsertRepositories(repositories);
                if(affectedRows != 0)
                {
                    var allRepositories = _repositoryBL.GetAllRepositories();
                    return Ok(allRepositories);
                }
            }
            return BadRequest(response);
        }

        /// <summary>
        /// Insere tags a um repositório.
        /// </summary>
        /// <param name="tagRequest"></param>
        /// <returns>Mensagem com feedback do processo de inserção.</returns>
        /// <returns code="200">Mensagem confirmando a inserção.</returns>
        /// <returns code="404">Mensagem de erro.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [Route("insert/repository/tag")]
        public ActionResult InsertTag([FromBody] RepositoryTagRequest tagRequest)
        {
            var affectedRows = _repositoryBL.InsertTag(tagRequest);

            if(affectedRows == 1)
            {
                return Ok(new { message = "As tags foram cadastradas com sucesso." });
            }
            else
            {
                return BadRequest(new { message = "Erro ao cadastrar as tags" });
            }
        }

        /// <summary>
        /// Atualiza tags de um repositório.
        /// </summary>
        /// <param name="tagRequest"></param>
        /// <returns>Mensagem com feedback do processo de atualização.</returns>
        /// <returns code="200">Mensagem confirmando a atualização.</returns>
        /// <returns code="404">Mensagem de erro.</returns>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [Route("update/repository/tag")]
        public ActionResult UpdateTag([FromBody] RepositoryTagRequest tagRequest)
        {
            var affectedRows = _repositoryBL.UpdateTag(tagRequest);

            if (affectedRows == 1)
            {
                return Ok(new { message = "Tags atualizadas com sucesso." });
            }
            else
            {
                return BadRequest(new { message = "Erro ao atualizar as tags." });
            }
        }

        /// <summary>
        /// Deleta as tags de um repositório.
        /// </summary>
        /// <param name="tagRequest"></param>
        /// <returns>Mensagem com feedback do processo de atualização.</returns>
        /// <returns code="200">Mensagem confirmando o delete.</returns>
        /// <returns code="404">Mensagem de erro.</returns>
        [HttpDelete]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [Route("delete/repository/tag")]
        public ActionResult DeleteTag([FromBody] RepositoryTagRequest tagRequest)
        {
            var affectedRows = _repositoryBL.DeleteTag(tagRequest);

            if (affectedRows == 1)
            {
                return Ok(new { message = "Tags deletadas com sucesso." });
            }
            else
            {
                return BadRequest(new { message = "Erro ao deletar as tags." });
            }
        }
    }
}

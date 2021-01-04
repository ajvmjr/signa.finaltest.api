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
        public class PermissaoUsuarioController : Controller
    {
        private readonly PermissaoUsuarioBL _permissaoUsuarioBLL;

        public PermissaoUsuarioController(PermissaoUsuarioBL permissaoUsuarioBLL)
        {
            _permissaoUsuarioBLL = permissaoUsuarioBLL;
        }

        /// <summary>
        /// Busca permissão do usuário logado na base de dados
        /// </summary>
        /// <response code="200">Pessoas cadastradas na base de dados</response>
        /// <response code="404">Nenhuma pessoa encontrada na base de dados</response>
        [HttpGet]
        [Route("permissao")]
        public ActionResult<PermissaoUsuarioModel> Get() => Ok(_permissaoUsuarioBLL.Get());

        
    }
}

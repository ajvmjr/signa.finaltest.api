using AutoMapper;
using Signa.Library.Core;
using Signa.Library.Core.Exceptions;
using Signa.Library.Core.Extensions;
using Signa.FinalTest.Api.Data.Repository;
using Signa.FinalTest.Api.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signa.FinalTest.Api.Business
{
    public class PermissaoUsuarioBL
    {
        private readonly IMapper _mapper;
        private readonly PermissaoUsuarioDAO _permissaoUsuarioDAO;
        private readonly int _usuarioId;
        private readonly int _funcaoId;

        public PermissaoUsuarioBL(
            IMapper mapper,
            PermissaoUsuarioDAO permissaoUsuarioDAO
        )
        {
            _mapper = mapper;
            _permissaoUsuarioDAO = permissaoUsuarioDAO;
            _usuarioId = Global.UsuarioId;
            _funcaoId = Global.FuncaoId;
        }


        public PermissaoUsuarioModel Get()
        {
            if (_usuarioId.IsZeroOrNull())
                throw new SignaSqlNotFoundException("Usuario não encontrado");

            var permissoes = _permissaoUsuarioDAO.Get(_usuarioId, _funcaoId);

            

            if (permissoes == null)
                throw new SignaSqlNotFoundException("Nenhuma permissao encontrada para essa funcao com este usuario");
            

            return _mapper.Map<PermissaoUsuarioModel>(permissoes);
        }
    }
}

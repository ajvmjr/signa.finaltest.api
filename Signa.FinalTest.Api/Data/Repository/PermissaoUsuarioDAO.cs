using Dapper;
using Signa.Library.Core.Data.Repository;
using Signa.FinalTest.Api.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signa.FinalTest.Api.Data.Repository
{
    public class PermissaoUsuarioDAO : RepositoryBase
    {
        public PermissaoUsuarioEntity Get(int usuarioId, int funcaoId)
        {
            var sql = @"
                    SELECT 
	                    GUF.FLAG_PERMISSAO_ACESSO,
	                    GUF.FLAG_PERMISSAO_EXCLUSAO,
	                    GUF.FLAG_PERMISSAO_GRAVACAO,
	                    GUF.FLAG_PERMISSAO_IMPRESSAO
                    FROM
	                    GRUPO_USUARIO_FUNCAO GUF
	                    INNER JOIN GRUPO_USUARIO GU ON GU.GRUPO_USUARIO_ID = GUF.GRUPO_USUARIO_ID 
	                    INNER JOIN USUARIO USU ON USU.GRUPO_USUARIO_ID = GU.GRUPO_USUARIO_ID
	                    INNER JOIN FUNCAO FUNC ON FUNC.FUNCAO_ID = GUF.FUNCAO_ID
                    WHERE 
	                    USU.TAB_STATUS_ID = 1
	                    AND GUF.TAB_STATUS_ID = 1
	                    AND GU.TAB_STATUS_ID = 1
	                    AND FUNC.TAB_STATUS_ID = 1
	                    AND USU.USUARIO_ID = @usuarioId
	                    AND GUF.FUNCAO_ID = @funcaoId";

            var param = new
            {
                usuarioId,
                funcaoId
            };

            using (var db = Connection)
            {
                return db.QueryFirstOrDefault<PermissaoUsuarioEntity>(sql, param);
            }
        }
    }
}

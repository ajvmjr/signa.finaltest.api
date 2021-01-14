using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signa.FinalTest.Api.Data.Entities
{
    public class PermissaoUsuarioEntity
    {
        public string FlagPermissaoAcesso { get; set; }
        public string FlagPermissaoExclusao { get; set; }
        public string FlagPermissaoGravacao { get; set; }
        public string FlagPermissaoImpressao { get; set; }
        
    }
}

using Microsoft.Extensions.Configuration;
using Signa.TemplateCore.Api.Data.Repository;

namespace Angis.Account.Api.Data.Repository
{
    public class PessoaDAO : RepositoryBase
    {
        public PessoaDAO(IConfiguration configuration)
        {
            base.configuration = configuration;
        }
    }
}
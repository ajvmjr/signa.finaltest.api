using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Signa.TemplateCore.Api.Data.Repository;
using Signa.TemplateCore.Api.Domain.Entities;

namespace Angis.Account.Api.Data.Repository
{
    public class PessoaDAO : RepositoryBase
    {
        public PessoaDAO(IConfiguration configuration)
        {
            base.configuration = configuration;
        }

        public int Insert(PessoaEntity pessoa)
        {
            throw new System.NotImplementedException();
        }

        public void Update(PessoaEntity pessoa)
        {
            throw new System.NotImplementedException();
        }

        public PessoaEntity GetById(int id)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PessoaEntity> Get()
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}
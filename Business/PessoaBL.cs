using System.Collections.Generic;
using Angis.Account.Api.Data.Repository;
using AutoMapper;
using Signa.TemplateCore.Api.Domain.Models;

namespace Signa.TemplateCore.Api.Business
{
    public class PessoaBL
    {
        private readonly IMapper _mapper;
        private readonly PessoaDAO _pessoaDAO;

        public PessoaBL(
            IMapper mapper,
            PessoaDAO pessoaDAO
        )
        {
            _mapper = mapper;
            _pessoaDAO = pessoaDAO;
        }

        public PessoaModel Insert(PessoaModel pessoa)
        {
            throw new System.NotImplementedException();
        }

        public PessoaModel GetById(int id)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PessoaModel> Get()
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}
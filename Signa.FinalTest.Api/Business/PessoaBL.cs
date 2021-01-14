using AutoMapper;
using Signa.Library.Core.Exceptions;
using Signa.Library.Core.Extensions;
using Signa.FinalTest.Api.Data.Repository;
using Signa.FinalTest.Api.Data.Entities;
using Signa.FinalTest.Api.Domain.Models;
using System.Collections.Generic;

namespace Signa.FinalTest.Api.Business
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
            int id = 0;
            var entidade = _mapper.Map<PessoaEntity>(pessoa);

            if (pessoa.Id.IsZeroOrNull())
            {
                id = _pessoaDAO.Insert(entidade);
            }
            else
            {
                id = pessoa.Id;
                _pessoaDAO.Update(entidade);
            }

            return GetById(id);
        }

        public PessoaModel GetById(int id)
        {
            var pessoa = _pessoaDAO.GetById(id);

            if (pessoa == null)
            {
                throw new SignaSqlNotFoundException("Nenhuma pessoa encontrada com esse id");
            }

            return _mapper.Map<PessoaModel>(pessoa);
        }

        public IEnumerable<PessoaModel> Get()
        {
            var results = _pessoaDAO.Get();

            if (results.IsNullOrEmpty())
            {
                throw new SignaSqlNotFoundException("Nenhuma pessoa encontrada");
            }

            return _mapper.Map<IEnumerable<PessoaModel>>(results);
        }

        public void Delete(int id)
        {
            _pessoaDAO.Delete(id);
        }
    }
}
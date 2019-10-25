using System.Collections.Generic;
using AutoMapper;
using Signa.Library.Core.Extensions;
using Signa.TemplateCore.Api.Data.Repository;
using Signa.TemplateCore.Api.Domain.Entities;
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

            return _mapper.Map<PessoaModel>(GetById(id));
        }

        public PessoaModel GetById(int id) => _mapper.Map<PessoaModel>(_pessoaDAO.GetById(id));

        public IEnumerable<PessoaModel> Get() => _mapper.Map<IEnumerable<PessoaModel>>(_pessoaDAO.Get());

        public void Delete(int id)
        {
            _pessoaDAO.Delete(id);
        }
    }
}
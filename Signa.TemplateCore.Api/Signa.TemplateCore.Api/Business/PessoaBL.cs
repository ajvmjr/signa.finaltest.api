using AutoMapper;
using Signa.Library.Core.Exceptions;
using Signa.Library.Core.Extensions;
using Signa.TemplateCore.Api.Data.Repository;
using Signa.TemplateCore.Api.Domain.Entities;
using Signa.TemplateCore.Api.Domain.Models;
using System.Collections.Generic;

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

            return GetById(id);
        }

        public PessoaModel GetById(int id)
        {
            var pessoa = _mapper.Map<PessoaModel>(_pessoaDAO.GetById(id));

            if (pessoa == null)
            {
                throw new SignaSqlNotFoundException("Nenhuma pessoa encontrada com esse id");
            }

            return pessoa;
        }

        public IEnumerable<PessoaModel> Get() => _mapper.Map<IEnumerable<PessoaModel>>(_pessoaDAO.Get());

        public void Delete(int id)
        {
            _pessoaDAO.Delete(id);
        }
    }
}
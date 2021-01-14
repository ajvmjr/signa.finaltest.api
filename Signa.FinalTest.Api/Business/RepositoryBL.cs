using AutoMapper;
using Signa.FinalTest.Api.Data.Entities;
using Signa.FinalTest.Api.Data.Repository;
using Signa.FinalTest.Api.Domain.Models.Request;
using Signa.FinalTest.Api.Domain.Models.Response;
using Signa.Library.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Signa.FinalTest.Api.Business
{
    public class RepositoryBL
    {
        private readonly IMapper _mapper;
        private readonly RepositoryDAO _repositoryDAO;

        public RepositoryBL(IMapper mapper, RepositoryDAO repositoryDAO)
        {
            _mapper = mapper;
            _repositoryDAO = repositoryDAO;
        }

        public IEnumerable<RepositoryResponse> GetAllRepositories()
        {
            var repositoriesListEntity = _repositoryDAO.GetAllRepositories();
            return repositoriesListEntity.Select(x => _mapper.Map<RepositoryResponse>(x));
        }

        public IEnumerable<RepositoryResponse> GetRepositoriesByQuery(string q)
        {
            var repositoriesListEntity = _repositoryDAO.GetRepositoriesByQuery(q);
            return repositoriesListEntity.Select(x => _mapper.Map<RepositoryResponse>(x));
        }

        public int InsertRepositories(List<RepositoryRequest> repositoriesList)
        {
            var repositoriesEntitiesList =  repositoriesList.Select(x => _mapper.Map<RepositoryEntity>(x));
            foreach(var repository in repositoriesEntitiesList)
            {
                _repositoryDAO.InsertRepositories(repository);
            }
            return repositoriesEntitiesList.Count();
        }

        public int InsertTag(RepositoryTagRequest repositoryTagRequest)
        {
            var repository = _repositoryDAO.GetRepositoryById(repositoryTagRequest.RepositoryId);

            if(repository.RepositoryTags != null) {
                string[] strTagsArray = repositoryTagRequest.RepositoryTags.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                string[] strCurrentTagsArray = repository.RepositoryTags.Split(", ", StringSplitOptions.RemoveEmptyEntries);

                foreach (string s in strTagsArray)
                {
                    if (strCurrentTagsArray.Contains(s))
                    {
                        throw new SignaRegraNegocioException($"Repositório já possui a tag {s}");
                    }
                    repositoryTagRequest.RepositoryTags = String.Join(", ", strTagsArray.Concat(strCurrentTagsArray));
                    var repoTagEntity = _mapper.Map<RepositoryEntity>(repositoryTagRequest);
                    return _repositoryDAO.InsertTag(repoTagEntity);
                }
            }

            var repositoryTagEntity = _mapper.Map<RepositoryEntity>(repositoryTagRequest);
            return _repositoryDAO.InsertTag(repositoryTagEntity);
        }

        public int UpdateTag(RepositoryTagRequest repositoryTagRequest)
        {
            var repository = _repositoryDAO.GetRepositoryById(repositoryTagRequest.RepositoryId);

            if(repository == null)
            {
                throw new SignaRegraNegocioException("Nenhum repositório foi encontrado.");
            }

            var repositoryEntity = _mapper.Map<RepositoryEntity>(repositoryTagRequest);
            return _repositoryDAO.UpdateTag(repositoryEntity);
        }

        public int DeleteTag(RepositoryTagRequest repositoryTagRequest)
        {
            var repository = _repositoryDAO.GetRepositoryById(repositoryTagRequest.RepositoryId);

            if (repository == null)
            {
                throw new SignaRegraNegocioException("Nenhum repositório foi encontrado.");
            }

            string[] strTagsArray = repositoryTagRequest.RepositoryTags.Split(", ", StringSplitOptions.RemoveEmptyEntries);
            string[] strCurrentTagsArray = repository.RepositoryTags.Split(", ", StringSplitOptions.RemoveEmptyEntries);

            foreach(string s in strTagsArray)
            {
                if(strCurrentTagsArray.Contains(s))
                {
                    strCurrentTagsArray = strCurrentTagsArray.Where(val => val != s).ToArray();
                }
            }

            repositoryTagRequest.RepositoryTags = String.Join(",", strCurrentTagsArray);
            var repositoryEntity = _mapper.Map<RepositoryEntity>(repositoryTagRequest);
            return _repositoryDAO.DeleteTag(repositoryEntity);
        }
    }
}

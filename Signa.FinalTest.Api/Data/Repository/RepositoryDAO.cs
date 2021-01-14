using Dapper;
using Signa.FinalTest.Api.Data.Entities;
using Signa.Library.Core.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signa.FinalTest.Api.Data.Repository
{
    public class RepositoryDAO : RepositoryBase
    {
        public IEnumerable<RepositoryEntity> GetAllRepositories()
        {
            using var db = Connection;

            var query = @"SELECT * FROM repositories;";

            return db.Query<RepositoryEntity>(query);
        }

        public IEnumerable<RepositoryEntity> GetRepositoriesByQuery(string q)
        {
            using var db = Connection;

            var query = @"SELECT * FROM repositories WHERE repository_tags like '%'+@q+'%';";

            return db.Query<RepositoryEntity>(query, new { q });
        }

        public RepositoryEntity GetRepositoryById(int id)
        {
            using var db = Connection;

            var query = "SELECT * FROM repositories WHERE repository_id = @id;";
            return db.QueryFirstOrDefault<RepositoryEntity>(query, new { id });
        }

        public int InsertRepositories(RepositoryEntity repositoryEntity)
        {
            using var db = Connection;

            var query = @"INSERT INTO repositories
                                (repository_id, repository_name, repository_description, repository_language, repository_urlhttp)
                            VALUES 
                                (@RepositoryId, @RepositoryName, @RepositoryDescription, @RepositoryLanguage, @RepositoryUrlhttp);
                            SELECT SCOPE_IDENTITY()";
            return db.ExecuteScalar<int>(query, new
            {
                repositoryEntity.RepositoryId,
                repositoryEntity.RepositoryName,
                repositoryEntity.RepositoryDescription,
                repositoryEntity.RepositoryLanguage,
                repositoryEntity.RepositoryUrlhttp
            });
        }

        public int InsertTag(RepositoryEntity repositoryEntity)
        {
            using var db = Connection;

            var query = @"UPDATE repositories
                            SET repository_tags = @RepositoryTags
                            WHERE repository_id = @RepositoryId;";
            return db.Execute(query, new 
            { 
                repositoryEntity.RepositoryTags,
                repositoryEntity.RepositoryId
            });
        }

        public int UpdateTag(RepositoryEntity repositoryEntity)
        {
            using var db = Connection;

            var query = @"UPDATE repositories
                            SET repository_tags = @RepositoryTags
                            WHERE repository_id = @RepositoryId;";
            return db.Execute(query, new
            {
                repositoryEntity.RepositoryTags,
                repositoryEntity.RepositoryId
            });
        }

        public int DeleteTag(RepositoryEntity repositoryEntity)
        {
            using var db = Connection;

            var query = @"UPDATE repositories
                            SET repository_tags = @RepositoryTags
                            WHERE repository_id = @RepositoryId;";
            return db.Execute(query, new
            {
                repositoryEntity.RepositoryTags,
                repositoryEntity.RepositoryId
            });
        }
    }
}

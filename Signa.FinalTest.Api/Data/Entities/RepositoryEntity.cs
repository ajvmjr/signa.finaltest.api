using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signa.FinalTest.Api.Data.Entities
{
    public class RepositoryEntity
    {
        public int RepositoryId { get; set; }
        public string RepositoryName { get; set; }
        public string RepositoryDescription { get; set; }
        public string RepositoryLanguage { get; set; }
        public string RepositoryTags{ get; set; }
        public string RepositoryUrlhttp { get; set; }
    }
}

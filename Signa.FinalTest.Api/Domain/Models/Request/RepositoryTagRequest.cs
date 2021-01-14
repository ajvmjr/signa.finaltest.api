using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signa.FinalTest.Api.Domain.Models.Request
{
    public class RepositoryTagRequest
    {
        public int RepositoryId { get; set; }
        public string RepositoryTags { get; set; }
    }
}

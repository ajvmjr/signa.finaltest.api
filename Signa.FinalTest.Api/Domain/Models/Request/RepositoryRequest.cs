using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signa.FinalTest.Api.Domain.Models.Request
{
    public class RepositoryRequest
    {
        public int Id { get; set; }
        public string Name{ get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public string html_url { get; set; }
    }
}

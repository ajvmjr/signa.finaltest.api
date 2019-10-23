using System;

namespace Signa.TemplateCore.Api.Domain.Entities
{
    public class PessoaEntity
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string NomeFantasia { get; set; }
        public string IndicativoPfPj { get; set; }
        public string PfCpf { get; set; }
        public string PjCnpj { get; set; }
        public string Email { get; set; }
        public DateTime DataNascimento { get; set; }
    }
}
using System;
using FluentValidation;

namespace Signa.TemplateCore.Api.Domain.Models
{
    public class PessoaModel
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public string NomeFantasia { get; set; }

        public string CnpjCpf { get; set; }
        
        public string Email { get; set; }
        public DateTime DataNascimento { get; set; }
        // DOC: você pode voltar a data no formato nativo - para utilizar objetos date - ou formatá-la
        public string DataNascimentoFormatada { get; set; }
    }

    // DOC: guia de utilização do FluentValidation https://fluentvalidation.net/start
    // DOC: funções de validação https://fluentvalidation.net/built-in-validators
    public class PessoaValidator : AbstractValidator<PessoaModel>
    {
        public PessoaValidator()
        {
            var tamanhoNome = 255;

            RuleFor(p => p.Nome).NotNull().MaximumLength(tamanhoNome);
            RuleFor(p => p.NomeFantasia).NotNull().MaximumLength(tamanhoNome);
            RuleFor(p => p.Email).NotNull().EmailAddress();
            RuleFor(p => p.CnpjCpf).NotNull().MaximumLength(19);
        }
    }
}
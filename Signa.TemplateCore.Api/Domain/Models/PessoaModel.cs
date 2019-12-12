using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Signa.TemplateCore.Api.Domain.Models
{
    public class PessoaModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Nome { get; set; }

        [Required]
        [MaxLength(255)]
        public string NomeFantasia { get; set; }

        [Required]
        [MaxLength(19)]
        public string CnpjCpf { get; set; }
        
        [Required]
        [EmailAddress]
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
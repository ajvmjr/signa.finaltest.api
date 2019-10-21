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
    }

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
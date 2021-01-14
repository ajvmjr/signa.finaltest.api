using FluentValidation;
using Signa.FinalTest.Api.Domain.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Signa.FinalTest.Api.Domain.Validators
{
    public class RepositoryTagValidator : AbstractValidator<RepositoryTagRequest>
    {
        public RepositoryTagValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.RepositoryId)
                .NotNull().WithMessage("Informe o id do repositório.")
                .GreaterThan(0).WithMessage("Informe o id do repositório.")
                .DependentRules(() =>
                {
                    RuleFor(x => x.RepositoryTags)
                    .NotEmpty().WithMessage("Informe o nome da tag desejada.")
                    .NotNull().WithMessage("Informe o nome da tag desejada.");
                });
        }
    }
}

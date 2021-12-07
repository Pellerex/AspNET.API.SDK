using Business.ViewModels;
using FluentValidation;

namespace Ecosystem.ML.LanguageProcessingApi.Validators
{
    public class LanguageProcessingRequestViewModelValidator : AbstractValidator<LanguageProcessingRequestViewModel>
    {
        public LanguageProcessingRequestViewModelValidator()
        {
            RuleFor(x => x.Context).NotEmpty();
            RuleFor(x => x.Question).NotEmpty();
        }
    }
}
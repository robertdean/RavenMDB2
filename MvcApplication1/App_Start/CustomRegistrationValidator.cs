using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

namespace MvcApplication1.App_Start
{
    public class CustomRegistrationValidator : RegistrationValidator
    {
        public CustomRegistrationValidator()
        {
            RuleSet(ApplyTo.Post, () =>
                {
                    RuleFor(x => x.DisplayName).NotEmpty();
                });
        }
    }
}
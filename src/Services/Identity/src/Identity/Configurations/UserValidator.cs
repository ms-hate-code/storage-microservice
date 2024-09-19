using Identity.Identity.Exceptions;
using Identity.Identity.Models;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;

namespace Identity.Configurations
{
    public class UserValidator(
        SignInManager<AppUser> _signInManager,
        UserManager<AppUser> _userManager
    ) : IResourceOwnerPasswordValidator
    {
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(context.UserName)
                    ?? throw new InvalidCredentialsException("Email is incorrect");

                context.Result = new GrantValidationResult(
                    subject: user.Id,
                    authenticationMethod: GrantType.ResourceOwnerPassword
                );
            }
            catch (Exception)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, "Invalid Credentials");

                throw;
            }
        }
    }
}

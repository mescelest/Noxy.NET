using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Models.Responses.Authentication;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Authentication;

public class FormModelAuthenticationSignIn : BaseFormAPIModel<AuthenticationSignInResponse>
{
    public override string APIEndpoint => "Authentication/SignIn";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [Required]
    [EmailAddress]
    [DisplayName(TextConstants.LabelFormEmail)]
    [Description(TextConstants.HelpFormConfirmPassword)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(12), MaxLength(512)]
    [DisplayName(TextConstants.LabelFormPassword)]
    [Description(TextConstants.HelpFormConfirmPassword)]
    public string Password { get; set; } = string.Empty;
}

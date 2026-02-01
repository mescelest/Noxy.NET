using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Models.Responses.Authentication;
using Noxy.NET.UI.Abstractions;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Authentication;

public class AuthenticationSignInFormModel : BaseFormAPIModel<AuthenticationSignInResponse>
{
    public override string APIEndpoint => "Authentication/SignIn";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [Required]
    [EmailAddress]
    [DisplayName("Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(12), MaxLength(512)]
    [DisplayName("Password")]
    public string Password { get; set; } = string.Empty;
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.Domain.Requests;

public class RequestAuthenticationSignIn : BaseRequestPost<ResponseAuthenticationSignIn>
{
    public override string APIEndpoint => "Authentication/SignIn";

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

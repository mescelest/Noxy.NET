using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Authentication;

namespace Noxy.NET.EntityManagement.Domain.Requests.Authentication;

public class RequestAuthenticationSignIn : BaseRequestPost<ResponseAuthenticationSignIn>
{
    public override string APIEndpoint => "authentication/signin";

    [Required]
    [EmailAddress]
    [DisplayName(ParameterTextConstants.LabelFormEmail)]
    [Description(ParameterTextConstants.HelpFormEmail)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(12), MaxLength(512)]
    [DisplayName(ParameterTextConstants.LabelFormPassword)]
    [Description(ParameterTextConstants.HelpFormPassword)]
    public string Password { get; set; } = string.Empty;
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Authentication;

namespace Noxy.NET.EntityManagement.Domain.Requests.Authentication;

public class RequestAuthenticationSignUp : BaseRequestPost<ResponseAuthenticationSignUp>
{
    public override string APIEndpoint => "Authentication";

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

    [Required]
    [PropertyMatchValidation(nameof(Password))]
    [DisplayName(ParameterTextConstants.LabelFormConfirmPassword)]
    [Description(ParameterTextConstants.HelpFormConfirmPassword)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

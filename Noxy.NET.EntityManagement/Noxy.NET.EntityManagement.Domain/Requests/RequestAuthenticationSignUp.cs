using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.Domain.Requests;

public class RequestAuthenticationSignUp : BaseRequestPost<ResponseAuthenticationSignUp>
{
    public override string APIEndpoint => "Authentication";

    [Required]
    [EmailAddress]
    [DisplayName(TextConstants.LabelFormEmail)]
    [Description(TextConstants.HelpFormEmail)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(12), MaxLength(512)]
    [DisplayName(TextConstants.LabelFormPassword)]
    [Description(TextConstants.HelpFormPassword)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [PropertyMatchValidation(nameof(Password))]
    [DisplayName(TextConstants.LabelFormConfirmPassword)]
    [Description(TextConstants.HelpFormConfirmPassword)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

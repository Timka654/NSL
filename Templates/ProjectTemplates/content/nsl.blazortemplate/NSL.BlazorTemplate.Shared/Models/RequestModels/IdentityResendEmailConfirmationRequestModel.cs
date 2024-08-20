using System.ComponentModel.DataAnnotations;

namespace NSL.BlazorTemplate.Shared.Models.RequestModels
{
    public partial class IdentityResendEmailConfirmationRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
    }
}
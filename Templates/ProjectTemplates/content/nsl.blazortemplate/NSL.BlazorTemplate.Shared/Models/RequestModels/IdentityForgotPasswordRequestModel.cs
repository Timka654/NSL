using System.ComponentModel.DataAnnotations;

namespace NSL.BlazorTemplate.Shared.Models.RequestModels
{
    public partial class IdentityForgotPasswordRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
    }
}
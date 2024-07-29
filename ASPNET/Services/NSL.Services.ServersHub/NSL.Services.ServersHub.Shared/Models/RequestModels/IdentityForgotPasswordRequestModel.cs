using System.ComponentModel.DataAnnotations;

namespace NSL.Services.ServersHub.Shared.Models.RequestModels
{
    public partial class IdentityForgotPasswordRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
    }
}
using System.ComponentModel.DataAnnotations;

namespace NSL.Services.ServersHub.Shared.Models.RequestModels
{
    public partial class IdentityExternalLoginRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
    }
}
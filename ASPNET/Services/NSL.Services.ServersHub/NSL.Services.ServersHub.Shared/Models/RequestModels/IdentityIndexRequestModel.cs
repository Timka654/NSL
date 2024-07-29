using System.ComponentModel.DataAnnotations;

namespace NSL.Services.ServersHub.Shared.Models.RequestModels
{
    public partial class IdentityIndexRequestModel
    {
        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }
    }
}
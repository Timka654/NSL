using System.ComponentModel.DataAnnotations;

namespace NSL.BlazorTemplate.Shared.Models.RequestModels
{
    public partial class IdentityLoginRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}

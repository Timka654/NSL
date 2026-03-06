using System.ComponentModel.DataAnnotations;

namespace NSL.BlazorTemplate.Shared.Models.RequestModels
{
    public partial class IdentityLoginWithRecoveryCodeRequestModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Recovery Code")]
        public string RecoveryCode { get; set; } = "";
    }
}
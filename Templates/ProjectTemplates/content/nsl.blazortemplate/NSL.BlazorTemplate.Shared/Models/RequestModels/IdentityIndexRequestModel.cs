using NSL.Generators.FillTypeGenerator.Attributes;
using System.ComponentModel.DataAnnotations;

namespace NSL.BlazorTemplate.Shared.Models.RequestModels
{
    [FillTypeGenerate(typeof(UserModel))]
    [FillTypeFromGenerate(typeof(UserModel))]
    public partial class IdentityIndexRequestModel
    {
        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }
    }
}
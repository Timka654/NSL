using System.ComponentModel.DataAnnotations;

namespace NSL.BlazorTemplate.Shared.Models.RequestModels
{
    public partial class IdentityDeletePersonalDataRequestModel
    {
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
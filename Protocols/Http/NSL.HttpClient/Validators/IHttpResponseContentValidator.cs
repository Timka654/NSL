using NSL.HttpClient.Models;

namespace NSL.HttpClient.Validators
{
    public interface IHttpResponseContentValidator
    {
        void DisplayApiErrors(BaseResponse response, bool reset = false);
    }
}

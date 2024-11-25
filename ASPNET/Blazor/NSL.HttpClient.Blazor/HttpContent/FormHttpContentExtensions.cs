using Microsoft.AspNetCore.Components.Forms;
using NSL.HttpClient.HttpContent;
using System.IO;
using System.Net.Http;

namespace NSL.HttpClient.Blazor.HttpContent
{
    public static class FormHttpContentExtensions
    {
        public static FormHttpContent AddFile(this FormHttpContent _content, IBrowserFile file, Stream content, string fieldName = "file")
        {
            _content.Add(new StreamContent(content), fieldName, file.Name);

            return _content;
        }
    }
}

﻿using DevExtensions.Blazor.Http.HttpContent;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NSL.HttpClient.HttpContent
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

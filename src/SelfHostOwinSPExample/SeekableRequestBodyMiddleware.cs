﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace SelfHostOwinSPExample
{
    public class SeekableRequestBodyMiddleware : OwinMiddleware
    {
        public SeekableRequestBodyMiddleware(OwinMiddleware next) : base(next) { }

        public async override Task Invoke(IOwinContext context)
        {
            ReplaceBodyWithMemoryStreamIfNotCanSeek(context.Request);
            await Next.Invoke(context);
        }
        /// <summary>
        /// In OWIN selfhost environment, Request.Body is using HttpRequestStream that is not rewindable, it is causing issues for other components in pipeline trying to access the body content.
        /// </summary>
        /// <param name="request"></param>
        private void ReplaceBodyWithMemoryStreamIfNotCanSeek(IOwinRequest request)
        {
            if (!request.Body.CanSeek)
            {
                var bodyStream = request.Body;
                request.Body = new MemoryStream();
                bodyStream.CopyTo(request.Body);
                request.Body.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}

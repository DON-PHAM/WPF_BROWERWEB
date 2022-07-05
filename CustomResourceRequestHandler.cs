namespace KDCLGD
{
    using CefSharp;
    using CefSharp.Handler;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CustomResourceRequestHandler : ResourceRequestHandler
    {
        private readonly string role;

        public CustomResourceRequestHandler(string role)
        {
            this.role = role;
        }
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            var headers = request.Headers;
            headers["RoleApp"] = role;
            request.Headers = headers;

            return CefReturnValue.Continue;
        }
    }

    public class CustomRequestHandler : RequestHandler
    {
        private readonly string role;

        public CustomRequestHandler(string role)
        {
            this.role = role;
        }
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return new CustomResourceRequestHandler(role);
        }
    }
}

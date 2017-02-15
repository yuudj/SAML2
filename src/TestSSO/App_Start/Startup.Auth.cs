using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Owin.Security.Saml;
using SAML2;
using SAML2.Config;

namespace TestSSO
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(SamlAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions {
                AuthenticationType = "SAML2", // Not available as a constant, strangely
                AuthenticationMode = AuthenticationMode.Active
            });

            Saml2Configuration saml2Configuration = new Saml2Configuration
            {
                ServiceProvider = new ServiceProvider {
                    SigningCertificate = new X509Certificate2(FileEmbeddedResource("TestSSO.sp.pfx"), "password"),
                    Server = "https://localhost:44338",
                    Id = "https://localhost:44338"
                },
                AllowedAudienceUris = new List<Uri>(new[] { new Uri("https://localhost:44338") })
            };

            // These URLs are intercepted by the library and handled appropriately
            saml2Configuration.ServiceProvider.Endpoints.AddRange(new[] {
                new ServiceProviderEndpoint(EndpointType.SignOn, "/saml2/login", "/"),
                new ServiceProviderEndpoint(EndpointType.Logout, "/saml2/logout", "/"),
                new ServiceProviderEndpoint(EndpointType.Metadata, "/saml2/metadata")
            });

            // testshib-providers.xml is not supported because it contains multiple <EntityDescription> elements
            if (!saml2Configuration.IdentityProviders.TryAddByMetadata(@"c:\users\anthony\source\SAML2\src\TestSSO\idpMetadata.xml"))
            {
                throw new ArgumentException("Invalid metadata file");
            }

            // I think this is a defect in the library.
            saml2Configuration.IdentityProviders.First().OmitAssertionSignatureCheck = true;
            saml2Configuration.LoggingFactoryType = "SAML2.Logging.DebugLoggerFactory";

            app.UseSamlAuthentication(
                new SamlAuthenticationOptions
                {
                    Configuration = saml2Configuration,
                    RedirectAfterLogin = "/"
                });
        }

        private byte[] FileEmbeddedResource(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = path;

            byte[] result = null;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                result = memoryStream.ToArray();
            }
            return result;
        }
    }
}
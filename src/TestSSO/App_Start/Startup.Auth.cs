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
        //private static string realm = ConfigurationManager.AppSettings["ida:Wtrealm"];
        //private static string adfsMetadata = ConfigurationManager.AppSettings["ida:ADFSMetadata"];

        ///// <summary>
        ///// SP Test certificate.
        ///// </summary>
        ///// <remarks>
        ///// Created using <code>makecert -n "CN=Test SP" -a sha256 -len2048 -pe -ss My -r -sky exchange</code>
        ///// then exporting the certificate.
        ///// </remarks>
        //private static string signingCertificate = @"            
        //    MIIDAzCCAeugAwIBAgIVAPX0G6LuoXnKS0Muei006mVSBXbvMA0GCSqGSIb3DQEB 
        //    CwUAMBsxGTAXBgNVBAMMEGlkcC50ZXN0c2hpYi5vcmcwHhcNMTYwODIzMjEyMDU0
        //    WhcNMzYwODIzMjEyMDU0WjAbMRkwFwYDVQQDDBBpZHAudGVzdHNoaWIub3JnMIIB
        //    IjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAg9C4J2DiRTEhJAWzPt1S3ryh
        //    m3M2P3hPpwJwvt2q948vdTUxhhvNMuc3M3S4WNh6JYBs53R+YmjqJAII4ShMGNEm
        //    lGnSVfHorex7IxikpuDPKV3SNf28mCAZbQrX+hWA+ann/uifVzqXktOjs6DdzdBn
        //    xoVhniXgC8WCJwKcx6JO/hHsH1rG/0DSDeZFpTTcZHj4S9MlLNUtt5JxRzV/MmmB 
        //    3ObaX0CMqsSWUOQeE4nylSlp5RWHCnx70cs9kwz5WrflnbnzCeHU2sdbNotBEeTH
        //    ot6a2cj/pXlRJIgPsrL/4VSicPZcGYMJMPoLTJ8mdy6mpR6nbCmP7dVbCIm/DQID
        //    AQABoz4wPDAdBgNVHQ4EFgQUUfaDa2mPi24x09yWp1OFXmZ2GPswGwYDVR0RBBQw
        //    EoIQaWRwLnRlc3RzaGliLm9yZzANBgkqhkiG9w0BAQsFAAOCAQEASKKgqTxhqBzR
        //    OZ1eVy++si+eTTUQZU4+8UywSKLia2RattaAPMAcXUjO+3cYOQXLVASdlJtt+8QP
        //    dRkfp8SiJemHPXC8BES83pogJPYEGJsKo19l4XFJHPnPy+Dsn3mlJyOfAa8RyWBS 
        //    80u5lrvAcr2TJXt9fXgkYs7BOCigxtZoR8flceGRlAZ4p5FPPxQR6NDYb645jtOT
        //    MVr3zgfjP6Wh2dt+2p04LG7ENJn8/gEwtXVuXCsPoSCDx9Y0QmyXTJNdV1aB0AhO
        //    RkWPlFYwp+zOyOIR+3m1+pqWFpn0eT/HrxpdKa74FA3R2kq4R7dXe4G0kUgXTdqX
        //    MLRKhDgdmA==";

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            Saml2Configuration saml2Configuration = new Saml2Configuration
            {
                ServiceProvider = new ServiceProvider {
                    SigningCertificate = new X509Certificate2(FileEmbeddedResource("TestSSO.sp.pfx"), "password"),
                    Server = "https://localhost:44338",
                    Id = "https://localhost:44338"
                },
                AllowedAudienceUris = new List<Uri>(new[] { new Uri("https://localhost:44338") })
            };
            saml2Configuration.ServiceProvider.Endpoints.AddRange(new[] {
                new ServiceProviderEndpoint
                {
                    Type = EndpointType.SignOn,
                    LocalPath  = "/account/signin",
                    RedirectUrl = "/account/signin"
                },
                new ServiceProviderEndpoint
                {
                    Type = EndpointType.Logout,
                    LocalPath  = "/account/signout",
                    RedirectUrl = "/account/signout"
                }
            });
            // testshib-providers.xml is not supported because it contains an <EntitiesDescription> element
            if (!saml2Configuration.IdentityProviders.TryAddByMetadata(Path.Combine(@"c:\users\anthony\source\SAML2\src\TestSSO\idpMetadata.xml")))
            {
                throw new ArgumentException("Invalid metadata file");
            }
            saml2Configuration.IdentityProviders.First().Default = true;
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
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = path;

            byte[] result = null;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                result = memoryStream.ToArray();
            }
            return result;
        }
    }
}
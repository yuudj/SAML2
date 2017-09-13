using System;
using System.Linq;
using Owin;
using SAML2.Config;
using System.IO;
using SAML2.Logging;
using Microsoft.Owin.Security;
using Owin.Security.Saml;
using System.Collections.Generic;
using System.Web.Http;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
//using System.Threading.Tasks;
//using System.Collections.Generic;

namespace SelfHostOwinSPExample
{
    internal partial class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = GetSamlConfiguration();

            //In OWIN selfhost environment, Request.Body is using HttpRequestStream that is not rewindable, it is causing issues for other components in pipeline trying to access the body content.
            appBuilder.Use<SeekableRequestBodyMiddleware>();

            appBuilder.UseCookieAuthentication(new Microsoft.Owin.Security.Cookies.CookieAuthenticationOptions
            {
                AuthenticationType = "SAML2",
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active
            });
            appBuilder.UseSamlAuthentication(new SamlAuthenticationOptions
            {
                Configuration = config,
                RedirectAfterLogin = "/core",
                LoginPath = "/identity/login",

            });

            appBuilder.UseWebApi(GetWebApiConfig());

            
            appBuilder.Run(async c =>
            {
                if (c.Authentication.User != null &&
                    c.Authentication.User.Identity != null &&
                    c.Authentication.User.Identity.IsAuthenticated)
                {
                    await c.Response.WriteAsync(c.Authentication.User.Identity.Name + "\r\n");
                    await c.Response.WriteAsync(c.Authentication.User.Identity.AuthenticationType + "\r\n");
                    foreach (var claim in c.Authentication.User.Identities.SelectMany(i => i.Claims))
                        await c.Response.WriteAsync(claim.Value + "\r\n");
                    await c.Response.WriteAsync("authenticated");
                }
                else
                {
                    // trigger authentication
                    c.Authentication.Challenge(c.Authentication.GetAuthenticationTypes().Select(d => d.AuthenticationType).ToArray());
                }
                return;
            });

        }



        private Saml2Configuration GetSamlConfiguration()
        {
            // TODO: ADD A CERTIFICATE HERE
            System.Security.Cryptography.X509Certificates.X509Certificate2 cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(@"C:\WorkingFolder\tmp\saml-idp\mycert.pfx", "fruta");

            var myconfig = new Saml2Configuration
            {
                ServiceProvider = new ServiceProvider
                {
                    SigningCertificate = cert,
                    Server = "http://localhost:7777/identity",
                    Id = "http://localhost:7777/identity",
                },
                LoggingFactoryType = typeof(LoggingFactory).AssemblyQualifiedName,
                AllowedAudienceUris = new List<Uri>
                                              {
                                                  new Uri("uri:pqs")// this must mathc with the IDP configuration
                                              }

            };
            SAML2.Logging.LoggerProvider.Configuration = myconfig;

            myconfig.ServiceProvider.Endpoints.AddRange(new[] {
                new ServiceProviderEndpoint(EndpointType.SignOn, "/identity/login", "/identity", BindingType.Redirect),
                new ServiceProviderEndpoint(EndpointType.Logout, "/identity/logout", "/identity", BindingType.Redirect),
                new ServiceProviderEndpoint(EndpointType.Metadata, "/identity/metadata")
            });



            myconfig.IdentityProviders.AddByMetadata(new string[] { @"Metadata/idp.metadata.xml" });

            myconfig.IdentityProviders[0].OmitAssertionSignatureCheck = true;
            SAML2.Logging.LoggerProvider.Configuration = myconfig;
            return myconfig;
        }
        private HttpConfiguration GetWebApiConfig() {
            HttpConfiguration config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            return config;
        }


        /// <summary>
        /// Console loguer for the SP
        /// </summary>
        public class LoggingFactory : ILoggerFactory
        {
            private static readonly IInternalLogger debugLogger = new DebugLogger();
            public IInternalLogger LoggerFor(Type type)
            {
                return debugLogger;
            }

            public IInternalLogger LoggerFor(string keyName)
            {
                return debugLogger;
            }
            private class DebugLogger : IInternalLogger
            {
                public bool IsDebugEnabled
                {
                    get
                    {
                        return true;
                    }
                }

                public bool IsErrorEnabled
                {
                    get
                    {
                        return true;
                    }
                }

                public bool IsFatalEnabled
                {
                    get
                    {
                        return true;
                    }
                }

                public bool IsInfoEnabled
                {
                    get
                    {
                        return true;
                    }
                }

                public bool IsWarnEnabled
                {
                    get
                    {
                        return true;
                    }
                }

                public void Debug(object message)
                {
                    Console.WriteLine(message);
                }

                public void Debug(object message, Exception exception)
                {
                    Console.WriteLine(message);
                }

                public void DebugFormat(string format, params object[] args)
                {
                    Console.WriteLine(string.Format(format, args));
                }

                public void Error(object message)
                {
                    Console.WriteLine(message);
                }

                public void Error(object message, Exception exception)
                {
                    Console.WriteLine(message);
                }

                public void ErrorFormat(string format, params object[] args)
                {
                    Console.WriteLine(string.Format(format, args));
                }

                public void Fatal(object message)
                {
                    Console.WriteLine(message);

                }

                public void Fatal(object message, Exception exception)
                {
                    Console.WriteLine(message);
                }

                public void Info(object message)
                {
                    Console.WriteLine(message);
                }

                public void Info(object message, Exception exception)
                {
                    Console.WriteLine(message);
                }

                public void InfoFormat(string format, params object[] args)
                {
                    Console.WriteLine(string.Format(format, args));
                }

                public void Warn(object message)
                {
                    Console.WriteLine(message);
                }

                public void Warn(object message, Exception exception)
                {
                    Console.WriteLine(message);
                }

                public void WarnFormat(string format, params object[] args)
                {
                    Console.WriteLine(string.Format(format, args));
                }
            }
        }
    }
}
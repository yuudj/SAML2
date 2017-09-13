using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;


namespace SelfHostOwinSPExample.Controllers.api
{
    [RoutePrefix("api/values")]
    [Authorize]
    public class ValuesController:ApiController
    {

        [HttpGet, Route("")]
        public string[] Get() {
            var claims = Request.GetOwinContext().Authentication.User.Claims.Select(c => c.Value);

            return  claims.ToArray() ;
        }
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace TestSSO.Models
{
    public class IndexModel
    {
        public ClaimsIdentity ClaimsIdentity { get; set; }
    }
}
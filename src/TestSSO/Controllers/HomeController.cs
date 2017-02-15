using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using TestSSO.Models;

namespace TestSSO.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            IndexModel indexModel = new IndexModel
            {
                ClaimsIdentity = User?.Identity as ClaimsIdentity
            };
            return View(indexModel);
        }
    }
}
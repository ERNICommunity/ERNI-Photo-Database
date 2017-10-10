using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ERNI.PhotoDatabase.Server.Controllers
{
    [Route("[controller]")]
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GameOfMemory.Web.Controllers
{
    public class HomeController : Controller
    {
  
		//Método do controlador que retorna a view principal..
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Test()
        {
            return View();
        }

    }
}

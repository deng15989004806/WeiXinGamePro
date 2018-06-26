using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GamePro.Controllers
{
    public class RoomController : Controller
    {
        // GET: Room
        //擂台
        public ActionResult Room()
        {
            return View();
        }
        //摆擂
        public ActionResult addUI()
        {
            return View();
        }
    }
}
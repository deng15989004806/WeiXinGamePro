using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using GamePro.App_Start;
using wxBase;
using wxBase.Model;
using wxBase.Model.Pay;
using GamePro.Models;
using ecd_csharp_demo;
using GamePro.ViewModel;
using GamePro.BaseFunction;
using System.Net;
using System.IO;

namespace GamePro.Controllers
{
    public class HomeController : Controller
    {
        

        // GET: Home
        GameWZEntities db = new GameWZEntities();
        public ActionResult Index()
        {
            //string echostring = Request.QueryString["echoStr"];
            //string signature = Request.QueryString["signature"];
            //string timestamp = Request.QueryString["timestamp"];
            //string nonce = Request.QueryString["nonce"];
            //string tmpsignature = Ycbase.makesignature(Ycbase.token, timestamp, nonce);
            //if (tmpsignature == signature && !string.IsNullOrEmpty(echostring))
            //{
            //    Response.Write(echostring);
            //    Response.End();
            //}
            //else
            //{
            //    Response.Write("Invalid request");
            //    Response.End();
            //}

            if (Request["code"] == null)
            {
                 string url = weixinService.OAuth2("http://www.abc.karslo.com/Home/Index", 0);
              
                System.Web.HttpContext.Current.Response.Redirect(url);
            }
            var result = weixinService.get_accesstoken_bycode(Request["code"]);
            if (result != null)
                HttpContext.Session.Add("OpenID", result.openid);   //获取openid
            if ((Session["id"] == null || string.IsNullOrEmpty(Session["id"].ToString())) && result.openid == null)
            {
                if (Session["OpenID"] != null)
                {
                    var user = db.User.FirstOrDefault(x => x.OpenID == Session["OpenID"].ToString());
                    if (user != null)
                    {
                        Session["id"] = user.ID;
                        Session["nickname"] = user.nickname;

                    }
                    else
                        return RedirectToAction("Login", "Home");
                }
                else
                    return RedirectToAction("Login", "Home");
            }
            else
                weixinService.AutoLogin(result.openid, Convert.ToInt32(Session["id"])); //当openid或用户ID有一个不为空时自动登录

            //return Content("用户" + Session["nickname"] + "登陆成功" + Session["OpenID"]);
            //wxModelMessage.sendMessage("", weixinService.Access_token + ","+ result.openid);
            //return Content(weixinService.Access_token + "," + result.openid);
            return View();
        }
        public ActionResult getaccesstoken()
        {
            Response.Write("access_token:"+Ycbase.Access_token);
            return View();
        }
        public ActionResult CreateMenu()
        {
            Response.Write(wxMenuService.Create(Server.MapPath("~/menu.txt")));
            return View();
        }
        public ActionResult JSPay()
        {
            string strBillNo = wxPayService.getTimestamp(); // 订单号
            string strWeixin_OpenID = "";  // 当前用户的openid
            string strCode = Request.QueryString["code"] == null ? "" : Request.QueryString["code"]; // 接收微信认证服务器发送来的code

         

            if (string.IsNullOrEmpty(strCode)) //如果接收到code，则说明是OAuth2服务器回调
            {
                //进行OAuth2认证，获取code
                string _OAuth_Url = wxPayService.OAuth2_GetUrl_Pay(Request.Url.ToString());

              
                Response.Redirect(_OAuth_Url);
                return Content("");
            }
            else
            {
                //根据返回的code，获得
                wxPayReturnValue retValue = wxPayService.OAuth2_Access_Token(strCode);

                if (retValue.HasError)
                {
                    Response.Write("获取code失败：" + retValue.Message);
                    return Content("");
                }
                

                strWeixin_OpenID = retValue.GetStringValue("Weixin_OpenID");
                string strWeixin_Token = retValue.GetStringValue("Weixin_Token");
              
                if (string.IsNullOrEmpty(strWeixin_OpenID))
                {
                    Response.Write("openid出错");
                    return Content("");
                }
            }
            if (string.IsNullOrEmpty(strWeixin_OpenID))
                return Content("");

            wxpayPackage pp = wxPayService.MakePayPackage(strWeixin_OpenID, strBillNo, 0.01M, "测试");
            //  LogService.Write("_Pay_json1:" + _Pay_json);
            ViewBag.appid = pp.appId;
            ViewBag.nonceStr = pp.nonceStr;
            ViewBag.package = pp.package;
            ViewBag.paySign = pp.paySign;
            ViewBag.signType = pp.signType;
            ViewBag.timeStamp = pp.timeStamp;

            return View();
        }

        public ActionResult regist()
        {
            return View();
        }
        [HttpPost]
        public JsonResult registed(FormCollection collection)
        {
            //测试先不用发送手机
            //HttpContext.Session.Add("phone", collection["phone"].Trim());
            //HttpContext.Session.Add("code", collection["code"].Trim());
            User u = new User();
            u.game_name = collection["game_name"].Trim();
            u.nickname= collection["nickname"].Trim();
            u.code = collection["code"].Trim();
            u.level = collection["level"].Trim();
            u.password = collection["password"].Trim();
            u.region_name = collection["region_name"].Trim();
            u.phone = collection["phone"].Trim();
            u.area_wx = collection["region"].Trim();
            u.InvitationCode = collection["InvitationCode"].Trim();
            if(Session["OpenID"]!=null)
            u.OpenID = Session["OpenID"].ToString();
            if (db.User.Any(x => x.phone == u.phone))
                return Json("该手机已注册过不能再注册。");
            switch (u.level)
            {
                case "1":
                    u.level = "青铜";
                    break;
                
                case "2":
                    u.level = "白银";
                    break;
                case "3":
                    u.level = "黄金";
                    break;
                case "4":
                    u.level = "铂金";
                    break;
                case "5":
                    u.level = "钻石";
                    break;
                case "6":
                    u.level = "星耀";
                    break;
                case "7":
                    u.level = "王者";
                    break;
            }

            switch (u.area_wx)
            {
                case "1":
                    u.area_wx = "微信区";
                    break;
                case "2":
                    u.area_wx = "QQ区";
                    break;
            }
            //  System.IO.Stream s = Request.InputStream;
            //  int count = 0;
            //  byte[] buffer = new byte[1024];
            //  StringBuilder builder = new StringBuilder();
            //  while ((count = s.Read(buffer, 0, 1024)) > 0)
            //  {
            //      builder.Append(Encoding.UTF8.GetString(buffer,0,count));
            //  }
            ////  builder.Append("success=true");
            //  s.Flush();
            //  s.Close();
            //  s.Dispose();
            //  string a = builder.ToString();
            if (u.phone == Session["phone"].ToString() && u.code == Session["code"].ToString() && u != null)
            {
                if(!string.IsNullOrEmpty(u.InvitationCode))
                {
                    if(!db.StaffSeller.Any(x=>x.OwnerInvitationcode== u.InvitationCode))
                        return Json("注册失败，邀请码错误");
                }
                db.User.Add(u);
                if (db.SaveChanges() > 0)
                {
                    Session["id"] = u.ID;
                    Session["nickname"] = u.nickname;
                    return Json("注册成功");
                }
                else
                {
                    return Json("注册失败");
                }
            }
            else
            {
                return Json("注册失败，验证码错误");
            }

            
        }
        [HttpPost]
        public JsonResult getCode()
        {
            System.IO.Stream s = Request.InputStream;
            int count = 0;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            //  builder.Append("success=true");
            s.Flush();
            s.Close();
            s.Dispose();
            string phone = builder.ToString();
           
            Random ran =new Random();
            string code = ran.Next(1000, 10000).ToString();
            Session["code"] = code;
            phone = phone.Split('=')[1];
            
               
            Session["phone"] = phone;
            CloudInfDemo.sendSmsCode(phone,1,code);
            return Json("验证码发送成功");
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Login(FormCollection f)
        {
            string phone = f["phone"].ToString();
            string password = f["password"].ToString();
            User u = (from c in db.User where c.phone == phone && c.password == password select c).FirstOrDefault();
            if (u != null)
            {
                Session["id"] = u.ID;
                Session["nickname"] = u.nickname;
                if (Session["OpenID"] != null)
                {
                    u.OpenID = Session["OpenID"].ToString();
                    db.SaveChanges();
                    return Json(u.OpenID+"1111122222");
                }


                //wxModelMessage mm = new wxModelMessage();
                //wxModelMessage.sendMessage(mm.FromUserName, "您好" + Session["nickname"]);
                //return View();
                //return Json(Session["id"]);
                //return RedirectToAction("Index", "Home");
                return Json("登陆成功");
            }
            else
            {
                //wxModelMessage mm = new wxModelMessage();
                //wxModelMessage.sendMessage(mm.FromUserName, "请先进行报名");
                return Json("手机号或密码错误，若从没报名，请先进行参赛报名。");
            }
        }
        public ActionResult pageindex()
        {
            if (Session["id"] == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }


    }
}
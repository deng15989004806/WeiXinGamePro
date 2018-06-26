using GamePro.App_Start;
using GamePro.Models;
using GamePro.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Serialization;
using wxBase.Model;

namespace GamePro.BaseFunction
{

    public static class weixinService
    {
        static Dictionary<string, wxaccessToken> OAuthCodeCollection = new Dictionary<string, wxaccessToken>();
        static object OAuthCodeCollectionLock = new object();

        #region 属性       
        public static string token
        {
            get
            {
                return GetAppConfig("token");
            }
        }
        /// <summary>
        /// 微信公众平台开发者appid
        /// </summary>
        public static string appid
        {
            get
            {
                return GetAppConfig("appid");
            }
        }

        /// <summary>
        /// 微信公众平台开发者appsecret
        /// </summary>
        public static string appsecret
        {
            get
            {
                return GetAppConfig("appsecret");
            }
        }

        /// <summary>
        /// access_token的有效期
        /// </summary>
        public static DateTime token_validate_time = DateTime.Now.AddDays(-1);
        public static string TokenCode;
        private static string access_token;
        public static string Access_token
        {
            get
            {
                // 过期时再重新获取
                if (token_validate_time <= DateTime.Now)
                {
                    string url = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + appid + "&secret=" + appsecret;
                    access_token = HttpService.Get(url);
                }
                wxaccessToken token = JSONHelper.JSONToObject<wxaccessToken>(access_token);
                token_validate_time = DateTime.Now.AddSeconds(token.expires_in);
                return token.access_token;
            }
        }

        //微信公众平台openID
        private static string openID;
        public static UserAccessToken get_accesstoken_bycode(string code)
        {
            
            string url = "https://api.weixin.qq.com/sns/oauth2/access_token?appid=" + weixinService.appid + "&secret=" + weixinService.appsecret + "&code=" + code + "&grant_type=authorization_code";
            string result = HttpService.Get(url);
            UserAccessToken t = JSONHelper.JSONToObject<UserAccessToken>(result);
            openID = t.openid;
            return t;
        }

        #region 取得OAuth2 URL地址
        /// <summary>
        ///  取得OAuth2 URL地址,格式为https://open.weixin.qq.com/connect/oauth2/authorize?appid=APPID&redirect_uri=REDIRECT_URI&response_type=code&scope=SCOPE&state=STATE #wechat_redirect
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="Scope"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string OAuth2(string URL, int Scope = 0, string state = "STATE")
        {
            StringBuilder sbCode = new StringBuilder("https://open.weixin.qq.com/connect/oauth2/authorize"); // https://open.weixin.qq.com/connect/oauth2/authorize
            sbCode.Append("?appid=" + weixinService.appid);
            sbCode.Append("&scope=snsapi_base");
            sbCode.Append("&state=" + state);
            sbCode.Append("&redirect_uri=" + URL);// + Uri.EscapeDataString(URL));
            sbCode.Append("&response_type=code"   + "#wechat_redirect");
            return sbCode.ToString();
        }
        #endregion


        /// <summary>
        ///消息加解密密钥
        /// </summary>
        public static string EncodingAESKey
        {
            get
            {
                return GetAppConfig("EncodingAESKey");
            }
        }

        #endregion



        /// <summary>
        /// 微信服务器IP地址
        /// </summary>
        /// <returns>微信服务器IP地址</returns>
        public static List<string> GetCallbackip()
        {
            string url = "https://api.weixin.qq.com/cgi-bin/getcallbackip?access_token=" + Access_token;
            string json = HttpService.Get(url);
            //解析JSON字符串
            wxCallbackip ip = JSONHelper.JSONToObject<wxCallbackip>(json);
            return ip.ip_list;
        }

        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="strKey">配置项</param>
        /// <returns></returns>
        private static string GetAppConfig(string strKey)
        {
            foreach (string key in ConfigurationManager.AppSettings)
            {
                if (key == strKey)
                {
                    return ConfigurationManager.AppSettings[strKey];
                }
            }
            return null;
        }

        public static string make_signature(string timestamp, string nonce)
        {
            //字典序排序
            var arr = new[] { token, timestamp, nonce }.OrderBy(z => z).ToArray();
            // 字符串连接
            var arrString = string.Join("", arr);
            // SHA1加密
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var sha1Arr = sha1.ComputeHash(Encoding.UTF8.GetBytes(arrString));
            StringBuilder signature = new StringBuilder();
            foreach (var b in sha1Arr)
            {
                signature.AppendFormat("{0:x2}", b);
            }
            return signature.ToString();
        }

        /// <summary>  
        /// xml字符串转类  
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <param name="key"></param>  
        /// <returns></returns>  
        public static T XmlStr2Class<T>(string msg)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StringReader sr = new StringReader(msg);
            return (T)serializer.Deserialize(sr);
        }

        ///
        /// 将c# DateTime时间格式转换为Unix时间戳格式
        ///
        public static double ConvertDateTimeInt(System.DateTime time)
        {
            double intResult = 0;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            TimeSpan ts = time - startTime;
            intResult = ts.TotalSeconds;
            return intResult;
        }

        /// <summary>
        /// 当openid或用户ID有一个不为空时自动登录
        /// </summary>
        /// <param name="OpenID"></param>
        /// <param name="ID"></param>
        public static void AutoLogin(string OpenID, int ID)
        {
            GameWZEntities db = new GameWZEntities();
            if (ID!=0)  //用户ID不为空时
            {
                var user = (
                    from a in db.User.Where(x => x.ID == ID) select a
                    ).FirstOrDefault();
                if (user != null)
                {
                    HttpContext.Current.Session["id"] = user.ID;
                    HttpContext.Current.Session["nickname"] = user.nickname;
                    HttpContext.Current.Session["OpenID"] = user.OpenID;
                    if (user.OpenID != OpenID)
                    {   //如果OpenID不一致时更数据库
                        user.OpenID = OpenID;
                        db.SaveChanges();
                    }
                }
            }
            else //当openid不为空时自动登录
            {
                var user = (
                      from a in db.User.Where(x => x.OpenID == OpenID) select a
                      ).FirstOrDefault();
                if (user != null)
                {
                    HttpContext.Current.Session["id"] = user.ID;
                    HttpContext.Current.Session["nickname"] = user.nickname;
                    HttpContext.Current.Session["OpenID"] = user.OpenID;
                }
            }
        }
    }

}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using GamePro.ViewModel;

namespace GamePro.App_Start
{
    public static class Ycbase
    {
        private static string GetAppconfig(string strKey)
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

        public static string token
        {
            get { return GetAppconfig("token"); }
        }
        public static string  makesignature(string token, string timestamp, string nonce)
        {
            var arr = new[] { token, timestamp, nonce }.OrderBy(z => z).ToArray();
            var arrString = string.Join("",arr);
            var sha1 = System.Security.Cryptography.SHA1.Create();
            var sha1arr = sha1.ComputeHash(Encoding.UTF8.GetBytes(arrString));
            StringBuilder signature = new StringBuilder();
            foreach (var b in sha1arr)
            {
                signature.AppendFormat("{0:x2}",b);

            }
            return signature.ToString();
        }

        private static string access_token;
        public static DateTime token_validate_time = DateTime.Now.AddDays(-1);
        public static string Access_token
        {
            get
            {
                if (token_validate_time <= DateTime.Now)
                {
                    string url = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=" + GetAppconfig("appid") + "&secret=" + GetAppconfig("appsecret");
                    access_token = HttpService.Get(url);
                  
                }
                wxaccessToken token = JSONHelper.JSONToObject<wxaccessToken>(access_token);
                token_validate_time = DateTime.Now.AddSeconds(token.expires_in);
                return token.access_token;
            }
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
namespace GamePro.App_Start
{
    public class wxMenuService
    {
        public static string Create(string menufile)
        {
            string menu_content = File.ReadAllText(menufile, Encoding.GetEncoding("GB2312"));
            string url = "https://api.weixin.qq.com/cgi-bin/menu/create?access_token="+Ycbase.Access_token;
            string result = HttpService.Post(url,menu_content);
            return result;
        }

    }
}
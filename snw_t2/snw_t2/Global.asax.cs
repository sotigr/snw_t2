using System;
using System.Web;
using System.Web.SessionState;
using System.Web.Http;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace snw
{
    public class Global : HttpApplication
    {

        void Application_Start(object sender, EventArgs e)
        {
            LoadSettings();
            GlobalConfiguration.Configure(WebConfig.Register);
            core.utility.vplink = new Dictionary<string, core.utility.VirtualPageInfo>();
            RegisterPages();
            core.database.MySQL = new System.Net.MySql.MySqlHelper(core.system.settings.DATABASE.DOMAIN, core.system.settings.DATABASE.PORT, core.system.settings.DATABASE.DATABASE_NAME, core.system.settings.DATABASE.USERNAME, core.system.settings.DATABASE.PASSWORD);

            core.scheduler.Current = new core.scheduler();
            core.scheduler.Current.Start(10);
            core.utility.BackupSchedulerCurrent = new core.system.backups.BackupScheduler();
            core.utility.BackupSchedulerCurrent.AddBackupPath("C:\\storage\\");
            core.utility.architect = new core.system.architect(false);
            core.utility.architect_cr = new core.system.architect_crypt(false);
        }
        void RegisterPages()
        {
            core.utility.AddVirtualPage("?", "main_master.html","main.html" );
            core.utility.AddVirtualPage("index", "main_master.html", "main.html");
            core.utility.AddVirtualPage("page2", "main_master.html", "page2.html"); 
            core.utility.AddVirtualPage("login", "main_master.html", "login.html"); 
            core.utility.AddVirtualPage("read", "main_master.html", "article_page.html"); 
            core.utility.AddVirtualPage("about", "main_master.html", "about.html", ()=> HttpContext.Current.Session["user"] != null);
        }
        void LoadSettings()
        {
            IniFile pathtosettingsini = new IniFile(HttpContext.Current.Server.MapPath("/") + "settings.ini");

            IniFile ini = new IniFile(pathtosettingsini.ReadValue("SETTINGS", "PATH"));
            core.system.settings.GENERAL.DOMAIN = ini.ReadValue("GENERAL", "DOMAIN");

            core.system.settings.ARCHITECT.MASTER_PAGE_PATH = IniPathFix(ini.ReadValue("ARCHITECT", "MASTER_PAGE_PATH"));
            core.system.settings.ARCHITECT.CONTENT_PAGE_PATH = IniPathFix(ini.ReadValue("ARCHITECT", "CONTENT_PAGE_PATH"));
            core.system.settings.ARCHITECT.CSS_PATH = IniPathFix(ini.ReadValue("ARCHITECT", "CSS_PATH"));
            core.system.settings.ARCHITECT.JS_PATH = IniPathFix(ini.ReadValue("ARCHITECT", "JS_PATH"));
            core.system.settings.ARCHITECT.STATUS_PAGE_PATH = IniPathFix(ini.ReadValue("ARCHITECT", "STATUS_PAGE_PATH"));

            core.system.settings.DATABASE.DOMAIN = ini.ReadValue("DATABASE", "DOMAIN");
            core.system.settings.DATABASE.PORT = ini.ReadValue("DATABASE", "PORT");
            core.system.settings.DATABASE.USERNAME = ini.ReadValue("DATABASE", "USERNAME");
            core.system.settings.DATABASE.PASSWORD = ini.ReadValue("DATABASE", "PASSWORD");
            core.system.settings.DATABASE.DATABASE_NAME = ini.ReadValue("DATABASE", "DATABASE_NAME");
        }
        private string IniPathFix(string path)
        {
            if (path.StartsWith("~"))
            {
                Debug.WriteLine(Server.MapPath(@"~/") + path.Remove(0, 1));
                return Server.MapPath(@"~/") + path.Remove(0, 1);

            }
            return path;
        }

        void Application_PostAuthorizeRequest()
        {
            HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
        }
        void Application_AcquireRequestState(object sender, EventArgs e)
        {
            if (!Request.Path.StartsWith("/api/") && !Request.Path.StartsWith("/img/"))
            { if (Request.Path.Trim() == "/" || Request.Path.Trim() == "")
                {
                    Server.ClearError();
                    Response.Clear();
                    Response.ContentType = "text/html";

                    string page = System.IO.File.ReadAllText(core.system.settings.ARCHITECT.CONTENT_PAGE_PATH + "auth.html");
                    string parameters = "";

                    if ((bool)HttpContext.Current.Session["use_encryption"])
                    {
                        parameters = core.system.javascript.PortToJavascript.Parse.VarsToJavaScript(
                          new core.system.javascript.PortToJavascript.JsConversionParam("use_encryption", true),
                          new core.system.javascript.PortToJavascript.JsConversionParam("aes_decryption_token_received", (bool)HttpContext.Current.Session["decryption_token_received"]),
                          new core.system.javascript.PortToJavascript.JsConversionParam("rsa_e", ((core.RSA)HttpContext.Current.Session["rsa"]).GetPublicKey().e.ToString()),
                          new core.system.javascript.PortToJavascript.JsConversionParam("rsa_n", ((core.RSA)HttpContext.Current.Session["rsa"]).GetPublicKey().n.ToString())
                          );
                    }
                    else
                    {
                        parameters = core.system.javascript.PortToJavascript.Parse.VarsToJavaScript(
                       new core.system.javascript.PortToJavascript.JsConversionParam("use_encryption", false)
                       );
                    }
                    page = page.Replace("<params></params>", parameters);
                    Response.BinaryWrite(System.Text.Encoding.UTF8.GetBytes(page));
                    Response.End();
                }
            }

            GC.Collect();
        }
  
        static Regex MobileCheck = new Regex(@"android|(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        static Regex MobileVersionCheck = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        public static bool fBrowserIsMobile()
        {
            Debug.Assert(HttpContext.Current != null);

            if (HttpContext.Current.Request != null && HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"] != null)
            {
                var u = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();

                if (u.Length < 4)
                    return false;

                if (MobileCheck.IsMatch(u) || MobileVersionCheck.IsMatch(u.Substring(0, 4)))
                    return true;
            }

            return false;
        }
        protected void Session_Start(object sender, EventArgs e)
        {
            if (!fBrowserIsMobile())
            {
                core.RSA rsa = new core.RSA(512, false, true);
                HttpContext.Current.Session["rsa"] = rsa;
                HttpContext.Current.Session["use_encryption"] = true;
                HttpContext.Current.Session["decryption_token_received"] = false;
                HttpContext.Current.Session["aes_key"] = null;
                HttpContext.Current.Session["aes_iv"] = null;
            }
            else
            {
                //for mobile use 128bit keys
                core.RSA rsa = new core.RSA(128, false, true);
                HttpContext.Current.Session["rsa"] = rsa;
                HttpContext.Current.Session["use_encryption"] = true;
                HttpContext.Current.Session["decryption_token_received"] = false;
                HttpContext.Current.Session["aes_key"] = null;
                HttpContext.Current.Session["aes_iv"] = null;
            }
            /*
            else
            {
                HttpContext.Current.Session["use_encryption"] = false;
            }*/
        }
        protected void Application_Error(object sender, EventArgs e)
        {

            Server.ClearError();
            Response.Clear();
            Response.ContentType = "text/html";

            string page = System.IO.File.ReadAllText(core.system.settings.ARCHITECT.CONTENT_PAGE_PATH + "RSAtest.html");
            string parameters = "";

            if ((bool)HttpContext.Current.Session["use_encryption"])
            {
                parameters = core.system.javascript.PortToJavascript.Parse.VarsToJavaScript(
                  new core.system.javascript.PortToJavascript.JsConversionParam("use_encryption", true),
                  new core.system.javascript.PortToJavascript.JsConversionParam("aes_decryption_token_received", (bool)HttpContext.Current.Session["decryption_token_received"]),
                  new core.system.javascript.PortToJavascript.JsConversionParam("rsa_e", ((core.RSA)HttpContext.Current.Session["rsa"]).GetPublicKey().e.ToString()),
                  new core.system.javascript.PortToJavascript.JsConversionParam("rsa_n", ((core.RSA)HttpContext.Current.Session["rsa"]).GetPublicKey().n.ToString())
                  );
            }
            else
            {
                parameters = core.system.javascript.PortToJavascript.Parse.VarsToJavaScript(
               new core.system.javascript.PortToJavascript.JsConversionParam("use_encryption", false)
               );
            }
            page = page.Replace("<params></params>", parameters);
            Response.BinaryWrite(System.Text.Encoding.UTF8.GetBytes(page));
            Response.End();

        }
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

            /*  if (!(Request.UserHostAddress == "::1" || Request.UserHostAddress.StartsWith("192.168") || Request.UserHostAddress.StartsWith("loalhost") || Request.UserHostAddress != "127.0.0.0"))
              {
                  Response.Clear();
                  Response.Write("The website is currently unavailable. Only local connections allowed, IP: " + Request.UserHostAddress);
                  Response.End();
              }*/
        }


        /* 
        void Application_AcquireRequestState(object sender, EventArgs e)
        { 
         
        }
        protected void Session_Start(object sender, EventArgs e)
        {
         
        } 

      
		
        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
        */
    }
}
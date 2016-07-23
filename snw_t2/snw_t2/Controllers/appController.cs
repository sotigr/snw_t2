using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;
using System.Web;
using System.ServiceModel.Channels;
using System.Net.MySql;
using System.Text;
using System.IO;
using System.Net;
using System.Linq;
using System.Globalization;
using snw.core.system.javascript;
using System.Diagnostics;

namespace snw.Controllers
{
    public class appController : ApiController
    {
        [Route("api/style_lib")]
        [HttpGet]
        public HttpResponseMessage api_lib_css()
        {
            return core.utility.architect.ResponseCSS("lib.css");
        }
        [Route("api/script_lib")]
        [HttpGet]
        public HttpResponseMessage api_lib_js()
        {
            return core.utility.architect.ResponseJs("lib.js");
        }
        [Route("api/login")]
        [HttpPost]
        public string api_login(JObject data)
        {
            dynamic ddata = data;
            string nickname, password;
            try
            {
                nickname = ddata.nick;
                password =  ddata.pass;
            }
            catch { return core.AES.Encrypt("Invalid Data."); }
            nickname = core.AES.Decrypt(nickname);
            password = core.AES.Decrypt(password);
            if (HttpContext.Current.Session["user"] != null)
            {
                return core.AES.Encrypt("User already logged in.");
            }
            if (password == core.database.MySQL.SendQuerySingle("select pass from account_proto where nick = '" + nickname + "';"))
            {
                if (core.database.MySQL.SendQuerySingle("select active from account_proto where nick = '" + nickname + "';") == "0")
                {
                    return core.AES.Encrypt("This account is not active.");
                }
                try
                {
                    MysqlResault res = core.database.MySQL.SendQuery("select *, DATE_FORMAT(NOW(),'%d-%m-%Y') AS date from account_proto where nick='" + nickname + "';");
                    core.system.user user = new core.system.userallinfo()
                    {
                        id = int.Parse(res[0]["id"]),
                        nick = res[0]["nick"],
                        pass = res[0]["pass"],
                        email = res[0]["email"],
                        firstname = res[0]["firstname"],
                        lastname = res[0]["lastname"],
                        icon = res[0]["icon"],
                        sec_clearence = int.Parse(res[0]["sec_clearence"]),
                        banned = res[0]["banned"],
                        active = res[0]["active"],
                        creation_date = DateTime.ParseExact(res[0]["date"], "dd-MM-yyyy", null),
                        last_ip = res[0]["last_ip"]
                    };

                    HttpContext.Current.Session["user"] = user;
                    core.utility.UpdateUserInfo("main_master.html");
                    return core.AES.Encrypt("1");
                }
                catch (Exception ex)
                {
                    return core.AES.Encrypt(ex.Message);
                }
            }
            else
            {
                return core.AES.Encrypt("Wrong username or password.");
            }
        }
        [Route("api/logout")]
        [HttpPost]
        public string api_logout(JObject data)
        {
            string b64 = ((dynamic)data).lg;
            if (core.AES.Decrypt(b64) == "logout")
            {
                if (HttpContext.Current.Session["user"] != null)
                {
                    HttpContext.Current.Session["user"] = null;
                    HttpContext.Current.Session.Abandon();
                    return "1";
                }
                return "Not logged in";
            }
            return "";
        }
        //==============================================================================================================================================//
        //============== NOT SECURE ====================================================================================================================//
        //==============================================================================================================================================//
        [Route("api/get_articles/{page}/{length}")]
        [HttpGet]
        public MysqlResault snw_get_articles_game(int page, int length)
        {
            if (page <= 0)
                return null;
            int valmin, valmax;
            valmin = (page - 1) * length;
            valmax = length;

            MysqlResault resault = core.database.MySQL.SendQuery("SELECT article_proto.id,article_proto.article_title, article_proto.article_category, article_proto.article_desc,article_proto.article_name,article_proto.owner_id ,(SELECT account_proto.nick FROM account_proto WHERE account_proto.id = article_proto.owner_id) as username, DATE_FORMAT(article_edit_date,'%d/%m/%y') as date FROM article_proto  ORDER BY article_proto.article_edit_date DESC LIMIT " + valmin + ", " + valmax + ";");

            IDictionary<string, string> dictcn = new Dictionary<string, string>();
            string count = core.database.MySQL.SendQuerySingle("SELECT COUNT(*) as count FROM article_proto;");
            dictcn.Add("count", count);
            resault.Add(resault.Count(), dictcn);

            return resault;
        }
        [Route("api/get_articles/{userid}/{page}/{length}")]
        [HttpGet]
        public MysqlResault snw_article_game(string userid, int page, int length)
        {
            if (page <= 0)
                return null;
            int valmin, valmax;
            valmin = (page - 1) * length;
            valmax = length;

            MysqlResault resault = core.database.MySQL.SendQuery("SELECT article_proto.id,article_proto.article_title, article_proto.article_category, article_proto.article_desc,article_proto.article_name,article_proto.owner_id ,(SELECT account_proto.nick FROM account_proto WHERE account_proto.id = article_proto.owner_id) as username, DATE_FORMAT(article_edit_date,'%d/%m/%y') as date FROM article_proto where article_proto.owner_id='" + userid + "'  ORDER BY article_proto.article_edit_date DESC LIMIT " + valmin + ", " + valmax + ";");

            IDictionary<string, string> dictcn = new Dictionary<string, string>();
            string count = core.database.MySQL.SendQuerySingle("SELECT COUNT(*) as count FROM article_proto;");
            dictcn.Add("count", count);
            resault.Add(resault.Count(), dictcn);

            return resault;
        }
        [Route("api/get_articles/all")]
        [HttpGet]
        public MysqlResault snw_get_articles_all()
        {
            if (HttpContext.Current.Session["user"] == null)
            {
                return null;
            }
            MysqlResault resault = core.database.MySQL.SendQuery("SELECT article_proto.id,article_proto.article_title, article_proto.article_category, article_proto.article_desc,article_proto.article_name,article_proto.owner_id ,(SELECT account_proto.nick FROM account_proto WHERE account_proto.id = article_proto.owner_id) as username, DATE_FORMAT(article_edit_date,'%d/%m/%y') as date FROM article_proto where article_proto.owner_id='" + ((core.system.userallinfo)HttpContext.Current.Session["user"]).id + "'  ORDER BY article_proto.article_edit_date;");

            return resault;
        }
        [Route("api/get_articles/{userid}/all")]
        [HttpGet]
        public MysqlResault snw_get_articles_user_all(string userid)
        {

            MysqlResault resault = core.database.MySQL.SendQuery("SELECT article_proto.id,article_proto.article_title, article_proto.article_category, article_proto.article_desc,article_proto.article_name,article_proto.owner_id ,(SELECT account_proto.nick FROM account_proto WHERE account_proto.id = article_proto.owner_id) as username, DATE_FORMAT(article_edit_date,'%d/%m/%y') as date FROM article_proto where article_proto.owner_id='" + userid + "'  ORDER BY article_proto.article_edit_date;");

            return resault;
        }
        [Route("api/getuserimage")]
        [HttpGet]
        public HttpResponseMessage api_getuserimage()
        {
            try
            {
                string usericon = core.database.MySQL.SendQuerySingle("select icon from account_proto where id = '" + ((core.system.userallinfo)HttpContext.Current.Session["user"]).id + "';");
                if (usericon == "null")
                {
                    return core.utility.architect.ResponseNotFound();
                }
                MemoryStream ms = new MemoryStream(core.utility.BackupSchedulerCurrent.ReadBytes("users/icons/" + usericon));
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(ms);
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                return response;
            }
            catch
            {
                return core.utility.architect.ResponseNotFound();
            }
        }
        [Route("api/getuserimage/{uid}")]
        [HttpGet]
        public HttpResponseMessage api_getuserimage(string uid)
        {
            try
            {
                string usericon = core.database.MySQL.SendQuerySingle("select icon from account_proto where id = '" + uid + "';");
                if (usericon == "null")
                {
                    return core.utility.architect.ResponseNotFound();
                }
                MemoryStream ms = new MemoryStream(core.utility.BackupSchedulerCurrent.ReadBytes("users/icons/" + usericon));
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(ms);
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                return response;
            }
            catch
            {
                return core.utility.architect.ResponseNotFound();
            }
        }

        [Route("api/get_article/{art_name}")]
        [HttpGet]
        public core.system.text.article api_get_article(string art_name)
        {
            if (core.utility.BackupSchedulerCurrent.FileExists("articles/html/" + art_name))
            {
                MysqlResault art_res = core.database.MySQL.SendQuery("select * from article_proto where article_name = '" + art_name + "';");
                MysqlResault user_res = core.database.MySQL.SendQuery("select * from account_proto where id='" + art_res[0]["owner_id"] + "';");

                core.system.user usr = new core.system.user() { nick = user_res[0]["nick"], icon = user_res[0]["icon"], email = user_res[0]["email"], id = user_res[0]["id"] };

                return new core.system.text.article()
                {
                    Name = art_name,
                    Content = core.utility.BackupSchedulerCurrent.ReadString("articles/html/" + art_name),
                    Publisher = usr,
                    Group = art_res[0]["article_category"],
                    Title = art_res[0]["article_title"]
                };

            }
            else
            { return null; }
        }
    }
}
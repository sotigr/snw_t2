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
    public class authController : ApiController
    {

        [Route("api/dependencies")]
        [HttpGet]
        public HttpResponseMessage api_dependencies_js()
        {
            return core.utility.architect.ResponseJs("dependencies.js");
        }
        [Route("api/auth")]
        [HttpGet]
        public HttpResponseMessage api_auth_js()
        {
            return core.utility.architect.ResponseJs("auth_min.js");
        }
        [Route("api/apply_decryption_token")]
        [HttpPost]
        public string api_apply_decryption_token(JObject data)
        {
            if ((bool)HttpContext.Current.Session["decryption_token_received"] != true)
            {
                if (((bool)HttpContext.Current.Session["use_encryption"]) == true)
                {
                    string base64message;
                    try
                    {
                        base64message = ((dynamic)data).key;
                    }
                    catch { return "Invalid input."; }
                    string[] rsa_numbers = Encoding.ASCII.GetString(Convert.FromBase64String(base64message)).Split(' ');
                    rsa_numbers = rsa_numbers.Take(rsa_numbers.Count() - 1).ToArray();
                    string[] aes_token = Encoding.ASCII.GetString(((core.RSA)HttpContext.Current.Session["rsa"]).Decrypt(rsa_numbers)).Split(' ');

                    if (aes_token[0].Length != 16 || aes_token[1].Length != 16)
                        return "Bad token";

                    HttpContext.Current.Session["aes_key"] = Encoding.UTF8.GetBytes(aes_token[0]);
                    HttpContext.Current.Session["aes_iv"] = Encoding.UTF8.GetBytes(aes_token[1]);
                    HttpContext.Current.Session["decryption_token_received"] = true;

                    return "ok";
                }
                else
                {
                    return "Your devise can't support this option.";
                }
            }
            else
            {
                HttpContext.Current.Session.Abandon();
                return "Keys for this session have already been registered. This session is terminated for security reasons.";
            }

        }
        [Route("api/page_provider")]
        [HttpPost]
        public string api_page_provider(JObject data)
        {
            if ((bool)HttpContext.Current.Session["decryption_token_received"] == true)
            {
                if (((bool)HttpContext.Current.Session["use_encryption"]) == true)
                {
                    string b64p_path;
                    try
                    {
                        b64p_path = ((dynamic)data).path;
                    }
                    catch { return "Invalid input."; }

                    string path = core.AES.Decrypt(b64p_path);

                    if (!core.utility.vplink.ContainsKey(path))
                        return core.utility.architect_cr.ResponseNotFound();
                    else
                    {
                        bool flag = false;
                 
                        if (core.utility.vplink[path].condition != null)
                        {
                            if (core.utility.vplink[path].condition())
                                flag = true;
                        }
                        else
                            flag = true;

                        if (flag)
                        {
                            string[] plink = core.utility.vplink[path].link;
                            if (plink[1] == null)
                                return core.utility.architect_cr.ResponsePlain(plink[0]);
                            else
                                return core.utility.architect_cr.ResponseWithMaster(plink[0], plink[1]);
                        }
                        else

                        return core.utility.architect_cr.ResponseForbidden();
                    }

                }
                return "";
            }
            return "";
        }
    }
}
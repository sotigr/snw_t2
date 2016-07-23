using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Web;

namespace snw.core.system
{
    public class architect_crypt
    {
        private bool _load_mem;
        private IDictionary<string, string> _m_pg_list;
        private IDictionary<string, string> _p_pg_list;
        private IDictionary<string, string> _css_list;
        private IDictionary<string, string> _js_list;
        public architect_crypt()
        {
            _load_mem = false;
        }
        public architect_crypt(bool LoadPages_In_Memmory)
        {
            if (!LoadPages_In_Memmory)
            {
                _load_mem = false;
            }
            else
            {
                _m_pg_list = new Dictionary<string, string>();
                _p_pg_list = new Dictionary<string, string>();
                _css_list = new Dictionary<string, string>();
                _js_list = new Dictionary<string, string>();
                _load_mem = true;
            }
        }

        public HttpResponseMessage ResponseCSS(string CSSName)
        {
            return EnfoldWithinHttpResponseCss(ReadCSS(CSSName));
        }
        public HttpResponseMessage ResponseJs(string JSName)
        {
            return EnfoldWithinHttpResponsePlain(ReadJS(JSName));
        }
        public string ResponsePlain(string PageName)
        {
            return AES.Encrypt(ReadPage(PageName));
        }
        public string ResponsePlain(string PageName, string var_tag, string vars)
        {
            return AES.Encrypt(ReadPage(PageName).Replace(var_tag, vars)) ;
        }
        public string ResponseWithMaster(string MasterName, string PageName)
        {
            if (HttpContext.Current.Session["_master_page_vars"] != null)
            {
                return AES.Encrypt(CombineWithMasterPage(ReadMaster(MasterName), ReadPage(PageName)).Replace(((IDictionary<string, string[]>)HttpContext.Current.Session["_master_page_vars"])[MasterName][0], ((IDictionary<string, string[]>)HttpContext.Current.Session["_master_page_vars"])[MasterName][1]));
            }
            return  AES.Encrypt(CombineWithMasterPage(ReadMaster(MasterName), ReadPage(PageName)));
        }
        public string ResponseWithMaster(string MasterName, string PageName, string var_tag, string vars)
        {
            if (HttpContext.Current.Session["_master_page_vars"] != null)
            {
                return AES.Encrypt(CombineWithMasterPage(ReadMaster(MasterName), ReadPage(PageName)).Replace(var_tag, vars).Replace(((IDictionary<string, string[]>)HttpContext.Current.Session["_master_page_vars"])[MasterName][0], ((IDictionary<string, string[]>)HttpContext.Current.Session["_master_page_vars"])[MasterName][1]));
            }
            return AES.Encrypt( CombineWithMasterPage(ReadMaster(MasterName), ReadPage(PageName)).Replace(var_tag, vars));
        }
        public string ResponseForbidden()
        {
            string res = "";
            using (StreamReader reader = new StreamReader(settings.ARCHITECT.STATUS_PAGE_PATH + "forbidden.html"))
            {
                res = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
            }
            return AES.Encrypt(res);
        }
        public string ResponseNotFound()
        {
            string res = "";
            using (StreamReader reader = new StreamReader(settings.ARCHITECT.STATUS_PAGE_PATH + "notfound.html"))
            {
                res = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
            }
            return AES.Encrypt(res);
        }
        private string CombineWithMasterPage(string MasterPage, string Content)
        {
            return MasterPage.Replace("<mpcontent></mpcontent>", Content);
        }
        private HttpResponseMessage EnfoldWithinHttpResponsePlain(string ResponseContent)
        {
            HttpResponseMessage response = new HttpResponseMessage() { Content = new StringContent(ResponseContent) };
            response.Headers.Add("Accept", "text/plain");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            return response;
        }
        private HttpResponseMessage EnfoldWithinHttpResponseCss(string ResponseContent)
        {
            HttpResponseMessage response = new HttpResponseMessage() { Content = new StringContent(ResponseContent) };
            response.Headers.Add("Accept", "text/css");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/css");
            return response;
        }
   
        private string ReadMaster(string Name)
        {
            string res = "<mpcontent></mpcontent>";
            if (_load_mem)
            {
                if (_m_pg_list.ContainsKey(Name))
                {
                    res = _m_pg_list[Name];
                }
                else
                {
                    using (StreamReader reader = new StreamReader(settings.ARCHITECT.MASTER_PAGE_PATH + Name))
                    {
                        res = reader.ReadToEnd();
                        _m_pg_list[Name] = res;
                        reader.Close();
                        reader.Dispose();
                    }
                }
            }
            else
            {
                using (StreamReader reader = new StreamReader(settings.ARCHITECT.MASTER_PAGE_PATH + Name))
                {
                    res = reader.ReadToEnd();
                    reader.Close();
                    reader.Dispose();
                }
            }
            return res;
        }
        private string ReadPage(string Name)
        {
            string res = "";
            if (_load_mem)
            {
                if (_p_pg_list.ContainsKey(Name))
                {
                    res = _p_pg_list[Name];
                }
                else
                {
                    using (StreamReader reader = new StreamReader(settings.ARCHITECT.CONTENT_PAGE_PATH + Name))
                    {
                        res = reader.ReadToEnd();
                        _p_pg_list[Name] = res;
                        reader.Close();
                        reader.Dispose();
                    }
                }
            }
            else
            {
                using (StreamReader reader = new StreamReader(settings.ARCHITECT.CONTENT_PAGE_PATH + Name))
                {
                    res = reader.ReadToEnd();
                    reader.Close();
                    reader.Dispose();
                }
            }

            return res;
        }
        private string ReadJS(string Name)
        {
            string res = "";
            if (_load_mem)
            {
                if (_js_list.ContainsKey(Name))
                {
                    res = _js_list[Name];
                }
                else
                {
                    using (StreamReader reader = new StreamReader(settings.ARCHITECT.JS_PATH + Name))
                    {
                        res = reader.ReadToEnd();
                        _js_list[Name] = res;
                        reader.Close();
                        reader.Dispose();
                    }
                }
            }
            else
            {
                using (StreamReader reader = new StreamReader(settings.ARCHITECT.JS_PATH + Name))
                {
                    res = reader.ReadToEnd();
                    reader.Close();
                    reader.Dispose();
                }
            }
            return res;
        }
        private string ReadCSS(string Name)
        {
            string res = "";
            if (_load_mem)
            {
                if (_css_list.ContainsKey(Name))
                {
                    res = _css_list[Name];
                }
                else
                {
                    using (StreamReader reader = new StreamReader(settings.ARCHITECT.CSS_PATH + Name))
                    {
                        res = reader.ReadToEnd();
                        _css_list[Name] = res;
                        reader.Close();
                        reader.Dispose();
                    }
                }

            }
            else
            {
                using (StreamReader reader = new StreamReader(settings.ARCHITECT.CSS_PATH + Name))
                {
                    res = reader.ReadToEnd();
                    reader.Close();
                    reader.Dispose();
                }
            }
            return res;
        }
    }
}
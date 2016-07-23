using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Web;

namespace snw.core.system
{
    public class architect
    {
        private bool _load_mem; 
        private IDictionary<string, string> _m_pg_list;
        private IDictionary<string, string> _p_pg_list;
        private IDictionary<string, string> _css_list;
        private IDictionary<string, string> _js_list; 
        public architect()
        {
            _load_mem = false;
        }
        public architect(bool LoadPages_In_Memmory)
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
        public HttpResponseMessage ResponsePlain(string PageName)
        {
            return EnfoldWithinHttpResponseHtml(ReadPage(PageName));
        }
        public HttpResponseMessage ResponsePlain(string PageName, string var_tag, string vars)
        {
            return EnfoldWithinHttpResponseHtml(ReadPage(PageName).Replace(var_tag, vars));
        }
        public HttpResponseMessage ResponseWithMaster(string MasterName, string PageName)
        {
            if (HttpContext.Current.Session["_master_page_vars"] != null)
            {
                return EnfoldWithinHttpResponseHtml(CombineWithMasterPage(ReadMaster(MasterName), ReadPage(PageName)).Replace(((IDictionary<string, string[]>)HttpContext.Current.Session["_master_page_vars"])[MasterName][0], ((IDictionary<string, string[]>)HttpContext.Current.Session["_master_page_vars"])[MasterName][1]));
            }  
            return EnfoldWithinHttpResponseHtml(CombineWithMasterPage(ReadMaster(MasterName), ReadPage(PageName)));
        }
        public HttpResponseMessage ResponseWithMaster(string MasterName, string PageName, string var_tag, string vars)
        {
            if (HttpContext.Current.Session["_master_page_vars"] != null)
            {
                return EnfoldWithinHttpResponseHtml(CombineWithMasterPage(ReadMaster(MasterName), ReadPage(PageName)).Replace(var_tag, vars).Replace(((IDictionary<string, string[]>)HttpContext.Current.Session["_master_page_vars"])[MasterName][0], ((IDictionary<string, string[]>)HttpContext.Current.Session["_master_page_vars"])[MasterName][1]));
            }
            return EnfoldWithinHttpResponseHtml(CombineWithMasterPage(ReadMaster(MasterName), ReadPage(PageName)).Replace(var_tag, vars));
        }
        public HttpResponseMessage ResponseForbidden()
        {
            string res = "";
            using (StreamReader reader = new StreamReader(settings.ARCHITECT.STATUS_PAGE_PATH + "forbidden.html"))
            {
                res = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
            }
            HttpResponseMessage response = new HttpResponseMessage() { Content = new StringContent(res) };
            response.Headers.Add("Accept", "text/html;level=1");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
        public HttpResponseMessage ResponseNotFound()
        {
            string res = "";
            using (StreamReader reader = new StreamReader(settings.ARCHITECT.STATUS_PAGE_PATH + "notfound.html"))
            {
                res = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
            }
            HttpResponseMessage response = new HttpResponseMessage() { Content = new StringContent(res) };
            response.Headers.Add("Accept", "text/html;level=1");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
        private string CombineWithMasterPage(string MasterPage, string Content)
        {
            return MasterPage.Replace("<mpcontent></mpcontent>", Content);
        }
        private HttpResponseMessage EnfoldWithinHttpResponseHtml(string ResponseContent)
        {
            HttpResponseMessage response = new HttpResponseMessage() { Content = new StringContent(ResponseContent) };
            response.Headers.Add("Accept", "text/html;level=1");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
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
        public HttpResponseMessage Redirect(string location)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.Headers.Location = new Uri(location);
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
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;
using snw.core.system.backups;
using snw.core.system.javascript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Xml.Linq;

namespace snw.core
{
    public class utility
    {
        public static core.system.architect architect;
        public static core.system.architect_crypt architect_cr;
        public static Dictionary<string, Dictionary<string, string>> locale;
        public static core.system.backups.BackupScheduler BackupSchedulerCurrent;
        public static IDictionary<string, string[]> vplink; 
        public static bool RegularMatch(string pattern, string input) { return new Regex(pattern).Match(input).Success; }
    
        private List<SessionStateItemCollection> AllActiveSessions()
        {
            List<SessionStateItemCollection> activeSessions = new List<SessionStateItemCollection>();
            object obj = typeof(HttpRuntime).GetProperty("CacheInternal", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null, null);
            object[] obj2 = (object[])obj.GetType().GetField("_caches", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
            for (int i = 0; i < obj2.Length; i++)
            {
                Hashtable c2 = (Hashtable)obj2[i].GetType().GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj2[i]);
                foreach (DictionaryEntry entry in c2)
                {
                    object o1 = entry.Value.GetType().GetProperty("Value", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(entry.Value, null);
                    if (o1.GetType().ToString() == "System.Web.SessionState.InProcSessionState")
                    {
                        SessionStateItemCollection sess = (SessionStateItemCollection)o1.GetType().GetField("_sessionItems", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(o1);
                        if (sess != null)
                        {
                            activeSessions.Add(sess);
                        }
                    }
                }
            }
            return activeSessions;
        }
        public static void AddVirtualPage(string virtualname, string literalpagename, string literalmasterpagename)
        {
            vplink.Add(virtualname, new string[2] { literalpagename, literalmasterpagename });
        }
        public static void AddVirtualPage(string virtualname, string literalpagename)
        {
            vplink.Add(virtualname, new string[2] { literalpagename, null });
        }
        public static void UpdateUserInfo(string master_page_name)
        {
            if (HttpContext.Current.Session["user"] != null)
            {
                System.Net.MySql.MysqlResault resault = core.database.MySQL.SendQuery("select nick, email, firstname, lastname, icon, sec_clearence, DATE_FORMAT(creation_date,'%d/%m/%Y') AS date from account_proto where nick='" + ((core.system.user)HttpContext.Current.Session["user"]).nick + "';");
               IDictionary<string,string[]> dict = new Dictionary<string, string[]>();

                dict[master_page_name] = new string[2] { "<userinfo></userinfo>", PortToJavascript.Parse.VarsToJavaScript(
                        new PortToJavascript.JsConversionParam("user_data_available", true),
                         new PortToJavascript.JsConversionParam("user_data_nick", resault[0]["nick"]),
                         new PortToJavascript.JsConversionParam("user_data_email", resault[0]["email"]),
                         new PortToJavascript.JsConversionParam("user_data_firstname", resault[0]["firstname"]),
                         new PortToJavascript.JsConversionParam("user_data_lastname", resault[0]["lastname"]),
                         new PortToJavascript.JsConversionParam("user_data_icon", resault[0]["icon"]),
                         new PortToJavascript.JsConversionParam("user_data_security", resault[0]["sec_clearence"]),
                         new PortToJavascript.JsConversionParam("user_data_date", resault[0]["date"])
                     )};
                HttpContext.Current.Session["_master_page_vars"] = dict;
            }
            else
            {
                IDictionary<string, string[]> dict = new Dictionary<string, string[]>();

                dict[master_page_name] = new string[2] { "<userinfo></userinfo>", PortToJavascript.Parse.VarsToJavaScript(
                        new PortToJavascript.JsConversionParam("user_data_available", false)
                     )};
                HttpContext.Current.Session["_master_page_vars"] = dict;
            }

        }
        public static string docxToHtml(byte[] docx_doc)
        {
            string fin = "";
            using (MemoryStream memoryStream = new MemoryStream(docx_doc))
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true))
                {
                    HtmlConverterSettings settings = new HtmlConverterSettings()
                    {
                        AdditionalCss = "span.pt-Hyperlink{color:#60cdff; text-decoration: underlined; transition: color 0.3s linear;}span.pt-Hyperlink:hover{transition: color 0.3s linear;color:#91DBFF;}",
                        PageTitle = "doc",
                        ImageHandler = imgInfo =>
                          {
                              string extension = imgInfo.ContentType.Split('/')[1].ToLower();
                              ImageFormat imageFormat = null;
                              if (extension == "png")
                              {
                                  // Convert the .png file to a .jpeg file.
                                  extension = "jpeg";
                                  imageFormat = ImageFormat.Jpeg;
                              }
                              else if (extension == "bmp")
                                  imageFormat = ImageFormat.Bmp;
                              else if (extension == "jpeg")
                                  imageFormat = ImageFormat.Jpeg;
                              else if (extension == "tiff")
                                  imageFormat = ImageFormat.Tiff;

                              System.IO.MemoryStream ms = new System.IO.MemoryStream();
                              imgInfo.Bitmap.Save(ms, imageFormat);
                              byte[] byteImage = ms.ToArray();

                              XElement img = new XElement(Xhtml.img,
                                  new XAttribute("style", "max-height:" + imgInfo.Bitmap.Height + "px; max-width:100%; width:" + imgInfo.Bitmap.Width + "px;"),
                                  new XAttribute(NoNamespace.src, "data:image/" + extension + ";base64," + Convert.ToBase64String(byteImage)),
                                  imgInfo.AltText != null ?
                                      new XAttribute(NoNamespace.alt, imgInfo.AltText) : null);

                              return img;
                          }
                    };

                    XElement html = HtmlConverter.ConvertToHtml(doc, settings);

                    fin = html.ToStringNewLineOnAttributes();
                };

            }

            return fin;
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
        public static void SendEmail(string sendto, string subject, string message)
        {
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("papsotigms@gmail.com", "sotig123");

            MailMessage mm = new MailMessage("papsotigms@gmail.com", sendto, subject, message);
            mm.IsBodyHtml = true;
            mm.BodyEncoding = System.Text.UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);
        }
        public static string ToMD5(string text)
        {
            string password = @"" + text;
            byte[] encodedPassword = new System.Text.UTF8Encoding().GetBytes(password);
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
            string encoded = BitConverter.ToString(hash)
               .Replace("-", string.Empty)
               .ToLower();
            return encoded;
        }
        public static void DestroyObject(object obj)
        {
            obj = null;
        }
        public static void CopyStream(System.IO.Stream sourceStream, System.IO.Stream targetStream)
        {
            byte[] buffer = new byte[0x10000];
            int n;
            while ((n = sourceStream.Read(buffer, 0, buffer.Length)) != 0)
                targetStream.Write(buffer, 0, n);
        }
        public static byte[] ResizeImage(byte[] image, int width, int height, System.Drawing.Imaging.ImageFormat format)
        {
            System.IO.MemoryStream myMemStream = new System.IO.MemoryStream(image);
            System.Drawing.Image fullsizeImage = System.Drawing.Image.FromStream(myMemStream);
            System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(width, height, null, IntPtr.Zero);
            System.IO.MemoryStream myResult = new System.IO.MemoryStream();
            newImage.Save(myResult, format);  //Or whatever format you want.
            return myResult.ToArray();  //Returns a new byte array.
        }
        public static byte[] StreamToBytes(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }
            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
    }
}
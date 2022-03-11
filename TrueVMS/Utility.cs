using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TrueVMS.Model;

namespace TrueVMS
{
    public class Utility
    {
        private static Logger log = new Logger("Utility");


        public static string STATUS_SUCCESS = "SUCCESS";
        public static string STATUS_FAILED = "FAILED";

        private static DirectoryInfo getLastedFolder(string baseFolder)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(baseFolder);
                var sorted = dirInfo.GetDirectories("*.*", SearchOption.TopDirectoryOnly).ToList();
                DirectoryInfo[] subDirs = dirInfo.GetDirectories("*.*", SearchOption.TopDirectoryOnly);

                return subDirs[subDirs.Length - 1];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static string getFrmEntryBackgroundImageFile()
        {
            try
            {
                String TitleFolder = @"Resources\Screen010";
                DirectoryInfo di = getLastedFolder(TitleFolder);
                return di.GetFiles()[0].FullName;

            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                return "";
            }
        }



        internal static bool ValidateServerCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }


        private static TCFileVersionModel getLegalDocument(int language,int documentType, string SERVER_API_URL)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

                var client = new RestClient(SERVER_API_URL + "/api/kiosk/getLegalDocument/"+language+","+documentType);
                client.Timeout = -1;
                
                var request = new RestRequest(Method.GET);
                //request.AddHeader();
                IRestResponse response = client.Execute(request);
                //Console.WriteLine(response.Content);


                TCFileVersionModel m = JsonConvert.DeserializeObject<TCFileVersionModel>(response.Content);
                if (m != null)
                {
                    return m;
                }
                else
                {
                    log.Error("Can not get legal document : "+ SERVER_API_URL + "/api/kiosk/getLegalDocument/" + language + "," + documentType);

                    TCFileVersionModel a = new TCFileVersionModel();
                    a.Version = "0.0";
                    a.HtmlDetail = "ไม่สามารถโหลดเอกสารได้";
                    return a;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }



        public static TCFileVersionModel getPrivacySensitiveTextEng(string SERVER_API_URL)
        {
            try
            {
                return getLegalDocument(1, 2, SERVER_API_URL);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }



        public static TCFileVersionModel getPrivacySensitiveTextThai(string SERVER_API_URL)
        {
            try
            {
                return getLegalDocument(0, 2, SERVER_API_URL);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }


        public static TCFileVersionModel getPrivacyMarketingTextEng(string SERVER_API_URL)
        {
            try
            {
                return getLegalDocument(1, 1, SERVER_API_URL);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }


        public static TCFileVersionModel getPrivacyMarketingTextThai(string SERVER_API_URL)
        {
            try
            {
                return getLegalDocument(0, 1, SERVER_API_URL);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }




        public static TCFileVersionModel getTermAndConditionTextThai(string SERVER_API_URL)
        {
            try
            {
                return getLegalDocument(0, 0, SERVER_API_URL);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }


        public static TCFileVersionModel getTermAndConditionTextEng(string SERVER_API_URL)
        {
            try
            {
                return getLegalDocument(1, 0, SERVER_API_URL);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                throw ex;
            }
        }


        public static string getFrmEnterIDCardOrPassportBackgroundImageFile()
        {
            try
            {
                String TitleFolder = @"Resources\Screen020";
                DirectoryInfo di = getLastedFolder(TitleFolder);
                return di.GetFiles()[0].FullName;

            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                return "";
            }
        }



        public static string getFrmTCBackgroundImageFile()
        {
            try
            {
                String TitleFolder = @"Resources\Screen020";
                DirectoryInfo di = getLastedFolder(TitleFolder);
                return di.GetFiles()[0].FullName;

            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                return "";
            }
        }





        internal static string getHeadTitleBackgroundImageFile()
        {
            try
            {
                String TitleFolder = @"Resources\Header";
                DirectoryInfo di = getLastedFolder(TitleFolder);

                //log.Info(di.FullName);
                //log.Info(di.GetFiles()[0].FullName);

                return di.GetFiles()[0].FullName;

            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                return "";
            }
        }


        public static string getHomeVDOFile()
        {
            try
            {
                String TitleFolder = @"Resources\ScreenSaverVideo";
                DirectoryInfo di = getLastedFolder(TitleFolder);

                return di.GetFiles()[0].FullName;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                return "";
            }
        }
    }
}
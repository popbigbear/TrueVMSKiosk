using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrueVMS
{
    public class Logger
    {

        private NLog.Logger _logger;
        private NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public static string LOG_SYSTEM = "SYS_INT";


        private static string TERMINAL_CODE = ConfigurationManager.AppSettings["terminalCode"];


        public Logger(String className)
        {
            _logger = NLog.LogManager.GetLogger(className);

        }


        private void addDbLog(string desc, string logResult, string logType, string trnCode, string usercode)
        {
        }

        /*-------------------------Start log on text only method--------------------------*/
        public void Error(String msg, Object obj)
        {
            try
            {
                _logger.Error(msg, obj);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }
        public void Error(String msg)
        {
            try
            {
                _logger.Error(msg);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        public void Info(String msg, Object obj)
        {
            try
            {
                _logger.Info(msg, obj);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }
        public void Info(String msg)
        {
            try
            {
                _logger.Info(msg);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        /*-------------------------End log on text only method--------------------------*/

        public void InfoToDblog(string desc, string logType, string trnCode, string usercode)
        {
            try
            {
                addDbLog(desc, Utility.STATUS_SUCCESS, logType, trnCode, usercode);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }


        public void ErrorToDblog(string desc, string logType, string trnCode, string usercode)
        {
            try
            {
                addDbLog(desc, Utility.STATUS_FAILED, logType, trnCode, usercode);

            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }
    }
}

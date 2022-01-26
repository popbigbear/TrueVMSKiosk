using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThaiNationalIDCard;

namespace TrueVMS
{    
    public class IDCardManager
    {
        private static ThaiIDCard idcard;
        private Logger logger;

        public IDCardManager()
        {
            logger = new Logger(this.GetType().Name);

            logger.Info("Initial id card reader...");
            idcard = new ThaiIDCard();
        }

        public ThaiIDCard Idcard { get => idcard; set => idcard = value; }

        public string getReaderName()
        {
            try
            {
                string[] readers = idcard.GetReaders();

                if (readers == null)
                {
                    logger.Error("Can not get id card reader");
                    return null;
                }
                if (readers.Length > 1)
                {
                    logger.Error("Get readers count :"+readers.Length);
                    throw new Exception("Get more than one id card reader, Please contact administrator");
                }
                
                return readers[0];
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw ex;
            }
        }
    }
}

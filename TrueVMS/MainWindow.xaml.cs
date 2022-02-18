using Json.Net;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ThaiNationalIDCard;
using TrueVMS.Model;
using WPFMediaKit.DirectShow.Controls;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace TrueVMS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Logger logger;
        public static string version = "";
        public static string buildDate = "";
        public static string LocationCode = "";
        public static string KioskCode = "";
        public static string FloorCode = "";
        private string BuildingCode = "";
        private static Panel CURRENT_PANEL = null;

        private static int PANEL_MAX_WIDTH = 1280;
        private static int PANEL_MAX_HEIGHT = 924;
        private static string FONT_NAME = ConfigurationManager.AppSettings["FontName"];

        private static bool IS_DEBUG = Convert.ToBoolean(ConfigurationManager.AppSettings["isDebug"]);

        private static int TimeoutPanelEntry = 30;

        private static int TimeoutPanelScanQR = 30;
        private static int TimeoutPanelResendQR = 30;

        private static int TimeoutPanelPrivacyMarketing = 30;
        private static int TimeoutPanelPrivacySensitive = 30;
        private static int TimeoutPanelWelcomeToTrueIDC = 30;
        private static int TimeoutPanelTC = 30;
        private static int TimeoutPanelCustomerEnterIDcard = 30;
        private static int TimeoutPanelEntryOTP = 30;
        private static int TimeoutPanelEntryQR = 30;
        private static int TimeoutPanelDisplayProject = 30;
        private static int TimeoutPanelDisplayError = 30;

        private static int TimeoutPanelReturnCard = 30;
        private static int TimeoutPanelReturnCardSuccess = 30;
        private static int TimeoutPanelSelectProject = 30;

        public static int LANGUAGE = 1;//0:Eng,1=Thai
        public static int LANGUAGE_THAI = 1;
        public static int LANGUAGE_ENG = 0;

        private static int QRTIMEOUT = -15;


        private static int RESEND_OTP = 0;
        private static int RESEND_QR = 0;
        private static int resendOTPCount = 120;
        private static int resendQRCount = 120;
        private static int MASTER_RESEND_OTP_COUNT;
        private static int MASTER_RESEND_QR_COUNT;


        UInt32 HandComm;
        private string FEEDDEVICE_COMPORT = "COM2";
        private uint BAUT = 9600;



        DispatcherTimer timer;
        DispatcherTimer resendOtpTimer;
        DispatcherTimer resendQrTimer;
        DispatcherTimer timerCallReturnCardDll;
        DispatcherTimer amAliveTimer;
        int timeWaitForCallDll = 0;

        private int AUTO_CANCEL_COUNT;


        private Label targetTextbox;
        private int WORKING_TYPE;
        private readonly int WORKING_TYPE_CUSTOMER = 0;
        private readonly int WORKING_TYPE_WALKIN = 1;


        IDCardManager idm;
        ThaiIDCard idcard;
        private string RFID_READER_COMPORT;
        private string SERVER_API_URL;
        private int WALKIN_OBJ;
        private string WALKIN_OBJ_TXT = "";
        private WorkpermitVw WORKPERMIT = null;
        private CustProjectVw PROJECT;
        private CustStaffVw STAFFEMER;
        private List<CustProjectVw> ALL_PROJECT;
        private WorkpermitStaffMappingVw STAFF;
        private List<WorkpermitVw> ALL_WORKPERMIT = null;
        private string idCardOrPassportNumber;
        private string cardType;
        private string NodeType = "KIOSK";
        private string NodeIP = "1.1.1.1";


        private static bool keyboardJustOpen = false;
        //private string TC_VERSION = "1.0";

        AlertBox alertWindow = new AlertBox();
        //private string PRIVACY_MARKETING_VERSION = "1.0";
        //private string PRIVACY_SENSITIVE_VERSION = "1.0";


        public delegate void NextPrimeDelegate();

        TCFileVersionModel LAST_TC_TH = null;
        TCFileVersionModel LAST_PRIVACY_MARKETING_TH = null;
        TCFileVersionModel LAST_PRIVACY_SENSITIVE_TH = null;
        TCFileVersionModel LAST_TC_EN = null;
        TCFileVersionModel LAST_PRIVACY_MARKETING_EN = null;
        TCFileVersionModel LAST_PRIVACY_SENSITIVE_EN = null;

        double KeyboardVerOffset = Convert.ToDouble(ConfigurationManager.AppSettings["KeyboardVerOffset"]);
        double KeyboardHorOffset = Convert.ToDouble(ConfigurationManager.AppSettings["KeyboardHorOffset"]);


        public MainWindow()
        {
            InitializeComponent();
            InitialScreen();
        }


        private void InitialScreen()
        {
            InitialLogger();

            logger.Info("InitialConfiguration");
            initialConfiguration();

            logger.Info("initialHardware");
            initialHardware();

            //decodeQR("202109130080&1234567890123&ID&09/11/2564 16:33:11&72#Emer");


            ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

            logger.Info("InitialWindows");
            InitialWindows();

            logger.Info("InitialTime");
            InitialTime();


            logger.Info("InitialHeaderPanel");
            InitialHeaderPanel();


            logger.Info("InitialBackgroudImage");
            InitialBackgroudImage();

            logger.Info("InitialVideo");
            InitialVideo();

            InitialLabel();

            panelScreenSaver.Visibility = Visibility.Visible;
            
            setPanelProperty(panelEntry, System.Windows.Visibility.Hidden);
            setPanelProperty(panelEnterIDCardOrPassport, System.Windows.Visibility.Hidden);
            setPanelProperty(panelTC, System.Windows.Visibility.Hidden);
            setPanelProperty(panelScanQR, System.Windows.Visibility.Hidden);
            setPanelProperty(panelResendQR, System.Windows.Visibility.Hidden);
            setPanelProperty(panelEntryOTP, System.Windows.Visibility.Hidden);
            setPanelProperty(panelEntryQR, System.Windows.Visibility.Hidden);
            setPanelProperty(panelDisplayProject, System.Windows.Visibility.Hidden);
            setPanelProperty(panelDisplayError, System.Windows.Visibility.Hidden);
            setPanelProperty(panelPrivacyMarketing, System.Windows.Visibility.Hidden);
            setPanelProperty(panelPrivacySensitive, System.Windows.Visibility.Hidden);
            setPanelProperty(panelWelcomeTrueIDC, System.Windows.Visibility.Hidden);
            setPanelProperty(panelReturnCard, System.Windows.Visibility.Hidden);
            setPanelProperty(panelReturnCardSuccess, System.Windows.Visibility.Hidden);
            setPanelProperty(panelSelectProject, System.Windows.Visibility.Hidden);
            setPanelProperty(panelSelectWorkpermit, System.Windows.Visibility.Hidden);





            panelFullKeyboardPopup.Visibility = System.Windows.Visibility.Hidden;
            panelSmallKeyboardPopup.Visibility = System.Windows.Visibility.Hidden;
            panelFotter.Visibility = Visibility.Hidden;
            panelHeader.Visibility = Visibility.Hidden;

            setLabelProperty(lblHeaderTimeText);
            setLabelProperty(lblHeaderTimeout);
            setLabelProperty(lblTitle);
            setLabelProperty(lblTime);
            setLabelProperty(lblEnterIDCardOrPassportTitle);
            setLabelProperty(lblEnterIDCardOrPassportDesc);
            setLabelProperty(lblIDCardNumber);
            setLabelProperty(lblTitlepanelTC);
            setLabelProperty(lblTitlepanelScanQR);
            setLabelProperty(lblOTPpanelScanQR);
            setLabelProperty(lblOTPpanelScanQR_panelEntryQR);
            setLabelProperty(lblOTPpanelEntryOTP);
            setLabelProperty(lblTitlePanelDisplayProject);
            setLabelProperty(lblTitlePanelDisplayProject2);
            setLabelProperty(lblTitlePanelDisplayProject3);
            setLabelProperty(lblTitlePanelDisplayProject3_ROOM);
            setLabelProperty(lblTitlePanelDisplayProject4);
            setLabelProperty(lblTitlePanelDisplayProject5);
            setLabelProperty(lblTitlePanelDisplayProject6);
            setLabelProperty(lblTitlePanelDisplayProject7);
            setLabelProperty(lblTitlePanelWelcomeTrueIDC);
            setLabelProperty(lblTitlePanelWelcomeTrueIDC2);
            setLabelProperty(lblTitlePanelWelcomeTrueIDC3);

            setLabelProperty(lblTitlePanelDisplayError);
            setLabelProperty(lblTitlePanelReturnCardSuccess);

            setLabelProperty(lblTitlePanelSelectProject);
            setLabelProperty(lblTitlePanelSelectProject2);
            setLabelProperty(lblTitlePanelSelectWorkpermit);
            setLabelProperty(lblTitlePanelSelectWorkpermit2);



            setLabelProperty(lblRequestNewOTP);
            setLabelProperty(lblTitlepanelResendQR);
            setLabelProperty(lblTitlepanelResendQR2);
            setLabelProperty(lblTitlepanelResendQR3);

            setLabelProperty(lblTitlepanelEntryOTP);
            setLabelProperty(lblTitlepanelEntryOTP2);
            setLabelProperty(lblTitlepanelEntryOTP3);
            setLabelProperty(lblTitlepanelEntryOTP4);


            setLabelProperty(lblTitlepanelEntryQR);
            setLabelProperty(lblTitlepanelEntryQR1);
            setLabelProperty(lblTitlepanelEntryQR2);
            setLabelProperty(lblTitlepanelEntryQR3);
            setLabelProperty(lblTitlepanelEntryQR4);

            setLabelProperty(lblTitlePanelReturnCard);
            setLabelProperty(lblTitlePanelReturnCardSuccess);

            setLabelProperty(lblTitlePanelDisplayError2);
            setLabelProperty(lblTitlePanelDisplayError3);
            setLabelProperty(lblTitlePanelDisplayError4);
            setLabelProperty(lblTitlePanelDisplayError5);
            setLabelProperty(lblTitlePanelDisplayError6);
            setLabelProperty(lblTitlePanelDisplayError7);
            setLabelProperty(lblTitlePanelPrivacySensitive);
            setLabelProperty(lblTitlePanelPrivacyMarketing);

            setLabelProperty(lblRequestNewOTP_panelEntryOTP);
            lblRequestNewOTP_panelEntryOTP.Foreground = new SolidColorBrush(Colors.Gray);
            setLabelProperty(lblRequestNewQR_panelEntryQR);
            lblRequestNewQR_panelEntryQR.Foreground = new SolidColorBrush(Colors.Gray);


            lblTitlePanelDisplayProject.FontSize = 36;
            lblTitlePanelDisplayProject2.FontSize = 36;
            lblTitlePanelDisplayProject3.FontSize = 32;
            lblTitlePanelDisplayProject3_ROOM.FontSize = 32;
            lblTitlePanelDisplayProject4.FontSize = 32;
            lblTitlePanelDisplayProject5.FontSize = 32;
            lblTitlePanelDisplayProject6.FontSize = 32;
            lblTitlePanelDisplayProject7.FontSize = 32;
            lblTitlePanelPrivacySensitive.FontSize = 36;
            lblTitlePanelPrivacyMarketing.FontSize = 36;
            lblTitlePanelWelcomeTrueIDC.FontSize = 36;
            lblTitlePanelWelcomeTrueIDC2.FontSize = 32;
            lblTitlePanelWelcomeTrueIDC3.FontSize = 32;

            lblTitlePanelReturnCardSuccess.FontSize = 36;

            lblTitlePanelSelectProject.FontSize = 36;
            lblTitlePanelSelectProject2.FontSize = 32;

            lblTitlePanelSelectWorkpermit.FontSize = 36;
            lblTitlePanelSelectWorkpermit2.FontSize = 32;

            lblTitlePanelDisplayError.FontSize = 36;
            lblTitlePanelDisplayError2.FontSize = 36;
            lblTitlePanelDisplayError3.FontSize = 32;
            lblTitlePanelDisplayError4.FontSize = 32;
            lblTitlePanelDisplayError5.FontSize = 32;
            lblTitlePanelDisplayError6.FontSize = 32;
            lblTitlePanelDisplayError7.FontSize = 32;

            lblTitle.FontSize = 36;
            lblTitlepanelResendQR.FontSize = 36;
            lblTitlepanelResendQR2.FontSize = 36;
            lblTitlepanelResendQR3.FontSize = 36;
            lblTitlepanelTC.FontSize = 36;
            lblTitlepanelScanQR.FontSize = 36;
            lblTitlepanelEntryOTP.FontSize = 36;
            lblEnterIDCardOrPassportTitle.FontSize = 36;
            lblEnterIDCardOrPassportDesc.FontSize = 32;
            lblIDCardNumber.FontSize = 46;
            lblOTPpanelScanQR.FontSize = 46;
            lblOTPpanelScanQR_panelEntryQR.FontSize = 46;
            lblOTPpanelEntryOTP.FontSize = 46;
            lblRequestNewOTP.FontSize = 36;
            lblTitlePanelReturnCard.FontSize = 36;

            lblTitlepanelEntryOTP2.FontSize = 32;
            lblTitlepanelEntryOTP3.FontSize = 32;
            lblTitlepanelEntryOTP4.FontSize = 32;

            lblTitlepanelEntryQR.FontSize = 36;
            lblTitlepanelEntryQR1.FontSize = 36;
            lblTitlepanelEntryQR2.FontSize = 32;
            lblTitlepanelEntryQR3.FontSize = 32;
            lblTitlepanelEntryQR4.FontSize = 32;


            lblRequestNewOTP_panelEntryOTP.FontSize = 32;
            lblRequestNewQR_panelEntryQR.FontSize = 32;


            setButtonProperty(btnCustomer);
            setButtonProperty(btnWalkin);
            setButtonProperty(btnResendQRByEmail);
            setButtonProperty(btnResendQRBySMS);

            setButtonProperty(btnReturnCard);
            setButtonProperty(btnHome1);
            setButtonProperty(btnHome2);
            setButtonProperty(btnHome3);
            setButtonProperty(btnHome4);
            setButtonProperty(btnHome5);
            setButtonProperty(btnHome6);
            setButtonProperty(btnHome7);
            setButtonProperty(btnHome9);
            setButtonProperty(btnHome10);
            setButtonProperty(btnHome11);
            setButtonProperty(btnHome12);
            setButtonProperty(btnHome13);
            setButtonProperty(btnHome51);
            setButtonProperty(btnHome52);

            setButtonProperty(btnNext_panelDisplayProject);
            setButtonProperty(btnNext_panelTC);
            setButtonProperty(btnNext_panelEnterIDCardOrPassport);
            setButtonProperty(btnNext_panelScanQR);
            setButtonProperty(btnNext_panelEntryOTP);
            setButtonProperty(btnNext_panelEntryQR);
            setButtonProperty(btnNext_panelReturnCard);
            setButtonProperty(btnNext_panelSelectProject);
            setButtonProperty(btnNext_panelSelectWorkpermit);

            setButtonProperty(btnSkip_panelPrivacySensitive);
            setButtonProperty(btnNext_panelPrivacySensitive);
            setButtonProperty(btnSkip_panelPrivacyMarketing);
            setButtonProperty(btnNext_panelPrivacyMarketing);




            btnFlagEng.Opacity = 0.5;


            if (popupNumPad != null)
                popupNumPad.IsOpen = false;

            if (fullKeyboardPopup != null)
                fullKeyboardPopup.IsOpen = false;




            ActionLogModel model = newActionLogModel();
            model.Event = "Kiosk start";
            model.SubEvent = "Application started";
            model.Description = "Kiosk " + KioskCode + " is started. IP :" + NodeIP + " Location ID :" + LocationCode + " Building ID :" + BuildingCode + ".";
            var task = Task.Run(async () => await addActionLog(model));
        }

        private void initialHardware()
        {
            try
            {
                idm = new IDCardManager();
                this.idcard = idm.Idcard;
                idcard.MonitorStart(idm.getReaderName());
                idcard.eventCardInsertedWithPhoto += new handleCardInserted(CardInserted);
            }
            catch (Exception ex)
            {
                ActionLogModel model = newActionLogModel();
                model.Event = "Hardware";
                model.SubEvent = "ID card reader device error";
                model.Description = ex.ToString();
                var task = Task.Run(async () => await addActionLog(model));

                logger.Error(ex.ToString());
                logger.ErrorToDblog(ex.ToString(), Logger.LOG_SYSTEM, null, null);
            }


            try
            {
                logger.Info("This version use dll for connect card dispenser");
                //openFeedCardDevice();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                logger.ErrorToDblog(ex.ToString(), Logger.LOG_SYSTEM, null, null);
            }
        }

        private void initialConfiguration()
        {
            try
            {
                TimeoutPanelEntry = Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutPanelEntry"]);
                TimeoutPanelCustomerEnterIDcard = Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutPanelCustomerEnterIDcard"]);
                TimeoutPanelTC = Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutPanelTC"]);
                TimeoutPanelScanQR = Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutPanelScanQR"]);
                TimeoutPanelResendQR = Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutPanelResendQR"]);
                TimeoutPanelEntryOTP = Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutPanelEntryOTP"]);
                TimeoutPanelEntryQR = Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutPanelEntryQR"]);
                TimeoutPanelDisplayProject = Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutPanelDisplayProject"]);
                TimeoutPanelDisplayError = Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutPanelDisplayError"]);

                TimeoutPanelPrivacyMarketing = Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutPanelPrivacyMarketing"]);
                TimeoutPanelPrivacySensitive = Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutPanelPrivacySensitive"]);
                TimeoutPanelWelcomeToTrueIDC = Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutPanelWelcomeToTrueIDC"]);

                TimeoutPanelReturnCard = Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutPanelReturnCard"]);
                TimeoutPanelReturnCardSuccess = Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutPanelReturnCardSuccess"]);
                TimeoutPanelSelectProject = Convert.ToInt32(ConfigurationManager.AppSettings["TimeoutPanelSelectProject"]);

                

                MASTER_RESEND_OTP_COUNT = Convert.ToInt32(ConfigurationManager.AppSettings["ResendOTPCount"]);
                resendOTPCount = MASTER_RESEND_OTP_COUNT;

                MASTER_RESEND_QR_COUNT = Convert.ToInt32(ConfigurationManager.AppSettings["ResendQRCount"]);
                resendQRCount = MASTER_RESEND_QR_COUNT;

                RFID_READER_COMPORT = Convert.ToString(ConfigurationManager.AppSettings["RFID_READER_COMPORT"]);
                FEEDDEVICE_COMPORT = Convert.ToString(ConfigurationManager.AppSettings["FEEDDEVICE_COMPORT"]);
                BAUT = Convert.ToUInt32(ConfigurationManager.AppSettings["BAUT"]);

                SERVER_API_URL = Convert.ToString(ConfigurationManager.AppSettings["SERVER_API_URL"]);

                LAST_TC_TH = Utility.getTermAndConditionTextThai(SERVER_API_URL);
                LAST_PRIVACY_MARKETING_TH = Utility.getPrivacyMarketingTextThai(SERVER_API_URL);
                LAST_PRIVACY_SENSITIVE_TH = Utility.getPrivacySensitiveTextThai(SERVER_API_URL);

                LAST_TC_EN = Utility.getTermAndConditionTextEng(SERVER_API_URL);
                LAST_PRIVACY_MARKETING_EN = Utility.getPrivacyMarketingTextEng(SERVER_API_URL);
                LAST_PRIVACY_SENSITIVE_EN = Utility.getPrivacySensitiveTextEng(SERVER_API_URL);

                //LAST_PRIVACY_MARKETING_TH.Version = "9.9";
                //LAST_PRIVACY_SENSITIVE_TH.Version = "9.9";
                //LAST_TC_TH.Version = "9.9";


                try
                {
                    QRTIMEOUT = -1 * Convert.ToInt32(ConfigurationManager.AppSettings["QRTIMEOUT"]);

                    logger.Info("Last term and cond version :" + LAST_TC_TH.Version);
                    if (IS_DEBUG)
                    {
                        logger.Info(LAST_TC_TH.HtmlDetail);
                    }
                }
                catch {
                    QRTIMEOUT = -15;
                }


                logger.Info("QRTIMEOUT :" + QRTIMEOUT);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                logger.ErrorToDblog(ex.ToString(), Logger.LOG_SYSTEM, null, null);
            }
        }


        internal static bool ValidateServerCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private void InitialLabel()
        {
            setWalkinObjective();

            if (LANGUAGE == LANGUAGE_THAI)
            {
                btnCustomer.Content = "ลูกค้า / ผู้เยี่ยมชม IDC";
                btnWalkin.Content = "เจ้าหน้าที่ Walk - in";
                btnReturnCard.Content = "คืนบัตร";
                btnHome1.Content = "หน้าหลัก";
                btnHome2.Content = "หน้าหลัก";
                btnHome3.Content = "หน้าหลัก";
                btnHome4.Content = "หน้าหลัก";
                btnHome5.Content = "หน้าหลัก";
                btnHome51.Content = "หน้าหลัก";
                btnHome52.Content = "หน้าหลัก";
                btnHome6.Content = "หน้าหลัก";
                btnHome7.Content = "หน้าหลัก";
                btnHome8.Content = "หน้าหลัก";
                btnHome9.Content = "หน้าหลัก";
                btnHome10.Content = "หน้าหลัก";
                btnHome11.Content = "หน้าหลัก";
                btnHome12.Content = "หน้าหลัก";
                btnHome13.Content = "หน้าหลัก";

                btnResendQRByEmail.Content = "Email";
                btnResendQRBySMS.Content = "SMS";

                btnNext_panelTC.Content = "ยอมรับเงื่อนไขและดำเนินการต่อ";
                btnNext_panelEnterIDCardOrPassport.Content = "ต่อไป";
                btnNext_panelScanQR.Content = "ต่อไป";
                btnNext_panelEntryOTP.Content = "ต่อไป";
                btnNext_panelEntryQR.Content = "ต่อไป";
                btnNext_panelDisplayProject.Content = "ต่อไป";
                btnNext_panelReturnCard.Content = "ต่อไป";
                btnNext_panelSelectProject.Content = "ต่อไป";
                btnNext_panelSelectWorkpermit.Content = "ต่อไป";

                btnSkip_panelPrivacySensitive.Content = "ข้าม";
                btnSkip_panelPrivacyMarketing.Content = "ข้าม";
                btnNext_panelPrivacySensitive.Content = "ยินยอม";
                btnNext_panelPrivacyMarketing.Content = "ยินยอม";

                lblHeaderTimeText.Content = "ท่านมีเวลาดำเนินการ              วินาที";
                lblEnterIDCardOrPassportTitle.Content = "กรุณาระบุข้อมูลเพื่อยืนยันตัวตน";
                lblEnterIDCardOrPassportDesc.Content = "โดยใช้รหัสประจำตัวประชาชน หรือ หมายเลขพาสปอร์ต";
                lblIDCardNumber.Content = "เสียบบัตรหรือกดตัวเลข(แตะที่นี่)";

                lblOTPpanelScanQR.Content = "หรือกรอกรหัส OTP ที่ได้รับ(แตะที่นี่)";
                lblOTPpanelScanQR_panelEntryQR.Content = "หรือกรอกรหัส OTP ที่ได้รับ(แตะที่นี่)";
                lblTitlepanelTC.Content = "ข้อตกลงการใช้งานระบบ";
                lblTitlepanelScanQR.Content = "กรุณา SCAN QR-CODE ที่ท่านได้รับ";
                lblRequestNewOTP.Content = "ขอรับ QR code ใหม่";

                lblTitle.Content = "กรุณาเลือกประเภทการใช้งาน";

                lblOTPpanelEntryOTP.Content = "รหัส OTP ที่ได้รับ";

                lblRequestNewOTP_panelEntryOTP.Content = "ขอรับโค้ดใหม่";
                lblRequestNewQR_panelEntryQR.Content = "ขอรับโค้ดใหม่";

                lblTitlePanelSelectProject.Content = "กรุณาเลือกโปรเจคที่จะเข้าดำเนินการ";
                lblTitlePanelSelectProject2.Content = "";
                lblTitlePanelSelectWorkpermit.Content = "กรุณาเลือก Workpermit ที่จะเข้าดำเนินการ";
                lblTitlePanelSelectWorkpermit2.Content = "";


                lblTitlepanelResendQR.Content = "ระบบจะจัดส่ง QR-Code หรือ OTP";
                lblTitlepanelResendQR2.Content = "เพื่อให้ท่านยืนยันตัวตน ผ่านข้อมูลที่ท่านได้ลงทะเบียนไว้";
                lblTitlepanelResendQR3.Content = "กรุณาเลือกวิธีรับ";

                lblTitlepanelEntryOTP.Content = "กรุณากรอก OTP ที่ท่านได้รับ";
                lblTitlepanelEntryOTP2.Content = "ระบบจัดส่ง QR-Code ไปยังหมายเลข ";
                lblTitlepanelEntryOTP3.Content = "กรุณาตรวจสอบ SMS เพื่อดำเนินการต่อ หรือขอรับ QR-Code ใหม่";
                lblTitlepanelEntryOTP4.Content = "ได้ในอีก " + MASTER_RESEND_OTP_COUNT + " วินาที (ครั้งที่ 0/3)";

                lblTitlepanelEntryQR.Content = "กรุณาสแกน QR-Code ที่ท่านได้รับ";
                lblTitlepanelEntryQR1.Content = "หรือกรอกรหัส OTP ที่ท่านได้รับ";
                lblTitlepanelEntryQR2.Content = "ระบบจัดส่ง QR-Code และรหัส OTP ไปยังอีเมล์ ";
                lblTitlepanelEntryQR3.Content = "กรุณาตรวจสอบอีเมล์เพื่อดำเนินการต่อ หรือขอรับ QR-Code ใหม่";
                lblTitlepanelEntryQR4.Content = "ได้ในอีก " + MASTER_RESEND_QR_COUNT + " วินาที (ครั้งที่ 0/3)";

                lblTitlePanelDisplayProject.Content = "ข้อมูล QR-Code ถูกต้อง";

                lblTitlePanelPrivacySensitive.Content = "DATA PRIVACY : SENSITIVE DATA";
                lblTitlePanelPrivacyMarketing.Content = "DATA PRIVACY : MARKETING PROPOSE";

                lblTitlePanelWelcomeTrueIDC.Content = "ยินดีต้อนรับสู่ True IDC";
                lblTitlePanelWelcomeTrueIDC2.Content = "กรุณารับบัตร";
                lblTitlePanelWelcomeTrueIDC3.Content = "กรุณาตรวจสอบและรับบัตรคืนจากตู้(ถ้ามี)";

                lblTitlePanelReturnCard.Content = "กรุณาเสียบบัตรคืนที่ช่อง";

                lblTitlePanelReturnCardSuccess.Content = "ขอบพระคุณที่ใช้บริการ";

                lblTCPopup.Text = LAST_TC_TH.HtmlDetail;
                lblPrivacyMarketing.Text = LAST_PRIVACY_MARKETING_TH.HtmlDetail;
                lblPrivacySensitive.Text = LAST_PRIVACY_SENSITIVE_TH.HtmlDetail;

                ImageBrush ib = new ImageBrush();
                String fileUrl = @"Resources\Screen030\TCPopBckground.png";
                ib.ImageSource = new BitmapImage(new Uri(fileUrl, UriKind.RelativeOrAbsolute));
                borderTc.Background = ib;

                fileUrl = @"Resources\Screen090\backgroundConsent.png";
                ib.ImageSource = new BitmapImage(new Uri(fileUrl, UriKind.RelativeOrAbsolute));
                borderPrivacySensitive.Background = ib;
                borderPrivacyMarketing.Background = ib;

            }
            else
            {
                btnCustomer.Content = "Customer / Visitor IDC";
                btnWalkin.Content = "Staff Walk - in";
                btnReturnCard.Content = "Return Card";
                btnHome1.Content = "Home";
                btnHome2.Content = "Home";
                btnHome3.Content = "Home";
                btnHome4.Content = "Home";
                btnHome5.Content = "Home";
                btnHome6.Content = "Home";
                btnHome7.Content = "Home";
                btnHome8.Content = "Home";
                btnHome9.Content = "Home";
                btnHome10.Content = "Home";
                btnHome11.Content = "Home";
                btnHome12.Content = "Home";
                btnHome13.Content = "Home";
                btnHome51.Content = "Home";
                btnHome52.Content = "Home";

                btnResendQRByEmail.Content = "Email";
                btnResendQRBySMS.Content = "SMS";


                lblTitle.Content = "Please select access type";

                btnNext_panelTC.Content = "Accept T&C and continue.";

                btnNext_panelEnterIDCardOrPassport.Content = "Next";
                btnNext_panelEntryOTP.Content = "Next";
                btnNext_panelScanQR.Content = "Next";
                btnNext_panelEntryQR.Content = "Next";
                btnNext_panelDisplayProject.Content = "Next";
                btnNext_panelReturnCard.Content = "Next";
                btnSkip_panelPrivacySensitive.Content = "Skip";
                btnSkip_panelPrivacyMarketing.Content = "Skip";
                btnNext_panelPrivacySensitive.Content = "Accept";
                btnNext_panelPrivacyMarketing.Content = "Accept";
                btnNext_panelSelectProject.Content = "Next";
                btnNext_panelSelectWorkpermit.Content = "Next";

                lblHeaderTimeText.Content = "Time remaining                Second";
                lblEnterIDCardOrPassportTitle.Content = "Please verify yourself";
                lblEnterIDCardOrPassportDesc.Content = "By your ID-Card or passport number";
                lblIDCardNumber.Content = "Insert card or entry number";
                lblTitlepanelTC.Content = "Terms and condition";
                lblTitlepanelScanQR.Content = "Scan your work permit QR-Code";
                lblOTPpanelScanQR.Content = "Or enter your OTP (Touch here)";
                lblOTPpanelScanQR_panelEntryQR.Content = "Or enter your OTP (Touch here)";



                lblTitlepanelResendQR.Content = "System will send QR-Code or OTP";
                lblTitlepanelResendQR2.Content = "From your register information";
                lblTitlepanelResendQR3.Content = "Please select method";

                lblRequestNewOTP.Content = "Request new QR-Code ";
                lblRequestNewOTP_panelEntryOTP.Content = "Request new QR-Code";
                lblRequestNewQR_panelEntryQR.Content = "Request new QR-Code";

                lblOTPpanelEntryOTP.Content = "Entry OTP";


                lblTitlePanelSelectProject.Content = "Select your project";
                lblTitlePanelSelectProject2.Content = "";
                lblTitlePanelSelectWorkpermit.Content = "Select your workpermit";
                lblTitlePanelSelectWorkpermit2.Content = "";

                lblTitlepanelEntryOTP.Content = "Please entry OTP ";
                lblTitlepanelEntryOTP2.Content = "System already sent QR-Code to ";
                lblTitlepanelEntryOTP3.Content = "Please check your SMS or touch for receive new QR-Code";
                lblTitlepanelEntryOTP4.Content = "in " + MASTER_RESEND_OTP_COUNT + " second (0/3)";


                lblTitlepanelEntryQR.Content = "Scan your QR-Code";
                lblTitlepanelEntryQR1.Content = "Or entry OTP ";
                lblTitlepanelEntryQR2.Content = "System already sent QR-Code and OTP to email : ";
                lblTitlepanelEntryQR3.Content = "Please check your email or touch for receive new QR-Code";
                lblTitlepanelEntryQR4.Content = "in " + MASTER_RESEND_QR_COUNT + " second (0/3)";


                lblTitlePanelDisplayProject.Content = "QR-Code correct";

                lblTitlePanelWelcomeTrueIDC.Content = "Welcome to True IDC";
                lblTitlePanelWelcomeTrueIDC2.Content = "Receive card";
                lblTitlePanelWelcomeTrueIDC3.Content = "Please check card and get your ID-Card " +
                    "back";

                lblTitlePanelReturnCard.Content = "Please insert card for return";
                lblTitlePanelReturnCardSuccess.Content = "Thank you";

                lblTCPopup.Text = LAST_TC_EN.HtmlDetail;
                lblPrivacyMarketing.Text = LAST_PRIVACY_MARKETING_EN.HtmlDetail;
                lblPrivacySensitive.Text = LAST_PRIVACY_SENSITIVE_EN.HtmlDetail;

                ImageBrush ib = new ImageBrush();
                String fileUrl = @"Resources\Screen030\TCPopBckground_en.png";
                ib.ImageSource = new BitmapImage(new Uri(fileUrl, UriKind.RelativeOrAbsolute));
                borderTc.Background = ib;



                fileUrl = @"Resources\Screen090\backgroundConsent_en.png";
                ib.ImageSource = new BitmapImage(new Uri(fileUrl, UriKind.RelativeOrAbsolute));
                borderPrivacySensitive.Background = ib;
                borderPrivacyMarketing.Background = ib;

                lblTitlePanelPrivacySensitive.Content = "DATA PRIVACY : SENSITIVE DATA";
                lblTitlePanelPrivacyMarketing.Content = "DATA PRIVACY : MARKETING PROPOSE";
            }
        }


        private void InitialHeaderPanel()
        {
            String fileUrl = Utility.getHeadTitleBackgroundImageFile();

            ImageBrush ib = new ImageBrush();
            ib.ImageSource = new BitmapImage(new Uri(fileUrl, UriKind.RelativeOrAbsolute));

            panelHeader.Background = ib;
        }


        private void InitialTime()
        {
            //lblTime.FontFamily = new FontFamily(FONT_NAME);
            //lblTime.FontSize = 22;
            string timeElapsedInstring = DateTime.Now.ToString("dd MMM yyyy HH:mm");
            updateTime(timeElapsedInstring);

            InitializeDispatchTimer();
        }


        private void InitialWindows()
        {
            if (!IS_DEBUG)
            {
                //this.WindowState = System.Windows.WindowState.Maximized;
                //this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                //this.WindowStyle = System.Windows.WindowStyle.None;
            }
        }


        public void CardInserted(Personal personal)
        {
            try
            {
                if (personal == null)
                {
                    if (idcard.ErrorCode() > 0)
                    {
                        string error = idcard.Error();
                        logger.Error("Can not read idcard " + error);
                        //alertWindow.setMessage("พบข้อผิดพลาด", "ไม่สามารถอ่านบัตรประชาชนได้", "Can not read idcard :" + error);
                        //alertWindow.ShowDialog();
                        //MessageBox.Show(idcard.Error());
                    }
                    return;
                }

                Dispatcher.Invoke(() =>
                {
                    lblIDCardNumber.Content = personal.Citizenid;
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                //alertWindow.setMessage("พบข้อผิดพลาด", "ไม่สามารถอ่านบัตรประชาชนได้", "Can not read idcard");
                //alertWindow.ShowDialog();
            }
        }




        public static string GetIPAddress()
        {
            try
            {
                string strHostName = "";
                strHostName = System.Net.Dns.GetHostName();

                IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);

                foreach (var ip in ipEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }

                return ConfigurationManager.AppSettings["NodeIP"];
                //string ipaddress = Convert.ToString(ipEntry.AddressList[1]);

                //return ipaddress.ToString();
            }
            catch (Exception ex)
            {
                return ConfigurationManager.AppSettings["NodeIP"]; // or IPAddress.None if you prefer
            }
            
        }


        private void InitialLogger()
        {
            logger = new Logger(this.GetType().Name);
            version = ConfigurationManager.AppSettings["version"];
            buildDate = ConfigurationManager.AppSettings["buildDate"];
            LocationCode = ConfigurationManager.AppSettings["LocationCode"];
            KioskCode = ConfigurationManager.AppSettings["KioskCode"];
            FloorCode = ConfigurationManager.AppSettings["FloorCode"];


            BuildingCode = ConfigurationManager.AppSettings["BuildingCode"];
            NodeType = ConfigurationManager.AppSettings["NodeType"];
            //NodeIP = ConfigurationManager.AppSettings["NodeIP"];
            NodeIP = GetIPAddress();



            logger.Info("********************************************************");
            //logger.InfoToDblog(Utility.CODE_000, Utility.LOG_TYPE_APPLICATION_GENERAL, Utility.START_APPLICATION, terminalCode);
            logger.Info("Application start at version :" + version + "[" + buildDate + "] Kiosk Code :" + KioskCode + " BuildingCode :"+ BuildingCode + " FloorCode :" + FloorCode);

            //lblProgramInfo.Content = "True VMS System Version " + version + " Last Update " + buildDate;

            lblProgramInfo.Content = "";
        }


        private void InitialBackgroudImage()
        {
            String fileUrl;
            ImageBrush ib;

            fileUrl = Utility.getFrmEntryBackgroundImageFile();
            ib = new ImageBrush();
            ib.ImageSource = new BitmapImage(new Uri(fileUrl, UriKind.RelativeOrAbsolute));
            panelEntry.Background = ib;
            panelEntryOTP.Background = ib;
            panelEntryQR.Background = ib;
            panelSelectProject.Background = ib;
            panelSelectWorkpermit.Background = ib;


            fileUrl = Utility.getFrmEnterIDCardOrPassportBackgroundImageFile();
            ib = new ImageBrush();
            ib.ImageSource = new BitmapImage(new Uri(fileUrl, UriKind.RelativeOrAbsolute));
            panelEnterIDCardOrPassport.Background = ib;
            panelResendQR.Background = ib;
            panelReturnCard.Background = ib;
            panelReturnCardSuccess.Background = ib;
            panelDisplayProject.Background = ib;



            fileUrl = Utility.getFrmTCBackgroundImageFile();
            ib = new ImageBrush();
            ib.ImageSource = new BitmapImage(new Uri(fileUrl, UriKind.RelativeOrAbsolute));
            panelTC.Background = ib;
            panelPrivacyMarketing.Background = ib;
            panelPrivacySensitive.Background = ib;
            panelScanQR.Background = ib;
            panelWelcomeTrueIDC.Background = ib;

           

            fileUrl = @"Resources\Keyboard\FullKeyboardBackground.png";
            ib = new ImageBrush();
            ib.ImageSource = new BitmapImage(new Uri(fileUrl, UriKind.RelativeOrAbsolute));
            panelFullKeyboardPopup.HorizontalAlignment = HorizontalAlignment.Center;
            double left = Convert.ToDouble( ConfigurationManager.AppSettings["KeyboardBgLeft"]);
            double top = Convert.ToDouble(ConfigurationManager.AppSettings["KeyboardBgTop"]);
            panelFullKeyboardPopup.Margin = new Thickness(left,top,0,0);
            //panelFullKeyboardPopup.HorizontalAlignment = HorizontalAlignment.Center;
            //panelFullKeyboardPopup.Orientation = Orientation.Horizontal;

            panelFullKeyboardPopup.Background = ib;


            double numpadLeft = Convert.ToDouble(ConfigurationManager.AppSettings["NumpadHorOffset"]);
            popupNumPad.HorizontalOffset = numpadLeft;
        }


        private void setPanelProperty(StackPanel panel, System.Windows.Visibility v)
        {
            panel.Width = PANEL_MAX_WIDTH;
            panel.Visibility = v;
            panel.Height = PANEL_MAX_HEIGHT;

        }



        private void setLabelProperty(Label lbl)
        {
            lbl.FontFamily = new FontFamily(FONT_NAME);
            lbl.FontSize = 20;
        }



        private void setButtonProperty(Button btn)
        {
            btn.FontFamily = new FontFamily(FONT_NAME);
            btn.FontSize = 28;
        }


        private void ShowEntryPanel()
        {
            clearControl();
            CURRENT_PANEL.Visibility = Visibility.Hidden;
            panelScreenSaver.Visibility = Visibility.Hidden;
            panelEntry.Visibility = Visibility.Visible;
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelEntry;
        }

        private void ShowTCPanel()
        {
            CURRENT_PANEL.Visibility = Visibility.Hidden;
            panelEnterIDCardOrPassport.Visibility = Visibility.Hidden;
            panelTC.Visibility = Visibility.Visible;
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelTC;
        }


        private void ShowScanQRPanel()
        {
            CURRENT_PANEL.Visibility = Visibility.Hidden;
            panelScanQR.Visibility = Visibility.Visible;
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelScanQR;

            readQRCode.Text = "";
            readQRCode.Focus();

        }

        private string decodeQR(string qrStr)
        {
            try
            {
                //qrStr = "44&A12345&PASSPORT&01/12/2021 14:06:09&416&Emer#";

                if (WORKING_TYPE!=WORKING_TYPE_CUSTOMER)
                {
                    logger.Error("Type missmatch between visitor type and qr");
                    return null;
                }


                if (!qrStr.Contains("&"))
                {
                    logger.Error("qr code not contain &");
                    return null;
                }
                else
                {

                    string[] splitEnd = qrStr.Split('#');

                    if(splitEnd.Length > 1)
                    {
                        qrStr = splitEnd[0];
                    }


                    string[] qrStrSplit = qrStr.Split('&');

                    logger.Info("Qr-Code & count is " + qrStrSplit.Length);

                    if (qrStrSplit.Length != 6)
                        return null;

                    string workpermitID = qrStrSplit[0];
                    string idcardPassport = qrStrSplit[1];
                    string cardType = qrStrSplit[2];
                    string validTime = qrStrSplit[3];

                    DateTime oDate = DateTime.ParseExact(qrStrSplit[3], "dd/MM/yyyy HH:mm:ss", null);


                    logger.Info("QRCode date is " + oDate.ToString("dd/MM/yyyy HH:mm:ss"));
                    logger.Info("QRTIMEOUT " + QRTIMEOUT);
                    logger.Info("DateTime.Now " + DateTime.Now);

                    if (oDate.Year > 2500)
                    {
                        oDate = oDate.AddYears(-543);
                    }


                    var ci = new CultureInfo("en-US");
                    DateTime dt = DateTime.Now;
                    string s = dt.ToString("dd/MM/yyyy HH:mm:ss", ci);
                    dt = DateTime.ParseExact(s, "dd/MM/yyyy HH:mm:ss", null);

                    if (dt.Year > 2500)
                    {
                        dt = dt.AddYears(-543);

                        logger.Info("dt " + dt);
                    }



                    logger.Info("oDate :" + oDate);
                    logger.Info("dt 2 :" + dt);
                    logger.Info("dt.AddMinutes(QRTIMEOUT) :" + dt.AddMinutes(QRTIMEOUT));
                    logger.Info("(oDate >= dt.AddMinutes(QRTIMEOUT)) :" + (oDate >= dt.AddMinutes(QRTIMEOUT)));

                    //pop check valid date
                    if (oDate >= dt.AddMinutes(QRTIMEOUT))
                    {
                        return workpermitID;
                    }
                    else
                    {
                        logger.Info("Qr-Code is expire");

                        return null;
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex.ToString());
                return null;
            }
        }



        private string decodeQREmer(string qrStr)
        {
            try
            {
                //qrStr = "44&A12345&PASSPORT&01/12/2021 15:06:09&416&Emer#";

                if (WORKING_TYPE != WORKING_TYPE_WALKIN)
                {
                    logger.Error("Type missmatch between visitor type and qr");
                    return null;
                }

                if (!qrStr.Contains("&"))
                {
                    logger.Error("qr code not contain &");
                    return null;
                }
                else
                {
                    string[] splitEnd = qrStr.Split('#');

                    if (splitEnd.Length > 1)
                    {
                        qrStr = splitEnd[0];
                    }


                    string[] qrStrSplit = qrStr.Split('&');

                    logger.Info("Qr-Code & count is " + qrStrSplit.Length);

                    if (qrStrSplit.Length != 6)
                        return null;

                    string projectId = qrStrSplit[0];
                    string idcardPassport = qrStrSplit[1];
                    string cardType = qrStrSplit[2];
                    string validTime = qrStrSplit[3];

                    DateTime oDate = DateTime.ParseExact(qrStrSplit[3], "dd/MM/yyyy HH:mm:ss", null);

                    logger.Info("QRCode date is " + oDate.ToString("dd/MM/yyyy HH:mm:ss"));
                    logger.Info("QRTIMEOUT " + QRTIMEOUT);
                    logger.Info("DateTime.Now " + DateTime.Now);

                    if (oDate.Year > 2500)
                    {
                        oDate = oDate.AddYears(-543);
                    }


                    var ci = new CultureInfo("en-US");
                    DateTime dt = DateTime.Now;
                    string s = dt.ToString("dd/MM/yyyy HH:mm:ss", ci);
                    dt = DateTime.ParseExact(s, "dd/MM/yyyy HH:mm:ss", null);

                    if (dt.Year > 2500)
                    {
                        dt = dt.AddYears(-543);

                        logger.Info("dt " + dt);
                    }



                    logger.Info("oDate :" + oDate);
                    logger.Info("dt 2 :" + dt);
                    logger.Info("dt.AddMinutes(QRTIMEOUT) :" + dt.AddMinutes(QRTIMEOUT));
                    logger.Info("(oDate >= dt.AddMinutes(QRTIMEOUT)) :" + (oDate >= dt.AddMinutes(QRTIMEOUT)));


                    //pop check valid date
                    if (oDate >= dt.AddMinutes(QRTIMEOUT))
                    {
                        return projectId;
                    }
                    else
                    {
                        logger.Info("Qr-Code is expire");

                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return null;
            }
        }


        private void loadErrorData()
        {
            if (LANGUAGE == LANGUAGE_THAI)
            {
                lblTitlePanelDisplayError2.Content = "Project Code : - ";
                lblTitlePanelDisplayError3.Content = "บริษัท พีจี เอ็นเตอร์ไพรซ์ จํากัด";
                lblTitlePanelDisplayError4.Content = "ยังไม่ถึงเวลาเข้าใช้งาน";
            }
            else
            {
                lblTitlePanelDisplayError2.Content = "Project Code : 000666 ";
                lblTitlePanelDisplayError3.Content = "บริษัท พีจี เอ็นเตอร์ไพรซ์ จํากัด";
                lblTitlePanelDisplayError4.Content = "Project time not yet";
            }
        }

        private void ShowDisplayErrorPanel()
        {
            CURRENT_PANEL.Visibility = Visibility.Hidden;
            loadErrorData();
            panelDisplayError.Visibility = Visibility.Visible;
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelDisplayError;
        }


        private void ShowPanelResendQR()
        {
            CURRENT_PANEL.Visibility = Visibility.Hidden;

            panelResendQR.Visibility = Visibility.Visible;
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelResendQR;
        }


        private void ShowPanelPrivacyMarketing()
        {
            CURRENT_PANEL.Visibility = Visibility.Hidden;
            panelPrivacyMarketing.Visibility = Visibility.Visible;
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelPrivacyMarketing;
        }
        private void ShowPanelPrivacySensitive()
        {
            CURRENT_PANEL.Visibility = Visibility.Hidden;
            panelPrivacySensitive.Visibility = Visibility.Visible;
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelPrivacySensitive;
        }






        private void ShowPanelReturnCardSuccess()
        {
            CURRENT_PANEL.Visibility = Visibility.Hidden;
            panelReturnCardSuccess.Visibility = Visibility.Visible;
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelReturnCardSuccess;
        }

        private List<MLocationFloor> getDeviceID(string workpermitid)
        {
            try
            {
                logger.Info("getDeviceID :" + workpermitid + " floor code :"+ FloorCode);

                var client = new RestClient(SERVER_API_URL + "/api/Kiosk/GetDoorByWorkpermitId/"+ workpermitid + ","+FloorCode);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);


                IEnumerable<MLocationFloor> m = JsonConvert.DeserializeObject<IEnumerable<MLocationFloor>>(response.Content);
                if (m != null)
                {
                    logger.Info("finish getDeviceID :" + workpermitid + " floor code :" + FloorCode);

                    return m.ToList<MLocationFloor>();
                }
                else
                {
                    return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return null;
            }
        }


        private List<MLocationFloor> getDeviceIDByProject(string projectid)
        {
            try
            {
                var client = new RestClient(SERVER_API_URL + "/api/Kiosk/GetDoorByProjectId/" + projectid + "," + FloorCode);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);


                IEnumerable<MLocationFloor> m = JsonConvert.DeserializeObject<IEnumerable<MLocationFloor>>(response.Content);
                if (m != null)
                {
                    return m.ToList<MLocationFloor>();
                }
                else
                {
                    return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return null;
            }
        }


        private void ShowPanelWelcomeToTrueIDC()
        {
            string card = "";

            try
            {
                logger.Info("Start call dll for feed card");
                //ยกเลิกการดีดบัตร ไปใช้การเรียก dll
                //feed();

                //อ่านเพื่อแจกบัตร

                //Workpermit ไม่มีทางเป็น null แล้ว
                /*
                if (WORKPERMIT == null)
                {
                    if (ALL_WORKPERMIT != null && ALL_WORKPERMIT.Count > 1)
                    {
                        WORKPERMIT = ALL_WORKPERMIT[0];
                        logger.Info("System get workpetmit more than one, so system select first workpermit");
                    }
                }*/

                List<MLocationFloor> devideID = null;
                DateTime start = DateTime.MinValue;
                DateTime end = DateTime.MinValue;
                string customerCode = "", CustStaffCardId = "", CustStaffCardType = "", docId = ""; 

                if (WORKPERMIT != null)
                {
                    devideID = getDeviceID(WORKPERMIT.WorkpermitId);
                    start = WORKPERMIT.WorkStartDatetime;
                    end = WORKPERMIT.WorkEndDatetime;
                    customerCode = WORKPERMIT.CustomerCode;
                    CustStaffCardId = STAFF.CustStaffCardId;
                    CustStaffCardType = STAFF.CustStaffCardType;
                    docId = WORKPERMIT.WorkpermitId;
                }
                else
                {
                    devideID = getDeviceIDByProject(Convert.ToString(PROJECT.CustProjectId));
                    start = PROJECT.CustProjectStart;
                    end = PROJECT.CustProjectEnd;
                    customerCode = PROJECT.CustomerCode;

                    CustStaffCardId = STAFFEMER.CustStaffCardId;
                    CustStaffCardType = STAFFEMER.CustStaffCardType;
                    docId = Convert.ToString(PROJECT.CustProjectId);
                }

                //if (!"".Equals(devideID))
                //{
                //    //devideID = devideID.Replace("\"", "");
                //    devideID = devideID.Replace(",", "\",\"");
                //}

                string jsonString = JsonConvert.SerializeObject(devideID);
                //string jsonString = JsonSerializer.Serialize(devideID);

                int walkingObjId = -1;
                if(WORKING_TYPE == WORKING_TYPE_WALKIN)
                {
                    //yy
                    //walkingObjId = Convert.ToInt32(cmbObjective.SelectedValue);
                    //logger.Info("cmbObjective.SelectedValue :" + cmbObjective.SelectedValue);
                    //logger.Info("cmbObjective.SelectedItem :" + cmbObjective.SelectedItem.ToString());
                    logger.Info("WALKIN_OBJ :" + WALKIN_OBJ);
                    walkingObjId = WALKIN_OBJ;
                }


                var taskFeedCard = Task.Run(async () => await feedCardAsync(start, end , customerCode
                    , CustStaffCardId, CustStaffCardType, docId, jsonString, walkingObjId));
                taskFeedCard.Wait();
                card = taskFeedCard.Result;
                taskFeedCard.Dispose();


                ActionLogModel model = newActionLogModel();

                if (WORKPERMIT != null)
                {
                    if (card != "")
                    {
                        model.Description = "Read card (" + card + ") for spent card complete (Staff id : " + CustStaffCardId + ", Workpermit id : " + WORKPERMIT.WorkpermitId + ")";
                        model.Event = "Spent Card";
                        model.SubEvent = "Spent Card Success";
                        model.WorkPermitID = Convert.ToInt64( WORKPERMIT.WorkpermitId);
                        model.CardNo = card;
                    }
                    else
                    {
                        model.Description = "Can not read card for spent card (Staff id : " + CustStaffCardId + ", Workpermit id : " + WORKPERMIT.WorkpermitId + ")";
                        model.Event = "Spent Card";
                        model.SubEvent = "Spent Card Not Success";
                    }
                }
                else
                {

                    if (card != "")
                    {
                        model.Description = "Read card (" + card + ") for spent card complete (Staff id : " + CustStaffCardId + ", Project id : " + PROJECT.CustProjectId + ")";
                        model.Event = "Spent Card";
                        model.SubEvent = "Spent Card Success";

                        model.WorkPermitID = Convert.ToInt64(PROJECT.CustProjectId);
                        model.walkinObjectiveID = walkingObjId;
                        model.CardNo = card;
                    }
                    else
                    {
                        model.Description = "Can not read card for spent card (Staff id : " + CustStaffCardId + ", Project id : " + PROJECT.CustProjectId + ")";
                        model.Event = "Spent Card";
                        model.SubEvent = "Spent Card Not Success";
                    }
                }

                //addActionLog(model);
                var task = Task.Run(async () => await addActionLog(model));
            }
            catch (Exception ex)
            {

                ActionLogModel model = newActionLogModel();
                logger.Error(ex.ToString());

                if(WORKPERMIT!=null)
                    model.Description = "Can not read card for spent card (Staff id : " + STAFF.CustStaffId + ", Workpermit id : " + WORKPERMIT.WorkpermitId + ")";
                else
                    model.Description = "Can not read card for spent card (Staff id : " + STAFFEMER.CustStaffId + ", Project id : " + PROJECT.CustProjectId + ")";

                model.Event = "Spent Card";
                model.SubEvent = "Spent Card Not Success";
                var task = Task.Run(async () => await addActionLog(model));
                logger.Info("Spent Card Not Success");


                if (IS_DEBUG)
                    MessageBox.Show(ex.ToString());

                try
                {
                    if ("102".Equals(ex.Message))
                    {
                        AlertBox aw = new AlertBox();
                        aw.setMessage("Error", "Spent Card", "ไม่มีบัตรว่าง ทำให้ไม่สามารถแจกบัตรได้");
                        aw.ShowDialog();
                    }
                    else
                    {

                        AlertBox aw = new AlertBox();
                        //aw.setMessage("Error", "Spent Card", "Spent Card Not Success : Can not connect device");
                        aw.setMessage("Error", "Spent Card Not Success", ex.Message);
                        aw.ShowDialog();
                    }
                }
                catch { }

                return;
            }

            if (card != "")
            {
                //pop call distribute card log
                var task2 = Task.Run(async () => await addDistributeCardLog("ISSUED", card));
                logger.Info("Add Distribute Card Log Success");
            }

            CURRENT_PANEL.Visibility = Visibility.Hidden;
            panelWelcomeTrueIDC.Visibility = Visibility.Visible;
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelWelcomeToTrueIDC;
        }


        private string roomFromDevide(List<MLocationFloor> devide)
        {
            try
            {
                string room = "";

                if(devide.Count < 1)
                {
                    return "";
                }

                int i = 0;
                foreach(MLocationRoom m in devide[0].MLocationRooms)
                {
                    room += m.Title+",";

                    ++i;

                    if (i == 8)
                    {
                        break;
                    }
                }

                if (devide[0].MLocationRooms.Count > 8)
                {
                    room += "...";
                }
                else
                {
                    room = room.Substring(0, room.Length - 1);
                }

                return room;

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return "";
            }

        }

        private void loadProjectData()
        {

            logger.Info("start loadProjectData");
            if (WORKPERMIT != null)
            {

                List<MLocationFloor> devide =  getDeviceID(WORKPERMIT.WorkpermitId);

                string rooms = roomFromDevide(devide);


                if (LANGUAGE == LANGUAGE_THAI)
                {
                    lblTitlePanelDisplayProject2.Content = "Workpermit Code : " + WORKPERMIT.WorkpermitCode;
                    lblTitlePanelDisplayProject3.Content = STAFF.CustStaffName;
                    lblTitlePanelDisplayProject3_ROOM.Content = WORKPERMIT.CustomerCode;
                    lblTitlePanelDisplayProject4.Content = WORKPERMIT.CustProjectName;
                    lblTitlePanelDisplayProject5.Content = "ห้อง " + rooms + "";
                    lblTitlePanelDisplayProject6.Content = "ระยะเวลาเข้าใช้งาน";
                    lblTitlePanelDisplayProject7.Content = WORKPERMIT.WorkStartDatetime.ToString("dd MMM yyyy HH:mm") + " ถึง " + WORKPERMIT.WorkEndDatetime.ToString("dd MMM yyyy HH:mm");
                }
                else
                {

                    lblTitlePanelDisplayProject2.Content = "Workpermit Code : " + WORKPERMIT.WorkpermitCode;
                    lblTitlePanelDisplayProject3.Content = STAFF.CustStaffName;
                    lblTitlePanelDisplayProject3_ROOM.Content = WORKPERMIT.CustomerCode;
                    lblTitlePanelDisplayProject4.Content = WORKPERMIT.CustProjectName;
                    lblTitlePanelDisplayProject5.Content = "Room " + rooms + "";
                    lblTitlePanelDisplayProject6.Content = "Working time";
                    lblTitlePanelDisplayProject7.Content = WORKPERMIT.WorkStartDatetime.ToString("dd MMM yyyy HH:mm") + " To " + WORKPERMIT.WorkEndDatetime.ToString("dd MMM yyyy HH:mm");


                }
            }
            else
            {

                List<MLocationFloor> devide = getDeviceIDByProject(Convert.ToString(PROJECT.CustProjectId));
                string rooms = roomFromDevide(devide);

                if (LANGUAGE == LANGUAGE_THAI)
                {

                    lblTitlePanelDisplayProject2.Content = "Project Code : " + PROJECT.CustomerCode;
                    lblTitlePanelDisplayProject3.Content = STAFFEMER.CustStaffName;
                    lblTitlePanelDisplayProject3_ROOM.Content = PROJECT.CustProjectName;
                    lblTitlePanelDisplayProject4.Content = "";
                    lblTitlePanelDisplayProject5.Content = "ห้อง " + rooms + "";
                    lblTitlePanelDisplayProject6.Content = "ระยะเวลาเข้าใช้งาน";
                    lblTitlePanelDisplayProject7.Content = PROJECT.CustProjectStart.ToString("dd MMM yyyy HH:mm") + " ถึง " + PROJECT.CustProjectEnd.ToString("dd MMM yyyy HH:mm");

                    /*lblTitlePanelDisplayProject2.Content = "Project Code : " + PROJECT.CustomerCode;
                    lblTitlePanelDisplayProject3.Content = PROJECT.CustProjectName;
                    lblTitlePanelDisplayProject3_ROOM.Content = "(" + rooms + ")";
                    lblTitlePanelDisplayProject4.Content = "ระยะเวลาเข้าใช้งาน";
                    lblTitlePanelDisplayProject5.Content = PROJECT.CustProjectStart.ToString("dd MMM yyyy HH:mm");
                    lblTitlePanelDisplayProject6.Content = "ถึง";
                    lblTitlePanelDisplayProject7.Content = PROJECT.CustProjectEnd.ToString("dd MMM yyyy HH:mm");*/
                }
                else
                {
                    /*
                    lblTitlePanelDisplayProject2.Content = "Project Code : " + PROJECT.CustomerCode;
                    lblTitlePanelDisplayProject3.Content = PROJECT.CustProjectName;
                    lblTitlePanelDisplayProject3_ROOM.Content = "(" + rooms + ")";
                    lblTitlePanelDisplayProject4.Content = "Working time";
                    lblTitlePanelDisplayProject5.Content = PROJECT.CustProjectStart.ToString("dd MMM yyyy HH:mm");
                    lblTitlePanelDisplayProject6.Content = "To";
                    lblTitlePanelDisplayProject7.Content = PROJECT.CustProjectEnd.ToString("dd MMM yyyy HH:mm");
                    */


                    lblTitlePanelDisplayProject2.Content = "Project Code : " + PROJECT.CustomerCode;
                    lblTitlePanelDisplayProject3.Content = STAFFEMER.CustStaffName;
                    lblTitlePanelDisplayProject3_ROOM.Content = PROJECT.CustProjectName;
                    lblTitlePanelDisplayProject4.Content = "";
                    lblTitlePanelDisplayProject5.Content = "Room " + rooms + "";
                    lblTitlePanelDisplayProject6.Content = "Working time";
                    lblTitlePanelDisplayProject7.Content = PROJECT.CustProjectStart.ToString("dd MMM yyyy HH:mm") + " To " + PROJECT.CustProjectEnd.ToString("dd MMM yyyy HH:mm");
                }

            }


            logger.Info("finish loadProjectData");
        }

        private void ShowPanelProjectInformation()
        {

            logger.Info("start ShowPanelProjectInformation");
            panelEnterIDCardOrPassport.Visibility = Visibility.Hidden;
            CURRENT_PANEL.Visibility = Visibility.Hidden;

            loadProjectData();

            panelDisplayProject.Visibility = Visibility.Visible;
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelDisplayProject;
            logger.Info("finish ShowPanelProjectInformation");
        }

        /**
         * Email = 0
         * SMS = 1
         */
        private void getQRCodeOrOTP(int chanel)
        {
            try
            {
                logger.Info("Start resend qr-code/sms");

                string body = "";

                if (WORKING_TYPE == WORKING_TYPE_WALKIN)
                {
                    string projectId = "";
                    if (PROJECT != null)
                    {
                        projectId = Convert.ToString(PROJECT.CustProjectId);


                        //Refersh staff emer
                        STAFFEMER = getStaffEmerDetail(projectId, STAFFEMER.CustStaffCardId, STAFFEMER.CustStaffCardType);
                    }




                    logger.Info("Calling api for resend qr-code/sms projectid :" + projectId + ", Staff :" + STAFFEMER.CustStaffId + ", chanel :" + chanel);



                    body = @"{
" + "\n" +
                    @"    ""ProjectId"": """ + projectId + @""",
" + "\n" +
                    @"    ""CustStaffId"": """ + STAFFEMER.CustStaffId + @""",
" + "\n" +
                    @"    ""SendChanel"":""" + chanel + @"""
" + "\n" +
                    @"}";

                }
                else
                {

                    string workpermitid = "";
                    if (WORKPERMIT != null)
                    {
                        workpermitid = WORKPERMIT.WorkpermitId;
                    }
                    /*if (ALL_WORKPERMIT != null && ALL_WORKPERMIT.Count > 1)
                    {
                        workpermitid = ALL_WORKPERMIT[0].WorkpermitId;

                        logger.Info("System get workpetmit more than one, so system select first workpermit");
                    }*/

                    logger.Info("Calling api for resend qr-code/sms workpermitid :" + workpermitid + ", Staff :" + STAFF.CustStaffId + ", chanel :" + chanel);



                    body = @"{
" + "\n" +
                    @"    ""WorkpermitId"": """ + workpermitid + @""",
" + "\n" +
                    @"    ""CustStaffId"": """ + STAFF.CustStaffId + @""",
" + "\n" +
                    @"    ""SendChanel"":""" + chanel + @"""
" + "\n" +
                    @"}";


                }


                if (IS_DEBUG)
                    logger.Info(body);



                var client = new RestClient(this.SERVER_API_URL + "/api/kiosk/SendCodeOTP");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request); //
                Console.WriteLine(response.Content);

                logger.Info("Call api for resend qr-code/sms result :"+ response.Content);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private string hidePhoneNumber(string mobileNumber)
        {
            try
            {
                return mobileNumber.Substring(0, 5) + "***" + mobileNumber.Substring(8, 2);
            }
            catch
            {
                return mobileNumber;
            }
        }


        private string hideIdCard(string Idcard)
        {
            try
            {
                if (Idcard.Length > 7)
                {
                    return Idcard.Substring(0, 5) + "***" + Idcard.Substring(8);
                }
                else
                {
                    return Idcard;
                }
            }
            catch
            {
                return Idcard;
            }
        }

        private string hideEmail(string email)
        {
            try
            {
                string[] a = email.Split('@');

                return a[0].Substring(0, a[0].Length-2) + "**@**" + a[1].Substring(2, a[1].Length - 2);
            }
            catch
            {
                return email;
            }
        }

        private void ShowPanelEntryOTP()
        {

            logger.Info("ShowPanelEntryOTP");

            if (resendOtpTimer != null)
                resendOtpTimer.Start();

            if (RESEND_OTP == 3)
            {

                ActionLogModel model = newActionLogModel();
                model.Event = "Resend OTP";
                model.SubEvent = "Resend OTP more than 3 times.";   
                if(STAFF!=null)
                    model.Description = "Staff " + STAFF.CustStaffName + " try to resend OTP more than 3 times.";
                else if(STAFFEMER!=null)
                    model.Description = "Staff " + STAFFEMER.CustStaffName + " try to resend OTP more than 3 times.";
                var task = Task.Run(async () => await addActionLog(model));



                if (LANGUAGE == LANGUAGE_THAI)
                {
                    alertWindow.setMessage("พบข้อผิดพลาด", "ระบบไม่สามารถส่ง OTP ติดต่อกันเกิน 3 ครั้งได้", "กรุณาติดต่อผู้ดูแลระบบเพื่อตรวจสอบความถูกต้อง");
                }
                else
                {

                    alertWindow.setMessage("Error", "Can not send otp more than 3 times", "Please contact administrator");
                }

                alertWindow.ShowDialog();

                ShowEntryPanel();
            }
            else
            {
                getQRCodeOrOTP(1);

                if(RESEND_OTP > 0)
                {
                    if (LANGUAGE == LANGUAGE_THAI)
                    {
                        lblOTPpanelScanQR.Content = "หรือกรอกรหัส OTP ที่ได้รับ(แตะที่นี่)";
                        lblOTPpanelScanQR_panelEntryQR.Content = "หรือกรอกรหัส OTP ที่ได้รับ(แตะที่นี่)";
                        lblOTPpanelEntryOTP.Content = "หรือกรอกรหัส OTP ที่ได้รับ(แตะที่นี่)";
                    }
                    else
                    {

                        lblOTPpanelScanQR.Content = "Or enter your OTP (Touch here)";
                        lblOTPpanelScanQR_panelEntryQR.Content = "Or enter your OTP (Touch here)";
                        lblOTPpanelEntryOTP.Content = "Or enter your OTP (Touch here)";  
                    }
                }

                RESEND_OTP++;

                if (LANGUAGE == LANGUAGE_THAI)
                {
                    lblTitlepanelEntryOTP2.Content = "ระบบจัดส่ง QR-Code ไปยังหมายเลข ";
                }
                else
                {
                    lblTitlepanelEntryOTP2.Content = "System already sent QR-Code to ";
                }


                if (STAFF != null)
                    lblTitlepanelEntryOTP2.Content = lblTitlepanelEntryOTP2.Content + hidePhoneNumber(STAFF.CustStaffMobile);
                else if (STAFFEMER != null)
                    lblTitlepanelEntryOTP2.Content = lblTitlepanelEntryOTP2.Content + hidePhoneNumber(STAFFEMER.CustStaffMobile);



                if (LANGUAGE == LANGUAGE_THAI)
                    lblTitlepanelEntryOTP4.Content = "ได้ในอีก " + MASTER_RESEND_OTP_COUNT + " วินาที (ครั้งที่ " + RESEND_OTP + "/3)";
                else
                    lblTitlepanelEntryOTP4.Content = "in " + MASTER_RESEND_OTP_COUNT + " second (" + RESEND_OTP + "/3)";



                logger.Info("ShowPanelEntryOTP set lable before display panel complete");
            }


            CURRENT_PANEL.Visibility = Visibility.Hidden;



            logger.Info("ShowPanelEntryOTP set lable before display panel complete (hided current panel)");

            panelEntryOTP.Visibility = Visibility.Visible;



            logger.Info("ShowPanelEntryOTP set lable before display panel complete (display EntryOTP Panel)");
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelEntryOTP;
        }


        private void ShowPanelEntryQR()
        {

            logger.Info("ShowPanelEntryQR");

            if (resendQrTimer != null)
                resendQrTimer.Start();

            if (RESEND_QR == 3)
            {
                ActionLogModel model = newActionLogModel();
                model.Event = "Resend QR";
                model.SubEvent = "Resend QR-Code more than 3 times.";
                if (STAFF != null)
                {
                    model.Description = "Staff " + STAFF.CustStaffName + " try to resend qr code more than 3 times.";
                }else if (STAFFEMER != null)
                {
                    model.Description = "Staff " + STAFFEMER.CustStaffName + " try to resend qr code more than 3 times.";
                }
                var task = Task.Run(async () => await addActionLog(model));

                //alertWindow.setMessage("พบข้อผิดพลาด", "ระบบไม่สามารถส่ง Email ติดต่อกันเกิน 3 ครั้งได้", "กรุณาติดต่อผู้ดูแลระบบเพื่อตรวจสอบความถูกต้อง");
                if (LANGUAGE == LANGUAGE_THAI)
                {
                    alertWindow.setMessage("พบข้อผิดพลาด", "ระบบไม่สามารถส่ง Email ติดต่อกันเกิน 3 ครั้งได้", "กรุณาติดต่อผู้ดูแลระบบเพื่อตรวจสอบความถูกต้อง");
                }
                else
                {

                    alertWindow.setMessage("Error", "Can not send email more than 3 times", "Please contact administrator");
                }

                alertWindow.ShowDialog();

                ShowEntryPanel();
            }
            else
            {

                getQRCodeOrOTP(0);

                RESEND_QR++;


                if (LANGUAGE == LANGUAGE_THAI)
                {
                    lblTitlepanelEntryQR2.Content = "ระบบจัดส่ง QR-Code และรหัส OTP ไปยังอีเมล์ ";
                }
                else
                {
                    lblTitlepanelEntryQR2.Content = "System already sent QR-Code and OTP to email : ";
                }

                if (STAFF != null)
                {
                    lblTitlepanelEntryQR2.Content = lblTitlepanelEntryQR2.Content + hideEmail(STAFF.CustStaffEmail);
                }
                else if (STAFFEMER != null)
                {
                    lblTitlepanelEntryQR2.Content = lblTitlepanelEntryQR2.Content + hideEmail(STAFFEMER.CustStaffEmail);
                }

                

                if (LANGUAGE == LANGUAGE_THAI)
                    lblTitlepanelEntryQR4.Content = "ได้ในอีก " + MASTER_RESEND_QR_COUNT + " วินาที (ครั้งที่ " + RESEND_QR + "/3)";
                else
                    lblTitlepanelEntryQR4.Content = "in " + MASTER_RESEND_QR_COUNT + " second (" + RESEND_QR + "/3)";
            }


            CURRENT_PANEL.Visibility = Visibility.Hidden;

            panelEntryQR.Visibility = Visibility.Visible;
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelEntryQR;

            readQRCodeEmer.Text = "";
            readQRCodeEmer.Focus();
        }


        private void ShowCustomerIDCardEntryScreenPanel()
        {
            panelEntry.Visibility = Visibility.Hidden;
            panelEnterIDCardOrPassport.Visibility = Visibility.Visible;
            //panelFullKeyboardPopup.Visibility = Visibility.Visible;
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelCustomerEnterIDcard;

            if (WORKING_TYPE == WORKING_TYPE_CUSTOMER)
                cmbObjective.Visibility = Visibility.Hidden;
            else
                cmbObjective.Visibility = Visibility.Visible;


            //showkeyboard
            panelFullKeyboardPopup.Visibility = Visibility.Visible;
            panelFullKeyboardPopup.HorizontalAlignment = HorizontalAlignment.Center;
            
            fullKeyboardPopup.PlacementTarget = lblIDCardNumber;
            fullKeyboardPopup.HorizontalAlignment = HorizontalAlignment.Center;
            fullKeyboardPopup.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            fullKeyboardPopup.VerticalOffset = KeyboardVerOffset;
            fullKeyboardPopup.HorizontalOffset = KeyboardHorOffset;
            fullKeyboardPopup.IsOpen = true;
            keyboardJustOpen = true;
        }

        #region window event


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                var hwndTarget = hwndSource.CompositionTarget;
                hwndTarget.RenderMode = RenderMode.SoftwareOnly;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }

            timer.Start();
            amAliveTimer.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                ActionLogModel model = newActionLogModel();
                model.Event = "Kiosk is close";
                model.SubEvent = "Application closed";
                model.Description = "Kiosk " + KioskCode + " is closed.";
                var task = Task.Run(async () => await addActionLog(model));

                if (HandComm != 0)
                {
                    DLLCLASS.CommClose(HandComm);
                    HandComm = 0;
                    logger.Info("Comm. Port is Closed");
                }

                //logger.InfoToDblog(Utility.CODE_999, Utility.LOG_TYPE_APPLICATION_GENERAL, Utility.FINISH_APPLICATION, terminalCode);

                if (threadCallReturnCard != null)
                {
                    threadCallReturnCard.Abort();
                    threadCallReturnCard = null;
                }

                timer.Stop();
                homeVideo.Close();
            }
            catch { }
        }

        #endregion

        #region Timer for every panel
        private void InitializeDispatchTimer()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;

            resendOtpTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            resendOtpTimer.Tick += ResendOtpTimer_Tick;


            resendQrTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            resendQrTimer.Tick += ResendQrTimer_Tick;

            timerCallReturnCardDll = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timerCallReturnCardDll.Tick += timerCallReturnCardDll_Tick;

            amAliveTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMinutes(5)
            };
            amAliveTimer.Tick += AmAliveTimer_Tick;
        }

        private void AmAliveTimer_Tick(object sender, EventArgs e)
        {
            ActionLogModel model = newActionLogModel();
            model.Event = "System";
            model.SubEvent = "Kiosk alive";
            model.Description = "Kiosk " + KioskCode + " is alive on IP :" + NodeIP + " Location ID :" + LocationCode 
                              + " Building ID :" + BuildingCode + " Floor :"+FloorCode;
            //var task = Task.Run(async () => await addActionLog(model));
        }

        private void timerCallReturnCardDll_Tick(object sender, EventArgs e)
        {
            try
            {
                ++timeWaitForCallDll;
                if(timeWaitForCallDll == 2)
                {

                    timerCallReturnCardDll.Stop();

                    if (threadCallReturnCard != null)
                    {
                        logger.Info("threadCallReturnCard.ThreadState :"+ threadCallReturnCard.ThreadState);
                        if (threadCallReturnCard.ThreadState == ThreadState.Running || threadCallReturnCard.ThreadState == ThreadState.Background)
                        {
                            threadCallReturnCard.Abort();
                        }
                    }

                    ThreadStart ts = new ThreadStart(connectReturnCardDll);
                    threadCallReturnCard = new Thread(ts);
                    threadCallReturnCard.Start();

                    //connectReturnCardDll();
                    
                }
            }
            catch { }
        }

        private void ResendQrTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                resendQRCount--;

                if (resendQRCount > 0)
                {
                    if (LANGUAGE == LANGUAGE_THAI)
                        lblRequestNewQR_panelEntryQR.Content = "ขอรับโค้ดใหม่(" + resendQRCount + "s)";
                    else
                        lblRequestNewQR_panelEntryQR.Content = "Request new QR-Code(" + resendQRCount + "s)";
                }

                if (resendQRCount == 0)
                {
                    if (LANGUAGE == LANGUAGE_THAI)
                        lblRequestNewQR_panelEntryQR.Content = "ขอรับโค้ดใหม่";
                    else
                        lblRequestNewQR_panelEntryQR.Content = "Request new QR-Code";

                    lblRequestNewQR_panelEntryQR.IsEnabled = true;
                    lblRequestNewQR_panelEntryQR.Foreground = new SolidColorBrush(Colors.Red);

                    resendQrTimer.Stop();
                }
                else
                {
                    lblRequestNewQR_panelEntryQR.IsEnabled = false;
                    lblRequestNewQR_panelEntryQR.Foreground = new SolidColorBrush(Colors.Gray);
                }
            }
            catch { }
        }

        private void ResendOtpTimer_Tick(object sender, EventArgs e)
        {
            //string timeElapsedInstring = DateTime.Now.ToString("HH:MM:ss");
            try
            {
                resendOTPCount--;

                if (resendOTPCount > 0)
                {
                    if (LANGUAGE == LANGUAGE_THAI)
                        lblRequestNewOTP_panelEntryOTP.Content = "ขอรับโค้ดใหม่(" + resendOTPCount + "s)";
                    else
                        lblRequestNewOTP_panelEntryOTP.Content = "Request new QR-Code(" + resendOTPCount + "s)";
                }

                if (resendOTPCount == 0)
                {
                    if (LANGUAGE == LANGUAGE_THAI)
                        lblRequestNewOTP_panelEntryOTP.Content = "ขอรับโค้ดใหม่";
                    else
                        lblRequestNewOTP_panelEntryOTP.Content = "Request new QR-Code";

                    lblRequestNewOTP_panelEntryOTP.IsEnabled = true;
                    lblRequestNewOTP_panelEntryOTP.Foreground = new SolidColorBrush(Colors.Red);

                    resendOtpTimer.Stop();
                }
                else
                {
                    lblRequestNewOTP_panelEntryOTP.IsEnabled = false;
                    lblRequestNewOTP_panelEntryOTP.Foreground = new SolidColorBrush(Colors.Gray);
                }
            }
            catch { }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            string timeElapsedInstring = DateTime.Now.ToString("dd MMM yyyy HH:mm");
            try
            {
                //lblTime.Content = timeElapsedInstring;
                updateTime(timeElapsedInstring);

                if ((CURRENT_PANEL != null || !panelScreenSaver.Equals(CURRENT_PANEL)))
                {
                    AUTO_CANCEL_COUNT--;
                    if (AUTO_CANCEL_COUNT == 0)
                    {
                        clearControl();
                        if (CURRENT_PANEL != null && !panelScreenSaver.Equals(CURRENT_PANEL))
                        {
                            CURRENT_PANEL.Visibility = Visibility.Hidden;
                            panelHeader.Visibility = Visibility.Hidden;
                            panelFotter.Visibility = Visibility.Hidden;
                        }

                        if (CURRENT_PANEL != null && !panelEntry.Equals(CURRENT_PANEL))
                        {
                            ShowEntryPanel();
                        }
                        else {
                            panelScreenSaver.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        updateRemainTime(AUTO_CANCEL_COUNT);

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private void clearControl()
        {
            try { 
            //LANGUAGE = LANGUAGE_THAI;
            WORKING_TYPE = -1;
            CARD = "";
            IDCARD_NOT_CORRECT_COUNT = 0;
                WALKIN_OBJ = -1;
                WALKIN_OBJ_TXT = "";

            if (LANGUAGE == LANGUAGE_THAI)
            {
                lblOTPpanelScanQR.Content = "หรือกรอกรหัส OTP ที่ได้รับ(แตะที่นี่)";
                lblOTPpanelScanQR_panelEntryQR.Content = "หรือกรอกรหัส OTP ที่ได้รับ(แตะที่นี่)";
                lblIDCardNumber.Content = "เสียบบัตรหรือกดตัวเลข(แตะที่นี่)";
                lblOTPpanelEntryOTP.Content = "หรือกรอกรหัส OTP ที่ได้รับ(แตะที่นี่)";
            }
            else
            {
                lblOTPpanelScanQR.Content = "Or enter your OTP (Touch here)";
                lblOTPpanelScanQR_panelEntryQR.Content = "Or enter your OTP (Touch here)";
                lblOTPpanelEntryOTP.Content = "Or enter your OTP (Touch here)";
                lblIDCardNumber.Content = "Insert card or entry number"; 
            }

            lblRequestNewOTP_panelEntryOTP.IsEnabled = false;
            lblRequestNewOTP_panelEntryOTP.Foreground = new SolidColorBrush(Colors.Gray);

            lblRequestNewQR_panelEntryQR.IsEnabled = false;
            lblRequestNewQR_panelEntryQR.Foreground = new SolidColorBrush(Colors.Gray);

            lblTitlePanelDisplayProject2.Content = "";
            lblTitlePanelDisplayProject3.Content = "";
            lblTitlePanelDisplayProject3_ROOM.Content = "";
            lblTitlePanelDisplayProject4.Content = "";
            lblTitlePanelDisplayProject5.Content = "";
            lblTitlePanelDisplayProject6.Content = "";
            lblTitlePanelDisplayProject7.Content = "";

            

            RESEND_OTP = 0;
            resendOTPCount = MASTER_RESEND_OTP_COUNT;

            RESEND_QR = 0;
            resendQRCount = MASTER_RESEND_QR_COUNT;

            this.cmbObjective.SelectedIndex = 0;

            WORKPERMIT = null;
            ALL_WORKPERMIT = null;
            STAFF = null;
                PROJECT = null;
                ALL_PROJECT = null;
                STAFFEMER = null;

            readQRCode.Text = "";
            readQRCodeEmer.Text = "";

            projectLstBox.Items.Clear();
            workpermitLstBox.Items.Clear();

            btnReturnCard.IsEnabled = true;


            this.idCardOrPassportNumber = "";
            this.cardType = "";

            if (alertWindow != null)
            {
                alertWindow.Hide();
            }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            try
            {
                if (fullKeyboardPopup != null)
                {
                    fullKeyboardPopup.IsOpen = false;
                    panelFullKeyboardPopup.Visibility = Visibility.Hidden;
                }

                if (popupNumPad!=null)
                {
                    targetTextbox = null;
                    popupNumPad.IsOpen = false;
                }

                if (threadCallReturnCard != null)
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                    threadCallReturnCard.Abort();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            timeWaitForCallDll = 0;
        }

        private void updateRemainTime(int remainValue)
        {
            //if (CURRENT_PANEL != null && !panelHome.Equals(CURRENT_PANEL))
            //{
            lblHeaderTimeout.Content = remainValue;
            //}

        }

        private void updateTime(string label)
        {
            lblTime.FontSize = 16;

            /* try
             {
                 int i = Convert.ToInt32(label);

                 int min = i / 60;
                 int sec = i / 60;

                 lblTime.Content = min + ":"+sec;
             }
             catch {

                 lblTime.Content = label;
             }*/

                    lblTime.Content = label;
        }

        #endregion



        #region Panel Event 

        private void panelEntry_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CURRENT_PANEL = (Panel)sender;
        }

        private void panelScreenSaver_TouchDown(object sender, TouchEventArgs e)
        {
            ShowEntryPanel();
        }

        private void panelScreenSaver_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Panel p = (Panel)sender;
            if (p.IsVisible)
            {
                /*
                if (homeVideo.Visibility == Visibility.Visible)
                {
                    String fileUrl = Utility.getHomeVDOFile();
                    logger.Info("home video :" + fileUrl);
                    if (fileUrl.IndexOf(".mp4") > 0)
                    {

                        //MediaTimeline timeline = new MediaTimeline(new Uri(fileUrl));
                        //timeline.RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever;
                        //MediaClock clock = timeline.CreateClock();
                        //homeVideo.Clock = clock;
                    }
                }*/
                //logger.Info("CurrentTime :" + homeVideo.Clock.CurrentTime);
                /*logger.Info("CurrentState :" + homeVideo.Clock.CurrentState);
                logger.Info("IsSealed :" + homeVideo.Clock.Timeline.IsSealed);
                logger.Info("IsFrozen :" + homeVideo.Clock.Timeline.IsFrozen);
                logger.Info("homeVideo.Clock.IsPaused :" + homeVideo.Clock.IsPaused);*/

                System.GC.Collect();
                
                homeVideo.Play();
                timer.Stop();
                CURRENT_PANEL = p;
            }
            else
            {
                timer.Start();
                homeVideo.Pause();
            }

        }


        private void panelTC_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            Panel p = (Panel)sender;
            if (p.IsVisible)
            {
                CURRENT_PANEL = p;
            }
        }

        private void panelScreenSaver_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowEntryPanel();
        }



        private void panelEntryQR_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Panel p = (Panel)sender;
            if (p.IsVisible)
            {
                CURRENT_PANEL = p;

                readQRCodeEmer.Text = "";
                readQRCodeEmer.Focus();
            }
            else
            {
                targetTextbox = null;
                popupNumPad.IsOpen = false;
            }
        }


        private void panelEntryOTP_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            logger.Info("panelEntryOTP_IsVisibleChanged");

            Panel p = (Panel)sender;
            if (p.IsVisible)
            {
                CURRENT_PANEL = p;
            }
            else
            {
                targetTextbox = null;
                popupNumPad.IsOpen = false;
            }
        }

        private void panelEnterIDCardOrPassport_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Panel p = (Panel)sender;
            if (p.IsVisible)
            {
                CURRENT_PANEL = p;
                /*
                if(WORKING_TYPE == WORKING_TYPE_WALKIN)
                {

                    lblIDCardNumber.Margin = new Thickness(0, 30, 0, 0);
                    //btnHome1.Margin = new Thickness(-950, 379, 0, 0);
                    //btnNext_panelEnterIDCardOrPassport.Margin = new Thickness(950, -43, 0, 0);
                }
                else
                {
                    lblIDCardNumber.Margin = new Thickness(0, -60, 0, 0);
                    btnHome1.Margin = new Thickness(- 950,439,0,0); 
                    btnNext_panelEnterIDCardOrPassport.Margin = new Thickness(950, -13, 0, 0);
                }*/
            }
            else
            {
                if (fullKeyboardPopup != null)
                {
                    fullKeyboardPopup.IsOpen = false;
                    panelFullKeyboardPopup.Visibility = Visibility.Hidden;
                }
            }
        }


        private void panelResendQR_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            logger.Info("panelResendQR_IsVisibleChanged");

            Panel p = (Panel)sender;
            if (p.IsVisible)
            {
                CURRENT_PANEL = p;

                if (LANGUAGE == LANGUAGE_THAI)
                {
                    lblOTPpanelScanQR.Content = "หรือกรอกรหัส OTP ที่ได้รับ(แตะที่นี่)";
                    lblOTPpanelScanQR_panelEntryQR.Content = "หรือกรอกรหัส OTP ที่ได้รับ(แตะที่นี่)";
                }
                else
                {
                    lblOTPpanelScanQR.Content = "Or enter your OTP (Touch here)";
                    lblOTPpanelScanQR_panelEntryQR.Content = "Or enter your OTP (Touch here)";
                }


                resendOTPCount = MASTER_RESEND_OTP_COUNT;
                resendQRCount = MASTER_RESEND_QR_COUNT;
            }
        }

        #endregion


        private void keypad_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)e.OriginalSource;
            string s = btn.Content.ToString();

            

            if (s == "X")
            {
                popupNumPad.IsOpen = false;
                targetTextbox = null;
            }
            else if (s == "Del")
            {
                if (!"หรือกรอกรหัส OTP ที่ได้รับ(แตะที่นี่)".Equals(targetTextbox.Content) && !"Or enter your OTP (Touch here)".Equals(targetTextbox.Content))
                {
                    String content = targetTextbox.Content.ToString();
                    if (content.Length > 0)
                        content = content.Substring(0, content.Length - 1);

                    targetTextbox.Content = content;
                }
            }
            else
            {

                if ("หรือกรอกรหัส OTP ที่ได้รับ(แตะที่นี่)".Equals(targetTextbox.Content) || "Or enter your OTP (Touch here)".Equals(targetTextbox.Content))
                {
                    targetTextbox.Content = "";
                }
                else if ("รหัส OTP ที่ได้รับ".Equals(targetTextbox.Content) || "Entry OTP".Equals(targetTextbox.Content))
                {
                    targetTextbox.Content = "";

                }

                targetTextbox.Content += s;
            }
        }


        private void key_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)e.OriginalSource;
            string s = btn.Content.ToString();

            if ("เสียบบัตรหรือกดตัวเลข(แตะที่นี่)".Equals(lblIDCardNumber.Content) || "Insert card or entry number".Equals(lblIDCardNumber.Content))
            {
                lblIDCardNumber.Content = "";
            }

            // if (s == "X")
            // {
            //    fullKeyboardPopup.IsOpen = false;
            //    lblIDCardNumber.Content = "";
            //}
            //else

            if (s == "Del")
            {
                String content = lblIDCardNumber.Content.ToString();


                if (content.Length > 0)
                    content = content.Substring(0, content.Length - 1);

                lblIDCardNumber.Content = content;
            }
            else
            {


                lblIDCardNumber.Content += s;
            }
        }

        private void btnCustomer_TouchDown(object sender, TouchEventArgs e)
        {
            WORKING_TYPE = WORKING_TYPE_CUSTOMER;
            ShowCustomerIDCardEntryScreenPanel();
        }


        private void btnFlagEng_TouchDown(object sender, TouchEventArgs e)
        {
            LANGUAGE = LANGUAGE_ENG;
            InitialLabel();

            btnFlagThai.Opacity = 0.5;
            btnFlagEng.Opacity = 1;

        }

        private void btnFlagThai_TouchDown(object sender, TouchEventArgs e)
        {
            LANGUAGE = LANGUAGE_THAI;
            InitialLabel();

            btnFlagEng.Opacity = 0.5;
            btnFlagThai.Opacity = 1;
        }

        private void btnHome1_TouchDown(object sender, TouchEventArgs e)
        {
            CURRENT_PANEL.Visibility = Visibility.Hidden;
            ShowEntryPanel();
        }

        private void btnNext_panelEnterIDCardOrPassport_TouchDown(object sender, TouchEventArgs e)
        {

            WALKIN_OBJ = -1;
            WALKIN_OBJ_TXT = "";

            if (WORKING_TYPE == this.WORKING_TYPE_WALKIN)
            {
                if(cmbObjective.SelectedIndex < 1 )
                {
                    if (LANGUAGE == LANGUAGE_THAI)
                    {
                        alertWindow.setMessage("พบข้อผิดพลาด", "กรุณาเลือกเหตุผลการเข้าใช้งาน", "");
                    }
                    else
                    {
                        alertWindow.setMessage("Warning", "Please select objective of entry", "");
                    }
                    alertWindow.ShowDialog();

                    return;
                }

                //yy
                WALKIN_OBJ = Convert.ToInt32(cmbObjective.SelectedIndex);
                WALKIN_OBJ_TXT = cmbObjective.SelectedItem.ToString();
            }



            WORKPERMIT = null;
            ALL_WORKPERMIT = null;
            STAFF = null;
            PROJECT = null;
            STAFFEMER = null;
            ALL_PROJECT = null;

            idCardOrPassportNumber = lblIDCardNumber.Content.ToString();
            cardType = "";

            if (Regex.IsMatch(idCardOrPassportNumber, @"^[0-9]+$") && lblIDCardNumber.Content.ToString().Length == 13)
            {
                cardType = "ID";
            } else if (Regex.IsMatch(idCardOrPassportNumber, @"^[a-zA-Z0-9]+$"))
            {
                cardType = "PASSPORT";
            } else if (lblIDCardNumber.Content.ToString().Length != 13)
            {
                cardType = "PASSPORT";
            }
            logger.Info("Start checking id-card/passport :"+ hideIdCard(idCardOrPassportNumber) + " cardType :"+ cardType);


            if (WORKING_TYPE == this.WORKING_TYPE_WALKIN)
            {
                //Check id card 
                List<CustProjectVw> allProjectVw = checkWalkinIdCardOrPassport(idCardOrPassportNumber, cardType, DateTime.Now);
                if (allProjectVw == null || allProjectVw.Count == 0)
                {
                    logger.Info("ไม่พบข้อมูล id-card/passport ในช่วงเวลาที่ระบุ :" + hideIdCard(idCardOrPassportNumber));
                    ++IDCARD_NOT_CORRECT_COUNT;

                    if (IDCARD_NOT_CORRECT_COUNT < 5)
                    {
                        if (LANGUAGE == LANGUAGE_THAI)
                        {
                            alertWindow.setMessage("การยืนยันตัวตันครั้งที่ " + IDCARD_NOT_CORRECT_COUNT + "/5", "ข้อมูลไม่ถูกต้องกรุณาระบุข้อมูลใหม่", "");

                        }
                        else
                        {
                            alertWindow.setMessage("Data not match with work permit information (" + IDCARD_NOT_CORRECT_COUNT + "/5)", "Incorrect Data", "");

                        }
                        alertWindow.ShowDialog();
                    }
                    else
                    {
                        if (LANGUAGE == LANGUAGE_THAI)
                        {
                            alertWindow.setMessage("Error", "เนื่องจากท่านยืนยันตัวตนผิดเป็นจำนวน 5 ครั้ง", "ระบบไม่สามารถออกบัตรให้ท่านได้กรุณาติดต่อเจ้าหน้าที่");
                        }
                        else
                        {
                            alertWindow.setMessage("Error", "Your data is not correct 5 times", "Please contact administrator");
                        }
                            
                            alertWindow.ShowDialog();

                        ShowEntryPanel();
                    }



                }
                else if (allProjectVw.Count == 1)
                {
                    IDCARD_NOT_CORRECT_COUNT = 0;

                    PROJECT = allProjectVw[0];
                    STAFFEMER = getStaffEmerDetail(Convert.ToString(PROJECT.CustProjectId), idCardOrPassportNumber, cardType);


                    logger.Info("พบข้อมูล id-card/passport :" + hideIdCard(idCardOrPassportNumber) + " พบ project 1 รายการคือ :" + PROJECT.CustProjectName +  " STAFF-EMER :" + STAFFEMER.CustStaffName);

                    if (Convert.ToDecimal(LAST_TC_TH.Version) > (STAFFEMER.TcVersion) || STAFFEMER.TcVersion == null)
                    {
                        ShowTCPanel();
                    }
                    else
                    {
                        if (WORKING_TYPE == WORKING_TYPE_CUSTOMER)
                        {
                            //aa
                            logger.Info("ShowScanQRPanel (allProjectVw Count = 1)");
                            ShowScanQRPanel();
                        }
                        else
                        {
                            //bb


                            logger.Info("ShowPanelResendQR ((allProjectVw Count = 1))");
                            ShowPanelResendQR();
                        }
                    }
                }
                else if (allProjectVw.Count > 1)
                {
                    IDCARD_NOT_CORRECT_COUNT = 0;
                    ALL_PROJECT = allProjectVw;

                    STAFFEMER = getStaffEmerDetail(Convert.ToString(ALL_PROJECT[0].CustProjectId), idCardOrPassportNumber, cardType);

                    logger.Info("พบข้อมูล id-card/passport(allProjectVw Count > 1) :" + hideIdCard(idCardOrPassportNumber) + "พบ project จำนวน " + ALL_PROJECT.Count + " รายการ id รายการแรกคือ :" + ALL_PROJECT[0].CustProjectId + " " + ALL_PROJECT[0].CustProjectName + " STAFF-EMER :" + STAFFEMER.CustStaffName);

                    if (WORKING_TYPE_WALKIN == WORKING_TYPE)
                    {
                        //cc
                        logger.Info("showPanelSelectProject ((allProjectVw Count > 1))");
                        showPanelSelectProject();
                    }
                    else
                    {
                        if (Convert.ToDecimal(LAST_TC_TH.Version) > (STAFFEMER.TcVersion) || STAFFEMER.TcVersion == null)
                        {
                            ShowTCPanel();
                        }
                        else
                        {
                            if (WORKING_TYPE == WORKING_TYPE_CUSTOMER)
                            {

                                logger.Info("ShowScanQRPanel ((allProjectVw Count > 1))");
                                ShowScanQRPanel();
                            }
                            else
                            {

                                logger.Info("ShowPanelResendQR ((allProjectVw Count > 1))");

                                ShowPanelResendQR();
                            }
                        }
                    }
                }
            }
            else
            {
                //Check id card 
                List<WorkpermitVw> allWorkpromise = checkStaffIdCardOrPassport(idCardOrPassportNumber, cardType, DateTime.Now);

                if (allWorkpromise == null || allWorkpromise.Count == 0)
                {
                    logger.Info("ไม่พบข้อมูล id-card/passport ในช่วงเวลาที่ระบุ :" + hideIdCard(idCardOrPassportNumber));

                    ++IDCARD_NOT_CORRECT_COUNT;

                    if (IDCARD_NOT_CORRECT_COUNT < 5)
                    {
                        if (LANGUAGE == LANGUAGE_THAI)
                        {
                            alertWindow.setMessage("การยืนยันตัวตันครั้งที่ " + IDCARD_NOT_CORRECT_COUNT + "/5", "ข้อมูลไม่ถูกต้องกรุณาระบุข้อมูลใหม่", "");

                        }
                        else
                        {
                            alertWindow.setMessage("Data not match with work permit information (" + IDCARD_NOT_CORRECT_COUNT + "/5)", "Incorrect Data", "");

                        }

                        //alertWindow.setMessage("การยืนยันตัวตันครั้งที่ " + IDCARD_NOT_CORRECT_COUNT + "/5", "ข้อมูลไม่ถูกต้องกรุณาระบุข้อมูลใหม่", "Data not match with work permit information (" + IDCARD_NOT_CORRECT_COUNT + "/5 times)");

                        alertWindow.ShowDialog();
                    }
                    else
                    {
                        if (LANGUAGE == LANGUAGE_THAI)
                        {
                            alertWindow.setMessage("Error", "เนื่องจากท่านยืนยันตัวตนผิดเป็นจำนวน 5 ครั้ง", "ระบบไม่สามารถออกบัตรให้ท่านได้กรุณาติดต่อเจ้าหน้าที่");
                        }
                        else
                        {
                            alertWindow.setMessage("Error", "Your data is not correct 5 times", "Please contact administrator");
                        }
                        alertWindow.ShowDialog();

                        ShowEntryPanel();
                    }
                }
                else if (allWorkpromise.Count == 1)
                {
                    IDCARD_NOT_CORRECT_COUNT = 0;
                    WORKPERMIT = allWorkpromise[0];
                    STAFF = getStaffDetail(WORKPERMIT.WorkpermitId, idCardOrPassportNumber, cardType);


                    logger.Info("พบข้อมูล id-card/passport :" + hideIdCard(idCardOrPassportNumber) + " พบ workpermit 1 รายการคือ :" + WORKPERMIT.WorkpermitId + " " + WORKPERMIT.CustProjectName + " STAFF :" + STAFF.CustStaffName);

                    if (Convert.ToDecimal(LAST_TC_TH.Version) > (STAFF.TcVersion) || STAFF.TcVersion == null)
                    {
                        ShowTCPanel();
                    }
                    else
                    {
                        if (WORKING_TYPE == WORKING_TYPE_CUSTOMER)
                        {
                            //aa
                            ShowScanQRPanel();
                        }
                        else
                        {
                            //bb
                            ShowPanelResendQR();
                        }
                    }
                }
                else if (allWorkpromise.Count > 1)
                {
                    IDCARD_NOT_CORRECT_COUNT = 0;
                    ALL_WORKPERMIT = allWorkpromise;

                    STAFF = getStaffDetail(ALL_WORKPERMIT[0].WorkpermitId, idCardOrPassportNumber, cardType);

                    logger.Info("พบข้อมูล id-card/passport(Case Workpermit) :" + hideIdCard(idCardOrPassportNumber) + "พบ workpermit จำนวน " + ALL_WORKPERMIT.Count + " รายการ id รายการแรกคือ :" + ALL_WORKPERMIT[0].WorkpermitId + " " + ALL_WORKPERMIT[0].CustProjectName + " STAFF :" + STAFF.CustStaffName);

                    //Select workpermit
                    if (WORKING_TYPE_CUSTOMER == WORKING_TYPE)
                    {
                        showPanelSelectWorkpermit();
                    }
                    /*else
                    {
                        if (Convert.ToDecimal(LAST_TC_TH.Version) > (STAFF.TcVersion) || STAFF.TcVersion == null)
                        {
                            ShowTCPanel();
                        }
                        else
                        {
                            if (WORKING_TYPE == WORKING_TYPE_CUSTOMER)
                            {

                                logger.Info("ShowScanQRPanel (Case Workpermit)");
                                ShowScanQRPanel();
                            }
                            else
                            {
                                logger.Info("ShowPanelResendQR (Case Workpermit)");
                                ShowPanelResendQR();
                            }
                        }
                    }*/
                }

            }
        }

        private void setWalkinObjective()
        {
            var client = new RestClient(SERVER_API_URL + "/api/kiosk/GetWalkinObjective");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            var body = @"{
" + "\n" +
            @"    
" + "\n" +
            @"}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            IEnumerable<MWalkinObjective> m = JsonConvert.DeserializeObject<IEnumerable<MWalkinObjective>>(response.Content);

            cmbObjective.Items.Clear();
            if (LANGUAGE == LANGUAGE_THAI)
            {
                cmbObjective.Items.Insert(0, "กรุณาระบุวัตถุประสงค์การเข้าใช้งาน");
            }
            else
            {//xx
                cmbObjective.Items.Insert(0, "Please select objective");
            }

            if (m != null)
            {
                foreach (MWalkinObjective mWalkinObjective in m)
                {
                    cmbObjective.Items.Insert(mWalkinObjective.WalkinObjectiveId, mWalkinObjective.Description);
                }
            }

            cmbObjective.SelectedIndex = 0;
        }

        /**
         * CustStaffCardType = "ID","PASSPORT"
         * CardNo = เลขของบัตรที่แจก
         */
        private async Task addActionLog(ActionLogModel model)
        {
            try
            {
                logger.Info(model.Event + ", " + model.SubEvent + " : " + model.Description);


                if (WORKPERMIT != null)
                {
                    model.WorkPermitID =Convert.ToInt64( WORKPERMIT.WorkpermitId);
                    model.walkinObjectiveID = -1;
                }

                if( PROJECT!= null && model.walkinObjectiveID>0)
                {
                    model.WorkPermitID = Convert.ToInt64(PROJECT.CustProjectId);
                    model.walkinObjectiveID = WALKIN_OBJ;
                }

                String logTime = convertDatetime(DateTime.Now);// DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var client = new RestClient(SERVER_API_URL + "/api/kiosk/AddActionLog");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                var body = @"{
" + "\n" +
                @"    ""CustStaffCardId"": """ + model.CustStaffCardId + @""",
" + "\n" +
                @"    ""CustStaffCardType"": """ + model.CustStaffCardType + @""",
" + "\n" +
                @"    ""CreatedDate"": """ + logTime + @""",
" + "\n" +
                @"    ""CardNo"" : """ + model.CardNo + @""",
" + "\n" +
                @"    ""WorkPermitID"" : """ + model.WorkPermitID + @""",
" + "\n" +
                @"    ""walkinObjectiveID"" : " + model.walkinObjectiveID + @",
" + "\n" +
                @"    ""VisitorType"" : """ + model.VisitorType + @""",
" + "\n" +
                @"    ""Name"" : """ + model.Name + @""",
" + "\n" +
                @"    ""Email"" : """ + model.Email + @""",
" + "\n" +
                @"    ""MobileNo"" : """ + model.MobileNo + @""",
" + "\n" +
                @"    ""NodeType"" : ""KIOSK"",
" + "\n" +
                @"    ""NodeID"" : """ + KioskCode + @""",
" + "\n" +
                @"    ""NodeIP"" : """ + NodeIP + @""",
" + "\n" +
                @"    ""Type"" : """ + NodeType + @""",
" + "\n" +
                @"    ""Event"" : """ + model.Event + @""",
" + "\n" +
                @"    ""SubEvent"" : """ + model.SubEvent + @""",
" + "\n" +
                @"    ""StaffTypeName"" : """ + model.StaffTypeName + @""",
" + "\n" +
                @"    ""LocationSiteId"" : " + LocationCode + @",
" + "\n" +
                @"    ""LocationBuildingId"" : " + BuildingCode + @",
" + "\n" +
                @"    ""LacationFloorId"":""" + FloorCode + @""",
" + "\n" +
                @"    ""Description"" : """ + model.Description + @"""
" + "\n" +
                @"
" + "\n" +
                @"
" + "\n" +
                @"}";


                if (IS_DEBUG)
                    logger.Info(body);

                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private string convertDatetime(DateTime dt)
        {

            var ci = new CultureInfo("en-US");
            return dt.ToString("yyyy-MM-dd HH:mm:ss",ci);
        }

        private List<WorkpermitVw> checkStaffIdCardOrPassport(string cardNo, string cardType, DateTime dt)
        {
            try
            {

                string requestDate = convertDatetime(dt); //dt.ToString("yyyy-MM-dd HH:mm:ss");

                var client = new RestClient(SERVER_API_URL + "/api/kiosk/GetWorkpermitsByIDCard");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");

                var body = @"{
" + "\n" +
@"    ""CustStaffCardId"": """ + cardNo + @""",
" + "\n" +
@"    ""CustStaffCardType"": """ + cardType + @""",
" + "\n" +
@"    ""RequestDate"": """ + requestDate + @""",
" + "\n" +
@"    ""LocationFloorId"": """ + FloorCode + @"""
" + "\n" +
@"}";
                if(IS_DEBUG)
                logger.Info(body);

                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);

                IEnumerable<WorkpermitVw> m = JsonConvert.DeserializeObject<IEnumerable<WorkpermitVw>>(response.Content);
                if (m != null)
                {
                    return m.ToList<WorkpermitVw>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return null;
            }
        }


        private List<CustProjectVw> checkWalkinIdCardOrPassport(string cardNo, string cardType, DateTime dt)
        {
            try
            {

                string requestDate = convertDatetime(dt); //dt.ToString("yyyy-MM-dd HH:mm:ss");

                var client = new RestClient(SERVER_API_URL + "/api/kiosk/GetProjectsByIDCard");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");

                var body = @"{
" + "\n" +
@"    ""CustStaffCardId"": """ + cardNo + @""",
" + "\n" +
@"    ""CustStaffCardType"": """ + cardType + @""",
" + "\n" +
@"    ""RequestDate"": """ + requestDate + @""",
" + "\n" +
@"    ""LocationFloorId"": """ + FloorCode + @"""
" + "\n" +
@"}";

                if (IS_DEBUG)
                    logger.Info(body);

                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);

                IEnumerable<CustProjectVw> m = JsonConvert.DeserializeObject<IEnumerable<CustProjectVw>>(response.Content);
                if (m != null)
                {
                    return m.ToList<CustProjectVw>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return null;
            }
        }

        private void btnNext_panelTC_TouchDown(object sender, TouchEventArgs e)
        {
            string email = "";
            if(WORKING_TYPE == WORKING_TYPE_WALKIN)
            {
                email = STAFFEMER.CustStaffEmail;
            }
            else
            {
                email = STAFF.CustStaffEmail;
            }

            addTcPrivacyAccept(1, 0, LAST_TC_TH.Version, email);


            if (WORKING_TYPE == WORKING_TYPE_CUSTOMER)
            {
                ShowScanQRPanel();
            }
            else
            {
                ShowPanelResendQR();
            }
        }


        private void panelDisplayError_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Panel p = (Panel)sender;
            if (p.IsVisible)
            {
                CURRENT_PANEL = p;
            }
        }


        private void panelReturnCardSuccess_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Panel p = (Panel)sender;
            if (p.IsVisible)
            {
                CURRENT_PANEL = p;
            }
        }


        private void PanelDisplayProject_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Panel p = (Panel)sender;
            if (p.IsVisible)
            {
                CURRENT_PANEL = p;
            }
        }


        private void panelPrivacyMarketing_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Panel p = (Panel)sender;
            if (p.IsVisible)
            {
                CURRENT_PANEL = p;
            }

        }

        Thread threadCallReturnCard = null;

        private void connectReturnCardDll()
        {           

                try
                {
                    logger.Info("Start call dll for return card");
                   
                    Thread.CurrentThread.IsBackground = true;
                    CARD = readCardForReturnAsync().Result;                

                    ActionLogModel model = newActionLogModel();

                    if (CARD != "")
                    {
                        model.Description = "Read card for retrun card complete";
                        model.Event = "Return Card";
                        model.SubEvent = "Return Card Success";
                        model.CardNo = CARD;
                        var task = Task.Run(async () => await addActionLog(model));


                        var task2 = Task.Run(async () => await addDistributeCardLog("COLLECTED", CARD));
                        logger.Info("Return Card Success :" + CARD);

                        Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                        {


                            ShowPanelReturnCardSuccess();

                        }), DispatcherPriority.Background);

                    }
                    else
                    {
                            if (CURRENT_PANEL == panelReturnCard)
                            {
                                model.Description = "Can not read card for retrun card";
                                model.Event = "Return Card";
                                model.SubEvent = "Return Card Not Success";
                                logger.Info("Return Card Not Success");
                                var task = Task.Run(async () => await addActionLog(model));

                                //alertWindow.setMessage("Error", "Card Return", "Return Card Not Success");
                                //alertWindow.ShowDialog();


                                logger.Info("Return Card Not Success");

                                throw new Exception("Return Card Not Success");
                            }
                    }

                    }
                    catch (ThreadAbortException ex1)
                    {

                    }
                    catch (Exception ex)
                    {
                        if (CURRENT_PANEL == panelReturnCard)
                        {
                            ActionLogModel model = newActionLogModel();
                            logger.Error(ex.ToString());
                            model.Description = "Can not read card for retrun card " + ex.ToString();
                            model.Event = "Return Card";
                            model.SubEvent = "Return Card Not Success";
                            var task = Task.Run(async () => await addActionLog(model));
                            logger.Info("Return Card Not Success");

                            //if (IS_DEBUG)
                            //    MessageBox.Show(ex.ToString());

                            Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                            {

                                //AlertBox aw = new AlertBox();

                                //alertWindow.setMessage("Error", "Card Return", "Return Card Not Success: Can not connect device ");
                                alertWindow.setMessage("Error", "Return Card Not Success", ex.Message);
                                alertWindow.ShowDialog();

                            }), DispatcherPriority.Background);
                            

                            //throw ex;
                            logger.Error(ex.ToString());
                        }
                    }
                     
        }


        private void connectReturnCardDllold()
        {



            new Thread(() =>
            {
                try
                {
                    logger.Info("Start call dll for return card");

                    Thread.CurrentThread.IsBackground = true;
                    CARD = readCardForReturnAsync().Result;

                    ActionLogModel model = newActionLogModel();

                    if (CARD != "")
                    {
                        model.Description = "Read card for retrun card complete";
                        model.Event = "Return Card";
                        model.SubEvent = "Return Card Success";
                        model.CardNo = CARD;
                        var task = Task.Run(async () => await addActionLog(model));

                        logger.Info("Return Card Success :" + CARD);

                        Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                        {


                            ShowPanelReturnCardSuccess();

                        }), DispatcherPriority.Background);

                    }
                    else
                    {
                        if (CURRENT_PANEL == panelReturnCard)
                        {
                            model.Description = "Can not read card for retrun card";
                            model.Event = "Return Card";
                            model.SubEvent = "Return Card Not Success";
                            logger.Info("Return Card Not Success");
                            var task = Task.Run(async () => await addActionLog(model));

                            //alertWindow.setMessage("Error", "Card Return", "Return Card Not Success");
                            //alertWindow.ShowDialog();


                            logger.Info("Return Card Not Success");

                            throw new Exception("Return Card Not Success");
                        }
                    }

                }
                catch (Exception ex)
                {
                    if (CURRENT_PANEL == panelReturnCard)
                    {
                        ActionLogModel model = newActionLogModel();
                        logger.Error(ex.ToString());
                        model.Description = "Can not read card for retrun card " + ex.ToString();
                        model.Event = "Return Card";
                        model.SubEvent = "Return Card Not Success";
                        var task = Task.Run(async () => await addActionLog(model));
                        logger.Info("Return Card Not Success");

                        if (IS_DEBUG)
                            MessageBox.Show(ex.ToString());

                        Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                        {

                            //AlertBox aw = new AlertBox();

                            //alertWindow.setMessage("Error", "Card Return", "Return Card Not Success: Can not connect device ");
                            alertWindow.setMessage("Error", "Return Card Not Success", ex.Message);
                            alertWindow.ShowDialog();

                        }), DispatcherPriority.Background);


                        //throw ex;
                        logger.Error(ex.ToString());
                    }
                }


            }).Start();



        }




        private void panelWelcomeTrueIDC_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Panel p = (Panel)sender;
            if (p.IsVisible)
            {
                CURRENT_PANEL = p;

                //แจกบัตร
                //RFIDCard.RFIDCardReader rfidReader = new RFIDCard.RFIDCardReader();
                //rfidReader.Init();
                //RFIDCard.ReponseIDCard response = rfidReader.Read();
                //ReadWriteCard.ReadWriteCardReader



            }
        }

        private async Task<String> feedCardAsync(DateTime StartDate, DateTime EndDate, string projectCode,string idcardNumber, 
            string cardtype, string WorkpremitID, string deviceList, int walkinObjective)
        {
            try
            {
                string json = @"{
" + "\n" +
                @"    ""StartDate"": """ + convertDatetime(StartDate)   + @""",
" + "\n" +
                @"    ""EndDate"":""" + convertDatetime(EndDate)  + @""",
" + "\n" +
                @"    ""ProjectCode"":""" + projectCode + @""",
" + "\n" +
                @"    ""WorkpremitID"":""" + WorkpremitID + @""",
" + "\n" +
                @"    ""WalkingObjectiveID"":""" + walkinObjective + @""",
" + "\n" +
                @"    ""UserID"":""" + idcardNumber + @""",
" + "\n" +
                @"    ""CardType"":""" + cardtype + @""",
" + "\n" +
                @"    ""LocationSiteId"":""" + LocationCode + @""",
" + "\n" +
                @"    ""LocationBuildingId"":""" + BuildingCode + @""",
" + "\n" +
                @"    ""LocationFloorId"":""" + FloorCode + @""",
" + "\n" +
                @"    ""ItemList"":" + "" + deviceList + @"" + @"
" + "\n" +
                @"}";

                if(IS_DEBUG)
                logger.Info(json);


                RFIDCard.RFIDCardReader reader = new RFIDCard.RFIDCardReader();
                RFIDCard.ReponseStatus initialReaderResult = reader.Init(RFID_READER_COMPORT);


                logger.Info("feedCardAsync initialReaderResult.ResponseCode :" + initialReaderResult.ResponseCode);
                logger.Info("feedCardAsync initialReaderResult.ResponseDescription :" + initialReaderResult.ResponseDescription);

                if (!"0".Equals(initialReaderResult.ResponseCode))
                {
                    ActionLogModel model = newActionLogModel();
                    model.Description = "Can not initail rfid-reader code " + initialReaderResult.ResponseCode + " " + initialReaderResult.ResponseDescription;
                    model.Event = "Hardware";
                    model.SubEvent = "Feed card error code "+initialReaderResult.ResponseCode;
                    var task = Task.Run(async () => await addActionLog(model));

                    throw new Exception("Can not initail rfid-reader : " + initialReaderResult.ResponseCode + " " + initialReaderResult.ResponseDescription);
                }

               



            var datafeedout = await reader.Read(json);


                logger.Info("feedCardAsync datafeedout :" + datafeedout.ResponseCode);
                logger.Info("feedCardAsync ResponseDescription :" + datafeedout.ResponseDescription);

                if ("102".Equals(initialReaderResult.ResponseCode))
                {
                    ActionLogModel model = newActionLogModel();
                    model.Description = "Can not feed card code " + datafeedout.ResponseCode + " " + datafeedout.ResponseDescription;
                    model.Event = "Hardware";
                    model.SubEvent = "Feed card error code " + datafeedout.ResponseCode;
                    var task = Task.Run(async () => await addActionLog(model));


                    throw new Exception("Can not feed card :" + datafeedout.ResponseCode + " " + datafeedout.ResponseDescription);
                    //throw new Exception("102");
                }
                else if(!"0".Equals(datafeedout.ResponseCode))
                {
                    ActionLogModel model = newActionLogModel();
                    model.Description = "Can not feed card code " + datafeedout.ResponseCode + " " + datafeedout.ResponseDescription;
                    model.Event = "Hardware";
                    model.SubEvent = "Feed card error code " + datafeedout.ResponseCode;
                    var task = Task.Run(async () => await addActionLog(model));

                    throw new Exception("Can not feed card :" + datafeedout.ResponseCode + " " + datafeedout.ResponseDescription);
                }

                logger.Info("feedCardAsync datafeedout.IDCard :" + datafeedout.IDCard);

                string cardNumber = datafeedout.IDCard;

                return cardNumber;
            }
            catch (Exception ex)
            {
                /*
                ActionLogModel model = newActionLogModel();
                model.Description = "Can not feed card :" + ex.ToString();
                model.Event = "Hardware ";
                model.SubEvent = "Feed card error";
                var task = Task.Run(async () => await addActionLog(model));
                */
                throw ex;
            }
        }

        public static RFIDCard.RFIDCardReader reader = null;
        private async Task<String> readCardForReturnAsync()
        {
            try
            {
                if (reader == null)
                {
                    reader = new RFIDCard.RFIDCardReader();
                }


                RFIDCard.ReponseStatus initialReaderResult = reader.Init(RFID_READER_COMPORT);
                logger.Info("readCardForReturnAsync initialReaderResult.ResponseCode :" + initialReaderResult.ResponseCode);
                logger.Info("readCardForReturnAsync initialReaderResult.ResponseDescription :" + initialReaderResult.ResponseDescription);
                if (!"0".Equals(initialReaderResult.ResponseCode))
                {
                    throw new Exception("Can not initail rfid-reader : " + initialReaderResult.ResponseCode + " " + initialReaderResult.ResponseDescription);
                }

                logger.Info("readCardForReturnAsync before call cardreturn()");
                var datafeedout = await reader.CardReturn();

                logger.Info("readCardForReturnAsync end call cardreturn() ");


                logger.Info("readCardForReturnAsync datafeedout :" + datafeedout.ResponseCode);
                logger.Info("readCardForReturnAsync ResponseDescription :" + datafeedout.ResponseDescription);

                if (!"0".Equals(datafeedout.ResponseCode))
                {
                    //disable this function change to dll
                    //เมื่อบัตรผิดให้ดีดบัตรออก
                    //feed();
                    //alertWindow.setMessage("Error", "บัตรไม่ถูกต้อง", "Can not read card :" + datafeedout.ResponseCode + " " + datafeedout.ResponseDescription);

                    //alertWindow.ShowDialog();



                    throw new Exception("Can not read card :" + datafeedout.ResponseCode + " " + datafeedout.ResponseDescription);
                }


                logger.Info("readCardForReturnAsync datafeedout.IDCard :" + datafeedout.IDCard);

                string cardNumber = datafeedout.IDCard;

                return cardNumber;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw ex;
            }
        }


        private void panelPrivacySensitive_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Panel p = (Panel)sender;
            if (p.IsVisible)
            {
                CURRENT_PANEL = p;
            }
        }

        private void panelScanQR_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            logger.Info("in panelScanQR_IsVisibleChanged");

            Panel p = (Panel)sender;
            if (p.IsVisible)
            {
                CURRENT_PANEL = p;
            }
            else
            {
                targetTextbox = null;
                popupNumPad.IsOpen = false;
            }


        }

        private void lblOTPpanelScanQR_TouchDown(object sender, TouchEventArgs e)
        {
            if (!popupNumPad.IsOpen)
            {
                panelSmallKeyboardPopup.Visibility = Visibility.Visible;

                popupNumPad.VerticalOffset = 10;
                popupNumPad.PlacementTarget = lblOTPpanelScanQR;
                //popupNumPad.Margin = lblOTPpanelScanQR.PointFromScreen;
                popupNumPad.IsOpen = true;
                targetTextbox = sender as Label;


                keyboardJustOpen = true;
            }
            else
            {
                panelSmallKeyboardPopup.Visibility = Visibility.Hidden;
                popupNumPad.IsOpen = false;
            }
        }


        private void lblRequestNewOTP_TouchDown(object sender, TouchEventArgs e)
        {
            ShowPanelResendQR();
        }

        private List<WorkpermitVw> checkOTP(string otp)
        {
            try
            {
                var client = new RestClient(SERVER_API_URL + "/api/kiosk/getWorkpermitByOTP");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");

                var body = @"{
" + "\n" +
@"    ""otp"": """ + otp + @""",
" + "\n" +
@"     ""CustStaffCardId"": """ + this.idCardOrPassportNumber + @""",
" + "\n" +
@"    ""CustStaffCardType"": """ + this.cardType + @""",
" + "\n" +
@"    ""LocationFloorId"": """ + FloorCode + @"""
" + "\n" +
@"}";

                if (IS_DEBUG)
                    logger.Info(body);



                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);
                IEnumerable<WorkpermitVw> m = JsonConvert.DeserializeObject<IEnumerable<WorkpermitVw>>(response.Content);

                return m.ToList();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return null;
            }
        }


        private List<CustProjectVw> checkEmerOTP(string otp)
        {
            try
            {
                var client = new RestClient(SERVER_API_URL + "/api/kiosk/getProjectByOTP");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");

                var body = @"{
" + "\n" +
@"    ""otp"": """ + otp + @""",
" + "\n" +
@"     ""CustStaffCardId"": """ + this.idCardOrPassportNumber + @""",
" + "\n" +
@"    ""CustStaffCardType"": """ + this.cardType + @""",
" + "\n" +
@"    ""LocationFloorId"": """ + FloorCode + @"""
" + "\n" +
@"}";

                if (IS_DEBUG)
                    logger.Info(body);

                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);
                IEnumerable<CustProjectVw> m = JsonConvert.DeserializeObject<IEnumerable<CustProjectVw>>(response.Content);

                return m.ToList();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return null;
            }
        }

        private void btnNext_panelScanQR_TouchDown(object sender, TouchEventArgs e)
        {
            string otp = lblOTPpanelScanQR.Content.ToString();

            if (otp.Length != 4)
            {


                if (LANGUAGE == LANGUAGE_THAI)
                {
                    alertWindow.setMessage("คำเตือน", "กรุณาระบุรหัส OTP ให้ครบ 4 หลัก", "");
                }
                else
                {

                    alertWindow.setMessage("Warning", "Please entry 4 digits of OTP", "");
                }

                //alertWindow.setMessage("คำเตือน/Warning", "กรุณาระบุรหัส OTP ให้ครบ 4 หลัก", "Please entry 4 digits of OTP");
                alertWindow.ShowDialog();
                return;
            }

            if (WORKING_TYPE == WORKING_TYPE_CUSTOMER)
            {
                displayCheckOTPResult(otp);
            }
            else
            {
                displayCheckOTPEmerResult(otp);
            }
        }


        private void displayCheckOTPResult(string otp)
        {
            logger.Info("displayCheckOTPResult");
            List<WorkpermitVw> m = checkOTP(otp);

            logger.Info("finish check otp");

            bool match = false;

            //ตรวจสอบว่า workpermit ตรงกันหรือไม่

            if (m != null)
            {
                /* if (ALL_WORKPERMIT != null)
                 {
                     foreach (WorkpermitVw b in m)
                     {
                         foreach (WorkpermitVw a in ALL_WORKPERMIT)
                         {
                             if (a.WorkpermitId == b.WorkpermitId)
                             {
                                 match = true;
                                 WORKPERMIT = a;
                                 ShowPanelProjectInformation();
                             }
                         }
                     }
                 }
                 else
                 {
                     foreach (WorkpermitVw b in m)
                     {
                         if (WORKPERMIT.WorkpermitId == b.WorkpermitId)
                         {
                             match = true;
                             ShowPanelProjectInformation();
                         }
                     }
                 }*/

                foreach (WorkpermitVw b in m)
                {
                    if (WORKPERMIT.WorkpermitId == b.WorkpermitId)
                    {
                        match = true;
                        ShowPanelProjectInformation();
                    }
                }
            }

            if (!match)
            {
                ++IDCARD_NOT_CORRECT_COUNT;

                logger.Info("OTP(" + otp + ") not match to id/passport :" + hideIdCard(this.idCardOrPassportNumber));
                //alertWindow.setMessage("Error", "OTP ไม่สอดคล้องกับข้อมูลในระบบ", "OTP not match with work permit information");
                //alertWindow.ShowDialog();

               
                    if (IDCARD_NOT_CORRECT_COUNT < 5)
                    {
                        if (LANGUAGE == LANGUAGE_THAI)
                        {
                            alertWindow.setMessage("การยืนยันตัวตันครั้งที่ " + IDCARD_NOT_CORRECT_COUNT + "/5", "ข้อมูลไม่ถูกต้องกรุณาระบุข้อมูลใหม่", "");

                        }
                        else
                        {
                            alertWindow.setMessage("OTP not match with work permit information (" + IDCARD_NOT_CORRECT_COUNT + "/5)", "Incorrect Data", "");

                        }

                        //alertWindow.setMessage("การยืนยันตัวตันครั้งที่ "+IDCARD_NOT_CORRECT_COUNT +"/5", "ข้อมูลไม่ถูกต้องกรุณาระบุข้อมูลใหม่", "OTP not match with work permit information (" + IDCARD_NOT_CORRECT_COUNT + "/5 times)");

                        alertWindow.ShowDialog();
                    }
                    else
                    {
                        if (LANGUAGE == LANGUAGE_THAI)
                        {
                            alertWindow.setMessage("Error", "เนื่องจากท่านยืนยันตัวตนผิดเป็นจำนวน 5 ครั้ง", "ระบบไม่สามารถออกบัตรให้ท่านได้กรุณาติดต่อเจ้าหน้าที่");
                        }
                        else
                        {
                            alertWindow.setMessage("Error", "Your data is not correct 5 times", "Please contact administrator");
                        }
                    
                    alertWindow.ShowDialog();

                    ShowEntryPanel();
                }
            }
        }



        private void displayCheckOTPEmerResult(string otp)
        {
            logger.Info("displayCheckOTPEmerResult");
            List<CustProjectVw> m = checkEmerOTP(otp);

            bool match = false;

            //ตรวจสอบว่า project ตรงกันหรือไม่

            if (m != null)
            {
                if (ALL_PROJECT != null)
                {
                    foreach (CustProjectVw b in m)
                    {
                        foreach (CustProjectVw a in ALL_PROJECT)
                        {
                            if (a.CustProjectId == b.CustProjectId)
                            {
                                match = true;
                                PROJECT = a;

                                ShowPanelProjectInformation();
                            }
                        }
                    }
                }
                else
                {
                    foreach (CustProjectVw b in m)
                    {
                        if (PROJECT.CustProjectId == b.CustProjectId)
                        {
                            match = true;
                            ShowPanelProjectInformation();
                        }
                    }
                }
            }

            if (!match)
            {
                ++IDCARD_NOT_CORRECT_COUNT;
                logger.Info("OTP(" + otp + ") not match to id/passport :" + hideIdCard(this.idCardOrPassportNumber));
                //alertWindow.setMessage("Error", "OTP ไม่สอดคล้องกับข้อมูลในระบบ", "OTP not match with project information");
                //alertWindow.ShowDialog();


                if (IDCARD_NOT_CORRECT_COUNT < 5)
                {
                    if (LANGUAGE == LANGUAGE_THAI)
                    {
                        alertWindow.setMessage("การยืนยันตัวตันครั้งที่ " + IDCARD_NOT_CORRECT_COUNT + "/5", "ข้อมูลไม่ถูกต้องกรุณาระบุข้อมูลใหม่", "");
                    }
                    else
                    {
                        alertWindow.setMessage("OTP not match with work permit information (" + IDCARD_NOT_CORRECT_COUNT + "/5)", "Incorrect Data", "");

                    }

                    //alertWindow.setMessage("การยืนยันตัวตันครั้งที่ " + IDCARD_NOT_CORRECT_COUNT + "/5", "ข้อมูลไม่ถูกต้องกรุณาระบุข้อมูลใหม่", "OTP not match with work permit information (" + IDCARD_NOT_CORRECT_COUNT + "/5 times)");

                    alertWindow.ShowDialog();
                }
                else
                {
                    if (LANGUAGE == LANGUAGE_THAI)
                    {
                        alertWindow.setMessage("Error", "เนื่องจากท่านยืนยันตัวตนผิดเป็นจำนวน 5 ครั้ง", "ระบบไม่สามารถออกบัตรให้ท่านได้กรุณาติดต่อเจ้าหน้าที่");
                    }
                    else
                    {
                        alertWindow.setMessage("Error", "Your data is not correct 5 times", "Please contact administrator");
                    }
                    
                    alertWindow.ShowDialog();

                    ShowEntryPanel();
                }
            }
        }


        private void btnResendQRByEmail_TouchDown(object sender, TouchEventArgs e)
        {
            logger.Info("btnResendQRByEmail_TouchDown");
            ShowPanelEntryQR();
        }

        private void btnResendQRBySMS_TouchDown(object sender, TouchEventArgs e)
        {
            logger.Info("btnResendQRBySMS_TouchDown");
            ShowPanelEntryOTP();
        }

        private void lblOTPpanelEntryOTP_TouchDown(object sender, TouchEventArgs e)
        {

            if (!popupNumPad.IsOpen)
            {
                panelSmallKeyboardPopup.Visibility = Visibility.Visible;

                popupNumPad.VerticalOffset = 10;
                popupNumPad.PlacementTarget = lblOTPpanelEntryOTP;
                popupNumPad.IsOpen = true;
                targetTextbox = sender as Label;
                keyboardJustOpen = true;
            }
            else
            {
                panelSmallKeyboardPopup.Visibility = Visibility.Hidden;
                popupNumPad.IsOpen = false;
            }
        }

        private void lblRequestNewOTP_panelEntryOTP_TouchDown(object sender, TouchEventArgs e)
        {
            ShowPanelResendQR();
        }

        private void btnNext_panelEntryOTP_TouchDown(object sender, TouchEventArgs e)
        {
            logger.Info("Entry otp and click next");

            string otp = lblOTPpanelEntryOTP.Content.ToString();

            if (otp.Length != 4)
            {
                if (LANGUAGE == LANGUAGE_THAI)
                {
                    alertWindow.setMessage("คำเตือน", "กรุณาระบุรหัส OTP ให้ครบ 4 หลัก", "");
                }
                else
                {

                    alertWindow.setMessage("Warning", "Please entry 4 digits of OTP", "");
                }
                alertWindow.ShowDialog();
                return;
            }

            if (WORKING_TYPE == WORKING_TYPE_CUSTOMER)
            {
                displayCheckOTPResult(otp);
            }
            else
            {
                displayCheckOTPEmerResult(otp);
            }
        }


        private void lblOTPpanelScanQR_panelEntryQR_TouchDown(object sender, TouchEventArgs e)
        {

            if (!popupNumPad.IsOpen)
            {
                panelSmallKeyboardPopup.Visibility = Visibility.Visible;
                popupNumPad.VerticalOffset = 10;
                popupNumPad.PlacementTarget = lblOTPpanelScanQR_panelEntryQR;
                popupNumPad.IsOpen = true;
                targetTextbox = sender as Label;

                keyboardJustOpen = true;
            }
            else
            {
                panelSmallKeyboardPopup.Visibility = Visibility.Hidden;
                popupNumPad.IsOpen = false;
            }
        }

        private void lblRequestNewOTP_panelEntryQR_TouchDown(object sender, TouchEventArgs e)
        {
            ShowPanelResendQR();
        }

        private void btnNext_panelEntryQR_TouchDown(object sender, TouchEventArgs e)
        {
            string otp = lblOTPpanelScanQR_panelEntryQR.Content.ToString();

            if (otp.Length != 4)
            {

                if (LANGUAGE == LANGUAGE_THAI)
                {
                    alertWindow.setMessage("คำเตือน", "กรุณาระบุรหัส OTP ให้ครบ 4 หลัก", "");
                }
                else
                {

                    alertWindow.setMessage("Warning", "Please entry 4 digits of OTP", "");
                }

                //alertWindow.setMessage("คำเตือน/Warning", "กรุณาระบุรหัส OTP ให้ครบ 4 หลัก", "Please entry 4 digits of OTP");
                alertWindow.ShowDialog();
                return;
            }

            if (WORKING_TYPE == WORKING_TYPE_CUSTOMER)
            {
                displayCheckOTPResult(otp);
            }
            else
            {
                displayCheckOTPEmerResult(otp);
            }
        }

        private void btnNext_panelDisplayProject_TouchDown(object sender, TouchEventArgs e)
        {
            if (STAFF != null)
            {
                decimal ver = 0;               
                if (STAFF.PrivacyMarketingVersion != null)
                    ver = STAFF.PrivacyMarketingVersion.Value;
                

                if (Convert.ToDecimal(LAST_PRIVACY_MARKETING_TH.Version) > ver )
                {
                    ShowPanelPrivacyMarketing();
                }
                else
                {
                    ver = 0;
                    if (STAFF.PrivacySensitiveVersion != null)
                        ver = STAFF.PrivacySensitiveVersion.Value;

                    if (Convert.ToDecimal(LAST_PRIVACY_SENSITIVE_TH.Version) > ver ) 
                    {
                        ShowPanelPrivacySensitive();
                    }
                    else
                    {
                        ShowPanelWelcomeToTrueIDC();
                    }
                }
            }
            else
            {

                decimal ver = 0;
                if (STAFFEMER.PrivacyMarketingVersion != null)
                    ver = STAFFEMER.PrivacyMarketingVersion.Value;

                if (Convert.ToDecimal(LAST_PRIVACY_MARKETING_TH.Version) > ver)
                {
                    ShowPanelPrivacyMarketing();
                }
                else
                {
                    ver = 0;
                    if (STAFFEMER.PrivacySensitiveVersion != null)
                        ver = STAFFEMER.PrivacySensitiveVersion.Value;

                    if (Convert.ToDecimal(LAST_PRIVACY_SENSITIVE_TH.Version) > ver)
                    {
                        ShowPanelPrivacySensitive();
                    }
                    else
                    {
                        ShowPanelWelcomeToTrueIDC();
                    }
                }
            }
        }

        private void lblIDCardNumber_TouchDown(object sender, TouchEventArgs e)
        {
            if (!fullKeyboardPopup.IsOpen)
            {
                panelFullKeyboardPopup.Visibility = Visibility.Visible;
                fullKeyboardPopup.PlacementTarget = lblIDCardNumber;
                fullKeyboardPopup.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                fullKeyboardPopup.VerticalOffset = KeyboardVerOffset;
                fullKeyboardPopup.HorizontalOffset = KeyboardHorOffset;
                fullKeyboardPopup.IsOpen = true;

                keyboardJustOpen = true;
            }
            else
            {
                panelFullKeyboardPopup.Visibility = Visibility.Hidden;
                fullKeyboardPopup.IsOpen = false;
            }
        }


        private void btnNext_panelPrivacyMarketing_TouchDown(object sender, TouchEventArgs e)
        {
            //SaveAcceptPrivacyMarketing

            string email = "";
            decimal ver = 0;
            if (WORKING_TYPE == WORKING_TYPE_WALKIN)
            {
                email = STAFFEMER.CustStaffEmail;
                if (STAFFEMER.PrivacySensitiveVersion != null)
                    ver = STAFFEMER.PrivacySensitiveVersion.Value;
            }
            else
            {
                email = STAFF.CustStaffEmail;
                if (STAFF.PrivacySensitiveVersion != null)
                    ver = STAFF.PrivacySensitiveVersion.Value;
            }


            addTcPrivacyAccept(2,0,LAST_PRIVACY_MARKETING_TH.Version,email);

            if (Convert.ToDecimal(LAST_PRIVACY_SENSITIVE_TH.Version) > ver)
            {
                ShowPanelPrivacySensitive();
            }
            else
            {
                ShowPanelWelcomeToTrueIDC();
            }
        }

        private void btnSkip_panelPrivacyMarketing_TouchDown(object sender, TouchEventArgs e)
        {

            string email = "";
            decimal ver = 0;
            if (WORKING_TYPE == WORKING_TYPE_WALKIN)
            {
                email = STAFFEMER.CustStaffEmail;

                if (STAFFEMER.PrivacySensitiveVersion != null)
                    ver = STAFFEMER.PrivacySensitiveVersion.Value;
            }
            else
            {
                email = STAFF.CustStaffEmail;
                if (STAFF.PrivacySensitiveVersion != null)
                    ver = STAFF.PrivacySensitiveVersion.Value;
            }


            addTcPrivacyAccept(2, 2, LAST_PRIVACY_MARKETING_TH.Version , email);
            if (Convert.ToDecimal(LAST_PRIVACY_SENSITIVE_TH.Version) > ver)
            {
                ShowPanelPrivacySensitive();
            }
            else
            {
                ShowPanelWelcomeToTrueIDC();
            }
        }

        private void btnNext_panelPrivacySensitive_TouchDown(object sender, TouchEventArgs e)
        {

            string email = "";
            if (WORKING_TYPE == WORKING_TYPE_WALKIN)
            {
                email = STAFFEMER.CustStaffEmail;
            }
            else
            {
                email = STAFF.CustStaffEmail;
            }

            addTcPrivacyAccept(3, 0, LAST_PRIVACY_SENSITIVE_TH.Version, email);
            ShowPanelWelcomeToTrueIDC();
        }

        private void btnSkip_panelPrivacySensitive_TouchDown(object sender, TouchEventArgs e)
        {

            string email = "";
            if (WORKING_TYPE == WORKING_TYPE_WALKIN)
            {
                email = STAFFEMER.CustStaffEmail;
            }
            else
            {
                email = STAFF.CustStaffEmail;
            }

            addTcPrivacyAccept(3, 2, LAST_PRIVACY_SENSITIVE_TH.Version, email);
            ShowPanelWelcomeToTrueIDC();
        }

        private ActionLogModel newActionLogModel()
        {
            ActionLogModel m = new ActionLogModel();
            return m;
        }

        private WorkpermitStaffMappingVw getStaffDetail(string workpermitID, string idcardPassportNo, string cardType)
        {
            try 
            { 
                var client = new RestClient(SERVER_API_URL + "/api/kiosk/GetStaffDetail");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                var body = @"{
    " + "\n" +
                @"    ""WorkpermitId"": """ + workpermitID + @""",
    " + "\n" +
                @"    ""custStaffCardId"": """ + idcardPassportNo + @""",
    " + "\n" +
                @"    ""CustStaffCardType"": """ + cardType + @"""
    " + "\n" +
                @"}";



                if (IS_DEBUG)
                    logger.Info(body);

                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);


                IEnumerable<WorkpermitStaffMappingVw> m = JsonConvert.DeserializeObject<IEnumerable<WorkpermitStaffMappingVw>>(response.Content);


                return m.FirstOrDefault();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return null;
            }
       }


        private CustStaffVw getStaffEmerDetail(string projectID, string idcardPassportNo, string cardType)
        {
            try
            {
                var client = new RestClient(SERVER_API_URL + "/api/kiosk/GetStaffEmerDetail");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                var body = @"{
    " + "\n" +
                @"    ""ProjectId"": """ + projectID + @""",
    " + "\n" +
                @"    ""custStaffCardId"": """ + idcardPassportNo + @""",
    " + "\n" +
                @"    ""CustStaffCardType"": """ + cardType + @"""
    " + "\n" +
                @"}";


                if (IS_DEBUG)
                    logger.Info(body);

                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);


                IEnumerable<CustStaffVw> m = JsonConvert.DeserializeObject<IEnumerable<CustStaffVw>>(response.Content);


                return m.FirstOrDefault();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return null;
            }
        }



        /* DocType
         * 1=TC,2=PrivacyMarketing,3=PrivacySensitive
         * 
         * Action
         * 0=Accept
         * 1=NotAccept
         * 2=Skip         
         */
        private void addTcPrivacyAccept(int doctype, int action, string version,string email)
        {
            try
            {
                logger.Info("Start addTcPrivacyAccept doctype" + doctype + " action:" + action + " version :" + version + " email :" + email);

                String logTime = convertDatetime(DateTime.Now);  //DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var client = new RestClient(SERVER_API_URL + "/api/kiosk/AcceptTCPrivacy");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                var body = @"{
" + "\n" +
                @"    ""CustStaffEmail"": """ + email + @""",
" + "\n" +
                @"    ""LogVersion"":"""+version+@""",
" + "\n" +
                @"    ""IPAddress"":"""+ NodeIP + @""",
" + "\n" +
                @"    ""Language"":"""+ LANGUAGE + @""",
" + "\n" +
                @"    ""DocType"":"""+ doctype + @""",
" + "\n" +
                @"    ""Action"":"""+ action+@""",
" + "\n" +
                @"    ""ActionDate"":"""+ logTime + @""",
" + "\n" +
                @"    ""ActionChanel"":""1""
" + "\n" +
                @"}";

                if (IS_DEBUG)
                    logger.Info(body);

                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
        private static string CARD = "";
        private void btnNext_panelReturnCard_TouchDown(object sender, TouchEventArgs e)
        {
            try
            {
                logger.Info("btnNext_panelReturnCard_TouchDown ");
                ActionLogModel model = newActionLogModel();

                if (CARD != "")
                {
                    //model.Description = "Read card for retrun card complete";
                    //model.Event = "Return Card";
                    //model.SubEvent = "Return Card Success";
                    //model.CardNo = CARD;
                    //var task = Task.Run(async () => await addActionLog(model));

                    //pop add carddistribute_log
                    //var task2 = Task.Run(async () => await addDistributeCardLog("COLLECTED",CARD));
                    //logger.Info("Add Distribute Card Log Success");
                    //logger.Info("Return Card Success :" + CARD);

                    ShowPanelReturnCardSuccess();
                }
                else
                {
                    //model.Description = "Can not read card for retrun card";
                    //model.Event = "Return Card";
                    //model.SubEvent = "Return Card Not Success";
                    //logger.Info("Return Card Not Success");
                    //var task = Task.Run(async () => await addActionLog(model));

                    alertWindow.setMessage("Error", "Card Return", "Return Card Not Success");
                    alertWindow.ShowDialog();
                }


            }
            catch (Exception ex)
            {

                logger.Error(ex.ToString());

                try
                {
                    ActionLogModel model = newActionLogModel();
                    model.Description = "Can not read card for retrun card " + ex.ToString();
                    model.Event = "Return Card";
                    model.SubEvent = "Return Card Not Success";
                    var task = Task.Run(async () => await addActionLog(model));
                }
                catch { }
                logger.Info("Return Card Not Success");


                if (IS_DEBUG)
                    MessageBox.Show(ex.ToString());

                alertWindow.setMessage("Error", "Card Return", "Return Card Not Success : Can not connect device" );
                alertWindow.ShowDialog();
            }


        }

        private async Task addDistributeCardLog(string cardStatus,string cardnumber)
        {
            try
            {

                String logTime = convertDatetime(DateTime.Now);  //DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var client = new RestClient(SERVER_API_URL + "/api/kiosk/distributeCard");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");

                string obj = "";
                string custProject = "";

                try
                {
                    if ("ISSUED".Equals(cardStatus))
                    {
                        if (WORKING_TYPE == WORKING_TYPE_WALKIN)
                        {
                            obj = WALKIN_OBJ_TXT;
                            //obj = cmbObjective.SelectedItem.ToString();
                            custProject = Convert.ToString(PROJECT.CustProjectId);
                        }
                    }
                }
                catch(Exception ex) {
                    logger.Error(ex.ToString());
                }

                string body = "";

                if ("ISSUED".Equals(cardStatus))
                {

                    if (STAFF != null)
                    {


                        body = @"{
" + "\n" +
                       @"    ""CustStaffCardId"": """ + STAFF.CustStaffCardId + @""",
" + "\n" +
                       @"    ""CustStaffCardType"":""" + STAFF.CustStaffCardType + @""",
" + "\n" +
                       @"    ""CardSerial"":""" + cardnumber + @""",
" + "\n" +
                       @"    ""WorkPermitID"":""" + WORKPERMIT.WorkpermitId + @""",
" + "\n" +
                       @"    ""ActionDate"":""" + logTime + @""",
" + "\n" +
                       @"    ""CardRegisterDate"":""" + logTime + @""",
" + "\n" +
                       @"    ""NodeID"":""" + KioskCode + @""",
" + "\n" +
                       @"    ""NodeIP"":""" + NodeIP + @""",
" + "\n" +
                       @"    ""CardStatus"":""" + cardStatus + @""",
" + "\n" +
                       @"    ""WalkinObj"":""" + obj + @""",
" + "\n" +
                       @"    ""ProjectCode"":""" + custProject + @""",
" + "\n" +
                       @"    ""DistributeFor"":""" + WORKING_TYPE + @"""
" + "\n" +
                       @"}";
                    }
                    else
                    {


                        body = @"{
" + "\n" +
                       @"    ""CustStaffCardId"": """ + STAFFEMER.CustStaffCardId + @""",
" + "\n" +
                       @"    ""CustStaffCardType"":""" + STAFFEMER.CustStaffCardType + @""",
" + "\n" +
                       @"    ""CardSerial"":""" + cardnumber + @""",
" + "\n" +
                       @"    ""WorkPermitID"":""-1"",
" + "\n" +
                       @"    ""ActionDate"":""" + logTime + @""",
" + "\n" +
                       @"    ""CardRegisterDate"":""" + logTime + @""",
" + "\n" +
                       @"    ""NodeID"":""" + KioskCode + @""",
" + "\n" +
                       @"    ""NodeIP"":""" + NodeIP + @""",
" + "\n" +
                       @"    ""CardStatus"":""" + cardStatus + @""",
" + "\n" +
                       @"    ""WalkinObj"":""" + obj + @""",
" + "\n" +
                       @"    ""ProjectCode"":""" + custProject + @""",
" + "\n" +
                       @"    ""DistributeFor"":""" + WORKING_TYPE + @"""
" + "\n" +
                       @"}";
                    }

                }
                else
                {


                    body = @"{
" + "\n" +
                   @"    ""CustStaffCardId"": """",
" + "\n" +
                   @"    ""CustStaffCardType"":"""",
" + "\n" +
                   @"    ""CardSerial"":""" + cardnumber + @""",
" + "\n" +
                   @"    ""WorkPermitID"":""-1"",
" + "\n" +
                   @"    ""ActionDate"":""" + logTime + @""",
" + "\n" +
                   @"    ""CardRegisterDate"":""" + logTime + @""",
" + "\n" +
                   @"    ""NodeID"":""" + KioskCode + @""",
" + "\n" +
                   @"    ""NodeIP"":""" + NodeIP + @""",
" + "\n" +
                   @"    ""CardStatus"":""" + cardStatus + @""",
" + "\n" +
                   @"    ""WalkinObj"":""" + obj + @""",
" + "\n" +
                   @"    ""ProjectCode"":"""",
" + "\n" +
                   @"    ""DistributeFor"":""-1""
" + "\n" +
                   @"}";
                }


               // if (IS_DEBUG)
                    logger.Info(body);

                request.AddParameter("application/json", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);

                logger.Info("end addDistributeCardLog");
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }


        private void InitialVideo()
        {
            try
            {
                String fileUrl = Utility.getHomeVDOFile();
                logger.Info("home video :" + fileUrl);
                if (fileUrl.IndexOf(".mp4") > 0)
                {
                    //logger.Info("Load file video for home :" + fileUrl);

                    //MediaTimeline timeline = new MediaTimeline(new Uri(fileUrl));
                    //timeline.RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever;
                    //MediaClock clock = timeline.CreateClock();
                    
                    //clock.CurrentStateInvalidated += Clock_CurrentStateInvalidated;
                    //clock.CurrentTimeInvalidated += Clock_CurrentTimeInvalidated;
                    //homeVideo.Clock = clock;
                    
                    homeVideo.Source = new Uri(fileUrl);
                    homeVideo.Visibility = Visibility.Visible;
                    //homeVideo.Loop = true;
                }
                else
                {
                    //logger.Info("Load file video for home :" + fileUrl);
                    homeVideo.Pause();
                    homeVideo.Visibility = Visibility.Hidden;
                    //homeVideo.Loop = false;

                    ImageBrush ib = new ImageBrush();
                    ib.ImageSource = new BitmapImage(new Uri(fileUrl, UriKind.RelativeOrAbsolute));
                    ib.Stretch = Stretch.Fill;
                    panelScreenSaver.Background = ib;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private void Clock_CurrentTimeInvalidated(object sender, EventArgs e)
        {
            logger.Info("Clock_CurrentTimeInvalidated");
        }

        private void Clock_CurrentStateInvalidated(object sender, EventArgs e)
        {
            logger.Info("Clock_CurrentStateInvalidated");
        }

        private void panelEnterIDCardOrPassport_TouchDown(object sender, TouchEventArgs e)
        {
            if (fullKeyboardPopup.IsOpen && !keyboardJustOpen)
            {                
                panelFullKeyboardPopup.Visibility = Visibility.Hidden;
                fullKeyboardPopup.IsOpen = false;
            }

            keyboardJustOpen = false;
        }

        private WorkpermitVw getWorkPermitDetail(string workpermitID)
        {
            try
            {

                if (IS_DEBUG)
                    logger.Info("getWorkPermitDetail :"+workpermitID);

                var client = new RestClient(SERVER_API_URL + "/api/kiosk/GetWorkpermit/" + workpermitID);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);

                WorkpermitVw m = JsonConvert.DeserializeObject<WorkpermitVw>(response.Content);

                return m;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return null;
            }
        }


        private CustProjectVw getProjectDetail(string projectID)
        {
            try
            {
                if (IS_DEBUG)
                    logger.Info("getProjectDetail :" + projectID);

                var client = new RestClient(SERVER_API_URL + "/api/kiosk/GetProject/" + projectID);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);
                if (IS_DEBUG)
                    logger.Info(response.Content);

                CustProjectVw m = JsonConvert.DeserializeObject<CustProjectVw>(response.Content);

                return m;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return null;
            }
        }


        private void openFeedCardDevice()
        {
            try
            {
                HandComm = DLLCLASS.CommOpenWithBaut(FEEDDEVICE_COMPORT, BAUT);
                if (HandComm != 0)
                {
                    logger.Info("Feed card comm. port is opened");
                    //MessageBox.Show("Comm. Port is Opened");
                }
                else
                {
                    logger.Info("Feed card open comm. port error");

                    ActionLogModel model = newActionLogModel();
                    model.Event = "Feed card";
                    model.SubEvent = "Feed card open comm. port error";
                    model.Description = "Device port '" + FEEDDEVICE_COMPORT + "' Baut " + BAUT;
                    var task = Task.Run(async () => await addActionLog(model));

                    if (IS_DEBUG)
                        MessageBox.Show("Feed card open comm. port error, Application can not start.");

                    if(!IS_DEBUG)
                        Application.Current.Shutdown();
                    
                }

        
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());


                if (IS_DEBUG)
                    MessageBox.Show("Application can not start :"+ ex.ToString());

                if (!IS_DEBUG)
                    Application.Current.Shutdown();
            }
        }

        private void keepCard()
        {
            if(HandComm != 0) {

                Int32 I;
                Byte DeviceAddr;
                Byte Txcmcode;
                Byte Txcpcode;
                UInt16 TxDataLen;

                Byte[] TxData = new Byte[1024];
                Byte RxReplyType = new byte();
                Byte RxStCode0 = new byte();
                Byte RxStCode1 = new byte();
                Byte RxStCode2 = new byte();
                UInt16 RxDataLen = new byte();
                Byte[] RxData = new Byte[1024];
                String AddrStr = FEEDDEVICE_COMPORT;
                DeviceAddr = Convert.ToByte("00");


                for (int i = 0; i < 1024; ++i)
                {
                    TxData[i] = 0;
                    RxData[i] = 0;
                }

                Txcmcode = 51;
                Txcpcode = 48;
                TxDataLen = 0;

                I = -1;


                I = DLLCLASS.ExecuteCommand(HandComm, DeviceAddr, Txcmcode, Txcpcode, TxDataLen, TxData[0],
                    ref RxReplyType, ref RxStCode0, ref RxStCode1, ref RxStCode2, ref RxDataLen, ref RxData[0]);


                if (I == 0)
                {
                    if (RxReplyType == 80)
                    {
                        logger.Info("Allowed success", "Allowed");
                    }
                    if (RxReplyType == 78)
                    {
                        logger.Error("AllowedError(Error code:)" + RxStCode1 + RxStCode2 + "Allowed");
                    }
                }
                else
                    logger.Error("Allowed Error", "Prohibition");

            }
            else
                logger.Error("Comm. port is not Opened", "Caution");

        }

        private int waitForInsertCard()
        {

            int returnValue = 0;

            if (HandComm != 0)
            {
                Int32 I;
                Byte DeviceAddr;
                Byte Txcmcode;
                Byte Txcpcode;
                UInt16 TxDataLen;

                Byte[] TxData = new Byte[1024];
                Byte RxReplyType = new byte();
                Byte RxStCode0 = new byte();
                Byte RxStCode1 = new byte();
                Byte RxStCode2 = new byte();
                UInt16 RxDataLen = new byte();
                Byte[] RxData = new Byte[1024];
                String AddrStr = FEEDDEVICE_COMPORT;
                DeviceAddr = Convert.ToByte("00");

                for (int i = 0; i < 1024; ++i)
                {
                    TxData[i] = 0;
                    RxData[i] = 0;
                }

                Txcmcode = 49;
                Txcpcode = 48;
                TxDataLen = 0;

                I = -1;


                I = DLLCLASS.ExecuteCommand(HandComm, DeviceAddr, Txcmcode, Txcpcode, TxDataLen, TxData[0],
                    ref RxReplyType, ref RxStCode0, ref RxStCode1, ref RxStCode2, ref RxDataLen, ref RxData[0]);


                if (I == 0)
                {
                    if (RxReplyType == 80) {
                        String S1, S2, S3;
                        S1 = "";
                        S2 = "";
                        S3 = "";

                        returnValue = RxStCode0;

                        switch (RxStCode0) {
                            case 48:
                                S1 = "No card in card channel";
                                break;
                            case 49:
                                S1 = "There is card at front side"; 
                                break;
                            case 50:
                                S1 = "There is card at RF/IC card operation position"; 
                                break;
                        }
                        /*
                Select Case RxStCode1
                    Case 48
                        S2 = "No card in stacker"
                    Case 49
                        S2 = "Lack of card in stacker"
                    Case 50
                        S2 = "Enough cards in stacker"
                End Select
                Select Case RxStCode2
                    Case 48
                        S3 = "The error card bin is not full"
                    Case 49
                        S3 = "The error card bin is full"
                End Select*/

                        logger.Info("Card Status Success" + System.Environment.NewLine + S1 + System.Environment.NewLine + S2 + System.Environment.NewLine + S3, "Card Status");
                    }
                    if (RxReplyType == 78)
                    {
                        logger.Error("Card Status Error(Error code:)" + RxStCode1 + RxStCode2 + "Card Status");
                    }
                }
                else
                    logger.Error("Card Status Error", "Card Status");
            }
            else
                logger.Error("Comm. port is not Opened", "Caution");

            return returnValue;
        }

        private void feed()
        {
            if (HandComm != 0)
            {
                Int32 I;
                Byte DeviceAddr;
                Byte Txcmcode;
                Byte Txcpcode;
                UInt16 TxDataLen;

                Byte[] TxData = new Byte[1024];
                Byte RxReplyType = new byte();
                Byte RxStCode0 = new byte();
                Byte RxStCode1 = new byte();
                Byte RxStCode2 = new byte();
                UInt16 RxDataLen = new byte();
                Byte[] RxData = new Byte[1024];
                String AddrStr = FEEDDEVICE_COMPORT;
                DeviceAddr = Convert.ToByte("00");

                for (int i = 0; i < 1024; ++i)
                {
                    TxData[i] = 0;
                    RxData[i] = 0;
                }

                Txcmcode = 50;
                Txcpcode = 57;
                TxDataLen = 0;

                I = -1;

                I = DLLCLASS.ExecuteCommand(HandComm, DeviceAddr, Txcmcode, Txcpcode, TxDataLen, TxData[0],
                    ref RxReplyType, ref RxStCode0, ref RxStCode1, ref RxStCode2, ref RxDataLen, ref RxData[0]);

                if (I == 0)
                {
                    if (RxReplyType == 80)
                    {
                        logger.Info("Move Card success", "Move Card");
                    }
                    if (RxReplyType == 78)
                    {
                        logger.Error("Move Card Error(Error code:)" + RxStCode1 + RxStCode2 + "Move Card");
                    }
                }
                else
                    logger.Error("Move Card Error", "Move Card");
            }
            else
                logger.Error("Comm. port is not Opened", "Caution");           

        }

        private static int IDCARD_NOT_CORRECT_COUNT = 0;

        private void readQRCode_KeyDown(object sender, KeyEventArgs e)
        {

            TextBox t = (TextBox)sender;

            if (e.Key == Key.Enter)
            {
                string qrCode = t.Text;
                logger.Info("read QRCode :" + qrCode);

                if (qrCode.Contains("Cust"))
                {
                    string workpermit = decodeQR(qrCode);
                    if (workpermit == null)
                    {
                        ActionLogModel model = newActionLogModel();
                        model.Event = "QRCode Reader";
                        model.SubEvent = "QRCode Missmatch";
                        model.Description = "QRCode '" + qrCode + "' is not match to id/passport :" + this.idCardOrPassportNumber + " or QRCode is expire";
                        var task = Task.Run(async () => await addActionLog(model));

                        logger.Info("QRCode '" + qrCode + "' is not match to id/passport :" + hideIdCard(this.idCardOrPassportNumber));


                        if (LANGUAGE == LANGUAGE_THAI)
                        {
                            alertWindow.setMessage("ข้อผิดพลาด", "QRCode ไม่สอดคล้องกับข้อมูลในระบบ", "");
                        }
                        else
                        {

                            alertWindow.setMessage("Error", "QRCode not match with work permit information or QRCode is expire", "");
                        }



                        alertWindow.ShowDialog();
                        return;
                    }
                    logger.Info("workpermit from qrcode is " + workpermit);

                    WorkpermitVw workpermitFromQrCode = getWorkPermitDetail(workpermit);

                    bool match = false;
                    /*if (ALL_WORKPERMIT != null)
                    {
                        foreach (WorkpermitVw a in ALL_WORKPERMIT)
                        {
                            if (a.WorkpermitId == workpermitFromQrCode.WorkpermitId)
                            {
                                match = true;
                                WORKPERMIT = a;
                            }
                        }
                    }
                    else
                    {
                        if (WORKPERMIT.WorkpermitId == workpermitFromQrCode.WorkpermitId)
                        {
                            match = true;
                        }
                    }*/

                    if (WORKPERMIT.WorkpermitId == workpermitFromQrCode.WorkpermitId)
                    {
                        match = true;
                    }

                    if (match)
                    {
                        IDCARD_NOT_CORRECT_COUNT = 0;
                        ActionLogModel model = newActionLogModel();
                        model.Event = "QRCode Reader";
                        model.SubEvent = "QRCode Matched";
                        model.Description = "QRCode '" + qrCode + "' is match to id/passport :" + this.idCardOrPassportNumber;
                        var task = Task.Run(async () => await addActionLog(model));


                        ShowPanelProjectInformation();
                    }


                    if (!match)
                    {
                        ++IDCARD_NOT_CORRECT_COUNT;


                        ActionLogModel model = newActionLogModel();
                        model.Event = "QRCode Reader";
                        model.SubEvent = "QRCode Missmatch";
                        model.Description = "QRCode '" + qrCode + "' is not match to id/passport :" + this.idCardOrPassportNumber + ", not correct count "+ IDCARD_NOT_CORRECT_COUNT;
                        var task = Task.Run(async () => await addActionLog(model));

                        logger.Info("QRCode '" + qrCode + "' is not match to id/passport :" + hideIdCard(this.idCardOrPassportNumber) + ", not correct count " + IDCARD_NOT_CORRECT_COUNT);


                        if (IDCARD_NOT_CORRECT_COUNT < 5)
                        {
                            if (LANGUAGE == LANGUAGE_THAI)
                            {
                                alertWindow.setMessage("การยืนยันตัวตันครั้งที่ " + IDCARD_NOT_CORRECT_COUNT + "/5", "ข้อมูลไม่ถูกต้องกรุณาระบุข้อมูลใหม่", "");

                            }
                            else
                            {
                                alertWindow.setMessage("QRCode not match with work permit information (" + IDCARD_NOT_CORRECT_COUNT + "/5)", "Incorrect Data", "");

                            }

                            //alertWindow.setMessage("การยืนยันตัวตันครั้งที่ " + IDCARD_NOT_CORRECT_COUNT + "/5", "ข้อมูลไม่ถูกต้องกรุณาระบุข้อมูลใหม่", "QRCode not match with work permit information (" + IDCARD_NOT_CORRECT_COUNT + "/5 times)");

                            alertWindow.ShowDialog();
                        }
                        else
                        {
                            if (LANGUAGE == LANGUAGE_THAI)
                            {
                                alertWindow.setMessage("Error", "เนื่องจากท่านยืนยันตัวตนผิดเป็นจำนวน 5 ครั้ง", "ระบบไม่สามารถออกบัตรให้ท่านได้กรุณาติดต่อเจ้าหน้าที่");
                            }
                            else
                            {
                                alertWindow.setMessage("Error", "Your data is not correct 5 times", "Please contact administrator");
                            }
                            
                            alertWindow.ShowDialog();

                            ShowEntryPanel();
                        }
                    }
                }
                else if (qrCode.Contains("Emer"))
                {
                    string project = decodeQREmer(qrCode);
                    if (project == null)
                    {
                        ActionLogModel model = newActionLogModel();
                        model.Event = "QRCode Reader";
                        model.SubEvent = "QRCode Missmatch";
                        model.Description = "QRCode '" + qrCode + "' is not match to id/passport :" + this.idCardOrPassportNumber + " or QRCode is expire";
                        var task = Task.Run(async () => await addActionLog(model));

                        logger.Info("QRCode '" + qrCode + "' is not match to id/passport :" + hideIdCard(this.idCardOrPassportNumber));


                        if (LANGUAGE == LANGUAGE_THAI)
                        {
                            alertWindow.setMessage("ข้อผิดพลาด", "QRCode ไม่สอดคล้องกับข้อมูลในระบบ", "");
                        }
                        else
                        {

                            alertWindow.setMessage("Error", "QRCode not match with work permit information or QRCode is expire", "");
                        }




                        //alertWindow.setMessage("Error", "QRCode ไม่สอดคล้องกับข้อมูลในระบบ", "QRCode not match with work permit information or QRCode is expire");

                        alertWindow.ShowDialog();
                        return;
                    }
                    logger.Info("project from qrcode is " + project);


                    CustProjectVw projectFromQrCode = getProjectDetail(project);

                    bool match = false;
                    if (ALL_PROJECT != null)
                    {
                        foreach (CustProjectVw a in ALL_PROJECT)
                        {
                            if (a.CustProjectId == projectFromQrCode.CustProjectId)
                            {
                                match = true;
                                PROJECT = a;
                            }
                        }
                    }
                    else
                    {
                        if (PROJECT.CustProjectId == projectFromQrCode.CustProjectId)
                        {
                            match = true;
                        }
                    }


                    if (match)
                    {
                        IDCARD_NOT_CORRECT_COUNT = 0;

                        ActionLogModel model = newActionLogModel();
                        model.Event = "QRCode Reader";
                        model.SubEvent = "QRCode Matched";
                        model.Description = "QRCode '" + qrCode + "' is match to id/passport :" + this.idCardOrPassportNumber;
                        var task = Task.Run(async () => await addActionLog(model));


                        ShowPanelProjectInformation();
                    }


                    if (!match)
                    {

                        ++IDCARD_NOT_CORRECT_COUNT;

                        ActionLogModel model = newActionLogModel();
                        model.Event = "QRCode Reader";
                        model.SubEvent = "QRCode Missmatch";
                        model.Description = "QRCode '" + qrCode + "' is not match to id/passport :" + this.idCardOrPassportNumber + ", not correct count " + IDCARD_NOT_CORRECT_COUNT;
                        var task = Task.Run(async () => await addActionLog(model));

                        logger.Info("QRCode '" + qrCode + "' is not match to id/passport :" + hideIdCard(this.idCardOrPassportNumber) + ", not correct count " + IDCARD_NOT_CORRECT_COUNT);



                        if (IDCARD_NOT_CORRECT_COUNT < 5)
                        {
                            if (LANGUAGE == LANGUAGE_THAI)
                            {
                                alertWindow.setMessage("การยืนยันตัวตันครั้งที่ " + IDCARD_NOT_CORRECT_COUNT + "/5", "ข้อมูลไม่ถูกต้องกรุณาระบุข้อมูลใหม่", "");

                            }
                            else
                            {
                                alertWindow.setMessage("QRCode not match with work permit information (" + IDCARD_NOT_CORRECT_COUNT + "/5)", "Incorrect Data", "");

                            }
                            //alertWindow.setMessage("การยืนยันตัวตันครั้งที่ " + IDCARD_NOT_CORRECT_COUNT + "/5", "ข้อมูลไม่ถูกต้องกรุณาระบุข้อมูลใหม่", "QRCode not match with work permit information (" + IDCARD_NOT_CORRECT_COUNT + "/5 times)");

                            alertWindow.ShowDialog();
                        }
                        else
                        {
                            if (LANGUAGE == LANGUAGE_THAI)
                            {
                                alertWindow.setMessage("Error", "เนื่องจากท่านยืนยันตัวตนผิดเป็นจำนวน 5 ครั้ง", "ระบบไม่สามารถออกบัตรให้ท่านได้กรุณาติดต่อเจ้าหน้าที่");
                            }
                            else
                            {
                                alertWindow.setMessage("Error", "Your data is not correct 5 times", "Please contact administrator");
                            }
                            alertWindow.ShowDialog();

                            ShowEntryPanel();
                        }



                    }
                }


                t.Text = "";
                t.Focus();
            }
        }



        private void Image_TouchDown(object sender, TouchEventArgs e)
        {
            if (IS_DEBUG)
                Application.Current.Shutdown();
        }

        private void homeVideo_MediaFailed_1(object sender, ExceptionRoutedEventArgs e)
        {
            try
            {
                logger.Error("In home media failed");
                //logger.Error(((WPFMediaKit.DirectShow.Controls.MediaUriElement)sender).Name);
                logger.Error(e.ToString());
                logger.Error(e.ErrorException.ToString());
                logger.Error(e.ErrorException.Message);
                //logger.ErrorToDblog(e.Exception.Message, Logger.LOG_SYSTEM, null, null);
                //MessageBox.Show(this, e.ToString());
                logger.Error("End media failed");
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }


        private void homeVideo_MediaFailed(object sender, WPFMediaKit.DirectShow.MediaPlayers.MediaFailedEventArgs e)
        {
            try
            {
                logger.Error("In home media failed");
                //logger.Error(((WPFMediaKit.DirectShow.Controls.MediaUriElement)sender).Name);
                logger.Error(e.ToString());
                logger.Error(e.Exception.Message);
                logger.Error(e.Exception.InnerException.Message);
                //logger.ErrorToDblog(e.Exception.Message, Logger.LOG_SYSTEM, null, null);
                //MessageBox.Show(this, e.ToString());
                logger.Error("End media failed");
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IS_DEBUG)
                Application.Current.Shutdown();
        }

        private void homeVideo_Loaded(object sender, RoutedEventArgs e)
        {
            homeVideo.Play();
        }

        private void homeVideo_MediaEnded(object sender, RoutedEventArgs e)
        {

            homeVideo.Position = TimeSpan.FromSeconds(0);
            homeVideo.Play();
        }

        private void panelEntryOTP_TouchDown(object sender, TouchEventArgs e)
        {

            if (popupNumPad.IsOpen && !keyboardJustOpen)
            {
                panelSmallKeyboardPopup.Visibility = Visibility.Hidden;
                popupNumPad.IsOpen = false;
            }

            keyboardJustOpen = false;
        }

        private void panelEntryQR_TouchDown(object sender, TouchEventArgs e)
        {
            if (popupNumPad.IsOpen && !keyboardJustOpen)
            {
                panelSmallKeyboardPopup.Visibility = Visibility.Hidden;
                popupNumPad.IsOpen = false;
            }

            keyboardJustOpen = false;
        }

        private void panelScanQR_TouchDown(object sender, TouchEventArgs e)
        {
            if (popupNumPad.IsOpen && !keyboardJustOpen)
            {
                panelSmallKeyboardPopup.Visibility = Visibility.Hidden;
                popupNumPad.IsOpen = false;
            }

            keyboardJustOpen = false;
        }

        private void loadProjectToList()
        {
            projectLstBox.Items.Clear();
            if (ALL_PROJECT != null)
            {
                int i = 0;
                foreach (CustProjectVw w in ALL_PROJECT)
                {
                    //15 Feb 2022 (Pop)
                    List<MLocationFloor> devideID = null;
                    devideID = getDeviceIDByProject(Convert.ToString(w.CustProjectId));
                    string rooms = roomFromDevide(devideID);
                    
                    w.Location = rooms;
                    projectLstBox.Items.Insert(i++, w);
                }

                
            }

        }

        

        private void loadWorkpermitToList()
        {
            workpermitLstBox.Items.Clear();
            if (ALL_WORKPERMIT != null)
            {
                int i = 0;
                foreach (WorkpermitVw w in ALL_WORKPERMIT)
                {
                    //15 Feb 2022 (Pop)

                    List<MLocationFloor> devideID = null;
                    devideID = getDeviceID(w.WorkpermitId);
                    string rooms = roomFromDevide(devideID);

                    w.Location = rooms;

                    workpermitLstBox.Items.Insert(i++, w);
                }
            }
        }

        private void showPanelSelectProject()
        {
            loadProjectToList();

            CURRENT_PANEL.Visibility = Visibility.Hidden;
            panelSelectProject.Visibility = Visibility.Visible;
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelSelectProject;
        }


        private void showPanelSelectWorkpermit()
        {
            loadWorkpermitToList();

            CURRENT_PANEL.Visibility = Visibility.Hidden;
            panelSelectWorkpermit.Visibility = Visibility.Visible;
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelSelectProject;
        }

        private void panelSelectProject_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            CURRENT_PANEL = (Panel)sender;
        }

        private void btnNext_panelSelectProject_TouchDown(object sender, TouchEventArgs e)
        {

            if (projectLstBox.SelectedIndex < 0)
            {

                if (LANGUAGE == LANGUAGE_THAI)
                {
                    alertWindow.setMessage("พบข้อผิดพลาด", "กรุณาเลือกโครงการก่อนเข้าใช้งาน", "");
                }
                else
                {

                    alertWindow.setMessage("Error", "Please select your project", "");
                }


                alertWindow.ShowDialog();

                return;
            }

            CustProjectVw selectProject = (CustProjectVw)projectLstBox.SelectedValue;

            foreach (CustProjectVw w in ALL_PROJECT)
            {
                if(w.CustProjectId == selectProject.CustProjectId)
                {
                    PROJECT = w;
                }
            }

            projectLstBox.Items.Clear();

            decimal ver = 0;
            if (WORKING_TYPE == WORKING_TYPE_WALKIN)
            {
                if(STAFFEMER.TcVersion!=null)
                    ver = STAFFEMER.TcVersion.Value;
            }
            else
            {
                if(STAFF.TcVersion!=null)
                    ver = STAFF.TcVersion.Value;
            }

            if (Convert.ToDecimal(LAST_TC_TH.Version) > ver)
            {
                ShowTCPanel();
            }
            else
            {
                if (WORKING_TYPE == WORKING_TYPE_CUSTOMER)
                {
                    ShowScanQRPanel();
                }
                else
                {
                    ShowPanelResendQR();
                }
            }
        }

        private void lblIDCardNumber_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IS_DEBUG)
            {
                if (!fullKeyboardPopup.IsOpen)
                {
                    panelFullKeyboardPopup.Visibility = Visibility.Visible;
                    fullKeyboardPopup.PlacementTarget = lblIDCardNumber;
                    fullKeyboardPopup.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    fullKeyboardPopup.VerticalOffset = KeyboardVerOffset;
                    fullKeyboardPopup.HorizontalOffset = KeyboardHorOffset;
                    fullKeyboardPopup.IsOpen = true;

                    keyboardJustOpen = true;
                }
                else
                {
                    panelFullKeyboardPopup.Visibility = Visibility.Hidden;
                    fullKeyboardPopup.IsOpen = false;
                }
            }
        }


        private void panelReturnCard_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Panel p = (Panel)sender;
            if (p.IsVisible)
            {
                CURRENT_PANEL = p;

                timeWaitForCallDll = 0;
                timerCallReturnCardDll.Start();
            }

        }


        private void ShowPanelReturnCard()
        {
            CURRENT_PANEL.Visibility = Visibility.Hidden;
            panelReturnCard.Visibility = Visibility.Visible;
            panelHeader.Visibility = Visibility.Visible;
            panelFotter.Visibility = Visibility.Visible;

            AUTO_CANCEL_COUNT = TimeoutPanelReturnCard;
        }


        private void Op_Completed(object sender, EventArgs e)
        {
            try { 
                var op2 = App.Current.Dispatcher.BeginInvoke(
                   System.Windows.Threading.DispatcherPriority.Background,
                   new NextPrimeDelegate(this.connectReturnCardDll));
                op2.Wait();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //throw new NotImplementedException();
        }

        private void btnWalkin_TouchDown(object sender, TouchEventArgs e)
        {
            WORKING_TYPE = WORKING_TYPE_WALKIN;
            ShowCustomerIDCardEntryScreenPanel();
        }

        private void btnWalkin_Click(object sender, RoutedEventArgs e)
        {
            WORKING_TYPE = WORKING_TYPE_WALKIN;
            ShowCustomerIDCardEntryScreenPanel();
        }

        private void btnCustomer_Click(object sender, RoutedEventArgs e)
        {
            WORKING_TYPE = WORKING_TYPE_CUSTOMER;
            ShowCustomerIDCardEntryScreenPanel();

        }



        private void btnReturnCard_TouchDown(object sender, TouchEventArgs e)
        {
            
            try
            {
                logger.Info("Click return card button");
                btnReturnCard.IsEnabled = false;

                //disable ปุ่ม back

                var op = App.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Render,
                    new NextPrimeDelegate(this.ShowPanelReturnCard));

                op.Wait();
                //op.Completed += Op_Completed;
                //op.

               
            }
            catch (Exception ex){

                if (LANGUAGE == LANGUAGE_THAI)
                {
                    alertWindow.setMessage("พบข้อผิดพลาด", "ไม่สามารถคืนบัตรได้", "กรุณาตรวจสอบบัตรที่คืน หรือ ติดต่อผู้ดูแลระบบ");
                }
                else
                {

                    alertWindow.setMessage("Error", "Can not return card", "Please contact administrator");
                }


                alertWindow.ShowDialog();
            }
            finally
            {
                btnReturnCard.IsEnabled = true;
            }
        }

        private void btnReturnCard_Click(object sender, RoutedEventArgs e)
        {
            /*
            try
            {
                
                logger.Info("Click return card button");
                btnReturnCard.IsEnabled = false;

                var op = App.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Render,
                    new NextPrimeDelegate(this.ShowPanelReturnCard));

                op.Wait();
                
            }
            catch (Exception ex)
            {
                alertWindow.setMessage("พบข้อผิดพลาด", "ไม่สามารถคืนบัตรได้", "กรุณาตรวจสอบบัตรที่คืน หรือ ติดต่อผู้ดูแลระบบ");
                alertWindow.ShowDialog();
            }
            finally
            {
                btnReturnCard.IsEnabled = true;
            }*/
            
        }

        private void panelSelectWorkpermit_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CURRENT_PANEL = (Panel)sender;
        }

        private void btnNext_panelSelectWorkpermit_TouchDown(object sender, TouchEventArgs e)
        {

            if (workpermitLstBox.SelectedIndex < 0)
            {

                if (LANGUAGE == LANGUAGE_THAI)
                {
                    alertWindow.setMessage("พบข้อผิดพลาด", "กรุณาเลือกโครงการก่อนเข้าใช้งาน", "");
                }
                else
                {

                    alertWindow.setMessage("Error", "Please select your workpermit", "");
                }


                alertWindow.ShowDialog();

                return;
            }

            WorkpermitVw selectWorkpermit = (WorkpermitVw)workpermitLstBox.SelectedValue;

            foreach (WorkpermitVw w in ALL_WORKPERMIT)
            {
                if (w.WorkpermitId == selectWorkpermit.WorkpermitId)
                {
                    WORKPERMIT = w;
                }
            }

            workpermitLstBox.Items.Clear();

            decimal ver = 0;
            if (WORKING_TYPE == WORKING_TYPE_WALKIN)
            {
                if (STAFFEMER.TcVersion != null)
                    ver = STAFFEMER.TcVersion.Value;
            }
            else
            {
                if (STAFF.TcVersion != null)
                    ver = STAFF.TcVersion.Value;
            }

            if (Convert.ToDecimal(LAST_TC_TH.Version) > ver)
            {
                ShowTCPanel();
            }
            else
            {
                if (WORKING_TYPE == WORKING_TYPE_CUSTOMER)
                {
                    ShowScanQRPanel();
                }
                else
                {
                    ShowPanelResendQR();
                }
            }
        }

        private void btnHome12_TouchDown(object sender, TouchEventArgs e)
        {
            CURRENT_PANEL.Visibility = Visibility.Hidden;

            if (reader != null)
            {
                reader.Close();
            }

            ShowEntryPanel();
        }
    }
}

public class ReponseStatus
    {
        private string _ResponseCode;
        private string _ResponseDescription;
        public string ResponseCode
        {
            get { return _ResponseCode; }
            set { _ResponseCode = value; }
        }
        public string ResponseDescription
        {
            get { return _ResponseDescription; }
            set { _ResponseDescription = value; }
        }

    }
    public class ReponseIDCard
    {
        private string _ResponseCode;
        private string _ResponseDescription;
        private string _IDCard;
        private string _ProjectCode;
        public string ResponseCode
        {
            get { return _ResponseCode; }
            set { _ResponseCode = value; }
        }
        public string ResponseDescription
        {
            get { return _ResponseDescription; }
            set { _ResponseDescription = value; }
        }
        public string IDCard
        {
            get { return _IDCard; }
            set { _IDCard = value; }
        }
        public string ProjectCode
        {
            get { return _ProjectCode; }
            set { _ProjectCode = value; }
        }

    }

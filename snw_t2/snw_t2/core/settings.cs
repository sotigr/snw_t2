namespace snw.core.system.settings
{
    public class GENERAL
    {
        public static string DOMAIN;
    }
    public class ARCHITECT
    {
        public static string MASTER_PAGE_PATH;
        public static string CONTENT_PAGE_PATH;
        public static string CSS_PATH;
        public static string JS_PATH;
        public static string STATUS_PAGE_PATH;
    }
    public class DATABASE
    {
        public static string DOMAIN;
        public static string PORT;
        public static string USERNAME;
        public static string PASSWORD;
        public static string DATABASE_NAME;
    }
    public class STORAGE
    {
        public static string IP_ADDRESS_RECEIVE;
        public static string IP_ADDRESS_SEND;
        public static string IP_ADDRESS_DELETE;
        public static string PORT_RECEIVE;
        public static string PORT_SEND;
        public static string PORT_DELETE;
        public static int BUFFER_SIZE;
    }
    public class PROXY_REPORTER
    {
        public static string IP_ADDRESS;
        public static string PORT;
        public static int INTERVAL;
    }
}

namespace snw.core
{

    public class rsa_component
    {
        public struct PublicKey
        {
            public string n;
            public string e;
        };
        public struct PrivateKey
        {
            public string n;
            public string d;
        };
        public struct PrimeSet
        {
            public string p;
            public string q;
        }
    }
    public class rsa_info
    {
        public static rsa_component.PublicKey publicKey;
        public static rsa_component.PrivateKey privateKey;
        public static rsa_component.PrimeSet primeSet;
        public static string phi;
    }
    public class aes_info
    {
        public static byte[] key;
    }
}
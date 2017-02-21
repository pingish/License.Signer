using System;
namespace Zymergi.License.Signature
{
    /// <summary>
    /// Command-line application to use LicenseSigner to sign files.
    /// </summary>
    class Program
    {
        static LicenseSigner s;
        static string _keyContainerName;
        static string _pathXmlToSign;

        private static void signOrShowKeys(string filePathOrState)
        {
            switch(filePathOrState)
            {
                case "both":
                case "private":
                    Console.WriteLine(s.RSABothKeysXml);
                    break;
                case "public":
                    Console.WriteLine(s.RSAPublicKeyOnlyXml);
                    break;
                default:
                    _pathXmlToSign = filePathOrState;
                    s.Sign(_pathXmlToSign);
                    break;
            }
        }
        static void Main(string[] args)
        {
           
            if (args.Length == 0)
                ShowHelp();

            if (args.Length >= 1)
            { 
                _keyContainerName = args[0];
                s = new LicenseSigner(_keyContainerName);
          
            }

            if (args.Length >= 2)
                signOrShowKeys(args[1]);
            else
                signOrShowKeys("public");
           
            
        }

        static void ShowHelp()
        {
            Console.WriteLine("Affixes signature to an xml file using RSA.");
            Console.WriteLine("");
            Console.WriteLine("ZYMERGI.LICENSE.SIGNATURE <keyContainerName> <xml file>");
            Console.WriteLine("");
            Console.WriteLine("Note: <xml file> will be modified upon affixing digital signature.");
        }
    }
}

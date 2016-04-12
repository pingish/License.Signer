using System;
namespace Zymergi.License.Signature
{
    /// <summary>
    /// Command-line application to use LicenseSigner to sign files.
    /// </summary>
    class Program
    {
        static LicenseSigner s;
        static string keyContainerName;
        static string pathXmlToSign;

        static void Main(string[] args)
        {
            if (args.Length == 0)
                showHelp();

            if (args.Length == 1)
            { 
                keyContainerName = args[0];
                s = new LicenseSigner(keyContainerName);
                Console.WriteLine(s.RSAPublicKeyOnlyXml);
            }

            if (args.Length== 2 )
            {
                keyContainerName = args[0];
                pathXmlToSign = args[1];
                s = new LicenseSigner(keyContainerName);
                s.Sign(pathXmlToSign);
            }
            
        }

        static void showHelp()
        {
            Console.WriteLine("Affixes signature to an xml file using RSA.");
            Console.WriteLine("");
            Console.WriteLine("ZYMERGI.LICENSE.SIGNATURE <keyContainerName> <xml file>");
            Console.WriteLine("");
            Console.WriteLine("Note: <xml file> will be modified upon affixing digital signature.");
        }
    }
}

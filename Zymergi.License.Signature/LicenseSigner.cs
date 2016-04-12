using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;


namespace Zymergi.License.Signature
{
    /// <summary>
    /// LicenseSigner's job is to affix a signature (xml element) to an xml file.  
    /// </summary>
    /// <remarks>
    /// The purpose of LicenseSigner is to allow a LicenseVerifier (given a public key) to verify
    /// the authenticity of the signature in an xml file.
    /// 
    /// Since the xml file is intended to be a (software) license, where there should be one 
    /// signature per license, LicenseSigner removes all previous signatures.
    /// </remarks>
    public class LicenseSigner
    {

        CspParameters cspParams;
        RSACryptoServiceProvider rsaKey;
        XmlDocument xmlDocToSign;
        string pathXmlToSign;
        /// <summary>
        /// LicenseSigner depends on the name of a "key container" whose string value is used to generate a public and private key.
        /// </summary>
        /// <param name="nameOfKeyContainer">Name of Key Container</param>
        /// <param name="uriReference">(Optional) namespace to sign</param>
        public LicenseSigner(string nameOfKeyContainer)
        {
            cspParams = new CspParameters();
            cspParams.KeyContainerName = nameOfKeyContainer;
            rsaKey = new RSACryptoServiceProvider(cspParams);
        }

        /// <summary>
        /// Both public and private RSA keys in xml.
        /// </summary>
        /// <remarks>The private key should be kept secret.</remarks>
        public string RSABothKeysXml
        {
            get { return rsaKey.ToXmlString(true); }

        }
        /// <summary>
        /// Only public RSA key in xml.
        /// </summary>
        public string RSAPublicKeyOnlyXml
        {
            get { return rsaKey.ToXmlString(false); }
        }

        private string originalXml;
        /// <summary>
        /// Contains the string xml of the document to be signed.
        /// </summary>
        public string InputXml { get { return originalXml; } }

        private string signedXml;

        /// <summary>
        /// Affixes an xml signature element to the end of the document.
        /// </summary>
        /// <param name="pathXmlFile">path to the source xml file.</param>
        /// <param name="uriReference">(Optional) Namespace to sign.</param>
        public void Sign(string pathXmlFile, string uriReference = "" )
        {
            loadXmlDocument(pathXmlFile);
            
            XmlElement xmlSignature = generateSignature(uriReference);

            affixSignatureToDocument(xmlSignature);
        }

        private void loadXmlDocument(string pathXmlFile)
        {
            pathXmlToSign = pathXmlFile;
            xmlDocToSign = new XmlDocument();
            xmlDocToSign.PreserveWhitespace = true;
            xmlDocToSign.Load(pathXmlToSign);

            XmlNodeList existingSignatures = xmlDocToSign.DocumentElement.GetElementsByTagName("Signature");

            //remove existing signatures
            for (int i = existingSignatures.Count - 1; i >= 0; i--)
                xmlDocToSign.DocumentElement.RemoveChild(existingSignatures[i]);

            originalXml = xmlDocToSign.OuterXml;
        }

        private XmlElement generateSignature(string uriReference)
        {
            //create signed xml dogument
            SignedXml signedXml = new SignedXml(xmlDocToSign);
            signedXml.SigningKey = rsaKey;

            //tell it what to sign
            Reference whatToSign = new Reference();
            whatToSign.Uri = uriReference;

            XmlDsigEnvelopedSignatureTransform envelope = new XmlDsigEnvelopedSignatureTransform();
            whatToSign.AddTransform(envelope);

            signedXml.AddReference(whatToSign);
            signedXml.ComputeSignature();

            return signedXml.GetXml();
        }

        private void affixSignatureToDocument(XmlElement xmlSignature)
        {
            bool performDeepClone = true;

            XmlNode nodeSignature = xmlDocToSign.ImportNode(xmlSignature, performDeepClone);

            xmlDocToSign.DocumentElement.AppendChild(nodeSignature);

            signedXml = xmlDocToSign.OuterXml;
            xmlDocToSign.Save(pathXmlToSign);
        }
    }
}

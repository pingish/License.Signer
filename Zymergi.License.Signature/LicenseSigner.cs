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
        readonly RSACryptoServiceProvider _rsaKey;
        XmlDocument _xmlDocToSign;
        string _pathXmlToSign;
        /// <summary>
        /// LicenseSigner depends on the name of a "key container" whose string value is used to generate a public and private key.
        /// </summary>
        /// <param name="nameOfKeyContainer">Name of Key Container</param>
        /// <param name="uriReference">(Optional) namespace to sign</param>
        public LicenseSigner(string nameOfKeyContainer)
        {
            var cspParams = new CspParameters
            {
                KeyContainerName = nameOfKeyContainer
            };
            _rsaKey = new RSACryptoServiceProvider(cspParams);
        }

        /// <summary>
        /// Both public and private RSA keys in xml.
        /// </summary>
        /// <remarks>The private key should be kept secret.</remarks>
        public string RSABothKeysXml => _rsaKey.ToXmlString(true);

        /// <summary>
        /// Only public RSA key in xml.
        /// </summary>
        public string RSAPublicKeyOnlyXml => _rsaKey.ToXmlString(false);

        private string _originalXml;
        /// <summary>
        /// Contains the string xml of the document to be signed.
        /// </summary>
        public string InputXml { get { return _originalXml; } }

        // private string signedXml;

        /// <summary>
        /// Affixes an xml signature element to the end of the document.
        /// </summary>
        /// <param name="pathXmlFile">path to the source xml file.</param>
        /// <param name="uriReference">(Optional) Namespace to sign.</param>
        public void Sign(string pathXmlFile, string uriReference = "" )
        {
            loadXmlDocument(pathXmlFile);
            
            var xmlSignature = generateSignature(uriReference);

            affixSignatureToDocument(xmlSignature);
        }

        private void loadXmlDocument(string pathXmlFile)
        {
            _pathXmlToSign = pathXmlFile;
            _xmlDocToSign = new XmlDocument {PreserveWhitespace = true};
            _xmlDocToSign.Load(_pathXmlToSign);

            if (_xmlDocToSign.DocumentElement != null)
            {
                var existingSignatures = _xmlDocToSign.DocumentElement.GetElementsByTagName("Signature");

                //remove existing signatures
                for (int i = existingSignatures.Count - 1; i >= 0; i--)
                    _xmlDocToSign.DocumentElement.RemoveChild(existingSignatures[i]);
            }

            _originalXml = _xmlDocToSign.OuterXml;
        }

        private XmlElement generateSignature(string uriReference)
        {
            //create signed xml dogument
            var signedXml = new SignedXml(_xmlDocToSign)
            {
                SigningKey = _rsaKey
            };

            //tell it what to sign
            var whatToSign = new Reference
            {
                Uri = uriReference
            };

            var envelope = new XmlDsigEnvelopedSignatureTransform();
            whatToSign.AddTransform(envelope);

            signedXml.AddReference(whatToSign);
            signedXml.ComputeSignature();

            return signedXml.GetXml();
        }

        private void affixSignatureToDocument(XmlElement xmlSignature)
        {
            const bool performDeepClone = true;

            var nodeSignature = _xmlDocToSign.ImportNode(xmlSignature, performDeepClone);

            _xmlDocToSign.DocumentElement?.AppendChild(nodeSignature);

            // signedXml = _xmlDocToSign.OuterXml;
            _xmlDocToSign.Save(_pathXmlToSign);
        }
    }
}

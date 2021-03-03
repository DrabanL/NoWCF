using NoWCF.Utilities;
using System;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace NoWCF.Models
{
    public class NoWCFSettings
    {
        public int ReceiveBufferMaxSize = 1024 * 8;
        public int ReceivePacketMaxSize = 1024 * 100;
        public int ListenerBacklog = 1000;
        /// <summary>
        /// The certificate (name) which will be used to encrypt the network traffic. By default, the certificate is being searched in Local Machine "Trusted Root" location. Note: in "Server" mode, the certificate specified must have a valid Private key. in "Client" mode, the "Private" key is not mandatory.
        /// </summary>
        public string FindEncryptionCertificateSubjectName;
        /// <summary>
        /// The certificate which will be used to encrypt the network traffic. Note: in "Server" mode, the certificate specified must have a valid Private key. in "Client" mode, the "Private" key is not mandatory.
        /// </summary>
        public X509Certificate2 EncryptionCertificate;
        public Func<Socket> SocketFactory = () => new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Func<ICrypt> CryptorFactory = () => new SimpleCryptor();
        public string BasicCommunicationEncryptionKey;
        public bool PropogateExceptions;

        internal CertificateCryptor CertificateCryptor;
        internal ICrypt GenericCryptor;

        public int ListenerBacklogCount { get; internal set; }

        internal bool UseEncryption => GenericCryptor != null;

        public NoWCFSettings InitializeEncryption()
        {
            if (BasicCommunicationEncryptionKey != null)
                GenericCryptor = CryptorFactory().Initialize(BasicCommunicationEncryptionKey);

            initializeCertificate();

            return this;
        }

        private void initializeCertificate()
        {
            if (EncryptionCertificate == null && string.IsNullOrWhiteSpace(FindEncryptionCertificateSubjectName))
                return;

            if (EncryptionCertificate != null)
            {
                CertificateCryptor = new CertificateCryptor(EncryptionCertificate);
                return;
            }
            
            CertificateCryptor = new CertificateCryptor(StoreName.Root, StoreLocation.LocalMachine, FindEncryptionCertificateSubjectName);
        }
    }
}

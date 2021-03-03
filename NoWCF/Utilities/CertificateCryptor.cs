using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace NoWCF.Utilities
{
    public class CertificateCryptor : IDisposable
    {
        private RSA _algo;

        public CertificateCryptor(StoreName storeName, StoreLocation storeLocation, string certName)
        {
            using (var certificate = CertificateExtensions.Find(storeName, storeLocation, certName))
                Initialize(certificate);
        }

        public CertificateCryptor(X509Certificate2 certificate) => Initialize(certificate);

        public void Initialize(X509Certificate2 certificate) => _algo = certificate.GetRSAPrivateKey() ?? certificate.GetRSAPublicKey();

        public byte[] Decrypt(byte[] data) => _algo.Decrypt(data, RSAEncryptionPadding.OaepSHA512);

        public byte[] Encrypt(byte[] data) => _algo.Encrypt(data, RSAEncryptionPadding.OaepSHA512);

        public void Dispose()
        {
            _algo?.Dispose();
        }
    }
}

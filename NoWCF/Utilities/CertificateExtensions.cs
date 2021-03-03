using System.Security.Cryptography.X509Certificates;

namespace NoWCF.Utilities
{
    public class CertificateExtensions
    {
        public static X509Certificate2 Find(StoreName storeName, StoreLocation storeLocation, string certName)
        {
            using (var store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(X509FindType.FindBySubjectName, certName, true);
                foreach (var cert in certs)
                    return cert;
            }

            return default;
        }
    }
}

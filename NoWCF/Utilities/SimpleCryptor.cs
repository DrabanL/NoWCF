using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NoWCF.Models;

namespace NoWCF.Utilities
{
    public class SimpleCryptor : ICrypt
    {
        private TripleDESCryptoServiceProvider _algo;

        public SimpleCryptor() { }

        public SimpleCryptor(string key) => Initialize(key);

        public ICrypt Initialize(string key)
        {
            _algo?.Dispose();

            using (var hashProvider = new MD5CryptoServiceProvider())
                _algo = new TripleDESCryptoServiceProvider()
                {
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7,
                    Key = hashProvider.ComputeHash(Encoding.UTF8.GetBytes(key))
                };

            return this;
        }

        public byte[] Decrypt(byte[] data)
        {
            using (var cryptor = _algo.CreateDecryptor())
                return cryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public void Dispose()
        {
            _algo?.Dispose();
        }

        public byte[] Encrypt(byte[] data)
        {
            using (var cryptor = _algo.CreateEncryptor())
                return cryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public int GetEncryptedLength(int len) => Encrypt(Enumerable.Repeat((byte)255, len).ToArray()).Length;
    }
}

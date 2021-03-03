using System;

namespace NoWCF.Models
{
    public interface ICrypt : IDisposable
    {
        ICrypt Initialize(string key);
        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] data);
        int GetEncryptedLength(int len);
    }
}

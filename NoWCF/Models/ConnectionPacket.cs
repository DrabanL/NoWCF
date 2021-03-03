using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NoWCF.Models
{
    class ConnectionPacket
    {
        public readonly byte[] Data;
        private int _len;
        private int _offset;

        public ConnectionPacket(int len)
        {
            _len = len;
            Data = new byte[_len];
        }

        public async Task<bool> Read(NetworkStream stream, int maxBufferSize)
        {
            var rem = _len - _offset;
            var len = await stream.ReadAsync(Data, _offset, Math.Min(rem, maxBufferSize));
            if (len == 0)
                throw new Exception("End of stream.");

            _offset += len;

            return _offset == _len;
        }
    }
}

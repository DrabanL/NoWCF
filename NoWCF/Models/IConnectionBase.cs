using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoWCF.Models
{
    public interface IConnectionBase : IDisposable
    {
        void Invoke(string protocol, string method, Dictionary<string, object> parameters);

        Task InvokeAsync(string protocol, string method, Dictionary<string, object> parameters);

        Task<T> InvokeWithResponseAsync<T>(string protocol, string method, Dictionary<string, object> parameters);

        T InvokeWithResponse<T>(string protocol, string method, Dictionary<string, object> parameters);
    }
}

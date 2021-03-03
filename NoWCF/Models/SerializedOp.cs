using System;
using System.Collections.Generic;

namespace NoWCF.Models
{
    public class SerializedOp
    {
        public string ID;
        public string Protocol;
        public string Method;
        public Dictionary<string, object> Parameters;
        public object Result;
        public bool InvokeCompleted;
        public bool WaitResponse;
        public string OperationError;
        public Exception OperationException;

        public bool Error => OperationException != null || OperationError != null;

        public SerializedOp SetResult(object value)
        {
            Parameters = null;
            Method = null;
            Protocol = null;

            Result = value;
            InvokeCompleted = true;

            return this;
        }

        public SerializedOp SetError(Exception exception)
        {
            Parameters = null;
            Method = null;
            Protocol = null;

            OperationException = exception;
            InvokeCompleted = true;

            return this;
        }

        public SerializedOp SetError(string message)
        {
            Parameters = null;
            Method = null;
            Protocol = null;

            OperationError = message;
            InvokeCompleted = true;

            return this;
        }
    }
}

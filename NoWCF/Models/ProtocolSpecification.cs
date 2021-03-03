using System;

namespace NoWCF.Models
{
    public class ProtocolSpecification
    {
        public readonly Type ImplementationType;
        public readonly object ImplementationObject;
        public readonly IInvokeProtocol InvokeObject;

        //public ProtocolContainer(ConnectionBase connection, Type implType, Type objType)
        //{
        //    ImplementationType = implType;
        //    ImplementationObject = Activator.CreateInstance(objType, connection);
        //    InvokeObject =  (IInvokeProtocol)ImplementationObject;
        //}

        public ProtocolSpecification(ConnectionBase connection, Type implType, Type invokeType)
        {
            ImplementationType = implType;
            InvokeObject = (IInvokeProtocol)Activator.CreateInstance(invokeType, connection);
            ImplementationObject = Activator.CreateInstance(ImplementationType, InvokeObject);
        }

        public ProtocolSpecification(Type implType)
        {
            ImplementationType = implType;
            ImplementationObject = Activator.CreateInstance(ImplementationType);
        }

        public ProtocolSpecification(ConnectionBase connection, Type invokeType)
        {
            InvokeObject = (IInvokeProtocol)Activator.CreateInstance(invokeType, connection);
        }
    }
}

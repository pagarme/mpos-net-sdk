using System;
using Type = PagarMe.Mpos.Bridge.Commands.PaymentRequest.Type;

namespace PagarMe.Mpos.Bridge.Commands
{
    public static class TypeExtension
    {
        internal static Type[] GetNextAllowed(this Type type)
        {
            switch (type)
            {
                case Type.UnknownCommand:
                case Type.Close:
                    return new[] { Type.Initialize };

                case Type.Initialize:
                    return new[] { Type.Process };

                case Type.Process:
                    return new[] { Type.Finish, Type.Close, Type.Initialize };

                case Type.Finish:
                    return new[] { Type.Process, Type.Close, Type.Initialize };

                default:
                    throw new NotImplementedException();
            }

        }
    }
}
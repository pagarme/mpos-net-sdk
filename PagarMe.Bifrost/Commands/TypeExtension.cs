using System;
using Type = PagarMe.Bifrost.Commands.PaymentRequest.Type;

namespace PagarMe.Bifrost.Commands
{
    public static class TypeExtension
    {
        internal static Type[] GetNextAllowed(this Type type)
        {
            switch (type)
            {
                case Type.UnknownCommand:
                case Type.CloseContext:
                    return new[] { Type.Initialize };

                case Type.Initialize:
                    return new[] { Type.Process };

                case Type.Process:
                    return new[] { Type.Finish, Type.CloseContext, Type.Initialize };

                case Type.Finish:
                    return new[] { Type.Process, Type.CloseContext, Type.Initialize };

                default:
                    throw new NotImplementedException();
            }

        }
    }
}
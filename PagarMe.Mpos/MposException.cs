using System;

namespace PagarMe.Mpos
{
    public class MposException : Exception
    {
        internal MposException(Mpos.Error error)
            : base("An error ocurred: " + error)
        {
            
        }
    }
}


using System;
using PagarMe.Mpos.Entities;

namespace PagarMe.Mpos
{
    public class MposException : Exception
    {
        internal MposException(Error error)
            : base("An error ocurred: " + error)
        {
        }
    }
}

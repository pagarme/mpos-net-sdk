using System;

namespace PagarMe.Mpos
{
    public enum PaymentFlags
    {
        None       = 0x0,

        CreditCard = 0x1,
        DebitCard  = 0x2,

        AllCards   = CreditCard | DebitCard,
        Default    = AllCards
    }
}


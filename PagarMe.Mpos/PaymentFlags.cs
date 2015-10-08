using System;

namespace PagarMe.Mpos
{
    public enum PaymentFlags
    {
        None       = 0x0,

        CreditCard = 0x1,
        DebitCard  = 0x2,
        AllApplications = CreditCard | DebitCard,

        Visa = 0x4,
        MasterCard = 0x8,
        AllBrands = Visa | MasterCard,

        Default = AllBrands | AllApplications
    }
}


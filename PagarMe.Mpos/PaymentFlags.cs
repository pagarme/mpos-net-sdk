using System;

namespace PagarMe.Mpos
{
    [Flags]
    public enum PaymentFlags
    {
        None = 0x0,

        CreditCard = 0x1,
        DebitCard = 0x2,
        AllApplications = CreditCard | DebitCard,

        VisaCard = 0x4,
        MasterCard = 0x8,
        AllCards = VisaCard | MasterCard,

        AllVisa = AllApplications | VisaCard,
        AllMaster = AllApplications | MasterCard,

        AllCredit = AllCards | CreditCard,
        AllDebit = AllCards | DebitCard,

        Default = AllCards | AllApplications
    }
}
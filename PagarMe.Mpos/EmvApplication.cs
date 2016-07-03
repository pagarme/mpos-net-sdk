using System;

namespace PagarMe.Mpos
{
	public enum EmvApplication
	{
		/* Stone */
		VisaCredit = 2,
		VisaDebit = 3,
		MasterCredit = 4,
		MasterDebit = 5,
		Cirrus = 6,

		/* Pagar.me */
		AmexCredit = 20,
		EloCredit = 21,
		DinersCredit = 22
	}
}


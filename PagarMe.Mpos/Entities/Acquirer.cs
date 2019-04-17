using System.Runtime.InteropServices;
using PagarMe.Mpos.Tms;
using static PagarMe.Mpos.Natives.Native;

namespace PagarMe.Mpos.Entities
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Acquirer
	{
		public int Number;
		public int CryptographyMethod;
		public int KeyIndex;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] SessionKey;

		public Acquirer(AcquirerEntry e)
		{
			Number = e.Number;
			CryptographyMethod = e.CryptographyMethod;
			KeyIndex = e.KeyIndex;

			if (e.SessionKey != null) SessionKey = GetHexBytes(e.SessionKey, 32);
			else SessionKey = GetHexBytes("", 32);
		}
	}
}
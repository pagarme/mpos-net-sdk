using System.Runtime.InteropServices;
using PagarMe.Mpos.Tms;

namespace PagarMe.Mpos.Entities
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct RiskManagement
	{
		public int AcquirerNumber;
		public int RecordNumber;

		[MarshalAs(UnmanagedType.I1)] public bool MustRiskManagement;
		public int FloorLimit;
		public int BiasedRandomSelectionPercentage;
		public int BiasedRandomSelectionThreshold;
		public int BiasedRandomSelectionMaxPercentage;

		public RiskManagement(RiskManagementEntry e)
		{
			AcquirerNumber = e.AcquirerNumber;
			RecordNumber = e.RecordNumber;

			MustRiskManagement = e.MustRiskManagement;
			FloorLimit = e.FloorLimit;
			BiasedRandomSelectionPercentage = e.BiasedRandomSelectionPercentage;
			BiasedRandomSelectionThreshold = e.BiasedRandomSelectionThreshold;
			BiasedRandomSelectionMaxPercentage = e.BiasedRandomSelectionMaxPercentage;
		}
	}
}
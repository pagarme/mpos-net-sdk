using System.Linq;
using System.Runtime.InteropServices;
using PagarMe.Mpos.Tms;

namespace PagarMe.Mpos.Entities
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Application
    {
        public int PaymentMethod;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)] public string CardBrand;

        public int AcquirerNumber;
        public int RecordNumber;

        public int EmvTagsLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public int[] EmvTags;

        public Application(ApplicationEntry e)
        {
            PaymentMethod = e.PaymentMethod;
            CardBrand = e.CardBrand;

            AcquirerNumber = e.AcquirerNumber;
            RecordNumber = e.RecordNumber;

            EmvTags = new int[256];
            var tags = e.EmvTags.Split(',').Select(int.Parse).ToArray();
            for (var i = 0; i < tags.Length; i++)
                EmvTags[i] = tags[i];
            EmvTagsLength = tags.Length;
        }
    }
}

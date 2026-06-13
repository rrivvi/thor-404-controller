
using HidSharp;

namespace Thor404Controller
{
    partial class Usb
    {
        public const int VENDOR_ID = 0x331A;
        public const int PRODUCT_ID = 0x501C;

        public static readonly string[] HANDSHAKE_PACKETS = {
            "04020000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
            "04190000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
            "04130000000000001200000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"
        };

        public static readonly string[] COMMON_PREFIX_PACKETS = {
            "01ff00000000000000100c000000aa55020000ff0000000001100c000000aa55030000ff0000000001100c000000aa55040000ff0000000001100c000000aa55",
            "050000ff0000000001100c000000aa55060000ff0000000000100c000000aa55075affff0000000000100c000000aa55080000ff0000000000100c000000aa55",
            "090000ff0000000001100c030000aa550a0000ff0000000001100c030000aa550b0000ff0000000001100c000000aa550c0000ff0000000001100c000000aa55",
            "0d0000ff0000000001100c000000aa550e0000ff0000000001100c000000aa550f0000ff0000000001100c000000aa55100000ff0000000001100c000000aa55",
            "110000ff0000000001100c000000aa55120000ff0000000001100c000000aa55130000ff0000000001100c000000aa558000000000000000001000000000aa55",
            new string('0', 128),
            new string('0', 128),
            new string('0', 128),
        };

        // custom mode per key
        public static readonly string[] COMMON_FRAME_PACKETS = {
            string.Concat(Enumerable.Repeat("80000000", 16)),
            string.Concat(Enumerable.Repeat("80000000", 16)),
            string.Concat(Enumerable.Repeat("80000000", 16)),
            string.Concat(Enumerable.Repeat("80000000", 16)),
            string.Concat(Enumerable.Repeat("80000000", 16)),
            string.Concat(Enumerable.Repeat("80000000", 16)),
            string.Concat(Enumerable.Repeat("80000000", 16)),
            string.Concat(Enumerable.Repeat("80000000", 16)),
            string.Concat(Enumerable.Repeat("80000000", 16)),
        };

        public static readonly string[] FINALIZE_PACKETS = {
            "04020000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
            "04f00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
        };

        internal static HidDevice? TryGetHidDevice()
        {
            try
            {
                return DeviceList.Local.GetHidDevices(VENDOR_ID, PRODUCT_ID)
                    .FirstOrDefault(d => d.GetMaxFeatureReportLength() == 65); // 64 + 1, otherwise it sometimes selects the wrong device
            }
            catch
            {
                return null;
            }
        }
    }
}
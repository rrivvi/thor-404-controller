namespace Thor404Controller
{
    partial class Helpers
    {
        public static string RgbaToHex(Gdk.RGBA rgba)
        {
            var r = (byte)Math.Round(rgba.Red * 255);
            var g = (byte)Math.Round(rgba.Green * 255);
            var b = (byte)Math.Round(rgba.Blue * 255);

            return $"{r:X2}{g:X2}{b:X2}";
        }
    }
}
using HidSharp;
using Newtonsoft.Json.Linq;

namespace Thor404Controller
{
    partial class Packet
    {
        public static string BuildEffectPacket(
            Effects.EffectsEnum effectId,
            string rgb,
            bool multicolor,
            int brightness,
            int speed,
            Effects.DirectionEnum direction)
        {
            brightness = Math.Clamp(brightness, 1, 16);
            speed = Math.Clamp(speed, 1, 16);

            byte r = Convert.ToByte(rgb.Substring(0, 2), 16);
            byte g = Convert.ToByte(rgb.Substring(2, 2), 16);
            byte b = Convert.ToByte(rgb.Substring(4, 2), 16);

            byte[] packet = new byte[64];

            packet[0] = (byte)effectId;
            packet[1] = r;
            packet[2] = g;
            packet[3] = b;

            packet[8] = multicolor ? (byte)0x01 : (byte)0x00;
            packet[9] = (byte)brightness;
            packet[10] = (byte)speed;
            packet[11] = (byte)direction;

            packet[14] = 0xAA;
            packet[15] = 0x55;

            return Convert.ToHexString(packet).ToLower();
        }

        // constructs the repeating 80RRGGBB per-key hex
        public static List<string> BuildCustomFramePackets(
        JObject keymap,
        Dictionary<string, string> keyColors)
        {
            // in case there's no entry in the dictionary, don't light up the key (black)
            string fallbackRgb = "000000";

            List<byte[]> rows = Enumerable.Range(0, 9)
                .Select(_ => Enumerable.Repeat((byte)0x80, 1)
                    .Concat(new byte[3])
                    .ToArray())
                .Select(block =>
                {
                    byte[] row = new byte[64];

                    for (int i = 0; i < 16; i++)
                    {
                        Array.Copy(block, 0, row, i * 4, 4);
                    }

                    return row;
                })
                .ToList();

            byte[] PackFor(string keyName)
            {
                string rgb = ValidateRgb(
                    keyColors.TryGetValue(keyName, out var value)
                        ? value
                        : fallbackRgb
                );

                return Convert.FromHexString("80" + rgb);
            }

            JObject keys = (JObject)keymap["keys"]!;

            foreach (var key in keys)
            {
                string keyName = key.Key;
                JObject info = (JObject)key.Value!;

                List<JToken> blocks = new();

                if (info["capture"] != null)
                    blocks.Add(info["capture"]!);

                if (info["extra_dwords"] is JArray extras)
                    blocks.AddRange(extras);

                foreach (JObject block in blocks)
                {
                    int? line = block["line"]?.Value<int>();
                    int? dword = block["dword"]?.Value<int>();

                    if (line == null || dword == null)
                        continue;

                    int rowIndex = line.Value - 12;

                    if (rowIndex >= 0 && rowIndex < 8 &&
                        dword.Value >= 0 && dword.Value < 16)
                    {
                        byte[] packed = PackFor(keyName);

                        Array.Copy(
                            packed,
                            0,
                            rows[rowIndex],
                            dword.Value * 4,
                            4
                        );
                    }
                }
            }

            return rows.Select(row => Convert.ToHexString(row).ToLower()).ToList();
        }

        private static string ValidateRgb(string rgb)
        {
            if (string.IsNullOrWhiteSpace(rgb))
                throw new ArgumentException("ValidateRgb EMPTY VALUE.");

            rgb = rgb.Trim().TrimStart('#');

            if (rgb.Length != 6 || !System.Text.RegularExpressions.Regex.IsMatch(rgb, "^[0-9A-Fa-f]{6}$"))
                throw new ArgumentException($"ValidateRgb BAD VALUE: {rgb}");

            return rgb.ToUpper();
        }

        public static void SendPacket(HidStream stream, string payloadHex)
        {
            byte[] payload = Convert.FromHexString(payloadHex);

            // 64 + 1
            byte[] buffer = new byte[payload.Length + 1];

            buffer[0] = 0x00;
            Array.Copy(payload, 0, buffer, 1, payload.Length);

            // Console.WriteLine("max feature report len: " + stream.Device.GetMaxFeatureReportLength());
            // Console.WriteLine(payloadHex);
            // Console.WriteLine(buffer.Length);
            // Console.WriteLine();

            stream.SetFeature(buffer);

            try
            {
                byte[] response = new byte[65];
                stream.GetFeature(response);
            }
            catch
            {
            }
        }
    };
}
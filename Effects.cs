using HidSharp;

namespace Thor404Controller
{
    partial class Effects
    {
        public static void ApplyEffect(
           EffectsEnum effect,
           string rgb,
           bool multicolor,
           int brightness,
           int speed,
           DirectionEnum direction)
        {
            HidDevice? device = Usb.TryGetHidDevice();

            if (device == null)
            {
                throw new Exception("keyboard not found");
            }

            bool isCustomPerKey = effect == EffectsEnum.Custom;

            using (HidStream stream = device.Open())
            {

                List<string> packets = new();

                packets.AddRange(Usb.HANDSHAKE_PACKETS);
                packets.AddRange(Usb.COMMON_PREFIX_PACKETS);

                if (isCustomPerKey)
                {
                    packets.AddRange(
                    Packet.BuildCustomFramePackets(
                        Program.keymapJObject,
                        Program.customColorsPairs
                    ));
                }
                else
                {
                    packets.AddRange(Usb.COMMON_FRAME_PACKETS);
                }

                packets.Add(
                    Packet.BuildEffectPacket(
                        effect,
                        rgb,
                        multicolor,
                        brightness,
                        speed,
                        direction
                    )
                );

                packets.AddRange(Usb.FINALIZE_PACKETS);

                foreach (string packet in packets)
                {
                    Packet.SendPacket(stream, packet);
                    Thread.Sleep(10);
                }
            }
        }

        public enum EffectsEnum
        {
            // 0x00 is not implemented in the original software, but 0x00 just disables the keyboard's lighting
            Disabled = 0x00,
            Static = 0x01,
            SingleOn = 0x02,
            SingleOff = 0x03,
            Stars = 0x04,
            Raindrops = 0x05,
            Colourful = 0x06,
            Breathing = 0x07,
            Neon = 0x08,
            Spectrum = 0x09,
            Rainbow = 0x0A,
            Prismo = 0x0B,
            Circle = 0x0C,
            EscapeBeam = 0x0D,
            Shockwave = 0x0E,
            Explosion = 0x0F,
            Escape = 0x10,
            SineWave = 0x11,
            Wave = 0x12,
            Shuttle = 0x13,
            Custom = 0x14,
        }

        // fun fact: (0x00, 0x03) and (0x01, 0x02) seem to have the same behaviour
        public enum DirectionEnum : byte
        {
            Right = 0x00,
            Left = 0x01,
            Up = 0x02,
            Down = 0x03,
        }
    }
}
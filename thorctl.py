#!/usr/bin/env python3

import argparse
import sys
import time

import usb.core
import usb.util


VENDOR_ID = 0x331A
PRODUCT_ID = 0x501C


HANDSHAKE_PACKETS = [
    "04020000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
    "04190000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
    "04130000000000001200000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
]


COMMON_PACKETS = [
    "01ff00000000000000100c000000aa55020000ff0000000001100c000000aa55030000ff0000000001100c000000aa55040000ff0000000001100c000000aa55",
    "050000ff0000000001100c000000aa55060000ff0000000000100c000000aa55075affff0000000000100c000000aa55080000ff0000000000100c000000aa55",
    "090000ff0000000001100c000000aa550a0000ff0000000001100c030000aa550b0000ff0000000001100c000000aa550c0000ff0000000001100c000000aa55",
    "0d0000ff0000000001100c000000aa550e0000ff0000000001100c000000aa550f0000ff0000000001100c000000aa55100000ff0000000001100c000000aa55",
    "110000ff0000000001100c000000aa55120000ff0000000001100c000000aa55130000ff0000000001100c000000aa558000000000000000001000000000aa55",

    "00" * 64,
    "00" * 64,
    "00" * 64,

    "80000000" * 16,
    "80000000" * 16,
    "80000000" * 16,
    "80000000" * 16,
    "80000000" * 16,
    "80000000" * 16,
    "80000000" * 16,
    "80000000" * 16,
    "80000000" * 16,
]


FINALIZE_PACKETS = [
    "04020000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
    "04f00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
]


#
# Effect IDs discovered from captures
#
EFFECTS = {
    "static":            0x01,
    "single_on":         0x02,
    "single_off":        0x03,
    "stars":             0x04,
    "raindrops":         0x05,
    "colourful":         0x06,
    "breathing":         0x07,
    "neon":              0x08,
    "spectrum":          0x09,
    "rainbow":           0x0A,
    "prismo":            0x0B,
    "circle":            0x0C,
    "escape_beam":       0x0D,
    "shockwave":         0x0E,
    "explosion":         0x0F,
    "escape":            0x10,
    "sine_wave":         0x11,
    "wave":              0x12,
    "shuttle":           0x13,
}


DIRECTIONS = {
    "none":  0x00,
    "left":  0x00,
    "right": 0x01,
    "up":    0x02,
    "down":  0x03,
}


def clamp(v, lo, hi):
    return max(lo, min(hi, v))


def validate_rgb(rgb):
    rgb = rgb.lower().strip().replace("#", "")

    if len(rgb) != 6:
        raise ValueError("RGB must be 6 hex chars")

    int(rgb, 16)
    return rgb


def build_effect_packet(effect_id, rgb, multicolor, brightness, speed, direction):
    """
    Packet layout discovered:

    byte 0  : effect id
    byte 1-3: RGB
    byte 8  : multicolor flag
    byte 9  : brightness
    byte 10 : speed
    byte 11 : direction
    """

    brightness = clamp(brightness, 1, 16)
    speed = clamp(speed, 1, 16)

    r = int(rgb[0:2], 16)
    g = int(rgb[2:4], 16)
    b = int(rgb[4:6], 16)

    packet = bytearray(64)

    packet[0] = effect_id
    packet[1] = r
    packet[2] = g
    packet[3] = b

    packet[8] = 0x01 if multicolor else 0x00
    packet[9] = brightness
    packet[10] = speed
    packet[11] = direction

    packet[14] = 0xAA
    packet[15] = 0x55

    return packet.hex()


def find_keyboard_interface(dev):
    cfg = dev.get_active_configuration()

    for intf in cfg:
        if (
            intf.bInterfaceClass == 0x03 and
            intf.bInterfaceSubClass == 0x01 and
            intf.bInterfaceProtocol == 0x01
        ):
            return intf.bInterfaceNumber

    raise RuntimeError("keyboard HID interface not found")


def send_packet(dev, intf, payload_hex):
    payload = bytes.fromhex(payload_hex)

    dev.ctrl_transfer(
        0x21,
        0x09,
        0x0300,
        intf,
        payload,
        timeout=1000,
    )

    try:
        dev.ctrl_transfer(
            0xA1,
            0x01,
            0x0300,
            intf,
            64,
            timeout=1000,
        )
    except Exception:
        pass


def apply_effect(
    effect,
    rgb,
    multicolor,
    brightness,
    speed,
    direction,
    verbose=False,
):
    dev = usb.core.find(
        idVendor=VENDOR_ID,
        idProduct=PRODUCT_ID,
    )

    if dev is None:
        raise SystemExit("device not found")

    intf = find_keyboard_interface(dev)

    detached = False

    try:
        if dev.is_kernel_driver_active(intf):
            dev.detach_kernel_driver(intf)
            detached = True
    except Exception:
        pass

    usb.util.claim_interface(dev, intf)

    try:
        packets = []

        packets.extend(HANDSHAKE_PACKETS)
        packets.extend(COMMON_PACKETS)

        effect_packet = build_effect_packet(
            effect_id=EFFECTS[effect],
            rgb=rgb,
            multicolor=multicolor,
            brightness=brightness,
            speed=speed,
            direction=DIRECTIONS[direction],
        )

        packets.append(effect_packet)
        packets.extend(FINALIZE_PACKETS)

        for i, pkt in enumerate(packets, 1):
            if verbose:
                print(f"[{i}/{len(packets)}] {pkt}")

            send_packet(dev, intf, pkt)
            time.sleep(0.01)

    finally:
        usb.util.release_interface(dev, intf)

        if detached:
            try:
                dev.attach_kernel_driver(intf)
            except Exception:
                pass


def print_effects():
    print("Available effects:\n")

    for name in EFFECTS:
        print(f"  {name}")


def tui():
    print("Thor RGB Controller")
    print()

    effects = list(EFFECTS.keys())

    for i, fx in enumerate(effects, 1):
        print(f"{i:2d}. {fx}")

    print()

    idx = int(input("effect number: ").strip())
    effect = effects[idx - 1]

    rgb = input("rgb hex [ff0000]: ").strip() or "ff0000"
    rgb = validate_rgb(rgb)

    brightness = int(input("brightness 1-16 [16]: ").strip() or "16")
    speed = int(input("speed 1-16 [12]: ").strip() or "12")

    multicolor = (
        input("multicolor y/n [n]: ").strip().lower() == "y"
    )

    direction = (
        input("direction none/left/right/up/down [none]: ")
        .strip()
        .lower()
        or "none"
    )

    apply_effect(
        effect=effect,
        rgb=rgb,
        multicolor=multicolor,
        brightness=brightness,
        speed=speed,
        direction=direction,
        verbose=True,
    )

    print()
    print("done")


def main():
    parser = argparse.ArgumentParser(
        description="Thor keyboard RGB controller",
    )

    parser.add_argument(
        "--effect",
        default="static",
        choices=sorted(EFFECTS.keys()),
    )

    parser.add_argument(
        "--rgb",
        default="ff0000",
        help="RRGGBB",
    )

    parser.add_argument(
        "--brightness",
        type=int,
        default=16,
    )

    parser.add_argument(
        "--speed",
        type=int,
        default=12,
    )

    parser.add_argument(
        "--multicolor",
        action="store_true",
    )

    parser.add_argument(
        "--direction",
        default="none",
        choices=["none", "left", "right", "up", "down"],
    )

    parser.add_argument(
        "--list",
        action="store_true",
    )

    parser.add_argument(
        "--tui",
        action="store_true",
    )

    parser.add_argument(
        "--verbose",
        action="store_true",
    )

    args = parser.parse_args()

    if args.list:
        print_effects()
        return

    if args.tui:
        tui()
        return

    try:
        rgb = validate_rgb(args.rgb)
    except Exception as e:
        raise SystemExit(str(e))

    apply_effect(
        effect=args.effect,
        rgb=rgb,
        multicolor=args.multicolor,
        brightness=args.brightness,
        speed=args.speed,
        direction=args.direction,
        verbose=args.verbose,
    )


if __name__ == "__main__":
    main()

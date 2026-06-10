# Genesis Thor 404 TKL RGB Controller for Linux
Control RGB lighting on the Genesis Thor 404 TKL keyboard under Linux.

Supports per-key "custom" mode, custom colour, presets, brightness adjustment & speed control.

> main win

Custom mode:
> per key win

---

### usage
1. download latest release

2. open terminal in the directory

3. use the gui with sudo (without sudo, HidSharp may fail)
```
sudo ./Thor404Controller
```

---

### technical
the main packet's offsets:
| Offset | Meaning           |
| ------ | ----------------- |
| 0x00   | effect ID         |
| 0x01   | red               |
| 0x02   | green             |
| 0x03   | blue              |
| 0x08   | multicolor toggle |
| 0x09   | brightness        |
| 0x0A   | speed             |
| 0x0B   | direction         |
| 0x0E   | 0xAA              |
| 0x0F   | 0x55              |

> **example `120000ff00000000011002010000aa55`**
> | Field      | Value            |
> | ---------- | ---------------- |
> | effect     | `0x12` = wave    |
> | RGB        | `0000ff` = blue  |
> | multicolor | `0x01` = enabled |
> | brightness | `0x10` = 16      |
> | speed      | `0x02` = 2       |
> | direction  | `0x01` = left   |

### notes

> [!tip]
> ### Resources
> previous research (offsets, notes, etc.) can be found in the [Resources](https://github.com/rrivvi/thor-404-linux-rgb-script/tree/resources) branch 

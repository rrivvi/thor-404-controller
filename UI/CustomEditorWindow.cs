using Gtk;

namespace Thor404Controller.UI
{
    internal static class CustomEditorWindow
    {
        private readonly record struct KeyDef(string Name, string Label, int Width);

        public static ApplicationWindow CreateWindow(Application app, Window? parent = null)
        {
            ApplyCss();

            var window = ApplicationWindow.New(app);
            window.SetTitle("Thor 404: Custom Configuration");
            window.SetDefaultSize(914, 480);
            window.SetResizable(false);

            if (parent != null)
            {
                window.SetTransientFor(parent);
                window.SetModal(true);
            }

            var colors = new Dictionary<string, string>(Program.customColorsPairs);
            var buttons = new Dictionary<string, ToggleButton>();

            var root = Box.New(Orientation.Vertical, 12);
            root.SetMarginTop(12);
            root.SetMarginBottom(12);
            root.SetMarginStart(12);
            root.SetMarginEnd(12);

            var topRow = Box.New(Orientation.Horizontal, 10);

            var colorLabel = Label.New("Color");
            colorLabel.SetSizeRequest(70, -1);
            colorLabel.SetXalign(0);

            var colorDialog = ColorDialog.New();
            colorDialog.SetTitle("Choose a key color");
            colorDialog.SetWithAlpha(false);

            var colorButton = ColorDialogButton.New(colorDialog);

            topRow.Append(colorLabel);
            topRow.Append(colorButton);

            var actionRow = Box.New(Orientation.Horizontal, 8);

            var applySelection = Button.NewWithLabel("Apply to selection");
            applySelection.OnClicked += (_, _) => ApplyToActiveButtons(buttons, colors, colorButton);

            var selectAll = Button.NewWithLabel("Select All");
            selectAll.OnClicked += (_, _) => SetAllButtons(buttons, true);

            var clearSelection = Button.NewWithLabel("Clear Selection");
            clearSelection.OnClicked += (_, _) => SetAllButtons(buttons, false);

            var fillAll = Button.NewWithLabel("Fill All");
            fillAll.OnClicked += (_, _) =>
            {
                var rgb = RgbaToHex(colorButton.GetRgba());
                foreach (var (keyName, button) in buttons)
                {
                    UpdateKeyColor(colors, button, keyName, rgb);
                }
            };

            actionRow.Append(applySelection);
            actionRow.Append(selectAll);
            actionRow.Append(clearSelection);
            actionRow.Append(fillAll);

            var scroller = ScrolledWindow.New();
            scroller.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroller.SetVexpand(true);
            scroller.SetHexpand(true);

            var keyboard = Box.New(Orientation.Horizontal, 8);
            keyboard.SetMarginTop(6);
            keyboard.SetMarginStart(6);
            keyboard.SetMarginEnd(6);
            keyboard.AddCssClass("keyboard");

            var left = Box.New(Orientation.Vertical, 18);
            left.SetSpacing(4);

            var right = Box.New(Orientation.Vertical, 18);
            right.SetMarginStart(8);
            right.SetSpacing(4);

            left.Append(Row(
                Key(buttons, colors, K("escape", "ESC", 45)),
                EmptyKey(),
                Key(buttons, colors, K("f1", "F1", 45)),
                Key(buttons, colors, K("f2", "F2", 45)),
                Key(buttons, colors, K("f3", "F3", 45)),
                Key(buttons, colors, K("f4", "F4", 45)),
                EmptyKey(22),
                Key(buttons, colors, K("f5", "F5", 45)),
                Key(buttons, colors, K("f6", "F6", 45)),
                Key(buttons, colors, K("f7", "F7", 45)),
                Key(buttons, colors, K("f8", "F8", 45)),
                EmptyKey(22),
                Key(buttons, colors, K("f9", "F9", 45)),
                Key(buttons, colors, K("f10", "F10", 45)),
                Key(buttons, colors, K("f11", "F11", 45)),
                Key(buttons, colors, K("f12", "F12", 45))
            ));

            left.Append(Row(
                Key(buttons, colors, K("backtick", "`", 45)),
                Key(buttons, colors, K("1", "1", 45)),
                Key(buttons, colors, K("2", "2", 45)),
                Key(buttons, colors, K("3", "3", 45)),
                Key(buttons, colors, K("4", "4", 45)),
                Key(buttons, colors, K("5", "5", 45)),
                Key(buttons, colors, K("6", "6", 45)),
                Key(buttons, colors, K("7", "7", 45)),
                Key(buttons, colors, K("8", "8", 45)),
                Key(buttons, colors, K("9", "9", 45)),
                Key(buttons, colors, K("0", "0", 45)),
                Key(buttons, colors, K("-", "-", 45)),
                Key(buttons, colors, K("=", "=", 45)),
                Key(buttons, colors, K("backspace", "Back", 93))
            ));

            left.Append(Row(
                Key(buttons, colors, K("tab", "Tab", 69)),
                Key(buttons, colors, K("q", "Q", 45)),
                Key(buttons, colors, K("w", "W", 45)),
                Key(buttons, colors, K("e", "E", 45)),
                Key(buttons, colors, K("r", "R", 45)),
                Key(buttons, colors, K("t", "T", 45)),
                Key(buttons, colors, K("y", "Y", 45)),
                Key(buttons, colors, K("u", "U", 45)),
                Key(buttons, colors, K("i", "I", 45)),
                Key(buttons, colors, K("o", "O", 45)),
                Key(buttons, colors, K("p", "P", 45)),
                Key(buttons, colors, K("[", "[", 45)),
                Key(buttons, colors, K("]", "]", 45)),
                Key(buttons, colors, K("reverse slash", "\\", 69))
            ));

            left.Append(Row(
                Key(buttons, colors, K("caps lock", "CapsLk", 82)),
                Key(buttons, colors, K("a", "A", 45)),
                Key(buttons, colors, K("s", "S", 45)),
                Key(buttons, colors, K("d", "D", 45)),
                Key(buttons, colors, K("f", "F", 45)),
                Key(buttons, colors, K("g", "G", 45)),
                Key(buttons, colors, K("h", "H", 45)),
                Key(buttons, colors, K("j", "J", 45)),
                Key(buttons, colors, K("k", "K", 45)),
                Key(buttons, colors, K("l", "L", 45)),
                Key(buttons, colors, K(";", ";", 45)),
                Key(buttons, colors, K("'", "'", 45)),
                Key(buttons, colors, K("enter", "Enter", 103))
            ));

            left.Append(Row(
                Key(buttons, colors, K("left shift", "Shift", 103)),
                Key(buttons, colors, K("z", "Z", 45)),
                Key(buttons, colors, K("x", "X", 45)),
                Key(buttons, colors, K("c", "C", 45)),
                Key(buttons, colors, K("v", "V", 45)),
                Key(buttons, colors, K("b", "B", 45)),
                Key(buttons, colors, K("n", "N", 45)),
                Key(buttons, colors, K("m", "M", 45)),
                Key(buttons, colors, K(",", ",", 45)),
                Key(buttons, colors, K(".", ".", 45)),
                Key(buttons, colors, K("/", "/", 45)),
                Key(buttons, colors, K("right shift", "Shift", 129))
            ));

            left.Append(Row(
                Key(buttons, colors, K("left ctrl", "Ctrl", 54)),
                Key(buttons, colors, K("windows", "Win", 54)),
                Key(buttons, colors, K("left alt", "Alt", 54)),
                Key(buttons, colors, K("space", "Space", 312)),
                Key(buttons, colors, K("right alt", "Alt", 54)),
                Key(buttons, colors, K("function", "Fn", 54)),
                Key(buttons, colors, K("menu", "Menu", 54)),
                Key(buttons, colors, K("right ctrl", "Ctrl", 54))
            ));

            right.Append(Row(
                Key(buttons, colors, K("prtsc", "PrtSc", 45)),
                Key(buttons, colors, K("scrllk", "ScrLk", 45)),
                Key(buttons, colors, K("pause", "Pause", 45))
            ));

            right.Append(Row(
                Key(buttons, colors, K("insert", "Ins", 45)),
                Key(buttons, colors, K("home", "Home", 45)),
                Key(buttons, colors, K("page up", "PgUp", 45))
            ));

            right.Append(Row(
                Key(buttons, colors, K("del", "Del", 45)),
                Key(buttons, colors, K("end", "End", 45)),
                Key(buttons, colors, K("page down", "PgDn", 45))
            ));

            right.Append(Row(
                EmptyKey(),
                EmptyKey(),
                EmptyKey()
            ));

            right.Append(Row(
                EmptyKey(),
                Key(buttons, colors, K("up arrow", "Up", 45)),
                EmptyKey()
            ));

            right.Append(Row(
                Key(buttons, colors, K("left arrow", "Left", 45)),
                Key(buttons, colors, K("down arrow", "Down", 45)),
                Key(buttons, colors, K("right arrow", "Right", 45))
            ));

            keyboard.Append(left);
            keyboard.Append(right);
            scroller.SetChild(keyboard);

            var bottomRow = Box.New(Orientation.Horizontal, 8);

            var cancelButton = Button.NewWithLabel("Cancel");
            cancelButton.OnClicked += (_, _) => window.Close();

            var saveButton = Button.NewWithLabel("Save");
            saveButton.OnClicked += (_, _) =>
            {
                Program.customColorsPairs = new Dictionary<string, string>(colors);
                window.Close();
            };

            bottomRow.Append(cancelButton);
            bottomRow.Append(saveButton);

            root.Append(topRow);
            root.Append(actionRow);
            root.Append(scroller);
            root.Append(bottomRow);

            window.SetChild(root);
            return window;
        }

        private static KeyDef K(string name, string label, int width) => new(name, label, width);

        private static Box Row(params Widget[] children)
        {
            var row = Box.New(Orientation.Horizontal, 2);

            foreach (var child in children)
                row.Append(child);

            return row;
        }

        private static ToggleButton Key(
            Dictionary<string, ToggleButton> buttons,
            Dictionary<string, string> colors,
            KeyDef key)
        {
            var rgb = colors.TryGetValue(key.Name, out var existing) ? existing : "000000";

            var button = ToggleButton.NewWithLabel(key.Label);
            button.AddCssClass("key-tile");
            button.SetSizeRequest(key.Width, 45);
            button.SetHexpand(false);
            button.SetVexpand(false);
            button.SetTooltipText($"{key.Name}\n#{rgb}");

            ApplyButtonColor(button, rgb);

            buttons[key.Name] = button;
            return button;
        }

        private static Widget EmptyKey(int width = 45, int height = 45)
        {
            var box = Box.New(Orientation.Horizontal, 0);
            box.SetSizeRequest(width, height);
            box.SetHexpand(false);
            box.SetVexpand(false);
            return box;
        }

        private static void SetAllButtons(Dictionary<string, ToggleButton> buttons, bool active)
        {
            foreach (var button in buttons.Values)
                button.SetActive(active);
        }

        private static void ApplyToActiveButtons(
            Dictionary<string, ToggleButton> buttons,
            Dictionary<string, string> colors,
            ColorDialogButton colorButton)
        {
            var rgb = RgbaToHex(colorButton.GetRgba());

            foreach (var (keyName, button) in buttons)
            {
                if (button.GetActive())
                    UpdateKeyColor(colors, button, keyName, rgb);
            }
        }

        private static void UpdateKeyColor(
            Dictionary<string, string> colors,
            ToggleButton button,
            string keyName,
            string rgb)
        {
            colors[keyName] = rgb;
            ApplyButtonColor(button, rgb);
            button.SetTooltipText($"{keyName}\n#{rgb}");
        }

        private static void ApplyButtonColor(ToggleButton button, string rgb)
        {
            var className = $"key-{button.GetHashCode():X}";
            button.AddCssClass(className);

            var provider = CssProvider.New();
            provider.LoadFromString($$"""
            .{{className}} {
                color: #{{rgb}};
            }
            """);

            button.GetStyleContext().AddProvider(provider, 1000);
        }

        private static void ApplyCss()
        {
            const string css = """
            .key-tile {
                padding: 0;
                margin: 0;
                border-radius: 6px;
                border-width: 1px;
                min-height: 45px;
                font-size: 12px;
            }

            .keyboard {
                border-radius: 8px;
                padding: 8px;
            }
            """;

            var provider = CssProvider.New();
            provider.LoadFromString(css);

            var display = Gdk.Display.GetDefault();
            if (display != null)
                StyleContext.AddProviderForDisplay(display, provider, 1000);
        }

        private static string RgbaToHex(Gdk.RGBA rgba)
        {
            var r = (byte)Math.Round(rgba.Red * 255);
            var g = (byte)Math.Round(rgba.Green * 255);
            var b = (byte)Math.Round(rgba.Blue * 255);

            return $"{r:X2}{g:X2}{b:X2}";
        }
    }
}
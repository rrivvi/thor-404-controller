using Gtk;

namespace Thor404Controller
{
    partial class MainWindow
    {
        internal static ApplicationWindow CreateWindow(Application app)
        {
            var win = ApplicationWindow.New(app);
            win.SetTitle("Thor 404");
            win.SetDefaultSize(640, 420);
            win.SetResizable(false);

            // 
            var main = Box.New(Orientation.Vertical, 12);
            main.SetMarginTop(12);
            main.SetMarginBottom(12);
            main.SetMarginStart(12);
            main.SetMarginEnd(12);

            // 
            var lightFrame = Frame.New("Lighting");
            lightFrame.SetHexpand(true);

            var lightingBox = Box.New(Orientation.Vertical, 10);
            lightingBox.SetMarginTop(12);
            lightingBox.SetMarginBottom(12);
            lightingBox.SetMarginStart(12);
            lightingBox.SetMarginEnd(12);

            // 
            var effectBox = Box.New(Orientation.Horizontal, 8);

            var effectLabel = Label.New("Effect");
            effectLabel.SetSizeRequest(80, -1);
            effectLabel.SetXalign(0);

            var effectDropdown = DropDown.NewFromStrings(Enum.GetNames<Effects.EffectsEnum>());
            effectDropdown.SetHexpand(true);

            effectBox.Append(effectLabel);
            effectBox.Append(effectDropdown);

            // 
            var colorBox = Box.New(Orientation.Horizontal, 8);

            var colorLabel = Label.New("Base Color");
            colorLabel.SetSizeRequest(80, -1);
            colorLabel.SetXalign(0);

            var colorDialog = ColorDialog.New();
            colorDialog.SetTitle("Choose a color");
            colorDialog.SetWithAlpha(false);

            // in gtk this opens the dialog on click, and color is accessible through colorButton.GetRgba
            var colorButton = ColorDialogButton.New(colorDialog);

            colorBox.Append(colorLabel);
            colorBox.Append(colorButton);

            // 
            var brightnessRow = Box.New(Orientation.Horizontal, 8);

            var brightnessLabel = Label.New("Brightness");
            brightnessLabel.SetSizeRequest(80, -1);
            brightnessLabel.SetXalign(0);

            var brightnessScale = Scale.NewWithRange(
                Orientation.Horizontal,
                0,
                16,
                1
            );

            brightnessScale.SetValue(16);
            brightnessScale.SetHexpand(true);
            brightnessScale.SetDrawValue(true);

            brightnessRow.Append(brightnessLabel);
            brightnessRow.Append(brightnessScale);

            // 
            var speedRow = Box.New(Orientation.Horizontal, 8);

            var speedLabel = Label.New("Speed");
            speedLabel.SetSizeRequest(80, -1);
            speedLabel.SetXalign(0);

            var speedScale = Scale.NewWithRange(
                Orientation.Horizontal,
                0,
                16,
                1
            );

            speedScale.SetValue(12);
            speedScale.SetHexpand(true);
            speedScale.SetDrawValue(true);

            speedRow.Append(speedLabel);
            speedRow.Append(speedScale);

            // 
            var directionRow = Box.New(Orientation.Horizontal, 8);

            var directionLabel = Label.New("Direction");
            directionLabel.SetSizeRequest(80, -1);
            directionLabel.SetXalign(0);

            var directionDropdown = DropDown.NewFromStrings(Enum.GetNames<Effects.DirectionEnum>());
            directionDropdown.SetHexpand(true);

            directionRow.Append(directionLabel);
            directionRow.Append(directionDropdown);

            // 
            var multicolorCheck =
                CheckButton.NewWithLabel("Enable multicolor mode");

            // custom
            var customRow = Box.New(Orientation.Horizontal, 8);

            var customLabel = Label.New("Custom");
            customLabel.SetSizeRequest(80, -1);
            customLabel.SetXalign(0);

            var customButton = Button.NewWithLabel("Customize per-key lighting");
            customButton.OnClicked += (_, __) =>
            {
                // custom todo
            };

            customButton.SetHexpand(true);
            customButton.SetSensitive(false);

            customRow.Append(customLabel);
            customRow.Append(customButton);

            //

            lightingBox.Append(effectBox);
            lightingBox.Append(colorBox);
            lightingBox.Append(multicolorCheck);
            lightingBox.Append(brightnessRow);
            lightingBox.Append(speedRow);
            lightingBox.Append(directionRow);
            lightingBox.Append(customRow);

            lightFrame.SetChild(lightingBox);

            // bottom
            var buttonGrid = Grid.New();
            buttonGrid.SetColumnSpacing(8);
            buttonGrid.SetRowSpacing(8);

            var applyButton = Button.NewWithLabel("Apply");
            applyButton.OnClicked += (_, __) =>
            {
                Gdk.RGBA selectedColor = colorButton.GetRgba();
                var rgb = string.Concat(
                    ((byte)Math.Round(selectedColor.Red * 255)).ToString("X2"),
                    ((byte)Math.Round(selectedColor.Green * 255)).ToString("X2"),
                    ((byte)Math.Round(selectedColor.Blue * 255)).ToString("X2")
                );
                Effects.EffectsEnum selectedPreset = (Effects.EffectsEnum)effectDropdown.GetSelected();
                bool enableMulticolor = multicolorCheck.GetActive();
                byte brightnessValue = (byte)brightnessScale.GetValue();
                byte speedValue = (byte)speedScale.GetValue();
                Effects.DirectionEnum selectedDirection = (Effects.DirectionEnum)directionDropdown.GetSelected();

                Console.WriteLine();
                Console.WriteLine("APPLYING PRESET");
                Console.WriteLine("   preset: " + selectedPreset);
                Console.WriteLine("   color: #" + rgb);
                Console.WriteLine("   multicolor: " + enableMulticolor);
                Console.WriteLine("   brightness: " + brightnessValue);
                Console.WriteLine("   speed: " + speedValue);
                Console.WriteLine("   direction: " + selectedDirection);
                Console.WriteLine();

                try
                {
                    Effects.ApplyEffect(
                        selectedPreset,
                        rgb,
                        enableMulticolor,
                        brightnessValue,
                        speedValue,
                        selectedDirection
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine("`Effects.ApplyEffect` failed in Program.cs:\n\n" + e.Message + "\n\n" + e.StackTrace + "\n\n");
                }
            };

            applyButton.SetHexpand(true);

            buttonGrid.Attach(applyButton, 0, 0, 1, 1);

            // 
            main.Append(lightFrame);
            main.Append(buttonGrid);

            win.SetChild(main);

            return win;
        }
    }
}
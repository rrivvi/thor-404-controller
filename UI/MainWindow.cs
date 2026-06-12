using Gtk;

namespace Thor404Controller.UI
{
    partial class MainWindow
    {
        static readonly StringList DirectionsEntries = StringList.New(["Left", "Up"]);

        internal static async Task<ApplicationWindow> CreateWindow(Application app)
        {
            var win = ApplicationWindow.New(app);
            win.SetTitle("Thor 404");
            win.SetDefaultSize(640, 420);
            win.SetResizable(false);

            Console.WriteLine("Checking for permissions / device");
            // will be null if 1) no device found, or 2) missing admin perms to open hidraw
            bool canProceed = Usb.TryGetHidDevice() != null;

            if (!canProceed)
            {
                Console.WriteLine("Device not found or root permissions not given");
                win.Present();

                var permsWarning = Box.New(Orientation.Vertical, 12);
                permsWarning.Halign = Align.Center;
                permsWarning.Valign = Align.Center;

                var title = Label.New(null);
                title.SetMarkup("<span size=\"x-large\" weight=\"bold\">Device or permissions not found!</span>");
                title.Halign = Align.Center;
                title.MarginBottom = 8;

                var message = Label.New(
                    "Please connect the device.\n" +
                    $"If it is already connected, please run this program {(OperatingSystem.IsLinux() ? "with sudo" : "as administrator")}."
                );
                message.Halign = Align.Center;
                message.Justify = Justification.Center;

                var retryButton = Button.NewWithLabel("Retry");

                retryButton.OnClicked += (_, _) =>
                {
                    canProceed = Usb.TryGetHidDevice() != null;
                };

                permsWarning.Append(title);
                permsWarning.Append(message);
                permsWarning.Append(retryButton);

                win.SetChild(permsWarning);

                while (!canProceed)
                {
                    await Task.Delay(500);
                }

                win.SetChild(null);
                permsWarning?.Dispose();
            }

            Console.WriteLine("Device found and sufficient permissions");

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

            DropDown presetsDropdown = DropDown.NewFromStrings(Enum.GetNames<Effects.EffectsEnum>());
            presetsDropdown.SetHexpand(true);

            effectBox.Append(effectLabel);
            effectBox.Append(presetsDropdown);

            // 
            var colorBox = Box.New(Orientation.Horizontal, 8);

            var colorLabel = Label.New("Base Color");
            colorLabel.SetSizeRequest(80, -1);
            colorLabel.SetXalign(0);

            var colorDialog = ColorDialog.New();
            colorDialog.SetTitle("Choose a color");
            colorDialog.SetWithAlpha(false);

            // in gtk this opens the dialog on click, and color is accessible through ColorDialogButton.GetRgba
            ColorDialogButton colorPicker = ColorDialogButton.New(colorDialog);

            colorBox.Append(colorLabel);
            colorBox.Append(colorPicker);

            // 
            var brightnessRow = Box.New(Orientation.Horizontal, 8);

            var brightnessLabel = Label.New("Brightness");
            brightnessLabel.SetSizeRequest(80, -1);
            brightnessLabel.SetXalign(0);

            Scale brightnessScale = Scale.NewWithRange(
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

            Scale speedScale = Scale.NewWithRange(
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

            DropDown directionDropdown = DropDown.New(DirectionsEntries, null);
            directionDropdown.SetHexpand(true);

            directionRow.Append(directionLabel);
            directionRow.Append(directionDropdown);

            // 
            CheckButton multicolorCheck = CheckButton.NewWithLabel("Enable multicolor mode");

            // custom
            var customRow = Box.New(Orientation.Horizontal, 8);

            var customLabel = Label.New("Custom");
            customLabel.SetSizeRequest(80, -1);
            customLabel.SetXalign(0);

            Button perKeyButton = Button.NewWithLabel("Customize per-key lighting");
            perKeyButton.OnClicked += (_, _) =>
            {
                var editor = CustomEditorWindow.CreateWindow(app, win);
                editor.Present();
            };

            perKeyButton.SetHexpand(true);
            perKeyButton.SetSensitive(true);

            customRow.Append(customLabel);
            customRow.Append(perKeyButton);

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
            applyButton.OnClicked += async (_, __) =>
            {
                applyButton.SetSensitive(false);

                Gdk.RGBA selectedColor = colorPicker.GetRgba();
                var rgb = string.Concat(
                    ((byte)Math.Round(selectedColor.Red * 255)).ToString("X2"),
                    ((byte)Math.Round(selectedColor.Green * 255)).ToString("X2"),
                    ((byte)Math.Round(selectedColor.Blue * 255)).ToString("X2")
                );
                Effects.EffectsEnum selectedPreset = (Effects.EffectsEnum)presetsDropdown.GetSelected();
                bool enableMulticolor = multicolorCheck.GetActive();
                byte brightnessValue = (byte)brightnessScale.GetValue();
                byte speedValue = (byte)speedScale.GetValue();
                Effects.DirectionEnum selectedDirection = Enum.Parse<Effects.DirectionEnum>(DirectionsEntries.GetString(directionDropdown.GetSelected())!);

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

                await Task.Delay(1000);
                applyButton.SetSensitive(true);
            };

            applyButton.SetHexpand(true);

            buttonGrid.Attach(applyButton, 0, 0, 1, 1);

            // 
            main.Append(lightFrame);
            main.Append(buttonGrid);

            win.SetChild(main);

            //

            // when switching presets, this updates the enabled states of the controls
            void ToggleEnabledStates(Effects.EffectsEnum preset)
            {
                void UpdateDirectionDropdown(bool horizontal)
                {
                    while (DirectionsEntries.GetNItems() > 0)
                        DirectionsEntries.Remove(0);

                    DirectionsEntries.Append(horizontal ? "Left" : "Up");
                    DirectionsEntries.Append(horizontal ? "Right" : "Down");

                    directionDropdown.SetSelected(0);
                }

                if (preset == Effects.EffectsEnum.Custom)
                {
                    perKeyButton.SetSensitive(true);
                    colorPicker.SetSensitive(false);
                    brightnessScale.SetSensitive(true);
                    speedScale.SetSensitive(false);
                    directionDropdown.SetSensitive(false);
                    multicolorCheck.SetSensitive(false);
                }
                else if (preset == Effects.EffectsEnum.Disabled)
                {
                    perKeyButton.SetSensitive(false);
                    colorPicker.SetSensitive(false);
                    brightnessScale.SetSensitive(false);
                    speedScale.SetSensitive(false);
                    directionDropdown.SetSensitive(false);
                    multicolorCheck.SetSensitive(false);
                }
                else
                {
                    perKeyButton.SetSensitive(false);
                    switch (preset)
                    {
                        case Effects.EffectsEnum.Static:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(false);
                            speedScale.SetSensitive(false);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.SingleOn:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(false);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.SingleOff:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(false);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.Stars:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(false);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.Raindrops:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(false);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.Colourful:
                            colorPicker.SetSensitive(false);
                            multicolorCheck.SetSensitive(false);
                            directionDropdown.SetSensitive(false);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.Breathing:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(false);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.Neon:
                            colorPicker.SetSensitive(false);
                            multicolorCheck.SetSensitive(false);
                            directionDropdown.SetSensitive(false);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.Spectrum:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(false);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.Rainbow:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(true);
                            UpdateDirectionDropdown(horizontal: false);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.Prismo:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(true);
                            UpdateDirectionDropdown(horizontal: true);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.Circle:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(false);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.EscapeBeam:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(false);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.Shockwave:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(false);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.Explosion:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(false);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.Escape:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(true);
                            UpdateDirectionDropdown(horizontal: true);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.SineWave:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(false);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.Wave:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(true);
                            UpdateDirectionDropdown(horizontal: true);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                        case Effects.EffectsEnum.Shuttle:
                            colorPicker.SetSensitive(true);
                            multicolorCheck.SetSensitive(true);
                            directionDropdown.SetSensitive(false);
                            speedScale.SetSensitive(true);
                            brightnessScale.SetSensitive(true);
                            break;
                    }
                }
            }

            // attached to this event here so that
            // no null references are thrown
            presetsDropdown.OnNotify += (sender, notifyArgs) =>
            {
                ToggleEnabledStates((Effects.EffectsEnum)presetsDropdown.GetSelected());
            };
            // also call it now so that the state is correct on startup 
            ToggleEnabledStates((Effects.EffectsEnum)presetsDropdown.GetSelected());

            //

            Console.WriteLine("Window Created");
            return win;
        }
    }
}
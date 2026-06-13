using System.Diagnostics;
using Gtk;

namespace Thor404Controller.UI
{
    partial class MainWindow
    {
        static readonly StringList DirectionsEntries = StringList.New(["Left", "Up"]);
        private static byte _brightness = 12;
        public static byte GetBrightness() => _brightness;

        static async Task RunChecks(ApplicationWindow win)
        {
            // `issueFound = null` would mean that no issues were found
            // this may get set to a non-null value in later checks
            string? issueFound = null;

            // not an actual loop. it won't get ran more than once
            // this is only used for its `break` functionality
            // this is to avoid a nested `if` if more checks get added
            while (true)
            {
                Console.WriteLine("Checking for permissions");
                if (!Helpers.IsCurrentProcessElevated())
                {
                    issueFound = "admin";
                    break;
                }

                Console.WriteLine("Searching for device");
                if (Usb.TryGetHidDevice() == null)
                {
                    issueFound = "device";
                    break;
                }

                break;
            }

            if (issueFound != null)
            {
                string titleString =
                    issueFound == "admin" ?
                    "Root permissions not given" :
                    issueFound == "device" ?
                    "Device not found" :
                    ""; // should not be reachable

                string messageString =
                    issueFound == "admin" ?
                    "Please authorise the program to launch an elevated process." :
                    issueFound == "device" ?
                    "Make sure you have plugged in the Thor 404 TKL keyboard.\nThis program is made only for that keyboard." :
                    ""; // should not be reachable      

                string buttonString =
                    issueFound == "admin" ?
                    "Grant Permissions" :
                    issueFound == "device" ?
                    "Retry" :
                    ""; // should not be reachable 
                Console.WriteLine(titleString);

                //

                var permsWarning = Box.New(Orientation.Vertical, 12);
                permsWarning.Halign = Align.Center;
                permsWarning.Valign = Align.Center;

                var title = Label.New(null);
                title.SetMarkup($"<span size=\"x-large\" weight=\"bold\">{titleString}</span>");
                title.Halign = Align.Center;
                title.MarginBottom = 8;

                var message = Label.New(messageString);
                message.Halign = Align.Center;
                message.Justify = Justification.Center;

                var retryButton = Button.NewWithLabel(buttonString);

                retryButton.OnClicked += async (_, _) =>
                {
                    switch (issueFound)
                    {
                        case "device":
                            if (Usb.TryGetHidDevice() != null)
                            {
                                issueFound = null;
                            }
                            break;
                        case "admin":
                            // if Item1 is true, Item2 is the new process asking to be elevated
                            (bool, Process) elevationResult = Helpers.TryStartElevated();
                            if (elevationResult.Item1)
                            {
                                // hide this window visually and let the new process
                                // show the pkexec authorisation screen
                                win.Hide();
                                win?.Dispose();
                                Console.WriteLine("Original window hidden and disposed");

                                await elevationResult.Item2.WaitForExitAsync();
                                elevationResult.Item2?.Dispose();

                                // user elevated priviledges (or didn't) for a new process of this executable
                                // continuing here would run this instance hidden in the background
                                Environment.Exit(0);
                            }
                            break;
                    }
                };

                permsWarning.Append(title);
                permsWarning.Append(message);
                permsWarning.Append(retryButton);

                win.SetChild(permsWarning);
                win.Present();

                while (issueFound != null)
                {
                    await Task.Delay(1000);
                }

                win.SetChild(null);
                permsWarning?.Dispose();
            }
        }

        internal static async Task<ApplicationWindow> CreateWindow(Application app)
        {
            var win = ApplicationWindow.New(app);
            win.SetTitle("Thor 404");
            win.SetDefaultSize(640, 420);
            win.SetResizable(false);

            // before the actual gui is shown to the user,
            // run checks for admin, device, etc.
            await RunChecks(win);
            Console.WriteLine("Checks passed, now creating the GUI");

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
            brightnessScale.OnNotify += (_, __) =>
            {
                _brightness = (byte)brightnessScale.GetValue();
            };

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

                var rgb = Helpers.RgbaToHex(colorPicker.GetRgba());
                Effects.EffectsEnum selectedPreset = (Effects.EffectsEnum)presetsDropdown.GetSelected();
                bool enableMulticolor = multicolorCheck.GetActive();
                byte brightnessValue = (byte)brightnessScale.GetValue();
                byte speedValue = (byte)speedScale.GetValue();
                Effects.DirectionEnum selectedDirection = Enum.Parse<Effects.DirectionEnum>(DirectionsEntries.GetString(directionDropdown.GetSelected())!);

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

            // attached to this event here because ToggleEnabledStates
            // had to be defined after the variables it references
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
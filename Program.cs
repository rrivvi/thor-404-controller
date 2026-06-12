using Gtk;
using Newtonsoft.Json.Linq;

namespace Thor404Controller
{
    partial class Program
    {
        static JObject keymap = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(Keymap.GetString())!;
        static Dictionary<string, string>? customColors = null;

        protected static void Main()
        {
            var app = Application.New("controller.thor404", Gio.ApplicationFlags.DefaultFlags);

            app.OnActivate += (_, _) =>
            {
                var win = MainWindow.CreateWindow(app);
                win.Present();
            };

            app.Run(null);
        }
    }
}
using Gtk;
using Newtonsoft.Json.Linq;
using Thor404Controller.UI;

namespace Thor404Controller
{
    partial class Program
    {
        internal static JObject keymapJObject = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(Keymap.GetString())!;
        internal static Dictionary<string, string> customColorsPairs = new();

        protected static void Main()
        {
            var app = Application.New("controller.thor404", Gio.ApplicationFlags.DefaultFlags);

            app.OnActivate += async (_, _) =>
            {
                Console.WriteLine("Creating Window");
                var win = await MainWindow.CreateWindow(app);
                win.Present();
            };

            app.Run(null);
        }
    }
}
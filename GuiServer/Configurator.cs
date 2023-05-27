using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace GuiServer
{
    internal class Configurator
    {
        public string FilePath { get; private set; }
        public int ServerPort { get; set; }
        public bool AutostartServer { get; set; }
        public string PublicDirectory { get; set; }

        public delegate void SavingDelegate(object sender, JObject root);
        public delegate void LoadingDelegate(object sender, JObject root);
        public delegate void LoadedDelegate(object sender);
        public SavingDelegate Saving;
        public LoadingDelegate Loading;
        public LoadedDelegate Loaded;

        public Configurator()
        {
            string exePath = Application.ExecutablePath;
            string selfDirectory = Path.GetDirectoryName(exePath);
            string fileName = $"{Path.GetFileNameWithoutExtension(exePath)}_config.json";
            FilePath = Path.Combine(selfDirectory, fileName);
            LoadDefaults();
        }

        public void LoadDefaults()
        {
            ServerPort = 5555;
            AutostartServer = false;
            PublicDirectory = Path.GetDirectoryName(Application.ExecutablePath);
        }

        public void Load()
        {
            LoadDefaults();
            if (File.Exists(FilePath) && Loading != null)
            {
                string content = File.ReadAllText(FilePath);
                Loading.Invoke(this, JObject.Parse(content));
            }
            Loaded?.Invoke(this);
        }

        public void Save()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }

            if (Saving != null)
            {
                JObject json = new JObject();
                Saving.Invoke(this, json);
                File.WriteAllText(FilePath, json.ToString());
            }
        }
    }
}

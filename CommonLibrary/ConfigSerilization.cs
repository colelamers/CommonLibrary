using System.Reflection;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace CommonLibrary;

public class ConfigSerilization {
    public string _defaultFileLocation { get; set; }
    public ConfigSerilization() {
        _defaultFileLocation = Pathing.ExecutablePath + Pathing.ExeFileName + "_config.xml";
    }

    public ConfigSerilization(string customConfigLocationFullFileName, bool prependToExecPath) {
        if (prependToExecPath){
            _defaultFileLocation = Pathing.ExecutablePath + customConfigLocationFullFileName;
        }
        else {
            _defaultFileLocation = customConfigLocationFullFileName;
        }
    }

    // _boEcas.LogDetail(MethodBase.GetCurrentMethod());
    public void WriteXmlConfig<T>(T configObject) where T : class, new() {
        try {
            File.Delete(_defaultFileLocation);
            // todo 2; seems redundant?
            if (!File.Exists(_defaultFileLocation)) {
                using (StreamWriter sw = File.CreateText(_defaultFileLocation)) {
                    sw.Flush();
                    sw.Close();
                }
            }

            using (FileStream stream = new FileStream(_defaultFileLocation, FileMode.Create)) {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stream, configObject);
            }
        }
        catch { }
    }

    public T? ReadXmlConfig<T>(Type inType) where T : class, new() {
        try {
            using (FileStream stream = new FileStream(_defaultFileLocation, FileMode.Open)) {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                T? xml = (T?)serializer.Deserialize(stream);
                return xml;
            }
        }
        catch { }
        return new T();
    }
}

using System.Reflection;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace CommonLibrary;

public class ConfigSerilization
{
    public string _defaultFileLocation { get; set; }
    public ConfigSerilization()
    {
        _defaultFileLocation = Pathing.ExecutablePath + Pathing.ExeFileName + "_config.xml";
    }

    public ConfigSerilization(string customConfigLocationFullFileName, bool prependToExecPath)
    {
        if (prependToExecPath)
        {
            _defaultFileLocation = Pathing.ExecutablePath + customConfigLocationFullFileName;
        }
        else
        {
            _defaultFileLocation = customConfigLocationFullFileName;
        }
    }

    public void WriteXmlConfig<T>(T configObject) where T : class, new()
    {
        try
        {
            string dir = Path.GetDirectoryName(_defaultFileLocation) ?? Pathing.ExecutablePath;
            Directory.CreateDirectory(dir);

            // Write atomically to avoid partial file corruption
            string tempPath = dir + Path.GetFileNameWithoutExtension(_defaultFileLocation) + ".tmp";
            using (FileStream stream = new FileStream(
                    tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stream, configObject);
            }

            if (File.Exists(_defaultFileLocation))
            {
                // replace the original file; no backup
                File.Replace(tempPath, _defaultFileLocation, null);
            }
            else
            {
                File.Move(tempPath, _defaultFileLocation);
            }
        }
        catch { }
    }

    public T? ReadXmlConfig<T>(Type inType) where T : class, new()
    {
        try
        {
            if (!File.Exists(_defaultFileLocation))
            {
                return new T();
            }
            using (FileStream stream = new FileStream(_defaultFileLocation, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                T? xml = (T?)serializer.Deserialize(stream);
                return xml;
            }
        }
        catch { }
        return new T();
    }
}

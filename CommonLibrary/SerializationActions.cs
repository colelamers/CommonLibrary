using System.Reflection;
using System.Xml.Serialization;

namespace CommonLibrary.SerializationActions
{
    /*
     * Created by Cole Lamers 
     * 
     * This code allows large amounts of data to be saved in a serialized binary 
     * file and loaded back in. I initially built this 
     * 
     * Code grabbed from users Manuael Faux and deadlydog: 
     * https://stackoverflow.com/questions/6115721/how-to-save-restore-serializable-object-to-from-file
     * 
     * 
     * 2021-11-10    First version
     * 2021-11-10    Built initially to save my Trie Data structure to minimize startup time
     *               for school project final.
     * 2023-08-12    Updated comments, made some minor code revisions for better readability.
     *               Removed Configuration file and placed that info over here because it generally
     *               is pertinent to serialization activities.
     * 2023-08-12    Discovered BinaryFormatter is unsafe. Quote from the link:
     *               "As a simpler analogy, assume that calling BinaryFormatter.Deserialize over a 
     *               payload is the equivalent of interpreting that payload as a standalone executable 
     *               and launching it."
     *               https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-security-guide
     * 2023-08-13    Revised GetAssemblyNamePath and GetConfigFilePath with explicity full path values
     *               instead of relative assumptions. 
     *               Performed a file rename to resolve some conflicting naming on MS serialization.
     * 2023-08-19    Revised xml reading features to support error handling and logging making xml
     *               debugging much easier.
     * 2023-09-20    Replaced IsNullOrEmpty's with IsNullOrWhiteSpace. Also forgot a "return T" in XML
     *               loading which is why it would load, but always return null.
     *
     */
    public class Serializing : Init // todo 4; look into having this extend configuration?
    {
        #region XmlDelegates
        /**
         * This is utilized to support error handling when debugging xml
         * serializing errors. This specifically detects an unknown node
         * in the type of serialized configuration.
         * 
         * @param | object |
         * Required for the delegate.
         * 
         * @param | XmlNodeEventArgs |
         * This is the default instantiated delegate item from the xml api
         * that provides node error handling.
         */
        protected static void serializer_UnknownNode
        (object sender, XmlNodeEventArgs e)
        {
            throw new Exception($"Unknown Node in Xml! Name: {e.Name}, Text: {e.Text}");
        }
        /**
         * This is utilized to support error handling when debugging xml
         * serializing errors. This specifically detects an unknown attributes
         * in the type of serialized configuration.
         * 
         * @param | object |
         * Required for the delegate.
         * 
         * @param | XmlAttributeEventArgs |
         * This is the default instantiated delegate item from the xml api
         * that provides attribute error handling.
         */
        protected static void serializer_UnknownAttribute
        (object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            throw new Exception($"Unknown Node in Xml! Name: {attr.Name}, Value: {attr.Value}");

        }
        #endregion XmlDelegates
        #region Writing
        /**
         * Writes the given object instance to a binary file.
         * 
         * @type | T |
         * The type of object being written to the binary file.
         * -Object type (and all child types) must be denoted with the [Serializable] attribute.
         * -To prevent a variable from being serialized, denote it with the [NonSerialized] attribute.
         * -Cannot be applied to properties.
         * 
         * @param | string | "C:\\fully\\qualified\\path.file" |
         * The file path to read the object instance from.
         * -Only Public properties and variables will be written to the file. 
         * These can be any type, even other classes.
         * 
         * @param | T |
         * The object instance to write to the binary file.
         * -If there are public properties/variables that you do not want 
         * written to the file, denote them with the [XmlIgnore] attribute.
         * 
         * @param | bool | true, false |
         * False: File will be overwritten if already exists.
         * True:  Contents will be appended to file
         * Object type must have a parameterless constructor.
         * 
         * @returns | T | 
         * Returns a new instance of the object read from the bin (binary) file.
         */
        public static void WriteToFile_Binary<T>(string filePath, T objectToWrite, bool append = false)
        {
            // todo 4; apparently the append is somewhat pointless according to deadlydog
            // because you'd most likely just rather create a new file and not risk appending
            // a binary serialized file
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }
        #endregion Writing
        #region Reading
        /**
         * Reads an object instance from a binary file.
         * 
         * @type | T |
         * Type of serialized object from the config file.
         * -Object type must have a parameterless constructor.
         * 
         * @param | string | "C:\\fully\\qualified\\path.file" |
         * The file path to read the object instance from.
         * -Only Public properties and variables will be written to the file. 
         * -These can be any type though, even other classes.
         * 
         * @returns | T | 
         * Returns a new instance of the object read from the binary file.
         */
        public static T ReadFromFile_Binary<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }
        #endregion Reading
        #region Configuration Serialization
        /* 
         * todo 4; Need to work on a function that sets the default path/location when saving/generating the file
         * todo 4; rework file naming for GetProjectName()? maybe not go off of project?
         * todo 4; create a function that does not require a reference object and that it can make the xml 
         *         config file and update to it as items are added to it; likely a modification to the save? 
         * todo 4; establish a means for configuration that does not default to xml file saving?
         */
        /**
         * Detects if the specified file exists. It will create and/or serialize it in XML.
         * 
         * @type | T |
         * The type of object associated with the configuration file.
         * -Object type (and all child types) must be denoted with the [Serializable] attribute.
         * -To prevent a variable from being serialized, denote it with the [NonSerialized] attribute.
         * -Cannot be applied to properties.
         * 
         * @param | string | "xml" |
         * Pass the file type, no period preceeding it. Only the file type designation.
         * -Default set to xml
         */
        public static void CreateNewConfiguration<T>(Logging Logger, string fileType = "xml") where T : new()
        {
            try
            {
                Logger.Log("CreateNewConfiguration...");

                // todo 4; allow for additional serialization?
                string fileName = GetConfigFilePath<T>(Logger, fileType);

                // Creates the config file if it doesn't exist
                if (!File.Exists(Path.GetFullPath(fileName)))
                {
                    using (StreamWriter sw = File.CreateText(fileName))
                    {
                        sw.Flush();
                        sw.Close();
                    }
                }

                using (FileStream stream = new FileStream(fileName, FileMode.Create))
                {
                    if (fileType.Equals("xml"))
                    {
                        XmlSerializer xml = new XmlSerializer(typeof(T));
                        xml.Serialize(stream, new T());
                    }
                }
            }
            catch { }
        }
        /**
         * Overwrites current configuration file with object passed in.
         * 
         * @type | T |
         * The type of object associated with the configuration file.
         * -Object type (and all child types) must be denoted with the [Serializable] attribute.
         * -To prevent a variable from being serialized, denote it with the [NonSerialized] attribute.
         * -Cannot be applied to properties.
         * 
         * @param | T | 
         * This is the configuration object you are passing in to save/overwrite for the 
         * current file.
         * 
         * @param | string | "xml" |
         * Pass the file type, no period preceeding it. Only the file type designation.
         * -Default set to xml
         */
        public static void SaveConfigFile<T>(Logging Logger, T configObject, string fileType = "xml")
        {
            try
            {
                Logger.Log("SaveConfigFile...");
                string fileName = GetConfigFilePath<T>(Logger, fileType);
                Logger.Log("FileName:" + fileName);
                using (FileStream stream = new FileStream(fileName, FileMode.Create))
                {
                    if (fileType.Equals("xml"))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(T));
                        serializer.UnknownNode += new
                            XmlNodeEventHandler(serializer_UnknownNode);
                        serializer.UnknownAttribute += new
                            XmlAttributeEventHandler(serializer_UnknownAttribute);

                        serializer.Serialize(stream, configObject);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("SaveConfigFile error", ex);
            }
        }
        /**
         * Loads in an already existing XML file.
         * 
         * @type | T |
         * The type of object associated with the configuration file.
         * -Object type (and all child types) must be denoted with the [Serializable] attribute.
         * -To prevent a variable from being serialized, denote it with the [NonSerialized] attribute.
         * -Cannot be applied to properties.
         * 
         * @param | string | "xml" |
         * Pass the file type, no period preceeding it. Only the file type designation.
         * -Default set to xml
         * 
         * @return | T or null |
         * Configuration type T will be returned after XML serialization. Null returns
         * if the code fails at any point to load in the configuration.
         */
        public static T? LoadConfigFile<T>(Logging Logger, string fileType = "xml") where T : class, new()
        {
            try
            {
                Logger.Log("LoadConfigFile...");
                // todo 4; allow for additional serialization loading?
                string fileName = GetConfigFilePath<T>(Logger, fileType);
                Logger.Log("FileName:" + fileName);
                using (FileStream stream = new FileStream(fileName, FileMode.Open))
                {
                    // todo 4; use switch case in future for multiple file types
                    if (fileType.Equals("xml"))
                    {
                        // Event Handlers help with debugging with issues related to xml
                        XmlSerializer serializer = new XmlSerializer(typeof(T));
                        serializer.UnknownNode += new
                            XmlNodeEventHandler(serializer_UnknownNode);
                        serializer.UnknownAttribute += new
                            XmlAttributeEventHandler(serializer_UnknownAttribute);

                        T xml = (T)serializer.Deserialize(stream);
                        return xml;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("LoadConfigFile error", ex);
            }
            return null;
        }
        /**
         * Retrieves the configuration file's fully qualified path name. 
         * The default values assume a standard way for configuration file naming
         * and as a designated type of XML.
         * 
         * @param | string | "xml" |
         * Pass the file type, no period preceeding it. Only the file type designation.
         * -Default set to xml
         * 
         * @returns | "C:\\fully\\qualified\\path_Config.filetype" |
         * Returns the config file path.
         */
        public static string GetConfigFilePath<T>(Logging Logger, string fileType = "xml")
        {
            Logger.Log("LoadConfigFile...");

            try
            {


            string? compiledCodeFullPath = GetAssemblyNamePath<T>(Logger);
            if (!string.IsNullOrWhiteSpace(compiledCodeFullPath))
            {
                string? execPath = Path.GetDirectoryName(compiledCodeFullPath);
                string fileName = Path.GetFileNameWithoutExtension(compiledCodeFullPath);
                if (!string.IsNullOrWhiteSpace(execPath) && !string.IsNullOrWhiteSpace(fileName))
                {
                    return execPath + "\\" + fileName + "_Config." + fileType;
                }
            }
            }
            catch (Exception ex)
            {
                Logger.Log("error...", ex);

            }
            return string.Empty;
        }
        #endregion Configuration Serialization
        /**
         * Retrieves just the assembly path of the project name;
         * 
         * @returns | string | "C:\\fully\\qualified\\path.file"
         * Returns the full path of the assembly item.
         */
        public static string GetAssemblyNamePath<T>(Logging Logger)
        {
            Logger.Log("GetAssemblyNamePath...");
            string? compiledSolutionDllFullPath = Assembly.GetAssembly(typeof(T))?.Location;
            if (!string.IsNullOrWhiteSpace(compiledSolutionDllFullPath))
            {
                return compiledSolutionDllFullPath;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}

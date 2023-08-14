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
     *
     */
    public class Serializing : Init // todo 4; look into having this extend configuration?
    {
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
        /**
         * Writes the given object instance to an XML file.
         * 
         * @type | T |
         * Type of serialized object from the config file.
         * -Object type must have a parameterless constructor.
         * 
         * @param | string | "C:\\fully\\qualified\\path.file" |
         * The file path to read the object instance from.
         * -Only Public properties and variables will be written to the file. 
         * -These can be any type, even other classes.
         * 
         * @param | T |
         * The object instance of Type "T" to write to the file.
         * -If there are public properties/variables that you do not want 
         * written to the file, decorate them with the [XmlIgnore] attribute.
         * 
         * @param | bool | true, false |
         * False: File will be overwritten if already exists.
         * True:  Contents will be appended to file
         * 
         * @returns | T | 
         * Returns a new instance of the object read from the XML file.
         */
        public static void WriteToFile_Xml<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            // todo 4; why this TextWriter logic?
            TextWriter writer = null;
            try
            {
                writer = new StreamWriter(filePath, append);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, objectToWrite);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }
        #endregion Writing
        #region Reading
        /**
         * Reads an object instance from an XML file.
         * 
         * @type | T |
         * Type of serialized object from the config file.
         * -Object type must have a parameterless constructor.
         * 
         * @param | string | "C:\\fully\\qualified\\path.file" |
         * The file path to read the object instance from.
         * 
         * @returns | T | 
         * Returns a new instance of the configuration object read from the XML file.
         */
        public static T ReadFromFile_Xml<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                reader = new StreamReader(filePath);
                return (T)serializer.Deserialize(reader);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }
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
        public static void CreateNewConfiguration<T>(string fileType = "xml") where T : new()
        {
            try
            {
                // todo 4; allow for additional serialization?
                string fileName = GetConfigFilePath<T>(fileType);

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
        public static void SaveConfigFile<T>(T configObject, string fileType = "xml") where T : new()
        {
            try
            {
                string fileName = GetConfigFilePath<T>(fileType);
                using (FileStream stream = new FileStream(fileName, FileMode.Create))
                {
                    if (fileType.Equals("xml"))
                    {
                        XmlSerializer xml = new XmlSerializer(typeof(T));
                        xml.Serialize(stream, configObject);
                    }
                }
            }
            catch { }

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
        public static T LoadConfigFile<T>(string fileType = "xml") where T : class
        {
            // todo 4; allow for additional serialization loading?
            string fileName = GetConfigFilePath<T>(fileType);
            
            // Attemp to serialize, if you cannot
            try
            {
                using (FileStream stream = new FileStream(fileName, FileMode.Open))
                {
                    if (fileType.Equals("xml"))
                    {
                        XmlSerializer xml = new XmlSerializer(typeof(T));
                        return (T)xml.Deserialize(stream);
                    }
                }
            }
            catch { }

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
        public static string GetConfigFilePath<T>(string fileType = "xml")
        {
            string compiledCodeFullPath = GetAssemblyNamePath<T>();
            if (!string.IsNullOrEmpty(compiledCodeFullPath))
            {
                string execPath = Path.GetDirectoryName(compiledCodeFullPath);
                string fileName = Path.GetFileNameWithoutExtension(compiledCodeFullPath);
                return execPath + "\\" + fileName + "_Config." + fileType;
            }
            else
            {
                return string.Empty;
            }
        }
        #endregion Configuration Serialization
        /**
         * Retrieves just the assembly path of the project name;
         * 
         * @returns | string | "C:\\fully\\qualified\\path.file"
         * Returns the full path of the assembly item.
         */
        public static string GetAssemblyNamePath<T>()
        {
            string compiledSolutionDllFullPath = Assembly.GetAssembly(typeof(T)).Location;
            if (!string.IsNullOrEmpty(compiledSolutionDllFullPath))
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

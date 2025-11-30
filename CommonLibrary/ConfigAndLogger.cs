/*
using System;
using System.IO;
using System.Reflection;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ConfigAndLogger;

public sealed class BOEcasSetupObjects {
    
    public static string ConfigSuffixFileWithExtension { get; } = "_Config.xml";

    public static string ProjectExecutable {
        get {
            return MethodBase.GetCurrentMethod().DeclaringType.Assembly.Location;
        }
    }

    public static string ProjectExePath {
        get {
            return Path.GetDirectoryName(this.ProjectExecutable);
        }
    }

    public static string ProjectExeFileName {
        get {
            return Path.GetFileNameWithoutExtension(this.ProjectExecutable);
        }
    }

    public static string XmlFilePath {
        get {
            return this.ProjectExePath + "\\" + this.ProjectExeFileName + this.ConfigSuffixFileWithExtension;
        }
    }

    public static string AssemblyName {
        get {
            return System.Reflection.Assembly.GetEntryAssembly().GetModules()[0].FullyQualifiedName;
        }
    }

    public static string GetNameWithoutExtension {
        get {
            return Path.GetDirectoryName(this.AssemblyName) + @"\" + Path.GetFileNameWithoutExtension(this.AssemblyName);
        }
    }

    // Singleton instance
    private static volatile BOEcasSetupObjects _boEcas = null;
    private static object _syncRoot = new object();
    private BOEcasSetupObjects() { }
    public static BOEcasSetupObjects SetBoEcasObject() {
        // For multithreaded purposes
        lock (_syncRoot) {
            if (_boEcas == null) {
                _boEcas = LoadConfigAndLogger();
            }
        }

        return _boEcas;
    }

    private DebugLogging _Log { get; set; }
    public Configuration Config { get; private set; }

    public void SaveConfigDataToFile() {
        // Need to save it at this time so if initialized every again,
        // it reads in the new version.
        SaveXMLFile(_boEcas.Config);
    }

    // The initial loader class. This is how the class becomes initialized.
    private static BOEcasSetupObjects LoadConfigAndLogger() {
        _boEcas = new BOEcasSetupObjects();
        // Set up Config File
        if (!File.Exists(XmlFilePath)) {
            CreateBaseConfiguration(new Configuration(), ProjectExecutable);
            _boEcas.Config = (Configuration)ReadXMLFile(typeof(Configuration));
            _boEcas.Config.InitializeConfigLists();
            SaveXMLFile(_boEcas.Config);
        }
        else {
            // Purpose of using ReadXMLFile here is because then I can
            // debug errors with XML file loading if they occur since
            // this returns the location of where the serialization occurs.
            // NOTE: If you have list objects that are "doubling" the
            //       number of items upon XML serialization, it means you
            //       are initializing a constructor, ON TOP of reading in
            //       your XML. This cannot be helped because the XML
            //       serialization must initialize the objects as is. To
            //       resolve this, you cannot have constructors for
            //       serialized objects and can only have (basically)
            //       struct-like objects to avoid constructor initializing.
            _boEcas.Config = (Configuration)ReadXMLFile(typeof(Configuration), XmlFilePath);
        }

        _boEcas._Log = new DebugLogging(_boEcas.Config.DebugEmailTo, _boEcas.Config.DebugLevel,
            _boEcas.Config.SMTPHost, _boEcas.Config.DebugEmailSubject);
        return _boEcas;
    }

    public void SendEmailIfErrors() {
        _Log.SendEmailIfErrors();
    }

    public void LogDetail(MethodBase method) {
        _Log.LogDetailMessage($"{method.Name} - {method.DeclaringType.Name}");
    }

    public void LogDetail(string log) {
        _Log.LogDetailMessage(log);
    }

    public void LogDetail(string log, Exception ex) {
        _Log.LogDetailMessage(log, ex);
    }

    public void LogDetail(Exception ex) {
        _Log.LogDetailMessage($"Error: {ex}");
    }

    public void LogInfo(MethodBase method) {
        _Log.LogInfoMessage($"{method.Name} - {method.DeclaringType.Name}");
    }

    public void LogInfo(string log) {
        _Log.LogInfoMessage(log);
    }

    public void LogInfo(string log, Exception ex) {
        _Log.LogInfoMessage(log, ex);
    }

    public void LogInfo(Exception ex) {
        _Log.LogInfoMessage($"Error: {ex}");
    }

    public void LogError(MethodBase method) {
        _Log.LogError($"{method.Name} - {method.DeclaringType.Name}");
    }

    public void LogError(string log) {
        _Log.LogError(log);
    }

    public void LogError(string log, Exception ex) {
        _Log.LogError(log, ex);
    }

    public void LogError(Exception ex) {
        _Log.LogError($"Error: {ex}");
    }

    public void SendEmail(string emailSubject, string emailBody,
        string[] emailAddresses, string[] attachementPaths) {
        SendEmail email = new SendEmail(Config.SMTPHost);
        email.emailSubject = emailSubject;
        email.emailBody = emailBody;
        
        foreach (string emailAccount in emailAddresses) {
            email.emailTo = emailAccount;
        }

        foreach (string attach in attachementPaths) {
            email.emailAttachments = attach;
        }

        email.send();
    }

    public void SendBasicEmail(string emailSubject, string emailBody, string emailAddress) {
        SendEmail email = new SendEmail(Config.SMTPHost);
        email.emailSubject = emailSubject;
        email.emailBody = emailBody;
        email.emailTo = emailAddress;
        email.send();
    }

    public static string GetStringHash(string inStringToHash) {
        // Code from here:  https://stackoverflow.com/questions/38043954/generate-unique-hash-code-based-on-string
        SHA256 shaHash = SHA256.Create();

        // Convert the inStringToHash string to a byte array and compute the hash.
        byte[] data = shaHash.ComputeHash(Encoding.UTF8.GetBytes(inStringToHash));

        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        StringBuilder sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data 
        // and format each one as a hexadecimal string.
        for (int i = 0; i < data.Length; i++) {
            sBuilder.Append(data[i].ToString("x2"));
        }

        // Return the hexadecimal string.
        return sBuilder.ToString();
    }

    public static bool GetBoolFromString(string inString) {
        bool outputData = false;
        if (!string.IsNullOrWhiteSpace(inString)) {
            string[] potentialTrueValues = {"1", "true", "on", "yes"};
            outputData = potentialTrueValues.Contains(inString.Trim(), StringComparer.InvariantCultureIgnoreCase);
        }

        return outputData;
    }

    public static string ConvertSpecialCharsToHtmlHex(string dataWithSpecialChars) {
        string htmlEncode = WebUtility.HtmlEncode(dataWithSpecialChars);
        var t = htmlEncode.Split(new string[] { "&","#","amp",";" }, StringSplitOptions.RemoveEmptyEntries);
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < t.Length; ++i) {
            int isNum;
            int.TryParse(t[i],out isNum);
            string hex = "";
            if (isNum > 0) {
                hex = string.Format("\\u{0:X4}",isNum);
            }

            if (!string.IsNullOrWhiteSpace(hex)) {
                sb.Append(hex);
            }
            else {
                sb.Append(t[i]);
            }
        }
        return sb.ToString();
    }

    public static object ReadXMLFile(Type inType, string inFileName = "") {
        if (!string.isNullOrWhiteSpace(this.GetNameWithoutExtension)){
            
        }
        object outConfig = inType.GetConstructor(new Type[] { }).Invoke(new object[] { });
        XmlSerializer xmlS = new XmlSerializer(inType);
        using (StreamReader sr = new StreamReader(inFileName)) {
            outConfig = xmlS.Deserialize(sr);
        }
        xmlS = null;
        return outConfig;
    }

    public static void SaveXMLFile(object inObject, string inFileName) {
        File.Delete(inFileName);
        XmlSerializer xmlS = new XmlSerializer(inObject.GetType());
        using (StreamWriter sw = new StreamWriter(inFileName)) {
            xmlS.Serialize(sw, inObject);
        }
    }

    public static bool IsInRange(string inLowerIP, string inHigherIP, string inTestIP) {
        IPAddress lowerInclusive = IPAddress.Parse(inLowerIP);
        IPAddress upperInclusive = IPAddress.Parse(inHigherIP);
        IPAddress address = IPAddress.Parse(inTestIP);

        AddressFamily addressFamily = lowerInclusive.AddressFamily;
        byte[] lowerBytes = lowerInclusive.GetAddressBytes();
        byte[] upperBytes = upperInclusive.GetAddressBytes();
        if (address.AddressFamily != addressFamily) {
            return false;
        }

        byte[] addressBytes = address.GetAddressBytes();
        bool lowerBoundary = true, upperBoundary = true;

        for (int i = 0; i < lowerBytes.Length &&
            (lowerBoundary || upperBoundary); i++) {
            if ((lowerBoundary && addressBytes[i] < lowerBytes[i]) ||
                (upperBoundary && addressBytes[i] > upperBytes[i])) {
                return false;
            }

            lowerBoundary &= (addressBytes[i] == lowerBytes[i]);
            upperBoundary &= (addressBytes[i] == upperBytes[i]);
        }

        return true;
    }

    public static byte[] GetDataAsCompressedBytes(object inObject) {
        XmlSerializer xmlS = new XmlSerializer(inObject.GetType());
        MemoryStream tData = new MemoryStream();
        xmlS.Serialize(tData,inObject);

        byte[] data = tData.ToArray();

        MemoryStream output = new MemoryStream();
        using (DeflateStream dstream = new DeflateStream(output,CompressionLevel.Fastest)) {
            dstream.Write(data,0,data.Length);
        }

        byte[] returnData = output.ToArray();

        return returnData;
    }

    public static string GetDataFromCompressedBytes(byte[] inBytes) {
        MemoryStream input = new MemoryStream(inBytes);
        MemoryStream output = new MemoryStream();

        using (DeflateStream dsstream = new DeflateStream(input,CompressionMode.Decompress)) {
            dsstream.CopyTo(output);
        }

        byte[] tArrayData = output.ToArray();
        string returnData = System.Text.Encoding.Default.GetString(tArrayData);

        return returnData;
    }

    public static void SaveConfiguration(object inConfig, string FullyQualifiedExeName = "") {
        if (!string.IsNullOrWhiteSpace(FullyQualifiedExeName)) {
            executableFullyQualifiedName = FullyQualifiedExeName;
        }
        string configFileName = GetConfigFileName();
        SaveXMLFile(inConfig, configFileName);
    }
}
*/

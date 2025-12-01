using System.IO;
using System.Reflection;

namespace CommonLibrary;

public static class Pathing
{
    public static string UserProfilePath
    {
        get
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
    }

    public static string SolutionPath
    {
        get
        {
            return Path.GetFullPath(Path.Combine(ExecutablePath, "../../../../"));
        }
    }

    public static string SolutionFile
    {
        get
        {
            return SolutionPath + ExeFileName + ".sln";
        }
    }

    public static string ProjectPath
    {
        get
        {
            return Path.GetFullPath(Path.Combine(ExecutablePath, "../../../"));
        }
    }

    public static string ProjectFile
    {
        get
        {
            return ProjectPath + ExeFileName + ".csproj";
        }
    }

    public static string ExecutablePath
    {
        get
        {
            return AppContext.BaseDirectory;
        }
    }

    public static string ExecutableFile
    {
        get
        {
            //return System.Reflection.Assembly.GetEntryAssembly().GetModules()[0].FullyQualifiedName;
            return MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly.Location ?? "";
        }
    }

    public static string ExeFileName
    {
        get
        {
            return Path.GetFileNameWithoutExtension(ExecutableFile);
        }
    }

    public static string XmlFile
    {
        get
        {
            return XmlFilePath + ExeFileName + ".xml";
        }
    }

    public static string XmlFilePath
    {
        get
        {
            return ProjectPath + "config/";
        }
    }
}

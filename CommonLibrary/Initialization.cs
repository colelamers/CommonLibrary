using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CommonLibrary
{
    /*
     * Created by Cole Lamers 
     * 
     * This code helps to perform all the baseline initialization needed
     * to get a proper log file and xml configuration file setup, as well
     * as a processing class (if needed) so that you can have that all
     * ready to go with one class initialization as opposed to manually doing
     * it for each new project.
     * 
     * 2023-08-12    First version
     */     
    public class Initialization<T> where T : new() 
    {
        public Logging? Logging { get; private set; }
        public T? Configuration { get; private set; }
        /**
         * todo 3;
         */
        public Initialization() 
        {
            Logging = new Logging();
            Configuration = new T();
            string projectNamePath = SerializationActions.GetAssemblyNamePath<T>();
            string xmlConfigFile = SerializationActions.GetConfigFilePath<T>();

            try
            {
                if (!File.Exists(xmlConfigFile))
                {
                    SerializationActions.SaveConfigFile<T>(Configuration, projectNamePath);
                    Configuration = SerializationActions.LoadConfigFile<T>(Configuration);
                }
                else
                {
                    Configuration = SerializationActions.LoadConfigFile<T>(Configuration);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Error Initializing Config file.", ex);
            }
        }
    }
}

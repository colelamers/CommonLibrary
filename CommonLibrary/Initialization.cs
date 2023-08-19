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
     * 2023-08-13    Revised Initialization function a bit. Added some additional
     *               type constraints for the "T" type. Streamlined it pretty
     *               well to have everything I need work on startup as soon as the
     *               class is initialized. All self contained.
     *               Revised Initialization to become a subclass because I don't
     *               want the serializationactions functions callable outside of 
     *               the Init class. Specifically because the Binary reading can be
     *               potentially dangerous.
     * 
     */
    public class Init
    {
        // todo 3;
        public class Initialization<T> where T : class, new()
        {
            public Logging Logger { get; private set; }
            public T Configuration { get; private set; }
            // todo 3;
            public Initialization()
            {
                Logger = new Logging();
                Configuration = new T();

                try
                {
                    string xmlConfigFile = SerializationActions.Serializing.GetConfigFilePath<T>(Logger);
                    if (!File.Exists(xmlConfigFile))
                    {
                        SerializationActions.Serializing.CreateNewConfiguration<T>(Logger);
                    }

                    Configuration = SerializationActions.Serializing.LoadConfigFile<T>(Logger);
                }
                catch (Exception ex)
                {
                    Logger.Log("Error Initializing Config file.", ex);
                }
            }
            /**
             * Save the current configuration file located in the Initialization<T> class.
             * Keeps it OO and prevents from having multiple types of the same object
             * initalized.
             * 
             * Leaving Configuration as private set allows for the contents of the xml to
             * be updated, which means it will be capable of being serialized properly.
             * This prevents errors that could occur if a new object is accidentally 
             * assigned to the member class.
             */
            public void SaveConfiguration()
            {
                SerializationActions.Serializing.SaveConfigFile(Logger, Configuration);
            }
            // todo 3;
            public string GetFullAssemblyPath()
            {
                // todo 1; unsure if this is needed?
                return SerializationActions.Serializing.GetAssemblyNamePath<T>(Logger);
            }
        }
    }
}

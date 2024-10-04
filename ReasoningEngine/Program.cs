using System;
using DotNetEnv;
using ReasoningEngine.GraphFileHandling;
using ReasoningEngine.GraphAccess;
using DebugUtils;

namespace ReasoningEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load environment variables from the .env file
            Env.Load();

            // Get the data folder path from environment variables, or throw an exception if not set
            string dataFolderPath = Environment.GetEnvironmentVariable("DATA_FOLDER_PATH") 
                                    ?? throw new Exception("DATA_FOLDER_PATH is not set in the environment variables.");

            // Create an instance of GraphFileManager with the data folder path
            var graphFileManager = new GraphFileManager(dataFolderPath);

            // Create an instance of CommandProcessor with the GraphFileManager
            var commandProcessor = new CommandProcessor(graphFileManager);

            // Display the menu and handle user input
            ShowMenu(commandProcessor);
        }

        static void ShowMenu(CommandProcessor commandProcessor)
        {
            while (true)
            {
                DebugWriter.DebugWriteLine("#00D7D1#", "\nMain Menu:");
                DebugWriter.DebugWriteLine("#00D7D2#", "1. Run Setup");
                DebugWriter.DebugWriteLine("#00D7D8#", "2. Command Processor Options");
                DebugWriter.DebugWriteLine("#00D7D9#", "3. Debug Options");
                DebugWriter.DebugWriteLine("#00D7DA#", "0. Exit");
                DebugWriter.DebugWrite("#00D7D0#", "Enter option: ");

                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        OneTimeSetup.Initialize();
                        break;
                    case "2":
                        commandProcessor.ShowCommandProcessorMenu();
                        break;
                    case "3":
                        DebugOptions.ShowDebugOptionsMenu();
                        break;
                    case "0":
                        return;
                    default:
                        DebugWriter.DebugWriteLine("#00INV1#", "Invalid option. Please try again.");
                        break;
                }
            }
        }
    }
}
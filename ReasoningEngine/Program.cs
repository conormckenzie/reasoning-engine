﻿using System;
using DotNetEnv;
using ReasoningEngine.GraphFileHandling;
using ReasoningEngine.GraphAccess;
using ReasoningEngine.Utils.Scenarios;
using DebugUtils;
using System.IO;
using System.Threading.Tasks; // Added for async Main

namespace ReasoningEngine
{
    /// <summary>
    /// The main entry point for the Reasoning Engine application.
    /// Handles command-line arguments and the interactive console menu.
    /// </summary>
    class Program
    {
        private static List<MenuItem> mainMenuItems = new List<MenuItem>
        {
            new MenuItem("Run Setup", "#RNKA1C#", "setup"),
            new MenuItem("Graph Operations", "#D7SFN1#", "graph_operations"),
            new MenuItem("Debug Options", "#E1QTUA#", "debug_options"),
            new MenuItem("Start Web Server", "#WEB000#", "start_web_server"),
        };

        /// <summary>
        /// The main entry point of the application.
        /// Loads environment variables, initializes the graph components, and processes command-line arguments or shows the interactive menu.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        // Changed to async Task Main
        static async Task Main(string[] args)
        {
            // Try current directory first
            string currentDirectory = Directory.GetCurrentDirectory();
            string envPath = Path.Combine(currentDirectory, ".env");
            
            // If not found, try looking up from executable location
            if (!File.Exists(envPath))
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string solutionDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../"));
                envPath = Path.Combine(solutionDirectory, ".env");
            }
            
            DebugWriter.DebugWriteLine("#ENV001#", $"Looking for .env file at: {envPath}");
            
            if (!File.Exists(envPath))
            {
                throw new Exception($".env file not found at {envPath}. Please ensure the .env file exists in the project root directory.");
            }

            Env.Load(envPath);
            DebugWriter.DebugWriteLine("#ENV002#", "Loaded .env file successfully");

            string dataFolderPath = Environment.GetEnvironmentVariable("DATA_FOLDER_PATH") 
                                    ?? throw new Exception("DATA_FOLDER_PATH is not set in the environment variables.");

            // Instantiate the storage provider and the object mapper
            IGraphStorageProvider storageProvider = new FileGraphStorageProvider(dataFolderPath); 
            var graphObjectMapper = new GraphObjectMapper(storageProvider);
            var commandProcessor = new CommandProcessor(graphObjectMapper); // Pass mapper to processor
            var graphOperationsUserMenu = new GraphOperationsUserMenu(commandProcessor);

            // Check for command-line arguments
            if (args.Length > 0)
            {
                // Pass mapper instead of the old file manager
                // Await the async processing
                await ProcessCommandLineArguments(args, commandProcessor, graphObjectMapper).ConfigureAwait(false);
            }
            else
            {
                // No arguments provided, show interactive menu
                ShowMenu(commandProcessor, graphOperationsUserMenu);
            }
        }

        /// <summary>
        /// Processes command-line arguments to perform specific actions (setup, run scenario, list scenarios, show help).
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="commandProcessor">The CommandProcessor instance for executing graph commands.</param>
        /// <param name="graphObjectMapper">The GraphObjectMapper instance for scenario management.</param>
        // Updated signature to take GraphObjectMapper and return async Task
        static async Task ProcessCommandLineArguments(string[] args, CommandProcessor commandProcessor, GraphObjectMapper graphObjectMapper)
        {
            string command = args[0].ToLower();

            switch (command)
            {
                case "--setup":
                case "-s":
                    DebugWriter.DebugWriteLine("#RNFEUV#", "Running setup from command line");
                    OneTimeSetup.Initialize();
                    break;

                case "--run-scenario":
                case "-r":
                    if (args.Length < 2)
                    {
                        DebugWriter.DebugWriteLine("#PVBY9F#", "Error: Scenario name is required");
                        DebugWriter.DebugWriteLine("#3DUDG4#", "Usage: dotnet run --run-scenario <scenario-name> [--verbosity <level>]");
                        return;
                    }

                    string scenarioName = args[1].ToLower();
                    DebugUtils.VerbosityLevel? verbosity = null;
                    
                    // Check for verbosity parameter
                    for (int i = 2; i < args.Length; i++)
                    {
                        if ((args[i] == "--verbosity" || args[i] == "-v") && i + 1 < args.Length)
                        {
                            string verbosityArg = args[i + 1];
                            if (Enum.TryParse<DebugUtils.VerbosityLevel>(verbosityArg, true, out var parsedVerbosity))
                            {
                                verbosity = parsedVerbosity;
                                DebugWriter.DebugWriteLine("#6BE6ZN#", $"Setting verbosity to {verbosity}");
                            }
                            else
                            {
                                DebugWriter.DebugWriteLine("#F2B4PH#", $"Invalid verbosity level: {verbosityArg}");
                                DebugWriter.DebugWriteLine("#M9PEEV#", "Valid values are: Minimal, Normal, Detailed");
                            }
                            break;
                        }
                    }
                    
                    // ScenarioManager likely needs the mapper now
                    var scenarioManager = new ScenarioManager(commandProcessor, graphObjectMapper);
                    // Await the async scenario run
                    await scenarioManager.RunScenario(scenarioName, verbosity).ConfigureAwait(false);
                    break;

                case "--list-scenarios":
                case "-l":
                     // ScenarioManager likely needs the mapper now
                    var listManager = new ScenarioManager(commandProcessor, graphObjectMapper);
                    listManager.ListScenarios();
                    break;

                case "--help":
                case "-h":
                    ShowHelp();
                    break;

                default:
                    DebugWriter.DebugWriteLine("#69B28C#", $"Unknown command: {command}");
                    ShowHelp();
                    break;
            }
        }

        /// <summary>
        /// Displays the command-line usage help message.
        /// </summary>
        static void ShowHelp()
        {
            DebugWriter.DebugWriteLine("#L9GCJP#", "Reasoning Engine - Command Line Usage");
            DebugWriter.DebugWriteLine("#FNJOS0#", "------------------------------------");
            DebugWriter.DebugWriteLine("#CMD010#", "Usage: dotnet run [options]");
            DebugWriter.DebugWriteLine("#CMD011#", "");
            DebugWriter.DebugWriteLine("#CMD012#", "Options:");
            DebugWriter.DebugWriteLine("#CMD013#", "  --setup, -s                      Run one-time setup");
            DebugWriter.DebugWriteLine("#CMD014#", "  --run-scenario, -r <name>        Run a specific scenario");
            DebugWriter.DebugWriteLine("#CMD015#", "  --verbosity, -v <level>          Set verbosity level (Minimal, Normal, Detailed)");
            DebugWriter.DebugWriteLine("#CMD016#", "  --list-scenarios, -l             List available scenarios");
            DebugWriter.DebugWriteLine("#CMD017#", "  --help, -h                       Show this help message");
            DebugWriter.DebugWriteLine("#CMD018#", "");
            DebugWriter.DebugWriteLine("#CMD019#", "Examples:");
            DebugWriter.DebugWriteLine("#CMD020#", "  dotnet run --run-scenario weather");
            DebugWriter.DebugWriteLine("#CMD021#", "  dotnet run --run-scenario weather --verbosity Minimal");
            DebugWriter.DebugWriteLine("#CMD022#", "  (Recommended: Use --verbosity Minimal for scenarios to reduce output)");
            DebugWriter.DebugWriteLine("#CMD023#", "");
            DebugWriter.DebugWriteLine("#CMD024#", "If no options are provided, the interactive menu will be shown.");
        }

        /// <summary>
        /// Displays the main interactive console menu and handles user input.
        /// </summary>
        /// <param name="commandProcessor">The CommandProcessor instance for executing graph commands.</param>
        /// <param name="graphOperationsUserMenu">The GraphOperationsUserMenu instance for the graph operations submenu.</param>
        static void ShowMenu(CommandProcessor commandProcessor, GraphOperationsUserMenu graphOperationsUserMenu)
        {
            while (true)
            {
                DebugWriter.DebugWriteLine("#0D7D01#", "\nMain Menu:");

                for (int i = 0; i < mainMenuItems.Count; i++)
                {
                    DebugWriter.DebugWriteLine(mainMenuItems[i].DebugString, $"{i + 1}. {mainMenuItems[i].DisplayText}");
                }

                DebugWriter.DebugWriteLine("#0D7D00#", "0. Exit");

                DebugWriter.DebugWrite("#0D7E00#", "Enter option: ");
                var option = Console.ReadLine();

                if (option == "0")
                {
                    return;
                }
                else if (int.TryParse(option, out int selectedOption) && selectedOption > 0 && selectedOption <= mainMenuItems.Count)
                {
                    var selectedItem = mainMenuItems[selectedOption - 1];

                    switch (selectedItem.InternalText)
                    {
                        case "setup":
                            OneTimeSetup.Initialize();
                            break;
                        case "graph_operations":
                            graphOperationsUserMenu.ShowMenu();
                            break;
                        case "debug_options":
                            DebugOptions.ShowDebugOptionsMenu();
                            break;
                        case "start_web_server":
                            var webServer = new WebServer(commandProcessor);
                            webServer.Start();
                            break;
                        default:
                            DebugWriter.DebugWriteLine("#00INV1#", "Invalid option. Please try again.");
                            break;
                    }
                }
                else
                {
                    DebugWriter.DebugWriteLine("#00INV2#", "Invalid option. Please try again.");
                }
            }
        }
    }
}

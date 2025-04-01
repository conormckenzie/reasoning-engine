﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using DotNetEnv;
using ReasoningEngine.GraphFileHandling;
using ReasoningEngine.GraphAccess;
using ReasoningEngine.Utils.Scenarios;
using DebugUtils;
using System.IO;

namespace ReasoningEngine
{
    class Program
    {
        private static List<MenuItem> mainMenuItems = new List<MenuItem>
        {
            new MenuItem("Run Setup", "#RNKA1C#", "setup"),
            new MenuItem("Graph Operations", "#D7SFN1#", "graph_operations"),
            new MenuItem("Debug Options", "#E1QTUA#", "debug_options"),
            new MenuItem("Start Web Server", "#WEB000#", "start_web_server"),
        };

        static void Main(string[] args)
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

            var graphFileManager = new GraphFileManager(dataFolderPath);
            var commandProcessor = new CommandProcessor(graphFileManager);
            var graphOperationsUserMenu = new GraphOperationsUserMenu(commandProcessor);

            // Check for command-line arguments
            if (args.Length > 0)
            {
                ProcessCommandLineArguments(args, commandProcessor, graphFileManager);
            }
            else
            {
                // No arguments provided, show interactive menu
                ShowMenu(commandProcessor, graphOperationsUserMenu);
            }
        }

        static void ProcessCommandLineArguments(string[] args, CommandProcessor commandProcessor, GraphFileManager graphFileManager)
        {
            string command = args[0].ToLower();

            switch (command)
            {
                case "--setup":
                case "-s":
                    DebugWriter.DebugWriteLine("#CMD001#", "Running setup from command line");
                    OneTimeSetup.Initialize();
                    break;

                case "--run-scenario":
                case "-r":
                    if (args.Length < 2)
                    {
                        DebugWriter.DebugWriteLine("#CMD003#", "Error: Scenario name is required");
                        DebugWriter.DebugWriteLine("#CMD004#", "Usage: dotnet run --run-scenario <scenario-name> [--verbosity <level>]");
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
                                DebugWriter.DebugWriteLine("#CMD002#", $"Setting verbosity to {verbosity}");
                            }
                            else
                            {
                                DebugWriter.DebugWriteLine("#CMD005#", $"Invalid verbosity level: {verbosityArg}");
                                DebugWriter.DebugWriteLine("#CMD006#", "Valid values are: Minimal, Normal, Detailed");
                            }
                            break;
                        }
                    }
                    
                    var scenarioManager = new ScenarioManager(commandProcessor, graphFileManager);
                    scenarioManager.RunScenario(scenarioName, verbosity);
                    break;

                case "--list-scenarios":
                case "-l":
                    var listManager = new ScenarioManager(commandProcessor, graphFileManager);
                    listManager.ListScenarios();
                    break;

                case "--help":
                case "-h":
                    ShowHelp();
                    break;

                default:
                    DebugWriter.DebugWriteLine("#CMD007#", $"Unknown command: {command}");
                    ShowHelp();
                    break;
            }
        }

        static void ShowHelp()
        {
            DebugWriter.DebugWriteLine("#CMD008#", "Reasoning Engine - Command Line Usage");
            DebugWriter.DebugWriteLine("#CMD009#", "------------------------------------");
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
            DebugWriter.DebugWriteLine("#CMD022#", "");
            DebugWriter.DebugWriteLine("#CMD023#", "If no options are provided, the interactive menu will be shown.");
        }

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

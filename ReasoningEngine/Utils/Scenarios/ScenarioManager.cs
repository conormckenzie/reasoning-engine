using System;
using System.Collections.Generic;
using ReasoningEngine.GraphFileHandling;
using ReasoningEngine.GraphAccess;
using DebugUtils;

namespace ReasoningEngine.Utils.Scenarios
{
    /// <summary>
    /// Manages the execution of scenarios for the Reasoning Engine.
    /// </summary>
    public class ScenarioManager
    {
        private readonly CommandProcessor _commandProcessor;
        private readonly GraphFileManager _graphFileManager;

        public ScenarioManager(CommandProcessor commandProcessor, GraphFileManager graphFileManager)
        {
            _commandProcessor = commandProcessor;
            _graphFileManager = graphFileManager;
        }

        /// <summary>
        /// Lists all available scenarios.
        /// </summary>
        public void ListScenarios()
        {
            Console.WriteLine("Available scenarios:");
            Console.WriteLine("  weather    - Populate the graph with weather scenario data");
            // Add more scenarios here as they are created
            Console.WriteLine();
        }

        /// <summary>
        /// Runs a scenario by name.
        /// </summary>
        /// <param name="scenarioName">The name of the scenario to run.</param>
        /// <returns>True if the scenario was found and executed, false otherwise.</returns>
        public bool RunScenario(string scenarioName)
        {
            DebugWriter.DebugWriteLine("#SCN001#", $"Running scenario: {scenarioName}");

            switch (scenarioName.ToLower())
            {
                case "weather":
                    Console.WriteLine("Running Weather Scenario");
                    var weatherScenario = new PopulateWeatherScenarioData(_commandProcessor);
                    weatherScenario.PopulateData();
                    return true;

                // Add more scenarios here as needed

                default:
                    Console.WriteLine($"Unknown scenario: {scenarioName}");
                    ListScenarios();
                    return false;
            }
        }
    }
}

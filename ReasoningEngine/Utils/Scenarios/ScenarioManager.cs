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
        // Updated field to use GraphObjectMapper
        private readonly GraphObjectMapper _graphObjectMapper; 

        // Updated constructor to accept GraphObjectMapper
        public ScenarioManager(CommandProcessor commandProcessor, GraphObjectMapper graphObjectMapper) 
        {
            _commandProcessor = commandProcessor;
            _graphObjectMapper = graphObjectMapper; // Assign the mapper
        }

        /// <summary>
        /// Lists all available scenarios.
        /// </summary>
        public void ListScenarios()
        {
            DebugUtils.DebugWriter.DebugWriteLine("#SCN004#", "Available scenarios:");
            DebugUtils.DebugWriter.DebugWriteLine("#SCN005#", "  weather    - Populate the graph with weather scenario data (Recommend running with --verbosity Minimal)");
            // Add more scenarios here as they are created
            DebugUtils.DebugWriter.DebugWriteLine("#SCN006#", "");
        }

        /// <summary>
        /// Runs a scenario by name.
        /// </summary>
        /// <param name="scenarioName">The name of the scenario to run.</param>
        /// <param name="verbosity">Optional verbosity level for the scenario output. If not specified, uses the current global setting.</param>
        /// <returns>True if the scenario was found and executed, false otherwise.</returns>
        public bool RunScenario(string scenarioName, DebugUtils.VerbosityLevel? verbosity = null)
        {
            // Store the original verbosity level to restore it later
            var originalVerbosity = DebugUtils.DebugOptions.Verbosity;
            
            // Set the verbosity level for this scenario if specified
            if (verbosity.HasValue)
            {
                DebugUtils.DebugOptions.Verbosity = verbosity.Value;
                DebugWriter.DebugWriteLine("#SCN002#", $"Setting verbosity to {verbosity.Value} for this scenario");
            }
            
            DebugWriter.DebugWriteLine("#SCN001#", $"Running scenario: {scenarioName}");

            try
            {
                switch (scenarioName.ToLower())
                {
                    case "weather":
                        DebugUtils.DebugWriter.DebugWriteLine("#SCN007#", "Running Weather Scenario");
                        // Pass commandProcessor and the graphObjectMapper
                        var weatherScenario = new PopulateWeatherScenarioData(_commandProcessor, _graphObjectMapper); 
                        weatherScenario.PopulateData();
                        return true;

                    // Add more scenarios here as needed

                    default:
                        DebugUtils.DebugWriter.DebugWriteLine("#SCN008#", $"Unknown scenario: {scenarioName}");
                        ListScenarios();
                        return false;
                }
            }
            finally
            {
                // Restore the original verbosity level
                if (verbosity.HasValue)
                {
                    DebugUtils.DebugOptions.Verbosity = originalVerbosity;
                    DebugWriter.DebugWriteLine("#SCN003#", $"Restored verbosity to {originalVerbosity}");
                }
            }
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ReasoningEngine.GraphFileHandling;
using ReasoningEngine.GraphAccess;
using DotNetEnv;
using ReasoningEngine;

namespace ReasoningEngine.Utils.Scenarios
{
    /// <summary>
    /// This class populates the reasoning engine with data for the weather scenario.
    /// </summary>
    public class PopulateWeatherScenarioData
    {
        private readonly CommandProcessor _commandProcessor;

        public PopulateWeatherScenarioData(CommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;
        }

        public void PopulateData()
        {
            DebugUtils.DebugWriter.DebugWriteLine("#WS001#", "Starting to populate weather scenario data...", true, DebugUtils.VerbosityLevel.Minimal);
            
            // Add SIMO nodes
            AddSIMONodes();
            
            // Add MISO nodes
            AddMISONodes();
            
            // Add edges
            AddEdges();
            
            // Add probability distributions
            AddProbabilityDistributions();
            
            DebugUtils.DebugWriter.DebugWriteLine("#WS002#", "Weather scenario data population completed.", true, DebugUtils.VerbosityLevel.Minimal);
        }

        private void AddSIMONodes()
        {
            DebugUtils.DebugWriter.DebugWriteLine("#WS003#", "Adding SIMO nodes...", true, DebugUtils.VerbosityLevel.Normal);
            
            // Basic Propositions (Truth Domain)
            string result1 = _commandProcessor.ProcessCommand("add_node", "1|Proposition A: It is raining outside [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#WS004#", result1, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result2 = _commandProcessor.ProcessCommand("add_node", "2|Proposition B: The ground is wet [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#WS005#", result2, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Results of Logical Operations
            string result4 = _commandProcessor.ProcessCommand("add_node", "4|Result of AND: It is raining AND the ground is wet [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#WS006#", result4, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result6 = _commandProcessor.ProcessCommand("add_node", "6|Result of OR: It is raining OR the ground is wet [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#WS007#", result6, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result8 = _commandProcessor.ProcessCommand("add_node", "8|Result of NOT: It is NOT raining outside [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#WS008#", result8, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Causal Strength (Continuous Range Domain)
            string result9 = _commandProcessor.ProcessCommand("add_node", "9|Causal Strength of Rain → Wet Ground [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|ContinuousRange");
            DebugUtils.DebugWriter.DebugWriteLine("#WS009#", result9, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result11 = _commandProcessor.ProcessCommand("add_node", "11|Result of Implication: If it is raining, then the ground is wet [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#WS010#", result11, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Continuous and Discrete Concepts
            string result12 = _commandProcessor.ProcessCommand("add_node", "12|Rain Intensity in mm/hour [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|ContinuousRange");
            DebugUtils.DebugWriter.DebugWriteLine("#WS011#", result12, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result13 = _commandProcessor.ProcessCommand("add_node", "13|Wind Force according to Beaufort scale [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|DiscreteInteger");
            DebugUtils.DebugWriter.DebugWriteLine("#WS012#", result13, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result15 = _commandProcessor.ProcessCommand("add_node", "15|Puddle Formation Likelihood [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#WS013#", result15, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result17 = _commandProcessor.ProcessCommand("add_node", "17|Wind Impact on Umbrella [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#WS014#", result17, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Complex Reasoning Chain
            string result19 = _commandProcessor.ProcessCommand("add_node", "19|Is it raining heavily? [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#WS015#", result19, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result21 = _commandProcessor.ProcessCommand("add_node", "21|Is an umbrella usable? [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#WS016#", result21, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result23 = _commandProcessor.ProcessCommand("add_node", "23|Need for Umbrella [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#WS017#", result23, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Alternative Reasoning Path
            string result25 = _commandProcessor.ProcessCommand("add_node", "25|Ground wet because of rain [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#WS018#", result25, true, DebugUtils.VerbosityLevel.Detailed);
        }

        private void AddMISONodes()
        {
            DebugUtils.DebugWriter.DebugWriteLine("#WS019#", "Adding MISO nodes...", true, DebugUtils.VerbosityLevel.Normal);
            
            // Logical Operations
            string result3 = _commandProcessor.ProcessCommand("add_node", "3|AND Operation: Logical AND of inputs [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#WS020#", result3, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result5 = _commandProcessor.ProcessCommand("add_node", "5|OR Operation: Logical OR of inputs [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#WS021#", result5, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result7 = _commandProcessor.ProcessCommand("add_node", "7|NOT Operation: Logical NOT of input [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#WS022#", result7, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result10 = _commandProcessor.ProcessCommand("add_node", "10|Implication Evaluation: Evaluates if A implies B [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#WS023#", result10, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Analysis Nodes
            string result14 = _commandProcessor.ProcessCommand("add_node", "14|Puddle Formation Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#WS024#", result14, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result16 = _commandProcessor.ProcessCommand("add_node", "16|Wind Impact Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#WS025#", result16, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Complex Reasoning Chain
            string result18 = _commandProcessor.ProcessCommand("add_node", "18|Heavy Rain Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#WS026#", result18, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result20 = _commandProcessor.ProcessCommand("add_node", "20|Umbrella Usability Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#WS027#", result20, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result22 = _commandProcessor.ProcessCommand("add_node", "22|Umbrella Need Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#WS028#", result22, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Alternative Reasoning Path
            string result24 = _commandProcessor.ProcessCommand("add_node", "24|Reverse Implication Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#WS029#", result24, true, DebugUtils.VerbosityLevel.Detailed);
        }

        private void AddEdges()
        {
            DebugUtils.DebugWriter.DebugWriteLine("#WS030#", "Adding edges...", true, DebugUtils.VerbosityLevel.Normal);
            
            // Connect AND Operation
            string resultEdge1 = _commandProcessor.ProcessCommand("add_edge", "1|3|1.0|Input to AND operation");
            DebugUtils.DebugWriter.DebugWriteLine("#WS031#", resultEdge1, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge2 = _commandProcessor.ProcessCommand("add_edge", "2|3|1.0|Input to AND operation");
            DebugUtils.DebugWriter.DebugWriteLine("#WS032#", resultEdge2, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge3 = _commandProcessor.ProcessCommand("add_edge", "3|4|1.0|Output of AND operation");
            DebugUtils.DebugWriter.DebugWriteLine("#WS033#", resultEdge3, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect OR Operation
            string resultEdge4 = _commandProcessor.ProcessCommand("add_edge", "1|5|1.0|Input to OR operation");
            DebugUtils.DebugWriter.DebugWriteLine("#WS034#", resultEdge4, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge5 = _commandProcessor.ProcessCommand("add_edge", "2|5|1.0|Input to OR operation");
            DebugUtils.DebugWriter.DebugWriteLine("#WS035#", resultEdge5, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge6 = _commandProcessor.ProcessCommand("add_edge", "5|6|1.0|Output of OR operation");
            DebugUtils.DebugWriter.DebugWriteLine("#WS036#", resultEdge6, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect NOT Operation
            string resultEdge7 = _commandProcessor.ProcessCommand("add_edge", "1|7|1.0|Input to NOT operation");
            DebugUtils.DebugWriter.DebugWriteLine("#WS037#", resultEdge7, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge8 = _commandProcessor.ProcessCommand("add_edge", "7|8|1.0|Output of NOT operation");
            DebugUtils.DebugWriter.DebugWriteLine("#WS038#", resultEdge8, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect Implication Evaluation
            string resultEdge9 = _commandProcessor.ProcessCommand("add_edge", "1|10|1.0|Input to implication evaluation");
            DebugUtils.DebugWriter.DebugWriteLine("#WS039#", resultEdge9, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge10 = _commandProcessor.ProcessCommand("add_edge", "2|10|1.0|Input to implication evaluation");
            DebugUtils.DebugWriter.DebugWriteLine("#WS040#", resultEdge10, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge11 = _commandProcessor.ProcessCommand("add_edge", "9|10|1.0|Causal strength input to implication evaluation");
            DebugUtils.DebugWriter.DebugWriteLine("#WS041#", resultEdge11, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge12 = _commandProcessor.ProcessCommand("add_edge", "10|11|1.0|Output of implication evaluation");
            DebugUtils.DebugWriter.DebugWriteLine("#WS042#", resultEdge12, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect Puddle Formation Analysis
            string resultEdge13 = _commandProcessor.ProcessCommand("add_edge", "12|14|1.0|Rain intensity input to puddle formation analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WS043#", resultEdge13, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge14 = _commandProcessor.ProcessCommand("add_edge", "14|15|1.0|Output of puddle formation analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WS044#", resultEdge14, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect Wind Impact Analysis
            string resultEdge15 = _commandProcessor.ProcessCommand("add_edge", "13|16|1.0|Wind force input to wind impact analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WS045#", resultEdge15, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge16 = _commandProcessor.ProcessCommand("add_edge", "16|17|1.0|Output of wind impact analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WS046#", resultEdge16, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect Heavy Rain Analysis
            string resultEdge17 = _commandProcessor.ProcessCommand("add_edge", "12|18|1.0|Rain intensity input to heavy rain analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WS047#", resultEdge17, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge18 = _commandProcessor.ProcessCommand("add_edge", "18|19|1.0|Output of heavy rain analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WS048#", resultEdge18, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect Umbrella Usability Analysis
            string resultEdge19 = _commandProcessor.ProcessCommand("add_edge", "13|20|1.0|Wind force input to umbrella usability analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WS049#", resultEdge19, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge20 = _commandProcessor.ProcessCommand("add_edge", "20|21|1.0|Output of umbrella usability analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WS050#", resultEdge20, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect Umbrella Need Analysis
            string resultEdge21 = _commandProcessor.ProcessCommand("add_edge", "1|22|1.0|Rain status input to umbrella need analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WS051#", resultEdge21, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge22 = _commandProcessor.ProcessCommand("add_edge", "21|22|1.0|Umbrella usability input to umbrella need analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WS052#", resultEdge22, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge23 = _commandProcessor.ProcessCommand("add_edge", "22|23|1.0|Output of umbrella need analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WS053#", resultEdge23, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect Reverse Implication Analysis
            string resultEdge24 = _commandProcessor.ProcessCommand("add_edge", "1|24|1.0|Rain status input to reverse implication analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WS054#", resultEdge24, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge25 = _commandProcessor.ProcessCommand("add_edge", "2|24|1.0|Ground wetness input to reverse implication analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WS055#", resultEdge25, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge26 = _commandProcessor.ProcessCommand("add_edge", "9|24|1.0|Causal strength input to reverse implication analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WS056#", resultEdge26, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge27 = _commandProcessor.ProcessCommand("add_edge", "24|25|1.0|Output of reverse implication analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WS057#", resultEdge27, true, DebugUtils.VerbosityLevel.Detailed);
        }

        // Method to add probability distributions to SIMO nodes
        private void AddProbabilityDistributions()
        {
            DebugUtils.DebugWriter.DebugWriteLine("#WS058#", "Adding probability distributions to SIMO nodes...", true, DebugUtils.VerbosityLevel.Normal);
            
            try
            {
                // Load nodes from the graph file manager
                var nodes = LoadSIMONodes();
                
                if (nodes.Count == 0)
                {
                    DebugUtils.DebugWriter.DebugWriteLine("#WS059#", "No SIMO nodes found. Make sure to add nodes first.", true, DebugUtils.VerbosityLevel.Minimal);
                    return;
                }
                
                // Add probability distributions to each node based on its ID
                foreach (var node in nodes)
                {
                    switch (node.Id)
                    {
                        case 1: // Proposition A: It is raining
                            AddTruthDistribution(node, 0.7);
                            break;
                            
                        case 2: // Proposition B: The ground is wet
                            AddTruthDistribution(node, 0.8);
                            break;
                            
                        case 4: // Result of AND
                            AddTruthDistribution(node, 0.56);
                            break;
                            
                        case 6: // Result of OR
                            AddTruthDistribution(node, 0.94);
                            break;
                            
                        case 8: // Result of NOT
                            AddTruthDistribution(node, 0.3);
                            break;
                            
                        case 9: // Causal Strength of Rain → Wet Ground
                            AddCausalStrengthDistribution(node);
                            break;
                            
                        case 11: // Result of Implication
                            AddTruthDistribution(node, 0.85);
                            break;
                            
                        case 12: // Rain Intensity
                            AddRainIntensityDistribution(node);
                            break;
                            
                        case 13: // Wind Force
                            AddWindForceDistribution(node);
                            break;
                            
                        case 15: // Puddle Formation Likelihood
                            AddTruthDistribution(node, 0.3);
                            break;
                            
                        case 17: // Wind Impact on Umbrella
                            AddTruthDistribution(node, 0.35);
                            break;
                            
                        case 19: // Is it raining heavily?
                            AddTruthDistribution(node, 0.1);
                            break;
                            
                        case 21: // Is an umbrella usable?
                            AddTruthDistribution(node, 0.65);
                            break;
                            
                        case 23: // Need for Umbrella
                            AddTruthDistribution(node, 0.455);
                            break;
                            
                        case 25: // Ground wet because of rain
                            AddTruthDistribution(node, 0.65);
                            break;
                            
                        default:
                            DebugUtils.DebugWriter.DebugWriteLine("#WS060#", $"No distribution defined for node {node.Id}", true, DebugUtils.VerbosityLevel.Detailed);
                            break;
                    }
                }
                
                DebugUtils.DebugWriter.DebugWriteLine("#WS061#", "Probability distributions added successfully.", true, DebugUtils.VerbosityLevel.Normal);
            }
            catch (Exception ex)
            {
                DebugUtils.DebugWriter.DebugWriteLine("#WS062#", $"Error adding probability distributions: {ex.Message}", true, DebugUtils.VerbosityLevel.Minimal);
                DebugUtils.DebugWriter.DebugWriteLine("#WS063#", ex.StackTrace, true, DebugUtils.VerbosityLevel.Detailed);
            }
        }
        
        private List<SIMONode> LoadSIMONodes()
        {
            var result = new List<SIMONode>();
            
            // This is a placeholder. In a real implementation, we would:
            // 1. Get all node IDs from the graph file manager
            // 2. Load each node
            // 3. Filter for SIMO nodes
            // 4. Return the list of SIMO nodes
            
            DebugUtils.DebugWriter.DebugWriteLine("#WS064#", "Note: LoadSIMONodes is a placeholder. In a real implementation, we would load nodes from the graph file manager.", true, DebugUtils.VerbosityLevel.Normal);
            
            return result;
        }
        
        private void AddTruthDistribution(SIMONode node, double trueValue)
        {
            DebugUtils.DebugWriter.DebugWriteLine("#WS065#", $"Adding Truth distribution to node {node.Id}: True={trueValue}, False={1-trueValue}", true, DebugUtils.VerbosityLevel.Detailed);
            
            // In a real implementation, we would:
            // node.AddDistributionPoint(1.0, trueValue);
            // node.AddDistributionPoint(0.0, 1.0 - trueValue);
        }
        
        private void AddCausalStrengthDistribution(SIMONode node)
        {
            DebugUtils.DebugWriter.DebugWriteLine("#WS066#", $"Adding Causal Strength distribution to node {node.Id}", true, DebugUtils.VerbosityLevel.Detailed);
            
            // In a real implementation, we would:
            // node.AddDistributionRange(0.7, 0.9, 0.6);  // Strong causation
            // node.AddDistributionRange(0.4, 0.7, 0.3);  // Moderate causation
            // node.AddDistributionRange(0.0, 0.4, 0.1);  // Weak causation
        }
        
        private void AddRainIntensityDistribution(SIMONode node)
        {
            DebugUtils.DebugWriter.DebugWriteLine("#WS067#", $"Adding Rain Intensity distribution to node {node.Id}", true, DebugUtils.VerbosityLevel.Detailed);
            
            // In a real implementation, we would:
            // node.AddDistributionRange(0.0, 1.0, 0.3);   // No/trace rain
            // node.AddDistributionRange(1.0, 5.0, 0.4);   // Light rain
            // node.AddDistributionRange(5.0, 15.0, 0.2);  // Moderate rain
            // node.AddDistributionRange(15.0, 50.0, 0.1); // Heavy rain
        }
        
        private void AddWindForceDistribution(SIMONode node)
        {
            DebugUtils.DebugWriter.DebugWriteLine("#WS068#", $"Adding Wind Force distribution to node {node.Id}", true, DebugUtils.VerbosityLevel.Detailed);
            
            // In a real implementation, we would:
            // node.AddDistributionPoint(0, 0.05); // Calm
            // node.AddDistributionPoint(1, 0.10); // Light air
            // node.AddDistributionPoint(2, 0.20); // Light breeze
            // node.AddDistributionPoint(3, 0.30); // Gentle breeze
            // node.AddDistributionPoint(4, 0.20); // Moderate breeze
            // node.AddDistributionPoint(5, 0.10); // Fresh breeze
            // node.AddDistributionPoint(6, 0.05); // Strong breeze
        }

        public static void Main(string[] args)
        {
            try
            {
                // Load environment variables
                Env.Load();
                string dataFolderPath = Environment.GetEnvironmentVariable("DATA_FOLDER_PATH") 
                                       ?? throw new Exception("DATA_FOLDER_PATH is not set in the environment variables.");
                
                DebugUtils.DebugWriter.DebugWriteLine("#WS069#", $"Using data folder path: {dataFolderPath}", true, DebugUtils.VerbosityLevel.Normal);
                
                // Initialize GraphFileManager and CommandProcessor
                var graphFileManager = new GraphFileManager(dataFolderPath);
                var commandProcessor = new CommandProcessor(graphFileManager);
                
                // Run OneTimeSetup to ensure the data directory is properly initialized
                OneTimeSetup.Initialize();
                
                // Create and run the data populator
                var populator = new PopulateWeatherScenarioData(commandProcessor);
                populator.PopulateData();
                
                DebugUtils.DebugWriter.DebugWriteLine("#WS070#", "Weather scenario data population completed successfully.", true, DebugUtils.VerbosityLevel.Minimal);
            }
            catch (Exception ex)
            {
                DebugUtils.DebugWriter.DebugWriteLine("#WS071#", $"Error in Main: {ex.Message}", true, DebugUtils.VerbosityLevel.Minimal);
                DebugUtils.DebugWriter.DebugWriteLine("#WS072#", ex.StackTrace, true, DebugUtils.VerbosityLevel.Detailed);
            }
        }
    }
}

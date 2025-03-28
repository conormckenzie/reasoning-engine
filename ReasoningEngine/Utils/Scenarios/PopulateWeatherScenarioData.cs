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
            Console.WriteLine("Starting to populate weather scenario data...");
            
            // Add SIMO nodes
            AddSIMONodes();
            
            // Add MISO nodes
            AddMISONodes();
            
            // Add edges
            AddEdges();
            
            // Add probability distributions
            AddProbabilityDistributions();
            
            Console.WriteLine("Weather scenario data population completed.");
        }

        private void AddSIMONodes()
        {
            Console.WriteLine("Adding SIMO nodes...");
            
            // Basic Propositions (Truth Domain)
            string result1 = _commandProcessor.ProcessCommand("add_node", "1|Proposition A: It is raining outside [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            Console.WriteLine(result1);
            
            string result2 = _commandProcessor.ProcessCommand("add_node", "2|Proposition B: The ground is wet [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            Console.WriteLine(result2);
            
            // Results of Logical Operations
            string result4 = _commandProcessor.ProcessCommand("add_node", "4|Result of AND: It is raining AND the ground is wet [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            Console.WriteLine(result4);
            
            string result6 = _commandProcessor.ProcessCommand("add_node", "6|Result of OR: It is raining OR the ground is wet [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            Console.WriteLine(result6);
            
            string result8 = _commandProcessor.ProcessCommand("add_node", "8|Result of NOT: It is NOT raining outside [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            Console.WriteLine(result8);
            
            // Causal Strength (Continuous Range Domain)
            string result9 = _commandProcessor.ProcessCommand("add_node", "9|Causal Strength of Rain → Wet Ground [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|ContinuousRange");
            Console.WriteLine(result9);
            
            string result11 = _commandProcessor.ProcessCommand("add_node", "11|Result of Implication: If it is raining, then the ground is wet [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            Console.WriteLine(result11);
            
            // Continuous and Discrete Concepts
            string result12 = _commandProcessor.ProcessCommand("add_node", "12|Rain Intensity in mm/hour [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|ContinuousRange");
            Console.WriteLine(result12);
            
            string result13 = _commandProcessor.ProcessCommand("add_node", "13|Wind Force according to Beaufort scale [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|DiscreteInteger");
            Console.WriteLine(result13);
            
            string result15 = _commandProcessor.ProcessCommand("add_node", "15|Puddle Formation Likelihood [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            Console.WriteLine(result15);
            
            string result17 = _commandProcessor.ProcessCommand("add_node", "17|Wind Impact on Umbrella [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            Console.WriteLine(result17);
            
            // Complex Reasoning Chain
            string result19 = _commandProcessor.ProcessCommand("add_node", "19|Is it raining heavily? [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            Console.WriteLine(result19);
            
            string result21 = _commandProcessor.ProcessCommand("add_node", "21|Is an umbrella usable? [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            Console.WriteLine(result21);
            
            string result23 = _commandProcessor.ProcessCommand("add_node", "23|Need for Umbrella [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            Console.WriteLine(result23);
            
            // Alternative Reasoning Path
            string result25 = _commandProcessor.ProcessCommand("add_node", "25|Ground wet because of rain [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            Console.WriteLine(result25);
        }

        private void AddMISONodes()
        {
            Console.WriteLine("Adding MISO nodes...");
            
            // Logical Operations
            string result3 = _commandProcessor.ProcessCommand("add_node", "3|AND Operation: Logical AND of inputs [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            Console.WriteLine(result3);
            
            string result5 = _commandProcessor.ProcessCommand("add_node", "5|OR Operation: Logical OR of inputs [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            Console.WriteLine(result5);
            
            string result7 = _commandProcessor.ProcessCommand("add_node", "7|NOT Operation: Logical NOT of input [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            Console.WriteLine(result7);
            
            string result10 = _commandProcessor.ProcessCommand("add_node", "10|Implication Evaluation: Evaluates if A implies B [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            Console.WriteLine(result10);
            
            // Analysis Nodes
            string result14 = _commandProcessor.ProcessCommand("add_node", "14|Puddle Formation Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            Console.WriteLine(result14);
            
            string result16 = _commandProcessor.ProcessCommand("add_node", "16|Wind Impact Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            Console.WriteLine(result16);
            
            // Complex Reasoning Chain
            string result18 = _commandProcessor.ProcessCommand("add_node", "18|Heavy Rain Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            Console.WriteLine(result18);
            
            string result20 = _commandProcessor.ProcessCommand("add_node", "20|Umbrella Usability Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            Console.WriteLine(result20);
            
            string result22 = _commandProcessor.ProcessCommand("add_node", "22|Umbrella Need Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            Console.WriteLine(result22);
            
            // Alternative Reasoning Path
            string result24 = _commandProcessor.ProcessCommand("add_node", "24|Reverse Implication Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            Console.WriteLine(result24);
        }

        private void AddEdges()
        {
            Console.WriteLine("Adding edges...");
            
            // Connect AND Operation
            string resultEdge1 = _commandProcessor.ProcessCommand("add_edge", "1|3|1.0|Input to AND operation");
            Console.WriteLine(resultEdge1);
            
            string resultEdge2 = _commandProcessor.ProcessCommand("add_edge", "2|3|1.0|Input to AND operation");
            Console.WriteLine(resultEdge2);
            
            string resultEdge3 = _commandProcessor.ProcessCommand("add_edge", "3|4|1.0|Output of AND operation");
            Console.WriteLine(resultEdge3);
            
            // Connect OR Operation
            string resultEdge4 = _commandProcessor.ProcessCommand("add_edge", "1|5|1.0|Input to OR operation");
            Console.WriteLine(resultEdge4);
            
            string resultEdge5 = _commandProcessor.ProcessCommand("add_edge", "2|5|1.0|Input to OR operation");
            Console.WriteLine(resultEdge5);
            
            string resultEdge6 = _commandProcessor.ProcessCommand("add_edge", "5|6|1.0|Output of OR operation");
            Console.WriteLine(resultEdge6);
            
            // Connect NOT Operation
            string resultEdge7 = _commandProcessor.ProcessCommand("add_edge", "1|7|1.0|Input to NOT operation");
            Console.WriteLine(resultEdge7);
            
            string resultEdge8 = _commandProcessor.ProcessCommand("add_edge", "7|8|1.0|Output of NOT operation");
            Console.WriteLine(resultEdge8);
            
            // Connect Implication Evaluation
            string resultEdge9 = _commandProcessor.ProcessCommand("add_edge", "1|10|1.0|Input to implication evaluation");
            Console.WriteLine(resultEdge9);
            
            string resultEdge10 = _commandProcessor.ProcessCommand("add_edge", "2|10|1.0|Input to implication evaluation");
            Console.WriteLine(resultEdge10);
            
            string resultEdge11 = _commandProcessor.ProcessCommand("add_edge", "9|10|1.0|Causal strength input to implication evaluation");
            Console.WriteLine(resultEdge11);
            
            string resultEdge12 = _commandProcessor.ProcessCommand("add_edge", "10|11|1.0|Output of implication evaluation");
            Console.WriteLine(resultEdge12);
            
            // Connect Puddle Formation Analysis
            string resultEdge13 = _commandProcessor.ProcessCommand("add_edge", "12|14|1.0|Rain intensity input to puddle formation analysis");
            Console.WriteLine(resultEdge13);
            
            string resultEdge14 = _commandProcessor.ProcessCommand("add_edge", "14|15|1.0|Output of puddle formation analysis");
            Console.WriteLine(resultEdge14);
            
            // Connect Wind Impact Analysis
            string resultEdge15 = _commandProcessor.ProcessCommand("add_edge", "13|16|1.0|Wind force input to wind impact analysis");
            Console.WriteLine(resultEdge15);
            
            string resultEdge16 = _commandProcessor.ProcessCommand("add_edge", "16|17|1.0|Output of wind impact analysis");
            Console.WriteLine(resultEdge16);
            
            // Connect Heavy Rain Analysis
            string resultEdge17 = _commandProcessor.ProcessCommand("add_edge", "12|18|1.0|Rain intensity input to heavy rain analysis");
            Console.WriteLine(resultEdge17);
            
            string resultEdge18 = _commandProcessor.ProcessCommand("add_edge", "18|19|1.0|Output of heavy rain analysis");
            Console.WriteLine(resultEdge18);
            
            // Connect Umbrella Usability Analysis
            string resultEdge19 = _commandProcessor.ProcessCommand("add_edge", "13|20|1.0|Wind force input to umbrella usability analysis");
            Console.WriteLine(resultEdge19);
            
            string resultEdge20 = _commandProcessor.ProcessCommand("add_edge", "20|21|1.0|Output of umbrella usability analysis");
            Console.WriteLine(resultEdge20);
            
            // Connect Umbrella Need Analysis
            string resultEdge21 = _commandProcessor.ProcessCommand("add_edge", "1|22|1.0|Rain status input to umbrella need analysis");
            Console.WriteLine(resultEdge21);
            
            string resultEdge22 = _commandProcessor.ProcessCommand("add_edge", "21|22|1.0|Umbrella usability input to umbrella need analysis");
            Console.WriteLine(resultEdge22);
            
            string resultEdge23 = _commandProcessor.ProcessCommand("add_edge", "22|23|1.0|Output of umbrella need analysis");
            Console.WriteLine(resultEdge23);
            
            // Connect Reverse Implication Analysis
            string resultEdge24 = _commandProcessor.ProcessCommand("add_edge", "1|24|1.0|Rain status input to reverse implication analysis");
            Console.WriteLine(resultEdge24);
            
            string resultEdge25 = _commandProcessor.ProcessCommand("add_edge", "2|24|1.0|Ground wetness input to reverse implication analysis");
            Console.WriteLine(resultEdge25);
            
            string resultEdge26 = _commandProcessor.ProcessCommand("add_edge", "9|24|1.0|Causal strength input to reverse implication analysis");
            Console.WriteLine(resultEdge26);
            
            string resultEdge27 = _commandProcessor.ProcessCommand("add_edge", "24|25|1.0|Output of reverse implication analysis");
            Console.WriteLine(resultEdge27);
        }

        // Method to add probability distributions to SIMO nodes
        private void AddProbabilityDistributions()
        {
            Console.WriteLine("Adding probability distributions to SIMO nodes...");
            
            try
            {
                // Load nodes from the graph file manager
                var nodes = LoadSIMONodes();
                
                if (nodes.Count == 0)
                {
                    Console.WriteLine("No SIMO nodes found. Make sure to add nodes first.");
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
                            Console.WriteLine($"No distribution defined for node {node.Id}");
                            break;
                    }
                }
                
                Console.WriteLine("Probability distributions added successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding probability distributions: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
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
            
            Console.WriteLine("Note: LoadSIMONodes is a placeholder. In a real implementation, we would load nodes from the graph file manager.");
            
            return result;
        }
        
        private void AddTruthDistribution(SIMONode node, double trueValue)
        {
            Console.WriteLine($"Adding Truth distribution to node {node.Id}: True={trueValue}, False={1-trueValue}");
            
            // In a real implementation, we would:
            // node.AddDistributionPoint(1.0, trueValue);
            // node.AddDistributionPoint(0.0, 1.0 - trueValue);
        }
        
        private void AddCausalStrengthDistribution(SIMONode node)
        {
            Console.WriteLine($"Adding Causal Strength distribution to node {node.Id}");
            
            // In a real implementation, we would:
            // node.AddDistributionRange(0.7, 0.9, 0.6);  // Strong causation
            // node.AddDistributionRange(0.4, 0.7, 0.3);  // Moderate causation
            // node.AddDistributionRange(0.0, 0.4, 0.1);  // Weak causation
        }
        
        private void AddRainIntensityDistribution(SIMONode node)
        {
            Console.WriteLine($"Adding Rain Intensity distribution to node {node.Id}");
            
            // In a real implementation, we would:
            // node.AddDistributionRange(0.0, 1.0, 0.3);   // No/trace rain
            // node.AddDistributionRange(1.0, 5.0, 0.4);   // Light rain
            // node.AddDistributionRange(5.0, 15.0, 0.2);  // Moderate rain
            // node.AddDistributionRange(15.0, 50.0, 0.1); // Heavy rain
        }
        
        private void AddWindForceDistribution(SIMONode node)
        {
            Console.WriteLine($"Adding Wind Force distribution to node {node.Id}");
            
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
            // Load environment variables
            Env.Load();
            string dataFolderPath = Environment.GetEnvironmentVariable("DATA_FOLDER_PATH") 
                                   ?? throw new Exception("DATA_FOLDER_PATH is not set in the environment variables.");
            
            // Initialize GraphFileManager and CommandProcessor
            var graphFileManager = new GraphFileManager(dataFolderPath);
            var commandProcessor = new CommandProcessor(graphFileManager);
            
            // Run OneTimeSetup to ensure the data directory is properly initialized
            OneTimeSetup.Initialize();
            
            // Create and run the data populator
            var populator = new PopulateWeatherScenarioData(commandProcessor);
            populator.PopulateData();
        }
    }
}

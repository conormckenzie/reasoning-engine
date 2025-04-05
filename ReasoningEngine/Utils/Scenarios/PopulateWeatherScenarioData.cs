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
        // Updated field to use GraphObjectMapper
        private readonly GraphObjectMapper _graphObjectMapper; 

        // Updated constructor to accept GraphObjectMapper
        public PopulateWeatherScenarioData(CommandProcessor commandProcessor, GraphObjectMapper graphObjectMapper) 
        {
            _commandProcessor = commandProcessor;
            _graphObjectMapper = graphObjectMapper; // Assign the mapper
        }

        public void PopulateData()
        {
            DebugUtils.DebugWriter.DebugWriteLine("#Y8Z68I#", "Starting to populate weather scenario data...", true, DebugUtils.VerbosityLevel.Minimal);
            
            // Add SIMO nodes
            AddSIMONodes();
            
            // Add MISO nodes
            AddMISONodes();
            
            // Add edges
            AddEdges();
            
            // Add probability distributions
            AddProbabilityDistributions();
            
            DebugUtils.DebugWriter.DebugWriteLine("#TG4PZN#", "Weather scenario data population completed.", true, DebugUtils.VerbosityLevel.Minimal);
        }

        private void AddSIMONodes()
        {
            DebugUtils.DebugWriter.DebugWriteLine("#TR4P8P#", "Adding SIMO nodes...", true, DebugUtils.VerbosityLevel.Normal);
            
            // Basic Propositions (Truth Domain)
            string result1 = _commandProcessor.ProcessCommand("add_node", "1|Proposition A: It is raining outside [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#4AZGY7#", result1, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result2 = _commandProcessor.ProcessCommand("add_node", "2|Proposition B: The ground is wet [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#8ZLN4I#", result2, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Results of Logical Operations
            string result4 = _commandProcessor.ProcessCommand("add_node", "4|Result of AND: It is raining AND the ground is wet [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#5BI68R#", result4, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result6 = _commandProcessor.ProcessCommand("add_node", "6|Result of OR: It is raining OR the ground is wet [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#0I13I8#", result6, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result8 = _commandProcessor.ProcessCommand("add_node", "8|Result of NOT: It is NOT raining outside [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#3DQW3J#", result8, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Causal Strength (Continuous Range Domain)
            string result9 = _commandProcessor.ProcessCommand("add_node", "9|Causal Strength of Rain → Wet Ground [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|ContinuousRange");
            DebugUtils.DebugWriter.DebugWriteLine("#PSHWNG#", result9, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result11 = _commandProcessor.ProcessCommand("add_node", "11|Result of Implication: If it is raining, then the ground is wet [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#QXERWQ#", result11, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Continuous and Discrete Concepts
            string result12 = _commandProcessor.ProcessCommand("add_node", "12|Rain Intensity in mm/hour [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|ContinuousRange");
            DebugUtils.DebugWriter.DebugWriteLine("#L6TZP0#", result12, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result13 = _commandProcessor.ProcessCommand("add_node", "13|Wind Force according to Beaufort scale [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|DiscreteInteger");
            DebugUtils.DebugWriter.DebugWriteLine("#G7417P#", result13, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result15 = _commandProcessor.ProcessCommand("add_node", "15|Puddle Formation Likelihood [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#TMUMHC#", result15, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result17 = _commandProcessor.ProcessCommand("add_node", "17|Wind Impact on Umbrella [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#FN3RYC#", result17, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Complex Reasoning Chain
            string result19 = _commandProcessor.ProcessCommand("add_node", "19|Is it raining heavily? [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#BT7KDI#", result19, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result21 = _commandProcessor.ProcessCommand("add_node", "21|Is an umbrella usable? [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#0BPU33#", result21, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result23 = _commandProcessor.ProcessCommand("add_node", "23|Need for Umbrella [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#3VT2A9#", result23, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Alternative Reasoning Path
            string result25 = _commandProcessor.ProcessCommand("add_node", "25|Ground wet because of rain [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|SIMO|Truth");
            DebugUtils.DebugWriter.DebugWriteLine("#B605AW#", result25, true, DebugUtils.VerbosityLevel.Detailed);
        }

        private void AddMISONodes()
        {
            DebugUtils.DebugWriter.DebugWriteLine("#SKIXT4#", "Adding MISO nodes...", true, DebugUtils.VerbosityLevel.Normal);
            
            // Logical Operations
            string result3 = _commandProcessor.ProcessCommand("add_node", "3|AND Operation: Logical AND of inputs [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#DZ1M6Z#", result3, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result5 = _commandProcessor.ProcessCommand("add_node", "5|OR Operation: Logical OR of inputs [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#MB6F1V#", result5, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result7 = _commandProcessor.ProcessCommand("add_node", "7|NOT Operation: Logical NOT of input [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#DTXNNW#", result7, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result10 = _commandProcessor.ProcessCommand("add_node", "10|Implication Evaluation: Evaluates if A implies B [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#13VNHC#", result10, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Analysis Nodes
            string result14 = _commandProcessor.ProcessCommand("add_node", "14|Puddle Formation Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#P8N8QE#", result14, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result16 = _commandProcessor.ProcessCommand("add_node", "16|Wind Impact Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#EBO9LJ#", result16, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Complex Reasoning Chain
            string result18 = _commandProcessor.ProcessCommand("add_node", "18|Heavy Rain Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#H6UHBG#", result18, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result20 = _commandProcessor.ProcessCommand("add_node", "20|Umbrella Usability Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#RFRZIN#", result20, true, DebugUtils.VerbosityLevel.Detailed);
            
            string result22 = _commandProcessor.ProcessCommand("add_node", "22|Umbrella Need Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#K5CYJM#", result22, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Alternative Reasoning Path
            string result24 = _commandProcessor.ProcessCommand("add_node", "24|Reverse Implication Analysis [scenario: 8bf1ef9c-a64d-41ba-94f0-c7550357e0ef]|MISO");
            DebugUtils.DebugWriter.DebugWriteLine("#S8DJ7K#", result24, true, DebugUtils.VerbosityLevel.Detailed);
        }

        private void AddEdges()
        {
            DebugUtils.DebugWriter.DebugWriteLine("#OL1HMY#", "Adding edges...", true, DebugUtils.VerbosityLevel.Normal);
            
            // Connect AND Operation
            string resultEdge1 = _commandProcessor.ProcessCommand("add_edge", "1|3|1.0|Input to AND operation");
            DebugUtils.DebugWriter.DebugWriteLine("#RHJFW2#", resultEdge1, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge2 = _commandProcessor.ProcessCommand("add_edge", "2|3|1.0|Input to AND operation");
            DebugUtils.DebugWriter.DebugWriteLine("#04BBXD#", resultEdge2, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge3 = _commandProcessor.ProcessCommand("add_edge", "3|4|1.0|Output of AND operation");
            DebugUtils.DebugWriter.DebugWriteLine("#QHWR2H#", resultEdge3, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect OR Operation
            string resultEdge4 = _commandProcessor.ProcessCommand("add_edge", "1|5|1.0|Input to OR operation");
            DebugUtils.DebugWriter.DebugWriteLine("#DI7T6K#", resultEdge4, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge5 = _commandProcessor.ProcessCommand("add_edge", "2|5|1.0|Input to OR operation");
            DebugUtils.DebugWriter.DebugWriteLine("#08GVQ2#", resultEdge5, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge6 = _commandProcessor.ProcessCommand("add_edge", "5|6|1.0|Output of OR operation");
            DebugUtils.DebugWriter.DebugWriteLine("#DMMOQX#", resultEdge6, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect NOT Operation
            string resultEdge7 = _commandProcessor.ProcessCommand("add_edge", "1|7|1.0|Input to NOT operation");
            DebugUtils.DebugWriter.DebugWriteLine("#48HBDE#", resultEdge7, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge8 = _commandProcessor.ProcessCommand("add_edge", "7|8|1.0|Output of NOT operation");
            DebugUtils.DebugWriter.DebugWriteLine("#V3HATJ#", resultEdge8, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect Implication Evaluation
            string resultEdge9 = _commandProcessor.ProcessCommand("add_edge", "1|10|1.0|Input to implication evaluation");
            DebugUtils.DebugWriter.DebugWriteLine("#ZPOAI2#", resultEdge9, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge10 = _commandProcessor.ProcessCommand("add_edge", "2|10|1.0|Input to implication evaluation");
            DebugUtils.DebugWriter.DebugWriteLine("#3R8SYP#", resultEdge10, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge11 = _commandProcessor.ProcessCommand("add_edge", "9|10|1.0|Causal strength input to implication evaluation");
            DebugUtils.DebugWriter.DebugWriteLine("#UFIKSB#", resultEdge11, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge12 = _commandProcessor.ProcessCommand("add_edge", "10|11|1.0|Output of implication evaluation");
            DebugUtils.DebugWriter.DebugWriteLine("#LYJZO4#", resultEdge12, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect Puddle Formation Analysis
            string resultEdge13 = _commandProcessor.ProcessCommand("add_edge", "12|14|1.0|Rain intensity input to puddle formation analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#LLBEMY#", resultEdge13, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge14 = _commandProcessor.ProcessCommand("add_edge", "14|15|1.0|Output of puddle formation analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#JB4SMI#", resultEdge14, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect Wind Impact Analysis
            string resultEdge15 = _commandProcessor.ProcessCommand("add_edge", "13|16|1.0|Wind force input to wind impact analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#USRT79#", resultEdge15, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge16 = _commandProcessor.ProcessCommand("add_edge", "16|17|1.0|Output of wind impact analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#UG4GNL#", resultEdge16, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect Heavy Rain Analysis
            string resultEdge17 = _commandProcessor.ProcessCommand("add_edge", "12|18|1.0|Rain intensity input to heavy rain analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#QKIPWS#", resultEdge17, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge18 = _commandProcessor.ProcessCommand("add_edge", "18|19|1.0|Output of heavy rain analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#QZL12O#", resultEdge18, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect Umbrella Usability Analysis
            string resultEdge19 = _commandProcessor.ProcessCommand("add_edge", "13|20|1.0|Wind force input to umbrella usability analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#KXI8C6#", resultEdge19, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge20 = _commandProcessor.ProcessCommand("add_edge", "20|21|1.0|Output of umbrella usability analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#WKJH4Y#", resultEdge20, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect Umbrella Need Analysis
            string resultEdge21 = _commandProcessor.ProcessCommand("add_edge", "1|22|1.0|Rain status input to umbrella need analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#081MLC#", resultEdge21, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge22 = _commandProcessor.ProcessCommand("add_edge", "21|22|1.0|Umbrella usability input to umbrella need analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#CBGQNQ#", resultEdge22, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge23 = _commandProcessor.ProcessCommand("add_edge", "22|23|1.0|Output of umbrella need analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#IDZSFU#", resultEdge23, true, DebugUtils.VerbosityLevel.Detailed);
            
            // Connect Reverse Implication Analysis
            string resultEdge24 = _commandProcessor.ProcessCommand("add_edge", "1|24|1.0|Rain status input to reverse implication analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#TJC17Y#", resultEdge24, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge25 = _commandProcessor.ProcessCommand("add_edge", "2|24|1.0|Ground wetness input to reverse implication analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#8CRQT2#", resultEdge25, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge26 = _commandProcessor.ProcessCommand("add_edge", "9|24|1.0|Causal strength input to reverse implication analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#EMRFA3#", resultEdge26, true, DebugUtils.VerbosityLevel.Detailed);
            
            string resultEdge27 = _commandProcessor.ProcessCommand("add_edge", "24|25|1.0|Output of reverse implication analysis");
            DebugUtils.DebugWriter.DebugWriteLine("#YRVH2N#", resultEdge27, true, DebugUtils.VerbosityLevel.Detailed);
        }

        // Method to add probability distributions to SIMO nodes
        private void AddProbabilityDistributions()
        {
            DebugUtils.DebugWriter.DebugWriteLine("#FIWWAS#", "Adding probability distributions to SIMO nodes...", true, DebugUtils.VerbosityLevel.Normal);
            
            try
            {
                // Load Variable nodes from the graph file manager
                var nodes = LoadVariableNodes(); // Updated method call
                
                if (nodes.Count == 0) // This check should still be valid
                {
                    DebugUtils.DebugWriter.DebugWriteLine("#3M802W#", "No SIMO nodes found. Make sure to add nodes first.", true, DebugUtils.VerbosityLevel.Minimal);
                    return;
                }
                
                // Add probability distributions to each node based on its ID
                foreach (var node in nodes)
                {
                    bool nodeModified = false; // Flag to track if node was modified
                    switch (node.Id)
                    {
                        case 1: // Proposition A: It is raining
                            AddTruthDistribution(node, 0.7);
                            nodeModified = true;
                            break;
                            
                        case 2: // Proposition B: The ground is wet
                            AddTruthDistribution(node, 0.8);
                            nodeModified = true;
                            break;
                            
                        case 4: // Result of AND
                            AddTruthDistribution(node, 0.56);
                            nodeModified = true;
                            break;
                            
                        case 6: // Result of OR
                            AddTruthDistribution(node, 0.94);
                            nodeModified = true;
                            break;
                            
                        case 8: // Result of NOT
                            AddTruthDistribution(node, 0.3);
                            nodeModified = true;
                            break;
                            
                        case 9: // Causal Strength of Rain → Wet Ground
                            AddCausalStrengthDistribution(node);
                            nodeModified = true;
                            break;
                            
                        case 11: // Result of Implication
                            AddTruthDistribution(node, 0.85);
                            nodeModified = true;
                            break;
                            
                        case 12: // Rain Intensity
                            AddRainIntensityDistribution(node);
                            nodeModified = true;
                            break;
                            
                        case 13: // Wind Force
                            AddWindForceDistribution(node);
                            nodeModified = true;
                            break;
                            
                        case 15: // Puddle Formation Likelihood
                            AddTruthDistribution(node, 0.3);
                            nodeModified = true;
                            break;
                            
                        case 17: // Wind Impact on Umbrella
                            AddTruthDistribution(node, 0.35);
                            nodeModified = true;
                            break;
                            
                        case 19: // Is it raining heavily?
                            AddTruthDistribution(node, 0.1);
                            nodeModified = true;
                            break;
                            
                        case 21: // Is an umbrella usable?
                            AddTruthDistribution(node, 0.65);
                            nodeModified = true;
                            break;
                            
                        case 23: // Need for Umbrella
                            AddTruthDistribution(node, 0.455);
                            nodeModified = true;
                            break;
                            
                        case 25: // Ground wet because of rain
                            AddTruthDistribution(node, 0.65);
                            nodeModified = true;
                            break;
                            
                        default:
                            DebugUtils.DebugWriter.DebugWriteLine("#X78PHM#", $"No distribution defined for node {node.Id}", true, DebugUtils.VerbosityLevel.Detailed);
                            break;
                    }

                    // Save the node if it was modified using the mapper
                    if (nodeModified)
                    {
                        // Use Task.Result for simplicity in this synchronous method. Consider async/await pattern later.
                        if (_graphObjectMapper.SaveNodeAsync(node).Result) 
                        {
                            DebugUtils.DebugWriter.DebugWriteLine("#SAVE_OK#", $"Node {node.Id} saved successfully after adding distribution.", true, DebugUtils.VerbosityLevel.Detailed);
                        }
                        else
                        {
                            DebugUtils.DebugWriter.DebugWriteLine("#SAVE_ERR#", $"Failed to save node {node.Id} after adding distribution.", true, DebugUtils.VerbosityLevel.Minimal);
                        }
                    }
                }
                
                DebugUtils.DebugWriter.DebugWriteLine("#I3R80X#", "Finished attempting to add probability distributions.", true, DebugUtils.VerbosityLevel.Normal);
            }
            catch (Exception ex)
            {
                DebugUtils.DebugWriter.DebugWriteLine("#NM5HED#", $"Error adding probability distributions: {ex.Message}", true, DebugUtils.VerbosityLevel.Minimal);
                DebugUtils.DebugWriter.DebugWriteLine("#2IAIKF#", ex.StackTrace ?? "<No stack trace>", true, DebugUtils.VerbosityLevel.Detailed); // Added null check
            }
        }
        
        // Method now loads Variable nodes
        private List<Node> LoadVariableNodes() 
        {
            var variableNodes = new List<Node>(); // Changed type to Node
            DebugUtils.DebugWriter.DebugWriteLine("#LOAD_VAR_NODES#", "Loading Variable nodes...", true, DebugUtils.VerbosityLevel.Normal); // Updated message
            
            // Use Task.Result for simplicity in this synchronous method. Consider async/await pattern later.
            try
            {
                // Get IDs via mapper
                List<long> allNodeIds = _graphObjectMapper.GetAllNodeIdsAsync().Result; 
                DebugUtils.DebugWriter.DebugWriteLine("#LOAD_SIMO_IDS#", $"Found {allNodeIds.Count} total node IDs.", true, DebugUtils.VerbosityLevel.Detailed);

                foreach (long nodeId in allNodeIds)
                {
                    // Load node via mapper
                    Node? node = _graphObjectMapper.GetNodeAsync(nodeId).Result; 
                    // Check if it was loaded successfully and has the Variable role
                    if (node != null && node.Role == NodeRole.Variable) 
                    {
                        variableNodes.Add(node); // Add the Node object
                        DebugUtils.DebugWriter.DebugWriteLine("#LOAD_VAR_NODE#", $"Loaded Variable node {nodeId}.", true, DebugUtils.VerbosityLevel.Detailed); // Updated message
                    }
                    else if (node == null)
                    {
                         DebugUtils.DebugWriter.DebugWriteLine("#LOAD_VAR_ERR#", $"Failed to load node {nodeId} via mapper.", true, DebugUtils.VerbosityLevel.Minimal); // Updated message
                    }
                    // Optionally log if a node was loaded but wasn't a Variable node
                    else { // node != null but role is not Variable
                         DebugUtils.DebugWriter.DebugWriteLine("#LOAD_NONVAR_NODE#", $"Loaded node {nodeId} but it has role {node.Role}, expected Variable.", true, DebugUtils.VerbosityLevel.Detailed);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugUtils.DebugWriter.DebugWriteLine("#LOAD_VAR_EX#", $"Error loading nodes: {ex.Message}", true, DebugUtils.VerbosityLevel.Minimal); // Updated message
                DebugUtils.DebugWriter.DebugWriteLine("#LOAD_VAR_STACK#", ex.StackTrace ?? "<No stack trace>", true, DebugUtils.VerbosityLevel.Detailed); // Added null check
            }
            
            DebugUtils.DebugWriter.DebugWriteLine("#LOAD_VAR_DONE#", $"Loaded {variableNodes.Count} Variable nodes.", true, DebugUtils.VerbosityLevel.Normal); // Updated message
            return variableNodes; // Return list of Node
        }
        
        // Parameter type changed to Node
        private void AddTruthDistribution(Node node, double trueValue) 
        {
            // Ensure the node is a Variable node and has a distribution
            if (node.Role != NodeRole.Variable || node.Distribution == null) {
                 DebugUtils.DebugWriter.DebugWriteLine("#TRUTH_DIST_ERR#", $"Node {node.Id} is not a Variable node or has no distribution. Cannot add Truth distribution.", true, DebugUtils.VerbosityLevel.Minimal);
                 return;
            }
            DebugUtils.DebugWriter.DebugWriteLine("#SQ9FO2#", $"Adding Truth distribution to node {node.Id}: True={trueValue}, False={1-trueValue}", true, DebugUtils.VerbosityLevel.Detailed);
            
            // Add points to the existing distribution object
            node.Distribution.AddPoint(1.0, trueValue); 
            node.Distribution.AddPoint(0.0, 1.0 - trueValue);
        }
        
        // Parameter type changed to Node
        private void AddCausalStrengthDistribution(Node node) 
        {
             // Ensure the node is a Variable node and has a distribution
            if (node.Role != NodeRole.Variable || node.Distribution == null) {
                 DebugUtils.DebugWriter.DebugWriteLine("#CAUSAL_DIST_ERR#", $"Node {node.Id} is not a Variable node or has no distribution. Cannot add Causal Strength distribution.", true, DebugUtils.VerbosityLevel.Minimal);
                 return;
            }
            DebugUtils.DebugWriter.DebugWriteLine("#R1RH7B#", $"Adding Causal Strength distribution to node {node.Id}", true, DebugUtils.VerbosityLevel.Detailed);
            
            // Add ranges to the existing distribution object
            node.Distribution.AddRange(0.7, 0.9, 0.6);  
            node.Distribution.AddRange(0.4, 0.7, 0.3);  
            node.Distribution.AddRange(0.0, 0.4, 0.1);  
        }
        
        // Parameter type changed to Node
        private void AddRainIntensityDistribution(Node node) 
        {
             // Ensure the node is a Variable node and has a distribution
            if (node.Role != NodeRole.Variable || node.Distribution == null) {
                 DebugUtils.DebugWriter.DebugWriteLine("#RAIN_DIST_ERR#", $"Node {node.Id} is not a Variable node or has no distribution. Cannot add Rain Intensity distribution.", true, DebugUtils.VerbosityLevel.Minimal);
                 return;
            }
            DebugUtils.DebugWriter.DebugWriteLine("#7W0XRH#", $"Adding Rain Intensity distribution to node {node.Id}", true, DebugUtils.VerbosityLevel.Detailed);
            
             // Add ranges to the existing distribution object
            node.Distribution.AddRange(0.0, 1.0, 0.3);   
            node.Distribution.AddRange(1.0, 5.0, 0.4);   
            node.Distribution.AddRange(5.0, 15.0, 0.2);  
            node.Distribution.AddRange(15.0, 50.0, 0.1); 
        }
        
         // Parameter type changed to Node
        private void AddWindForceDistribution(Node node)
        {
             // Ensure the node is a Variable node and has a distribution
            if (node.Role != NodeRole.Variable || node.Distribution == null) {
                 DebugUtils.DebugWriter.DebugWriteLine("#WIND_DIST_ERR#", $"Node {node.Id} is not a Variable node or has no distribution. Cannot add Wind Force distribution.", true, DebugUtils.VerbosityLevel.Minimal);
                 return;
            }
            DebugUtils.DebugWriter.DebugWriteLine("#M26F8E#", $"Adding Wind Force distribution to node {node.Id}", true, DebugUtils.VerbosityLevel.Detailed);
            
             // Add points to the existing distribution object
            node.Distribution.AddPoint(0, 0.05); 
            node.Distribution.AddPoint(1, 0.10); 
            node.Distribution.AddPoint(2, 0.20); 
            node.Distribution.AddPoint(3, 0.30); 
            node.Distribution.AddPoint(4, 0.20); 
            node.Distribution.AddPoint(5, 0.10); 
            node.Distribution.AddPoint(6, 0.05); 
        }

        // Note: This Main method allows running the populator directly.
        // However, it's recommended to run scenarios via the main program:
        // `dotnet run --project ReasoningEngine/ReasoningEngine.csproj --run-scenario weather --verbosity Minimal`
        public static void Main(string[] args)
        {
            try
            {
                // Load environment variables
                Env.Load();
                string dataFolderPath = Environment.GetEnvironmentVariable("DATA_FOLDER_PATH") 
                                       ?? throw new Exception("DATA_FOLDER_PATH is not set in the environment variables.");
                
                DebugUtils.DebugWriter.DebugWriteLine("#WF3ARH#", $"Using data folder path: {dataFolderPath}", true, DebugUtils.VerbosityLevel.Normal);
                
                // Initialize Storage Provider, Mapper, and Command Processor
                IGraphStorageProvider storageProvider = new FileGraphStorageProvider(dataFolderPath);
                var graphObjectMapper = new GraphObjectMapper(storageProvider);
                var commandProcessor = new CommandProcessor(graphObjectMapper);
                
                // Run OneTimeSetup to ensure the data directory is properly initialized
                OneTimeSetup.Initialize();
                
                // Create and run the data populator, passing the mapper
                var populator = new PopulateWeatherScenarioData(commandProcessor, graphObjectMapper); 
                populator.PopulateData();
                
                DebugUtils.DebugWriter.DebugWriteLine("#ZLUMLC#", "Weather scenario data population completed successfully.", true, DebugUtils.VerbosityLevel.Minimal);
            }
            catch (Exception ex)
            {
                DebugUtils.DebugWriter.DebugWriteLine("#9KOY9E#", $"Error in Main: {ex.Message}", true, DebugUtils.VerbosityLevel.Minimal);
                DebugUtils.DebugWriter.DebugWriteLine("#W154KN#", ex.StackTrace ?? "<No stack trace>", true, DebugUtils.VerbosityLevel.Detailed); // Added null check
            }
        }
    }
}

namespace ReasoningEngine
{
    /// <summary>
    /// Defines the type of operation performed by a Function node.
    /// </summary>
    public enum FunctionType
    {
        // Removed Prior as it's handled by Variable nodes without Function inputs

        /// <summary>
        /// Linear combination of inputs: output = sum(weight_i * input_i) + bias.
        /// Parameters: "Weights" (List<double> or similar), "Bias" (double).
        /// </summary>
        Linear,

        /// <summary>
        /// A predefined operation like Multiply, Add, Sigmoid, etc.
        /// Parameters: "Operation" (string/enum, e.g., "Multiply", "Sigmoid"). Specific parameters depend on the operation.
        /// </summary>
        DefinedOp,

        /// <summary>
        /// Represents a neural network layer or a full network.
        /// Parameters: Configuration details (e.g., "LayerDefs", "WeightsPath", "Activation").
        /// </summary>
        NeuralNet,

        // Removed specific ops like Multiply, Add, Sigmoid as they fall under DefinedOp
        // Removed GaussianCPD as it's a specific functional form, could be a DefinedOp or NeuralNet

        // Future types could include:
        // LookupTable, // Could be a DefinedOp with specific parameters
        // NeuralNetworkLayer, // Represents a layer in a neural network
        // Custom // Reference to user-defined function logic
    }
}

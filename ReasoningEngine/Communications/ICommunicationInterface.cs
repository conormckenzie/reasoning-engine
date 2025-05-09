namespace ReasoningEngine.Communication
{
    /// <summary>
    /// Interface for handling communication between the reasoning engine and external systems.
    /// Supports multiple communication strategies, including but not limited to: APIs, IPC pipes, Python bridge.
    /// Provides synchronous and asynchronous methods for command handling, data saving, and data loading.
    /// </summary>
    public interface ICommunicationInterface
    {
        /// <summary>
        /// Sends a command to the reasoning engine and returns an immediate response.
        /// This method can be used for sending commands like "save" or "load" and receiving
        /// the result of the operation.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <param name="commandIdentifier">An identifier to track the specific command, ensuring the correct response is received.</param>
        /// <param name="payload">The payload or data associated with the command.</param>
        /// <returns>A string containing the response from the reasoning engine.</returns>
        string SendCommand(string command, string commandIdentifier, string payload);

        /// <summary>
        /// Asynchronously sends a command to the reasoning engine and returns an immediate response.
        /// This method can be used for sending commands like "save" or "load" and receiving
        /// the result of the operation.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <param name="commandIdentifier">An identifier to track the specific command, ensuring the correct response is received.</param>
        /// <param name="payload">The payload or data associated with the command.</param>
        /// <returns>A task representing the asynchronous operation, with a string containing the response from the reasoning engine.</returns>
        Task<string> SendCommandAsync(string command, string commandIdentifier, string payload);

        /// <summary>
        /// Receives a response from the reasoning engine for a previously sent command.
        /// This method allows for retrieving responses based on the command identifier.
        /// </summary>
        /// <param name="commandIdentifier">The identifier of the command whose response is being retrieved.</param>
        /// <returns>A string containing the response corresponding to the commandIdentifier.</returns>
        string ReceiveResponse(string commandIdentifier);

        /// <summary>
        /// Saves a piece of data to the reasoning engine's storage.
        /// This method allows for specifying what data to save via the identifier.
        /// </summary>
        /// <param name="dataIdentifier">An identifier for the data being saved, allowing for selective saving and overwriting of specific data.</param>
        /// <param name="data">The data to save, in string format.</param>
        /// <returns>A string containing the response or confirmation of the save operation.</returns>
        string SaveData(string dataIdentifier, string data);

        /// <summary>
        /// Asynchronously saves a piece of data to the reasoning engine's storage.
        /// This method allows for specifying what data to save via the identifier.
        /// </summary>
        /// <param name="dataIdentifier">An identifier for the data being saved, allowing for selective saving and overwriting of specific data.</param>
        /// <param name="data">The data to save, in string format.</param>
        /// <returns>A task representing the asynchronous operation, with a string containing the response or confirmation of the save operation.</returns>
        Task<string> SaveDataAsync(string dataIdentifier, string data);

        /// <summary>
        /// Loads a piece of data from the reasoning engine's storage.
        /// This method allows for specifying what data to load via the identifier.
        /// </summary>
        /// <param name="dataIdentifier">An identifier for the data being loaded, allowing for selective retrieval of specific data.</param>
        /// <returns>A string containing the loaded data.</returns>
        string LoadData(string dataIdentifier);

        /// <summary>
        /// Asynchronously loads a piece of data from the reasoning engine's storage.
        /// This method allows for specifying what data to load via the identifier.
        /// </summary>
        /// <param name="dataIdentifier">An identifier for the data being loaded, allowing for selective retrieval of specific data.</param>
        /// <returns>A task representing the asynchronous operation, with a string containing the loaded data.</returns>
        Task<string> LoadDataAsync(string dataIdentifier);
    }
}

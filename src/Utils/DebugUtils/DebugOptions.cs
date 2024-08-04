namespace DebugUtils
{
    /// <summary>
    /// Provides options for debug settings.
    /// </summary>
    public static class DebugOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether debug mode is enabled.
        /// </summary>
        public static bool DebugMode { get; set; } = false;

        /// <summary>
        /// Toggles the debug mode based on user input and updates the user on the current status.
        /// </summary>
        public static void SetDebugMode()
        {
            // Display current debug mode status
            DebugWriter.DebugWriteLine("#D7C9#", $"Debug mode is currently {(DebugMode ? "ON" : "OFF")}");
            DebugWriter.DebugWriteLine("#D7CA#", $"Would you like to turn it {(DebugMode ? "OFF" : "ON")}? (y/n): ");

            // Read user input
            string response = Console.ReadLine()?.ToLower() ?? string.Empty;
            if (response == "y")
            {
                // Toggle debug mode and display the new status
                DebugMode = !DebugMode;
                DebugWriter.DebugWriteLine("#D7CB#", $"Debug mode is now {(DebugMode ? "ON" : "OFF")}");
            }
            else
            {
                // Inform the user that debug mode remains unchanged
                DebugWriter.DebugWriteLine("#D7CC#", $"Debug mode unchanged. It is currently {(DebugMode ? "ON" : "OFF")}");
            }
        }
    }
}

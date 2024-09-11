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

        public static void ShowDebugOptionsMenu()
        {
            while (true)
            {
                DebugWriter.DebugWriteLine("#00TP7Q#", "\nDebug Options Menu:");
                DebugWriter.DebugWriteLine("#00TP7R#", $"1. Change Debug Mode (Currently: {(DebugOptions.DebugMode ? "ON" : "OFF")})");
                DebugWriter.DebugWriteLine("#00TP7S#", "2. Generate New Debug Message");
                DebugWriter.DebugWriteLine("#00TP7T#", "0. Return to Main Menu");
                DebugWriter.DebugWrite("#00TP7U#", "Enter option: ");

                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        DebugOptions.DebugMode = !DebugOptions.DebugMode;
                        DebugWriter.DebugWriteLine("#00W3BA#", $"Debug mode is now {(DebugOptions.DebugMode ? "ON" : "OFF")}");
                        break;
                    case "2":
                        string newDebugMessage = DebugWriter.GenerateRandomDebugMessage();
                        DebugWriter.DebugWriteLine("#00W3BB#", $"New debug message: {newDebugMessage}");
                        DebugWriter.DebugWriteLine("#00W3BC#", "Please check that this message is not already in use in the program.");
                        DebugWriter.DebugWriteLine("#00W3BD#", "In VS Code, you can use Ctrl+Shift+F to search across all files.");
                        break;
                    case "0":
                        return;
                    default:
                        DebugWriter.DebugWriteLine("#00W3BE#", "Invalid option. Please try again.");
                        break;
                }
            }
        }

        /// <summary>
        /// Toggles the debug mode based on user input and updates the user on the current status.
        /// </summary>
        /// <remarks>
        /// OBSOLETE - replaced by ShowDebugOptionsMenu()
        /// </remarks>
        public static void SetDebugMode()
        {
            // Display current debug mode status
            DebugWriter.DebugWriteLine("#00D7C9#", $"Debug mode is currently {(DebugMode ? "ON" : "OFF")}");
            DebugWriter.DebugWriteLine("#00D7CA#", $"Would you like to turn it {(DebugMode ? "OFF" : "ON")}? (y/n): ");

            // Read user input
            string response = Console.ReadLine()?.ToLower() ?? string.Empty;
            if (response == "y")
            {
                // Toggle debug mode and display the new status
                DebugMode = !DebugMode;
                DebugWriter.DebugWriteLine("#00D7CB#", $"Debug mode is now {(DebugMode ? "ON" : "OFF")}");
            }
            else
            {
                // Inform the user that debug mode remains unchanged
                DebugWriter.DebugWriteLine("#00D7CC#", $"Debug mode unchanged. It is currently {(DebugMode ? "ON" : "OFF")}");
            }
        }
    }
}

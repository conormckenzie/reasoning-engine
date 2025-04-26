namespace DebugUtils
{
    /// <summary>
    /// Defines the verbosity level for output messages.
    /// </summary>
    public enum VerbosityLevel
    {
        /// <summary>
        /// Minimal output, showing only essential information.
        /// </summary>
        Minimal,
        
        /// <summary>
        /// Normal output, showing important information.
        /// </summary>
        Normal,
        
        /// <summary>
        /// Detailed output, showing all available information.
        /// </summary>
        Detailed
    }

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
        /// Gets or sets the verbosity level for output messages.
        /// </summary>
        public static VerbosityLevel Verbosity { get; set; } = VerbosityLevel.Normal;

        /// <summary>
        /// Displays the debug options menu and handles user interaction.
        /// </summary>
        public static void ShowDebugOptionsMenu()
        {
            while (true)
            {
                DebugWriter.DebugWriteLine("#00TP7Q#", "\nDebug Options Menu:");
                DebugWriter.DebugWriteLine("#00TP7R#", $"1. Change Debug Mode (Currently: {(DebugOptions.DebugMode ? "ON" : "OFF")})");
                DebugWriter.DebugWriteLine("#00VB01#", $"2. Change Verbosity Level (Currently: {DebugOptions.Verbosity})");
                DebugWriter.DebugWriteLine("#00TP7S#", "3. Generate New Debug Message");
                DebugWriter.DebugWriteLine("#00TP7T#", "4. Return to Main Menu");
                DebugWriter.DebugWrite("#00TP7U#", "Enter option: ");

                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        DebugOptions.DebugMode = !DebugOptions.DebugMode;
                        DebugWriter.DebugWriteLine("#00W3BA#", $"Debug mode is now {(DebugOptions.DebugMode ? "ON" : "OFF")}");
                        break;
                    case "2":
                        ChangeVerbosityLevel();
                        break;
                    case "3":
                        string newDebugMessage = DebugWriter.GenerateRandomDebugMessage();
                        DebugWriter.DebugWriteLine("#00W3BB#", $"New debug message: {newDebugMessage}");
                        DebugWriter.DebugWriteLine("#00W3BC#", "Please check that this message is not already in use in the program.");
                        DebugWriter.DebugWriteLine("#00W3BD#", "In VS Code, you can use Ctrl+Shift+F to search across all files.");
                        break;
                    case "4":
                        return;
                    default:
                        DebugWriter.DebugWriteLine("#00W3BE#", "Invalid option. Please try again.");
                        break;
                }
            }
        }

        /// <summary>
        /// Allows the user to change the verbosity level.
        /// </summary>
        private static void ChangeVerbosityLevel()
        {
            DebugWriter.DebugWriteLine("#00VB02#", "\nVerbosity Level Options:");
            DebugWriter.DebugWriteLine("#00VB03#", "1. Minimal - Show only essential information");
            DebugWriter.DebugWriteLine("#00VB04#", "2. Normal - Show important information (default)");
            DebugWriter.DebugWriteLine("#00VB05#", "3. Detailed - Show all available information");
            DebugWriter.DebugWriteLine("#00VB06#", $"Current setting: {Verbosity}");
            DebugWriter.DebugWrite("#00VB07#", "Enter option (1-3): ");

            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    Verbosity = VerbosityLevel.Minimal;
                    break;
                case "2":
                    Verbosity = VerbosityLevel.Normal;
                    break;
                case "3":
                    Verbosity = VerbosityLevel.Detailed;
                    break;
                default:
                    DebugWriter.DebugWriteLine("#00VB08#", "Invalid option. Verbosity level unchanged.");
                    return;
            }

            DebugWriter.DebugWriteLine("#00VB09#", $"Verbosity level is now set to {Verbosity}");
        }

        // Removed obsolete SetDebugMode() method - functionality handled by ShowDebugOptionsMenu()
    }
}

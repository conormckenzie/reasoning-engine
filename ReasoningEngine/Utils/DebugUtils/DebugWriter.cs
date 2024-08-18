namespace DebugUtils
{
    /// <summary>
    /// Provides methods for writing debug and regular messages to the console.
    /// </summary>
    public static class DebugWriter
    {
        /// <summary>
        /// Writes a debug message and a regular message to the console.
        /// </summary>
        /// <param name="debugMessage">The debug message to write.</param>
        /// <param name="regularMessage">The regular message to write.</param>
        /// <param name="inLine">Whether to write the debug message in line with the regular message.</param>
        /// <param name="addNewLine">Whether to add a new line after the regular message.</param>
        public static void DebugWrite(string debugMessage, string regularMessage, bool inLine = true, bool addNewLine = true)
        {
            if (DebugOptions.DebugMode)
            {
                Console.ForegroundColor = ConsoleColor.Yellow; // Set the color for debug messages
                if (inLine)
                {
                    Console.Write($"[DEBUG] {{ {debugMessage} }}; ");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] {debugMessage}");
                }
            }
            Console.ResetColor(); // Reset the color to default
            if (addNewLine)
            {
                Console.WriteLine(regularMessage);
            }
            else
            {
                Console.Write(regularMessage);
            }
        }

        /// <summary>
        /// Writes a debug message and a regular message to the console, followed by a new line.
        /// </summary>
        /// <param name="debugMessage">The debug message to write.</param>
        /// <param name="regularMessage">The regular message to write.</param>
        /// <param name="inLine">Whether to write the debug message in line with the regular message.</param>
        public static void DebugWriteLine(string debugMessage, string regularMessage, bool inLine = true)
        {
            DebugWrite(debugMessage, regularMessage, inLine, true);
        }
    }
}

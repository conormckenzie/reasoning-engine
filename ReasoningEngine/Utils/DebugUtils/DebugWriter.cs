using System.Text.RegularExpressions;

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
                if (!IsDebugMessageValid(debugMessage)) 
                {
                    Console.BackgroundColor = ConsoleColor.Magenta;
                }
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

        /// <summary>
        /// Validates if a debug message adheres to the required format.
        /// </summary>
        /// <param name="debugMessage">The debug message to validate.</param>
        /// <returns>True if the debug message is valid; otherwise, false.</returns>
        /// <remarks>
        /// A valid debug message should be of the form "#XXXX#" where XXXX is any 4-character string.
        /// The 4-character string can contain any characters but should be unique across the whole program.
        /// </remarks>
        public static bool IsDebugMessageValid(string debugMessage)
        {
            if (string.IsNullOrEmpty(debugMessage))
            {
                return false;
            }

            // Use a regular expression to check the format
            var regex = new Regex(@"^#.{4}#$");
            return regex.IsMatch(debugMessage);
        }

        /// <summary>
        /// Generates a new random debug message in the format "#XXXX#".
        /// </summary>
        /// <returns>A randomly generated debug message.</returns>
        /// <remarks>
        /// The generated message consists of a '#' character, followed by 4 random uppercase letters or digits, and ends with another '#' character.
        /// </remarks>
        public static string GenerateRandomDebugMessage()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new char[4];

            for (int i = 0; i < 4; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return $"#{new string(result)}#";
        }
    }
}

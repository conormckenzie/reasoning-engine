namespace DebugUtils
{
    public static class DebugWriter
    {
        public static void DebugWrite(string debugMessage, string regularMessage, bool inLine = true, bool addNewLine = true)
        {
            if (DebugOptions.DebugMode)
            {
                Console.ForegroundColor = ConsoleColor.Yellow; // Set the color for debug messages
                if (inLine)
                {
                    Console.Write("[DEBUG] { " + debugMessage + " }; ");
                }
                else
                {
                    Console.WriteLine("[DEBUG] " + debugMessage);
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

        public static void DebugWriteLine(string debugMessage, string regularMessage, bool inLine = true)
        {
            DebugWrite(debugMessage, regularMessage, inLine, true);
        }
    }
}

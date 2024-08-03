namespace DebugUtils
{
    public static class DebugOptions
    {
        public static bool DebugMode { get; set; } = false;

        public static void SetDebugMode()
        {
            DebugWriter.DebugWriteLine("#D7C9#", "Debug mode is currently " + (DebugMode ? "ON" : "OFF"));
            DebugWriter.DebugWriteLine("#D7CA#", "Would you like to turn it " + (DebugMode ? "OFF" : "ON") + "? (y/n): ");
            string response = Console.ReadLine().ToLower();
            if (response == "y")
            {
                DebugMode = !DebugMode;
                DebugWriter.DebugWriteLine("#D7CB#", "Debug mode is now " + (DebugMode ? "ON" : "OFF"));
            }
            else
            {
                DebugWriter.DebugWriteLine("#D7CC#", "Debug mode unchanged. It is currently " + (DebugMode ? "ON" : "OFF"));
            }
        }
    }
}

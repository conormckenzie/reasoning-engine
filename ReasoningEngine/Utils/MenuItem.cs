// Removed unused using statements
namespace ReasoningEngine
{
    /// <summary>
    /// Represents a menu item with display text, a debug string, and internal text for command processing.
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// Gets or sets the text displayed to the user for this menu item.
        /// </summary>
        public string DisplayText { get; set; }
        
        /// <summary>
        /// Gets or sets a debug string associated with this menu item.
        /// </summary>
        public string DebugString { get; set; }
        
        /// <summary>
        /// Gets or sets the internal text used for processing the command associated with this menu item.
        /// </summary>
        public string InternalText { get; set; }

        /// <summary>
        /// Initializes a new instance of the MenuItem class.
        /// </summary>
        /// <param name="displayText">The text displayed to the user.</param>
        /// <param name="debugString">The debug string associated with the item.</param>
        /// <param name="internalText">The internal text for command processing.</param>
        public MenuItem(string displayText, string debugString, string internalText)
        {
            DisplayText = displayText;
            DebugString = debugString;
            InternalText = internalText;
        }
    }
}

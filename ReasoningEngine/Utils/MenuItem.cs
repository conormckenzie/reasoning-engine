using System;
using System.Collections.Generic;
using DotNetEnv;
using ReasoningEngine.GraphFileHandling;
using ReasoningEngine.GraphAccess;
using Newtonsoft.Json;
using DebugUtils;

namespace ReasoningEngine
{
    public class MenuItem
    {
        public string DisplayText { get; set; }
        public string DebugString { get; set; }
        public string InternalText { get; set; }

        public MenuItem(string displayText, string debugString, string internalText)
        {
            DisplayText = displayText;
            DebugString = debugString;
            InternalText = internalText;
        }
    }
}
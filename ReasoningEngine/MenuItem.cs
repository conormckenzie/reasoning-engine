using System;
using System.Collections.Generic;
using DotNetEnv;
using ReasoningEngine.GraphFileHandling;
using ReasoningEngine.GraphAccess; // Added namespace for CommandProcessor
using Newtonsoft.Json;
using DebugUtils;

namespace ReasoningEngine
{
    public class MenuItem
{
    public string Text { get; set; }
    public string DebugString { get; set; }

    public MenuItem(string text, string debugString)
    {
        Text = text;
        DebugString = debugString;
    }
}
}

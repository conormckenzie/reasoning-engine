// OneTimeSetup.cs
using System;
using System.Collections.Generic;
using System.IO;

public class OneTimeSetup
{
    public static void RunSetup(string folderPath, string filePath, List<(int, string)> nodes, List<(int, int, double, string)> edges)
    {
        // Check if the folder exists, and create it if it doesn't
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Check if the file exists, and create it if it doesn't
        if (!File.Exists(filePath))
        {
            // Create a file to write to with default new nodes and edge
            using (StreamWriter sw = File.CreateText(filePath))
            {
                nodes.Add((0, "new node 0"));
                nodes.Add((1, "new node 1"));
                edges.Add((0, 1, 0.5, "new edge between new nodes 1 and 2"));
                foreach (var node in nodes)
                {
                    sw.WriteLine("NODE:" + node);
                }
                foreach (var edge in edges)
                {
                    sw.WriteLine("EDGE:" + edge);
                }
            }
        }
    }
}

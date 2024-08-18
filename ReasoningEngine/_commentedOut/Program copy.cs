// using System;
// using System.IO;
// using System.Collections.Generic;
// using DotNetEnv;

// // Load the environment variables from the .env file
// Env.Load();

// // Get the folder path and file name from environment variables
// string folderPath = Environment.GetEnvironmentVariable("FOLDER_PATH");
// string fileName = Environment.GetEnvironmentVariable("FILE_NAME");

// // Combine folder path and file name to get the full file path
// string filePath = Path.Combine(folderPath, fileName);

// var nodes = new List<(int, string)>(); // [0] -> index; [1]-> semantic content of node 
// var edges = new List<(int, int, double, string)>(); // [0] -> source node; [1] -> destination node; [2] -> edge weight; [3] -> semantic content of edge

// // BEGIN ONE-TIME SETUP
// // Check if the folder exists, and create it if it doesn't
// if (!Directory.Exists(folderPath))
// {
//     Directory.CreateDirectory(folderPath);
// }

// // Check if the file exists, and create it if it doesn't
// if (!File.Exists(filePath))
// {
//     // Create a file to write to with default new nodes and edge
//     using (StreamWriter sw = File.CreateText(filePath))
//     {
//         nodes.Add((0, "new node 0"));
//         nodes.Add((1, "new node 1"));
//         edges.Add((0, 1, 1, "new edge between new nodes 1 and 2"));
//         foreach (var node in nodes) {
//             sw.WriteLine("NODE:" + node);
//         }
//         foreach (var edge in edges) {
//             sw.WriteLine("EDGE:" + edge);
//         }
//     }
// }

// // END ONE-TIME SETUP

// // BEGIN READ-WRITE TEST

// // Open the file to read from.
// using (StreamReader sr = File.OpenText(filePath))
// {
//     string s;
//     while ((s = sr.ReadLine()) != null)
//     {
//         Console.WriteLine(s);
//     }
// }

// using (StreamWriter sw = File.CreateText(filePath))
// {
//     sw.WriteLine("Birb");
//     sw.WriteLine("Lubs");
//     sw.WriteLine("Youb");
// }
// using (StreamReader sr = File.OpenText(filePath))
// {
//     string s;
//     while ((s = sr.ReadLine()) != null)
//     {
//         Console.WriteLine(s + " :) ");
//     }
// }

// // END READ-WRITE TEST

// /* TODO:

// (0) Upgrade to meaning-augmented nodes and weighted edges
// (1) Track new nodes; edges
// (2) Append new nodes & edges to file
// (3) Rework to split into multiple files - NEEDS DESIGN
// (4) Print all nodes, edges to console
// (5) Functionize the above
// */
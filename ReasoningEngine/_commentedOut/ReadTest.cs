// // ReadTest.cs
// using System;
// using System.IO;

// public class ReadTest
// {
//     public static void RunTest(string filePath)
//     {
//         // Open the file to read from.
//         using (StreamReader sr = File.OpenText(filePath))
//         {
//             string s;
//             while ((s = sr.ReadLine()) != null)
//             {
//                 Console.WriteLine(s);
//             }
//         }

//         // using (StreamWriter sw = File.CreateText(filePath))
//         // {
//         //     sw.WriteLine("Birb");
//         //     sw.WriteLine("Lubs");
//         //     sw.WriteLine("Youb");
//         // }

//         // using (StreamReader sr = File.OpenText(filePath))
//         // {
//         //     string s;
//         //     while ((s = sr.ReadLine()) != null)
//         //     {
//         //         Console.WriteLine(s + " :) ");
//         //     }
//         // }
//     }
// }

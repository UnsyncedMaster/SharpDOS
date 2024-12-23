using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
using System.Xml.Linq;
class SharpDOS
{
    static Dictionary<string, object> fileSystem = new Dictionary<string, object>
    {
        { Directory.GetCurrentDirectory() + "\\", new Dictionary<string, object>() }
    };

    static string currentDirectory = Directory.GetCurrentDirectory() + "\\";

    static string saveFilePath = "SharpDOSFileSys.inf";
    static void Main(string[] args)
    {
        Console.Title = "SharpDOS V0.01";
        LoadFileSystem();
        Console.WriteLine("Welcome to SharpDOS! Type 'EXIT' To Quit.");
        while (true)
        {
            Console.Write($"{currentDirectory}> ");
            string input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                continue;

            string[] parts = input.Split(' ', 2);
            string command = parts[0].ToUpper();
            string argument = parts.Length > 1 ? parts[1] : string.Empty;

            switch (command)
            {
                case "DIR":
                    Dir();
                    break;
                case "CD":
                    Cd(argument);
                    break;
                case "MKDIR":
                    Mkdir(argument);
                    break;
                case "CLS":
                    Console.Clear();
                    break;
                case "EXIT":
                    Console.WriteLine("Saving..");
                    SaveFileSystem();
                    Console.Beep();
                    Task.Delay(500);
                    Console.WriteLine("Exiting SharpDOS. Goodbye!");
                    Environment.Exit(0);
                    break;
                case "ECHO":
                    Echo(argument);
                    break;
                case "PING":
                    Ping(argument);
                    break;
                case "VER":
                    Ver();
                    break;
                case "RMDIR":
                    Rmdir(argument);
                    break;
                case "REN":
                    Ren(argument);
                    break;
                case "DATE":
                    Date();
                    break;
                case "VOL":
                    Vol();
                    break;
                case "CHKDSK":
                    Chkdsk();
                    break;
                default:
                    Console.WriteLine($"'{command}' Is Not recognized As An Internal Or External Command.");
                    break;
            }
        }
    }

    // DIR
    static void Dir()
    {
        if (fileSystem[currentDirectory] is Dictionary<string, object> directoryContents)
        {
            Console.WriteLine("\n Directory Of " + currentDirectory);
            foreach (var item in directoryContents)
            {
                Console.WriteLine("  " + item.Key);
            }
        }
        else
        {
            Console.WriteLine("Error: Current Path Is Not A Directory.");
        }
    }
    // MKDIR
    static void Mkdir(string name)
    {
        string newDir = currentDirectory + name + "\\";
        if (!fileSystem.ContainsKey(newDir))
        {
            fileSystem[newDir] = new Dictionary<string, object>();
            Console.WriteLine($"Directory '{name}' Created.");
        }
        else
        {
            Console.WriteLine($"Error: Directory '{name}' Already Exists.");
        }
    }

    // PING
    static void Ping(string address)
    {
        if (string.IsNullOrEmpty(address))
        {
            Console.WriteLine("Error: Address Not Specified.");
            return;
        }

        try
        {
            Process pingProcess = new Process();
            pingProcess.StartInfo.FileName = "ping";
            pingProcess.StartInfo.Arguments = $"-n 4 {address}"; // Ping 4 X Like The DOS command.
            pingProcess.StartInfo.RedirectStandardOutput = true;
            pingProcess.StartInfo.RedirectStandardError = true;
            pingProcess.StartInfo.UseShellExecute = false;
            pingProcess.StartInfo.CreateNoWindow = true;
            pingProcess.Start();

            string output = pingProcess.StandardOutput.ReadToEnd();
            string error = pingProcess.StandardError.ReadToEnd();
            pingProcess.WaitForExit();

            if (!string.IsNullOrEmpty(output))
            {
                Console.WriteLine(output);
            }
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine(error);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Pinging: {ex.Message}");
        }
    }

    // CHKDSK
    static void Chkdsk()
    {
        Console.WriteLine("Checking Disk For Errors...");
        Console.WriteLine("File System Check:");

        int totalFiles = 0;
        int totalDirs = 0;
        long totalSize = 0;

        void Traverse(Dictionary<string, object> dir)
        {
            foreach (var item in dir)
            {
                if (item.Value is Dictionary<string, object> subDir)
                {
                    totalDirs++;
                    Traverse(subDir);
                }
                else
                {
                    totalFiles++;
                    totalSize += 1024; // Assume 1KB For Each File
                }
            }
        }

        if (fileSystem[currentDirectory] is Dictionary<string, object> root)
        {
            Traverse(root);
        }

        Console.WriteLine($"Total Files: {totalFiles}");
        Console.WriteLine($"Total Directories: {totalDirs}");
        Console.WriteLine($"Total Size: {totalSize} Bytes");
        Console.WriteLine("No Errors Found.");
    }

    // VOL
    static void Vol()
    {
        try
        {
            DriveInfo drive = new DriveInfo(Path.GetPathRoot(currentDirectory));
            string label = string.IsNullOrEmpty(drive.VolumeLabel) ? "No Label" : drive.VolumeLabel;
            string serial = drive.DriveFormat; 
            Console.WriteLine($"Volume In Drive {drive.Name.TrimEnd('\\')} Is {label}");
            Console.WriteLine($"Volume Format Is {serial}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Retrieving VOL Info: {ex.Message}");
        }
    }

    // VER
    static void Ver()
    {
        Console.WriteLine("SharpDOS Version  V0.01");
        Console.WriteLine("Copyright 2024 - UnsyncedMaster Under Freeuse License");
    }

    // RMDIR
    static void Rmdir(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Console.WriteLine("Error: Directory Name Not Specified.");
            return;
        }

        string dirPath = currentDirectory + name + "\\";
        if (fileSystem.ContainsKey(dirPath))
        {
            fileSystem.Remove(dirPath);
            Console.WriteLine($"Directory '{name}' Removed.");
        }
        else
        {
            Console.WriteLine("Error: Directory Not Found.");
        }
    }

    // DATE
    static void Date()
    {
        Console.WriteLine("Current Date: " + DateTime.Now.ToShortDateString());
    }

    // REN
    static void Ren(string arguments)
    {
        var args = arguments.Split(' ');
        if (args.Length < 2)
        {
            Console.WriteLine("Error: Missing Args. Usage: REN oldname newname");
            return;
        }

        string oldName = currentDirectory + args[0];
        string newName = currentDirectory + args[1];

        if (fileSystem.ContainsKey(oldName))
        {
            fileSystem[newName] = fileSystem[oldName];
            fileSystem.Remove(oldName);
            Console.WriteLine($"Renamed '{args[0]}' To '{args[1]}'");
        }
        else
        {
            Console.WriteLine("Error: File Or Directory Not Found.");
        }
    }

    // ECHO
    static void Echo(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Console.WriteLine("Error: No Message Not Specified.");
            Console.WriteLine("Usage : ECHO (message here)");
            return;
        }
        Console.WriteLine(message);
    }

    // CD
    static void Cd(string path)
    {
        if (fileSystem[currentDirectory] is Dictionary<string, object> directoryContents)
        {
            Console.WriteLine("\n Directory of " + currentDirectory);
            foreach (var item in directoryContents)
            {
                Console.WriteLine("  " + item.Key);
            }
        }
        else
        {
            Console.WriteLine("Error: Current path Is Not A Directory.");
        }
    }

    // File Sys
    static void SaveFileSystem()
    {
        try
        {
            string json = JsonSerializer.Serialize(fileSystem, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(saveFilePath, json);
            Console.WriteLine("File System Saved.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error Saving : " + ex.Message);
        }
    }

    static void LoadFileSystem()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                fileSystem = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                FixFileSystem(fileSystem);
                if (!fileSystem.ContainsKey(currentDirectory) || !(fileSystem[currentDirectory] is Dictionary<string, object>))
                {
                    currentDirectory = "C:\\";
                }
                Console.WriteLine("File System Loaded.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error Loading File system: " + ex.Message);
        }
    }

    static void FixFileSystem(Dictionary<string, object> dir)
    {
        foreach (var key in dir.Keys)
        {
            if (dir[key] is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
            {
                dir[key] = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonElement.GetRawText());
                FixFileSystem((Dictionary<string, object>)dir[key]);
            }
        }
    }
}
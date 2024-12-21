using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

class SharpDOS
{
    static Dictionary<string, object> fileSystem = new Dictionary<string, object>
    {
        { "C:\\", new Dictionary<string, object>() }
    };
    static string currentDirectory = "C:\\";
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
                    return;
                default:
                    Console.WriteLine($"'{command}' Is Not recognized As An Internal Or External Command.");
                    break;
            }
        }
    }

    // Dir
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
    // MKDir
    static void Mkdir(string names)
    {
        if (string.IsNullOrEmpty(names))
        {
            Console.WriteLine("Error: Directory Name(s) Not specified.");
            return;
        }

        string[] directories = names.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (fileSystem[currentDirectory] is Dictionary<string, object> directoryContents)
        {
            foreach (string name in directories)
            {
                string newDir = currentDirectory + name + "\\";
                if (directoryContents.ContainsKey(newDir))
                {
                    Console.WriteLine($"Error: Directory '{name}' already exists.");
                }
                else
                {
                    directoryContents[newDir] = new Dictionary<string, object>();
                    fileSystem[newDir] = new Dictionary<string, object>();

                    string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string physicalPath = Path.Combine(exeDirectory, newDir.Replace("\\", Path.DirectorySeparatorChar.ToString()));
                    Directory.CreateDirectory(physicalPath);

                    Console.WriteLine($"Directory '{name}' Created.");
                }
            }
        }
        else
        {
            Console.WriteLine("Error: Current Path Is Not A Directory.");
        }
    }

    // CD
    static void Cd(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Console.WriteLine("Error: Path not specified.");
            return;
        }

        string newPath;
        if (path == "..")
        {
            int lastSlash = currentDirectory.LastIndexOf('\\', currentDirectory.Length - 2);
            newPath = lastSlash > 0 ? currentDirectory.Substring(0, lastSlash + 1) : "C:\\";
        }
        else if (path.StartsWith("C:\\"))
        {
            newPath = path;
        }
        else
        {
            newPath = currentDirectory + path + "\\";
        }

        if (fileSystem.ContainsKey(newPath) && fileSystem[newPath] is Dictionary<string, object>)
        {
            currentDirectory = newPath;
        }
        else
        {
            Console.WriteLine("Error: Path not found.");
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
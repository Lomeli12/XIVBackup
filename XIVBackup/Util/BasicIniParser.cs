using System;
using System.Collections.Generic;
using System.IO;

namespace XIVBackup.Util;

//I couldn't find an basic ini parser I liked, so I made my own.
public class BasicIniParser {

    private IDictionary<string, string> iniData;

    public BasicIniParser(string path) {
        Path = path;
        iniData = new Dictionary<string, string>();
        read();
    }

    private void read() {
        if (!File.Exists(Path)) throw new FileNotFoundException();
        var fileLines = File.ReadAllLines(Path);
        if (fileLines.Length < 1) throw new Exception($"Empty Ini File: {Path}");
        iniData.Clear();
        foreach (var line in fileLines) {
            var splitLine = line.Split('=', 2);
            if (splitLine.Length < 1) continue;
            iniData[splitLine[0].ToLower()] = splitLine[1];
        }
    }

    public string get(string key) => iniData[key.ToLower()];

    public string Path { get; private set; }
}
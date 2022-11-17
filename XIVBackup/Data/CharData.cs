using System.IO;

namespace XIVBackup.Data;

public class CharData {
    private readonly XIVData[] characterData = {
        new("ACQ"),
        new("ADDON"),
        new("COMMON"),
        new("CONTROL0"),
        new("CONTROL1"),
        new("GEARSET"),
        new("GS"),
        new("HOTBAR"),
        new("ITEMFDR"),
        new("ITEMODR"),
        new("KEYBIND"),
        new("LOGFLTR"),
        new("MACRO"),
        new("UISAVE")
    };

    public CharData(string charPath, string charID) {
        CharacterID = charID;
        CharPath = charPath;
    }

    public CharData() { }

    public void readFiles() {
        var files = Directory.GetFiles(CharPath, "*.DAT");
        foreach (var file in files) {
            foreach (var dat in characterData) {
                if (file.EndsWith(dat.Name + ".DAT")) {
                    dat.Data = File.ReadAllBytes(file);
                }
            }
        }
    }

    public void writeFiles() {
        if (!Directory.Exists(CharPath)) Directory.CreateDirectory(CharPath);
        foreach (var data in characterData)
            File.WriteAllBytes(Path.Combine(CharPath, data.Name + ".DAT"), data.Data);
    }

    public void toBytes(BinaryWriter writer) {
        writer.Write(CharacterID);
        foreach (var data in characterData)
            data.toBytes(writer);
    }

    public void fromBytes(BinaryReader reader) {
        CharacterID = reader.ReadString();
        foreach (var data in characterData)
            data.fromBytes(reader);
    }

    public string CharacterID { get; private set; }

    public string CharPath { get; set;  }
}
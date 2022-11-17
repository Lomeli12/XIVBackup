using System.IO;

namespace XIVBackup.Data; 

public class XIVData {

    public XIVData(string name) {
        Name = name;
    }

    public void fromBytes(BinaryReader reader) {
        Name = reader.ReadString();
        var length = (int) reader.ReadDouble();
        Data = reader.ReadBytes(length);
    }

    public void toBytes(BinaryWriter writer) {
        writer.Write(Name);
        writer.Write((double) Data.Length);
        writer.Write(Data);
    }

    public string Name { get; private set;  }
    
    public byte[] Data { get; set; }
}
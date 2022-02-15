using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace XIVBackup;

public class BackupFile {
    public const string FF_EXT = ".fcbz";
    private const string MACROS_FILE = "MACROSYS.dat";
    private const string CHAR_FOLDER = "FFXIV_CHR";

    private readonly XIVData sysMacros = new("MACROSYS");
    private readonly List<CharData> characterData = new();
    private string ffPath = PlatformUtil.getOSPath();

    private BackupResults readFiles() {
        characterData.Clear();

        try {
            var macrosPath = Path.Combine(ffPath, MACROS_FILE);
            if (File.Exists(macrosPath))
                sysMacros.Data = File.ReadAllBytes(macrosPath);

            var dirs = Directory.GetDirectories(ffPath, CHAR_FOLDER + "*");
            foreach (var charDir in dirs) {
                var charData = new CharData(charDir, charDir.Substring(ffPath.Length + CHAR_FOLDER.Length + 1));
                charData.readFiles();
                characterData.Add(charData);
            }
        } catch (Exception) {
            return BackupResults.FAILED_TO_READ_DATA;
        }

        return BackupResults.READ_DATA_SUCCESS;
    }

    private BackupResults writeFiles() {
        try {
            if (!Directory.Exists(ffPath))
                Directory.CreateDirectory(ffPath);
            File.WriteAllBytes(Path.Combine(ffPath, MACROS_FILE), sysMacros.Data);
            if (characterData.Count >= 1) {
                foreach (var charData in characterData)
                    charData.writeFiles();
            }
        } catch (Exception) {
            return BackupResults.FAILED_TO_RESTORE_BACKUP;
        }

        return BackupResults.RESTORE_SUCCESS;
    }

    public BackupResults saveBackup(string path) {
        var results = readFiles();
        if (results != BackupResults.READ_DATA_SUCCESS)
            return results;

        using var stream = File.Open(path, FileMode.Create);
        using var zipStream = new GZipStream(stream, CompressionMode.Compress, false);
        using var writer = new BinaryWriter(zipStream, Encoding.UTF8, false);
        try {
            sysMacros.toBytes(writer);
            writer.Write((double) characterData.Count);
            if (characterData.Count > 0) {
                foreach (var charData in characterData) {
                    charData.toBytes(writer);
                }
            }
        } catch (Exception) {
            return BackupResults.FAILED_TO_WRITE_BACKUP;
        }
        return BackupResults.BACKUP_SUCCESS;
    }

    public BackupResults openBackup(string path) {
        characterData.Clear();

        try {
            using var stream = File.Open(path, FileMode.Open);
            using var zipStream = new GZipStream(stream, CompressionMode.Decompress, false);
            using var reader = new BinaryReader(zipStream, Encoding.UTF8, false);
            sysMacros.fromBytes(reader);
            var count = (int) reader.ReadDouble();
            if (count > 0) {
                for (var i = 0; i < count; i++) {
                    var charData = new CharData();
                    charData.fromBytes(reader);
                    charData.CharPath = Path.Combine(ffPath, CHAR_FOLDER + charData.CharacterID);
                    characterData.Add(charData);
                }
            }
        } catch (Exception) {
            return BackupResults.FAILED_TO_READ_BACKUP;
        }

        return writeFiles();
    }
}
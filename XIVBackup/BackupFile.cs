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

    private BackupResults readFiles(MainWindow parent) {
        characterData.Clear();

        try {
            var macrosPath = Path.Combine(PlatformUtil.getFFConfigPath(parent), MACROS_FILE);
            if (File.Exists(macrosPath))
                sysMacros.Data = File.ReadAllBytes(macrosPath);

            var dirs = Directory.GetDirectories(PlatformUtil.getFFConfigPath(parent), CHAR_FOLDER + "*");
            foreach (var charDir in dirs) {
                var charData = new CharData(charDir, charDir.Substring(
                    PlatformUtil.getFFConfigPath(parent).Length + CHAR_FOLDER.Length + 1));
                charData.readFiles();
                characterData.Add(charData);
            }
        } catch (Exception) {
            return BackupResults.FAILED_TO_READ_DATA;
        }

        return BackupResults.READ_DATA_SUCCESS;
    }

    private BackupResults writeFiles(MainWindow parent) {
        try {
            if (!Directory.Exists(PlatformUtil.getFFConfigPath(parent)))
                Directory.CreateDirectory(PlatformUtil.getFFConfigPath(parent));
            File.WriteAllBytes(Path.Combine(PlatformUtil.getFFConfigPath(parent), MACROS_FILE), sysMacros.Data);
            if (characterData.Count >= 1) {
                foreach (var charData in characterData)
                    charData.writeFiles();
            }
        } catch (Exception) {
            return BackupResults.FAILED_TO_RESTORE_BACKUP;
        }

        return BackupResults.RESTORE_SUCCESS;
    }

    public BackupResults saveBackup(MainWindow parent, string path) {
        var results = readFiles(parent);
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

    public BackupResults openBackup(MainWindow parent, string path) {
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
                    charData.CharPath = Path.Combine(PlatformUtil.getFFConfigPath(parent), CHAR_FOLDER + charData.CharacterID);
                    characterData.Add(charData);
                }
            }
        } catch (Exception) {
            return BackupResults.FAILED_TO_READ_BACKUP;
        }

        return writeFiles(parent);
    }
}
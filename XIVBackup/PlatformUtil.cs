using System;
using System.IO;
using System.Runtime.InteropServices;

namespace XIVBackup;

public static class PlatformUtil {
    private static string ffPath = "";

    private const string GAMES_DIR = "My Games";
    private const string FF_DIR = "FINAL FANTASY XIV - A Realm Reborn";

    // XIVLauncher directory names
    private const string XIV_WIN_CONFIG = "XIVLauncher";
    private const string XIV_PLUGIN_CONFIG = "pluginConfigs";

    // Wine Bottle directory names
    private const string WINE_DRIVE = "drive_c";
    private const string WINE_USERS = "users";
    private const string WINE_DOCS = "My Documents";

    // XIVLauncher flatpak directory names
    private const string XIV_LINUX_CONFIG = ".xlcore";
    private const string XIV_GAME_CONFIG = "ffxivConfig";

    public static string getFFConfigPath(MainWindow parent) {
        if (string.IsNullOrEmpty(ffPath)) {
            ffPath = getFFConfigPath();
            if (string.IsNullOrEmpty(ffPath) || !Directory.Exists(ffPath)) {
                parent.displayWarning();
                ffPath = parent.selectConfigFolder();
            }
        }
        return ffPath;
    }

    public static string getFFConfigPath() {
        if (Directory.GetCurrentDirectory().EndsWith(FF_DIR))
            return Directory.GetCurrentDirectory();
        // Default FF config folder path
        var path = multiCombinePath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            GAMES_DIR, FF_DIR);
        if (!Directory.Exists(path)) {
            var newPath = path;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                // XIVLauncher flatpak
                newPath = combineAndVerifyPath(path, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    XIV_LINUX_CONFIG, XIV_GAME_CONFIG);
                if (newPath.Equals(path)) {
                    // Lutris
                    newPath = combineAndVerifyPath(path, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "Games", "final-fantasy-xiv-online", WINE_DRIVE, WINE_USERS, Environment.UserName,
                        WINE_DOCS, GAMES_DIR, FF_DIR);
                }
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                // Assuming they're using XIVMac if they're not using the vanilla launcher
                newPath = combineAndVerifyPath(path, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Library", "Application Support", "XIV on Mac", "game", WINE_DRIVE, WINE_USERS,
                    "emet-selch", WINE_DOCS, GAMES_DIR, FF_DIR); // I like that the user is "Emet-Selch"
            }

            if (!string.IsNullOrWhiteSpace(newPath) && newPath.Equals(path)) path = null;
            else path = newPath;
        }

        return path;
    }

    private static string combineAndVerifyPath(string basePath, params string[] folders) {
        var newPath = multiCombinePath(folders);
        return Directory.Exists(newPath) ? newPath : basePath;
    }

    private static string multiCombinePath(params string[] folders) {
        switch (folders.Length) {
            case 0:
                return "";
            case <= 1:
                return folders[0];
        }

        var path = folders[0];
        for (var i = 1; i < folders.Length; i++)
            path = Path.Combine(path, folders[i]);
        return path;
    }
}
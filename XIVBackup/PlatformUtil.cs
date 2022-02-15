using System;
using System.IO;
using System.Runtime.InteropServices;

namespace XIVBackup;

public static class PlatformUtil {
    private const string GAMES_DIR = "My Games";
    private const string FF_DIR = "FINAL FANTASY XIV - A Realm Reborn";

    // Wine Bottle directory names
    private const string WINE_DRIVE = "drive_c";
    private const string WINE_USERS = "users";
    private const string WINE_DOCS = "My Documents";

    public static string getOSPath() {
        if (Directory.GetCurrentDirectory().EndsWith(FF_DIR))
            return Directory.GetCurrentDirectory();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Path.Combine(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), GAMES_DIR), FF_DIR);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            var path =  Path.Combine(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), GAMES_DIR), FF_DIR);
            if (!Directory.Exists(path)) {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    Path.Combine("Library",
                        Path.Combine("Application Support",
                            Path.Combine("XIV on Mac",
                                Path.Combine("game",
                                    Path.Combine(WINE_DRIVE,
                                        Path.Combine(WINE_USERS,
                                            Path.Combine("emet-selch", //I really like that's what XIVMac uses lol
                                                Path.Combine(WINE_DOCS,
                                                    Path.Combine(GAMES_DIR, FF_DIR)
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                );
            }

            return path;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            // We're just assuming they're using lutris
            // Also this looks ugly AF
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Path.Combine("Games",
                    Path.Combine("final-fantasy-xiv-online",
                        Path.Combine(WINE_DRIVE,
                            Path.Combine(WINE_USERS,
                                Path.Combine(Environment.UserName,
                                    Path.Combine(WINE_DOCS,
                                        Path.Combine(GAMES_DIR, FF_DIR)
                                    )
                                )
                            )
                        )
                    )
                )
            );
        }

        return null;
    }
}
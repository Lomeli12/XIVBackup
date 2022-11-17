using System;

namespace XIVBackup; 

public static class I18n {
    public static string localize(string localization) => localization.Replace(@"\n", Environment.NewLine);

    public static string localize(string localization, params object[] objs) {
        localization = localize(localization);
        return objs != null ? string.Format(localization, objs) : localization;
    }
}
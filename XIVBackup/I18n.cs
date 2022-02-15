using System;

namespace XIVBackup; 

public class I18n {
    private I18n() {}
    
    public static string localize(string localization) => localization.Replace(@"\n", Environment.NewLine);

    public static string localize(string localization, params object[] objs) {
        localization = localize(localization);
        return objs != null ? string.Format(localization, objs) : localization;
    }
}
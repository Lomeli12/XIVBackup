using System;
using System.IO;
using Gtk;
using XIVBackup.Data;
using XIVBackup.Util;
using UI = Gtk.Builder.ObjectAttribute;

namespace XIVBackup {
    public class MainWindow : Window {
#pragma warning disable CS0649
        // Labels
        [UI] private Label infoLabel;
        [UI] private Label backupTitleLabel;
        [UI] private Label backupLabel;
        [UI] private Label restoreTitleLabel;
        [UI] private Label restoreLabel;

        // Buttons
        [UI] private Button backupBtn;
        [UI] private Button restoreBtn;
        [UI] private Button closeBtn;
#pragma warning restore CS0649

        private readonly FileFilter fcbzFilter = new();
        private readonly FileFilter anyFilter = new();

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow")) {
            builder.Autoconnect(this);
            loadLocalization();

            fcbzFilter.AddPattern("*.fcbz");
            anyFilter.AddPattern("*");

            DeleteEvent += Window_DeleteEvent;

            backupBtn.ButtonReleaseEvent += backupBtnEvent;
            restoreBtn.ButtonReleaseEvent += restoreBtnEvent;
            closeBtn.ButtonReleaseEvent += closeBtnEvent;

            PlatformUtil.getFFConfigPath(this);
            //var path = PlatformUtil.getFFConfigPath();
            //if (path == null || !Directory.Exists(path))
            //    displayWarning();
        }

        public void displayWarning() {
            backupBtn.Sensitive = false;
            restoreBtn.Sensitive = false;

            var warning = new MessageDialog(this, DialogFlags.DestroyWithParent, MessageType.Error,
                ButtonsType.Ok, Localization.warning_autodetect);
            warning.Title = Localization.warning_autodetect_title;
            warning.Run();
            warning.Destroy();
        }

        private void loadLocalization() {
            // General localization
            Title = I18n.localize(Localization.xivbackup_title);
            infoLabel.Text = I18n.localize(Localization.info_label);
            fcbzFilter.Name = I18n.localize(Localization.filter_fcbz);
            anyFilter.Name = I18n.localize(Localization.filter_all);

            // Backup localization
            backupTitleLabel.Text = I18n.localize(Localization.backup_label_title);
            backupLabel.Text = I18n.localize(Localization.backup_label_text);
            backupBtn.Label = I18n.localize(Localization.backup_button_text);

            // Restore localization
            restoreTitleLabel.Text = I18n.localize(Localization.restore_label_title);
            restoreLabel.Text = I18n.localize(Localization.restore_label_text);
            restoreBtn.Label = I18n.localize(Localization.restore_button_text);

            // Close Localization
            closeBtn.Label = I18n.localize(Localization.close_button_text);
        }

        public string selectConfigFolder() {
            var path = "";

            var pathSelect = new FileChooserDialog(I18n.localize(""), this,
                FileChooserAction.SelectFolder);
            pathSelect.AddButton(Stock.Ok, ResponseType.Accept);
            pathSelect.AddButton(Stock.Cancel, ResponseType.Cancel);
            pathSelect.DefaultResponse = ResponseType.Accept;

            var response = (ResponseType)pathSelect.Run();
            if (response == ResponseType.Accept)
                path = pathSelect.CurrentFolder;
            
            Console.WriteLine(path);
            
            pathSelect.Destroy();
            return path;
        }

        private void backupBtnEvent(object sender, ButtonReleaseEventArgs args) {
            var saveBackupDialog = new FileChooserDialog(I18n.localize(Localization.backup_label_title), this,
                FileChooserAction.Save);
            saveBackupDialog.AddFilter(fcbzFilter);
            saveBackupDialog.AddFilter(anyFilter);
            saveBackupDialog.AddButton(Stock.Save, ResponseType.Accept);
            saveBackupDialog.AddButton(Stock.Cancel, ResponseType.Cancel);
            saveBackupDialog.DefaultResponse = ResponseType.Accept;

            var response = (ResponseType)saveBackupDialog.Run();
            if (response == ResponseType.Accept) {
                var fileName = saveBackupDialog.Filename;
                if (!fileName.EndsWith(BackupFile.FF_EXT))
                    fileName += BackupFile.FF_EXT;
                if (File.Exists(fileName)) {
                    var warning = new MessageDialog(this, DialogFlags.DestroyWithParent, MessageType.Warning,
                        ButtonsType.YesNo, I18n.localize(
                            Localization.backup_exists_warn, System.IO.Path.GetFileName(fileName))
                    );

                    var warnResponse = (ResponseType)warning.Run();
                    warning.Destroy();
                    if (warnResponse == ResponseType.No) {
                        saveBackupDialog.Destroy();
                        return;
                    }
                }

                var backup = new BackupFile();
                handleResults(backup.saveBackup(this, fileName), fileName);
            }

            saveBackupDialog.Destroy();
        }

        private void restoreBtnEvent(object sender, ButtonReleaseEventArgs args) {
            var openBackupDialog = new FileChooserDialog(I18n.localize(Localization.backup_label_title), this,
                FileChooserAction.Open);
            openBackupDialog.AddFilter(fcbzFilter);
            openBackupDialog.AddFilter(anyFilter);
            openBackupDialog.AddButton(Stock.Open, ResponseType.Accept);
            openBackupDialog.AddButton(Stock.Cancel, ResponseType.Cancel);
            openBackupDialog.DefaultResponse = ResponseType.Accept;

            var response = (ResponseType)openBackupDialog.Run();
            if (response == ResponseType.Accept) {
                var fileName = openBackupDialog.Filename;
                if (File.Exists(fileName)) {
                    var backup = new BackupFile();
                    handleResults(backup.openBackup(this, fileName), fileName);
                }
            }

            openBackupDialog.Destroy();
        }

        private void handleResults(BackupResults results, string path) {
            var message = new MessageDialog(this, DialogFlags.DestroyWithParent, MessageType.Info,
                ButtonsType.Ok, I18n.localize(Localization.results_empty));
            message.Text = results switch {
                BackupResults.BACKUP_SUCCESS => I18n.localize(Localization.results_backup_success, path),
                BackupResults.FAILED_TO_READ_DATA => I18n.localize(Localization.results_backup_error_read,
                    PlatformUtil.getFFConfigPath(this)),
                BackupResults.FAILED_TO_WRITE_BACKUP => I18n.localize(Localization.results_backup_error_write, path),
                BackupResults.RESTORE_SUCCESS => I18n.localize(Localization.results_restore_success, path),
                BackupResults.FAILED_TO_READ_BACKUP => I18n.localize(Localization.results_restore_error_read, path),
                BackupResults.FAILED_TO_RESTORE_BACKUP => I18n.localize(Localization.results_restore_error_write,
                    PlatformUtil.getFFConfigPath(this)),
                _ => I18n.localize(Localization.results_error_default, results)
            };

            message.Run();
            message.Destroy();
        }

        private void closeBtnEvent(object sender, ButtonReleaseEventArgs args) {
            Application.Quit();
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a) {
            Application.Quit();
        }
    }
}
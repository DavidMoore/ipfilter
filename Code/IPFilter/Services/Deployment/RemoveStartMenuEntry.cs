namespace IPFilter.Services.Deployment
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class RemoveStartMenuEntry : IUninstallStep
    {
        private readonly UninstallInfo _uninstallInfo;
        private List<string> _foldersToRemove;
        private List<string> _filesToRemove;

        public RemoveStartMenuEntry(UninstallInfo uninstallInfo)
        {
            _uninstallInfo = uninstallInfo;
        }

        public void Prepare(List<string> componentsToRemove)
        {
            var programsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            var folder = Path.Combine(programsFolder, _uninstallInfo.ShortcutFolderName);
            var suiteFolder = Path.Combine(folder, _uninstallInfo.ShortcutSuiteName ?? string.Empty);
            var shortcut = Path.Combine(suiteFolder, _uninstallInfo.ShortcutFileName + ".appref-ms");
            var supportShortcut = Path.Combine(suiteFolder, _uninstallInfo.SupportShortcutFileName + ".url");

            var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var desktopShortcut = Path.Combine(desktopFolder, _uninstallInfo.ShortcutFileName + ".appref-ms");

            _filesToRemove = new List<string>();
            if (File.Exists(shortcut)) _filesToRemove.Add(shortcut);
            if (File.Exists(supportShortcut)) _filesToRemove.Add(supportShortcut);
            if (File.Exists(desktopShortcut)) _filesToRemove.Add(desktopShortcut);

            _foldersToRemove = new List<string>();
            if (Directory.Exists(suiteFolder) && Directory.GetFiles(suiteFolder).All(d => _filesToRemove.Contains(d)))
            {
                _foldersToRemove.Add(suiteFolder);

                if (Directory.GetDirectories(folder).Count() == 1 && !Directory.GetFiles(folder).Any())
                    _foldersToRemove.Add(folder);
            }
        }

        public void PrintDebugInformation()
        {
            if (_foldersToRemove == null)
                throw new InvalidOperationException("Call Prepare() first.");

            var programsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            Console.WriteLine("Remove start menu entries from " + programsFolder);

            foreach (var file in _filesToRemove)
            {
                Console.WriteLine("Delete file " + file);
            }

            foreach (var folder in _foldersToRemove)
            {
                Console.WriteLine("Delete folder " + folder);
            }

            Console.WriteLine();
        }

        public void Execute()
        {
            if (_foldersToRemove == null)
                throw new InvalidOperationException("Call Prepare() first.");

            try
            {
                foreach (var file in _filesToRemove)
                {
                    File.Delete(file);
                }

                foreach (var folder in _foldersToRemove)
                {
                    Directory.Delete(folder, false);
                }
            }
            catch (IOException)
            {
            }
        }

        public void Dispose()
        {
        }
    }
}

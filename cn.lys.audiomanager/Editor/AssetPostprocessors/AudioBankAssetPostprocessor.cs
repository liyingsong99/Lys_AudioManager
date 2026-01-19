using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Lys.Audio.Editor
{
    public class AudioBankAssetPostprocessor : AssetPostprocessor
    {
        private static readonly HashSet<string> SupportedExtensions = new HashSet<string>
        {
            ".wav",
            ".mp3",
            ".ogg",
            ".aiff",
            ".aif"
        };

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var changedPaths = new HashSet<string>();

            foreach (var path in importedAssets.Concat(deletedAssets).Concat(movedAssets).Concat(movedFromAssetPaths))
            {
                if (IsAudioClipPath(path))
                {
                    var dirPath = Path.GetDirectoryName(path)?.Replace("\\", "/");
                    if (!string.IsNullOrEmpty(dirPath))
                    {
                        changedPaths.Add(dirPath);
                    }
                }
            }

            if (changedPaths.Count == 0)
            {
                return;
            }

            var bankGuids = AssetDatabase.FindAssets("t:AudioBank");
            foreach (var guid in bankGuids)
            {
                var bankPath = AssetDatabase.GUIDToAssetPath(guid);
                var bank = AssetDatabase.LoadAssetAtPath<AudioBank>(bankPath);
                if (bank == null)
                {
                    continue;
                }

                bool needsUpdate = false;
                foreach (var folder in bank.FolderEntries)
                {
                    if (folder == null || string.IsNullOrEmpty(folder.folderPath))
                    {
                        continue;
                    }

                    var normalizedFolderPath = folder.folderPath.Replace("\\", "/");

                    foreach (var changedPath in changedPaths)
                    {
                        bool isInFolder = changedPath.StartsWith(normalizedFolderPath);
                        bool isInSubfolder = folder.includeSubfolders && changedPath.StartsWith(normalizedFolderPath + "/");

                        if (isInFolder || isInSubfolder)
                        {
                            needsUpdate = true;
                            break;
                        }
                    }

                    if (needsUpdate)
                    {
                        break;
                    }
                }

                if (needsUpdate)
                {
                    RescanAndSaveBank(bank);
                }
            }
        }

        private static bool IsAudioClipPath(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(ext) && SupportedExtensions.Contains(ext);
        }

        private static void RescanAndSaveBank(AudioBank bank)
        {
            AudioFolderScanner.ScanAllFolders(bank);

            Debug.Log($"[AudioManager] Auto-updated AudioBank: {bank.BankName}");
        }
    }
}

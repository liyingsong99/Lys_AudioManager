using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Lys.Audio.Editor
{
    public static class AudioFolderScanner
    {
        private static readonly HashSet<string> SupportedExtensions = new HashSet<string>
        {
            ".wav",
            ".mp3",
            ".ogg",
            ".aiff",
            ".aif"
        };

        public static List<AudioClipEntry> ScanFolder(string folderPath, bool includeSubfolders = true)
        {
            var entries = new List<AudioClipEntry>();

            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogWarning("[AudioFolderScanner] Folder path is null or empty");
                return entries;
            }

            string assetPath = folderPath;
            if (!assetPath.StartsWith("Assets"))
            {
                assetPath = "Assets/" + assetPath;
            }

            string fullPath = Path.GetFullPath(assetPath);
            if (!Directory.Exists(fullPath))
            {
                Debug.LogWarning($"[AudioFolderScanner] Folder does not exist: {fullPath}");
                return entries;
            }

            SearchOption searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            string[] files = Directory.GetFiles(fullPath, "*.*", searchOption);

            foreach (string file in files)
            {
                string extension = Path.GetExtension(file).ToLowerInvariant();
                if (!SupportedExtensions.Contains(extension))
                {
                    continue;
                }

                string relativePath = GetRelativePath(file);
                if (string.IsNullOrEmpty(relativePath))
                {
                    continue;
                }

                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(relativePath);
                if (clip == null)
                {
                    continue;
                }

                var entry = new AudioClipEntry
                {
                    clipName = Path.GetFileNameWithoutExtension(file),
                    assetPath = relativePath,
                    editorPreviewClip = clip
                };

                entries.Add(entry);
            }

            Debug.Log($"[AudioFolderScanner] Scanned {entries.Count} audio clips from: {assetPath}");
            return entries;
        }

        public static void ScanFolderEntry(AudioFolderEntry folderEntry)
        {
            if (folderEntry == null)
            {
                return;
            }

            string folderPath = folderEntry.folderPath;
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogWarning("[AudioFolderScanner] FolderEntry has no folder path");
                return;
            }

            var entries = ScanFolder(folderPath, folderEntry.includeSubfolders);

            if (!string.IsNullOrEmpty(folderEntry.namePrefix))
            {
                foreach (var entry in entries)
                {
                    entry.clipName = folderEntry.namePrefix + entry.clipName;
                }
            }

            folderEntry.scannedClips = entries;

            folderEntry.ApplyOverridesToScannedClips();

            Debug.Log($"[AudioFolderScanner] Updated folder entry: {folderEntry.folderPath}, {entries.Count} clips");
        }

        public static void ScanAllFolders(AudioBank bank)
        {
            if (bank == null)
            {
                return;
            }

            foreach (var folderEntry in bank.FolderEntries)
            {
                ScanFolderEntry(folderEntry);
            }

            EditorUtility.SetDirty(bank);
            AssetDatabase.SaveAssets();

            Debug.Log($"[AudioFolderScanner] Scanned all folders in bank: {bank.BankName}");
        }

        private static string GetRelativePath(string fullPath)
        {
            fullPath = fullPath.Replace('\\', '/');
            int assetsIndex = fullPath.IndexOf("Assets/");
            if (assetsIndex < 0)
            {
                assetsIndex = fullPath.IndexOf("Assets\\");
            }

            if (assetsIndex >= 0)
            {
                return fullPath.Substring(assetsIndex);
            }

            return null;
        }

        public static bool ValidateFolderPath(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return false;
            }

            string assetPath = folderPath;
            if (!assetPath.StartsWith("Assets"))
            {
                assetPath = "Assets/" + assetPath;
            }

            return AssetDatabase.IsValidFolder(assetPath);
        }

        public static int GetAudioClipCount(string folderPath, bool includeSubfolders = true)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return 0;
            }

            string assetPath = folderPath;
            if (!assetPath.StartsWith("Assets"))
            {
                assetPath = "Assets/" + assetPath;
            }

            string fullPath = Path.GetFullPath(assetPath);
            if (!Directory.Exists(fullPath))
            {
                return 0;
            }

            SearchOption searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] files = Directory.GetFiles(fullPath, "*.*", searchOption);

            int count = 0;
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file).ToLowerInvariant();
                if (SupportedExtensions.Contains(extension))
                {
                    count++;
                }
            }

            return count;
        }
    }
}

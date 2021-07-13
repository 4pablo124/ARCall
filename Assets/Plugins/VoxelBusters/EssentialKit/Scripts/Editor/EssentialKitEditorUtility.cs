using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VoxelBusters.CoreLibrary.Editor;
using VoxelBusters.EssentialKit;
using VoxelBusters.CoreLibrary.NativePlugins.UnityUI;
using VoxelBusters.CoreLibrary.NativePlugins;

namespace VoxelBusters.EssentialKit.Editor
{
    public static class EssentialKitEditorUtility
    {
        #region Asset methods

        public static EssentialKitSettings GetOrCreateEssentialKitSettings()
        {
            return GetEssentialKitSettings(throwError: false) ?? CreateEssentialKitSettings();
        }

        public static EssentialKitSettings CreateEssentialKitSettings()
        {
            string  filePath    = Constants.kPluginSettingsFileFullPath;
            var     settings    = ScriptableObject.CreateInstance<EssentialKitSettings>();
            InitialiseSettingsObjectIfRequired(settings);

            // create file
            AssetDatabaseUtility.CreateAssetAtPath(settings, filePath);
            AssetDatabase.Refresh();

            return settings;
        }

        public static EssentialKitSettings GetEssentialKitSettings(bool throwError = true)
        {
            string  filePath    = Constants.kPluginSettingsFileFullPath;
            var     settings    = AssetDatabaseUtility.LoadAssetAtPath<EssentialKitSettings>(filePath);
            if (settings)
            {
                InitialiseSettingsObjectIfRequired(settings);
                return settings;
            }

            if (throwError)
            {
                throw Diagnostics.PluginNotConfiguredException();
            }

            return null;
        }

        public static bool CheckWhetherPluginIsConfigured(out EssentialKitSettings settings, bool showError = true)
        {
            settings    = GetEssentialKitSettings(throwError: false);
            if (settings)
            {
                return true;
            }

            if (showError)
            {
                EditorUtility.DisplayDialog("Error", "Native plugins is not configured. Please select plugin settings file from menu and configure it according to your preference.", "Ok");
            }

            return false;
        }

        #endregion

        #region Private static methods

        private static void InitialiseSettingsObjectIfRequired(EssentialKitSettings settings)
        {
            // set properties
            var     uiCollection        = settings.NativeUISettings.CustomUICollection;
            if (uiCollection.RendererPrefab == null)
            {
                uiCollection.RendererPrefab         = AssetDatabaseUtility.LoadAssetAtPath<UnityUIRenderer>(UnityUIUtility.kDefaultUnityUIRendererPrefabFullPath);
            }
            if (uiCollection.AlertDialogPrefab == null)
            {
                var     prefabObject    = AssetDatabaseUtility.LoadAssetAtPath<GameObject>(UnityUIUtility.kDefaultUnityUIAlertDialogPrefabFullPath);
                if (prefabObject)
                {
                    uiCollection.AlertDialogPrefab  = prefabObject.GetComponent<UnityUIAlertDialog>();
                }
            }
        }

        #endregion
    }
}
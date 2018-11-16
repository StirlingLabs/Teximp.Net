/*
* Copyright (c) 2012-2018 TeximpNet - Nicholas Woodfield
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using TeximpNet.Unmanaged;
using System.IO;
using UnityEngine;

namespace TeximpNet
{
    /// <summary>
    /// TeximpNet Unity integration. This handles one-time initialization (before scene load) of the FreeImageLibrary and NvTextureToolsLibrary instances, setting DLL probing
    /// paths to load the correct native dependencies, if the current platform is supported.
    /// </summary>
    public class TeximpUnity
    {
        private static bool s_triedLoading = false;
        private static bool s_freeImageAvailable = false;
        private static bool s_nvttAvailable = false;

        /// <summary>
        /// Gets if the FreeImage library is available on this platform (e.g. the library can load native dependencies).
        /// </summary>
        public static bool IsFreeImageAvailable
        {
            get
            {
                return s_freeImageAvailable;
            }
        }

        /// <summary>
        /// Gets if the Nvidia Texture Tools (Nvtt) library is available on this platform (e.g. the library can load native dependencies).
        /// </summary>
        public static bool IsNvTextureToolsAvailable
        {
            get
            {
                return s_nvttAvailable;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializePlugin()
        {
            if(s_triedLoading)
                return;

            s_freeImageAvailable = InitializeFreeImage();
            s_nvttAvailable = InitializeNvtt();
        }

        private static bool InitializeFreeImage()
        {
            return InitializeUnmanagedLibrary(FreeImageLibrary.Instance);
        }

        private static bool InitializeNvtt()
        {
            return InitializeUnmanagedLibrary(NvTextureToolsLibrary.Instance);
        }

        private static bool InitializeUnmanagedLibrary(UnmanagedLibrary libInstance)
        {
            //If already initialized, set flags and return
            if(libInstance.IsLibraryLoaded)
                return true;

            //First time initialization, need to set a probing path (at least in editor) to resolve the native dependencies
            string pluginsFolder = Path.Combine(Application.dataPath, "Plugins");
            string editorPluginNativeFolder = Path.Combine(pluginsFolder, "TeximpNet", "Native");
            string native64LibPath = null;
            string native32LibPath = null;

            //Set if any platform needs to tweak the default name AssimpNet uses for the platform, null clears using an override at all
            string override64LibName = null;
            string override32LibName = null;

            //Setup DLL paths based on platforms. When run inside the editor, the path will be to the AssimpNet plugin folder structure. When in standalone,
            //Unity copies the native DLLs for the specific target architecture into a single Plugin folder.
            switch(Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    native64LibPath = Path.Combine(editorPluginNativeFolder, "win", "x86_64");
                    native32LibPath = Path.Combine(editorPluginNativeFolder, "win", "x86");
                    break;
                case RuntimePlatform.WindowsPlayer:
                    //Seems like windows they are not added to any specific folder, just dropped inside Plugins folder
                    native64LibPath = pluginsFolder;
                    native32LibPath = pluginsFolder;
                    break;
                case RuntimePlatform.LinuxEditor:
                    native64LibPath = Path.Combine(editorPluginNativeFolder, "linux", "x86_64");
                    native32LibPath = Path.Combine(editorPluginNativeFolder, "linux", "x86");
                    break;
                case RuntimePlatform.LinuxPlayer:
                    //Linux standalone creates subfolders presumably since it allows "universal" types
                    native64LibPath = Path.Combine(pluginsFolder, "x86_64");
                    native32LibPath = Path.Combine(pluginsFolder, "x86");
                    break;
                case RuntimePlatform.OSXEditor:
                    native64LibPath = Path.Combine(editorPluginNativeFolder, "osx", "x86_64");
                    native32LibPath = Path.Combine(editorPluginNativeFolder, "osx", "x86");
                    
                    //In order to get unity to accept the dylib, had to rename it as *.bundle. Set an override name so we try and load that file. Seems to load
                    //fine.
                    string bundlelibName = Path.ChangeExtension(libInstance.DefaultLibraryName, ".bundle");
                    override64LibName = bundlelibName;
                    override32LibName = bundlelibName;
                    break;
                case RuntimePlatform.OSXPlayer:
                    native64LibPath = pluginsFolder;
                    native32LibPath = pluginsFolder;

                    //In order to get unity to accept the dylib, had to rename it as *.bundle. Set an override name so we try and load that file. Seems to load
                    //fine.
                    string bundlelibName = Path.ChangeExtension(libInstance.DefaultLibraryName, ".bundle");
                    override64LibName = bundlelibName;
                    override32LibName = bundlelibName;
                    break;
                    //TODO: Add more platforms if you have binaries that can run on it
            }

            //If both null, then we do not support the platform
            if(native64LibPath == null && native32LibPath == null)
            {
                Debug.Log(string.Format("Teximp does not support platform: {0}", Application.platform.ToString()));
                return false;
            }

            //Set resolver properties, null will clear the property
            libInstance.Resolver.SetOverrideLibraryName64(override64LibName);
            libInstance.Resolver.SetOverrideLibraryName32(override32LibName);
            libInstance.Resolver.SetProbingPaths64(native64LibPath);
            libInstance.Resolver.SetProbingPaths32(native32LibPath);
            libInstance.ThrowOnLoadFailure = false;

            //Try and load the native library, if failed we won't get an exception
            bool success = libInstance.LoadLibrary();

            //Turn exceptions back on
            libInstance.ThrowOnLoadFailure = true;

            return success;
        }
    }
}
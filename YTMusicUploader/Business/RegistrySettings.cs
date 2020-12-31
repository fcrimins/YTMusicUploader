﻿using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YTMusicUploader.Business
{
    /// <summary>
    /// Windows registry helper methods
    /// </summary>
    public class RegistrySettings
    {
        /// <summary>
        /// For setting whether the app starts with Windows or not.
        /// Uses the 'CurrentVersion\Run' sub key in the users key
        /// </summary>
        /// <param name="startWithWindows">Set to start with windows if true, false otherwise</param>
        public async static Task SetStartWithWindows(bool startWithWindows)
        {
            try
            {
                if (startWithWindows)
                {
                    string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                    var key = Registry.CurrentUser.OpenSubKey(path, true);
                    key.SetValue("YT Music Uploader", "\"" + Application.ExecutablePath.ToString() + "\" -hidden");
                }
                else
                {
                    string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                    var key = Registry.CurrentUser.OpenSubKey(path, true);
                    key.DeleteValue("YT Music Uploader", false);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e, "SetStartWithWindows");
            }

            await Task.Run(() => { });
        }
    }
}

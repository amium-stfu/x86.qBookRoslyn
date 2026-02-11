using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QB.UI
{
    public class Misc
    {
        /// <summary>
        /// Plays a beep
        /// </summary>
        /// <param name="frequency">in Hz</param>
        /// <param name="duration">in millis</param>
        public static void Beep(int frequency = 440, int duration = 200)
        {
            Task.Run(() => Console.Beep(frequency, duration));
        }


        /// <summary>
        /// Plays a sound-file
        /// </summary>
        /// <param name="filename">Full path to the sound-file</param>
        public static void PlaySound(string filename)
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(filename);
            player.Play();
        }


        /// <summary>
        /// Plays the 'asterisk' system sound
        /// </summary>
        public static void PlaySystemSoundAsterisk()
        {
            System.Media.SystemSounds.Asterisk.Play();
        }
        /// <summary>
        /// Plays the 'beep' system sound
        /// </summary>
        public static void PlaySystemSoundBeep()
        {
            System.Media.SystemSounds.Beep.Play();
        }
        /// <summary>
        /// Plays the 'exclamation' system sound
        /// </summary>
        public static void PlaySystemSoundExclamation()
        {
            System.Media.SystemSounds.Exclamation.Play();
        }
        /// <summary>
        /// Plays the 'hand' system sound (=Critical Stop)
        /// </summary>
        public static void PlaySystemSoundHand()
        {
            System.Media.SystemSounds.Hand.Play();
        }
        /// <summary>
        /// Plays the 'question' system sound
        /// </summary>
        public static void PlaySystemSoundQuestion()
        {
            System.Media.SystemSounds.Question.Play();
        }

    }

    public class Windows
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

        [ComImport, Guid("4ce576fa-83dc-4F88-951c-9d0782b4e376")]
        class UIHostNoLaunch
        {
        }

        [ComImport, Guid("37c994e7-432b-4834-a2f7-dce1f13b834b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface ITipInvocation
        {
            void Toggle(IntPtr hwnd);
        }

        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();

        public class NoFocusButton : Button
        {
            public NoFocusButton()
            {
                SetStyle(ControlStyles.Selectable, false);
            }
        }

        public static bool ToggleTabTip()
        {
            try
            {
                var uiHostNoLaunch = new UIHostNoLaunch();
                var tipInvocation = (ITipInvocation)uiHostNoLaunch;
                tipInvocation.Toggle(GetDesktopWindow());
                Marshal.ReleaseComObject(uiHostNoLaunch);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void OpenTouchKeyboard()
        {
            //string commonProgramFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
            var programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            string tabTipExePath = System.IO.Path.Combine(programFiles, "Common Files", "Microsoft Shared", "ink", "TabTip.exe");
            string tabTip32ExePath = System.IO.Path.Combine(programFiles, "Common Files", "Microsoft Shared", "ink", "TabTip32.exe");
            if (System.IO.File.Exists(tabTipExePath))
                ShellExecute(IntPtr.Zero, "open", tabTipExePath, "", "", 0);
            else if (System.IO.File.Exists(tabTip32ExePath))
                ShellExecute(IntPtr.Zero, "open", tabTip32ExePath, "", "", 0);
        }
    }


}

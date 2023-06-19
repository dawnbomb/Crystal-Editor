using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Crystal_Editor
{
    public partial class Workshop
    {

        ///Discord is missing, remember to add it later.

        // I plan to remove this entire menu / file later, as i change over to the new menus system.


        private void OpenMelonDS(object sender, RoutedEventArgs e)
        {

            Process Process = new Process();
            Process.StartInfo.FileName = @ExePath + "\\Emulators\\MelonDS\\melonDS.exe";
            Process.Start();
        }
                

        private void ToolDSBuff(object sender, RoutedEventArgs e)
        {
            Process yourProcess = new Process();
            yourProcess.StartInfo.FileName = @ExePath + "\\Tools\\Console\\Nintendo DS\\File Converter - DSBuff\\dsbuff.exe";
            yourProcess.Start();
        }

        private void ToolTinke(object sender, RoutedEventArgs e)
        {
            Process yourProcess = new Process();
            yourProcess.StartInfo.FileName = @ExePath + "\\Tools\\Console\\Nintendo DS\\Tinke\\Tinke.exe";
            yourProcess.Start();
        }
    }
}

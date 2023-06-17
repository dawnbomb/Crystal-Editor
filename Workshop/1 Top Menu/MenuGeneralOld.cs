using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Crystal_Editor
{
    public partial class Workshop
    {
        // I plan to remove this entire menu / file later, as i change over to the new menus system.

        private void MenuNPlusPlus(object sender, RoutedEventArgs e)
        {
            Process yourProcess = new Process();
            yourProcess.StartInfo.FileName = @ExePath + "\\Tools\\General\\Text Editor - N++\\notepad++.exe";
            yourProcess.Start();
        }

        private void MenuHxD(object sender, RoutedEventArgs e)
        {
            Process yourProcess = new Process();
            yourProcess.StartInfo.FileName = @ExePath + "\\Tools\\General\\Hex Editor - HxD\\HxD64.exe";
            yourProcess.Start();
        }

        private void Menu010(object sender, RoutedEventArgs e)
        {
            Process yourProcess = new Process();
            yourProcess.StartInfo.FileName = @ExePath + "\\Tools\\General\\Hex Editor - 010\\010EditorPortable.exe";
            yourProcess.Start();
        }


        private void MenuFLIPS(object sender, RoutedEventArgs e)
        {
            Process yourProcess = new Process();
            yourProcess.StartInfo.FileName = @ExePath + "\\Tools\\General\\Patch - Floating IPS\\flips.exe";
            yourProcess.Start();
        }

        private void MenuDeltaPatcher(object sender, RoutedEventArgs e)
        {
            Process yourProcess = new Process();
            yourProcess.StartInfo.FileName = @ExePath + "\\Tools\\General\\Patch - DeltaPatcher\\DeltaPatcher.exe";
            yourProcess.Start();
        }

        private void MenuUpset(object sender, RoutedEventArgs e)
        {
            Process yourProcess = new Process();
            yourProcess.StartInfo.FileName = @ExePath + "\\Tools\\General\\Patch - upset\\upset.exe";
            yourProcess.Start();
        }








    }
}

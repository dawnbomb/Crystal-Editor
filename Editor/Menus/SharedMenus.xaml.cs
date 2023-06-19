using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Crystal_Editor
{
    // The tools dictionary is actually in the game library. The dictionary gets made there, but populated here, so that everything involving the actual tools themself is here all in one file.
    // The game library then sets the shared menu with access to the dictionary. 
    // Hopefully later when the workshop is added, the workshop will also set the shared menu with access to the dictionary.

    public class StartupTools
    {       

        public void StartTools(Dictionary<string, Tool> Tools)
        {    
            Tools.Add("Notepad++",    new Tool {  Name = "Notepad++",      Application = "notepad++.exe",     Description = "The overall best text editor. Please use this instead of notepad :(" });
            Tools.Add("UABEAvalonia", new Tool {  Name = "UABEAvalonia",   Application = "UABEAvalonia.exe",  Description = "A tool for exporting and importing anything from a unity bundle."    });
            Tools.Add("MelonDS",      new Tool {  Name = "MelonDS",        Application = "melonDS.exe",       Description = "A Nintendo DS Emulator ideal for modding games due to it's save file format and smooth fastforward, unlike Desmume or No$GBA." });
            Tools.Add("FakeTool",     new Tool {  Name = "FakeTool",       Application = "FakeTool.exe",      Description = "A FakeTool." });
        }
    }
    
    
    public class Tool
    {
        public string Name { get; set; } = ""; //The name of the tool. IE Notepad++. This is what shows up in menus. It's also the key, but i've come to dislike having to refer to keys as variables.
        public string Application { get; set; } = ""; //The actual name of the executable (including the ".exe" suffix). IE notepad++.exe
        public string Description { get; set; } = ""; //A longer description of the tool. Appears in the tool setup window, and possibly as a tooltip.
        public string Location { get; set; } = ""; //The users location / path to the exe for this tool on their computer. IE "C:\Program Files\Notepad++\notepad++.exe"
        public string General { get; set; } = ""; //If the tool appears in the General Menu.
        public string Workshop { get; set; } = ""; //If the currently selected workshop uses this tool or not. [Yes, No, Event].              
        
        

    }

    public partial class SharedMenus : System.Windows.Controls.UserControl
    {
        public Dictionary<string, Tool> Tools { get; set; }

        string ToolsFolder = "";
        string WorkshopTools = "";
        string ExePath = "";

        public SharedMenus()
        {
            InitializeComponent();

            

            this.Loaded += SharedMenus_Loaded;
        }

        private void SharedMenus_Loaded(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            if (parentWindow is GameLibrary gameLibraryWindow)
            {
                ToolsFolder = gameLibraryWindow.ExePath + "\\Tools";
            }
            else if (parentWindow is Workshop workshopWindow)
            {
                ToolsFolder = workshopWindow.ExePath + "\\Tools";
            }
        }

        

        
















        public string SearchForApplication(string ToolName)
        {
            //var parentWindow = Window.GetWindow(this);
            //if (parentWindow is GameLibrary gameLibraryWindow)
            //{
            //    ToolsFolder = gameLibraryWindow.ExePath + "\\Tools";
            //}
            //else if (parentWindow is Workshop workshopWindow)
            //{
            //    ToolsFolder = workshopWindow.ExePath + "\\Tools";
            //}

            string AppLocation = FindExeInDirectory(ToolsFolder, ToolName);
            //If null or empty, search location.
            return AppLocation;
        }



        public void ApplicationNotFound(string ToolName)
        {
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("The tool " + ToolName + " was not found in it's last known location, nor was it anywhere inside the Tools folder.", ToolName + " not found", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {

            }
        }


        private string FindExeInDirectory(string path, string exeName)
        {
            foreach (var file in System.IO.Directory.GetFiles(path, exeName, System.IO.SearchOption.AllDirectories)) { return file; }
            return null;
        }



        public void RunTool(Tool Tool)
        {            

            string AppLocation = SearchForApplication(Tools[Tool.Name].Application);

            if (!string.IsNullOrEmpty(AppLocation))
            {
                System.Diagnostics.Process.Start(AppLocation);
            }
            else
            {
                ApplicationNotFound(Tool.Name);

            }
        }

        



































    }
}

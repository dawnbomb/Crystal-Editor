using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using Path = System.IO.Path;
//using System.Windows.Shapes;

namespace Crystal_Editor
{

    // This is the "start" of the program. Stuff here is kind of unorganized, but it's not that bad.
    // This file doesn't really interact with other files for the most part.
    // Click the wiki launches the Tutorial.xaml in the Tutorial folder. It's a wiki of everything to do with crystal editor and related, and teach stuff. 
    // the wiki is extremely under-developed, and should be probably entirely overhauled. The wiki does not interact with other files. (other then in the tutorial folder) (i should rename folder to wiki...)

    // When this program starts, it scans the workshops folder for every folder name. The list of workshops is just the direct folder names.
    // Clicking a workshop loads information from that workshop folder / LibraryInfo.xml. This window entirely handles saving and loading info for that file.
    // Clicking a workshop also loads info from Projects/WorkshopName/ for every folder inside, loads ProjectInfo into a list onscreen. Info in ProjectInfo.xml is used for this screen, NOT the actual project / workshop. 
    // Clicking to launch a project, opens workshop.xaml.cs and is the main part of the program, and extremely unorganized and messy.


    public class DataItem
    {
        /*public ICommand ButtonCommand { get; set; }*/
        public string ProjectName { get; set; }
        public string ProjectInputDirectory { get; set; }
        public string ProjectOutputDirectory { get; set; }
    }

    //1 open command prompt
    //2 navigate to the .csproj file folder   M:    cd X
    //3 copy paste this and hit enter
    //dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

    



    public partial class GameLibrary : Window
    {
        public string ExePath; //The location of the exe of crystal editor.

        string WorkshopName; //The name of the workshop (IE name of whats selected in Game Library)
        string Platform; //What platform of game the workshop is editing.
        string Region; //What game region the game is, to seperate USA from europe, as they often have diffrences.
        string Version; //the game version number, patch, expansion, etc.
        string Emulator; //What emulator someone thinks is best for this game, as some games have a prefered emulator.
        string WorkshopInputDirectory; //The intended InputDirectory (Folder name) for modding this game. This helps make sure end users aren't guessing what the correct one is.

        List<DataItem> Projects;
        public Dictionary<string, Tool>? Tools;

        StartupTools StartupTools = new();


        public GameLibrary()
        {
            InitializeComponent();
            

            string basepath = AppDomain.CurrentDomain.BaseDirectory;

            #if DEBUG //The ExePath (Probably not a good name) is the folder location of the executable. It's used by other parts of the program to find files.
            ExePath = Path.GetFullPath(Path.Combine(basepath, @"..\..\..\..\Release")); //I don't understand (and don't feel like spending the time) setting it up so release is the same as debug mode, so this is a temporary lazy fix.
            #else
            ExePath = basepath; //for published versions of the program to the public.
            #endif

            ScanForWorkshops();

            Projects = new();
            ProjectsSelector.ItemsSource = Projects; // Bind the collection to the ItemsSource property of the DataGrid control
            Projects.Add(new DataItem { ProjectName = "My Project 1", ProjectInputDirectory = "Set", ProjectOutputDirectory = "Not Set" });
            Projects.Add(new DataItem { ProjectName = "My Project 2", ProjectInputDirectory = "Set", ProjectOutputDirectory = "Not Set" });

            //ProjectsSelector.DataContext = Projects;
            CollectionViewSource.GetDefaultView(ProjectsSelector.ItemsSource).Refresh();


            ShowMain();

            Tools = new Dictionary<string, Tool>();
            StartupTools.StartTools(Tools);
            var sharedMenusControl = this.FindName("MenusForToolsAndEvents") as SharedMenus;            
            sharedMenusControl.Tools = this.Tools;

            this.Loaded += AddToMenus;

        }

        public void AddToMenus(object sender, RoutedEventArgs e)
        {
            

            foreach (var tool in Tools.Values) //First lets re-confirm each tool's location. 
            {
                //1 Search the last known location. (Includes where the user Set it as).


                //2 search Tools folder.
                var files = Directory.GetFiles(ExePath + "\\Tools", tool.Application, SearchOption.AllDirectories); //Search the Tools folder for the tool in question.
                                
                if (files.Length > 0)
                {                    
                    tool.Location = files[0]; // If the tool.exe is found, set the location.
                }
                else
                {  
                    tool.Location = ""; // If the tool.exe was not found, location is set to null.
                }
            }

            var sharedMenusControl = this.FindName("MenusForToolsAndEvents") as SharedMenus;  //if (sharedMenusControl == null) { return;}
                        

            foreach (var tool in Tools.Values)
            {
                MenuItem menuItem = new MenuItem { Header = $" {tool.Name}" };
                menuItem.Click += (s, args) => { sharedMenusControl.RunTool(tool); };

                Binding foregroundBinding = new Binding
                {
                    Source = tool,
                    Path = new PropertyPath("Location"),
                    Converter = (IValueConverter)FindResource("LocationToColorConverter"),
                };

                menuItem.SetBinding(MenuItem.ForegroundProperty, foregroundBinding);

                sharedMenusControl.WorkshopMenu.Items.Add(menuItem);
            }
        }

        


        private void ScanForWorkshops()
        {
            LibraryTreeOfWorkshops.Items.Clear();

            if (Directory.Exists(ExePath + "\\Workshops"))
            {
                string[] allWorkshopsPathsArray = Directory.GetDirectories(ExePath + "\\Workshops", "*", SearchOption.TopDirectoryOnly).Select(x => new DirectoryInfo(x).Name).ToArray();
                foreach (var WorkshopPath in allWorkshopsPathsArray)
                {
                    LibraryTreeOfWorkshops.Items.Add(WorkshopPath);
                }
            }
            //Make this line a warning popup that directory does not exist

            (TabControlWorkshopInfo.FindName("TabHome") as TabItem).Visibility = Visibility.Collapsed;
            (TabControlWorkshopInfo.FindName("TabWorkshopMaker") as TabItem).Visibility = Visibility.Collapsed;
            (TabControlWorkshopInfo.FindName("PatchNotes") as TabItem).Visibility = Visibility.Collapsed;
        }

        private void LibraryTreeOfWorkshops_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            
            if (LibraryTreeOfWorkshops.SelectedItem == null)
            {
                return;
            }
            ShowMain();


            WorkshopName = LibraryTreeOfWorkshops.SelectedItem.ToString();

            using (FileStream TargetXML = new FileStream(ExePath + "\\Workshops\\" + WorkshopName + "\\LibraryInfo.xml", FileMode.Open, FileAccess.Read))
            {
                XElement xml = XElement.Load(TargetXML);
                
                Platform = xml.Element("Platform")?.Value;
                Region = xml.Element("Region")?.Value;
                Version = xml.Element("Version")?.Value;
                Emulator = xml.Element("Emulator")?.Value;
                WorkshopInputDirectory = xml.Element("WorkshopInputDirectory")?.Value;               

            };

            //Check if an Art Banner exists for this workshop. Load it if it does. (A banner is just a image to look pretty)
            //NOTE: In WPF an images/picture is offically refered to as a "BitmapImage". 
            //string checkFileExist = ExePath + "\\Workshops\\" + WorkshopName + "\\LibraryBannerArt.png";
            //if (System.IO.File.Exists(checkFileExist))
            //{
            //    ImageLibraryBanner.Source = new BitmapImage(new Uri(string.Format(ExePath + "\\Workshops\\" + WorkshopName + "\\LibraryBannerArt.png")));

            //}
            //else
            //{
            //    ImageLibraryBanner.Source = new BitmapImage(new Uri(string.Format(ExePath + "\\Other\\Images\\LibraryBannerArt.png")));

            //}




            WorkshopInfoDocuments.Content = "Documents: " + Convert.ToString(System.IO.Directory.GetDirectories(ExePath + "\\Workshops\\" + WorkshopName + "\\Documentation", "*", SearchOption.TopDirectoryOnly).Count());
            WorkshopInfoRegion.Content = "Region: " + Region;//+ System.IO.File.ReadAllText(ExePath + "\\Workshops\\" + WorkshopName + "\\Game Region.txt");
            WorkshopInfoGameVersion.Content = "Version: " + Version ;
            WorkshopInfoEmulator.Content = "Emulator: " + Emulator;

            EditorsTree.Items.Clear();
            string directoryPath = ExePath + "\\Workshops\\" + WorkshopName + "\\Editors\\";
            string[] folders = Directory.GetDirectories(directoryPath);

            // Create TreeViewItems for each folder and add them to the EditorsTree
            foreach (string folder in folders)
            {
                string folderName = new DirectoryInfo(folder).Name;
                TreeViewItem folderItem = new TreeViewItem { Header = folderName };
                EditorsTree.Items.Add(folderItem);
            }

            
            TabControlWorkshopInfo.SelectedItem = PatchNotes;
            if (System.IO.File.Exists(ExePath + "\\Workshops\\" + WorkshopName + "\\Documentation\\READ ME\\Text.txt"))
            {

                TextBoxWorkshopReadMe.Text = System.IO.File.ReadAllText(ExePath + "\\Workshops\\" + WorkshopName + "\\Documentation\\READ ME\\Text.txt"); // Reads the READ ME if it exists.

            }
            else { TextBoxWorkshopReadMe.Text = "No Readme Found"; }

            //buttonLaunchWorkshop.Enabled = true;
            //buttonLaunchWorkshop.BackColor = Color.FromArgb(35, 35, 35);

            if (System.IO.File.Exists(ExePath + "\\Settings" + "\\Workshops\\" + WorkshopName + "\\UserInputDirectory.txt"))
            {
                TextBoxInputDirectory.Text = System.IO.File.ReadAllText(ExePath + "\\Settings" + "\\Workshops\\" + WorkshopName + "\\UserInputDirectory.txt"); // Reads the users last set folder directory.

            }
            else 
            {
                TextBoxInputDirectory.Text = "You must select something to launch the workshop.";
            }
            if (System.IO.File.Exists(ExePath + "\\Settings" + "\\Workshops\\" + WorkshopName + "\\UserOutputDirectory.txt"))
            {
                TextBoxOutputDirectory.Text = System.IO.File.ReadAllText(ExePath + "\\Settings" + "\\Workshops\\" + WorkshopName + "\\UserOutputDirectory.txt"); // Reads the users last set folder directory.

            }
            else
            {
                TextBoxOutputDirectory.Text = "If not set, defaults to the Input Directory.";
            }


            Projects.Clear();

            string ProjectsFolder = @ExePath + "\\Projects\\" + WorkshopName + "\\"; //"\\LibraryBannerArt.png";   
            if (Directory.Exists(ProjectsFolder))
            {
                foreach (string TheProjectFolder in Directory.GetDirectories(ProjectsFolder))
                {

                    using (FileStream fs = new FileStream(TheProjectFolder + "\\ProjectInfo.xml", FileMode.Open, FileAccess.Read))
                    {
                        XElement xml = XElement.Load(fs);
                        /*ICommand ButtonCommand = TestDummy();*/
                        string PName = xml.Element("Name")?.Value;
                        //string PDescription = xml.Element("Description")?.Value;
                        string PInput = xml.Element("InputDirectory")?.Value;
                        string POutput = xml.Element("OutputDirectory")?.Value;

                        Projects.Add(new DataItem { /*ButtonCommand = button,*/ ProjectName = PName, /*ProjectDescription = PDescription,*/ ProjectInputDirectory = PInput, ProjectOutputDirectory = POutput });
                    }

                }

                

            }
            else {  }

            CollectionViewSource.GetDefaultView(ProjectsSelector.ItemsSource).Refresh();

        }

        private void ButtonLaunchWorkshop_Click(object sender, RoutedEventArgs e)
        {
            if(ProjectsSelector.SelectedIndex < 0 || LibraryTreeOfWorkshops.SelectedItem == null)
            {
                return;
            }
            DataItem UserProject = Projects[ProjectsSelector.SelectedIndex];

            Workshop TheWorkshop = new Workshop(ExePath, WorkshopName, UserProject.ProjectName, UserProject.ProjectInputDirectory + "\\", Tools); //Thing One, the workshop

            DependencyObject parent = this;
            while (parent != null && !(parent is Window))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            Window currentWindow = parent as Window;
            if (currentWindow != null)
            {
                // Set the position of the new window to the position of the current window
                TheWorkshop.Left = currentWindow.Left;
                TheWorkshop.Top = currentWindow.Top;
            }

            TheWorkshop.Show();

            //LoadEditor.CreateEditor(f2); 
            //If auto mode, run load auto mode
            //this.Close();
        }

        private void LaunchWorkshopPreviewMode(object sender, RoutedEventArgs e)
        {
            string XNull = null;
            bool IsPreviewModeActive = true;

            Workshop TheWorkshop = new Workshop(ExePath, WorkshopName, XNull, XNull, Tools, IsPreviewModeActive); //Thing One, the workshop

            DependencyObject parent = this;
            while (parent != null && !(parent is Window))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            Window currentWindow = parent as Window;
            if (currentWindow != null)
            {
                // Set the position of the new window to the position of the current window
                TheWorkshop.Left = currentWindow.Left;
                TheWorkshop.Top = currentWindow.Top;
            }

            TheWorkshop.Show();

            //LoadEditor.CreateEditor(f2); 
            //If auto mode, run load auto mode
            //this.Close();
        }






















        private void ButtonHome_Click(object sender, RoutedEventArgs e)
        {
            TabControlWorkshopInfo.SelectedItem = TabHome;
        }

        

        private void ButtonCreateWorkshop_Click(object sender, RoutedEventArgs e)
        {
            ClearInfo();
            TabControlWorkshopInfo.SelectedItem = TabWorkshopMaker;
            ButtonCreateNewWorkshop.Content = "Create Workshop";
            TextboxGameVersion.Text = "1.0";
            ComboBoxLoadingMode.Text = "Auto";
            WorkshopTextboxExampleInputFolder.Text = "";
        }

        private void ButtonEditWorkshop_Click(object sender, RoutedEventArgs e)
        {
            ClearInfo();
            TabControlWorkshopInfo.SelectedItem = TabWorkshopMaker;
            ButtonCreateNewWorkshop.Content = "Save Workshop";
            TextBoxGameName.Text = WorkshopName;
            ComboBoxGamePlatform.Text = Platform;  //System.IO.File.ReadAllText(ExePath + "\\Workshops\\" + WorkshopName + "\\Game Platform.txt");
            ComboBoxGameRegion.Text = Region;// System.IO.File.ReadAllText(ExePath + "\\Workshops\\" + WorkshopName + "\\Game Region.txt");
            TextboxGameVersion.Text = Version;
            ComboBoxGameEmulator.Text = Emulator;  //System.IO.File.ReadAllText(ExePath + "\\Workshops\\" + WorkshopName + "\\Best Emulator.txt");
            //ComboBoxLoadingMode.Text = System.IO.File.ReadAllText(ExePath + "\\Workshops\\" + WorkshopName + "\\Loading Mode.txt");
            WorkshopTextboxExampleInputFolder.Text = WorkshopInputDirectory; //System.IO.File.ReadAllText(ExePath + "\\Workshops\\" + WorkshopName + "\\Input Directory.txt");
        }

        private void ClearInfo()
        {
            TextBoxGameName.Text = null;
            ComboBoxGamePlatform.Text = null;
            ComboBoxGameRegion.Text = null;
            ComboBoxGameEmulator.Text = null;
            ComboBoxLoadingMode.Text = null;
            //labelGamePlatform.Text = null;
            //labelGameRegion.Text = null;
            //labelBestEmulator.Text = null;
        }

        private void ButtonCreateNewWorkshop_Click(object sender, RoutedEventArgs e) //This button exists inside the game where a user flls out workshop info when first creating a workshop.
        {
            if (ButtonCreateNewWorkshop.Content.ToString() == "Create Workshop")
            {
                if (TextBoxGameName.Text != null && TextBoxGameName.Text != "" && WorkshopTextboxExampleInputFolder.Text != null && WorkshopTextboxExampleInputFolder.Text !="")
                {

                    Directory.CreateDirectory(ExePath + "\\Workshops\\" + TextBoxGameName.Text);
                    Directory.CreateDirectory(ExePath + "\\Workshops\\" + TextBoxGameName.Text + "\\Documentation");
                    Directory.CreateDirectory(ExePath + "\\Workshops\\" + TextBoxGameName.Text + "\\Editors");
                    Directory.CreateDirectory(ExePath + "\\Workshops\\" + TextBoxGameName.Text + "\\Tools");                    


                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = ("    ");
                    settings.CloseOutput = true;
                    settings.OmitXmlDeclaration = true;                    
                    using (XmlWriter writer = XmlWriter.Create(ExePath + "\\Workshops\\" + TextBoxGameName.Text + "\\WorkshopInfo.xml", settings))
                    {
                        writer.WriteStartElement("Root");
                        writer.WriteEndElement(); //End Root
                        writer.Flush(); //Ends the XML GameFile
                    }
                    System.IO.File.WriteAllText(ExePath + "\\Workshops\\" + TextBoxGameName.Text + "\\Documentation\\LoadOrder.txt", " "); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
                    SaveWorkshop();

                    Directory.CreateDirectory(ExePath + "\\Workshops\\" + TextBoxGameName.Text + "\\Documentation\\READ ME");
                    System.IO.File.WriteAllText(ExePath + "\\Workshops\\" + TextBoxGameName.Text + "\\Documentation\\READ ME\\Text.txt", "" +
                        "When inside the workshop, please fill out the READ ME document with useful information." +
                        "This document is special, and is shown to users in the game library." +
                        "Idealy, the READ ME explains how to extract the game files, what tools are needed, and how to setup a project for that game." +
                        "\n" +
                        "\nIt may also be a good idea to include a Workshop Version Number so users can know if their version of a workshop is outdated." +
                        "Other good ideas include patch / update notes for the workshop, links to discords, forumns, wikis, and any other relevant information about a game." +
                        "\n" +
                        "\nPS: READ ME does not need to include information on how a workshop is used, if you want that, make it a seperate document." +
                        "That way users who are still setting up a project and don't have the workshop open yet won't be overwhelmed with explanatory text.");

                }
                else 
                {
                    MessageBox.Show("Either you did not name the game, \nor you did not give it a Input Directory.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            if (ButtonCreateNewWorkshop.Content.ToString() == "Save Workshop")
            {
                if (TextBoxGameName.Text != null && TextBoxGameName.Text != "" && WorkshopTextboxExampleInputFolder.Text != null && WorkshopTextboxExampleInputFolder.Text != "")
                {
                    string OldWorkshopName = ExePath + "\\Workshops\\" + WorkshopName;
                    string NewWorkshopName = ExePath + "\\Workshops\\" + TextBoxGameName.Text;
                    string OldProjectsName = ExePath + "\\Projects\\" + WorkshopName;
                    string NewProjectsName = ExePath + "\\Projects\\" + TextBoxGameName.Text;
                    try
                    {
                        Directory.Move(OldWorkshopName, NewWorkshopName);
                        Directory.Move(OldProjectsName, NewProjectsName);
                    }
                    catch (IOException exp)
                    {
                        Console.WriteLine(exp.Message);
                    }

                    
                    
                    SaveWorkshop();
                    //ButtonLaunchWorkshop.Enabled = false;
                    //ButtonLaunchWorkshop.BackColor = Color.Gray;
                }
                else 
                {
                    MessageBox.Show("Either you are trying to save the game without a name, \nor you somehow deleted the Input Directory.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
        }
                

        private void SaveWorkshop()
        {
            //System.IO.File.WriteAllText(ExePath + "\\Workshops\\" + TextBoxGameName.Text + "\\Game Name.txt", TextBoxGameName.Text); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
            //System.IO.File.WriteAllText(ExePath + "\\Workshops\\" + TextBoxGameName.Text + "\\Input Directory.txt", WorkshopTextboxExampleInputFolder.Text);   
            //System.IO.File.WriteAllText(ExePath + "\\Workshops\\" + TextBoxGameName.Text + "\\Game Platform.txt", ComboBoxGamePlatform.Text); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
            //System.IO.File.WriteAllText(ExePath + "\\Workshops\\" + TextBoxGameName.Text + "\\Game Region.txt", ComboBoxGameRegion.Text); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
            //System.IO.File.WriteAllText(ExePath + "\\Workshops\\" + TextBoxGameName.Text + "\\Best Emulator.txt", ComboBoxBestEmulator.Text); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
            //System.IO.File.WriteAllText(ExePath + "\\Workshops\\" + TextBoxGameName.Text + "\\Loading Mode.txt", ComboBoxLoadingMode.Text); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
            TabControlWorkshopInfo.SelectedItem = PatchNotes;

            string TheRegion = "";
            string ThePlatform = "";
            string TheEmulator = "";

            if (ComboBoxGameRegion.Text == "USA / NTSC - U") { TheRegion = "USA"; }
            if (ComboBoxGameRegion.Text == "EUR / PAL")      { TheRegion = "EUR"; }
            if (ComboBoxGameRegion.Text == "JAP / NTSC-J")   { TheRegion = "JAP"; }
            
            if (ComboBoxGamePlatform.Text == "NES / Famicom")        { ThePlatform = "NES"; }
            if (ComboBoxGamePlatform.Text == "SNES / Super Famicom") { ThePlatform = "SNES"; }
            if (ComboBoxGamePlatform.Text == "N64")                  { ThePlatform = "N64"; }
            if (ComboBoxGamePlatform.Text == "Gamecube")             { ThePlatform = "Gamecube"; }
            if (ComboBoxGamePlatform.Text == "Wii")                  { ThePlatform = "Wii"; }
            if (ComboBoxGamePlatform.Text == "Wii U")                { ThePlatform = "Wii U"; }
            if (ComboBoxGamePlatform.Text == "GB")                   { ThePlatform = "GB"; }
            if (ComboBoxGamePlatform.Text == "GBC")                  { ThePlatform = "GBC"; }
            if (ComboBoxGamePlatform.Text == "GBA")                  { ThePlatform = "GBA"; }
            if (ComboBoxGamePlatform.Text == "DS")                   { ThePlatform = "DS"; }
            if (ComboBoxGamePlatform.Text == "3DS")                  { ThePlatform = "3DS"; }
            if (ComboBoxGamePlatform.Text == "Switch")               { ThePlatform = "Switch"; }

            if (ComboBoxGamePlatform.Text == "PS1") { ThePlatform = "PS1"; }
            if (ComboBoxGamePlatform.Text == "PS2") { ThePlatform = "PS2"; }
            if (ComboBoxGamePlatform.Text == "PS3") { ThePlatform = "PS3"; }
            if (ComboBoxGamePlatform.Text == "PS4") { ThePlatform = "PS4"; }
            if (ComboBoxGamePlatform.Text == "PS5") { ThePlatform = "PS5"; }
            if (ComboBoxGamePlatform.Text == "PSP") { ThePlatform = "PSP"; }
            if (ComboBoxGamePlatform.Text == "Vita") { ThePlatform = "Vita"; }

            //< ComboBoxItem Content = "Sega Mega Drive / Genesis" />
            //< ComboBoxItem Content = "Sega Mega CD / Sega CD" />
            //< ComboBoxItem Content = "Sega Saturn" />
            //< ComboBoxItem Content = "Sega Dreamcast" />
            //if (ComboBoxGamePlatform.Text == "Sega Mega Drive / Genesis") { ThePlatform = ""; }
            //if (ComboBoxGamePlatform.Text == "Sega Mega CD / Sega CD") { ThePlatform = ""; }
            //if (ComboBoxGamePlatform.Text == "Sega Saturn") { ThePlatform = ""; }
            //if (ComboBoxGamePlatform.Text == "Sega Dreamcast") { ThePlatform = ""; }

            if (ComboBoxGamePlatform.Text == "Other") { ThePlatform = "Other"; }
                        



            if (ComboBoxGameEmulator.Text == "") { TheEmulator = ""; }
            





            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(ExePath + "\\Workshops\\" + TextBoxGameName.Text + "\\LibraryInfo.xml", settings))
            {
                writer.WriteStartElement("LibraryInfo");
                writer.WriteElementString("Platform", ComboBoxGamePlatform.Text);
                writer.WriteElementString("Region", ComboBoxGameRegion.Text);
                writer.WriteElementString("Version", TextboxGameVersion.Text);
                writer.WriteElementString("Emulator", ComboBoxGameEmulator.Text);                
                writer.WriteElementString("WorkshopInputDirectory", WorkshopTextboxExampleInputFolder.Text);
                writer.WriteEndElement(); //End Root (Library Info)
                writer.Flush(); //Ends the XML LibraryInfo file                                

            }






            ScanForWorkshops();
            


        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            TabControlWorkshopInfo.SelectedItem = PatchNotes;
        }
               

        
        private void ColorModeComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (ColorModeComboBox.Text == "Light Mode")
            {
                Properties.Settings.Default.ColorTheme = "Light";
                Properties.Settings.Default.Save();
                
            }
            if (ColorModeComboBox.Text == "Dark Mode")
            {
                Properties.Settings.Default.ColorTheme = "Dark";
                Properties.Settings.Default.Save();

            }            
        }
        

        
                
                

        private void ButtonSelectInputDirectory_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog(); //This starts folder selection using Ookii.Dialogs.WPF NuGet Package
            FolderSelect.Description = "Please select the folder named " + WorkshopInputDirectory; //This sets a description to help remind the user what their looking for.
            FolderSelect.UseDescriptionForTitle = true;    //This enables the description to appear.        
            if ((bool)FolderSelect.ShowDialog(this)) //This triggers the folder selection screen, and if the user does not cancel out...
            {
                TextBoxInputDirectory.Text = FolderSelect.SelectedPath;
                //if (System.IO.File.ReadAllText(ExePath + "\\Workshops\\" + WorkshopName + "\\" +  WorkshopInputDirectory) == Path.GetFileName(FolderSelect.SelectedPath))
                if (WorkshopInputDirectory == Path.GetFileName(FolderSelect.SelectedPath))
                {
                    MessageBox.Show("You have selected the correct folder." +
                        "\nThis is based on the name of the folder you selected vs the intended folder name of the workshop. " +
                        "\nTechnically it could be wrong, but its very unlikely.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);                    
                }
                else 
                { 
                    MessageBox.Show("You did NOT select the correct folder!" +
                        "\n...or atleast the name of the folder you selected is not the same as the intended folder name." +
                        "\n" +
                        "\nIn most cases, this is an error, so you should probably re-select the folder. If you want, you can still choose to save anyway." +
                        "\n" +
                        "\nIf the folder structure is the exact same as the intended one, it will load just fine. " +
                        "If even a single file fails to load properly, a notice will popup when trying to launch and let you know why it failed before closing as a safety measure. " +
                        "So if you want to try using the folder you selected anyway, feel free to give it a try :)", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                
            }
        }

        private void ButtonSelectOutputDirectory_Click(object sender, RoutedEventArgs e)
        {
            #if DEBUG
            VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog(); //This starts folder selection using Ookii.Dialogs.WPF NuGet Package
            FolderSelect.Description = "Please select where files will save to (currently this does nothing)"; //This sets a description to help remind the user what their looking for.
            FolderSelect.UseDescriptionForTitle = true;    //This enables the description to appear.        
            if ((bool)FolderSelect.ShowDialog(this)) //This triggers the folder selection screen, and if the user does not cancel out...
            {
                TextBoxOutputDirectory.Text = FolderSelect.SelectedPath;
            }
            #else
            
            #endif
            
        }

        private void ButtonSetWorkshopInputFolder_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog(); //This starts folder selection using Ookii.Dialogs.WPF NuGet Package
            FolderSelect.Description = "Please select where files load from (For dev, this is CrystalEditor/CE/Hexfiles)"; //This sets a description to help remind the user what their looking for.
            FolderSelect.UseDescriptionForTitle = true;    //This enables the description to appear.        
            if ((bool)FolderSelect.ShowDialog(this)) //This triggers the folder selection screen, and if the user does not cancel out...
            {
                WorkshopTextboxExampleInputFolder.Text = Path.GetFileName(FolderSelect.SelectedPath);
            }
        }

        private void HUDButtonDiscord_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://discord.gg/pjJM8Tje";
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }

        private void ButtonTutorial_Click(object sender, RoutedEventArgs e)
        {
            Tutorial f2 = new Tutorial();
            f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            f2.Show();
        }

        private void ButtonBetaTest_Click(object sender, RoutedEventArgs e)
        {    
            MainWindow f2 = new MainWindow();
            f2.Show();

            
        }










        private void ShowMain()
        {
            TopMain.Visibility = Visibility.Visible;
            TopProjects.Visibility = Visibility.Collapsed;
        }

        private void ShowEdit()
        {
            TopMain.Visibility = Visibility.Collapsed;
            TopProjects.Visibility = Visibility.Visible;
        }


        private void ButtonCreateNewProject(object sender, RoutedEventArgs e)
        {
            if (LibraryTreeOfWorkshops.SelectedItem == null)
            {
                return;
            }

            ProjectNameTextbox.Text = "";
            ProjectDescriptionTextBox.Text = "";
            TextBoxInputDirectory.Text = "";
            TextBoxOutputDirectory.Text = "";
            ShowEdit();
            ProjectInfoModeNew = true;
        }


        private void ModifyProject(object sender, RoutedEventArgs e)
        {

            if (ProjectsSelector.SelectedIndex < 0 || LibraryTreeOfWorkshops.SelectedItem == null)
            {
                return;
            }
            ShowEdit();

            DataItem UserProject = Projects[ProjectsSelector.SelectedIndex];

            ProjectNameTextbox.Text = UserProject.ProjectName;
            //ProjectDescriptionTextBox.Text = UserProject.ProjectDescription;
            TextBoxInputDirectory.Text = UserProject.ProjectInputDirectory;
            TextBoxOutputDirectory.Text = UserProject.ProjectOutputDirectory;

            ProjectInfoModeNew = false;

        }

        private void DeleteProject(object sender, RoutedEventArgs e)
        {

            DataItem UserProject = Projects[ProjectsSelector.SelectedIndex];

            Directory.Delete(ExePath + "\\Projects" + "\\" + WorkshopName + "\\" + UserProject.ProjectName, true);
            //Directory.Delete(ExePath + "\\Projects" + WorkshopName + "\\" + UserProject.ProjectName, true);
        }



            bool ProjectInfoModeNew = true;

        private void SaveProjectInfo(object sender, RoutedEventArgs e)
        {


            if (ProjectInfoModeNew == false) //If, then this is modify mode
            {
                DataItem UserProject = Projects[ProjectsSelector.SelectedIndex];
                string oldFolderPath = ExePath + "\\Projects\\" + WorkshopName + "\\" + UserProject.ProjectName;
                string newFolderPath = ExePath + "\\Projects\\" + WorkshopName + "\\" + ProjectNameTextbox.Text;

                if (oldFolderPath != newFolderPath)
                {
                    Directory.Move(oldFolderPath, newFolderPath);// Rename the folder at the old path to the new path
                }



            }

            string TheProjectFolder = @ExePath + "\\Projects\\" + WorkshopName + "\\" + ProjectNameTextbox.Text + "\\"; //"\\LibraryBannerArt.png";   
            if (Directory.Exists(TheProjectFolder))
            {
               
            }
            else
            {
                Directory.CreateDirectory(TheProjectFolder);
                Directory.CreateDirectory(TheProjectFolder + "\\" + "Documentation" + "\\");
                System.IO.File.WriteAllText(TheProjectFolder + "\\" + "\\Documentation\\LoadOrder.txt", " "); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
            }



            string filePath = TheProjectFolder + "ProjectInfo.xml"; // Replace with your file name
            if (File.Exists(filePath))
            {
                // The file already exists, so you may want to update its contents
            }
            else
            {
                // The file does not exist, so you may want to create it before saving the project info
            }


            

            try 
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = ("    ");
                settings.CloseOutput = true;
                settings.OmitXmlDeclaration = true;
                using (XmlWriter writer = XmlWriter.Create(ExePath + "\\Projects\\" + WorkshopName + "\\" + ProjectNameTextbox.Text + "\\" + "ProjectInfo.xml", settings))
                {
                    writer.WriteStartElement("Project"); //This is the root of the XML                    
                    writer.WriteElementString("Name", ProjectNameTextbox.Text);
                    writer.WriteElementString("Description", ProjectDescriptionTextBox.Text);
                    writer.WriteElementString("InputDirectory", TextBoxInputDirectory.Text);
                    writer.WriteElementString("OutputDirectory", TextBoxOutputDirectory.Text);
                    writer.WriteEndElement(); //End Project  AKA the Root of the XML   
                    writer.Flush(); //Ends the XML
                }
            }
            catch 
            {
                
            }

            ShowMain();



            TreeViewItem selectedItem = LibraryTreeOfWorkshops.ItemContainerGenerator.ContainerFromItem(LibraryTreeOfWorkshops.SelectedItem) as TreeViewItem;
            if (selectedItem != null) //This stuff makes it so the data grid updates.
            {
                selectedItem.IsSelected = false;
                selectedItem.IsSelected = true;
            }

            


        }


        

        private void CancelProject(object sender, RoutedEventArgs e)
        {
            ShowMain();
        }

        private void ProjectsSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        
                
    }

    public class LocationToColorConverter : IValueConverter //Ignore this, used for binding the color of a tool in the general or workshop tools menus.
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var location = value as string;
            if (string.IsNullOrEmpty(location))
            {
                return new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

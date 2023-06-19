using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace Crystal_Editor
{

    //This file handles a sort of user-friendly coding to add custom things to a workshop, to handle edge cases when working with files and diffrent projects.
    //The goal is to be like RPG maker's eventing system. This way, there is no actual coding, you just click buttons and stuff gets setup. It is currently very lacking.
    // In the future, i plan on overhauling / remaking this, for the new menu system. However, it still works right now.


    public class StepInfo 
    {
        public DockPanel StepDockPanel { get; set; }
        public string StepName { get; set; }
        public ComboBox StepComboBox { get; set; }
        public DockPanel StepPanel { get; set; }
        public string StepTextOne { get; set; }
        public string StepTextTwo { get; set; }        
        public string StepTextThree { get; set; }

    }

    public partial class Workshop
    {

        private void OpenEventMenu(object sender, RoutedEventArgs e)
        {
            HIDEALL();
            EventScreen.Visibility = Visibility.Visible;
        }

        private void MenuSaveEvents(object sender, RoutedEventArgs e)
        {
            SaveEvents();
        }

        public void LoadEvents() 
        {
            //if directory exists
        }

        public void SaveEvents() 
        {
            //if (EventTree.Items.Count == 0) { return; } //This actually prevents deleting events though right? :/

            //Directory.CreateDirectory(ExePath + "\\Projects\\" + WorkshopName + "\\" + ProjectName + "\\Events\\");   //I think i dont need a folder, just a file?

            List<TreeViewItem> WorkshopEvents = new();
            List<TreeViewItem> ProjectEvents = new();

            foreach (TreeViewItem item in EventTree.Items) 
            {
                if (item.Foreground == Brushes.Orange)
                { WorkshopEvents.Add(item); }
                else if (item.Foreground == Brushes.White)
                { ProjectEvents.Add(item); }
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(ExePath + "\\Workshops\\" + WorkshopName + "\\CommonEvents.xml", settings))
            {
                writer.WriteStartElement("Events"); //This is the root of the XML     
                foreach (TreeViewItem TreeViewItem in WorkshopEvents)
                {                    
                    List<StepInfo> StepsList = TreeViewItem.Tag as List<StepInfo>;
                    writer.WriteStartElement("Event");
                    writer.WriteElementString("EventName", TreeViewItem.Header.ToString());
                    writer.WriteStartElement("StepList");
                    foreach (StepInfo StepInfo in StepsList) 
                    {
                        writer.WriteStartElement("Step");
                        writer.WriteElementString("StepName", StepInfo.StepName); //This is all misc editor data.
                        writer.WriteElementString("StepTextOne", StepInfo.StepTextOne);
                        writer.WriteElementString("StepTextTwo", StepInfo.StepTextTwo);
                        writer.WriteElementString("StepTextThree", StepInfo.StepTextThree);      
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    //writer.WriteElementString("EventName", ExtraTable.ExtraTableName);
                    writer.WriteEndElement(); // End Event
                }
                writer.WriteEndElement(); //End Events  AKA the Root of the XML   
                writer.Flush(); //Ends the XML Events
            }

            using (XmlWriter writer = XmlWriter.Create(ExePath + "\\Projects\\" + WorkshopName + "\\" + ProjectName + "\\CommonEvents.xml", settings))
            {
                writer.WriteStartElement("Events"); //This is the root of the XML     
                foreach (TreeViewItem TreeViewItem in ProjectEvents)
                {
                    List<StepInfo> StepsList = TreeViewItem.Tag as List<StepInfo>;
                    writer.WriteStartElement("Event");
                    writer.WriteElementString("EventName", TreeViewItem.Header.ToString());
                    writer.WriteStartElement("StepList");
                    foreach (StepInfo StepInfo in StepsList)
                    {
                        writer.WriteStartElement("Step");
                        writer.WriteElementString("StepName", StepInfo.StepName); //This is all misc editor data.
                        writer.WriteElementString("StepTextOne", StepInfo.StepTextOne);
                        writer.WriteElementString("StepTextTwo", StepInfo.StepTextTwo);
                        writer.WriteElementString("StepTextThree", StepInfo.StepTextThree);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    //writer.WriteElementString("EventName", ExtraTable.ExtraTableName);
                    writer.WriteEndElement(); // End Event
                }
                writer.WriteEndElement(); //End Events  AKA the Root of the XML   
                writer.Flush(); //Ends the XML Events
            }

        }

        public void LoadCommonEvents() 
        {
            string WorkshopCommonEventFile = @ExePath + "\\Workshops\\" + WorkshopName + "\\CommonEvents.xml"; //"\\LibraryBannerArt.png";   
            if (File.Exists(WorkshopCommonEventFile)) 
            {
                MenuCommonEvents.Items.Add(new Separator());
                using (FileStream fs = new FileStream(ExePath + "\\Workshops\\" + WorkshopName + "\\CommonEvents.xml", FileMode.Open, FileAccess.Read))
                {
                    XElement xml = XElement.Load(fs);

                    foreach (XElement Event in xml.Descendants("Event"))
                    {
                        TreeViewItem TreeViewItem = new();
                        TreeViewItem.Header = Event.Element("EventName")?.Value;
                        TreeViewItem.Foreground = Brushes.Orange;
                        List<StepInfo> StepsList = new();
                        foreach (XElement Step in Event.Descendants("Step"))
                        {
                            StepInfo StepInfo = new();
                            StepInfo.StepName = Step.Element("StepName")?.Value;
                            StepInfo.StepTextOne = Step.Element("StepTextOne")?.Value;
                            StepInfo.StepTextTwo = Step.Element("StepTextTwo")?.Value;
                            StepInfo.StepTextThree = Step.Element("StepTextThree")?.Value;
                            StepsList.Add(StepInfo);
                        }
                        TreeViewItem.Tag = StepsList;
                        EventTree.Items.Add(TreeViewItem);

                        MenuItem menuItem = new MenuItem { Header = TreeViewItem.Header };
                        menuItem.Click += (sender, e) => RunCommonEvent(StepsList);
                        MenuCommonEvents.Items.Add(menuItem);
                    }

                };
            }

            


            string ProjectCommonEventFile = @ExePath + "\\Projects\\" + WorkshopName + "\\" + ProjectName + "\\CommonEvents.xml"; //"\\LibraryBannerArt.png";   
            if (!File.Exists(ProjectCommonEventFile)){return;}

            MenuCommonEvents.Items.Add(new Separator());
            using (FileStream fs = new FileStream(ExePath + "\\Projects\\" + WorkshopName + "\\" + ProjectName + "\\CommonEvents.xml", FileMode.Open, FileAccess.Read))
            {
                XElement xml = XElement.Load(fs);

                foreach (XElement Event in xml.Descendants("Event")) 
                {
                    TreeViewItem TreeViewItem = new();
                    TreeViewItem.Header = Event.Element("EventName")?.Value;
                    TreeViewItem.Foreground = Brushes.White;
                    List<StepInfo> StepsList = new();
                    foreach (XElement Step in Event.Descendants("Step")) 
                    {
                        StepInfo StepInfo = new();
                        StepInfo.StepName = Step.Element("StepName")?.Value;   
                        StepInfo.StepTextOne = Step.Element("StepTextOne")?.Value;
                        StepInfo.StepTextTwo = Step.Element("StepTextTwo")?.Value;
                        StepInfo.StepTextThree = Step.Element("StepTextThree")?.Value;
                        StepsList.Add(StepInfo);
                    }
                    TreeViewItem.Tag = StepsList;
                    EventTree.Items.Add(TreeViewItem);

                    MenuItem menuItem = new MenuItem { Header = TreeViewItem.Header };
                    menuItem.Click += (sender, e) => RunCommonEvent(StepsList);
                    MenuCommonEvents.Items.Add(menuItem);
                }

            };

            
        }

        

        private void ButtonNewEvent(object sender, RoutedEventArgs e)
        {
            TreeViewItem NewEvent = new();
            NewEvent.Header = "New Event";            

            List<StepInfo> StepsList = new();
            NewEvent.Tag = StepsList;

            EventTree.Items.Add(NewEvent);
        }

        private void ButtonRenameEvent(object sender, RoutedEventArgs e)
        {        
            TreeViewItem TheEvent = EventTree.SelectedItem as TreeViewItem;
            TheEvent.Header = TextboxEventName.Text;
        }
        

        private void ButtonDeleteEvent(object sender, RoutedEventArgs e)
        {
            EventTree.Items.Remove(EventTree.SelectedItem);
        }

        private void ComboBoxCommonEventType_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                if (ComboBoxCommonEventType.Text == "Workshop Event")
                {
                    TreeViewItem TheEvent = EventTree.SelectedItem as TreeViewItem;
                    TheEvent.Foreground = Brushes.Orange;
                }
                if (ComboBoxCommonEventType.Text == "Project Event")
                {
                    TreeViewItem TheEvent = EventTree.SelectedItem as TreeViewItem;
                    TheEvent.Foreground = Brushes.White;
                }
            }
            catch 
            {
            
            }
            

        }

        private void ButtonNewStep(object sender, RoutedEventArgs e)
        {
            if (EventTree.SelectedItem == null) 
            {
                return;
            }
            

            TreeViewItem TheEvent = EventTree.SelectedItem as TreeViewItem;
            List<StepInfo> StepsList = TheEvent.Tag as List<StepInfo>;

            StepInfo StepInfo = new();
            var (StepDockPanel, ComboBox) = CreateStep(StepInfo);            
            StepInfo.StepDockPanel = StepDockPanel;
            StepInfo.StepComboBox = ComboBox;
            StepsList.Add(StepInfo);

            
            
        }

        private (DockPanel, ComboBox) CreateStep(StepInfo StepInfo) 
        {
            DockPanel StepDockPanel = new();
            StepDockPanel.Background = System.Windows.Media.Brushes.DarkGreen;
            StepDockPanel.Margin = new Thickness(0, 0, 0, 7); // Left Top Right Bottom 

            Label StepLabel = new();
            StepLabel.Content = "Step X";

            DockPanel StepBack2 = new();
            StepBack2.Background = System.Windows.Media.Brushes.DarkRed;

            Button DeleteStep = new();
            DeleteStep.Content = "Delete This Step";
            DeleteStep.Click += (s, ev) => DeleteThisStep(StepDockPanel);

            DockPanel StepPanel = new();
            StepPanel.Background = System.Windows.Media.Brushes.DarkSlateBlue;
            StepPanel.MinHeight = 40;
            StepInfo.StepPanel = StepPanel;

            ComboBox ComboBox = new();
            ComboBox.Name = "TheStepName";
            ComboBox.Items.Add("Save All");
            //ComboBox.Items.Add("Unpack NDS Rom");
            ComboBox.Items.Add("Pack NDS Rom");
            //ComboBox.Items.Add("Copy File");
            //ComboBox.Items.Add("Rename File");
            //ComboBox.Items.Add("Move File");
            ComboBox.Items.Add("Delete File");
            ComboBox.Items.Add("Launch Program");
            ComboBox.Items.Add("Launch File in Program");
            //ComboBox.Items.Add("Run Game On Emulator");
            ComboBox.Items.Add("Run Text in Command Prompt");

            DockPanel.SetDock(StepDockPanel, Dock.Top);
            DockPanel.SetDock(StepLabel, Dock.Top);

            DockPanel.SetDock(StepBack2, Dock.Bottom);
            DockPanel.SetDock(DeleteStep, Dock.Right);
            DeleteStep.HorizontalAlignment = HorizontalAlignment.Right;
            DockPanel.SetDock(ComboBox, Dock.Left);
            DockPanel.SetDock(StepPanel, Dock.Bottom);
            DockPanelEventSteps.Children.Add(StepDockPanel);

            StepDockPanel.Children.Add(StepLabel);
            StepBack2.Children.Add(DeleteStep);
            StepBack2.Children.Add(ComboBox);
            StepDockPanel.Children.Add(StepPanel);
            StepDockPanel.Children.Add(StepBack2);

            ComboBox.Text = StepInfo.StepName;
            ComboBox_DropDownClosed(ComboBox, StepInfo);
            ComboBox.DropDownClosed += (s, ev) => ComboBox_DropDownClosed(ComboBox, StepInfo);
            return (StepDockPanel, ComboBox);
        }

        private void DeleteThisStep(DockPanel DockPanel)
        {
            DockPanelEventSteps.Children.Remove(DockPanel);
        }

        


        private void EventTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem TheEvent = EventTree.SelectedItem as TreeViewItem;

            try
            {
                List<StepInfo> StepsList = TheEvent.Tag as List<StepInfo>;

                TextboxEventName.Text = TheEvent.Header.ToString();


                DockPanelEventSteps.Children.Clear();

                foreach (StepInfo StepInfo in StepsList)
                {

                    CreateStep(StepInfo);
                }
            }
            catch 
            {
            
            }

            


        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////Step Window Creation & Interaction///////////////////////////////
        ///////////////////////////(also what happens on tree selection)/////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////

        public void ComboBox_DropDownClosed(ComboBox comboBox, StepInfo StepInfo)
        {
            StepInfo.StepPanel.Children.Clear();

            if (comboBox.SelectedItem != null && comboBox.SelectedItem.ToString() == "Save All")                {CreateStepSaveAll(StepInfo);}
            if (comboBox.SelectedItem != null && comboBox.SelectedItem.ToString() == "Pack NDS Rom")            {CreateStepPackNDS(StepInfo);}
            if (comboBox.SelectedItem != null && comboBox.SelectedItem.ToString() == "Delete File")             {StepDeleteFile(StepInfo);}
            if (comboBox.SelectedItem != null && comboBox.SelectedItem.ToString() == "Launch Program")          {StepLaunchProgram(StepInfo);}
            if (comboBox.SelectedItem != null && comboBox.SelectedItem.ToString() == "Launch File in Program")  {StepLaunchFileInProgram(StepInfo); }
            if (comboBox.SelectedItem != null && comboBox.SelectedItem.ToString() == "Run Text in Command Prompt") { StepRunTextinCommandPrompt(StepInfo); }
            

            StepInfo.StepName = comboBox.Text;

        }


        

        public void CreateStepSaveAll(StepInfo StepInfo) 
        {
            Label Label = new();
            Label.Content = "This step causes everything (Editors, Game Data, Documents, Etc) to save when this event is run." +
                "\nUsually best to place as step 1 before any other steps happen.";
            StepInfo.StepPanel.Children.Add(Label);
        }
        public void CreateStepPackNDS(StepInfo StepInfo) 
        {
            Label Label = new();
            Label.Content = "This step will automatically pack a NDS rom, combine it with a step to launch rom with an emulator :)" +
                "\nNote: To use this, the NDS rom must be unpacked with this program, not DS buff / Tinke / NDS Factory / etc." +
                "\nThis is because of the GAME.xml file created during unpacking, thats needed to repack the rom." +
                "\nPS: Rom name must include the file extension. Such as .nds or .srl";
            DockPanel.SetDock(Label, Dock.Top);
            StepInfo.StepPanel.Children.Add(Label);



            DockPanel DockGame = new(); //Button to select pack target
            DockPanel.SetDock(DockGame, Dock.Top);
            StepInfo.StepPanel.Children.Add(DockGame);

            Button ButtonGame = new();
            DockPanel.SetDock(ButtonGame, Dock.Left);
            DockGame.Children.Add(ButtonGame);
            ButtonGame.Content = "Select GAME.xml";

            TextBox GameTextBox = new();
            DockPanel.SetDock(GameTextBox, Dock.Right);
            DockGame.Children.Add(GameTextBox);
            GameTextBox.IsEnabled = false;
            GameTextBox.Text = StepInfo.StepTextOne;

            ButtonGame.Click += (sender, e) =>
            {
                VistaOpenFileDialog UserSelection = new VistaOpenFileDialog();
                UserSelection.Title = "Please select the GAME.xml file in the NDS rom that will be repacked";
                if ((bool)UserSelection.ShowDialog(this))
                {
                    GameTextBox.Text = UserSelection.FileName;
                    StepInfo.StepTextOne = GameTextBox.Text;
                }
            };
            ///////////////////////////////////////////////////////
            DockPanel DockFolder = new(); //pack to where
            DockPanel.SetDock(DockFolder, Dock.Top);
            StepInfo.StepPanel.Children.Add(DockFolder);

            Button ButtonFolder = new();
            DockPanel.SetDock(ButtonFolder, Dock.Left);
            DockFolder.Children.Add(ButtonFolder);
            ButtonFolder.Content = "Select where the rom goes";

            TextBox FolderTextBox = new();
            DockPanel.SetDock(FolderTextBox, Dock.Right);
            DockFolder.Children.Add(FolderTextBox);
            FolderTextBox.IsEnabled = false;
            FolderTextBox.Text = StepInfo.StepTextTwo;

            ButtonFolder.Click += (sender, e) =>
            {
                VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog();
                FolderSelect.Description = "Please select the folder the rom will go to when repacked.";
                FolderSelect.UseDescriptionForTitle = true;
                if ((bool)FolderSelect.ShowDialog(this))
                {
                    FolderTextBox.Text = FolderSelect.SelectedPath;
                    StepInfo.StepTextTwo = FolderTextBox.Text;
                }
            };
            //////////////////////////////////////////////////////
            DockPanel DockName = new(); //with what name
            DockPanel.SetDock(DockName, Dock.Top);
            StepInfo.StepPanel.Children.Add(DockName);

            Button TheName = new();
            DockPanel.SetDock(TheName, Dock.Left);
            DockName.Children.Add(TheName);
            TheName.Content = "Set the rom name";

            TextBox TextBoxName = new();
            DockPanel.SetDock(TextBoxName, Dock.Right);
            DockName.Children.Add(TextBoxName);
            TextBoxName.Text = StepInfo.StepTextThree;

            TheName.Click += (sender, e) =>
            {                
                StepInfo.StepTextThree = TextBoxName.Text;                
            };
        }
        public void StepDeleteFile(StepInfo StepInfo) 
        {
            Button Button = new();
            Button.Content = "Select File to Delete";

            TextBox Textbox = new();
            Textbox.IsEnabled = false;
            Textbox.Text = StepInfo.StepTextOne;

            Button.Click += (sender, e) =>
            {
                VistaOpenFileDialog UserSelection = new VistaOpenFileDialog();
                UserSelection.Title = "Please select the program that will be Deleted";
                if ((bool)UserSelection.ShowDialog(this))
                {
                    Textbox.Text = UserSelection.FileName;
                    StepInfo.StepTextOne = Textbox.Text;
                }
            };

            DockPanel.SetDock(Button, Dock.Left);
            StepInfo.StepPanel.Children.Add(Button);

            DockPanel.SetDock(Textbox, Dock.Right);
            StepInfo.StepPanel.Children.Add(Textbox);
        }
        public void StepLaunchProgram(StepInfo StepInfo)
        {
            Button Button = new();
            Button.Content = "Select Program to launch";

            TextBox Textbox = new();
            Textbox.IsEnabled = false;
            Textbox.Text = StepInfo.StepTextOne;

            //openFileDialog.Title = "Select XML file"; // Set the dialog title
            //VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog(); //This starts folder selection using Ookii.Dialogs.WPF NuGet Package

            Button.Click += (sender, e) =>
            {
                VistaOpenFileDialog UserSelection = new VistaOpenFileDialog();
                UserSelection.Title = "Please select the program that will launch";
                if ((bool)UserSelection.ShowDialog(this))
                {
                    Textbox.Text = UserSelection.FileName;
                    StepInfo.StepTextOne = Textbox.Text;
                }
            };

            DockPanel.SetDock(Button, Dock.Left);
            StepInfo.StepPanel.Children.Add(Button);

            DockPanel.SetDock(Textbox, Dock.Right);
            StepInfo.StepPanel.Children.Add(Textbox);

        }

        public void StepLaunchFileInProgram(StepInfo StepInfo)
        {

            DockPanel DockPanelFile = new();

            Button ButtonFile = new();
            ButtonFile.Content = "Select File";

            TextBox TextboxFile = new();
            TextboxFile.IsEnabled = false;
            TextboxFile.Text = StepInfo.StepTextOne;

            ButtonFile.Click += (sender, e) =>
            {
                VistaOpenFileDialog UserSelection = new VistaOpenFileDialog();
                UserSelection.Title = "Please select the program the file will launch with.";
                if ((bool)UserSelection.ShowDialog(this))
                {
                    TextboxFile.Text = UserSelection.FileName;
                    StepInfo.StepTextOne = TextboxFile.Text;
                }
            };

            DockPanel.SetDock(DockPanelFile, Dock.Top);
            StepInfo.StepPanel.Children.Add(DockPanelFile);

            DockPanel.SetDock(ButtonFile, Dock.Left);
            DockPanelFile.Children.Add(ButtonFile);

            DockPanel.SetDock(TextboxFile, Dock.Right);
            DockPanelFile.Children.Add(TextboxFile);





            //////////////////////////////////////////////////////
            DockPanel DockPanelProgram = new();

            Button ButtonProgram = new();
            ButtonProgram.Content = "Select Program";

            TextBox TextboxProgram = new();
            TextboxProgram.IsEnabled = false;
            TextboxProgram.Text = StepInfo.StepTextTwo;

            ButtonProgram.Click += (sender, e) =>
            {
                VistaOpenFileDialog UserSelection = new VistaOpenFileDialog();
                UserSelection.Title = "Please select the program the file will launch with.";
                if ((bool)UserSelection.ShowDialog(this))
                {
                    TextboxProgram.Text = UserSelection.FileName;
                    StepInfo.StepTextTwo = TextboxProgram.Text;
                }
            };

            DockPanel.SetDock(DockPanelProgram, Dock.Top);
            StepInfo.StepPanel.Children.Add(DockPanelProgram);

            DockPanel.SetDock(ButtonProgram, Dock.Left);
            DockPanelProgram.Children.Add(ButtonProgram);

            DockPanel.SetDock(TextboxProgram, Dock.Right);
            DockPanelProgram.Children.Add(TextboxProgram);
        }

        public void StepRunTextinCommandPrompt(StepInfo StepInfo)
        {
            //DockPanel DockPanel = new();


            Button Button = new();
            Button.Content = "Set Text to run";
            DockPanel.SetDock(Button, Dock.Left);
            StepInfo.StepPanel.Children.Add(Button);

            TextBox TextBoxOne = new();
            TextBoxOne.Text = StepInfo.StepTextOne;

            DockPanel.SetDock(TextBoxOne, Dock.Right);
            StepInfo.StepPanel.Children.Add(TextBoxOne);

            Button.Click += (sender, e) =>
            {
                StepInfo.StepTextOne = TextBoxOne.Text;
            };
        }


        //ComboBox.Items.Add("Run Text in Command Prompt");
        public void StepX(StepInfo StepInfo)
        {

        }


        /////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////Executing Steps/////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////
        

        public void RunCommonEvent(List<StepInfo> StepsList)
        {
            foreach (StepInfo StepInfo in StepsList) 
            {
                if (StepInfo.StepName == "Save All") 
                {
                    SaveWorkshop.OnlySaveEditors(Database, this);
                    SaveWorkshop.SaveGameFiles(Database, this);
                    CSDocuments.SaveDocumentation(this);
                }


                


                if (StepInfo.StepName == "Delete File")
                {

                    if (File.Exists(StepInfo.StepTextOne))
                    {
                        File.Delete(StepInfo.StepTextOne);
                    }
                    else
                    {
                        string Error = "Error: Step Delete file's target file does not exist." +
                            "\n" +
                            "\nThis step will be skipped, the rest of automation will proceed as usual. " +
                            "\n";
                        Notification f2 = new(Error);
                        f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                        f2.ShowDialog();
                    };

                }


                if (StepInfo.StepName == "Launch Program")
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo(StepInfo.StepTextOne);
                        startInfo.UseShellExecute = true;
                        Process.Start(startInfo);
                    }
                    catch
                    {
                        DebugBox4.Text = "The program you tried to launch is not supported by your operating system";
                    }
                }



                if (StepInfo.StepName == "Launch File in Program")
                {
                    if (!File.Exists(StepInfo.StepTextOne) || !File.Exists(StepInfo.StepTextTwo)) 
                    {
                        return;
                    }

                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.Arguments = "\"" + StepInfo.StepTextOne + "\"";
                        startInfo.FileName = StepInfo.StepTextTwo;
                        Process.Start(startInfo);
                    }
                    catch
                    {
                        DebugBox4.Text = "The Common event to run a file in a program failed." +
                            "\nIt's possible the program you tried to launch is not supported by your operating system" +
                            "\nHowever, it could easily be a coding error on my part. Plz report this to discord.";
                    }

                    //string RomPath = lines[1];
                    //string RomName = lines[2];
                    //string TheNDSRom = RomPath + "\\" + RomName;
                    //ProcessStartInfo startInfo = new ProcessStartInfo();
                    //startInfo.FileName = ExePath + "\\Emulators\\MelonDS\\melonDS.exe";
                    //startInfo.Arguments = "\"" + TheNDSRom + "\"";
                    //Process.Start(startInfo);

                }



                if (StepInfo.StepName == "Pack NDS Rom")
                {
                    if ( (StepInfo.StepTextOne == "" || StepInfo.StepTextOne == null ) && (StepInfo.StepTextTwo == "" || StepInfo.StepTextTwo == null) && (StepInfo.StepTextThree == "" || StepInfo.StepTextThree == null) )
                    { return; }

                    if (!File.Exists(StepInfo.StepTextOne))
                    {
                        string Error = "Error: NDS Repack Automation Step 2 GAME.xml file does not exist. " +
                            "\n" +
                            "\nThis step will be skipped, the rest of automation will proceed as usual. " +
                            "\n";
                        Notification f2 = new(Error);
                        f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                        f2.ShowDialog();
                        return;
                    }

                    if (!Directory.Exists(StepInfo.StepTextTwo))
                    {
                        string Error = "Error: NDS Repack Automation Step 2 Output Folder does not exist. " +
                        "\n" +
                        "\nThis step will be skipped, the rest of automation will proceed as usual. " +
                        "\n";
                        Notification f2 = new(Error);
                        f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                        f2.ShowDialog();
                        return;
                    }

                    
                    string packCommand = $"\"{ExePath}\\Tools\\Console\\Nintendo DS\\NitroPacker\\NitroPacker.exe\" pack -p \"{StepInfo.StepTextOne}\" -r \"{StepInfo.StepTextTwo}\\{StepInfo.StepTextThree}\"";
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "cmd.exe";
                    psi.Arguments = $"/C \"{packCommand}\"";
                    psi.WorkingDirectory = $"{ExePath}\\Tools\\Console\\Nintendo DS\\NitroPacker";
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;

                    Process p = new Process();
                    p.StartInfo = psi;
                    p.Start();

                    p.WaitForExit();
                    
                }


                if (StepInfo.StepName == "Run Text in Command Prompt")
                {
                    if (  (StepInfo.StepTextOne == "" || StepInfo.StepTextOne == null)  )
                    { return; }

                    

                    string CommandText = $"{StepInfo.StepTextOne}";
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "cmd.exe";
                    psi.Arguments = $"/K \"{CommandText}\"";
                    //psi.WorkingDirectory = $"{ExePath}\\Tools\\Console\\Nintendo DS\\NitroPacker";
                    //psi.CreateNoWindow = true;
                    psi.UseShellExecute = true;
                    psi.RedirectStandardOutput = false;
                    psi.RedirectStandardError = false;

                    Process p = new Process();
                    p.StartInfo = psi;
                    p.Start();

                    //p.WaitForExit();  //this causes the code to wait for CMD to finish, :0

                }
















            }
            
        }


















    }
}

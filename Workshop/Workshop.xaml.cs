using Ookii.Dialogs.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xaml;
//using System.Linq;
//using System.Windows.Shapes;
//using System.Windows.Shapes;
//using System.Windows.Shapes;
//using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Crystal_Editor
{
    //This file is the most complex and barely sorted part of the program. You will hate it :(
    // it's a partial class, and some other parts of the class are ...all over. 
    // i'm not great with moving stuff between classes yet, so yeah, its just a massive partial class.
    // feel free to ask questions about this, and entrymanager. 
    // 
    // I could use help sorting this, like, a lot. 
    //
    //i tried to previously sort it using giant comment walls between major sections, but i kinda stopped moving stuff to proper sections because i was lazy.
    //
    // the bottom is whatever the newest random shit is. 

    public partial class Workshop : Window
    {
        //This is a pertial class.
        //Every file inside the Workshop folder SHOULD be a part of this partial class.

        public string ExePath; //The location of the exe of crystal editor.
        public string WorkshopName; //The name of the workshop (IE name of whats selected in Game Library)

        DocumentsWorkshop CSDocuments = new(); //Used for the documentation feature.
        SaveWorkshop SaveWorkshop = new(); //I haven't decided if i want all saving here yet, or in multiple methods.
        Database Database = new(); //The database of....everything to do with an editor.        
        EditorCreate EditorCreate = new();
        EntryManager EntryManager = new();

        public string EditorName = "Blank";

        //Project Info
        public bool IsPreviewMode; //VS preview mode. In preview mode, a project folder and input directory are not used, to allow users to preview a workshop. 
        public string ProjectName;
        public string ProjectDescription;
        public string InputDirectory;
        public string OutputDirectory;

        public Editor EditorClass;
        public Page PageClass;
        public Row RowClass;
        public Column ColumnClass;
        public Entry EntryClass;

        public List<Entry> EntryMoveList = new();
        public Entry MoveEntry;

        public Dictionary<string, Tool>? Tools;

        public Workshop(string TheCrystalEditorPath, string TheWorkshopName, string TheProjectName, string TheInputDirectory, Dictionary<string, Tool> Tools2, bool IsPreviewModeActive = false) //GameLibrary GameLibrary
        {
            InitializeComponent();

            ExePath = TheCrystalEditorPath;
            WorkshopName = TheWorkshopName;
            IsPreviewMode = IsPreviewModeActive;
            ProjectName = TheProjectName;
            InputDirectory = TheInputDirectory; //GameLibrary.TextBoxInputDirectory.Text + "\\";

            Tools = new Dictionary<string, Tool>();
            Tools = Tools2;

            var sharedMenusControl = this.FindName("MenusForToolsAndEvents") as SharedMenus;
            sharedMenusControl.Tools = this.Tools;


            if (System.IO.File.Exists(ExePath + "\\Settings" + "\\Workshops\\" + WorkshopName + "\\UserOutputDirectory.txt"))
            {
                OutputDirectory = System.IO.File.ReadAllText(ExePath + "\\Settings" + "\\Workshops\\" + WorkshopName + "\\UserOutputDirectory.txt") + "\\";

            }
            else { OutputDirectory = InputDirectory; }

            Database.StartUp();
            //This is all just making sure the current user settings are displayed.
            if (Properties.Settings.Default.EntryPrefix == "None") { ComboBoxEntryPrefixes.Text = "None"; }
            if (Properties.Settings.Default.EntryPrefix == "Row Offset - Decimal Starting at 0") { ComboBoxEntryPrefixes.Text = "Row Offset - Decimal Starting at 0"; }
            if (Properties.Settings.Default.EntryPrefix == "Row Offset - Decimal Starting at 1") { ComboBoxEntryPrefixes.Text = "Row Offset - Decimal Starting at 1"; }
            if (Properties.Settings.Default.EntryPrefix == "Row Offset - Hex Starting at 0x00") { ComboBoxEntryPrefixes.Text = "Row Offset - Hex Starting at 0x00"; }
            if (Properties.Settings.Default.EntryPrefix == "Row Offset - Hex Starting at 0x01") { ComboBoxEntryPrefixes.Text = "Row Offset - Hex Starting at 0x01"; }
            if (Properties.Settings.Default.EntryPrefix == "Byte Address - Decimal Starting at 0") { ComboBoxEntryPrefixes.Text = "Byte Address - Decimal Starting at 0"; }
            if (Properties.Settings.Default.EntryPrefix == "Byte Address - Hex Starting at 0x00") { ComboBoxEntryPrefixes.Text = "Byte Address - Hex Starting at 0x00"; }
            if (Properties.Settings.Default.ColorTheme == "Light") { ColorModeComboBox.Text = "Light Mode"; }
            if (Properties.Settings.Default.ColorTheme == "Dark") { ColorModeComboBox.Text = "Dark Mode"; }
            if (Properties.Settings.Default.DeveloperMode == "User") { ComboBoxDevMode.Text = "User Mode"; }
            if (Properties.Settings.Default.DeveloperMode == "Developer") { ComboBoxDevMode.Text = "Developer Mode"; }
            if (Properties.Settings.Default.CollectionPrefix == "Show") { ComboBoxCollectionPrefix.Text = "Show Collection Prefix"; }
            if (Properties.Settings.Default.CollectionPrefix == "Hide") { ComboBoxCollectionPrefix.Text = "Hide Collection Prefix"; }


            
            LoadDatabaseWithEditorInfo LoadDatabase = new();
            try 
            {
                LoadDatabase.LoadWorkshopInfo(this, Database); //First we load workshop files into the database.
            }
            catch
            {
                MessageBox.Show("The workshop failed to load all files." +
                    "\n" +
                    "\nPossible reasons are as follow:" +
                    "\n1: The input directory is incorrect" +
                    "\n2: You have moved or renamed some files." +
                    "\n3: You failed to extract everything you needed to begin with to use the workshop." +
                    "\n4: The workshop creator has changed the folder / file structure of the workshop." +
                    "\n" +
                    "\nIf you can't stop getting this error, don't keep trying, just ask for help. Especially if you can contact the workshop creator." +
                    "\n" +
                    "\nThe Program will now close as a safety measure.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);


                Application.Current.Shutdown();
                return;
            }            
            LoadDatabase.LoadEditorModeAuto(this, Database); //Then we load the editor info into the database.
            //The above method triggers CreateEditor in a loop, creating every editor using the database information.
            CSDocuments.LoadDocumentation(this);//This is a seperate feature and can be by you for now. It's literally just a built in notepad system.
            DocumentsProject.LoadDocumentation(this);
            
            LoadCommonEvents(); //loads everything related to the common events menu. A user-friendly way to visual program actions directly into the workshop.
            PopulateFileTree(TreeViewWorkshopFiles);

            //Finally we make sure everything is hidden. This is mostly so i don't have to make sure vision is collapsed all the time when developing the program.
            foreach (KeyValuePair<string, Editor> editor in Database.GameEditors)
            {
                editor.Value.EditorDockPanel.Visibility = Visibility.Collapsed;

            }

            

            HIDEALL();


            (Lists.FindName("Lists") as TabItem).Visibility = Visibility.Collapsed;


            if (IsPreviewMode == true)
            {
                // Disabling the WorkshopMenu MenuItem so it cannot be clicked or opened
                WorkshopMenu.IsEnabled = false;
            }

            
        }            
        
        

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////HUD//////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    private void MenuSaveAll(object sender, RoutedEventArgs e)
        {
            //CSDocuments.SaveDocumentsToComputer(DocumentButtonDockPanel, DocumentNameBox, DocumentTextBox);
            SaveWorkshop.OnlySaveEditors(Database, this);
            SaveWorkshop.SaveGameFiles(Database, this);
            CSDocuments.SaveDocumentation(this);
            DocumentsProject.SaveProjectDocumentation(this);



        }
                

        private void MenuSaveEditors(object sender, RoutedEventArgs e)
        {
            SaveWorkshop.OnlySaveEditors(Database, this);
        }

        private void MenuSaveGameData(object sender, RoutedEventArgs e)
        {
            SaveWorkshop.SaveGameFiles(Database, this);
        }

        
        private void ButtonHome_Click(object sender, RoutedEventArgs e)
        {
            
            

            PopulateFileTree(TreeViewWorkshopFiles);

            HIDEALL();
            DockPanelHome.Visibility= Visibility.Visible;

            //EditorsTree.SelectedItemChanged -= EditorsTree_SelectedItemChanged;
            //TreeViewItem Current = EditorsTree.SelectedItem as TreeViewItem;
            //Current.IsSelected = false;
            //EditorsTree.SelectedItemChanged += EditorsTree_SelectedItemChanged;


        }





        private void Button_Click(object sender, RoutedEventArgs e) //Button: Dummy Button (For Testing Stuff)
        {
            //DebugBox.Text = InputDirectory;
            //DebugBox2.Text = FileDatabase.GameFiles[DictionaryOfStrings.TreeCreateEditorFiles].FileName;
            //DebugBox3.Text = FileDatabase.GameFiles[DictionaryOfStrings.TreeCreateEditorFiles].FileNickName;
            //DebugBox4.Text = FileDatabase.GameFiles[DictionaryOfStrings.TreeCreateEditorFiles].FilePath;


            //string Error = "Bad!";
            //Notification f2 = new(Error);
            //f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            //f2.ShowDialog();


            
        }

        private void ComboBoxEntryPrefixes_DropDownClosed(object sender, EventArgs e)
        {
            Part2();
        }

        public void Part2() 
        {
            foreach (var editor in Database.GameEditors)
            {

                foreach (var page in editor.Value.PageList)
                {

                    foreach (var row in page.RowList)
                    {

                        foreach (var column in row.ColumnList)
                        {

                            foreach (var entry in column.EntryList)
                            {

                                if (ComboBoxEntryPrefixes.Text == "None")
                                {
                                    entry.EntryPrefix.Visibility = Visibility.Collapsed;
                                    Properties.Settings.Default.EntryPrefix = "None";
                                }

                                if (ComboBoxEntryPrefixes.Text == "Row Offset - Decimal Starting at 0")
                                {
                                    entry.EntryPrefix.Content = entry.EntryByteOffset;
                                    entry.EntryPrefix.Visibility = Visibility.Visible;
                                    Properties.Settings.Default.EntryPrefix = "Row Offset - Decimal Starting at 0";
                                }

                                if (ComboBoxEntryPrefixes.Text == "Row Offset - Decimal Starting at 1")
                                {
                                    entry.EntryPrefix.Content = entry.EntryByteOffset + 1;
                                    entry.EntryPrefix.Visibility = Visibility.Visible;
                                    Properties.Settings.Default.EntryPrefix = "Row Offset - Decimal Starting at 1";
                                }

                                if (ComboBoxEntryPrefixes.Text == "Row Offset - Hex Starting at 0x00")
                                {
                                    entry.EntryPrefix.Content = entry.EntryByteOffset.ToString("X");
                                    entry.EntryPrefix.Visibility = Visibility.Visible;
                                    Properties.Settings.Default.EntryPrefix = "Row Offset - Hex Starting at 0x00";
                                }

                                if (ComboBoxEntryPrefixes.Text == "Row Offset - Hex Starting at 0x01")
                                {
                                    entry.EntryPrefix.Content = (entry.EntryByteOffset + 1).ToString("X");
                                    entry.EntryPrefix.Visibility = Visibility.Visible;
                                    Properties.Settings.Default.EntryPrefix = "Row Offset - Hex Starting at 0x01";
                                }

                                if (ComboBoxEntryPrefixes.Text == "Byte Address - Decimal Starting at 0")
                                {
                                    entry.EntryPrefix.Content = entry.EntryByteOffset;
                                    entry.EntryPrefix.Visibility = Visibility.Visible;
                                    Properties.Settings.Default.EntryPrefix = "Byte Address - Decimal Starting at 0";
                                }

                                if (ComboBoxEntryPrefixes.Text == "Byte Address - Hex Starting at 0x00")
                                {
                                    entry.EntryPrefix.Content = entry.EntryByteOffset.ToString("X");
                                    entry.EntryPrefix.Visibility = Visibility.Visible;
                                    Properties.Settings.Default.EntryPrefix = "Byte Address - Hex Starting at 0x00";
                                }

                            }
                        }
                    }
                }
            }
            Properties.Settings.Default.Save();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) //Button: Tutorials
        {
            Tutorial f2 = new Tutorial();
            f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            f2.Show();
        }


        private void ComboBoxDevMode_DropDownClosed(object sender, EventArgs e)
        {            

            if (ComboBoxDevMode.Text == "Developer Mode")
            {
                try
                {
                    Properties.Settings.Default.DeveloperMode = "Developer";
                    Properties.Settings.Default.Save();

                    EditorClass.SelectedEntry.EntryBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#35383C"));
                    EditorClass.SelectedEntry.EntryBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999")); //999999  3E8B8E

                    Part2();
                }
                catch 
                {
                    string Error = "An error happened (Because you have not yet selected an editor)." +
                    "\n" +
                    "\nI'll fix it for a future demo. For now, just close and re-open the program. Sorry :(";
                    Notification f2 = new(Error);
                    f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    f2.ShowDialog();
                    return;
                }
                

            }
            else if (ComboBoxDevMode.Text == "User Mode")
            {
                try
                {
                    Properties.Settings.Default.DeveloperMode = "User";
                    Properties.Settings.Default.Save();

                    EditorClass.SelectedEntry.EntryBorder.ClearValue(Border.BackgroundProperty);
                    EditorClass.SelectedEntry.EntryBorder.ClearValue(Border.BorderBrushProperty);


                    foreach (var editor in Database.GameEditors)
                    {

                        foreach (var page in editor.Value.PageList)
                        {

                            foreach (var row in page.RowList)
                            {

                                foreach (var column in row.ColumnList)
                                {

                                    foreach (var entry in column.EntryList)
                                    {
                                        entry.EntryPrefix.Visibility = Visibility.Collapsed;

                                    }
                                }
                            }
                        }
                    }
                }
                catch 
                {
                    string Error = "An error happened (Because you have not yet selected an editor)." +
                    "\n" +
                    "\nI'll fix it for a future demo. For now, just close and re-open the program. Sorry :(";
                    Notification f2 = new(Error);
                    f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    f2.ShowDialog();
                    return;
                }
                




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

        

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////HUD//////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////HOME/////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //private void CreateANewEditor()
        //{

        //}

        private void CreateANewEditor(object sender, RoutedEventArgs e)
        {
            HIDEALL();
            CreateEditorPartDataTable.Visibility= Visibility.Visible;
            
            TreeViewItem item = null;
            EventHandler statusChangedHandler = null;
            statusChangedHandler = (sender, e) =>
            {
                if (FileTreeDataTable.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    FileTreeDataTable.ItemContainerGenerator.StatusChanged -= statusChangedHandler;
                    item = FileTreeDataTable.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                    if (item != null)
                    {
                        item.IsSelected = false;
                    }
                }
            };

            if (FileTreeDataTable.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                FileTreeDataTable.ItemContainerGenerator.StatusChanged += statusChangedHandler;
            }
            else
            {
                item = FileTreeDataTable.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                if (item != null)
                {
                    item.IsSelected = false;
                }
            }

            PopulateFileTree(FileTreeDataTable);



            FileTreeDataTable.Background = null;
            TextBoxRowSize.Background = null;

            var itemToSelect = CreateEditorPartNameTableComboBoxNamesFrom.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == "Pick One");
            if (itemToSelect != null)
            {
                CreateEditorPartNameTableComboBoxNamesFrom.SelectedItem = itemToSelect;
            }

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


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////HOME/////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////EDITOR CREATOR////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void PopulateFileTree(TreeView FileTree) //System.Windows.Controls.
        {
                        
            FileTree.Items.Clear();            
            foreach (KeyValuePair<string, GameFile> GameFile in Database.GameFiles) // Update the FileTree with the current File List.
            {
                TreeViewItem TreeViewItem = new TreeViewItem();
                TextBlock TextBlockItem = new TextBlock();
                if (GameFile.Value.FileNickName != null && GameFile.Value.FileNickName != "")
                {
                    TextBlockItem.Text = GameFile.Value.FileNickName;
                }
                else
                {
                    TextBlockItem.Text = GameFile.Value.FileName;
                }
                TreeViewItem.Header = TextBlockItem;
                TreeViewItem.Tag = GameFile.Value;
                FileTree.Items.Add(TreeViewItem);
                
            }
                        
        }

        private void FileTreeItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var TreeViewItem = e.NewValue as TreeViewItem;
            if (TreeViewItem != null)
            {
                GameFile GameFile = TreeViewItem.Tag as GameFile;

                if (GameFile.FileNickName != null || GameFile.FileNickName != "")
                {
                    FileNicknameBox.Text = GameFile.FileNickName;
                }
                else 
                {
                    FileNicknameBox.Text = GameFile.FileName;
                }
                
            }
            
        }

        

        private void PartNameTableTreeviewFiles_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (PartNameTableTreeviewFiles.SelectedItem == null) 
            {
                return;
            }

            PartNameTableSubTableGrid.Visibility = Visibility.Visible;

            var selectedGameFile = FileTreeDataTable.SelectedItem as TreeViewItem; //The file the user wants to make an editor for.
            GameFile HexFile = selectedGameFile.Tag as GameFile;



            var selectedNameFile = PartNameTableTreeviewFiles.SelectedItem as TreeViewItem; //The file the user wants to make an editor for.
            GameFile ItemNameFile = selectedNameFile.Tag as GameFile;
            if (HexFile.FilePath == ItemNameFile.FilePath)
            {
                GridIsSameTable.Visibility = Visibility.Visible;
                NameTableSetup("No");
            }
            else 
            {
                GridIsSameTable.Visibility = Visibility.Collapsed;
                NameTableSetup("Yes");
                NameTableSetupGrid.Visibility = Visibility.Visible;
            }


        }

        private void PartNameTableComboBoxMainOrSub_DropDownClosed(object sender, EventArgs e)
        {
            NameTableSetup("No");
        }

        private void NameTableSetup(string TriggerAnyway) 
        {
            if (PartNameTableComboBoxMainOrSub.SelectedItem == null) 
            {
                NameTableSetupGrid.Visibility = Visibility.Collapsed;
            }
            else if (PartNameTableComboBoxMainOrSub.SelectedItem.ToString() == "System.Windows.Controls.ComboBoxItem: No, they are in seperate tables" || TriggerAnyway == "Yes")
            {
                NewNameTableRowSize.Text = null;
                NewNameTableRowSize.IsEnabled = true;
                NameTableSetupGrid.Visibility = Visibility.Visible;

            }
            else if (PartNameTableComboBoxMainOrSub.SelectedItem.ToString() == "System.Windows.Controls.ComboBoxItem: Yes, they are in the same table")
            {
                NewNameTableRowSize.Text = TextBoxRowSize.Text;
                NewNameTableRowSize.IsEnabled = false;
                NameTableSetupGrid.Visibility = Visibility.Visible;
            }
            

            
        }

        private void AddNewWorkshopFile(object sender, RoutedEventArgs e)
        {

            VistaOpenFileDialog openFileDialog = new VistaOpenFileDialog();
            openFileDialog.InitialDirectory = InputDirectory;
            //openFileDialog.Filter = "All files (*.*)|*.*";
            //openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                string Testa = openFileDialog.FileName.Substring(InputDirectory.Length).TrimStart('\\');
                foreach (KeyValuePair<string, GameFile> gamefile in Database.GameFiles)
                {                    
                    if (gamefile.Key == Testa)
                    {
                        string Error = "That file is already associated with this workshop. Sometimes games have diffrent folders with identical file names inside them, causing those files to be hard to work with. " +
                            "To deal with this problem, Crystal Editor allows users to assign a file a Nickname. Files with nicknames are shown as if their Nickname IS their filename. " +
                            "To better understand what just happened involving the file you tried adding to the workshop, here is the workshops information on that file. " +
                            "\n" +
                            "\nRealname: " + gamefile.Value.FileName +
                            "\nNickname: " + gamefile.Value.FileNickName +
                            "\nFilepath: " + gamefile.Value.FilePath +
                            "\n" +
                            "\n*The file path is relative, based on the input directory of this project and is not an absolute location on your computer. " +
                            "You can access the input directory from Shortcuts -> open input directory.";
                        Notification Popup = new(Error);
                        Popup.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                        Popup.ShowDialog();
                        return;
                    }
                }
                
                



                GameFile FileInfo = new();    

                FileInfo.FileName = Path.GetFileName(openFileDialog.FileName);
                FileInfo.FilePath = openFileDialog.FileName.Substring(InputDirectory.Length).TrimStart('\\');
                FileInfo.FileBytes = System.IO.File.ReadAllBytes(InputDirectory + FileInfo.FilePath);

                Database.GameFiles.Add(FileInfo.FilePath, FileInfo);





                TreeViewItem TreeItem = new();
                TextBlock TextBlockItem = new();
                TextBlockItem.Text = FileInfo.FileName;
                TreeItem.Header = TextBlockItem;
                TreeItem.Tag = FileInfo;    
                TreeViewWorkshopFiles.Items.Add(TreeItem);


                TreeViewItem TreeItem2 = new();
                TextBlock TextBlockItem2 = new();
                TextBlockItem2.Text = FileInfo.FileName;
                TreeItem2.Header = TextBlockItem2;
                TreeItem2.Tag = FileInfo;
                FileTreeDataTable.Items.Add(TreeItem2);

                TreeViewItem TreeItem3 = new();
                TextBlock TextBlockItem3 = new();
                TextBlockItem3.Text = FileInfo.FileName;
                TreeItem3.Header = TextBlockItem3;
                TreeItem3.Tag = FileInfo;
                PartNameTableTreeviewFiles.Items.Add(TreeItem3);



                //TreeItem.IsSelected = true;




                

                //TreeViewWorkshopFilesCreateEditor.Items.Add(Path.GetFileName(openFileDialog.FileName));
                //FileInfo.FileNickName = Path.GetFileName(openFileDialog.FileName);
                //TreeViewItem addedItem = (TreeViewItem)TreeViewWorkshopFilesCreateEditor.ItemContainerGenerator.ContainerFromItem(FileInfo.FileName);

            }

        }

        


        

        private void Part2ButtonChangeFileNickName_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = TreeViewWorkshopFiles.SelectedItem as TreeViewItem;
            GameFile HexFile = selectedItem.Tag as GameFile;
            HexFile.FileNickName = FileNicknameBox.Text;
            selectedItem.Header = HexFile.FileNickName; //Yes im aware they use a textblock and not a string i will fix it later.

        }


                
        private void CreateEditorPart2ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            FileTreeDataTable.Background = null;
            TextBoxRowSize.Background = null;
            if (FileTreeDataTable.SelectedItem == null)
            {
                FileTreeDataTable.Background = Brushes.Red;

            }

            if (TextBoxRowSize.Text == "0" || TextBoxRowSize.Text == "") 
            {
                TextBoxRowSize.Background= Brushes.Red;
            }

            if (FileTreeDataTable.SelectedItem != null && TextBoxRowSize.Text != "0" && TextBoxRowSize.Text != "")
            {
                CreateEditorPartDataTable.Visibility = Visibility.Collapsed;
                CreateEditorPartNameTable.Visibility = Visibility.Visible;
                PopulateFileTree(PartNameTableTreeviewFiles);
            }

            
            
        }

        private void CreateEditorPart3ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            CreateEditorPartNameTable.Visibility = Visibility.Collapsed;
            CreateEditorPartDataTable.Visibility = Visibility.Visible;
            PopulateFileTree(FileTreeDataTable);

        }

        private void CreateEditorPart3ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            CreateEditorPartNameTable.Visibility = Visibility.Collapsed;
            CreateEditorPartReview.Visibility = Visibility.Visible;

        }
                

        private void CreateEditorPart5ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            CreateEditorPartReview.Visibility = Visibility.Collapsed;
            CreateEditorPartNameTable.Visibility = Visibility.Visible;

        }

        private void CreateEditorPartNameTableComboBoxNamesFrom_DropDownClosed(object sender, EventArgs e)
        {
            if (CreateEditorPartNameTableComboBoxNamesFrom.Text == "Pick One") 
            {
                GridNamesFromFile.Visibility = Visibility.Collapsed;
                GridNamesFromUser.Visibility= Visibility.Collapsed;
                
            }
            if (CreateEditorPartNameTableComboBoxNamesFrom.Text == "Get name list directly from a game file")
            {
                GridNamesFromFile.Visibility = Visibility.Visible;
                GridNamesFromUser.Visibility = Visibility.Collapsed;
            }
            if (CreateEditorPartNameTableComboBoxNamesFrom.Text == "Input my own name list")
            {
                GridNamesFromFile.Visibility = Visibility.Collapsed;
                GridNamesFromUser.Visibility = Visibility.Visible;
            }

            
        }
         
        

        private void CreateNewEditor_Click(object sender, RoutedEventArgs e)
        {
            LoadDatabaseWithEditorInfo LoadDatabase = new();
            LoadDatabase.SetupNewEditor(this, Database);  

        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////EDITOR CREATOR/////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////EDITOR PROPERTIES//////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool thing = false;

        private void PropertiesEditorButtonRenameEditor_Click(object sender, RoutedEventArgs e)
        {
            //string NewKey = PropertiesTextboxEditorName.Text;
            //string OldKey = Database.GameEditors.FirstOrDefault(x => x.Value == EditorClass).Key;
            //string OldKey = EditorClass.Key //Database.GameEditors.FirstOrDefault(x => x.Value == EditorClass).Key;



            //if (Database.GameEditors.ContainsKey(OldKey))
            //{
            //    Editor editor = Database.GameEditors[OldKey];
            //    Database.GameEditors.Remove(OldKey);
            //    Database.GameEditors.Add(NewKey, editor);
            //    editor.EditorTreeViewitem.Header = NewKey;
            //}

            
            Database.GameEditors.Remove(EditorClass.EditorName);
            EditorClass.EditorName = PropertiesTextboxEditorName.Text;
            EditorClass.EditorNameLabel.Content = EditorClass.EditorName;
            Database.GameEditors.Add(EditorClass.EditorName, EditorClass);
            

            
        }

        private void PropertiesEditorButtonDeleteEditor_Click(object sender, RoutedEventArgs e)
        {

            foreach (KeyValuePair<string, Editor> editor in Database.GameEditors)
            {
                if (editor.Value == EditorClass)
                {
                    System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("Are you sure you want to delete this editor?", "Delete Editor", (System.Windows.Forms.MessageBoxButtons)MessageBoxButton.YesNo);

                    if (dr == System.Windows.Forms.DialogResult.Yes)
                    {
                        try
                        {
                            EditorBar.Children.Remove(EditorClass.EditorBarDockPanel);
                            //EditorsTree.Items.Remove(editor.Value.EditorTreeViewitem); //editor.Key

                            Database.GameEditors.Remove(editor.Key);
                            this.MidGrid.Children.Remove(editor.Value.EditorDockPanel);
                            this.EditorProperties.Visibility = Visibility.Collapsed;

                            //Directory.Delete(ExePath + "\\Workshops\\" + WorkshopName + "\\Editors\\" + editor.Key, true);
                            //Returns the view to home should be here.



                        }
                        catch (IOException ex)
                        {
                            // File is being used, so it cannot be deleted
                            Console.WriteLine("File is being used: " + ex.Message);
                        }
                    }

                }
            }

        }














        private void PropertiesEditorButtonChangeTableStart_Click(object sender, RoutedEventArgs e)
        {


            int NewStart = Int32.Parse(PropertiesEditorTableStart.Text);
            int OldStart = EditorClass.EditorTableStart;
            int NumMod = NewStart - OldStart;




            //This next ForEach part makes it so if any of the new final 3 bytes of what would be the new offsets are part
            //of a merged entry, that the table start change is canceled. 
            foreach (var page in EditorClass.PageList)
            {
                foreach (var row in page.RowList)
                {
                    foreach (var column in row.ColumnList)
                    {
                        foreach (var entry in column.EntryList)
                        {
                            //This makes a number called Hoi. Hoi is the new Row offset, if the editor actually changed the start byte of the file it's editing.
                            //if it underflows, it adds tablewidth. If it overflows, it removes tablewidth. So it's always a number inside GameTableSize.
                            int Hoi = entry.EntryByteOffset + -NumMod;
                            if (Hoi < 0) { Hoi = Hoi + EditorClass.EditorTableRowSize; }
                            if (Hoi > EditorClass.EditorTableRowSize - 1) { Hoi = Hoi - EditorClass.EditorTableRowSize; }

                            if (Hoi == EditorClass.EditorTableRowSize - 1) //If the new Zero minus One'th entry is already part of a size 2 entry, cancel.
                            {
                                if (entry.EntryByteSizeNum == 2)
                                {
                                    PropertiesEditorTableStart.Text = OldStart.ToString();
                                    return;
                                }

                            }

                            //If the new Zero -1, or -2, or -3 entrys are already part of a size 4 entry, cancel.
                            if (Hoi == EditorClass.EditorTableRowSize - 1 || Hoi == EditorClass.EditorTableRowSize - 2 || Hoi == EditorClass.EditorTableRowSize - 3)
                            {
                                if (entry.EntryByteSizeNum == 4)
                                {
                                    PropertiesEditorTableStart.Text = OldStart.ToString();
                                    return;
                                }

                            }

                        }
                    }
                }
            }


            //We have not triggered a cancelation, and are now going to actually change the table start.

            EditorClass.EditorTableStart = NewStart; //Changes the starting byte of the editor's table, to the new one the user wanted.
            foreach (var page in EditorClass.PageList)
            {
                foreach (var row in page.RowList)
                {
                    foreach (var column in row.ColumnList)
                    {
                        foreach (var entry in column.EntryList)
                        {

                            entry.EntryByteOffset = entry.EntryByteOffset + -NumMod;
                            if (entry.EntryByteOffset < 0) { entry.EntryByteOffset = entry.EntryByteOffset + EditorClass.EditorTableRowSize; }
                            if (entry.EntryByteOffset > EditorClass.EditorTableRowSize - 1) { entry.EntryByteOffset = entry.EntryByteOffset - EditorClass.EditorTableRowSize; }
                            entry.EntryPrefix.Content = entry.EntryByteOffset.ToString();
                                                        
                            EntryManager.LoadEntry(this, EditorClass, entry);


                        }
                    }
                }
            }



            // +/- 1 to all offsets?
            // If NewOffset > RowSize: NewOffset - RowSize and reload ByteValue + Entry
        }





        

        

        private void PropertiesEditorButtonChangeNameSubTableStartByte_Click(object sender, RoutedEventArgs e)
        {

            ////PropertiesEditorButtonChangeNameMainTableStartByte

            ////int NewStart = Int32.Parse(PropertiesEditorTableStart.Text);
            ////int OldStart = EditorClass.GameTableStart;
            ////int NumMod = NewStart - OldStart;

            //EditorClass.NameMainTableStart = Int32.Parse(PropertiesEditorNameMainTableStartByte.Text);
            //CharacterSetAscii SetAscii = new();
            //SetAscii.DecodeAscii(EditorClass, EditorClass.NameTableFile.FileBytes);


            //foreach (TreeViewItem TreeViewItem in EditorClass.LeftBar.TreeView.Items)
            //{
            //    ItemInfo ItemInfo = TreeViewItem.Tag as ItemInfo;
            //    ItemNameBuilder(TreeViewItem);

            //}









            int Num = int.Parse(PropertiesEditorNameTableStartByte.Text);

            if (Num < 0)
            {

                PropertiesEditorNameTableStartByte.Text = EditorClass.NameTableStart.ToString();
                string Error = "You just attempted to make the Name Table's Starting Byte be less then 0. I'm not sure why you tried to do this, but it is not allowed. " +
                    "\n" +
                    "\nIf this was not a mistake and there is a reason i don't understand why this would ever be desired, you can tell me on discord and i'll consider not explicitly preventing this behavior." +
                    "\n" +
                    "\nThe textbox has been reset to it's previous value. ";
                Notification f2 = new(Error);
                f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
                return;
            }

            
            //if (Num + EditorClass.NameTableRowSize > EditorClass.NameTableTextSize)
            //{

            //    PropertiesEditorNameTableStartByte.Text = EditorClass.NameTableStart.ToString();
            //    string Error = "Probably an easy mistake or math error, but you just attempted to make the Name Sub Table's Starting Byte so close to the end of the Main Table's size, " +
            //        "that due to the size of this Sub-Table, the Sub-Table would have overflowed onto the next row of information. " +
            //        "\n" +
            //        "\nAn easy way to think about this, is if SubTableStart + SubTableSize is bigger then MainTableSize, then it's to big." +
            //        "\n" +
            //        "\nIf this was not entirely a mistake, Either the Sub-Table's size was axidentally set to large, or it's possible the current editor has the wrong " +
            //        "Main-Table size. Also, currently this is checking the *Current* values of everything, and the number you put into the textbox for SubTableStart." +
            //        "If you changed the number in the SubTableSize or MainTableSize but did not click the change button, then it's not using those numbers but the origonals." +                    
            //        "\n" +
            //        "\nThe textbox has been reset to it's previous value. ";
            //    Notification f2 = new(Error);
            //    f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            //    f2.ShowDialog();
            //    return;
            //}


            EditorClass.NameTableStart = Num;
            CharacterSetManager CharacterManager = new();
            CharacterManager.Decode(this, EditorClass, "Items");
            foreach (TreeViewItem TreeViewItem in EditorClass.LeftBar.TreeView.Items)
            {
                ItemInfo ItemInfo = TreeViewItem.Tag as ItemInfo;
                ItemNameBuilder(TreeViewItem);

            }


        }

        private void EditorCharacterSetDropdownClose(object sender, EventArgs e)
        {
            if (EditorClass.NameTableCharacterSet == PropertiesEditorNameTableCharacterSetDropdown.Text) 
            {
                return;
            }


            EditorClass.NameTableCharacterSet = PropertiesEditorNameTableCharacterSetDropdown.Text;
            CharacterSetManager CharacterManager = new();
            CharacterManager.Decode(this, EditorClass, "Items");
            foreach (TreeViewItem TreeViewItem in EditorClass.LeftBar.TreeView.Items)
            {
                ItemInfo ItemInfo = TreeViewItem.Tag as ItemInfo;
                ItemNameBuilder(TreeViewItem);

            }
        }
        //string Error = "";
        //Notification f2 = new(Error);
        //f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        //f2.ShowDialog();
        private void ChangeNameTableTextSize(object sender, RoutedEventArgs e)
        {
            int Num = int.Parse(PropertiesEditorNameTableTextSize.Text);

            if (Num < 0)
            {

                PropertiesEditorNameTableTextSize.Text = EditorClass.NameTableTextSize.ToString();
                string Error = "You just attempted to make the Name Table Text Size be less then 0." +
                    "\n" +
                    "\nThis value is how many letters / characters are being read from a file." +
                    "\nHopefully it is obvious why this number cannot be less then 0." +
                    "\n" +
                    "\nThe textbox has been reset to it's previous value. ";
                Notification f2 = new(Error);
                f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
                return;
            }



            EditorClass.NameTableTextSize = Num;
            //CharacterSetAscii SetAscii = new();
            //SetAscii.DecodeAscii(EditorClass, EditorClass.NameTableFile.FileBytes);
            CharacterSetManager CharacterManager = new();
            CharacterManager.Decode(this, EditorClass, "Items");
            foreach (TreeViewItem TreeViewItem in EditorClass.LeftBar.TreeView.Items)
            {
                ItemInfo ItemInfo = TreeViewItem.Tag as ItemInfo;
                ItemNameBuilder(TreeViewItem);

            }
        }

        private void ChangeNameTableSize(object sender, RoutedEventArgs e)
        {
            
            int Num = int.Parse(PropertiesEditorNameTableRowSize.Text);

            if (Num < 0) 
            {

                PropertiesEditorNameTableRowSize.Text = EditorClass.NameTableRowSize.ToString();
                string Error = "You just attempted to make the Name Table Row Size be less then 0." +
                    "\n" +
                    "\nIf you got confused, the Row Size is how many bytes are in 1 FULL Row of a table, not just how many bytes of text your dealing with." +
                    "\n" +
                    "\nFow example..." +
                    "\n01 02 03 04 05 00 00 00" +
                    "\n01 02 03 04 05 00 00 00" +
                    "\nThe Row Size here is 8, but the text size is 5." +
                    "\n(Actually text size is probably 7, The max text size + a 00 byte)" +
                    "\n" +
                    "\nThe textbox has been reset to it's previous value. ";
                Notification f2 = new(Error);
                f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
                return;
            }
             


            EditorClass.NameTableRowSize = Num;
            //CharacterSetAscii SetAscii = new();
            //SetAscii.DecodeAscii(EditorClass, EditorClass.NameTableFile.FileBytes);
            CharacterSetManager CharacterManager = new();
            CharacterManager.Decode(this, EditorClass, "Items");
            foreach (TreeViewItem TreeViewItem in EditorClass.LeftBar.TreeView.Items)
            {
                ItemInfo ItemInfo = TreeViewItem.Tag as ItemInfo;
                ItemNameBuilder(TreeViewItem);

            }


        }

        private void PropertiesEditorButtonChangeNameCount_Click(object sender, RoutedEventArgs e)
        {
            //All counts are treated as -1, in order to make it so humans can better understand items. This way item 22 is the one with number 22, not 23. (Due to 0 being a number)
            int NewCount = Int32.Parse(PropertiesEditorNameCount.Text);
            int OldCount = EditorClass.NameTableItemCount - 1;
            int Diffrence = NewCount - OldCount;

            if (NewCount < 1) 
            {

                string Error = "As a precautionary measure against any accidents, attempting to delete ALL items is not allowed." +
                "\n" +
                "\nThe textbox has been reset to it's previous value. ";
                Notification f2 = new(Error);
                f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
                PropertiesEditorNameCount.Text = OldCount.ToString();
                return;
            }

            if (Diffrence > 0) 
            {
                for (int i = OldCount; i != NewCount; i++) 
                {
                    TreeViewItem TreeItem = new();
                    ItemInfo ItemInfo = new();
                    TreeItem.Tag = ItemInfo;
                    ItemInfo.ItemIndex = i + 1;



                    EditorClass.LeftBar.ItemList.Add(ItemInfo);
                    EditorClass.LeftBar.TreeView.Items.Add(TreeItem);


                }


                //CharacterSetAscii Encoding = new();
                //Encoding.DecodeAscii(EditorClass, EditorClass.NameTableFile.FileBytes);
                CharacterSetManager CharacterManager = new();
                CharacterManager.Decode(this, EditorClass, "Items");
                foreach (TreeViewItem TreeViewItem in EditorClass.LeftBar.TreeView.Items)
                {
                    ItemInfo ItemInfo = TreeViewItem.Tag as ItemInfo;
                    ItemNameBuilder(TreeViewItem);

                }

                EditorClass.NameTableItemCount = NewCount + 1;


            }
            else if (Diffrence < 0) 
            {
                //Delete treeview items that have offsets X~Y.
                //
                for (int i = Diffrence; i != 0; i++) 
                {
                    int Target = OldCount + 1 + i; //i is a negative so we add it to lower the number.
                    foreach (TreeViewItem Item in EditorClass.LeftBar.TreeView.Items) 
                    {
                        ItemInfo ItemInfo = Item.Tag as ItemInfo;
                        if (ItemInfo.ItemIndex == Target) 
                        {
                            EditorClass.LeftBar.TreeView.Items.Remove(Item);
                            EditorClass.LeftBar.ItemList.Remove(ItemInfo);
                            break;
                        }
                        if (ItemInfo.ItemType == "Folder")
                        {
                            foreach (TreeViewItem childItem in Item.Items)
                            {
                                ItemInfo childItemInfo = childItem.Tag as ItemInfo;
                                if (childItemInfo.ItemIndex == Target)
                                {
                                    // If the child item has the target index, remove it
                                    Item.Items.Remove(childItem);
                                    EditorClass.LeftBar.ItemList.Remove(childItemInfo);
                                    break;
                                }
                            }
                                                        
                        }

                    }

                    
                }
                EditorClass.NameTableItemCount = NewCount + 1;
                
            }


        }







        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////EDITOR PROPERTIES//////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////LEFT BAR ITEM PROPERTIES////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        public void CreateFolder(TreeView TreeView, TreeViewItem TreeViewItem) 
        {
            ItemInfo TreeViewItemInfo = TreeViewItem.Tag as ItemInfo;
            if (TreeViewItemInfo.ItemType == "Folder" || TreeViewItemInfo.ItemType == "Child") { return; }

            TreeViewSelectionEnabled = false;


            int selectedIndex = TreeView.ItemContainerGenerator.IndexFromContainer(TreeViewItem);   
            TreeView.Items.Remove(TreeViewItem);

            TreeViewItem FolderItem = new TreeViewItem();
            ItemInfo FolderItemInfo = new();
            FolderItem.Tag = FolderItemInfo;

            FolderItemInfo.ItemName = "New Folder";
            FolderItemInfo.ItemType = "Folder";
            if (TreeViewItem.Tag is ItemInfo itemInfo)
            {
                itemInfo.ItemType = "Child";
            }

            ItemNameBuilder(FolderItem); //Created the Header text as a TextBlockItem
            TreeView.Items.Insert(selectedIndex, FolderItem);
            FolderItem.Items.Add(TreeViewItem);


            TreeViewSelectionEnabled = true;


            ContextMenu contextMenu = new ContextMenu();
            
            MenuItem MenuItemDeleteFolder = new MenuItem();
            MenuItemDeleteFolder.Header = "Delete Folder (If Empty)";
            MenuItemDeleteFolder.Click += (sender, e) => DeleteFolder(TreeView, FolderItem);
            contextMenu.Items.Add(MenuItemDeleteFolder);

            FolderItem.ContextMenu = contextMenu;
        }

        public void DeleteFolder(TreeView TreeView, TreeViewItem TreeViewItem) 
        {
            ItemInfo TreeViewItemInfo = TreeViewItem.Tag as ItemInfo;
            if (TreeViewItemInfo.ItemType != "Folder" || TreeViewItem.Items.Count > 0) { return; } 

            TreeViewSelectionEnabled = false;            
            TreeView.Items.Remove(TreeViewItem);
            TreeViewSelectionEnabled = true;
        }
       

        private void PropertiesItemButtonCreateFolder_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem TargetItem = EditorClass.LeftBar.TreeView.SelectedItem as TreeViewItem;
            System.Windows.Controls.TreeView parentTreeView = EditorClass.LeftBar.TreeView;
            int selectedIndex = parentTreeView.ItemContainerGenerator.IndexFromContainer(TargetItem);

            TreeViewSelectionEnabled = false;

            parentTreeView.Items.Remove(TargetItem);

            TreeViewItem TreeItem = new TreeViewItem();
            ItemInfo ItemInfo = new();
            TreeItem.Tag = ItemInfo;
            
            ItemInfo.ItemName = "New Folder";            
            //ItemInfo.Index = -1;
            //ItemInfo.Index = null;
            ItemInfo.ItemType = "Folder";
            if (TargetItem.Tag is ItemInfo itemInfo)
            {
                itemInfo.ItemType = "Child";
            }

            ItemNameBuilder(TreeItem); //Created the Header text as a TextBlockItem
            parentTreeView.Items.Insert(selectedIndex, TreeItem);
            TreeItem.Items.Add(TargetItem);

            


            TreeViewSelectionEnabled = true;
        }

        public void ItemNameBuilder(TreeViewItem TreeItem) 
        {
            ItemInfo ItemInfo = TreeItem.Tag as ItemInfo;
            TextBlock TextBlockItem = new TextBlock();
            Run run1 = new Run();
            if (Properties.Settings.Default.CollectionPrefix == "Show")
            {
                run1.Text = ItemInfo.ItemIndex + ": " + ItemInfo.ItemName + " ";
            }
            else if (Properties.Settings.Default.CollectionPrefix == "Hide")
            {
                run1.Text = ItemInfo.ItemName + " ";
            }
            if (ItemInfo.ItemType == "Folder")
            {
                Run runFolder = new Run();
                runFolder.Foreground = Brushes.Yellow;
                runFolder.Text = "📁 ";
                run1.Text = ItemInfo.ItemName + " ";
                TextBlockItem.Inlines.Add(runFolder);
            }

            // Create the second part of the text
            Run run2 = new Run();
            run2.Text = ItemInfo.ItemNote;
            run2.Foreground = Brushes.Orange; // Set the foreground to red   DeepSkyBlue

            // Add the runs to the text block
            TextBlockItem.Inlines.Add(run1);
            TextBlockItem.Inlines.Add(run2);

            if (ItemInfo.ItemTooltip != "") { TreeItem.ToolTip = ItemInfo.ItemTooltip; }


            TreeItem.Header = TextBlockItem;
        }

        private void ComboBoxCollectionPrefix_DropDownClosed(object sender, EventArgs e)
        {
            if (ComboBoxCollectionPrefix.Text == "Show Collection Prefix")
            {
                Properties.Settings.Default.CollectionPrefix = "Show";
                Properties.Settings.Default.Save();
                                

                foreach (var editor in Database.GameEditors)
                {                    

                    foreach (TreeViewItem TreeItem in editor.Value.LeftBar.TreeView.Items)
                    {
                        ItemNameBuilder(TreeItem);
                    }
                }

                
            }
            if (ComboBoxCollectionPrefix.Text == "Hide Collection Prefix")
            {
                Properties.Settings.Default.CollectionPrefix = "Hide";
                Properties.Settings.Default.Save();

                foreach (var editor in Database.GameEditors)
                {

                    foreach (TreeViewItem TreeItem in editor.Value.LeftBar.TreeView.Items)
                    {
                        ItemNameBuilder(TreeItem);
                    }
                }
            }
        }

        public bool TreeViewSelectionEnabled = true;

        private void PropertiesItemButtonMoveUp_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedItem = EditorClass.LeftBar.TreeView.SelectedItem as TreeViewItem;
            ItemInfo SelectedItemInfo = selectedItem.Tag as ItemInfo;
            System.Windows.Controls.TreeView parentTreeView = EditorClass.LeftBar.TreeView;

            int selectedIndex = parentTreeView.ItemContainerGenerator.IndexFromContainer(selectedItem);
            if (selectedIndex == -1) { selectedIndex = 1; }
            if (selectedIndex > 0)
            {

                TreeViewSelectionEnabled = false; // Disable the event

                TreeViewItem previousItem = parentTreeView.ItemContainerGenerator.ContainerFromIndex(selectedIndex - 1) as TreeViewItem;
                if (previousItem != null && ((ItemInfo)previousItem.Tag).ItemType == "Folder")
                {
                    parentTreeView.Items.Remove(selectedItem);
                    previousItem.Items.Add(selectedItem);
                    selectedItem.IsSelected = true;
                    SelectedItemInfo.ItemType = "Child";
                }
                else if (SelectedItemInfo.ItemType == "Child")
                {
                    TreeViewItem parentItem = (TreeViewItem)selectedItem.Parent;
                    int ChildIndex = parentItem.ItemContainerGenerator.IndexFromContainer(selectedItem);
                    int parentitems = parentItem.Items.Count - 1;
                    if (ChildIndex != 0) //3: Move while inside a folder.
                    {
                        parentItem.Items.Remove(selectedItem);
                        parentItem.Items.Insert(ChildIndex - 1, selectedItem);
                        selectedItem.IsSelected = true;
                    }
                    else //2: Move out of a folder.
                    {
                        int NextIndex = parentTreeView.ItemContainerGenerator.IndexFromContainer(parentItem);
                        parentItem.Items.Remove(selectedItem);
                        parentTreeView.Items.Insert(NextIndex, selectedItem);
                        selectedItem.IsSelected = true;
                        SelectedItemInfo.ItemType = null;
                    }
                }
                else
                {
                    int newIndex = selectedIndex - 1;
                    parentTreeView.Items.Remove(selectedItem);
                    parentTreeView.Items.Insert(newIndex, selectedItem);
                    selectedItem.IsSelected = true;

                }

                TreeViewSelectionEnabled = true; // Enable the event
            }
        }


        private void PropertiesItemButtonMoveDown_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedItem = EditorClass.LeftBar.TreeView.SelectedItem as TreeViewItem;
            ItemInfo SelectedItemInfo = selectedItem.Tag as ItemInfo;
            System.Windows.Controls.TreeView parentTreeView = EditorClass.LeftBar.TreeView;

            int selectedIndex = parentTreeView.ItemContainerGenerator.IndexFromContainer(selectedItem);
            if (selectedIndex < EditorClass.LeftBar.TreeView.Items.Count - 1) //if there is something next to us, then we do 1 of 3 movements.
            {

                TreeViewSelectionEnabled = false; // Disable the event

                TreeViewItem nextItem = parentTreeView.ItemContainerGenerator.ContainerFromIndex(selectedIndex + 1) as TreeViewItem;
                if (nextItem != null && ((ItemInfo)nextItem.Tag).ItemType == "Folder") //4: Move Into a folder.
                {
                    parentTreeView.Items.Remove(selectedItem);
                    nextItem.Items.Insert(0, selectedItem);
                    selectedItem.IsSelected = true;
                    SelectedItemInfo.ItemType = "Child";
                }
                else if (SelectedItemInfo.ItemType == "Child")
                {
                    TreeViewItem parentItem = (TreeViewItem)selectedItem.Parent;
                    int ChildIndex = parentItem.ItemContainerGenerator.IndexFromContainer(selectedItem);
                    int parentitems = parentItem.Items.Count - 1;
                    if (ChildIndex < parentitems) //3: Move while inside a folder.
                    {
                        parentItem.Items.Remove(selectedItem);
                        parentItem.Items.Insert(ChildIndex + 1, selectedItem);
                        selectedItem.IsSelected = true;
                    }
                    else //2: Move out of a folder.
                    {
                        int NextIndex = parentTreeView.ItemContainerGenerator.IndexFromContainer(parentItem) + 1;
                        parentItem.Items.Remove(selectedItem);
                        parentTreeView.Items.Insert(NextIndex, selectedItem);
                        selectedItem.IsSelected = true;
                        SelectedItemInfo.ItemType = null;
                    }
                }
                else //1: Move normally.
                {
                    int newIndex = selectedIndex + 1;
                    parentTreeView.Items.Remove(selectedItem);
                    parentTreeView.Items.Insert(newIndex, selectedItem);
                    selectedItem.IsSelected = true;
                }

                TreeViewSelectionEnabled = true; // Enable the event
            }
        }


        

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////LEFT BAR ITEM PROPERTIES////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////PAGE PROPERTIES///////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////PAGE PROPERTIES///////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////ROW PROPERTIES//////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void CreateNewRowAbove(Row TheRow)
        {
            Row NewRow = new();
            NewRow.RowName = "New Row";
            NewRow.ColumnList = new List<Column>();
            NewRow.RowPage = TheRow.RowPage;

            int TheIndex = TheRow.RowPage.RowList.IndexOf(TheRow);
            NewRow.RowPage.RowList.Insert(TheIndex, NewRow);



            EditorCreate.CreateRow(NewRow.RowPage, NewRow, this, Database, TheIndex);


            Column ColumnClass = new Column { ColumnName = "New Column" };
            NewRow.ColumnList.Add(ColumnClass);
            ColumnClass.EntryList = new List<Entry>();
            ColumnClass.ColumnRow = NewRow;
            EditorCreate.CreateColumn(NewRow, ColumnClass, this, Database, -1);

        }


        public void CreateNewRowBelow(Row TheRow)
        {
            Row NewRow = new();
            NewRow.RowName = "New Row";
            NewRow.ColumnList = new List<Column>();
            NewRow.RowPage = TheRow.RowPage;

            int TheIndex = TheRow.RowPage.RowList.IndexOf(TheRow) + 1;
            NewRow.RowPage.RowList.Insert(TheIndex, NewRow);



            EditorCreate.CreateRow(NewRow.RowPage, NewRow, this, Database, TheIndex);


            Column ColumnClass = new Column { ColumnName = "New Column" };
            NewRow.ColumnList.Add(ColumnClass);
            ColumnClass.EntryList = new List<Entry>();
            ColumnClass.ColumnRow = NewRow;
            EditorCreate.CreateColumn(NewRow, ColumnClass, this, Database, -1);

        }



        private void RowMoveUp_Click(object sender, RoutedEventArgs e)
        {
            int primaryIndex = PageClass.DockPanel.Children.IndexOf(RowClass.RowDockPanel);
            if (primaryIndex != 0)
            {
                var secondaryIndex = primaryIndex - 1;
                Row primary = PageClass.RowList[primaryIndex];
                Row secondary = PageClass.RowList[secondaryIndex];

                PageClass.DockPanel.Children.Remove(primary.RowDockPanel);
                PageClass.RowList.RemoveAt(primaryIndex);
                PageClass.DockPanel.Children.Insert(secondaryIndex, primary.RowDockPanel);
                PageClass.RowList.Insert(secondaryIndex, primary);

            }
        }

        private void RowMoveDown_Click(object sender, RoutedEventArgs e)
        {

            int primaryIndex = PageClass.DockPanel.Children.IndexOf(RowClass.RowDockPanel);
            if (primaryIndex + 1 < PageClass.RowList.Count)
            {
                var secondaryIndex = primaryIndex + 1;
                Row primary = PageClass.RowList[primaryIndex];
                Row secondary = PageClass.RowList[secondaryIndex];

                PageClass.DockPanel.Children.Remove(primary.RowDockPanel);
                PageClass.RowList.RemoveAt(primaryIndex);
                PageClass.DockPanel.Children.Insert(primaryIndex + 1, primary.RowDockPanel);
                PageClass.RowList.Insert(secondaryIndex, primary);

            }



        }

        private void SaveRow_Click(object sender, RoutedEventArgs e)
        {
            RowClass.RowName = PropertiesRowNameBox.Text;
            RowClass.RowLabel.Content = PropertiesRowNameBox.Text;
        }

        private void RowNewColumn_Click(object sender, RoutedEventArgs e)
        {
            Column Column = new Column { ColumnName = "New Column" };
            RowClass.ColumnList.Add(Column);
            Column.EntryList = new List<Entry>();
            Column.ColumnRow = RowClass;

            EditorCreate.CreateColumn(RowClass, Column, this, Database, -1);

        }


        private void PropertiesDeleteRow_Click(object sender, RoutedEventArgs e)
        {

            if (RowClass.ColumnList.Count == 0)
            {
                RowClass.RowPage.DockPanel.Children.Remove(RowClass.RowDockPanel);
                RowClass.RowPage.RowList.Remove(RowClass);
            }

            //Add a IF: count all entrys in all columns in this row, and do IF that is 0.
            //Be careful about other Rows updating their Row order!
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////ROW PROPERTIES//////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////COLUMN PROPERTIES/////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        

        public void CreateNewColumnRight(Column TheColumn)
        {
            Column Column = new();
            Column.ColumnName = "New Column";
            Column.EntryList = new List<Entry>();
            Column.ColumnRow = TheColumn.ColumnRow;

            int TheIndex = TheColumn.ColumnRow.ColumnList.IndexOf(TheColumn) + 1;
            Column.ColumnRow.ColumnList.Insert(TheIndex, Column);



            EditorCreate.CreateColumn(Column.ColumnRow, Column, this, Database, TheIndex);

        }

        public void CreateNewColumnLeft(Column TheColumn)
        {
            Column Column = new();
            Column.ColumnName = "New Column";
            Column.EntryList = new List<Entry>();
            Column.ColumnRow = TheColumn.ColumnRow;

            int TheIndex = TheColumn.ColumnRow.ColumnList.IndexOf(TheColumn);
            Column.ColumnRow.ColumnList.Insert(TheIndex, Column);



            EditorCreate.CreateColumn(Column.ColumnRow, Column, this, Database, TheIndex);

        }

        public void ColumnDelete(Column TheColumn)
        {
            if (TheColumn.EntryList.Count == 0)
            {
                TheColumn.ColumnRow.RowDockPanel.Children.Remove(TheColumn.ColumnGrid);
                TheColumn.ColumnRow.ColumnList.Remove(TheColumn);
                ColumnProperties.Visibility = Visibility.Collapsed;
            }


            if (TheColumn.ColumnRow.ColumnList.Count == 0)
            {
                TheColumn.ColumnRow.RowPage.DockPanel.Children.Remove(TheColumn.ColumnRow.RowDockPanel);
                TheColumn.ColumnRow.RowPage.RowList.Remove(TheColumn.ColumnRow);
            }
            

            //I need to delete from memory?

            //I WILL need to be careful about other columns updating their column order, because it WILL be used for the order they are drawn in? 
            //wait but rows dont actually save their order as a number to XML. ???

        }







        private void SaveColumn_Click(object sender, RoutedEventArgs e)
        {
            ColumnClass.ColumnName = PropertiesColumnNameBox.Text;
            ColumnClass.ColumnLabel.Content = PropertiesColumnNameBox.Text;
        }

        private void PropertiesColumnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ColumnClass.EntryList.Count == 0)
            {
                ColumnClass.ColumnRow.RowDockPanel.Children.Remove(ColumnClass.ColumnGrid);
                ColumnClass.ColumnRow.ColumnList.Remove(ColumnClass);
                ColumnProperties.Visibility= Visibility.Collapsed;
            }


            if (RowClass.ColumnList.Count == 0)
            {
                RowClass.RowPage.DockPanel.Children.Remove(RowClass.RowDockPanel);
                RowClass.RowPage.RowList.Remove(RowClass);
            }
            

            //I need to delete from memory?

            //I WILL need to be careful about other columns updating their column order, because it WILL be used for the order they are drawn in? 
            //wait but rows dont actually save their order as a number to XML. ???

        }






        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////COLUMN PROPERTIES//////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////ENTRY PROPERTIES//////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void LabelWidth(Column ColumnClass)
        {
            double maxWidth = 0;
            var EntryList = ColumnClass.EntryList;

            // Measure the desired width of each label without restrictions
            foreach (Entry entry in EntryList)
            {
                entry.EntryLabel.MinWidth = 0;
                entry.EntryLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double labelWidth = entry.EntryLabel.DesiredSize.Width;
                if (labelWidth > maxWidth)
                {
                    maxWidth = labelWidth;
                }
            }

            // Set the MinWidth of each label to the widest value
            foreach (Entry entry in EntryList)
            {
                entry.EntryLabel.MinWidth = maxWidth;
            }
        }

        public void PropertiesHide()
        {
            EditorProperties.Visibility = Visibility.Collapsed;
            //ItemProperties.Visibility = Visibility.Collapsed;
            PageProperties.Visibility = Visibility.Collapsed;
            RowProperties.Visibility = Visibility.Collapsed;
            ColumnProperties.Visibility = Visibility.Collapsed;
            EntryProperties.Visibility = Visibility.Collapsed;
        }



        private void SaveEntry_Click(object sender, RoutedEventArgs e)
        {
            EntryClass.EntryName = PropertiesNameBox.Text;
            EntryClass.EntryLabel.Content = PropertiesNameBox.Text;

            if (EntryClass.EntrySubType == "CheckBox") 
            {
                EntryClass.EntryTypeCheckBox.CheckBoxTrueText = PropertiesEntryCheckText.Text;
                EntryClass.EntryTypeCheckBox.CheckBoxFalseText = PropertiesEntryUncheckText.Text;
            }
            
            if (EntryClass.EntrySubType == "BitFlag")
            {
                EntryClass.EntryTypeBitFlag.BitFlag1Name = PropertiesEntryBitFlag1Name.Text;
                EntryClass.EntryTypeBitFlag.BitFlag1CheckText = PropertiesEntryBitFlag1CheckText.Text;
                EntryClass.EntryTypeBitFlag.BitFlag1UncheckText = PropertiesEntryBitFlag1UncheckText.Text;
                EntryClass.EntryTypeBitFlag.BitFlag2Name = PropertiesEntryBitFlag2Name.Text;
                EntryClass.EntryTypeBitFlag.BitFlag2CheckText = PropertiesEntryBitFlag2CheckText.Text;
                EntryClass.EntryTypeBitFlag.BitFlag2UncheckText = PropertiesEntryBitFlag2UncheckText.Text;
                EntryClass.EntryTypeBitFlag.BitFlag3Name = PropertiesEntryBitFlag3Name.Text;
                EntryClass.EntryTypeBitFlag.BitFlag3CheckText = PropertiesEntryBitFlag3CheckText.Text;
                EntryClass.EntryTypeBitFlag.BitFlag3UncheckText = PropertiesEntryBitFlag3UncheckText.Text;
                EntryClass.EntryTypeBitFlag.BitFlag4Name = PropertiesEntryBitFlag4Name.Text;
                EntryClass.EntryTypeBitFlag.BitFlag4CheckText = PropertiesEntryBitFlag4CheckText.Text;
                EntryClass.EntryTypeBitFlag.BitFlag4UncheckText = PropertiesEntryBitFlag4UncheckText.Text;
                EntryClass.EntryTypeBitFlag.BitFlag5Name = PropertiesEntryBitFlag5Name.Text;
                EntryClass.EntryTypeBitFlag.BitFlag5CheckText = PropertiesEntryBitFlag5CheckText.Text;
                EntryClass.EntryTypeBitFlag.BitFlag5UncheckText = PropertiesEntryBitFlag5UncheckText.Text;
                EntryClass.EntryTypeBitFlag.BitFlag6Name = PropertiesEntryBitFlag6Name.Text;
                EntryClass.EntryTypeBitFlag.BitFlag6CheckText = PropertiesEntryBitFlag6CheckText.Text;
                EntryClass.EntryTypeBitFlag.BitFlag6UncheckText = PropertiesEntryBitFlag6UncheckText.Text;
                EntryClass.EntryTypeBitFlag.BitFlag7Name = PropertiesEntryBitFlag7Name.Text;
                EntryClass.EntryTypeBitFlag.BitFlag7CheckText = PropertiesEntryBitFlag7CheckText.Text;
                EntryClass.EntryTypeBitFlag.BitFlag7UncheckText = PropertiesEntryBitFlag7UncheckText.Text;
                EntryClass.EntryTypeBitFlag.BitFlag8Name = PropertiesEntryBitFlag8Name.Text;
                EntryClass.EntryTypeBitFlag.BitFlag8CheckText = PropertiesEntryBitFlag8CheckText.Text;
                EntryClass.EntryTypeBitFlag.BitFlag8UncheckText = PropertiesEntryBitFlag8UncheckText.Text;
            }
            
                        
            EntryManager.LoadEntry(this,EditorClass, EntryClass);
            Dispatcher.InvokeAsync(() => LabelWidth(EntryClass.EntryColumn), System.Windows.Threading.DispatcherPriority.Loaded);
            LabelWidth(EntryClass.EntryColumn);
        }

        private void EntryMoveUp_Click(object sender, RoutedEventArgs e)
        {
            var editor = EditorClass;
            var page = editor.PageList.FirstOrDefault(p => p.RowList.Any(r => r.ColumnList.Any(c => c.EntryList.Contains(EntryClass))));
            var row = page.RowList.First(r => r.ColumnList.Any(c => c.EntryList.Contains(EntryClass)));
            int rowIndex = page.RowList.IndexOf(row);

            int primaryIndex = ColumnClass.ColumnGrid.Children.IndexOf(EntryClass.EntryBorder) - 1; //Counting starts at 1, but the first child is 0.            
            if (primaryIndex > 0)
            {
                var secondaryIndex = primaryIndex - 1;
                var primary = ColumnClass.EntryList[primaryIndex];     //The PrimaryEntry target being moved
                var secondary = ColumnClass.EntryList[secondaryIndex]; //The secondary target being moved.

                ColumnClass.ColumnGrid.Children.Remove(secondary.EntryBorder);
                ColumnClass.EntryList.RemoveAt(secondaryIndex);
                ColumnClass.EntryList.Insert(primaryIndex + EntryClass.EntryByteSizeNum - 1, secondary);
                ColumnClass.ColumnGrid.Children.Insert(primaryIndex + EntryClass.EntryByteSizeNum, secondary.EntryBorder);

            }
            else if (PageClass.RowList.IndexOf(RowClass) != 0) //RowClass.ColumnList.Count > CIndex + 1
            {
                for (int i = 0; i < EntryClass.EntryByteSizeNum; i++)
                {
                    var PrimaryEntry = ColumnClass.EntryList[primaryIndex];
                    int num = PageClass.RowList[rowIndex - 1].ColumnList[0].EntryList.Count; //This gets the bottom location of the column above the current one for a entry to move to.             

                    ColumnClass.ColumnGrid.Children.Remove(PrimaryEntry.EntryBorder);
                    ColumnClass.EntryList.RemoveAt(primaryIndex);
                    PageClass.RowList[rowIndex - 1].ColumnList[0].ColumnGrid.Children.Insert(num + 1, PrimaryEntry.EntryBorder);
                    PageClass.RowList[rowIndex - 1].ColumnList[0].EntryList.Insert(num, PrimaryEntry);

                    PrimaryEntry.EntryRow = PageClass.RowList[rowIndex - 1];
                    PrimaryEntry.EntryColumn = PageClass.RowList[rowIndex - 1].ColumnList[0];
                }

                RowClass = PageClass.RowList[rowIndex - 1];
                ColumnClass = PageClass.RowList[rowIndex - 1].ColumnList[0];


            }
        }

        private void EntryMoveDown_Click(object sender, RoutedEventArgs e)
        {

            var editor = EditorClass;
            var page = editor.PageList.FirstOrDefault(p => p.RowList.Any(r => r.ColumnList.Any(c => c.EntryList.Contains(EntryClass))));
            var row = page.RowList.First(r => r.ColumnList.Any(c => c.EntryList.Contains(EntryClass)));
            int rowIndex = page.RowList.IndexOf(row);

            int primaryIndex = ColumnClass.ColumnGrid.Children.IndexOf(EntryClass.EntryBorder) - 1; //Counting starts at 1, but the first child is 0.
            if (primaryIndex < ColumnClass.EntryList.Count - EntryClass.EntryByteSizeNum)
            {
                var secondaryIndex = primaryIndex + EntryClass.EntryByteSizeNum;
                var primary = ColumnClass.EntryList[primaryIndex];     //The PrimaryEntry target being moved.
                var secondary = ColumnClass.EntryList[secondaryIndex]; //The secondary target being moved.

                ColumnClass.EntryList.RemoveAt(secondaryIndex);
                ColumnClass.ColumnGrid.Children.Remove(secondary.EntryBorder);
                ColumnClass.EntryList.Insert(primaryIndex, secondary);
                ColumnClass.ColumnGrid.Children.Insert(primaryIndex + 1, secondary.EntryBorder);


            }
            else if (PageClass.RowList.Count - 1 > rowIndex)//if this entry's row has a row below it.
            {
                for (int i = 0; i < EntryClass.EntryByteSizeNum; i++)
                {
                    var primary = ColumnClass.EntryList[primaryIndex];

                    ColumnClass.ColumnGrid.Children.Remove(primary.EntryBorder);
                    ColumnClass.EntryList.RemoveAt(primaryIndex);
                    PageClass.RowList[rowIndex + 1].ColumnList[0].ColumnGrid.Children.Insert(i + 1, primary.EntryBorder);
                    PageClass.RowList[rowIndex + 1].ColumnList[0].EntryList.Insert(i, primary);

                    primary.EntryRow = PageClass.RowList[rowIndex + 1];
                    primary.EntryColumn = PageClass.RowList[rowIndex + 1].ColumnList[0];
                }

                RowClass = PageClass.RowList[rowIndex + 1];
                ColumnClass = PageClass.RowList[rowIndex + 1].ColumnList[0];

            }

        }

        private void EntryMoveRight_Click(object sender, RoutedEventArgs e)
        {
            int CIndex = EntryClass.EntryRow.ColumnList.IndexOf(EntryClass.EntryColumn);
            int primaryIndex = ColumnClass.ColumnGrid.Children.IndexOf(EntryClass.EntryBorder) - 1; //Counting starts at 1, but the first child is 0.

            if (RowClass.ColumnList.Count > CIndex + 1) //&& RowClass.ColumnList[CIndex + 1] != null
            {
                for (int i = 0; i < EntryClass.EntryByteSizeNum; i++)
                {
                    var primary = ColumnClass.EntryList[primaryIndex];

                    ColumnClass.ColumnGrid.Children.Remove(primary.EntryBorder);
                    ColumnClass.EntryList.RemoveAt(primaryIndex);
                    RowClass.ColumnList[CIndex + 1].ColumnGrid.Children.Insert(i + 1, primary.EntryBorder);
                    RowClass.ColumnList[CIndex + 1].EntryList.Insert(i, primary);

                    primary.EntryColumn = RowClass.ColumnList[CIndex + 1];
                }


                ColumnClass = RowClass.ColumnList[CIndex + 1];

            }
            else
            {

            }

        }

        private void EntryMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            int CIndex = EntryClass.EntryRow.ColumnList.IndexOf(EntryClass.EntryColumn);
            int primaryIndex = ColumnClass.ColumnGrid.Children.IndexOf(EntryClass.EntryBorder) - 1; //Counting starts at 1, but the first child is 0.

            if (CIndex != 0) //&& RowClass.ColumnList[CIndex + 1] != null
            {
                for (int i = 0; i < EntryClass.EntryByteSizeNum; i++)
                {
                    var primary = ColumnClass.EntryList[primaryIndex];

                    ColumnClass.ColumnGrid.Children.Remove(primary.EntryBorder);
                    ColumnClass.EntryList.RemoveAt(primaryIndex);
                    RowClass.ColumnList[CIndex - 1].ColumnGrid.Children.Insert(i + 1, primary.EntryBorder);
                    RowClass.ColumnList[CIndex - 1].EntryList.Insert(i, primary);

                    primary.EntryColumn = RowClass.ColumnList[CIndex - 1];
                }


                ColumnClass = RowClass.ColumnList[CIndex - 1];

            }
            else
            {

            }
        }


        private void PropertiesEntryLabelShow_DropDownClosed(object sender, EventArgs e)
        {
            if (PropertiesEntryLabelShow.Text == "Hide Name")
            {
                EntryClass.EntryLabelShown = "Hide Name";
                EntryClass.EntryLabel.Visibility = Visibility.Collapsed;
            }
            if (PropertiesEntryLabelShow.Text == "Show Name")
            {
                EntryClass.EntryLabelShown = "Show Name";
                EntryClass.EntryLabel.Visibility = Visibility.Visible;
            }
        }

        private void PropertiesEntryType_DropDownClosed(object sender, EventArgs e)
        {
            string NewEntryType = PropertiesEntryType.Text;
                        
            EntryManager.EntryChange(Database, NewEntryType, this, EntryClass);
        }



        private void PropertiesEntryByteSizeComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (EntryClass.EntryByteSize != PropertiesEntryByteSizeComboBox.Text) //Cancel method IF: The combobox text did not change.
            {
                string FindEntryByteSize = "Dummy";
                if (EntryClass.EntryByteSize == "1") { FindEntryByteSize = "1 Byte"; } //This makes it so the entrys current type appears in the properties dropdown.
                if (EntryClass.EntryByteSize == "2L") { FindEntryByteSize = "2 Bytes Little Endian"; }
                if (EntryClass.EntryByteSize == "2B") { FindEntryByteSize = "2 Bytes Big Endian"; }


                //Cancel method IF: Entry is attempting to merge with an already merged entry.
                if (PropertiesEntryByteSizeComboBox.Text == "2 Bytes Little Endian" || PropertiesEntryByteSizeComboBox.Text == "2 Bytes Big Endian")
                {
                    foreach (var page in EditorClass.PageList)
                    {
                        foreach (var row in page.RowList)
                        {
                            foreach (var column in row.ColumnList)
                            {
                                foreach (var entry in column.EntryList)
                                {
                                    if (entry.EntryByteOffset == EntryClass.EntryByteOffset + 1)
                                    {
                                        if (entry.EntryByteSizeNum == 2 ) 
                                        {
                                            PropertiesEntryByteSizeComboBox.Text = FindEntryByteSize;
                                            string Error = "You cannot merge an entry, with an entry that is already merged with something else. " +
                                                "\n\n" +
                                                "If your confused, entrys merge with those next in offset decimal order, not those that are just under them in the UI. ";
                                            Notification f2 = new(Error);
                                            f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                            f2.ShowDialog();
                                            return;
                                        }
                                        if (entry.EntrySaveState == "Disabled")
                                        { 
                                            PropertiesEntryByteSizeComboBox.Text = FindEntryByteSize;
                                            string Error = "You cannot merge an entry, with an entry that is disabled. " +
                                                "\n\n" +
                                                "Disabled entrys have a red color tint and users can't edit them. " +
                                                "Entrys can be disabled for a few reasons. One, they are disabled automatically if they are text used in the editor. " +
                                                "Two, an editor maker can choose to disable them manually. As for why, there are any number of reasons, but one example " +
                                                "is if editing that information causes the game to crash. " +
                                                "\n\n" +
                                                "You can manually un-disable the entry, but entrys related to text will automatically re-disable themself when the editor loads back up, " +
                                                "even if you save them as non-disabled.";
                                            Notification f2 = new(Error);
                                            f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                            f2.ShowDialog();
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //Cancel method IF: Entry is attempting to merge with an already merged entry.
                if (PropertiesEntryByteSizeComboBox.Text == "4 Bytes Little Endian" || PropertiesEntryByteSizeComboBox.Text == "4 Bytes Big Endian")
                {
                    foreach (var page in EditorClass.PageList)
                    {
                        foreach (var row in page.RowList)
                        {
                            foreach (var column in row.ColumnList)
                            {
                                foreach (var entry in column.EntryList)
                                {
                                    if (entry.EntryByteOffset == EntryClass.EntryByteOffset + 1 || entry.EntryByteOffset == EntryClass.EntryByteOffset + 2 || entry.EntryByteOffset == EntryClass.EntryByteOffset + 3)
                                    {
                                        if (entry.EntryByteSizeNum == 2 || entry.EntryByteSizeNum == 4) 
                                        { 
                                            PropertiesEntryByteSizeComboBox.Text = FindEntryByteSize;
                                            string Error = "You cannot merge an entry, with an entry that is already merged with something else. " +
                                               "\n\n" +
                                               "Atleast one of the 4 bytes / entrys you were attempting to merge with, is already merged. " +
                                               "If your confused, entrys merge with those next in offset decimal order, not those that are just under them in the UI. ";
                                            Notification f2 = new(Error);
                                            f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                            f2.ShowDialog();
                                            return; 
                                        }
                                        if (entry.EntrySaveState == "Disabled")
                                        {
                                            PropertiesEntryByteSizeComboBox.Text = FindEntryByteSize;
                                            string Error = "You cannot merge an entry, with an entry that is disabled. " +
                                                "\n\n" +
                                                "atleast one of the 4 entrys you were trying to merge, is disabled." +
                                                "\n\n" +
                                                "Disabled entrys have a red color tint and users can't edit them. " +
                                                "Entrys can be disabled for a few reasons. One, they are disabled automatically if they are text used in the editor. " +
                                                "Two, an editor maker can choose to disable them manually. As for why, there are any number of reasons, but one example " +
                                                "is if editing that information causes the game to crash. " +
                                                "\n\n" +
                                                "You can manually un-disable the entry, but entrys related to text will automatically re-disable themself when the editor loads back up, " +
                                                "even if you save them as non-disabled.";
                                            Notification f2 = new(Error);
                                            f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                            f2.ShowDialog();
                                            return;
                                        }
                                    }

                                }
                            }
                        }
                    }
                }





                if (EntryClass.EntrySubType == "List") //Cancel method IF: A List-Type entry wants to become size-4.
                {


                    if (PropertiesEntryByteSizeComboBox.Text == "4 Bytes Little Endian" || PropertiesEntryByteSizeComboBox.Text == "4 Bytes Big Endian")
                    {

                        foreach (ComboBoxItem item in PropertiesEntryByteSizeComboBox.Items)
                        {
                            if (item.Content.ToString() == FindEntryByteSize)
                            {
                                PropertiesEntryByteSizeComboBox.SelectedItem = item;
                                break;
                            }
                        }
                        return;
                    }
                }

                int OldSize = EntryClass.EntryByteSizeNum;
                int NewSize = 999;
                string NewSizeS = "x";


                if (PropertiesEntryByteSizeComboBox.Text == "1 Byte") { NewSize = 1; NewSizeS = "1"; }
                if (PropertiesEntryByteSizeComboBox.Text == "2 Bytes Little Endian") { NewSize = 2; NewSizeS = "2L"; }
                if (PropertiesEntryByteSizeComboBox.Text == "4 Bytes Little Endian") { NewSize = 4; NewSizeS = "4L"; }
                if (PropertiesEntryByteSizeComboBox.Text == "2 Bytes Big Endian") { NewSize = 2; NewSizeS = "2B"; }
                if (PropertiesEntryByteSizeComboBox.Text == "4 Bytes Big Endian") { NewSize = 4; NewSizeS = "4B"; }

                if (EntryClass.EntryByteOffset <= EditorClass.EditorTableRowSize - NewSize) //Cancel method IF: New size would overflow off the end of GameTableSize
                {


                    EntryClass.EntryByteSize = NewSizeS;
                    EntryClass.EntryByteSizeNum = NewSize;

                    if (EntryClass.EntryTypeList != null) 
                    {
                        if (EntryClass.EntryTypeList.ListItems != null)
                        {
                            if (NewSize == 1)
                            {
                                string[] items = EntryClass.EntryTypeList.ListItems;
                                Array.Resize(ref items, 256);
                                EntryClass.EntryTypeList.ListItems = items;
                                EntryClass.EntryTypeList.ListSize = 256;
                            }
                            if (NewSize == 2)
                            {
                                string[] items = EntryClass.EntryTypeList.ListItems;
                                Array.Resize(ref items, 65536);
                                EntryClass.EntryTypeList.ListItems = items;
                                EntryClass.EntryTypeList.ListSize = 65536;
                            }
                        }
                    }
                    



                    
                    //EntryData.ReloadEntry(Database, EditorClass, EntryClass);
                    EntryManager.LoadEntry(this, EditorClass, EntryClass);



                    //i need to add a check to make sure the next entrys are currently ALL ByteSize 1, if even one is not, cancel the process.


                    //Below is what happens to OTHER ENTRYS, NOT the main entry having it's size changed.


                    List<Entry> targets = new();


                    List<int> list = new();

                    //if Old smaller then new
                    if (OldSize < NewSize)
                    {
                        foreach (int i in Enumerable.Range(Math.Min(OldSize, NewSize), Math.Abs(OldSize - NewSize)))
                        {
                            list.Add(EntryClass.EntryByteOffset + i);
                        }
                        foreach (var num in list)
                        {
                            foreach (var page in EditorClass.PageList)
                            {
                                foreach (var row in page.RowList)
                                {
                                    foreach (var column in row.ColumnList)
                                    {
                                        foreach (Entry entry in column.EntryList)
                                        {
                                            if (entry.EntryByteOffset == num) //Search for entry with offset+1 over current entry.  Offset+X
                                            {
                                                entry.EntryByteSize = "0";
                                                entry.EntryByteSizeNum = 0;
                                                EntryManager.LoadEntry(this, EditorClass, entry);
                                                entry.EntryBorder.Visibility = Visibility.Collapsed;

                                                targets.Add(entry);

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //This makes sure when a entry is hidden / merged, that it is relocated to wherever the primary entry is.
                    foreach (var entry in targets)
                    {


                        int MyIndex = entry.EntryColumn.EntryList.IndexOf(entry); //entry.EntryColumn.ColumnGrid.Children.IndexOf(entry.EntryDockPanel);
                        entry.EntryColumn.EntryList.RemoveAt(MyIndex);
                        entry.EntryColumn.ColumnGrid.Children.Remove(entry.EntryBorder);


                        int EntryLocation = ColumnClass.ColumnGrid.Children.IndexOf(EntryClass.EntryBorder) - 1; //Counting starts at 1, but the first child is 0.
                        int Diffrence = entry.EntryByteOffset - EntryClass.EntryByteOffset;
                        ColumnClass.EntryList.Insert(EntryLocation + Diffrence, entry);
                        ColumnClass.ColumnGrid.Children.Insert(EntryLocation + Diffrence + 1, entry.EntryBorder);


                        entry.EntryRow = EntryClass.EntryRow;  //  PageClass.RowList[rowIndex + 1];
                        entry.EntryColumn = EntryClass.EntryColumn;   //PageClass.RowList[rowIndex + 1].ColumnList[0];
                    }

                    ////if Old bigger then new
                    if (OldSize > NewSize)
                    {
                        foreach (int i in Enumerable.Range(Math.Min(OldSize, NewSize), Math.Abs(OldSize - NewSize))) //.OrderByDescending(x => x)
                        {
                            list.Add(EntryClass.EntryByteOffset + i);
                        }
                        foreach (var num in list)
                        {
                            foreach (var page in EditorClass.PageList)
                            {
                                foreach (var row in page.RowList)
                                {
                                    foreach (var column in row.ColumnList)
                                    {
                                        foreach (var entry in column.EntryList)
                                        {
                                            if (entry.EntryByteOffset == num) //Search for entry with offset+1 over current entry.  Offset+X
                                            {
                                                entry.EntryByteSize = "1";
                                                entry.EntryByteSizeNum = 1;
                                                EntryManager.LoadEntry(this, EditorClass, entry);
                                                //EntryData.ReloadEntry(Database, EditorClass, entry);
                                                entry.EntryBorder.Visibility = Visibility.Visible;

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }



                    //I am currently not allowing size 4+ of Lists, i don't know that any game that uses more then 65K options to select from in one menu.

                    //if (NewSize == 4)   
                    //{
                    //    string[] items = EntryClass.ListItems;
                    //    Array.Resize(ref items, 4294967296);
                    //    EntryClass.ListItems = items;
                    //}




                }//end of IF Row Overflow  //Makes sure byte size only changes if it won't overflow off the end of GameTableSize
                else
                {
                    PropertiesEntryByteSizeComboBox.Text = FindEntryByteSize;
                }

            }//End of IF
        }



        



        private void PropertiesEntryNumberBoxSign_DropDownClosed(object sender, EventArgs e)
        {
            //if (PropertiesEntryNumberBoxSign.Text == "Positive Only")
            //{
            //    EntryClass.EntryTypeNumberBox.NumberSign = "Positive";
                
            //    EntryManager.LoadEntry(this, EditorClass, EntryClass);
            //}
            //if (PropertiesEntryNumberBoxSign.Text == "Positive and Negative")
            //{
            //    EntryClass.EntryTypeNumberBox.NumberSign = "Negative";
                
            //    EntryManager.LoadEntry(this, EditorClass, EntryClass);
            //}


        }

        


        



        private void PropertiesEntrySaveState_DropDownClosed(object sender, EventArgs e)
        {
            //If saving is disabled, changes to this entry will not be saved.
            //This is useful to explicitly prevent users from messing with things that crash the game.
            //Also can be used for other reasons.
            if (PropertiesEntrySaveState.Text == "Saving Enabled")
            {
                EntryClass.EntrySaveState = "Enabled";
                EntryClass.EntryBorder.Style = (Style)Application.Current.Resources[typeof(Border)];
                EditorClass.SelectedEntry.EntryBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#35383C")); //364448  232528
                EditorClass.SelectedEntry.EntryBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999")); //999999  3E8B8E
            }
            if (PropertiesEntrySaveState.Text == "Saving Disabled")
            {
                EntryClass.EntrySaveState = "Disabled";
                EntryClass.EntryBorder.Style = (Style)Application.Current.Resources["EntryDisabled"];
                EditorClass.SelectedEntry.EntryBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A272B")); //483137   2C1C20
                EditorClass.SelectedEntry.EntryBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#793F72")); //915968
            }


            EntryManager.EntryChange(Database, EntryClass.EntrySubType, this, EntryClass);


        }










        
        /// ///////////////////////////////////////////////// GOOGLE SHEETS
        
        private void ExportToSheetsHex(object sender, RoutedEventArgs e)
        {
            ExportToGoogleSheets ExportToGoogleSheets = new();
            ExportToGoogleSheets.ToGoogleSheetHex(this, EditorClass);
        }
        private void ExportToSheetsDec(object sender, RoutedEventArgs e)
        {
            ExportToGoogleSheets ExportToGoogleSheets = new();
            ExportToGoogleSheets.ToGoogleSheetDecimal(this, EditorClass);
        }

        private void OpenInputFolder(object sender, RoutedEventArgs e)
        {
            try
            {                
                if (Directory.Exists(InputDirectory))
                {
                    System.Diagnostics.Process.Start("explorer.exe", InputDirectory);
                }
                else
                {
                    MessageBox.Show("We can't find where your projects input folder is! :(" +
                        "\n" +
                        "\n(This is actually a pretty serious error, so you should probably look into fixing it." +
                        "\n" +
                        "\nIf your looking at the workshop in preview mode, you will always get this error and can ignore it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            catch
            {
                PropertiesEditorNameTableRowSize.Text = EditorClass.NameTableRowSize.ToString();
                string Error = "An error occured." +
                    "\n" +
                    "\nError in Workshop.xaml.cs InputFolder";
                Notification f2 = new(Error);
                f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
                return;
            }
            
        }

        private void OpenOutputFolder(object sender, RoutedEventArgs e)
        {
            try
            { 
                if (Directory.Exists(OutputDirectory))
                {
                    System.Diagnostics.Process.Start("explorer.exe", OutputDirectory);
                }
                else
                {
                    MessageBox.Show("We can't find where your projects output folder is! :(" +
                        "\n" +
                        "\n(This is actually a pretty serious error, so you should probably look into fixing it." +
                        "\n" +
                        "\nIf your looking at the workshop in preview mode, you will always get this error and can ignore it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch 
            {
                PropertiesEditorNameTableRowSize.Text = EditorClass.NameTableRowSize.ToString();
                string Error = "An error occured." +
                    "\n" +
                    "\nError in Workshop.xaml.cs OutputFolder";
                Notification f2 = new(Error);
                f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
                return;
            }
            
        }

        private void OpenWorkshopFolder(object sender, RoutedEventArgs e)
        {
            try
            {
                string folderPath = ExePath + "\\Workshops\\" + WorkshopName + "\\";


                if (Directory.Exists(folderPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", folderPath);
                }
                else
                {
                    MessageBox.Show("We can't find where your workshop folder is! :(" +
                        "\n" +
                        "\n(I actually have no idea how anyone could even get this error, " +
                        "\nexcept maybe moving your folders around while the program is still running, but i think windows would stop you?", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch 
            {
                PropertiesEditorNameTableRowSize.Text = EditorClass.NameTableRowSize.ToString();
                string Error = "An error occured." +
                    "\n" +
                    "\nError in Workshop.xaml.cs WorkshopFolder";
                Notification f2 = new(Error);
                f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
                return;
            }
            
        }

        private void OpenDownloadsFolder(object sender, RoutedEventArgs e)
        {

            try
            {
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                // Open the folder in the file explorer
                if (Directory.Exists(folderPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", folderPath);
                }
                else
                {
                    MessageBox.Show("We can't find where your downloads folder is! :(" +
                        "\n" +
                        "\nIDK what would cause this error. Maybe your on a max or linux PC instead of windows?" +
                        "\nIf anyone gets this error on windows, please report it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                PropertiesEditorNameTableRowSize.Text = EditorClass.NameTableRowSize.ToString();
                string Error = "An error occured." +
                    "\n" +
                    "\nError in Workshop.xaml.cs DownloadsFolder";
                Notification f2 = new(Error);
                f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
                return;
            }
            
        }
                

        private void DSNitroPack(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog FolderSelect = new VistaFolderBrowserDialog(); //This starts folder selection using Ookii.Dialogs.WPF NuGet Package
            FolderSelect.Description = "Please select the folder named " + System.IO.File.ReadAllText(ExePath + "\\Workshops\\" + WorkshopName + "\\Input Directory.txt"); //This sets a description to help remind the user what their looking for.
            FolderSelect.UseDescriptionForTitle = true;    //This enables the description to appear.        
            if ((bool)FolderSelect.ShowDialog(this)) //This triggers the folder selection screen, and if the user does not cancel out...
            {
                string Input = FolderSelect.SelectedPath;
                string Output = Path.GetDirectoryName(Input);
                string Name = "ROM.nds";
                


                string packCommand = $"\"{ExePath}\\Tools\\Console\\Nintendo DS\\NitroPacker\\NitroPacker.exe\" pack -p \"{Input}\\GAME.xml\" -r \"{Output}\\{Name}\"";
                ProcessStartInfo psi = new();
                psi.FileName = "cmd.exe";
                psi.Arguments = $"/K \"{packCommand}\"";
                psi.WorkingDirectory = $"{ExePath}\\Tools\\Console\\Nintendo DS\\NitroPacker";

                Process p = new Process();
                p.StartInfo = psi;
                p.Start();

                

            }



            
        }

        private void DSNitroUnpack(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog fileDialog = new VistaOpenFileDialog();
            fileDialog.Title = "Select a file"; // Set the dialog title
            fileDialog.Filter = "All files (*.*)|*.*"; // Set the file filter
            if ((bool)fileDialog.ShowDialog(this)) // Show the file dialog and check if the user clicked OK
            {
                string NDSRom = fileDialog.FileName; // Get the selected file path
                string RomFolder = Path.GetDirectoryName(NDSRom); // Get the directory path of the selected file

                // Create the "Unpacked Rom" folder at the directory path
                //string UnpackedRomFolder = Path.Combine(RomFolder, "Unpacked " + Path.GetFileNameWithoutExtension(fileDialog.FileName)); // Get just the name of the selected file without the extension);
                //Directory.CreateDirectory(UnpackedRomFolder);
                //string Output = UnpackedRomFolder + "\\GAME.xml\"";

                string Output = RomFolder + "\\Unpacked " + Path.GetFileNameWithoutExtension(fileDialog.FileName);
                // Set the text of the debug box to the directory path
                


                string UnpackNDSRom = $"\"{ExePath}\\Tools\\Console\\Nintendo DS\\NitroPacker\\NitroPacker.exe\" unpack -r \"{NDSRom}\" -o \"{Output}\" -p GAME";
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = $"/K \"{UnpackNDSRom}\"";
                psi.WorkingDirectory = $"{ExePath}\\Tools\\Console\\Nintendo DS\\NitroPacker";

                Process p = new Process();
                p.StartInfo = psi;
                p.Start();

                
            }
        }

        

        public void HIDEALL() 
        {
            foreach (KeyValuePair<string, Editor> editor in Database.GameEditors)
            {
                editor.Value.EditorDockPanel.Visibility = Visibility.Collapsed;
            }
            DockPanelHome.Visibility = Visibility.Collapsed;
            CreateEditorPartDataTable.Visibility = Visibility.Collapsed;
            CreateEditorPartNameTable.Visibility = Visibility.Collapsed;
            CreateEditorPartReview.Visibility = Visibility.Collapsed;
            

            PartNameTableSubTableGrid.Visibility = Visibility.Collapsed;
            NameTableSetupGrid.Visibility = Visibility.Collapsed;
            GridNamesFromFile.Visibility = Visibility.Collapsed;
            GridNamesFromUser.Visibility = Visibility.Collapsed;
            CreateEditorPartNameTable.Visibility = Visibility.Collapsed;
            GridIsSameTable.Visibility = Visibility.Collapsed;

            GridNewExtraTable.Visibility = Visibility.Collapsed;
            EventScreen.Visibility = Visibility.Collapsed;
            EditorGraphicsWindow.Visibility = Visibility.Collapsed;
            EditorListTableWindow.Visibility = Visibility.Collapsed;

            GridWorkshopToolsList.Visibility = Visibility.Collapsed;
        }

        

        

        
                

        private void ButtonAddExtraTable(object sender, RoutedEventArgs e)
        {
            HIDEALL();
            PopulateFileTree(FileTreeExtraTable);

            GridNewExtraTable.Visibility = Visibility.Visible;
            ExtraTableNameTextbox.Text = "";
            ExtraTableStartByteTextbox.Text = "";
            ExtraTableTextSizeTextbox.Text = "";
            ExtraTableFullRowTextbox.Text = "";
            ExtraTableCharacterSet.Text = "";

            TreeExtraTables.Items.Clear();
            foreach (ExtraTable ExtraTable in EditorClass.ExtraTableList) 
            {
                TreeViewItem TreeViewItem = new();
                TreeViewItem.Header = ExtraTable.ExtraTableName;
                TreeViewItem.Tag = ExtraTable;
                TreeExtraTables.Items.Add(TreeViewItem);
            }


        }

        private void TreeExtraTables_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem TreeViewItem = TreeExtraTables.SelectedItem as TreeViewItem;
            if (TreeViewItem == null) { return; }

            ExtraTable ExtraTable = TreeViewItem.Tag as ExtraTable;
            ExtraTableNameTextbox.Text = ExtraTable.ExtraTableName;
            ExtraTableStartByteTextbox.Text = ExtraTable.ExtraTableStart.ToString();
            ExtraTableTextSizeTextbox.Text = ExtraTable.ExtraTableTextSize.ToString();
            ExtraTableFullRowTextbox.Text = ExtraTable.ExtraTableRowSize.ToString();
            ExtraTableCharacterSet.Text = ExtraTable.ExtraTableCharacterSet;


            var FileItem = FileTreeExtraTable.SelectedItem as TreeViewItem;
            if (FileItem != null) { FileItem.IsSelected = false;  }

            foreach (TreeViewItem Item2 in FileTreeExtraTable.Items)
            {
                //ExtraTable ExtraTable2 = Item2.Tag as ExtraTable;
                if (Item2.Tag == ExtraTable.ExtraTableFile)
                {
                    Item2.IsSelected = true;
                    break;
                }
            }

            if (ExtraTable.ExtraTableFilePath != "" || ExtraTable.ExtraTableFilePath != null)
            {
                
            }
            else 
            {
                //var FileItem = FileTreeExtraTable.SelectedItem as TreeViewItem;
                //FileItem.IsSelected = false;
                //I need to make this so it unselects the previous file when moving to a new extra table.
            }



        }

        private void ButtonNewExtraTable(object sender, RoutedEventArgs e)
        {
            ExtraTable ExtraTable = new();
            EditorClass.ExtraTableList.Add(ExtraTable);
            ExtraTable.ExtraTableName = "New Extra Table";

            TreeViewItem TreeViewItem = new();
            TreeViewItem.Header = "New Extra Table";
            TreeViewItem.Tag = ExtraTable;
            TreeExtraTables.Items.Add(TreeViewItem);
        }

        private void ExtraTableCancel(object sender, RoutedEventArgs e)
        {
            GridNewExtraTable.Visibility = Visibility.Collapsed;
        }

        private void CreateNewExtraTable(object sender, RoutedEventArgs e)
        {
            TreeViewItem TreeViewItem = TreeExtraTables.SelectedItem as TreeViewItem;
            var ExtraTableItem = TreeExtraTables.SelectedItem as TreeViewItem;
            ExtraTable ExtraTable = TreeViewItem.Tag as ExtraTable;
                        

            var FileItem = FileTreeExtraTable.SelectedItem as TreeViewItem; //The file the user wants to make an editor for.

            ExtraTable.ExtraTableName = ExtraTableNameTextbox.Text;
            ExtraTableItem.Header= ExtraTable.ExtraTableName;
            ExtraTable.ExtraTableFile = FileItem.Tag as GameFile;
            ExtraTable.ExtraTableCharacterSet = ExtraTableCharacterSet.Text;
            ExtraTable.ExtraTableFilePath = ExtraTable.ExtraTableFile.FilePath;
            ExtraTable.ExtraTableStart = int.Parse(ExtraTableStartByteTextbox.Text);
            ExtraTable.ExtraTableTextSize = int.Parse(ExtraTableTextSizeTextbox.Text);
            ExtraTable.ExtraTableRowSize = int.Parse(ExtraTableFullRowTextbox.Text);


            //TextBox ExtraTextBox = new();
            //ExtraTextBox.AcceptsReturn = true;
            //ExtraTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //ExtraTextBox.TextWrapping = TextWrapping.Wrap;
            //ExtraTextBox.FontSize = 16;
            //DockPanel.SetDock(ExtraTextBox, Dock.Top);
            //EditorClass.TopBar.TopPanel.Children.Add(ExtraTextBox);
            //ExtraTextBox.Height = 45;
            //ExtraTable.ExtraTableTextBox = ExtraTextBox;

            //GridNewExtraTable.Visibility = Visibility.Collapsed;

                      
        }

        private void EditorBarMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                // Scroll right
                EditorBarScrollViewer.ScrollToHorizontalOffset(EditorBarScrollViewer.HorizontalOffset + 70);
            }
            else
            {
                // Scroll left
                EditorBarScrollViewer.ScrollToHorizontalOffset(EditorBarScrollViewer.HorizontalOffset - 70);
            }

            // Mark the event as handled so it doesn't propagate further
            e.Handled = true;
        }

        private void SetWindow1080P(object sender, RoutedEventArgs e)
        {
            Window window = this; // If this method is inside the Window's code-behind
                                  // Window window = someWindowInstance; // If you need to access a specific window instance

            //double targetWidth = 1920;
            //double targetHeight = 1080;

            //window.WindowState = WindowState.Normal; // Reset the window state to normal before resizing
            //window.Width = targetWidth;
            //window.Height = targetHeight;
            //window.Top = (SystemParameters.WorkArea.Height - targetHeight) / 2;
            //window.Left = (SystemParameters.WorkArea.Width - targetWidth) / 2;

            window.Width = 1920;
            window.Height = 1040;
        }


        

        public void FillLearnBox(Editor TheEditor, Entry TheEntry)
        {
            EntryValueInsightDataGrid.Items.Clear();

            int Goal = EditorClass.NameTableItemCount;
            if (Goal == 0) //Makes editors that don't get item names from a file work with sheet exports.
            {
                foreach (var Item in EditorClass.LeftBar.ItemList)
                {
                    if (Item.ItemType != "Folder")
                    {
                        Goal++;
                    }
                }
            }
            Dictionary<int, NumberCount> counts = new Dictionary<int, NumberCount>();

            for (int i = 0; i < Goal; i++)
            {
                int Num = EditorClass.EditorFile.FileBytes[EditorClass.EditorTableStart + (i * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset];

                if (counts.ContainsKey(Num))
                {
                    counts[Num].Count++;
                    counts[Num].RowIndices.Add(i);
                }
                else
                {
                    counts[Num] = new NumberCount { Number = Num, Count = 1, RowIndices = new List<int> { i } };
                }
            }

            List<int> sortedKeys = counts.Keys.ToList();
            sortedKeys.Sort();

            foreach (int key in sortedKeys)
            {
                EntryValueInsightDataGrid.Items.Add(counts[key]);
            }
        }

        private void ApplyFormulaToEntryAcrossAllItems(object sender, RoutedEventArgs e)
        {
            //Almost everything here uses doubles instead of ints to make ABSOLUTELY FUCKING SURE nothing EVER goes out of range, even when adding more value types or using large negatives from super robot wars / disgaea.
            //FormulaComboBox
            //FormulaTextBox

            if (EntryClass.EntrySaveState == "Disabled") { return; } //prevents users from axidentally modding values that should be otherwise already disabled.

            //if (EntryClass.EntryByteSize != "1") { return; } //temporary

            int FinalItem = 0;
            try
            {                
                if (EditorClass.NameTableItemCount != 0) { FinalItem = EditorClass.NameTableItemCount; }
                if (EditorClass.NameTableItemCount == 0)
                {
                    int ItemCount = 0;
                    foreach (var Item in EditorClass.LeftBar.ItemList)
                    {
                        if (Item.ItemType != "Folder")
                        {
                            ItemCount++;
                        }
                    }
                    FinalItem = ItemCount;
                }
                FinalItem = FinalItem - int.Parse(FormulaDoNotModTextBox.Text); //Allows users to NOT mod the final X number of items in the list.
            }
            catch 
            {
                string Error = "An error happened during the first step of auto-mod. " +
                    "\nIn this step, it simply tries to count how many items it's going to mod." +
                    "\nThis error can probably only appear if the editor is not getting it's item names from an actual game file." +
                    "\n" +
                    "\nAnyway, Auto-mod will now cancel. Nothing has been changed.";
                Notification f2 = new(Error);
                f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
                return;
            }
            
            
            for (int i = 0; i < FinalItem; i++) 
            {

                try 
                {
                    //Get Current Value Step
                    double CurrentValue = 0; //Will cause conflicts with negative numbers so im ignoring them for now. (Maybe i can support negative byte sizes 1 and 2 easily though ?)                
                    if (EntryClass.EntryByteSize == "1")
                    {
                        EntryClass.EntryByteDecimal = EditorClass.EditorFile.FileBytes[EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset].ToString("D");
                        CurrentValue = EditorClass.EditorFile.FileBytes[EditorClass.EditorTableStart + (i * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset];
                    }
                    if (EntryClass.EntryByteSize == "2B")
                    {
                        EntryClass.EntryByteDecimal = BitConverter.ToUInt16(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset).ToString("D");
                        CurrentValue = BitConverter.ToUInt16(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (i * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset);
                    }
                    if (EntryClass.EntryByteSize == "4B")
                    {
                        EntryClass.EntryByteDecimal = BitConverter.ToUInt32(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset).ToString("D");
                        CurrentValue = BitConverter.ToUInt32(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (i * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset);
                    }
                    if (EntryClass.EntryByteSize == "2L")
                    {
                        ushort WrongValue = BitConverter.ToUInt16(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset);
                        CurrentValue = (ushort)IPAddress.HostToNetworkOrder((short)WrongValue); // Swaps the endianness
                    }
                    if (EntryClass.EntryByteSize == "4L")
                    {
                        uint value = BitConverter.ToUInt32(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset);
                        byte[] valueBytes = BitConverter.GetBytes(value);    // Swaps the endianness
                        Array.Reverse(valueBytes);                           // Swaps the endianness
                        CurrentValue = BitConverter.ToUInt32(valueBytes, 0); // Swaps the endianness
                    }

                    //set above as IF size 1
                    //make more for IF size 2L 2B 4L 4B
                    double NewValue = 0;

                    if (FormulaComboBox.Text == "Multiply") //Multiply Step
                    {
                        double multiplier = 1;
                        string formulaText = FormulaTextBox.Text.Trim().TrimEnd('%');

                        if (double.TryParse(formulaText, out double percentage)) //the max size of a double is 9 quadrillion, so it should never cause any size limit errors.
                        {
                            multiplier = percentage / 100;
                        }
                        else
                        {
                            return;
                        }

                        double MathResult = CurrentValue * multiplier;
                        NewValue = (double)Math.Round(MathResult);

                    }

                    if (FormulaComboBox.Text == "Add") //Add Step
                    {
                        NewValue = CurrentValue + double.Parse(FormulaTextBox.Text);
                    }

                    if (FormulaComboBox.Text == "Subtract") //Subtract Step
                    {
                        NewValue = CurrentValue - double.Parse(FormulaTextBox.Text);
                    }

                    //MIN Step
                    if (FormulaMinTextBox.Text != "" && FormulaMinTextBox.Text != null) { if (NewValue < double.Parse(FormulaMinTextBox.Text)) { NewValue = double.Parse(FormulaMinTextBox.Text); } }
                    if (NewValue < 0) { NewValue = 0; } //True Min Step

                    //MAX Step
                    if (FormulaMaxTextBox.Text != "" && FormulaMaxTextBox.Text != null) { if (NewValue > double.Parse(FormulaMaxTextBox.Text)) { NewValue = double.Parse(FormulaMaxTextBox.Text); } }
                    if (EntryClass.EntryByteSize == "1" && NewValue > 255) { NewValue = 255; } //True Max Step
                    if (EntryClass.EntryByteSize == "2B" && NewValue > 65535) { NewValue = 65535; }
                    if (EntryClass.EntryByteSize == "2L" && NewValue > 65535) { NewValue = 65535; }
                    if (EntryClass.EntryByteSize == "4B" && NewValue > 4294967295) { NewValue = 4294967295; }
                    if (EntryClass.EntryByteSize == "4L" && NewValue > 4294967295) { NewValue = 4294967295; }





                    //Saving Step
                    string Result = NewValue.ToString();

                    if (EntryClass.EntryByteSize == "1")  // This is saving 1 Byte Size?   // First 1 byte save
                    {
                        Byte.TryParse(Result, out byte value8);
                        { ByteManager.ByteWriter(value8, EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (i * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset); }
                    }
                    if (EntryClass.EntryByteSize == "2L")
                    {
                        UInt16.TryParse(Result, out ushort value16);
                        value16 = (ushort)IPAddress.HostToNetworkOrder((short)value16); // Swap the endianness
                        { ByteManager.ByteWriter(value16, EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (i * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset); } //First 2 byte save


                    }
                    if (EntryClass.EntryByteSize == "4L")
                    {
                        UInt32.TryParse(Result, out uint value32);
                        byte[] valueBytes = BitConverter.GetBytes(value32); // Swap the endianness
                        Array.Reverse(valueBytes); // Swap the endianness
                        value32 = BitConverter.ToUInt32(valueBytes, 0); // Swap the endianness
                        { ByteManager.ByteWriter(value32, EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (i * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset); } //First 4 byte save

                    }
                    if (EntryClass.EntryByteSize == "2B")
                    {
                        UInt16.TryParse(Result, out ushort value16);
                        { ByteManager.ByteWriter(value16, EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (i * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset); } //First 2 byte save
                    }
                    if (EntryClass.EntryByteSize == "4B")
                    {
                        UInt32.TryParse(Result, out uint value32);
                        { ByteManager.ByteWriter(value32, EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (i * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset); } //First 4 byte save
                    }

                    //FormulaMinTextBox
                    //FormulaMaxTextBox
                }
                catch 
                {
                    string Error = "An error happened during the actual modifying of data in memory." +
                        "\nThis means some of the items have been changed, and others have not." +
                        "\nNothing has been saved to actual files on the computer, so don't worry." +
                        "\n" +
                        "\nHowever, this is a very serious error. It is strongly recommended you close the program WITHOUT saving your game files." +
                        "\n" +
                        "\nI chose not to automatically force crash the program, to give you a chance to save some non-game file related things first. " +
                        "before you close everything, in the workshop menu you may save your documents, common events, and editors, but absolutely do not save your game files. " +
                        "If you do, you will save them with only some items being changed, but not all of them." +
                        "\n";
                    Notification f2 = new(Error);
                    f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    f2.ShowDialog();
                    return;
                }
                

            }







        }

        private void ShowEditorGraphicScreen(object sender, RoutedEventArgs e)
        {
            HIDEALL();
            EditorGraphicsWindow.Visibility= Visibility.Visible;
        }
                        

        private void MenuWorkshopToolsList(object sender, RoutedEventArgs e)
        {
            HIDEALL();
            GridWorkshopToolsList.Visibility = Visibility.Visible;
        }

        private void ButtonSaveWorkshopTools(object sender, RoutedEventArgs e)
        {
            GridWorkshopToolsList.Visibility = Visibility.Collapsed;
        }
    }

    public class NumberCount
    {
        public int Number { get; set; }
        public int Count { get; set; }
        public List<int> RowIndices { get; set; } = new List<int>();
        public string RowIndicesAsString
        {
            get
            {
                return string.Join(", ", RowIndices);
            }
        }
    }

}

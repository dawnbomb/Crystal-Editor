using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Drawing;
using System.Formats.Asn1;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
//using System.Windows.Forms;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using System.Xml.Schema;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Crystal_Editor
{
    class EditorCreate
    {

        public void CreateEditor(Workshop TheWorkshop, Database Database, Editor EditorClass)
        {
            //This triggers when the user makes a new editor in SetupNewEditor (not a loop)
            //or when the workshop is first launched via LoadEditorModeAuto (A loop) -> LoadTheDatabase.
            //Either way, this method is what actually creates an editor.
            //This method pulls information from the Database, and builds an editor based on that.
            //The database is loaded from files on system, or for a new editor, information the user input during editor creation.

                        
            string ThisEditorName = TheWorkshop.EditorName; //Later: Purge this from program. Used for Button name, and temp for create another page in top bar.
            string IsFirstPage = "Yes"; //Used in a IF the first time createpage is used. Yes i know it could just be a bool, i like strings.            

            CreateEditor(EditorClass, TheWorkshop, Database);  //Creates the main DockPanel of the editor. All Editor GUI stuff goes inside this Dockpanel.
            CreateButton(ThisEditorName, TheWorkshop, Database, EditorClass); //Creates the button a user needs to click to make this editor appear.
            CreateLeftBar(TheWorkshop, Database, EditorClass); //Creates the LeftBar of the editor, the part that has the item collection.            
            CreateTopBar(EditorClass, TheWorkshop, Database); //Makes a top bar. Doesn't do anything not, but more features for it are planned.

            //This nested loop created the entire core part of the editor.
            foreach (Page PageClass in EditorClass.PageList)
            {
                CreatePage(EditorClass, PageClass, ref IsFirstPage, TheWorkshop, Database); 

                foreach (Row RowClass in PageClass.RowList)
                {
                    CreateRow(PageClass, RowClass, TheWorkshop, Database, -1);

                    foreach (Column ColumnClass in RowClass.ColumnList)
                    {
                        CreateColumn(RowClass, ColumnClass, TheWorkshop, Database, -1);

                        if (ColumnClass.EntryList != null) 
                        {
                            foreach (Entry EntryClass in ColumnClass.EntryList)
                            {
                                CreateEntry(EditorClass, PageClass, RowClass, ColumnClass, EntryClass, TheWorkshop, Database);
                            }
                            TheWorkshop.LabelWidth(ColumnClass);
                        }
                            
                    }
                }

                              
            }

            //Finally, we select the first option of every tree. A few things happen during this.
            //A lot of Various information is updated when a item becomes the selected item, so it gets it's own method. 
            EntryManager EManager = new();            
            EManager.EntryBecomeActive(EditorClass.PageList[0].RowList[0].ColumnList[0].EntryList[0]);

            
        }



        public void CreateEditor(Editor EditorClass, Workshop TheWorkshop, Database Database)
        {
            //This grid is the very back of the entire editor. Everything else is a child of this grid, and those things are all GUI.
            DockPanel EditorDockPanel = new();
            EditorDockPanel.Background = Brushes.Purple; //If the user ever SEES this grid, it's a bug. So it gets a obvious ugly color.            
            TheWorkshop.MidGrid.Children.Add(EditorDockPanel);
            EditorClass.EditorDockPanel = new();
            EditorClass.EditorDockPanel = EditorDockPanel;

                


            DockPanel LeftBarDockPanel = new();
            DockPanel.SetDock(LeftBarDockPanel, Dock.Left);
            LeftBarDockPanel.Width = 250;
            //LeftBarDockPanel.Style = (Style)Application.Current.Resources["StyleFrontGrid"];
            //LeftBarDockPanel.VerticalAlignment = VerticalAlignment.Stretch; //Top Bottom            
            //LeftBarDockPanel.HorizontalAlignment = HorizontalAlignment.Left;
            EditorDockPanel.Children.Add(LeftBarDockPanel);
            //EditorClass.EditorDockPanel.Children.Add(LeftBarDockPanel);
            EditorClass.LeftBar.LeftBarDockPanel = LeftBarDockPanel;


            DockPanel RightDock = new DockPanel();
            RightDock.Background = Brushes.Yellow;
            DockPanel.SetDock(RightDock, Dock.Left);
            EditorDockPanel.Children.Add(RightDock);
            EditorClass.EditorRightDockPanel = RightDock;


            DockPanel TopDockPanel = new();
            TopDockPanel.Background = new SolidColorBrush(Colors.Blue);
            DockPanel.SetDock(TopDockPanel, Dock.Top);
            //TopPanel.Height = 120;
            //DockPanel.SetDock(TopGrid, Dock.Top);
            TopDockPanel.VerticalAlignment = VerticalAlignment.Top; //Top Bottom
            //TopDockPanel.HorizontalAlignment = HorizontalAlignment.Stretch; //Left Right
            //TopDockPanel.Margin = new Thickness(250, 0.1, 0, 0); // Left Top Right Bottom 
            //TopGrid.VerticalAlignment= VerticalAlignment.Top;
            //TopGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            RightDock.Children.Add(TopDockPanel);
            EditorClass.TopBar = new();
            EditorClass.TopBar.TopPanel = TopDockPanel;


            DockPanel PageBar = new();
            PageBar.Background = new SolidColorBrush(Colors.DarkBlue);
            DockPanel.SetDock(PageBar, Dock.Bottom);
            //PageBar.VerticalAlignment = VerticalAlignment.Top; //Top Bottom
            EditorClass.EditorRightDockPanel.Children.Add(PageBar);
            //EditorClass.EditorRightDockPanel.Children.Add(PageBar);
            PageBar.Height = 30;
            EditorClass.TopBar.PageBar = PageBar;
            PageBar.Visibility = Visibility.Collapsed;

            Button ButtonNewPage = new Button();
            ButtonNewPage.Height = 30;
            ButtonNewPage.Width = 150;
            ButtonNewPage.Content = "The Banished Pile";
            DockPanel.SetDock(ButtonNewPage, Dock.Right);
            ButtonNewPage.Click += delegate
            {


            };
            EditorClass.TopBar.PageBar.Children.Add(ButtonNewPage);



            ScrollViewer ScrollViewer = new(); //The basic frame of a "Page". Makes sure pages can scroll down.
            //ScrollViewer.Margin = new Thickness(0, 0, 0, 30); // Left Top Right Bottom 
            ScrollViewer.Background = Brushes.Red;
            DockPanel.SetDock(ScrollViewer, Dock.Top);
            EditorClass.ScrollViewer = ScrollViewer;
            EditorClass.EditorRightDockPanel.Children.Add(ScrollViewer);
            ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            
            DockPanel ScrollPanel = new();
            ScrollPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            ScrollPanel.VerticalAlignment = VerticalAlignment.Stretch;
            ScrollViewer.Content = ScrollPanel;
            EditorClass.ScrollPanel = ScrollPanel;


            foreach (ExtraTable ExtraTable in EditorClass.ExtraTableList)
            {
                TextBox ExtraTextBox = new();
                ExtraTextBox.AcceptsReturn = true;
                ExtraTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                ExtraTextBox.TextWrapping = TextWrapping.NoWrap;
                ExtraTextBox.FontSize = 16;
                DockPanel.SetDock(ExtraTextBox, Dock.Bottom);
                EditorClass.TopBar.TopPanel.Children.Add(ExtraTextBox);
                ExtraTextBox.Height = 45;
                ExtraTable.ExtraTableTextBox = ExtraTextBox;
                ExtraTable.ExtraTableEncodeIsEnabled = true;

                ExtraTextBox.PreviewKeyDown += (sender, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        e.Handled = true;

                        if (ExtraTable.ExtraTableTextSize == ExtraTable.ExtraTableTextBox.Text.Length) { return; }

                        TextBox textBox = sender as TextBox;

                        int caretIndex = textBox.CaretIndex;
                        textBox.Text = textBox.Text.Insert(caretIndex, "\n");
                        textBox.CaretIndex = caretIndex + 1;
                    }
                };

                ExtraTextBox.PreviewTextInput += (sender, e) =>
                {
                    string NewText = ExtraTable.ExtraTableTextBox.Text + e.Text;

                    Encoding encoding;
                    if (ExtraTable.ExtraTableCharacterSet == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                    else if (ExtraTable.ExtraTableCharacterSet == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                    else { return; }
                    int NewByteSize = encoding.GetByteCount(NewText);

                    if (NewByteSize > ExtraTable.ExtraTableTextSize)
                    {
                        e.Handled = true;  // Mark the event as handled so the input is ignored
                    }
                    //else { TheWorkshop.DebugBox2.Text = "Current NameBox Text Length\n" + (NameBox.Text.Length + 1).ToString(); }

                };

                ExtraTextBox.TextChanged += (sender, e) =>
                {
                    TreeViewItem selectedItem = EditorClass.LeftBar.TreeView.SelectedItem as TreeViewItem;
                    ItemInfo ItemInfo = selectedItem.Tag as ItemInfo;
                    if (selectedItem == null || selectedItem.Tag == null || ItemInfo.ItemType == "Folder" || ExtraTable.ExtraTableEncodeIsEnabled == false) 
                    {
                        return;
                    }

                    CharacterSetManager CharacterSetManager = new();
                    CharacterSetManager.EncodeExtra(TheWorkshop, EditorClass, ExtraTable);

                };


                
            }



            TextBox ItemNoteBox = new();
            ItemNoteBox.AcceptsReturn = true;
            ItemNoteBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            ItemNoteBox.TextWrapping = TextWrapping.Wrap;
            ItemNoteBox.FontSize = 16;
            DockPanel.SetDock(ItemNoteBox, Dock.Left);
            EditorClass.TopBar.TopPanel.Children.Add(ItemNoteBox);
            ItemNoteBox.Height = 90;
            ItemNoteBox.Width = TopDockPanel.ActualWidth * 0.5;
            EditorClass.TopBar.ItemNoteBox = ItemNoteBox;

            

            ItemNoteBox.TextChanged += (sender, e) =>
            {
                // Get the selected TreeViewItem
                TreeViewItem selectedItem = EditorClass.LeftBar.TreeView.SelectedItem as TreeViewItem;
                if (selectedItem != null)
                {
                    selectedItem.ToolTip = ItemNoteBox.Text;
                    // Get the selected ItemInfo class
                    ItemInfo selectedInfo = selectedItem.Tag as ItemInfo;
                    if (selectedInfo != null)
                    {
                        // Update the Name property with the text from the TextBox
                        selectedInfo.ItemTooltip = ItemNoteBox.Text;
                    }
                }
            };

            TextBox EntryNoteBox = new();
            EntryNoteBox.AcceptsReturn = true;
            EntryNoteBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            EntryNoteBox.TextWrapping = TextWrapping.Wrap;
            EntryNoteBox.FontSize = 16;
            DockPanel.SetDock(EntryNoteBox, Dock.Right);
            EditorClass.TopBar.TopPanel.Children.Add(EntryNoteBox);
            EntryNoteBox.Height = 90;
            EntryNoteBox.Width = EditorClass.TopBar.TopPanel.ActualWidth * 0.5;
            EditorClass.TopBar.EntryNoteBox = EntryNoteBox;


            EditorClass.TopBar.TopPanel.SizeChanged += (sender, e) =>
            {
                ItemNoteBox.Width = EditorClass.TopBar.TopPanel.ActualWidth * 0.5;
                EntryNoteBox.Width = EditorClass.TopBar.TopPanel.ActualWidth * 0.5;
            };
            EntryNoteBox.TextChanged += (sender, e) =>
            {
                EditorClass.SelectedEntry.EntryTooltip = EntryNoteBox.Text;
                                
            };


            


        }

        private void CreateButton(string ThisEditorName, Workshop TheWorkshop, Database Database, Editor EditorClass)
        {
            //TreeViewItem EditorTreeItem = new();
            //EditorTreeItem.Header = ThisEditorName;
            //TheWorkshop.EditorsTree.Items.Add(EditorTreeItem);
            //EditorTreeItem.Tag = EditorClass;


            //EditorClass.EditorTreeViewitem = EditorTreeItem;

            Image EditorImage = new();


            string checkFileExist = TheWorkshop.ExePath + "\\Graphics\\" + EditorClass.EditorGraphic;
            if (System.IO.File.Exists(checkFileExist) && EditorClass.EditorGraphic != "" && EditorClass.EditorGraphic != null)
            {
                EditorImage.Source = new BitmapImage(new Uri(string.Format(TheWorkshop.ExePath + "\\Graphics\\" + EditorClass.EditorGraphic)));

            }
            else
            {
                //Image.Source = new BitmapImage(new Uri(TheWorkshop.ExePath + "\\Graphics\\Other\\Question Mark.png", UriKind.Absolute));
                EditorImage.Source = new BitmapImage(new Uri(string.Format(TheWorkshop.ExePath + "\\Graphics\\Other\\Question Mark.png")));

                //ImageLibraryBanner.Source = new BitmapImage(new Uri(string.Format(TheWorkshop.ExePath + "\\Other\\Images\\LibraryBannerArt.png")));

            }





            DockPanel EditorDock = new();
            EditorDock.Tag = EditorClass;
            EditorDock.Margin = new Thickness(0, 0, 5, 0);
            EditorDock.Background = (Brush)(new BrushConverter().ConvertFrom("#191919"));
            EditorClass.EditorBarDockPanel = EditorDock;
            EditorDock.AllowDrop = true;

            EditorDock.MouseMove += (sender, e) =>
            {

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    var theDockPanel = (DockPanel)sender;

                    if (Keyboard.IsKeyUp(Key.LeftShift)) // Single Entry capture
                    {
                        var currentPosition = e.GetPosition(theDockPanel);
                        var minimumDistance = (SystemParameters.MinimumHorizontalDragDistance + SystemParameters.MinimumVerticalDragDistance) / 2;

                        if (Math.Sqrt(Math.Pow(currentPosition.X, 2) + Math.Pow(currentPosition.Y, 2)) >= minimumDistance)
                        {
                            var data = new DataObject("MoveEditor", theDockPanel);
                            DragDrop.DoDragDrop(theDockPanel, data, DragDropEffects.Move);
                        }
                        theDockPanel.ReleaseMouseCapture();
                    }

                }
            };


            EditorDock.Drop += (sender, e) =>
            {

                if (e.Data.GetDataPresent("MoveEditor") && Keyboard.IsKeyUp(Key.LeftShift)) //Single Entry Drop
                {
                    DockPanel DropInput = (DockPanel)e.Data.GetData("MoveEditor");

                    if (DropInput != EditorDock)
                    {
                        TheWorkshop.EditorBar.Children.Remove(DropInput);
                        int GoTo = (TheWorkshop.EditorBar.Children.IndexOf(EditorDock)) + 1;
                        TheWorkshop.EditorBar.Children.Insert(GoTo, DropInput);

                    }
                }
            };
            



            Label Hi = new();
            Hi.VerticalAlignment= VerticalAlignment.Bottom;
            Hi.Content = ThisEditorName;
            EditorClass.EditorNameLabel = Hi;



            EditorDock.MouseLeftButtonDown += ClickEditor;
            void ClickEditor(object sender, MouseButtonEventArgs e)
            {
                Editor TheEditorClass = EditorClass;

                if (TheEditorClass == null)
                {

                    return;
                }

                TheWorkshop.HIDEALL();
                TheEditorClass.EditorDockPanel.Visibility = Visibility.Visible;

                TheWorkshop.PropertiesHide();
                TheWorkshop.EditorProperties.Visibility = Visibility.Visible;

                TheWorkshop.PropertiesTextboxEditorName.Text = Hi.Content.ToString();
                TheWorkshop.PropertiesEditorTableStart.Text = TheEditorClass.EditorTableStart.ToString();
                TheWorkshop.PropertiesEditorTableWidth.Text = TheEditorClass.EditorTableRowSize.ToString();



                TheWorkshop.PropertiesEditorNameTableTextSize.Text = TheEditorClass.NameTableTextSize.ToString();
                TheWorkshop.PropertiesEditorNameTableStartByte.Text = TheEditorClass.NameTableStart.ToString();
                TheWorkshop.PropertiesEditorNameTableRowSize.Text = TheEditorClass.NameTableRowSize.ToString();
                
                TheWorkshop.PropertiesEditorNameCount.Text = (TheEditorClass.NameTableItemCount - 1).ToString();
                foreach (var item in TheWorkshop.PropertiesEditorNameTableCharacterSetDropdown.Items)
                {
                    if (item is ComboBoxItem comboBoxItem && comboBoxItem.Content.ToString() == TheEditorClass.NameTableCharacterSet)
                    {
                        TheWorkshop.PropertiesEditorNameTableCharacterSetDropdown.SelectedItem = comboBoxItem;
                        break;
                    }
                }


                TheWorkshop.PropertiesEditorReadGameDataFrom.Text = TheWorkshop.InputDirectory + TheEditorClass.EditorFile.FilePath;

                TheWorkshop.EditorClass = TheEditorClass; //this used to be EditorClass = TheEditorClass; and i changed it because i might have meant this, but also maybe not and i made a new bug?
                TheWorkshop.DebugBox.Text = "Name Table Text Size Limit\n" + TheEditorClass.NameTableTextSize.ToString();



                if (TheEditorClass.NameTableFilePath == null || TheEditorClass.NameTableFilePath == "") //IE manual names
                {

                    TheWorkshop.PropertiesEditorNameTableTextSize.IsEnabled = false;
                    TheWorkshop.PropertiesEditorNameTableStartByte.IsEnabled = false;
                    TheWorkshop.PropertiesEditorNameTableRowSize.IsEnabled = false;
                    TheWorkshop.PropertiesEditorNameCount.IsEnabled = false;
                    TheWorkshop.PropertiesEditorNameTableCharacterSetDropdown.IsEnabled = false;
                }
                else if (TheEditorClass.EditorFilePath != TheEditorClass.NameTableFilePath) //A name file
                {

                    TheWorkshop.PropertiesEditorNameTableTextSize.IsEnabled = true;
                    TheWorkshop.PropertiesEditorNameTableStartByte.IsEnabled = true;
                    TheWorkshop.ButtonChangeNameTableRowSize.IsEnabled = true;
                    TheWorkshop.PropertiesEditorNameCount.IsEnabled = true;
                    TheWorkshop.PropertiesEditorNameTableCharacterSetDropdown.IsEnabled = true;
                }
                else if (TheEditorClass.EditorFilePath == TheEditorClass.NameTableFilePath) //Names are in the data file
                {

                    TheWorkshop.PropertiesEditorNameTableTextSize.IsEnabled = true;
                    TheWorkshop.PropertiesEditorNameTableStartByte.IsEnabled = true;
                    TheWorkshop.ButtonChangeNameTableRowSize.IsEnabled = true;
                    TheWorkshop.PropertiesEditorNameCount.IsEnabled = true;
                    TheWorkshop.PropertiesEditorNameTableCharacterSetDropdown.IsEnabled = true;
                }

                if (Properties.Settings.Default.DeveloperMode == "Developer")
                {
                    //Window parentWindow = Window.GetWindow(EditorButton);
                    //TabControl tabControl = parentWindow.FindName("TabSet") as TabControl;
                    foreach (TabItem tabItem in TheWorkshop.TabSet.Items)
                    {
                        if (tabItem.Header != null && tabItem.Header.ToString() == "Properties")
                        {
                            tabItem.IsSelected = true;
                            break;
                        }
                    }
                }


                
            }





            DockPanel.SetDock(EditorDock, Dock.Left);  
            TheWorkshop.EditorBar.Children.Add(EditorDock);

            DockPanel.SetDock(Hi, Dock.Bottom);
            EditorDock.Children.Add(Hi);
            DockPanel.SetDock(EditorImage, Dock.Top);
            EditorDock.Children.Add(EditorImage);

            //
        }



        public void CreateLeftBar(Workshop TheWorkshop, Database Database, Editor EditorClass)
        {
            bool FirstTime = true;
            ByteManager ByteManager = new();
            

            //The left bar contains a search bar, and the item collection. 
            


            TreeView TreeView = new();

            TextBox SearchBar = new TextBox();
            SearchBar.VerticalAlignment = VerticalAlignment.Top; //Top Bottom   
            DockPanel.SetDock(SearchBar, Dock.Top);
            EditorClass.LeftBar.LeftBarDockPanel.Children.Add(SearchBar);
            EditorClass.LeftBar.SearchBar = SearchBar;

            //Currently the search bar works, but can only search for items that are not in a folder, or are in a open folder.
            //I put off adding closed folder search to later as i work on other parts of the program, but it will be important to add later.
            SearchBar.TextChanged += (sender, e) =>
            {
                string searchText = SearchBar.Text;

                if (string.IsNullOrEmpty(searchText))
                {
                    TreeView.Items.Filter = null;
                }
                else
                {
                    TreeView.Items.Filter = (item) =>
                    {
                        if (((TreeViewItem)item).Tag is ItemInfo itemInfo)
                        {
                            // Search for the search text within the item name
                            if (itemInfo.ItemName.Contains(searchText))
                            {
                                return true;
                            }

                            // Check the children of the current TreeViewItem
                            foreach (var childItem in ((TreeViewItem)item).Items)
                            {
                                if (childItem is TreeViewItem childTreeViewItem && childTreeViewItem.Tag is ItemInfo childInfo)
                                {
                                    if (childInfo.ItemName.Contains(searchText))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }

                        return false;
                    };
                }
            };

            Button SaveItem = new Button();
            DockPanel.SetDock(SaveItem, Dock.Bottom);
            SaveItem.Height = 30;
            SaveItem.Content = "Save Item";
            EditorClass.LeftBar.LeftBarDockPanel.Children.Add(SaveItem);

            TextBox NameBoxExtra = new();
            DockPanel.SetDock(NameBoxExtra, Dock.Bottom);
            EditorClass.LeftBar.LeftBarDockPanel.Children.Add(NameBoxExtra);
            EditorClass.LeftBar.LeftBarNameBoxExtra = NameBoxExtra;

            TextBox NameBox = new();
            DockPanel.SetDock(NameBox, Dock.Bottom);
            EditorClass.LeftBar.LeftBarDockPanel.Children.Add(NameBox);
            EditorClass.LeftBar.LeftBarNameBox = NameBox;

            SaveItem.Click += (sender, e) =>
            {
                
                TreeViewItem selectedItem = TreeView.SelectedItem as TreeViewItem;
                if (selectedItem != null && selectedItem.Tag != null)
                {

                    ItemInfo ItemInfo = selectedItem.Tag as ItemInfo;

                    ItemInfo.ItemName = NameBox.Text;
                    ItemInfo.ItemNote = NameBoxExtra.Text;

                    TheWorkshop.ItemNameBuilder(selectedItem);
                    //PropertiesItemTextboxTooltip

                    if (ItemInfo.ItemType != "Folder" && EditorClass.NameTableFilePath != "" && EditorClass.NameTableFilePath != null) //The NameTableFilePath check prevents crashing when saving a note when the editor gets names from user instead of from file.
                    {
                        //CharacterSetAscii CharacterSetAscii = new();
                        //CharacterSetAscii.EncodeAscii(PropertiesItemTextboxName.Text, EditorClass);

                        CharacterSetManager CharacterSetManager = new();
                        CharacterSetManager.Encode(TheWorkshop, EditorClass, "Item", ItemInfo);
                    }

                }
            };

            NameBox.PreviewKeyDown += (sender, e) =>
            {
                if (e.Key == Key.Space || (e.Key >= Key.A && e.Key <= Key.Z) || (e.Key >= Key.D0 && e.Key <= Key.D9))
                {
                    string NewText;

                    if (e.Key == Key.Space)
                    {
                        NewText = NameBox.Text + " ";
                    }
                    else if (e.Key >= Key.A && e.Key <= Key.Z)
                    {
                        NewText = NameBox.Text + (char)('A' + (e.Key - Key.A));
                    }
                    else // e.Key >= Key.D0 && e.Key <= Key.D9
                    {
                        NewText = NameBox.Text + (char)('0' + (e.Key - Key.D0));
                    }

                    Encoding encoding;
                    if (EditorClass.NameTableCharacterSet == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                    else if (EditorClass.NameTableCharacterSet == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                    else { return; }
                    int NewByteSize = encoding.GetByteCount(NewText);

                    if (NewByteSize > EditorClass.NameTableTextSize)
                    {
                        e.Handled = true;  // Mark the event as handled so the input is ignored
                    }
                    else { TheWorkshop.DebugBox2.Text = "Current NameBox Text Length\n" + (NameBox.Text.Length + 1).ToString(); }
                }
            };
            //NameBox.PreviewTextInput += (sender, e) =>
            //{   
            //    string NewText = NameBox.Text + e.Text;

            //    Encoding encoding;                
            //    if (EditorClass.NameTableCharacterSet == "ASCII+ANSI") { encoding = Encoding.ASCII; }
            //    else if (EditorClass.NameTableCharacterSet == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
            //    else { return; }
            //    int NewByteSize = encoding.GetByteCount(NewText);

            //    if (NewByteSize > EditorClass.NameTableTextSize)
            //    {
            //        e.Handled = true;  // Mark the event as handled so the input is ignored
            //    }
            //    else { TheWorkshop.DebugBox2.Text = "Current NameBox Text Length\n" + (NameBox.Text.Length + 1).ToString(); }

            //};



            DockPanel.SetDock(TreeView, Dock.Top);
            TreeView.VerticalAlignment = VerticalAlignment.Top; //Top Bottom            

            //When the selected item is changed, we save the current item entry info, and load the new items info.
            TreeView.SelectedItemChanged += (sender, e) =>
            {
                //I disable selection effects sometimes when modifying items in the collection while it is open.
                if (TheWorkshop.TreeViewSelectionEnabled)
                {
                    var selectedItem = e.NewValue as TreeViewItem;
                    ItemInfo data = selectedItem.Tag as ItemInfo;
                    EntryManager EManager = new();
                                        

                    if (selectedItem.Items.Count == 0) //This might cause bugs later with 0 child folders.
                    {
                        EditorClass.TableRowIndex = data.ItemIndex;

                        foreach (var page in EditorClass.PageList)
                        {
                            foreach (var row in page.RowList)
                            {
                                foreach (var column in row.ColumnList)
                                {
                                    foreach (var entry in column.EntryList)
                                    {
                                        if (entry.EntrySubType == "NumberBox") 
                                        {
                                            entry.EntryTypeNumberBox.NumberBoxCanSave = false;
                                        }
                                        
                                        //if (FirstTime == false)
                                        //{
                                        //    EManager.SaveEntry(EditorClass, entry);

                                        //}
                                        EManager.LoadEntry(TheWorkshop, EditorClass, entry);
                                        if (FirstTime == false)
                                        {
                                            EManager.UpdateEntryProperties(TheWorkshop, EditorClass);
                                        }

                                        if (entry.EntrySubType == "NumberBox")
                                        {
                                            entry.EntryTypeNumberBox.NumberBoxCanSave = true;
                                        }
                                        

                                    }
                                }
                            }
                        }

                        //TheWorkshop.EntryProperties.Visibility = Visibility.Collapsed;
                        //TheWorkshop.ItemProperties.Visibility = Visibility.Visible;
                        FirstTime = false;
                    }

                    if (TheWorkshop.IsPreviewMode == false) { NameBox.Text = data.ItemName.TrimEnd('\0');  }
                    
                    NameBoxExtra.Text = data.ItemNote;
                                        
                    EditorClass.TopBar.ItemNoteBox.Text = data.ItemTooltip;

                    if (EditorClass.ExtraTableList.Count > 0)
                    {
                        CharacterSetManager CharacterSetManager = new();
                        CharacterSetManager.DecodeExtras(TheWorkshop, EditorClass);
                        //TheWorkshop.ExtraTableText.Text = EditorClass.ExtraTableList[0].
                    }


                    //foreach (ExtraTable ExtraTable in ) 
                    //{
                    //    //Decode, then fill textbox.
                    //}


                } //End of If selection is enabled.



            }; //End of Selected Item Changed








            bool _mousePressedOnItem = false;

            TreeView.PreviewMouseLeftButtonDown += TreeView_PreviewMouseLeftButtonDown;
            TreeView.PreviewMouseMove += TreeViewItemMove;

            void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                var treeView = (TreeView)sender;
                var treeViewItem = GetTreeViewItemAtPoint(treeView, e.GetPosition(treeView));

                if (treeViewItem != null)
                {
                    _mousePressedOnItem = true;
                }
            }

            void TreeViewItemMove(object sender, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyUp(Key.LeftShift) && _mousePressedOnItem)
                {
                    var treeView = (TreeView)sender;
                    var treeViewItem = GetTreeViewItemAtPoint(treeView, e.GetPosition(treeView));

                    if (treeViewItem != null)
                    {
                        var data = new DataObject("MoveTreeViewItem", treeViewItem);
                        DragDrop.DoDragDrop(treeViewItem, data, DragDropEffects.Move);
                    }
                }
                else
                {
                    _mousePressedOnItem = false;
                }
            }

            TreeViewItem GetTreeViewItemAtPoint(ItemsControl control, Point point)
            {
                var hitTestResult = VisualTreeHelper.HitTest(control, point);
                var visualHit = hitTestResult?.VisualHit;

                while (visualHit != null)
                {
                    if (visualHit is TreeViewItem treeViewItem)
                    {
                        return treeViewItem;
                    }

                    visualHit = VisualTreeHelper.GetParent(visualHit);
                }

                return null;
            }

                        


            //This part adds the items to the collection.
            //I have to use a TextBlockItem because only they support colored text :(
            //I attach the item's class directly to the tree item's tag, so it can always be accessed.
            //I need to make it so notes appear left side, and other note changes.
            foreach (var ItemInfo in EditorClass.LeftBar.ItemList)
            {
                TreeViewItem TreeItem = new();
                TreeItem.Tag = ItemInfo;
                //This literally just sets the tree item header, but theres actually a lot of item customiation so
                //it gets it's own method in workshop.cs so the user can customize it then re-run the method.
                TheWorkshop.ItemNameBuilder(TreeItem);






                ContextMenu contextMenu = new ContextMenu();

                if (ItemInfo.ItemType != "Folder") 
                {
                    MenuItem MenuItemCreateFolder = new MenuItem();
                    MenuItemCreateFolder.Header = "Create Folder (If not in folder)";
                    MenuItemCreateFolder.Click += (sender, e) => TheWorkshop.CreateFolder(TreeView, TreeItem);
                    contextMenu.Items.Add(MenuItemCreateFolder);
                }
                else if (ItemInfo.ItemType == "Folder") 
                {
                    MenuItem MenuItemDeleteFolder = new MenuItem();
                    MenuItemDeleteFolder.Header = "Delete Folder (If Empty)";
                    MenuItemDeleteFolder.Click += (sender, e) => TheWorkshop.DeleteFolder(TreeView, TreeItem);
                    contextMenu.Items.Add(MenuItemDeleteFolder);
                }
                              


                TreeItem.ContextMenu = contextMenu;
                






                if (ItemInfo.ItemType == "Child")
                {
                    TreeViewItem finalItem = (TreeViewItem)TreeView.Items.GetItemAt(TreeView.Items.Count - 1);
                    finalItem.Items.Add(TreeItem);
                    //TreeView.Items.Add(TreeItem);
                }
                else 
                {
                    TreeView.Items.Add(TreeItem);
                }


                SetChildrenHitTestVisible(TreeItem, false);

                void SetChildrenHitTestVisible(TreeViewItem treeViewItem, bool hitTestVisible)
                {
                    foreach (var child in treeViewItem.Items)
                    {
                        if (child is TreeViewItem childTreeViewItem)
                        {
                            childTreeViewItem.IsHitTestVisible = hitTestVisible;
                            SetChildrenHitTestVisible(childTreeViewItem, hitTestVisible);
                        }
                    }
                }
                                
                
                TreeItem.AllowDrop = true;
                TreeItem.Drop += ItemDrop;
                void ItemDrop(object sender, DragEventArgs e) 
                {     

                    TheWorkshop.TreeViewSelectionEnabled = false;
                    //I am not reordering the item list. This strikes me as problematic, and i will ignore it and hope nothing bad happens. :^)
                    if (e.Data.GetDataPresent("MoveTreeViewItem") && Keyboard.IsKeyUp(Key.LeftShift)) //Single Entry Drop
                    {
                        TreeViewItem InputItem = (TreeViewItem)e.Data.GetData("MoveTreeViewItem");

                        if (InputItem != TreeItem)
                        {
                            ItemInfo InputItemInfo = InputItem.Tag as ItemInfo;
                            ItemInfo TreeItemInfo = InputItem.Tag as ItemInfo;
                            System.Windows.Controls.TreeView parentTreeView = EditorClass.LeftBar.TreeView;

                            if (InputItemInfo.ItemType == "Child") 
                            {//This double IF if because moving a item to a item in a folder, triggers drop twice, once for item and once for folder.
                                if (ItemInfo.ItemType == "Folder")
                                {
                                    TheWorkshop.TreeViewSelectionEnabled = true;
                                    return;
                                }
                            }
                            

                            
                            if (InputItemInfo.ItemType != "Child" && ItemInfo.ItemType != "Child")
                            {                            
                                parentTreeView.Items.Remove(InputItem);
                                int ToIndex = parentTreeView.ItemContainerGenerator.IndexFromContainer(TreeItem) + 1;
                                parentTreeView.Items.Insert(ToIndex, InputItem);
                                InputItem.IsSelected = true;                            
                            }
                            else if (InputItemInfo.ItemType == "Child" && ItemInfo.ItemType != "Child")
                            {
                                TreeViewItem ParentFolderItem = (TreeViewItem)InputItem.Parent;
                                ParentFolderItem.Items.Remove(InputItem);
                                
                                int ToIndex = parentTreeView.ItemContainerGenerator.IndexFromContainer(TreeItem) + 1;
                                parentTreeView.Items.Insert(ToIndex, InputItem);
                                InputItem.IsSelected = true;
                                InputItemInfo.ItemType = null;                               


                            }
                            else if (InputItemInfo.ItemType != "Child" && ItemInfo.ItemType == "Child")
                            {                                

                                TreeViewItem ParentFolderItem = (TreeViewItem)TreeItem.Parent;

                                parentTreeView.Items.Remove(InputItem);
                                int ToIndex = ParentFolderItem.ItemContainerGenerator.IndexFromContainer(TreeItem) + 1;
                                ParentFolderItem.Items.Insert(ToIndex, InputItem);
                                InputItem.IsSelected = true;
                                InputItemInfo.ItemType = "Child"; 

                                
                            }
                            else if (InputItemInfo.ItemType == "Child" && ItemInfo.ItemType == "Child")
                            {
                                TreeViewItem ParentFolderItem = (TreeViewItem)InputItem.Parent;
                                ParentFolderItem.Items.Remove(InputItem);
                                int ToIndex = ParentFolderItem.ItemContainerGenerator.IndexFromContainer(TreeItem) + 1;
                                ParentFolderItem.Items.Insert(ToIndex, InputItem);
                                InputItem.IsSelected = true;                                


                            }


                        }

                    }
                    TheWorkshop.TreeViewSelectionEnabled = true;
                    

                    

                }


            }

            EditorClass.LeftBar.LeftBarDockPanel.Children.Add(TreeView);
            EditorClass.LeftBar.TreeView = TreeView;


            

            //This part makes it so every editor auto-selects it's first option.
            //I should clean this up later but it took so long to get working at all i don't care right now.
            TreeViewItem item = null;
            EventHandler statusChangedHandler = null;
            statusChangedHandler = (sender, e) =>
            {
                if (EditorClass.LeftBar.TreeView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    EditorClass.LeftBar.TreeView.ItemContainerGenerator.StatusChanged -= statusChangedHandler;
                    item = EditorClass.LeftBar.TreeView.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                    if (item != null)
                    {
                        item.IsSelected = true;
                    }
                }
            };
            
            if (EditorClass.LeftBar.TreeView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
            {
                EditorClass.LeftBar.TreeView.ItemContainerGenerator.StatusChanged += statusChangedHandler;
            }
            else
            {
                item = EditorClass.LeftBar.TreeView.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                if (item != null)
                {
                    item.IsSelected = true;
                }
            }




            

        }

        

        public void CreateTopBar(Editor EditorClass, Workshop TheWorkshop, Database Database)
        {
            //This part basically doesn't do anything right now.
            //Later on, i have a few features planned for the top bar besides just page tab buttons.

            


            




        }

        
        
        public void CreatePage(Editor EditorClass, Page PageClass, ref string IsFirstPage, Workshop TheWorkshop, Database Database)
        {
            //A page can be lightly seen by the user.
            //It can scroll down, so users can see an infinite numbers of contents / entrys.
            //Otherwise, it does nothing. Diffrent pages exist for sorting / logistical purposes only.
            //Note: Multiple pages are not working right now.

            PageClass.DockPanel = new();
            DockPanel PageGrid = new DockPanel();
            PageClass.DockPanel = PageGrid;
            PageGrid.Style = (Style)Application.Current.Resources["PageStyle"];
            //PageGrid.Margin = new Thickness(250, 120, 0, 0); // Left Top Right Bottom 
            PageGrid.LastChildFill = true;
            PageGrid.VerticalAlignment = VerticalAlignment.Stretch; //Up Down
            PageGrid.HorizontalAlignment = HorizontalAlignment.Stretch; //Left Right
            //PageGrid.MinWidth = EditorClass.ScrollViewer.Width;
            DockPanel.SetDock(PageGrid, Dock.Top);

            //EditorClass.ScrollViewer.Content = PageGrid;
            EditorClass.ScrollPanel.Children.Add(PageGrid);

                      



            Button PageButton = new Button();
            PageButton.Height = 30;
            PageButton.Width = 75;
            PageButton.Content = PageClass.PageName; 
            //PageButton.Margin = new Thickness(EditorClass.PageNumber * 75 - 75, 0, 0, 0); // Left Top Right Bottom 
            PageButton.VerticalAlignment = VerticalAlignment.Bottom; //Up Down
            PageButton.HorizontalAlignment = HorizontalAlignment.Left; //Left Right            
            DockPanel.SetDock(PageButton, Dock.Left);
            //PageButton.Foreground = (Brush)(new BrushConverter().ConvertFrom("#FF2463AE"));






            ContextMenu ContextMenu = new ContextMenu();

            MenuItem MenuItemNewPageRight = new MenuItem();
            MenuItemNewPageRight.Header = "  Create New Page (Right)  ";
            ContextMenu.Items.Add(MenuItemNewPageRight);
            MenuItemNewPageRight.Click += new RoutedEventHandler(CreateNewPageRight);
            void CreateNewPageRight(object sender, RoutedEventArgs e)
            {
                // Call the DeleteColumn method here
                TheWorkshop.NewPageRight(PageClass);
            }

            MenuItem MenuItemNewPageLeft = new MenuItem();
            MenuItemNewPageLeft.Header = "  Create New Page (Left)  ";
            ContextMenu.Items.Add(MenuItemNewPageLeft);
            MenuItemNewPageLeft.Click += new RoutedEventHandler(CreateNewPageLeft);
            void CreateNewPageLeft(object sender, RoutedEventArgs e)
            {
                // Call the DeleteColumn method here
                TheWorkshop.NewPageLeft(PageClass);
            }

            PageButton.ContextMenu = ContextMenu;



            PageButton.Click += delegate //When clicking this button, hide the previous page, and show the selected page.
            {
                EditorClass.PageCurrent.Visibility = Visibility.Collapsed;
                EditorClass.PageCurrent = PageGrid;
                //EditorClass.PageCurrent.Visibility = Visibility.Visible;
                EditorClass.PageCurrent.Visibility = Visibility.Visible;
                PageGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                //PageClass.DockPanel.Width = Width.Stretch;
                TheWorkshop.PageClass = PageClass;

                TheWorkshop.PropertiesHide();
                TheWorkshop.PageProperties.Visibility = Visibility.Visible;

                if (Properties.Settings.Default.DeveloperMode == "Developer")
                {
                    Window parentWindow = Window.GetWindow(PageGrid);
                    TabControl tabControl = parentWindow.FindName("TabSet") as TabControl;
                    foreach (TabItem tabItem in tabControl.Items)
                    {
                        if (tabItem.Header != null && tabItem.Header.ToString() == "Properties")
                        {
                            tabItem.IsSelected = true;
                            break;
                        }
                    }
                }

            };
            //DockPanel.SetDock(PageButton, Dock.Bottom);
            EditorClass.TopBar.PageBar.Children.Add(PageButton);



            if (IsFirstPage != "Yes") 
            {
                PageGrid.Visibility = Visibility.Collapsed;
                //EditorClass.PageCurrent = PageGrid;
            }

            if (IsFirstPage == "Yes")
            {   
                IsFirstPage = "No";
                EditorClass.PageCurrent = PageGrid;
            }
            else if (IsFirstPage == "New") 
            {
                
                PageGrid.Visibility = Visibility.Visible;
            }



            PageButton.Drop += EntryDrop;
            void EntryDrop(object sender, DragEventArgs e)
            {

                if (e.Data.GetDataPresent("MoveEntryClass") && Keyboard.IsKeyUp(Key.LeftShift)) //Single Entry Drop
                {
                    Entry InputEntry = (Entry)e.Data.GetData("MoveEntryClass");

                    //if (InputEntry != EntryClass)
                    //{
                    //}

                    InputEntry.EntryColumn.ColumnGrid.Children.Remove(InputEntry.EntryBorder);
                    int FromIndex = InputEntry.EntryColumn.EntryList.IndexOf(InputEntry);                        
                    InputEntry.EntryColumn.EntryList.RemoveAt(FromIndex);

                    PageClass.RowList[0].ColumnList[0].ColumnGrid.Children.Insert(1, InputEntry.EntryBorder);
                    PageClass.RowList[0].ColumnList[0].EntryList.Insert(0, InputEntry);

                    InputEntry.EntryPage   = PageClass;
                    InputEntry.EntryRow    = PageClass.RowList[0];
                    InputEntry.EntryColumn = PageClass.RowList[0].ColumnList[0];                    
                    
                    

                }
                else if (e.Data.GetDataPresent("MoveEntryClassGroup") && Keyboard.IsKeyDown(Key.LeftShift)) //Group Entry Drop
                {   

                    for (int i = 0; i < TheWorkshop.EntryMoveList.Count; i++)
                    {
                        var InputEntry = TheWorkshop.EntryMoveList[i];

                        InputEntry.EntryColumn.ColumnGrid.Children.Remove(InputEntry.EntryBorder);
                        int FromIndex = InputEntry.EntryColumn.EntryList.IndexOf(InputEntry);
                        InputEntry.EntryColumn.EntryList.RemoveAt(InputEntry.EntryColumn.EntryList.IndexOf(InputEntry));

                        PageClass.RowList[0].ColumnList[0].ColumnGrid.Children.Insert(1 + i, InputEntry.EntryBorder);
                        PageClass.RowList[0].ColumnList[0].EntryList.Insert(0 + i, InputEntry);   

                        InputEntry.EntryPage = PageClass;
                        InputEntry.EntryRow = PageClass.RowList[0];
                        InputEntry.EntryColumn = PageClass.RowList[0].ColumnList[0];

                        
                    }
                }




            }
            PageButton.AllowDrop = true;




        }

              



        public void CreateRow(Page PageClass, Row RowClass, Workshop TheWorkshop, Database Database, int Index)
        {
            //A row contains a number of columns.
            //This is useful for spread-sheet like editors needing to exist.
            //Such as etrian odyssey untold 2 have skills with 20 levels, and many attributes, all assigned per level.
            RowClass.RowPage = PageClass;

            DockPanel RowPanel = new DockPanel();
            RowPanel.Style = (Style)Application.Current.Resources["RowStyle"];
            //RowPanel.Width = 500;
            //RowPanel.Height = 300;
            DockPanel.SetDock(RowPanel, Dock.Top);
            RowPanel.VerticalAlignment = VerticalAlignment.Top; //Top Bottom
            RowPanel.HorizontalAlignment = HorizontalAlignment.Stretch; //Left Right
            
            RowPanel.Margin = new Thickness(12, 10, 12, 10); // Left Top Right Bottom 
            
            
            if (Index == -1) { PageClass.DockPanel.Children.Add(RowPanel); }
            else { RowClass.RowPage.DockPanel.Children.Insert(Index, RowPanel); }
            
            RowClass.RowDockPanel = new();
            RowClass.RowDockPanel = RowPanel;


            DockPanel Header = new();
            DockPanel.SetDock(Header, Dock.Top);
            RowPanel.Children.Add(Header);


            ContextMenu ContextMenu = new ContextMenu();

            MenuItem MenuItemNewRowAbove = new MenuItem();
            MenuItemNewRowAbove.Header = "  Create New Row (Above)  ";
            ContextMenu.Items.Add(MenuItemNewRowAbove);
            MenuItemNewRowAbove.Click += new RoutedEventHandler(CreateNewRowAbove);
            void CreateNewRowAbove(object sender, RoutedEventArgs e)
            {
                // Call the DeleteColumn method here
                TheWorkshop.CreateNewRowAbove(RowClass);
            }

            MenuItem MenuItemNewRowBelow = new MenuItem();
            MenuItemNewRowBelow.Header = "  Create New Row (Below)  ";
            ContextMenu.Items.Add(MenuItemNewRowBelow);
            MenuItemNewRowBelow.Click += new RoutedEventHandler(CreateNewRowBelow);
            void CreateNewRowBelow(object sender, RoutedEventArgs e)
            {
                // Call the DeleteColumn method here
                TheWorkshop.CreateNewRowBelow(RowClass);
            }

            Header.ContextMenu = ContextMenu;



            Label Label = new Label();
            Label.FontSize = 15;
            //Label.Height = 26;
            //Label.Width = 160;
            Label.Content = RowClass.RowName;// "Entry X";    //"Row X";
            DockPanel.SetDock(Label, Dock.Left);
            Label.VerticalAlignment = VerticalAlignment.Top; //Top Bottom
            //Label.HorizontalAlignment = HorizontalAlignment.Left; //Left Right
            Label.Margin = new Thickness(6, 0, 0, 0); // Left Top Right Bottom 
            RowClass.RowLabel = Label;


            //RowProperties
            Label.MouseLeftButtonDown += RowGrid_MouseLeftButtonDown;
            void RowGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                TheWorkshop.PropertiesHide();
                TheWorkshop.RowProperties.Visibility = Visibility.Visible;
                TheWorkshop.PropertiesRowNameBox.Text = RowClass.RowName;
                //TheWorkshop.EntryClass = EntryClass;
                TheWorkshop.PageClass = PageClass;
                TheWorkshop.RowClass = RowClass;

                if (Properties.Settings.Default.DeveloperMode == "Developer")
                {
                    Window parentWindow = Window.GetWindow(RowPanel);
                    TabControl tabControl = parentWindow.FindName("TabSet") as TabControl;
                    foreach (TabItem tabItem in tabControl.Items)
                    {
                        if (tabItem.Header != null && tabItem.Header.ToString() == "Properties")
                        {
                            tabItem.IsSelected = true;
                            break;
                        }
                    }
                }
            }
            Header.Children.Add(Label);
            

            Button Button = new Button();
            Button.Height = 26;
            Button.Width = 100;
            Button.Content = "Hide Row";
            DockPanel.SetDock(Button, Dock.Right);
            //Button.VerticalAlignment = VerticalAlignment.Top; //Top Bottom
            Button.HorizontalAlignment = HorizontalAlignment.Right;
            Button.Click += delegate //When clicking this button, hide the previous page, and show the selected page.
            {
                bool Hide = true;
                if (Button.Content == "Hide Row") { Hide = true; }
                if (Button.Content == "Show Row") { Hide = false; }

                if (Hide == true)
                {
                    foreach (Column column in RowClass.ColumnList)
                    {
                        column.ColumnGrid.Visibility = Visibility.Collapsed;
                    }
                    Button.Content = "Show Row";
                }

                if (Hide == false)
                {
                    foreach (Column column in RowClass.ColumnList)
                    {
                        column.ColumnGrid.Visibility = Visibility.Visible;
                    }
                    Button.Content = "Hide Row";
                }


            };
            Header.Children.Add(Button);

            
        }

        

        public void CreateColumn(Row RowClass, Column ColumnClass, Workshop TheWorkshop, Database Database, int Index)
        {
            DockPanel ColumnGrid = new DockPanel();
            ColumnGrid.Style = (Style)Application.Current.Resources["ColumnStyle"];
            //ColumnGrid.Width = 400;
            //ColumnGrid.Height = 200;
            ColumnGrid.VerticalAlignment = VerticalAlignment.Top; //Top Bottom
            ColumnGrid.HorizontalAlignment = HorizontalAlignment.Left; //Left Right
            DockPanel.SetDock(ColumnGrid, Dock.Left);
            ColumnGrid.Margin = new Thickness(5, 0, 0, 5); // Left Top Right Bottom 

            ColumnClass.ColumnRow = RowClass;

            if (Index == -1) { RowClass.RowDockPanel.Children.Add(ColumnGrid); }
            else { ColumnClass.ColumnRow.RowDockPanel.Children.Insert(Index + 1, ColumnGrid);  }
            //PageClass.Grid.Children.Add(ButtonAddRow);
            ColumnClass.ColumnGrid = new();
            ColumnClass.ColumnGrid = ColumnGrid;


            DockPanel Header = new();
            Header.Style = (Style)Application.Current.Resources["ColumnStyle"];
            DockPanel.SetDock(Header, Dock.Top);
            ColumnGrid.Children.Add(Header);




            ContextMenu ContextMenu = new ContextMenu();

            MenuItem MenuItemNewColumnLeft = new MenuItem();
            MenuItemNewColumnLeft.Header = "  Create New Column  (Left)  ";
            ContextMenu.Items.Add(MenuItemNewColumnLeft);
            MenuItemNewColumnLeft.Click += new RoutedEventHandler(NewColumnLeft);
            void NewColumnLeft(object sender, RoutedEventArgs e)
            {
                // Call the DeleteColumn method here
                TheWorkshop.CreateNewColumnLeft(ColumnClass);
            }

            MenuItem MenuItemNewColumnRight = new MenuItem();
            MenuItemNewColumnRight.Header = "  Create New Column  (Right)  ";
            ContextMenu.Items.Add(MenuItemNewColumnRight);
            MenuItemNewColumnRight.Click += new RoutedEventHandler(NewColumnRight);
            void NewColumnRight(object sender, RoutedEventArgs e)
            {
                // Call the DeleteColumn method here
                TheWorkshop.CreateNewColumnRight(ColumnClass);
            }

            

            MenuItem MenuItemDeleteColumn = new MenuItem();
            MenuItemDeleteColumn.Header = "  Delete Column  ";
            ContextMenu.Items.Add(MenuItemDeleteColumn);
            MenuItemDeleteColumn.Click += new RoutedEventHandler(DeleteColumn_Click);
            void DeleteColumn_Click(object sender, RoutedEventArgs e)
            {
                // Call the DeleteColumn method here
                TheWorkshop.ColumnDelete(ColumnClass);
            }

            //MenuItem MenuItemDummy = new MenuItem();
            //MenuItemDummy.Header = "  Create New Column  (Right)";
            //ContextMenu.Items.Add(MenuItemDummy);
            //MenuItemDummy.Click += new RoutedEventHandler(DummyMethod);
            //void DummyMethod(object sender, RoutedEventArgs e)
            //{
            //    // Click code goes here

            //}
                        

            Header.ContextMenu = ContextMenu;


            Label Label = new Label();
            Label.FontSize = 15;
            Label.Height = 28;
            //Label.Width = 120;
            Label.Margin = new Thickness(2, 0, 0, 0); // Left Top Right Bottom 
            Label.Content = ColumnClass.ColumnName; //"This is Column X";
            DockPanel.SetDock(Label, Dock.Top);
            Label.HorizontalContentAlignment = HorizontalAlignment.Center;
            //ButtonAddRow.VerticalAlignment = VerticalAlignment.Top;
            //ButtonAddRow.HorizontalAlignment = HorizontalAlignment.Right;
            Header.Children.Add(Label);
            ColumnClass.ColumnLabel = Label;

            Label.MouseLeftButtonDown += ColumnGrid_MouseLeftButtonDown;
            void ColumnGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                ColumnActivate();

            }

            void ColumnActivate() 
            {
                TheWorkshop.PropertiesHide();
                TheWorkshop.ColumnProperties.Visibility = Visibility.Visible;
                TheWorkshop.PropertiesColumnNameBox.Text = ColumnClass.ColumnName;
                //TheWorkshop.EntryClass = EntryClass;
                TheWorkshop.RowClass = RowClass;
                TheWorkshop.ColumnClass = ColumnClass;

                if (Properties.Settings.Default.DeveloperMode == "Developer")
                {
                    Window parentWindow = Window.GetWindow(ColumnGrid);
                    TabControl tabControl = parentWindow.FindName("TabSet") as TabControl;
                    foreach (TabItem tabItem in tabControl.Items)
                    {
                        if (tabItem.Header != null && tabItem.Header.ToString() == "Properties")
                        {
                            tabItem.IsSelected = true;
                            //TheWorkshop.DebugBox.Text = "Hai";
                            break;
                        }
                    }
                }
            }

            //ColumnGrid.Children.Add(Label);





            Header.MouseMove += ColumnDrag;
            void ColumnDrag(object sender, MouseEventArgs e)
            {
                

                if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyUp(Key.LeftShift)) //Single Column capture
                {
                    //var TheDockPanel = (DockPanel)sender;
                    //var TheBorder = (Border)TheDockPanel.Parent;
                    var TheDockPanel = ColumnGrid;
                    var currentPosition = e.GetPosition(ColumnGrid);
                    var minimumDistance = (SystemParameters.MinimumHorizontalDragDistance + SystemParameters.MinimumVerticalDragDistance) / 2;

                    if (Math.Sqrt(Math.Pow(currentPosition.X, 2) + Math.Pow(currentPosition.Y, 2)) >= minimumDistance)
                    {
                        var data = new DataObject("MoveColumnClass", ColumnClass);
                        DragDrop.DoDragDrop(ColumnGrid, data, DragDropEffects.Move);
                    }
                    TheDockPanel.ReleaseMouseCapture();


                }
                   



            }

            Header.Drop += ColumnDrop;
            void ColumnDrop(object sender, DragEventArgs e)
            {

                if (e.Data.GetDataPresent("MoveColumnClass") && Keyboard.IsKeyUp(Key.LeftShift)) //Single Entry Drop
                {
                    Column InputColumn = (Column)e.Data.GetData("MoveColumnClass");

                    if (InputColumn != ColumnClass)
                    {
                        InputColumn.ColumnRow.RowDockPanel.Children.Remove(InputColumn.ColumnGrid);
                        int FromIndex = InputColumn.ColumnRow.ColumnList.IndexOf(InputColumn);
                        int ToIndex = ColumnClass.ColumnRow.RowDockPanel.Children.IndexOf(ColumnClass.ColumnGrid);
                        InputColumn.ColumnRow.ColumnList.RemoveAt(FromIndex);

                        ColumnClass.ColumnRow.RowDockPanel.Children.Insert(ToIndex + 1, InputColumn.ColumnGrid);
                        ColumnClass.ColumnRow.ColumnList.Insert(ToIndex, InputColumn);

                        //InputColumn.ColumnRow = ColumnClass.ColumnRow; //DO NOT REFER TO COLUMN CLASS DIRECTLY, I HAVE NO IDEA WHY.                    
                        InputColumn.ColumnRow = ColumnClass.ColumnRow;
                    }

                }
                




            }
            //EntryDockPanel.AllowDrop = true;




            //This is part of how entrys can move with the mouse. The other part is the MouseMove event in CreateEntry.

            Header.Drop += ColumnGrid_Drop;
            //ColumnGrid.Drop += ColumnGrid_Drop;
            void ColumnGrid_Drop(object sender, DragEventArgs e)
            {
                if (e.Data.GetDataPresent("MoveEntryClass") && Keyboard.IsKeyUp(Key.LeftShift))
                {
                    Entry InputEntry = (Entry)e.Data.GetData("MoveEntryClass");

                    InputEntry.EntryColumn.ColumnGrid.Children.Remove(InputEntry.EntryBorder);
                    int FromIndex = InputEntry.EntryColumn.EntryList.IndexOf(InputEntry);
                    InputEntry.EntryColumn.EntryList.RemoveAt(FromIndex);

                    ColumnGrid.Children.Insert(1, InputEntry.EntryBorder);
                    ColumnClass.EntryList.Insert(0, InputEntry);


                    InputEntry.EntryColumn = ColumnClass;
                    InputEntry.EntryRow = ColumnClass.ColumnRow;

                }
                else if (e.Data.GetDataPresent("MoveEntryClassGroup") && Keyboard.IsKeyDown(Key.LeftShift)) 
                {                    

                    for (int i = 0; i < TheWorkshop.EntryMoveList.Count; i++ ) 
                    {
                        
                        var InputEntry = TheWorkshop.EntryMoveList[i];
                        
                        InputEntry.EntryColumn.ColumnGrid.Children.Remove(InputEntry.EntryBorder);
                        InputEntry.EntryColumn.EntryList.RemoveAt(InputEntry.EntryColumn.EntryList.IndexOf(InputEntry));

                        ColumnGrid.Children.Insert(1 + i, InputEntry.EntryBorder);                        
                        ColumnClass.EntryList.Insert(0 + i, InputEntry);

                        InputEntry.EntryColumn = ColumnClass;
                        InputEntry.EntryRow = ColumnClass.ColumnRow;
                    }
                }

                


            }            
            Header.AllowDrop = true;

        }
                

        public void CreateEntry(Editor EditorClass, Page PageClass, Row RowClass, Column ColumnClass, Entry EntryClass, Workshop TheWorkshop, Database Database)
        {
            //An entry is the main attraction of the program. It is the numerical value of a hex / cell in a file.
            //It can do all kinds of things, be displayed all kinds of ways, and is extremely flexable.
            //For more information, the file "EntryManager" and it's methods go over a lot about what a entry can do.

            //Temp block, move later
            
            //end of temp block

            Border Border = new();
            Border.BorderThickness = new Thickness(1);
            Border.CornerRadius = new CornerRadius(5);
            DockPanel.SetDock(Border, Dock.Top);
            Border.Margin = new Thickness(4, 0, 5, 3);// Left Top Right Bottom 
            

            DockPanel EntryDockPanel = new();
            EntryDockPanel.Style = (Style)Application.Current.Resources["EntryStyle"];
            //EntryDockPanel.MinWidth = 145;
            EntryDockPanel.MinHeight = 26;    //Any smalled and NumberBoxes "shake" when you move the cursor with the arrow keys.        
            DockPanel.SetDock(EntryDockPanel, Dock.Top);
            //EntryDockPanel.VerticalAlignment = VerticalAlignment.Top; //Top Bottom
            //EntryDockPanel.HorizontalAlignment = HorizontalAlignment.Left; //Left Right            
            //EntryDockPanel.Margin = new Thickness(3, 0, 4, 3); // Left Top Right Bottom 

            EntryClass.EntryEditor = EditorClass;//I say what it's parents all are for easy access.
            EntryClass.EntryPage = PageClass;
            EntryClass.EntryRow = RowClass;
            EntryClass.EntryColumn= ColumnClass;

            EntryDockPanel.MouseLeftButtonDown += EntryGrid_MouseLeftButtonDown;
            // define the event handler method
            void EntryGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    // Your code here
                }
                else 
                {
                    EntryManager EntryData = new();
                    EntryData.EntryBecomeActive(EntryClass);
                    EntryData.UpdateEntryProperties(TheWorkshop, EditorClass);

                    EditorClass.TopBar.EntryNoteBox.Text = EntryClass.EntryTooltip;
                }

                TheWorkshop.FillLearnBox(EditorClass, EntryClass);        

            }






            bool _mousePressedOnEntry = false;

            EntryDockPanel.MouseLeftButtonDown += EntryDockPanel_MouseLeftButtonDown;
            EntryDockPanel.MouseMove += EntryBorder_MouseMove;

            void EntryDockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (e.OriginalSource is TextBox)
                {
                    return; // If it's a TextBox, return and don't set the flag
                }

                var theDockPanel = (DockPanel)sender;
                var theBorder = (Border)theDockPanel.Parent;
                var hitTestResult = VisualTreeHelper.HitTest(theBorder, e.GetPosition(theBorder));

                if (hitTestResult != null)
                {
                    _mousePressedOnEntry = true;
                }
            }

            void EntryBorder_MouseMove(object sender, MouseEventArgs e)
            {
                if (e.OriginalSource is TextBox)
                {
                    _mousePressedOnEntry = false;
                    return; // If it's a TextBox, return and don't execute the drag logic
                }

                if (e.LeftButton == MouseButtonState.Pressed && _mousePressedOnEntry)
                {
                    var theDockPanel = (DockPanel)sender;
                    var theBorder = (Border)theDockPanel.Parent;
                    var hitTestResult = VisualTreeHelper.HitTest(theBorder, e.GetPosition(theBorder));

                    if (hitTestResult != null)
                    {
                        if (Keyboard.IsKeyUp(Key.LeftShift)) // Single Entry capture
                        {
                            var currentPosition = e.GetPosition(theBorder);
                            var minimumDistance = (SystemParameters.MinimumHorizontalDragDistance + SystemParameters.MinimumVerticalDragDistance) / 2;

                            if (Math.Sqrt(Math.Pow(currentPosition.X, 2) + Math.Pow(currentPosition.Y, 2)) >= minimumDistance)
                            {
                                var data = new DataObject("MoveEntryClass", EntryClass);
                                DragDrop.DoDragDrop(theBorder, data, DragDropEffects.Move);
                            }
                            theDockPanel.ReleaseMouseCapture();
                        }
                        else if (Keyboard.IsKeyDown(Key.LeftShift)) // Group Entry Capture
                        {
                            var ActiveEntry = EditorClass.SelectedEntry;

                            if (ActiveEntry.EntryColumn == EntryClass.EntryColumn)
                            {
                                TheWorkshop.EntryMoveList.Clear(); // This chunk adds all entrys to a new list, so we can move all of them.
                                int iOne = EntryClass.EntryColumn.EntryList.IndexOf(EntryClass);
                                int iTwo = EditorClass.SelectedEntry.EntryColumn.EntryList.IndexOf(EditorClass.SelectedEntry);
                                int startIndex = Math.Min(iOne, iTwo);
                                int endIndex = Math.Max(iOne, iTwo);
                                for (int i = startIndex; i <= endIndex; i++)
                                {
                                    TheWorkshop.EntryMoveList.Add(EntryClass.EntryColumn.EntryList[i]);
                                }

                                var currentPosition = e.GetPosition(theBorder);
                                var minimumDistance = (SystemParameters.MinimumHorizontalDragDistance + SystemParameters.MinimumVerticalDragDistance) / 2;

                                if (Math.Sqrt(Math.Pow(currentPosition.X, 2) + Math.Pow(currentPosition.Y, 2)) >= minimumDistance)
                                {
                                    var data = new DataObject("MoveEntryClassGroup", EntryClass);
                                    DragDrop.DoDragDrop(theBorder, data, DragDropEffects.Move);
                                }
                                theDockPanel.ReleaseMouseCapture();
                            }
                        }
                    }
                }
                else
                {
                    _mousePressedOnEntry = false;
                }
            }








            


            EntryDockPanel.Drop += EntryDrop;
            void EntryDrop(object sender, DragEventArgs e)
            {                
                
                if (e.Data.GetDataPresent("MoveEntryClass") && Keyboard.IsKeyUp(Key.LeftShift)) //Single Entry Drop
                {
                    Entry InputEntry = (Entry)e.Data.GetData("MoveEntryClass");                    

                    if (InputEntry != EntryClass) 
                    {
                        InputEntry.EntryColumn.ColumnGrid.Children.Remove(InputEntry.EntryBorder);
                        int FromIndex = InputEntry.EntryColumn.EntryList.IndexOf(InputEntry);
                        int ToIndex = EntryClass.EntryColumn.ColumnGrid.Children.IndexOf(EntryClass.EntryBorder);
                        InputEntry.EntryColumn.EntryList.RemoveAt(FromIndex);

                        EntryClass.EntryColumn.ColumnGrid.Children.Insert(ToIndex + 1, InputEntry.EntryBorder);
                        EntryClass.EntryColumn.EntryList.Insert(ToIndex, InputEntry);

                        InputEntry.EntryColumn = EntryClass.EntryColumn; //DO NOT REFER TO COLUMN CLASS DIRECTLY, I HAVE NO IDEA WHY.                    
                        InputEntry.EntryRow = EntryClass.EntryRow;
                    }

                    //int More = InputEntry.EntryByteSizeNum - 1;
                    //if (More != 0) 
                    //{
                    //    Entry TargetEntry = ;
                    //    TargetEntry.EntryColumn.ColumnGrid.Children.Remove(TargetEntry.EntryBorder);
                    //    int FromIndex = TargetEntry.EntryColumn.EntryList.IndexOf(TargetEntry);
                    //    int ToIndex = EntryClass.EntryColumn.ColumnGrid.Children.IndexOf(EntryClass.EntryBorder);
                    //    TargetEntry.EntryColumn.EntryList.RemoveAt(FromIndex);

                    //    EntryClass.EntryColumn.ColumnGrid.Children.Insert(ToIndex + 1, TargetEntry.EntryBorder);
                    //    EntryClass.EntryColumn.EntryList.Insert(ToIndex, TargetEntry);

                    //    TargetEntry.EntryColumn = EntryClass.EntryColumn; //DO NOT REFER TO COLUMN CLASS DIRECTLY, I HAVE NO IDEA WHY.                    
                    //    TargetEntry.EntryRow = EntryClass.EntryRow;
                    //}
                    
                }
                else if (e.Data.GetDataPresent("MoveEntryClassGroup") && Keyboard.IsKeyDown(Key.LeftShift)) //Group Entry Drop
                {                    

                    for (int i = 0; i < TheWorkshop.EntryMoveList.Count; i++) 
                    {                        
                        if (EntryClass == TheWorkshop.EntryMoveList[i])
                        {
                            return;
                        }
                    }
                    

                    for (int i = 0; i < TheWorkshop.EntryMoveList.Count; i++)
                    {
                        var InputEntry = TheWorkshop.EntryMoveList[i];

                        InputEntry.EntryColumn.ColumnGrid.Children.Remove(InputEntry.EntryBorder);
                        int FromIndex = InputEntry.EntryColumn.EntryList.IndexOf(InputEntry);
                        int ToIndex = EntryClass.EntryColumn.ColumnGrid.Children.IndexOf(EntryClass.EntryBorder);
                        //int TheIndex = ColumnClass.ColumnGrid.Children.IndexOf(EntryClass.EntryBorder); //Counting starts at 1, but the first child is 0.
                        InputEntry.EntryColumn.EntryList.RemoveAt(InputEntry.EntryColumn.EntryList.IndexOf(InputEntry));

                        EntryClass.EntryColumn.ColumnGrid.Children.Insert(ToIndex + 1 + i, InputEntry.EntryBorder);
                        EntryClass.EntryColumn.EntryList.Insert(ToIndex + 0 + i, InputEntry);

                        InputEntry.EntryColumn = EntryClass.EntryColumn; //DO NOT REFER TO COLUMN CLASS DIRECTLY, I HAVE NO IDEA WHY.     
                        InputEntry.EntryRow = EntryClass.EntryRow;
                    }
                }




            }
            EntryDockPanel.AllowDrop = true;



            ColumnClass.ColumnGrid.Children.Add(Border);
            Border.Child = EntryDockPanel;
            EntryClass.EntryDockPanel = new();
            EntryClass.EntryDockPanel = EntryDockPanel;
            EntryClass.EntryBorder = new();
            EntryClass.EntryBorder = Border;
            if (EntryClass.EntryByteSizeNum == 0) { EntryClass.EntryBorder.Visibility = Visibility.Collapsed; }

            Label Prefix = new Label();
            //Prefix.Height = 30;
            Prefix.MinWidth = 15;
            Prefix.FontSize = 14;
            Prefix.Content = EntryClass.EntryByteOffset;  //"P-x";//EntryClass.EntryName;// "Entry X";
            Prefix.Foreground = (Brush)new BrushConverter().ConvertFrom("#20A098");
            Prefix.HorizontalAlignment = HorizontalAlignment.Left;
            Prefix.VerticalContentAlignment = VerticalAlignment.Center;
            //Prefix.Margin = new Thickness(0, 0, 0, 0); // Left Top Right Bottom 
            Prefix.Visibility = Visibility.Collapsed;
            if (Properties.Settings.Default.EntryPrefix == "None")        {Prefix.Visibility = Visibility.Collapsed;}
            if (Properties.Settings.Default.EntryPrefix == "Row Offset - Decimal Starting at 0") {Prefix.Visibility = Visibility.Visible; Prefix.Content = EntryClass.EntryByteOffset; }
            if (Properties.Settings.Default.EntryPrefix == "Row Offset - Decimal Starting at 1") { Prefix.Visibility = Visibility.Visible; Prefix.Content = EntryClass.EntryByteOffset + 1; }
            if (Properties.Settings.Default.EntryPrefix == "Row Offset - Hex Starting at 0x00")  { Prefix.Visibility = Visibility.Visible; Prefix.Content = EntryClass.EntryByteOffset.ToString("X"); }
            if (Properties.Settings.Default.EntryPrefix == "Row Offset - Hex Starting at 0x01")  { Prefix.Visibility = Visibility.Visible; Prefix.Content = (EntryClass.EntryByteOffset +1).ToString("X"); }
            if (Properties.Settings.Default.EntryPrefix == "Byte Address - Decimal Starting at 0") { Prefix.Visibility = Visibility.Visible; Prefix.Content = EntryClass.EntryByteOffset; }
            if (Properties.Settings.Default.EntryPrefix == "Byte Address - Hex Starting at 0x00") { Prefix.Visibility = Visibility.Visible; Prefix.Content = EntryClass.EntryByteOffset.ToString("X"); }
            if (Properties.Settings.Default.DeveloperMode == "User") { Prefix.Visibility = Visibility.Collapsed; }
            EntryDockPanel.Children.Add(Prefix);
            EntryClass.EntryPrefix = Prefix;


            //add a option to properties where a entrys can have a Icon on the left side. for easy, universal, user styling / expression.
            Label NameBox = new Label();
            //NameBox.Background = Brushes.IndianRed;
            //NameBox.Height = 30;
            NameBox.MinWidth = 80;
            NameBox.FontSize = 15;
            NameBox.Content = EntryClass.EntryName;// "Entry X";
            NameBox.HorizontalAlignment = HorizontalAlignment.Left;
            NameBox.VerticalContentAlignment = VerticalAlignment.Center;
            EntryDockPanel.Children.Add(NameBox);
            if (EntryClass.EntryLabelShown == "Show Name") { NameBox.Visibility = Visibility.Visible; }
            if (EntryClass.EntryLabelShown == "Hide Name") { NameBox.Visibility = Visibility.Collapsed; }
            EntryClass.EntryLabel = NameBox;


            



            //private void BetaLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)

            //A entry can be one of 5 main types currently planned.
            //Note that dropdowns are not existing right now, but would be basically the same as a list.
            //If you play with lists, it's obvious how they are related.
            EntryManager EType = new();
            if (EntryClass.EntrySubType == "NumberBox")
            {
                EType.CreateNumberBox(TheWorkshop, EntryClass);
            }
            if (EntryClass.EntrySubType == "CheckBox")
            {
                EType.CreateCheckBox(TheWorkshop, EntryClass);  
            }
            if (EntryClass.EntrySubType == "BitFlag")
            {
                EType.CreateBitFlag(TheWorkshop, EntryClass);
            }
            if (EntryClass.EntrySubType == "DropDown")
            {
                EType.CreateDropDown(EntryClass);
            }
            if (EntryClass.EntrySubType == "List")
            {
                EType.CreateList(EntryClass, TheWorkshop);
            }

            
            
            if (EntryClass.EntrySaveState == "AutoDisabled")
            {
                EntryClass.EntrySaveState = "Enabled";
            }
            else if (EntryClass.EntrySaveState == "Disabled")
            {
                Border.Style = (Style)Application.Current.Resources["EntryDisabled"];
            }


            if (EditorClass.NameTableFilePath != null)
            {
                if (EditorClass.NameTableFilePath == EditorClass.EditorFilePath)
                {
                    int Min = EditorClass.EditorTableStart;
                    int Max = Min + EditorClass.EditorTableRowSize;
                    if (EditorClass.NameTableStart >= Min && EditorClass.NameTableStart <= Max)
                    {
                        int NAMEMIN = EditorClass.NameTableStart - EditorClass.EditorTableStart;
                        int NAMEMAX = NAMEMIN + EditorClass.NameTableTextSize - 1;//the 1st byte is "0", so we need a -1 for proper counting.
                        if (EntryClass.EntryByteOffset >= NAMEMIN && EntryClass.EntryByteOffset <= NAMEMAX)
                        {
                            EntryClass.EntrySaveState = "AutoDisabled";
                            Border.Style = (Style)Application.Current.Resources["EntryAutoDisabled"];
                        }
                    }
                }
            }
            foreach (ExtraTable ExtraTable in EditorClass.ExtraTableList)
            {
                if (EditorClass.EditorFilePath == ExtraTable.ExtraTableFilePath)
                {
                    int Min = EditorClass.EditorTableStart;
                    int Max = Min + EditorClass.EditorTableRowSize;
                    if (ExtraTable.ExtraTableStart >= Min && ExtraTable.ExtraTableStart <= Max)
                    {
                        int EXTRAMIN = ExtraTable.ExtraTableStart - EditorClass.EditorTableStart;
                        int EXTRAMAX = EXTRAMIN + ExtraTable.ExtraTableTextSize - 1;//the 1st byte is "0", so we need a -1 for proper counting.
                        if (EntryClass.EntryByteOffset >= EXTRAMIN && EntryClass.EntryByteOffset <= EXTRAMAX)
                        {
                            EntryClass.EntrySaveState = "AutoDisabled";
                            Border.Style = (Style)Application.Current.Resources["EntryAutoDisabled"];
                        }
                    }
                }
            }

            

            

        }

        

    }




}

using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.TextFormatting;
//using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace Crystal_Editor
{
    class LoadDatabaseWithEditorInfo
    {

        string TargetXML;
        string FilesXML;
        List<string> EditorsList = new();


        //Later: I should create a editor's button here instead of during EditorCreate, because some editors / users may
        //want Semi-Auto mode, causeing the program to only load an editor when clicked on instead of every editor at once. This may reduce lag / wait times?


        public void LoadWorkshopInfo(Workshop TheWorkshop, Database Database) //Triggers when the workshop is launched.
        {            
            FilesXML = Path.Combine(TheWorkshop.ExePath, "Workshops", TheWorkshop.WorkshopName, "WorkshopInfo.xml");
            XElement Filesxml = XElement.Load(FilesXML); //This loads a XML from your workshop called WorkshopFiles.xml

            foreach (XElement EditorName in Filesxml.Descendants("EditorName"))
            {
                EditorsList.Add(EditorName.Value);
            }

            foreach (XElement WorkshopFile in Filesxml.Descendants("File"))
            {
                GameFile FileInfo = new(); //This class stores everything about a file.
                FileInfo.FileName = WorkshopFile.Element("FileName")?.Value;
                FileInfo.FileNickName = WorkshopFile.Element("FileNickName")?.Value;
                FileInfo.FilePath = WorkshopFile.Element("FilePath")?.Value;
                if (TheWorkshop.IsPreviewMode == false)
                {
                    FileInfo.FileBytes = System.IO.File.ReadAllBytes(TheWorkshop.InputDirectory + FileInfo.FilePath);
                }


                Database.GameFiles.Add(FileInfo.FilePath, FileInfo);//Adding the GameFile to the Dictionary, with the Key of the FilePath so the key is ALWAYS unique.    
            }
            
            
        }
        

        

        

        public void LoadEditorModeAuto(Workshop TheWorkshop, Database Database) //Triggers when the workshop is launched / opened.
        {

            //string[] EditorFolderNames = Directory.GetDirectories(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors").Select(Path.GetFileName).ToArray();
            //foreach (string FolderName in EditorFolderNames)
            //{
            //    TargetXML = Path.Combine(TheWorkshop.ExePath, "Workshops", TheWorkshop.WorkshopName, "Editors", FolderName, "EditorInfo.xml");
            //    LoadTheDatabase(TheWorkshop, Database);
            //    //CreateButton(TheWorkshop);
            //}

            foreach (string EditorName in EditorsList) 
            {
                TargetXML = Path.Combine(TheWorkshop.ExePath, "Workshops", TheWorkshop.WorkshopName, "Editors", EditorName, "EditorInfo.xml");
                if (File.Exists(TargetXML))
                {
                    // The file exists, so load the database
                    TargetXML = Path.Combine(TheWorkshop.ExePath, "Workshops", TheWorkshop.WorkshopName, "Editors", EditorName, "EditorInfo.xml");
                    LoadTheDatabase(TheWorkshop, Database);
                }
                
                
            }

            string[] EditorFolderNames = Directory.GetDirectories(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors").Select(Path.GetFileName).ToArray();
            foreach (string FolderName in EditorFolderNames)
            {
                if (!EditorsList.Contains(FolderName))
                {
                    TargetXML = Path.Combine(TheWorkshop.ExePath, "Workshops", TheWorkshop.WorkshopName, "Editors", FolderName, "EditorInfo.xml");
                    LoadTheDatabase(TheWorkshop, Database);
                }                
                //CreateButton(TheWorkshop);
            }
            //For each editor in a list
            //Then for all editors NOT in that list.
        }
                

        public void LoadTheDatabase(Workshop TheWorkshop, Database Database)
        {


            XElement xml = XElement.Load(TargetXML);
            TheWorkshop.EditorName = xml.Element("EditorName")?.Value; //Sets the name of the editor were working with from the name stored in XML.
            

            Editor EditorClass = new();          //Creates a EditorClass
            EditorClass.EditorName = xml.Element("EditorName")?.Value; 
            EditorClass.EditorGraphic = xml.Element("EditorGraphic")?.Value;

            EditorClass.PageList = new();          //Creates a Page list in the core.    

            foreach (XElement item in xml.Descendants("EditorFile")) 
            {
                EditorClass.EditorFilePath = item.Element("EditorFilePath")?.Value;
                EditorClass.EditorTableStart = Int32.Parse(item.Element("EditorTableStart")?.Value);
                EditorClass.EditorTableRowSize = Int32.Parse(item.Element("EditorTableRowSize")?.Value);
            }
            

            foreach (XElement item in xml.Descendants("NameTableFile"))
            {
                EditorClass.NameTableFilePath = item.Element("NameTableFilePath")?.Value;
                EditorClass.NameTableCharacterSet = item.Element("NameTableCharacterSet")?.Value;
                EditorClass.NameTableStart = Int32.Parse(item.Element("NameTableStart")?.Value);
                EditorClass.NameTableTextSize = Int32.Parse(item.Element("NameTableTextSize")?.Value); 
                EditorClass.NameTableRowSize = Int32.Parse(item.Element("NameTableRowSize")?.Value);
                EditorClass.NameTableItemCount = Int32.Parse(item.Element("NameTableItemCount")?.Value);
            }

            EditorClass.ExtraTableList = new();
            foreach (XElement item in xml.Descendants("ExtraTable")) 
            {
                ExtraTable ExtraTable = new();
                ExtraTable.ExtraTableName = item.Element("ExtraTableName")?.Value;
                ExtraTable.ExtraTableCharacterSet = item.Element("ExtraTableCharacterSet")?.Value; 
                ExtraTable.ExtraTableFilePath = item.Element("ExtraTableFilePath")?.Value;
                ExtraTable.ExtraTableStart = Int32.Parse(item.Element("ExtraTableStart")?.Value);
                ExtraTable.ExtraTableTextSize = Int32.Parse(item.Element("ExtraTableTextSize")?.Value);
                ExtraTable.ExtraTableRowSize = Int32.Parse(item.Element("ExtraTableRowSize")?.Value);
                EditorClass.ExtraTableList.Add(ExtraTable);
            }

            

            foreach (KeyValuePair<string, GameFile> GameFile in Database.GameFiles)
            {
                //if (EditorClass.FileName == GameFile.Value.FileName)
                if (EditorClass.EditorFilePath == GameFile.Value.FilePath)
                {
                    EditorClass.EditorFile = GameFile.Value;
                }

                if (EditorClass.NameTableFilePath == GameFile.Value.FilePath)
                {
                    EditorClass.NameTableFile = GameFile.Value;
                }

                foreach (ExtraTable ExtraTable in EditorClass.ExtraTableList) 
                {
                    if (ExtraTable.ExtraTableFilePath == GameFile.Value.FilePath)
                    {
                        ExtraTable.ExtraTableFile = GameFile.Value;
                    }
                }
            }


            EditorClass.LeftBar = new();
            EditorClass.LeftBar.ItemList = new();

            //EditorClass.LeftBar.TreeViewNames = new();
            foreach (XElement item in xml.Descendants("CollectionItem"))
            {
                ItemInfo ItemInfo = new();

                ItemInfo.ItemName = item.Element("ItemName")?.Value;
                ItemInfo.ItemIndex = Int32.Parse(item.Element("ItemIndex")?.Value);
                ItemInfo.ItemType = item.Element("ItemType")?.Value;
                ItemInfo.ItemTooltip = item.Element("ItemTooltip")?.Value;
                ItemInfo.ItemColor = item.Element("ItemColor")?.Value;
                ItemInfo.ItemNote = item.Element("ItemNote")?.Value;

                EditorClass.LeftBar.ItemList.Add(ItemInfo);

                
                if (ItemInfo.ItemType == "Folder")
                {
                    foreach (XElement child in item.Descendants("ChildItem"))
                    {
                        ItemInfo childInfo = new ItemInfo(); 
                        childInfo.ItemName = child.Element("ItemName")?.Value; 
                        childInfo.ItemIndex = int.Parse(child.Element("ItemIndex")?.Value); 
                        childInfo.ItemType = child.Element("ItemType")?.Value; 
                        childInfo.ItemTooltip = child.Element("ItemTooltip")?.Value; 
                        childInfo.ItemColor = child.Element("ItemColor")?.Value; 
                        childInfo.ItemNote = child.Element("ItemNote")?.Value;
                        // Add the child item to the parent folder's children list
                        EditorClass.LeftBar.ItemList.Add(childInfo);
                        //ItemInfo.Children.Add(childInfo);
                    }
                    
                }

                
                
            }

            if (TheWorkshop.IsPreviewMode == false) 
            {
                CharacterSetManager CharacterManager = new();
                CharacterManager.Decode(TheWorkshop, EditorClass, "Items");
            }
            

            



            foreach (XElement Xpage in xml.Descendants("Page"))
            {
                //EditorClass.PageList.Add(new Page());  //Creates a page inside the page list in the core.
                string pageName = Xpage.Element("PageName")?.Value;
                //EditorClass.PageList.Add(new Page { PageName = pageName });
                Page PageClass = new Page { PageName = pageName };

                foreach (XElement Xrow in Xpage.Descendants("Row"))
                {
                    string rowName = Xrow.Element("RowName")?.Value;
                    //int rowOrder = int.Parse(row.Element("RowOrder")?.Value);
                    Row RowClass = new Row { RowName = rowName }; //, RowOrder = rowOrder

                    // Add the row object to the page's RowList
                    PageClass.RowList ??= new List<Row>();
                    PageClass.RowList.Add(RowClass);

                    foreach (XElement Xcolumn in Xrow.Descendants("Column"))
                    {
                        string columnName = Xcolumn.Element("ColumnName")?.Value;
                        Column ColumnClass = new Column { ColumnName = columnName }; //, ColumnOrder = columnOrder


                        RowClass.ColumnList ??= new List<Column>(); // Add the  object to the List
                        RowClass.ColumnList.Add(ColumnClass);
                        ColumnClass.EntryList ??= new List<Entry>(); // Add the  object to the List
                        foreach (XElement Xentry in Xcolumn.Descendants("Entry"))
                        {

                            Entry EntryClass = new();

                            EntryClass.EntryName = Xentry.Element("EntryName")?.Value;
                            EntryClass.EntryTooltip = Xentry.Element("EntryTooltip")?.Value;
                            EntryClass.EntrySaveState = Xentry.Element("EntrySaveState")?.Value;
                            EntryClass.EntryLabelShown = Xentry.Element("EntryLabelShown")?.Value;
                            EntryClass.EntryGameTableSize = Int32.Parse(Xentry.Element("EntryGameTableSize")?.Value);
                            EntryClass.EntryByteOffset = Int32.Parse(Xentry.Element("EntryByteOffset")?.Value);
                            EntryClass.EntryByteSize = Xentry.Element("EntryByteSize")?.Value;
                            EntryClass.EntryByteSizeNum = Int32.Parse(Xentry.Element("EntryByteSizeNum")?.Value);
                            EntryClass.EntryMainType = Xentry.Element("EntryMainType")?.Value;
                            EntryClass.EntrySubType = Xentry.Element("EntrySubType")?.Value;


                            if (EntryClass.EntrySubType == "NumberBox") 
                            {
                                EntryClass.EntryTypeNumberBox = new();
                                foreach (XElement XNumberBox in Xentry.Descendants("EntryTypeNumberBox"))
                                {
                                    
                                    EntryClass.EntryTypeNumberBox.NumberSign = XNumberBox.Element("NumberSign")?.Value;
                                }
                            }

                            if (EntryClass.EntrySubType == "CheckBox") 
                            {
                                EntryClass.EntryTypeCheckBox = new();
                                foreach (XElement XCheckBox in Xentry.Descendants("EntryTypeCheckBox"))
                                {                                    
                                    EntryClass.EntryTypeCheckBox.CheckBoxTrueText = XCheckBox.Element("CheckBoxTrueText")?.Value;
                                    EntryClass.EntryTypeCheckBox.CheckBoxFalseText = XCheckBox.Element("CheckBoxFalseText")?.Value;
                                    EntryClass.EntryTypeCheckBox.CheckBoxTrueValue = Int32.Parse(XCheckBox.Element("CheckBoxTrueValue")?.Value);
                                    EntryClass.EntryTypeCheckBox.CheckBoxFalseValue = Int32.Parse(XCheckBox.Element("CheckBoxFalseValue")?.Value);
                                }
                            }


                            
                            if (EntryClass.EntrySubType == "BitFlag") 
                            {
                                EntryClass.EntryTypeBitFlag = new();
                                foreach (XElement XBitFlag in Xentry.Descendants("EntryTypeBitFlag"))
                                {                                    
                                    EntryClass.EntryTypeBitFlag.BitFlag1Name = XBitFlag.Element("BitFlag1Name")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag2Name = XBitFlag.Element("BitFlag2Name")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag3Name = XBitFlag.Element("BitFlag3Name")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag4Name = XBitFlag.Element("BitFlag4Name")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag5Name = XBitFlag.Element("BitFlag5Name")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag6Name = XBitFlag.Element("BitFlag6Name")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag7Name = XBitFlag.Element("BitFlag7Name")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag8Name = XBitFlag.Element("BitFlag8Name")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag1CheckText = XBitFlag.Element("BitFlag1CheckText")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag2CheckText = XBitFlag.Element("BitFlag2CheckText")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag3CheckText = XBitFlag.Element("BitFlag3CheckText")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag4CheckText = XBitFlag.Element("BitFlag4CheckText")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag5CheckText = XBitFlag.Element("BitFlag5CheckText")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag6CheckText = XBitFlag.Element("BitFlag6CheckText")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag7CheckText = XBitFlag.Element("BitFlag7CheckText")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag8CheckText = XBitFlag.Element("BitFlag8CheckText")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag1UncheckText = XBitFlag.Element("BitFlag1UncheckText")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag2UncheckText = XBitFlag.Element("BitFlag2UncheckText")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag3UncheckText = XBitFlag.Element("BitFlag3UncheckText")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag4UncheckText = XBitFlag.Element("BitFlag4UncheckText")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag5UncheckText = XBitFlag.Element("BitFlag5UncheckText")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag6UncheckText = XBitFlag.Element("BitFlag6UncheckText")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag7UncheckText = XBitFlag.Element("BitFlag7UncheckText")?.Value;
                                    EntryClass.EntryTypeBitFlag.BitFlag8UncheckText = XBitFlag.Element("BitFlag8UncheckText")?.Value;

                                }
                            }



                            if (EntryClass.EntrySubType == "List")
                            {
                                EntryClass.EntryTypeList = new();
                                foreach (XElement XList in Xentry.Descendants("EntryTypeList"))
                                {
                                    EntryClass.EntryTypeList.ListSize = Int32.Parse(XList.Element("ListSize")?.Value);
                                    string[] listItems = new string[EntryClass.EntryTypeList.ListSize];

                                    XElement XItemList = XList.Element("ItemList");                                    
                                    foreach (XElement XItem in XItemList.Elements("Item"))
                                    {
                                        string listItemValue = XItem.Value;
                                        int colonIndex = listItemValue.IndexOf(':');
                                        if (colonIndex >= 0)
                                        {                                            
                                            string indexString = listItemValue.Substring(0, colonIndex).Trim(); // Extract the index and text from the list item value
                                            string text = listItemValue.Substring(colonIndex + 1).Trim();
                                                                                        
                                            if (int.TryParse(indexString, out int index)) // Try to parse the index as an integer
                                            {                                                
                                                if (index >= 0 && index < listItems.Length) // Check if the index is within the range of the list items array
                                                {                                                    
                                                    listItems[index] = text; // Add the text to the list items array at the specified index
                                                }
                                            }
                                        }
                                    }
                                    

                                    EntryClass.EntryTypeList.ListItems = listItems;
                                }
                            }


                                                       
                            

                            
                            
                            




                            ColumnClass.EntryList.Add(EntryClass);
                        }
                    }

                }
                EditorClass.PageList ??= new List<Page>();
                EditorClass.PageList.Add(PageClass);

            }
            Database.GameEditors.Add(TheWorkshop.EditorName, EditorClass); //Adds a core (aka the value) with the Key (New editor name from textbox) to the database dictionary.





            EditorCreate Maker = new EditorCreate();
            Maker.CreateEditor(TheWorkshop, Database, EditorClass); //Create a editor with this information.


        }



        public void LoadTheGraphics() 
        {


            //FilesXML = Path.Combine(TheWorkshop.ExePath, "Workshops", TheWorkshop.WorkshopName, "WorkshopFiles.xml");
            //XElement Filesxml = XElement.Load(FilesXML); //This loads a XML from your workshop called WorkshopFiles.xml
            //foreach ( File in TheWorkshop.ExePath + "\\Graphics\\" + subfolders   )
            //{

            //    Graphic Graphic = new(); //This class stores everything about a file.
            //    Graphic.GraphicName = ;
            //    Graphic.GraphicPath = ;


            //    Database.Graphics.Add(FileInfo.FilePath, FileInfo);//Adding the GameFile to the Dictionary, with the Key of the FilePath so the key is ALWAYS unique.




            //}
        }






        


























        //This Method is for creating a new editor while already inside a workshop. IE when going into a workshop and clicking the "New Editor" button.
        //It is diffrent from the previous stuff that was for loading the database with editors from files.
        //Both above and below load editor information into the database, and do not actually create the editor.
        //Both also run CreateEditor() at the end, which is what actually creates the editor.



        public void SetupNewEditor(Workshop TheWorkshop, Database Database) //This triggers when the user created a new editor.
        {
            TheWorkshop.EditorName = TheWorkshop.NewEditorNamebox.Text;

            Editor EditorClass = new(); //Creates the base class of the editor. Everything else becomes a child of this class, including other classes.
            EditorClass.EditorName = TheWorkshop.NewEditorNamebox.Text;
            //EditorClass.EditorGraphic = "Spells\\Magical Star.png";


            EditorClass.EditorTableStart = int.Parse(TheWorkshop.TextBoxBaseAddress.Text);
            EditorClass.EditorTableRowSize = int.Parse(TheWorkshop.TextBoxRowSize.Text);

            var selectedItem = TheWorkshop.FileTreeDataTable.SelectedItem as TreeViewItem; //The file the user wants to make an editor for.
            GameFile HexFile = selectedItem.Tag as GameFile;
            string Key = HexFile.FilePath;


            EditorClass.EditorFilePath = Database.GameFiles[Key].FilePath;
            EditorClass.EditorFile = HexFile;

            EditorClass.LeftBar = new();
            EditorClass.LeftBar.ItemList = new();
            int itemIndex = 0;

            //This part determines how the list of item names is gotten.
            //Type 1: Use user inputs names to a textbox and it uses those..
            //Type 2: The user points to a file to get them directly. It users more user info + needs to convert the bytes via character encoding.
            if (TheWorkshop.CreateEditorPartNameTableComboBoxNamesFrom.SelectedItem.ToString() == "System.Windows.Controls.ComboBoxItem: Input my own name list")
            {
                foreach (string line in TheWorkshop.NameList2.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                {
                    // Do something with the current line of text

                    //EditorClass.LeftBar.TreeViewNames.Add(line);
                    ItemInfo IInfo = new();
                    IInfo.ItemName = line;
                    IInfo.ItemIndex = itemIndex;
                    itemIndex++;
                    EditorClass.LeftBar.ItemList.Add(IInfo);
                }
            }
            if (TheWorkshop.CreateEditorPartNameTableComboBoxNamesFrom.SelectedItem.ToString() == "System.Windows.Controls.ComboBoxItem: Get name list directly from a game file")
            {
                TreeViewItem TreeItem = TheWorkshop.PartNameTableTreeviewFiles.SelectedItem as TreeViewItem;
                GameFile NameFile = TreeItem.Tag as GameFile;

                EditorClass.NameTableFile = NameFile;
                EditorClass.NameTableFilePath = NameFile.FilePath;
                EditorClass.NameTableCharacterSet = TheWorkshop.NewEditorComboBoxCharacterEncoding.Text;
                EditorClass.NameTableStart = int.Parse(TheWorkshop.NameTextboxNameTableStart.Text);
                EditorClass.NameTableTextSize = int.Parse(TheWorkshop.NewNameTableTextSize.Text);
                EditorClass.NameTableRowSize = int.Parse(TheWorkshop.NewNameTableRowSize.Text);
                EditorClass.NameTableItemCount = int.Parse(TheWorkshop.NameTextboxSubTableItemCount.Text);

                //int BaseAddress = Int32.Parse(TheWorkshop.NewEditorTextboxNameMainTableBaseAddress.Text);
                //int RowSize = Int32.Parse(TheWorkshop.NewEditorTextboxNameMainTableRowSize.Text);
                //int RowCount = Int32.Parse(TheWorkshop.NewEditorTextboxItemRowCount.Text);
                //

                for (int i = 0; i < EditorClass.NameTableItemCount; i++)
                {
                    ItemInfo ItemInfo = new();
                    //string name = "";
                    ItemInfo.ItemIndex = i;



                    EditorClass.LeftBar.ItemList.Add(ItemInfo);
                }

                CharacterSetManager CharacterManager = new();
                CharacterManager.Decode(TheWorkshop, EditorClass, "Items");


            }


            //IF extra text tables = Yes, Foreach extra text table...

            EditorClass.ExtraTableList = new();
            EditorClass.PageList = new();          //Creates a Page list. 

            Page PageClass = new();
            PageClass.PageName = "New Page";
            PageClass.RowList = new();
            EditorClass.PageList.Add(PageClass);

            Row RowClass = new();
            RowClass.RowName = "New Row";
            RowClass.ColumnList = new();
            PageClass.RowList.Add(RowClass);

            Column ColumnClass = new();
            ColumnClass.ColumnName = "New Column";
            ColumnClass.EntryList = new();
            RowClass.ColumnList.Add(ColumnClass);



            //ColumnClass.EntryList = new List<Entry>();
            for (int i = 0; i <= Int32.Parse(TheWorkshop.TextBoxRowSize.Text) - 1; i++)
            {
                //This is the default settings of every entry when a new editor is created.
                //All of these can be changed by the user, and need to be saved to XML, and loaded back from XML.
                //There exist some more as well, but those aren't strictly necessary to a new entry.

                Entry EntryClass = new();
                ColumnClass.EntryList.Add(EntryClass);


                EntryClass.EntryName = "??? " + i;
                EntryClass.EntryMainType = "Basic";
                EntryClass.EntrySaveState = "Enabled";
                EntryClass.EntryLabelShown = "Show Name";
                EntryClass.EntryMainType = "Basic";
                EntryClass.EntrySubType = "NumberBox";
                EntryClass.EntryGameTableSize = Int32.Parse(TheWorkshop.TextBoxRowSize.Text);
                EntryClass.EntryByteOffset = i;
                EntryClass.EntryByteSize = "1";
                EntryClass.EntryByteSizeNum = 1;

                EntryClass.EntryTypeNumberBox = new();
                EntryClass.EntryTypeNumberBox.NumberSign = "Positive";

                EntryClass.EntryTypeCheckBox = new();
                EntryClass.EntryTypeCheckBox.CheckBoxTrueValue = 1;
                EntryClass.EntryTypeCheckBox.CheckBoxFalseValue = 0;
                EntryClass.EntryTypeCheckBox.CheckBoxTrueText = "✔";
                EntryClass.EntryTypeCheckBox.CheckBoxFalseText = " ";

                EntryClass.EntryTypeBitFlag = new();
                EntryClass.EntryTypeBitFlag.BitFlag1Name = "Bit 1";
                EntryClass.EntryTypeBitFlag.BitFlag2Name = "Bit 2";
                EntryClass.EntryTypeBitFlag.BitFlag3Name = "Bit 3";
                EntryClass.EntryTypeBitFlag.BitFlag4Name = "Bit 4";
                EntryClass.EntryTypeBitFlag.BitFlag5Name = "Bit 5";
                EntryClass.EntryTypeBitFlag.BitFlag6Name = "Bit 6";
                EntryClass.EntryTypeBitFlag.BitFlag7Name = "Bit 7";
                EntryClass.EntryTypeBitFlag.BitFlag8Name = "Bit 8";
                EntryClass.EntryTypeBitFlag.BitFlag1CheckText = "✔";
                EntryClass.EntryTypeBitFlag.BitFlag2CheckText = "✔";
                EntryClass.EntryTypeBitFlag.BitFlag3CheckText = "✔";
                EntryClass.EntryTypeBitFlag.BitFlag4CheckText = "✔";
                EntryClass.EntryTypeBitFlag.BitFlag5CheckText = "✔";
                EntryClass.EntryTypeBitFlag.BitFlag6CheckText = "✔";
                EntryClass.EntryTypeBitFlag.BitFlag7CheckText = "✔";
                EntryClass.EntryTypeBitFlag.BitFlag8CheckText = "✔";
                EntryClass.EntryTypeBitFlag.BitFlag1UncheckText = " ";
                EntryClass.EntryTypeBitFlag.BitFlag2UncheckText = " ";
                EntryClass.EntryTypeBitFlag.BitFlag3UncheckText = " ";
                EntryClass.EntryTypeBitFlag.BitFlag4UncheckText = " ";
                EntryClass.EntryTypeBitFlag.BitFlag5UncheckText = " ";
                EntryClass.EntryTypeBitFlag.BitFlag6UncheckText = " ";
                EntryClass.EntryTypeBitFlag.BitFlag7UncheckText = " ";
                EntryClass.EntryTypeBitFlag.BitFlag8UncheckText = " ";

                EntryClass.EntryTypeList = new(); //A List is empty by default, this is not a mistake.

            }



            Database.GameEditors.Add(TheWorkshop.EditorName, EditorClass);

            EditorCreate Maker = new EditorCreate();
            Maker.CreateEditor(TheWorkshop, Database, EditorClass); //Create a editor with this information.
            //This is not inside any loop, so it really just makes an editor.

            TheWorkshop.CreateEditorPartReview.Visibility = Visibility.Collapsed;


        }





    }
















    

    

}

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
    class LoadDatabase
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






        public void LoadModeSemi(Workshop TheWorkshop)
        {
            //A feature in the future that i am putting off because i am intimidated by how much i let the problem get bigger when it
            //instead would have been very easy to do early on. 
            //That said, i also put it off because i can just wait and see if it's even needed to begin with.

            //In semiAuto mode, when the user clicks the editor button...
            //1: It saves the current editor information to MemoryFile.
            //2: It unloads the current editor.
            //3: It loads the new editor.
            //In this way, the program will never get laggy even if a huge amount of comically large editors all exist in one workshop.

        }




    }
}

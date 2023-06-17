using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
//using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace Crystal_Editor
{
    partial class SaveWorkshop
    {
        //I don't understand serization, so i manually create a XML for everything about a workshop.

        //add a Save entire workshop command?
        public void OnlySaveEditors(Database Database, Workshop TheWorkshop)
        {
            try
            {
                string ExtraPath = "";
                string FolderPath = "";

                ExtraPath = "\\Dummy Workshops"; //This extra string causes stuff to be saved to a path variant of the normal location, letting us test if a problem would occur, before actually saving to the right location.
                Directory.CreateDirectory(TheWorkshop.ExePath + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName);
                SaveAllEditors(Database, TheWorkshop, ExtraPath);
                SaveWorkshopInfo(Database, TheWorkshop, ExtraPath);
                Directory.Delete(TheWorkshop.ExePath + ExtraPath + "\\", true);

                ExtraPath = "";
                FolderPath = TheWorkshop.ExePath + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors";
                DirectoryInfo DummyDirectory = new DirectoryInfo(FolderPath);

                foreach (FileInfo file in DummyDirectory.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo subDirectory in DummyDirectory.GetDirectories())
                {
                    subDirectory.Delete(true);
                }

                
                SaveAllEditors(Database, TheWorkshop, ExtraPath);
                SaveWorkshopInfo(Database, TheWorkshop, ExtraPath);


                string[] EditorFolderNames = Directory.GetDirectories(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors").Select(Path.GetFileName).ToArray();
                foreach (string FolderName in EditorFolderNames) 
                {
                    //string textFilePath = ExePath + "\\Settings" + "\\Workshops\\" + WorkshopName + "\\UserInputDirectory.txt";
                    //string folderPath = File.ReadAllText(textFilePath);

                    //ExePath + "\\Projects\\" + WorkshopName + "\\" + ProjectNameTextbox.Text + "\\" + "ProjectInfo.xml"
                    if (Directory.Exists(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\Editors\\" + FolderName)) //folderpath
                    {
                        
                    }
                    else
                    {
                        Directory.CreateDirectory(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\Editors\\" + FolderName);
                    }
                    
                    string CopyFromPath = @TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + FolderName + "\\EditorInfo.xml";
                    string PasteToPath = @TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\Editors\\" + FolderName; // + "\\EditorInfo.xml"
                    File.Copy(CopyFromPath, Path.Combine(PasteToPath, Path.GetFileName(CopyFromPath)), true);
                }

                //System.IO.File.Delete(TheWorkshop.ExePath + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + editor.Key + "\\" + "\\EditorInfo.xml");
                //System.IO.File.Move(TheWorkshop.ExePath + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + editor.Key + "\\" + "\\EditorInfo2.xml", );
                //Copy file at TheWorkshop.ExePath + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + editor.Key + "\\" + "\\EditorInfo.xml"
                //to TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\Editors\\" + "ProjectInfo.xml"

            }
            catch 
            {



                string Error = "Error: Editors not saved." +
                    "\n" +
                    "\nAn error occured during the \"Saving Editors\" step of the save operation that just happened. Nothing has been corrupted, don't panic! :)" +
                    "\n" +
                    "\nAs you were probably saving more then only your editors, you'll be happy to hear that each part of saving is handled seperately. " +
                    "This means there is NO CHANCE that any other parts of the saving operation are affected, such as when also saving documents or game giles. " +
                    "\n" +
                    "\nAnyway as for editors, To help users know which documents are which on their computer, we save editors using the names you give them to actual folders. " +
                    "Each operating system has a diffrent list of symbols it doesn't allow folder names to use. " +
                    "To deal with this problem the program first runs a simulation of what would happen IF it actually saved anything, " +
                    "by creating a temporary dummy folder and saving everything to that temporary folder. " +
                    "This way there is no chance your actual editors folder will get corrupted or result in any other serious error. :)" +
                    "\n" +
                    "\nAs your seeing this error, it means your operating system doesn't like atleast one of the symbols you tried using in a editor's name. " +
                    "Try to avoid symbols like @, #, $, %, &, *, \\, /, :, ;, etc. Also most operating systems DO allow spaces in folder names, so the problem isn't that. " +
                    "\n" +
                    "\nTry changing the names of any editors you think might have caused the error, and then try saving your editors again. ";
                Notification f2 = new(Error);
                f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
            }
            
            

        }

        

        //Save Documents

        //Save Editors + Workshop Files + Documents

        //Save Editors + Workshop Files + Documents + Emulate


        public void SaveAllEditors(Database Database, Workshop TheWorkshop, string ExtraPath)
        {
            
            List<string> EditorsToDelete = new List<string>(); //Used to delete aka rename old folders. Currently causes a access forbidden crash.


            //For each editor, we are going to save EVERYTHING about it to a XML.
            //I am manually serializing because people online would not help me understand a better way
            //and doing it manually has some upsides like complete control, useful in the future when doing updates between program versions.
            //In the future, all foreach loops may want to be for loops, as i learned it gives a performance increase. For now this works fine.
            foreach (KeyValuePair<string, Editor> editor in Database.GameEditors)
            {               

                EditorsToDelete.Add(editor.Key);//Add editor string to name list
                Directory.CreateDirectory(TheWorkshop.ExePath + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + editor.Key);

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = ("    ");
                settings.CloseOutput = true;
                settings.OmitXmlDeclaration = true;
                using (XmlWriter writer = XmlWriter.Create(TheWorkshop.ExePath + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + editor.Key + "\\" + "\\EditorInfo2.xml", settings))
                {

                    writer.WriteStartElement("Editor"); //This is the root of the XML                    
                    writer.WriteElementString("EditorName",  editor.Key); //This is all misc editor data.
                    writer.WriteElementString("EditorGraphic", editor.Value.EditorGraphic); //This is the name of the file that this editor uses.

                    writer.WriteStartElement("EditorFile"); //Info about the file used for the main data of the editor.
                    writer.WriteElementString("EditorFilePath",    editor.Value.EditorFilePath);
                    writer.WriteElementString("EditorTableStart",  editor.Value.EditorTableStart.ToString());
                    writer.WriteElementString("EditorTableRowSize",  editor.Value.EditorTableRowSize.ToString());
                    writer.WriteEndElement(); // End EditorFile

                    writer.WriteStartElement("NameTableFile"); //Info about the file referenced for & table information about the editor's Name List.
                    writer.WriteElementString("NameTableFilePath", editor.Value.NameTableFilePath);
                    writer.WriteElementString("NameTableCharacterSet", editor.Value.NameTableCharacterSet);
                    writer.WriteElementString("NameTableStart", editor.Value.NameTableStart.ToString());
                    writer.WriteElementString("NameTableTextSize", editor.Value.NameTableTextSize.ToString()); 
                    writer.WriteElementString("NameTableRowSize", editor.Value.NameTableRowSize.ToString());                    
                    writer.WriteElementString("NameTableItemCount", editor.Value.NameTableItemCount.ToString()); //How many names / items are in the collection this editor edits. (Like weapons, spells, etc)
                    writer.WriteEndElement(); // End NameFile

                    writer.WriteStartElement("ExtraTableList");
                    foreach (ExtraTable ExtraTable in editor.Value.ExtraTableList) 
                    {
                        writer.WriteStartElement("ExtraTable");
                        writer.WriteElementString("ExtraTableName", ExtraTable.ExtraTableName);
                        writer.WriteElementString("ExtraTableCharacterSet", ExtraTable.ExtraTableCharacterSet);
                        writer.WriteElementString("ExtraTableFilePath", ExtraTable.ExtraTableFilePath);
                        writer.WriteElementString("ExtraTableStart", ExtraTable.ExtraTableStart.ToString());
                        writer.WriteElementString("ExtraTableTextSize", ExtraTable.ExtraTableTextSize.ToString());
                        writer.WriteElementString("ExtraTableRowSize", ExtraTable.ExtraTableRowSize.ToString());
                        writer.WriteEndElement(); // End ExtraTable
                    }
                    writer.WriteEndElement(); // End ExtraTables

                    editor.Value.LeftBar.SearchBar.Text = ""; //needed because otherwise it saves literally only the visible items in the treeview, but i still need it to be based on treeview for order.
                    //If you want to test removing the search bar cleansing, make a backup of the workshop first!
                    writer.WriteStartElement("LeftBar");  //The left bar holds the collection, and any items and folders inside it.
                    foreach (TreeViewItem TreeItem in editor.Value.LeftBar.TreeView.Items)
                    {
                        ItemInfo data = TreeItem.Tag as ItemInfo;

                        writer.WriteStartElement("CollectionItem");
                        if (editor.Value.NameTableFilePath == null || editor.Value.NameTableFilePath == "" || data.ItemType == "Folder") { writer.WriteElementString("ItemName", data.ItemName);}
                        //writer.WriteElementString("ItemName", data.ItemName); //if (editor.Value.NameFilePath == null || editor.Value.NameFilePath == "") { writer.WriteElementString("ItemName", data.ItemName);}
                        
                        writer.WriteElementString("ItemIndex", data.ItemIndex.ToString());
                        writer.WriteElementString("ItemType", data.ItemType);
                        writer.WriteElementString("ItemTooltip", data.ItemTooltip);
                        writer.WriteElementString("ItemColor", data.ItemColor);
                        writer.WriteElementString("ItemNote", data.ItemNote);
                                                
                        if (data.ItemType == "Folder" && TreeItem.HasItems) // A "Folder" is a item that expands, just like a windows folder!
                        {                            
                            foreach (TreeViewItem childItem in TreeItem.Items)
                            {
                                ItemInfo childData = childItem.Tag as ItemInfo;

                                writer.WriteStartElement("ChildItem");
                                if (editor.Value.NameTableFilePath == null || editor.Value.NameTableFilePath == "" || childData.ItemType == "Folder") { writer.WriteElementString("ItemName", childData.ItemName); }
                                writer.WriteElementString("ItemIndex", childData.ItemIndex.ToString());
                                writer.WriteElementString("ItemType", childData.ItemType);
                                writer.WriteElementString("ItemTooltip", childData.ItemTooltip);
                                writer.WriteElementString("ItemColor", childData.ItemColor);
                                writer.WriteElementString("ItemNote", childData.ItemNote);                                
                                writer.WriteEndElement(); // End ChildItem
                            }
                        }

                        writer.WriteEndElement(); //End CollectionItem  
                    }
                    writer.WriteEndElement(); //End LeftBar
                    
                    writer.WriteStartElement("PageList"); //Start of the main editor. Here, every page, row, column, entry, and all main data is saved.
                    foreach (var page in editor.Value.PageList)
                    {
                        writer.WriteStartElement("Page");
                        writer.WriteElementString("PageName", page.PageName);

                        foreach (var row in page.RowList)
                        {
                            writer.WriteStartElement("Row");
                            writer.WriteElementString("RowName", row.RowName);


                            foreach (var column in row.ColumnList)
                            {
                                writer.WriteStartElement("Column");
                                writer.WriteElementString("ColumnName", column.ColumnName);


                                foreach (var entry in column.EntryList)
                                {
                                    writer.WriteStartElement("Entry");
                                    writer.WriteElementString("EntryName", entry.EntryName);
                                    writer.WriteElementString("EntryTooltip", entry.EntryTooltip);
                                    writer.WriteElementString("EntrySaveState", entry.EntrySaveState.ToString());
                                    writer.WriteElementString("EntryLabelShown", entry.EntryLabelShown.ToString());                                    
                                    writer.WriteElementString("EntryGameTableSize", entry.EntryGameTableSize.ToString());
                                    writer.WriteElementString("EntryByteOffset", entry.EntryByteOffset.ToString());
                                    writer.WriteElementString("EntryByteSize", entry.EntryByteSize);
                                    writer.WriteElementString("EntryByteSizeNum", entry.EntryByteSizeNum.ToString());
                                    writer.WriteElementString("EntryMainType", entry.EntryMainType);
                                    writer.WriteElementString("EntrySubType", entry.EntrySubType);


                                    if (entry.EntrySubType == "NumberBox") 
                                    {
                                        writer.WriteStartElement("EntryTypeNumberBox");
                                        writer.WriteElementString("NumberSign", entry.EntryTypeNumberBox.NumberSign.ToString());
                                        writer.WriteEndElement(); //End NumberBox 
                                    }
                                                     

                                    if (entry.EntrySubType == "CheckBox")
                                    {
                                        writer.WriteStartElement("EntryTypeCheckBox");
                                        writer.WriteElementString("CheckBoxTrueText", entry.EntryTypeCheckBox.CheckBoxTrueText.ToString());
                                        writer.WriteElementString("CheckBoxFalseText", entry.EntryTypeCheckBox.CheckBoxFalseText.ToString());
                                        writer.WriteElementString("CheckBoxTrueValue", entry.EntryTypeCheckBox.CheckBoxTrueValue.ToString());
                                        writer.WriteElementString("CheckBoxFalseValue", entry.EntryTypeCheckBox.CheckBoxFalseValue.ToString());
                                        writer.WriteEndElement(); //End CheckBox 
                                    }


                                    if (entry.EntrySubType == "BitFlag")
                                    {
                                        writer.WriteStartElement("EntryTypeBitFlag");
                                        writer.WriteElementString("BitFlag1Name", entry.EntryTypeBitFlag.BitFlag1Name.ToString());
                                        writer.WriteElementString("BitFlag1CheckText", entry.EntryTypeBitFlag.BitFlag1CheckText.ToString());
                                        writer.WriteElementString("BitFlag1UncheckText", entry.EntryTypeBitFlag.BitFlag1UncheckText.ToString());
                                        writer.WriteElementString("BitFlag2Name", entry.EntryTypeBitFlag.BitFlag2Name.ToString());
                                        writer.WriteElementString("BitFlag2CheckText", entry.EntryTypeBitFlag.BitFlag2CheckText.ToString());
                                        writer.WriteElementString("BitFlag2UncheckText", entry.EntryTypeBitFlag.BitFlag2UncheckText.ToString());
                                        writer.WriteElementString("BitFlag3Name", entry.EntryTypeBitFlag.BitFlag3Name.ToString());
                                        writer.WriteElementString("BitFlag3CheckText", entry.EntryTypeBitFlag.BitFlag3CheckText.ToString());
                                        writer.WriteElementString("BitFlag3UncheckText", entry.EntryTypeBitFlag.BitFlag3UncheckText.ToString());
                                        writer.WriteElementString("BitFlag4Name", entry.EntryTypeBitFlag.BitFlag4Name.ToString());
                                        writer.WriteElementString("BitFlag4CheckText", entry.EntryTypeBitFlag.BitFlag4CheckText.ToString());
                                        writer.WriteElementString("BitFlag4UncheckText", entry.EntryTypeBitFlag.BitFlag4UncheckText.ToString());
                                        writer.WriteElementString("BitFlag5Name", entry.EntryTypeBitFlag.BitFlag5Name.ToString());
                                        writer.WriteElementString("BitFlag5CheckText", entry.EntryTypeBitFlag.BitFlag5CheckText.ToString());
                                        writer.WriteElementString("BitFlag5UncheckText", entry.EntryTypeBitFlag.BitFlag5UncheckText.ToString());
                                        writer.WriteElementString("BitFlag6Name", entry.EntryTypeBitFlag.BitFlag6Name.ToString());
                                        writer.WriteElementString("BitFlag6CheckText", entry.EntryTypeBitFlag.BitFlag6CheckText.ToString());
                                        writer.WriteElementString("BitFlag6UncheckText", entry.EntryTypeBitFlag.BitFlag6UncheckText.ToString());
                                        writer.WriteElementString("BitFlag7Name", entry.EntryTypeBitFlag.BitFlag7Name.ToString());
                                        writer.WriteElementString("BitFlag7CheckText", entry.EntryTypeBitFlag.BitFlag7CheckText.ToString());
                                        writer.WriteElementString("BitFlag7UncheckText", entry.EntryTypeBitFlag.BitFlag7UncheckText.ToString());
                                        writer.WriteElementString("BitFlag8Name", entry.EntryTypeBitFlag.BitFlag8Name.ToString());
                                        writer.WriteElementString("BitFlag8CheckText", entry.EntryTypeBitFlag.BitFlag8CheckText.ToString());
                                        writer.WriteElementString("BitFlag8UncheckText", entry.EntryTypeBitFlag.BitFlag8UncheckText.ToString());
                                        writer.WriteEndElement(); //End BitFlag    
                                    }



                                    if (entry.EntrySubType == "List") //A List can have upto 65000 options PER list entry. (2 bytes)
                                    {
                                        writer.WriteStartElement("EntryTypeList");
                                        writer.WriteElementString("ListSize", entry.EntryTypeList.ListSize.ToString());


                                        writer.WriteStartElement("ItemList");
                                        for (int i = 0; i < entry.EntryTypeList.ListItems.Length; i++)
                                        {
                                            if (!string.IsNullOrEmpty(entry.EntryTypeList.ListItems[i]))
                                            {
                                                string listItem = $"{i}: {entry.EntryTypeList.ListItems[i]}";
                                                writer.WriteElementString("Item", listItem);
                                            }
                                        }
                                        writer.WriteEndElement(); //End ItemList


                                        writer.WriteEndElement(); //End EntryListType    
                                    }
                                    
                                   

                                    writer.WriteEndElement(); //End Entry
                                }
                                writer.WriteEndElement(); //End Column
                            }
                            writer.WriteEndElement(); //End Row
                        }
                        writer.WriteEndElement(); //End Page
                    }
                    writer.WriteEndElement(); //End PageList  
                    writer.WriteEndElement(); //End Editor  AKA the Root of the XML   
                    writer.Flush(); //Ends the XML GameFile

                } //End of using XmlWriter

                System.IO.File.Delete(TheWorkshop.ExePath + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + editor.Key + "\\" + "\\EditorInfo.xml");
                System.IO.File.Move(TheWorkshop.ExePath + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + editor.Key + "\\" + "\\EditorInfo2.xml", TheWorkshop.ExePath + ExtraPath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + editor.Key + "\\" + "\\EditorInfo.xml");
                

            } //End of foreach (database)


            //This commented section wants to use the EditorsToDelete list declared at the start to delete any editors that should nolonger exist.
            //Unfortunately i keep getting access forbidden errors / crash, so this is commented while i work on other parts of the program.
            //as this bug is not a very big deal if users know about it and i can fix it after open beta.


            //string[] EditorFolderNames = Directory.GetDirectories(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors").Select(Path.GetFileName).ToArray();
            //foreach (string FolderName in EditorFolderNames)
            //{
            //    using (var fs = new FileStream(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + FolderName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
            //    {
            //        File.Delete(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + FolderName);
            //    }

            //    //if (!EditorsToDelete.Contains(FolderName))
            //    //{
            //    //    System.IO.File.Delete(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + FolderName);
            //    //}
            //}

            //foreach (string name in EditorsToDelete) 
            //{
            //    System.IO.File.Delete(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Editors\\" + name);
            //}
            //Delete any folders not in name list?

        } //End of SaveAllEditors (Method)                

        public void SaveWorkshopInfo(Database Database, Workshop Workshop, string ExtraPath)
        {
            //This is like saving editors, but it saves information on the list of files the workshop actually uses.
            
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(Workshop.ExePath + ExtraPath + "\\Workshops\\" + Workshop.WorkshopName + "\\WorkshopInfo.xml", settings))
            {
                writer.WriteStartElement("Root");

                writer.WriteStartElement("EditorList");
                foreach (DockPanel DockPanel in Workshop.EditorBar.Children.OfType<DockPanel>())
                {
                    Editor EditorData = DockPanel.Tag as Editor;
                    writer.WriteElementString("EditorName", EditorData.EditorName); //This is all misc editor data.
                }
                writer.WriteEndElement(); //End EditorList


                writer.WriteStartElement("WorkshopInfo");
                foreach (KeyValuePair<string, GameFile> WorkshopFile in Database.GameFiles) 
                {
                    writer.WriteStartElement("File");
                    writer.WriteElementString("FileName", WorkshopFile.Value.FileName);
                    writer.WriteElementString("FileNickName", WorkshopFile.Value.FileNickName);
                    writer.WriteElementString("FilePath", WorkshopFile.Value.FilePath);
                    writer.WriteEndElement(); //End WorkshopFile
                }
                writer.WriteEndElement(); //End FileList

                writer.WriteEndElement(); //End Root

                writer.Flush(); //Ends the XML GameFile
            }

        }


        public void SaveGameFiles(Database Database, Workshop TheWorkshop)
        {
            //I need to make it so for the entrys of the currently selected item, if a entry is edited, that it is saved.
            //Currently the user can just swap items back and forth to save to MemoryFile, and then here MemoryFile saves to your PC.
            foreach (KeyValuePair<string, GameFile> HFile in Database.GameFiles)
            {
                File.WriteAllBytes(TheWorkshop.OutputDirectory + HFile.Value.FilePath, HFile.Value.FileBytes); //saves to the path i set, everything in the array.
            }

           
        }



        




            






        

    }// End of Save Workshop (Class)

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Crystal_Editor
{
    class NewEditor
    {
        

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
            PageClass.RowList= new();
            EditorClass.PageList.Add(PageClass);

            Row RowClass = new();
            RowClass.RowName = "New Row";
            RowClass.ColumnList = new();
            PageClass.RowList.Add(RowClass);

            Column ColumnClass = new();
            ColumnClass.ColumnName = "New Column";
            ColumnClass.EntryList= new();
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

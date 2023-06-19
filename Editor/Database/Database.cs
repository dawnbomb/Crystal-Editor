using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Xml;

namespace Crystal_Editor
{

    // This is basically a massive list of most variables to do with a workshop.
    // More specificly, it works a workshop into editors, then stores everything about every single editor.


    public class Database
    {
        //List of (Editor names as strings) (but actually the strings will be...lists or dictionaries?)
        public Dictionary<string, Editor>? GameEditors; //Note that "Editors" is a folder name. So this is called "GameEditors".
        public Dictionary<string, GameFile>? GameFiles; //the term "File" prevents File.Read from working because it thinks "File" is a class. So i Needed another name.
        public Dictionary<string, Graphic>? Graphics;

        public void StartUp()
        {            
            GameEditors = new Dictionary<string, Editor>();
            GameFiles   = new Dictionary<string, GameFile>();
        }

    }
       

    public class GameFile //aka Database.GameEditors[X].
    {
        public string FileName { get; set; } //XML The actual name of the file
        public string FilePath { get; set; } //XML The path from InputDirectory to the FileName.
        public string FileNickName { get; set; } //XML. For games that have multiple files with identical names via being in diffrent folders, users can give files a nickname, but save as their real name.
        public byte[] FileBytes { get; set; } //Not XML  //The actual loaded information from a file. This is what editors edit! This is the ENTIRE file, not just a table of bytes.
        //FileBytes is also reffered to in comments as "Memory File" due to it being a version of the file entirely in memory, and not saved back to the computer.
        //"Memory File" helps differenciate between the file's origonal bytes, and it's current ones after editing but before saving.
    }

    public class Graphic 
    {
        public string GraphicName { get; set; }
        public string GraphicPath { get; set; }
    }

    

    public class Editor //XML (The Name goes to XML)
    {
        public GameFile EditorFile { get; set; } //Just a shortcut to the file, to cleanup code, and possibly processing time? IDK im to dumb to get what causes lag.
        public string EditorName { get; set; } //Not XML (yet)
        public string EditorType { get; set; } //XML
        public string EditorGraphic { get; set; } //XML
        

        //Game File chunk. The "Game File" is the file that has the main table of information this editor is editing. Like weapon stats, spell stats, item stats, etc.
        public string EditorFilePath { get; set; } //XML does nothing now. Later used to to also find the correct gamefile. For games with multiple files with the same name.
        public int EditorTableStart { get; set; } //XML Says what byte a table of information starts at. 
        public int EditorTableRowSize { get; set; } //XML  Determines the size of the table, IE how many bytes are in 1 row of the table.


        //Name File chunk. The "Name File" has the list of names that tells the user what their editing. This is optional, as users can instead input a manual name list.
        public GameFile NameTableFile { get; set; } //XML? The file that contains the ItemNames for this editors Collection Tree.
        public string NameTableFilePath { get; set; } //XML
        public string NameTableCharacterSet { get; set; } //XML The name of the Cypher used to Decrypt from Hex to English. (A=1, B=2, etc). Google what a Cypher is for more information.
        public int NameTableStart { get; set; } //XML
        public int NameTableTextSize { get; set; } //XML
        public int NameTableRowSize { get; set; } //XML
        public int NameTableItemCount { get; set; } //XML  How many items are in the collection. It determines how many rows of data the editor reads. For manual name lists, the number is always 0.
        //The name table item count only happens for editors getting names from file. Any function relying on this will break when using a manual name list.

        public List<ExtraTable>? ExtraTableList { get; set; }

        //The rest of the class
        public DockPanel? EditorDockPanel { get; set; }   //The back of the editor. A user should not see this, used just for organization.
        public DockPanel EditorRightDockPanel { get; set; } //The left side is the left bar, and this is the right side.
        public Button EditorButton { get; set; } //Used in delete editor, and rename editor.
        public DockPanel EditorBarDockPanel { get; set; }
        public Label EditorNameLabel { get; set; }
        public TreeViewItem EditorTreeViewitem { get; set; }
        public LeftBar? LeftBar { get; set; }
        public TopBar? TopBar { get; set; }
        public List<Page>? PageList { get; set; }

        // The following things are only used to keep track stuff that i'd rather not be here but 
        // because C# blocks ref variables inside click delegate events i need somewhere to store them. :(
        // They are not saved to XML or reloaded 
        public DockPanel? PageCurrent { get; set; }
        public Entry SelectedEntry { get; set; } //The entry the user is currently selecting. In DEV mode, This entry is highlighted.
        public int TableRowIndex { get; set; } //Not XML, used to save data when changing items in a collection. It's equal to an ItemInfo's Index.
        public ScrollViewer ScrollViewer { get; set; }
        public DockPanel ScrollPanel { get; set; }

        public Page BanishedPage { get; set; }
        
    }

    public class ExtraTable //Extra tables live encode / decode to file bytes instead of doing it on save, so they will always be accurate,
    { // even if multiple editors want the same information to appear and be editable. :)
        public GameFile ExtraTableFile { get; set; }
        public string ExtraTableName { get; set; }
        public string ExtraTableFilePath { get; set; }
        public string ExtraTableCharacterSet { get; set; }
        public int ExtraTableStart { get; set; }
        public int ExtraTableTextSize { get; set; }
        public int ExtraTableRowSize { get; set; }
        public TextBox ExtraTableTextBox {get; set;}
        public bool ExtraTableEncodeIsEnabled { get; set; }
    }

    public class LeftBar
    {
        public DockPanel? LeftBarDockPanel { get; set; }
        public TreeView TreeView { get; set; } //The tree view itself of the editor. As a reminder every editor has its own tree view.
        public List<ItemInfo> ItemList { get; set; } //i kinda forget.
        public TextBox LeftBarNameBox { get; set; } //The textbox for editing the item's name.
        public TextBox LeftBarNameBoxExtra { get; set; } //The textbox for creating a note for that item.

        public TextBox SearchBar { get; set; }

    }

    public class ItemInfo 
    {
                
        public string ItemName { get; set; } //XML
        public int ItemIndex { get; set; } //XML Basically, the row number in a table. Note: Folders have a index of 0.
        public string ItemType { get; set; }  //XML null or "Folder"
        public string? ItemTooltip { get; set; } //XML
        public string ItemColor { get; set; } //XML
        public string ItemNote { get; set; } //XML


        

    }

    public class TopBar
    {
        public DockPanel TopPanel { get; set; }
        public DockPanel PageBar { get; set; }
        public TextBox ItemNoteBox { get; set; }

        public TextBox EntryNoteBox { get; set; }

    }

    public class Page
    {
        public string? PageName { get; set; } //XML I think this is the name of a page button.
        public DockPanel? DockPanel { get; set; }
        public List<Row>? RowList { get; set; }
        
        
    }

    public class Row
    {
        public string RowName { get; set; } //XML the name of a row.
        public DockPanel? RowDockPanel { get; set; }
        public List<Column>? ColumnList { get; set; }
        public Label RowLabel { get; set; }
        public Page RowPage { get; set; }
    }

    public class Column
    {
        public string ColumnName { get; set; } //XML The name of a column.
        public DockPanel? ColumnGrid { get; set; }
        public List<Entry>? EntryList { get; set; }
        public Label ColumnLabel { get; set; }
        public Row ColumnRow { get; set; }
    }

    public class Entry
    {
        public string EntryName { get; set; }  //XML //The Name / Label an entry Gets. Later, it will default to "???"  
        public string EntryTooltip { get; set; } //XML
        public string EntrySaveState { get; set; } //XML  -Enabled or Disabled (AutoDisabled?)//Decides If entry can save to Memory File. Occurs when the byte is also in use by the NameTable or any ExtraTables.        
        public string EntryLabelShown { get; set; }  //XML //Yes or No, Defaults to Yes
        public string EntryMainType { get; set; }  //XML  -Basic  Currently unused, placeholder for future feature expansion for other types of entrys.
        public string EntrySubType { get; set; }  //XML //5 Types: NumberBox, Flag, BitFlag, Dropdown & List. Much later on: TextBox Type.
        //public int ByteStarting { get; set; } //This number is how many bytes from the start of a file the editor begins. Each entry keeps track of it's own, because some editors have more then 1 file.
        public int EntryGameTableSize { get; set; }  //XML //Literally the same as EditorClass.GameTableSize. I'm keeping it here as i might need it for future features to be backwards compatable.
        public int EntryByteOffset { get; set; }  //XML //This number is how many bytes into a row this entry edits. This is unique for every entry.
        
        public string EntryByteSize { get; set; } //XML - 1, 2L, 2B, 4L, 4B
        public int EntryByteSizeNum { get; set; } //XML the size as a number. Due to the number of references it was annoying not to make it a dedicated piece of info. I could technically remove it later.
        public string EntryByteDecimal { get; set; } //NOT XML  //Needed to deal with the true value of a checkbox or bitflag. 
        


        public Border EntryBorder { get; set; } //The border around a entry,
        public DockPanel? EntryDockPanel { get; set; } //The entrys Grid, visable to the used, and contains lots of information.
        public Label EntryPrefix { get; set; }   //Used to show the byte offset to a user. Useful when creating an editor, and you don't know what things do yet.
        public Label? EntryLabel { get; set; } //The name of an entry, appears on it's grid.


        public Editor EntryEditor { get; set; }
        public Page EntryPage { get; set; }
        public Row EntryRow { get; set; }
        public Column EntryColumn { get; set; }

        //The grid/Dockpanel? The labels and buttons to later delete?




        public EntryTypeNumberBox EntryTypeNumberBox { get; set; }
        public EntryTypeCheckBox EntryTypeCheckBox { get; set; }
        public EntryTypeBitFlag EntryTypeBitFlag { get; set; }
        public EntryTypeList EntryTypeList { get; set; }


    }

    

    public class EntryTypeBitFlag 
    {
        public DockPanel BitFlagsDockPanel { get; set; } // Used to hold the various Bigflags inside it.


        public DockPanel BitFlag1 { get; set; }
        public Label     BitFlag1Label { get; set; }
        public string    BitFlag1Name { get; set; } //XML
        public Button    BitFlag1CheckBox { get; set; }
        public string    BitFlag1CheckText { get; set; } //XML
        public string    BitFlag1UncheckText { get; set; } //XML
        public DockPanel BitFlag2 { get; set; }
        public Label     BitFlag2Label { get; set; }
        public string    BitFlag2Name { get; set; } //XML
        public Button    BitFlag2CheckBox { get; set; }
        public string    BitFlag2CheckText { get; set; } //XML
        public string    BitFlag2UncheckText { get; set; } //XML
        public DockPanel BitFlag3 { get; set; }
        public Label     BitFlag3Label { get; set; }
        public string    BitFlag3Name { get; set; } //XML
        public Button    BitFlag3CheckBox { get; set; }
        public string    BitFlag3CheckText { get; set; } //XML
        public string    BitFlag3UncheckText { get; set; } //XML
        public DockPanel BitFlag4 { get; set; }
        public Label     BitFlag4Label { get; set; }
        public string    BitFlag4Name { get; set; } //XML
        public Button    BitFlag4CheckBox { get; set; }
        public string    BitFlag4CheckText { get; set; } //XML
        public string    BitFlag4UncheckText { get; set; } //XML
        public DockPanel BitFlag5 { get; set; }
        public Label     BitFlag5Label { get; set; }
        public string    BitFlag5Name { get; set; } //XML
        public Button    BitFlag5CheckBox { get; set; }
        public string    BitFlag5CheckText { get; set; } //XML
        public string    BitFlag5UncheckText { get; set; } //XML
        public DockPanel BitFlag6 { get; set; }
        public Label     BitFlag6Label { get; set; }
        public string    BitFlag6Name { get; set; } //XML
        public Button    BitFlag6CheckBox { get; set; }
        public string    BitFlag6CheckText { get; set; } //XML
        public string    BitFlag6UncheckText { get; set; } //XML
        public DockPanel BitFlag7 { get; set; }
        public Label     BitFlag7Label { get; set; }
        public string    BitFlag7Name { get; set; } //XML
        public Button    BitFlag7CheckBox { get; set; }
        public string    BitFlag7CheckText { get; set; } //XML
        public string    BitFlag7UncheckText { get; set; } //XML
        public DockPanel BitFlag8 { get; set; }
        public Label     BitFlag8Label { get; set; }
        public string    BitFlag8Name { get; set; } //XML
        public Button    BitFlag8CheckBox { get; set; }
        public string    BitFlag8CheckText { get; set; } //XML
        public string    BitFlag8UncheckText { get; set; } //XML
    }

    

    public class EntryTypeNumberBox
    {
        public TextBox? NumberBoxTextBox { get; set; }
        public string NumberSign { get; set; } //XML - This determines if a numbersbox only accepts positive numbers, or both Positive and Negative.
        public bool NumberBoxCanSave { get; set; } //NOT XML //Makes it so number boxes do not save to MemoryFile when changing the selected Item in the current Collection.
    }

    public class EntryTypeCheckBox
    {

        public Button CheckBoxButton { get; set; }
        public int? CheckBoxTrueValue { get; set; }  //XML - Value a checkbox uses for checked. 
        public int? CheckBoxFalseValue { get; set; }//XML - Value a checkbox uses for unchecked. 
        public string CheckBoxTrueText { get; set; } //XML  The text that appears when the checkbox is true (IE checked)
        public string CheckBoxFalseText { get; set; } //XML The text that appears when the checkbox is false (IE not checked)


    }


    public class EntryTypeList
    {
        public Button ListButton { get; set; }
        public int ListSize { get; set; } //XML
        public string[] ListItems { get; set; } //XML
    }






























}

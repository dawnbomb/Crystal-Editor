using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Crystal_Editor
{
    class CharacterSetManager
    {
        //This file handles decoding and encoding text in the Item List, and any Extra Tables.
        //It changes from Hex to english or other languages, and those languages back to hex.
        
        //Maybe I should have a function that decodes just one string, and then the plural version command just runs the single version X times?
                

        public void Decode(Workshop TheWorkshop, Editor EditorClass, string Doing)
        {
            
            string Cypher = EditorClass.NameTableCharacterSet;

            Encoding encoding;
            if (Cypher == "ASCII+ANSI"){encoding = Encoding.ASCII;}
            else if (Cypher == "Shift-JIS"){encoding = Encoding.GetEncoding("shift_jis");}
            else{ return;}



            if (Doing == "Items")
            {
                foreach (var Item in EditorClass.LeftBar.ItemList)
                {
                    if (Item.ItemType != "Folder")
                    {
                        byte[] bytes = new byte[EditorClass.NameTableTextSize];
                        for (int RowIndex = 0; RowIndex < EditorClass.NameTableTextSize; RowIndex++)
                        {
                            bytes[RowIndex] = EditorClass.NameTableFile.FileBytes[EditorClass.NameTableStart + (Item.ItemIndex * EditorClass.NameTableRowSize) + RowIndex];
                        }
                        Item.ItemName = encoding.GetString(bytes);
                    }
                }
            }

            

            int ET = 0;
            //TreeViewItem Item2 = EditorClass.LeftBar.TreeView.SelectedItem.
                     
            
        }

        public void DecodeExtras(Workshop TheWorkshop, Editor EditorClass) 
        {
            if (TheWorkshop.IsPreviewMode == true) { return;  }
            

            ItemInfo Item2 = (EditorClass.LeftBar.TreeView.SelectedItem as TreeViewItem)?.Tag as ItemInfo;

            foreach (ExtraTable ExtraTable in EditorClass.ExtraTableList)
            {                

                Encoding encoding;
                if (ExtraTable.ExtraTableCharacterSet == "ASCII+ANSI") { encoding = Encoding.ASCII; }
                else if (ExtraTable.ExtraTableCharacterSet == "Shift-JIS") { encoding = Encoding.GetEncoding("shift_jis"); }
                else { return; }

                byte[] bytes = new byte[ExtraTable.ExtraTableTextSize];
                for (int RowIndex = 0; RowIndex < ExtraTable.ExtraTableTextSize; RowIndex++)
                {
                    bytes[RowIndex] = ExtraTable.ExtraTableFile.FileBytes[ExtraTable.ExtraTableStart + (Item2.ItemIndex * ExtraTable.ExtraTableRowSize) + RowIndex];
                }

                ExtraTable.ExtraTableEncodeIsEnabled = false;
                ExtraTable.ExtraTableTextBox.Text = encoding.GetString(bytes).TrimEnd('\0');
                ExtraTable.ExtraTableEncodeIsEnabled = true;

                //NameBox.Text = data.ItemName.TrimEnd('\0');
            }

            
        }





        public void Encode(Workshop TheWorkshop, Editor EditorClass, string Doing, ItemInfo ItemInfo = null)
        {
            

            string Cypher = "";
            if (Doing == "Item")
            {
                Cypher = EditorClass.NameTableCharacterSet;
            }
            

            if (Doing == "Item")
            {
                Encoding encoding;

                if (Cypher == "ASCII+ANSI")
                {
                    encoding = Encoding.ASCII;
                }
                else if (Cypher == "Shift-JIS")
                {
                    encoding = Encoding.GetEncoding("shift_jis");
                }
                else
                {
                    throw new InvalidOperationException("Unsupported character set");
                }

                //string TheText = TheWorkshop.PropertiesItemTextboxName.Text.PadRight(EditorClass.NameTableRowSize, '\0');
                string TheText = ItemInfo.ItemName.PadRight(EditorClass.NameTableTextSize, '\0');
                byte[] bytes = encoding.GetBytes(TheText);

                for (int i = 0; i < EditorClass.NameTableTextSize; i++)
                {
                    ByteManager.ByteWriter(bytes[i], EditorClass.NameTableFile.FileBytes, EditorClass.NameTableStart + (EditorClass.TableRowIndex * EditorClass.NameTableRowSize) + i);
                }
            }
            
            
        }


        public void EncodeExtra(Workshop TheWorkshop, Editor EditorClass, ExtraTable ExtraTable) 
        {
            string Cypher = ExtraTable.ExtraTableCharacterSet;

            Encoding encoding;
            if (Cypher == "ASCII+ANSI"){encoding = Encoding.ASCII;}
            else if (Cypher == "Shift-JIS"){ encoding = Encoding.GetEncoding("shift_jis"); }
            else { return; } //make this throw an error notification?
            

            string TheText = ExtraTable.ExtraTableTextBox.Text.PadRight(ExtraTable.ExtraTableTextSize, '\0');
            byte[] bytes = encoding.GetBytes(TheText);

            for (int i = 0; i < ExtraTable.ExtraTableTextSize; i++)
            {
                ByteManager.ByteWriter(bytes[i], ExtraTable.ExtraTableFile.FileBytes, ExtraTable.ExtraTableStart + (EditorClass.TableRowIndex * ExtraTable.ExtraTableRowSize) + i);
            }
                       


        }


    }
}

using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Net;
using System.Windows.Navigation;
//using System.Windows.Forms;
//using System.Drawing;

namespace Crystal_Editor
{
    internal class EntryManager
    {
        //This is a very complicated file to understand, feel free to ask for help.
        //In here, we take hex from a file, and load it into a entry as decimal, based on what type of entry it is (Number, checkbox / flag, bitflag, list)
        //I plan to add dropdown menus.


        //This comment is for myself mostly.

        //B! .TryParse(Name! .Text, out type! value! ); { Form1.ByteWriter( value! , enemydata_array, StartingHex! + (treeView1.SelectedNode.Index * RowSize! ) + ArrayOfset! ); }
        //B=Byte size, 1="Byte" 2="UInt16" 4="UInt32"         Name!=the name of the textbox        type!= byte / ushort / uint, only include this in the FIRST time this ever happens in a form, later copies ommit this!
        //value!=Name of variable that holds the byte type (so byte/short/int has diffrent names)       StartingHex!= The hex offset      RowSize!=How many bytes in a row    Arrayofset! = how far into the row do we start grabbing info or saving it
        //Byte.TryParse(textBoxLv.Text, out byte value8); { Form1.ByteWriter(value8, enemydata_array, 104 + (enemyTree.SelectedNode.Index * 96) + 96); } //First 1 byte save
                
        public void LoadEntry(Workshop TheWorkshop, Editor EditorClass, Entry EntryClass) 
        {
            //This method simply takes the byte(s) from MemoryFile the entry controls,
            //converts them from Hex to Decimal, and loads that number into EditorClass.EntryByteDecimal.
            //Afterward (Down below) it loads the the entry with that information.

            if (TheWorkshop.IsPreviewMode == true) { return; }

            if (EntryClass.EntryByteSize == "1") 
            { 
                EntryClass.EntryByteDecimal = EditorClass.EditorFile.FileBytes[EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset].ToString("D"); 
            }
            if (EntryClass.EntryByteSize == "2B")
            {
                EntryClass.EntryByteDecimal = BitConverter.ToUInt16(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset).ToString("D");
            }
            if (EntryClass.EntryByteSize == "4B")
            {
                EntryClass.EntryByteDecimal = BitConverter.ToUInt32(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset).ToString("D");
            }
            if (EntryClass.EntryByteSize == "2L") 
            {
                ushort value2 = BitConverter.ToUInt16(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset);
                ushort swappedValue2 = (ushort)IPAddress.HostToNetworkOrder((short)value2); // Swap the endianness
                EntryClass.EntryByteDecimal = swappedValue2.ToString("D");
            }
            if (EntryClass.EntryByteSize == "4L") 
            {
                uint value = BitConverter.ToUInt32(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset);
                byte[] valueBytes = BitConverter.GetBytes(value);
                Array.Reverse(valueBytes);
                uint swappedValue = BitConverter.ToUInt32(valueBytes, 0);
                EntryClass.EntryByteDecimal = swappedValue.ToString("D");
            }
            



            if (EntryClass.EntrySubType == "NumberBox") { LoadNumberBox(EntryClass); }
            if (EntryClass.EntrySubType == "CheckBox")  { LoadCheckBox(EntryClass); }
            if (EntryClass.EntrySubType == "BitFlag")   { LoadBitFlag(EntryClass); }
            if (EntryClass.EntrySubType == "List")      { LoadList(EntryClass); }
        }

        public void SaveEntry(Editor EditorClass, Entry EntryClass)
        {
            if (EntryClass.EntryByteSize != "0") 
            {
                //This Method takes the number in EntryClass.EntryByteDecimal, converts it from Decimal to Hex, then saves it to the correct file location.
                //This is the only way anything is saved to MemoryFile / eventually actual file.
                //This is triggered whenever any entry's module is changed in any way.


                //if (EntryClass.EntryType == "NumberBox") { SaveNumberBox(EntryClass); }
                //if (EntryClass.EntryType == "CheckBox")  { SaveCheckBox(EntryClass); }
                //If (EntryClass.EntryType == "BitFlag")  { SaveBitFlag(EntryClass); }
                //If Dropdown
                //if (EntryClass.EntryType == "List") { SaveList(EntryClass); }


                //Checks if the entry allows saving.
                //Creators may want to disable specific entrys from being editable if changing them causes the game to crash.
                //There could be other uses as well. All entrys default to true.
                if (EntryClass.EntrySaveState == "Enabled") 
                {
                    //If 1/2/4/r2/r4 Bytes...  

                    if (EntryClass.EntryByteSize == "1")  //This is saving 1 Byte Size?   //First 1 byte save
                    {
                        //Thing that loads       -----------------The Hex GameFile---------------------------  ---Starting Byte--- --The Tree--   --Row Size----  --Offset into a row-- --To Decimal--               
                        Byte.TryParse(EntryClass.EntryByteDecimal, out byte value8);
                        { ByteManager.ByteWriter(value8, EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset); }
                    }
                    if (EntryClass.EntryByteSize == "2L")
                    {
                        UInt16.TryParse(EntryClass.EntryByteDecimal, out ushort value16);
                        value16 = (ushort)IPAddress.HostToNetworkOrder((short)value16); // Swap the endianness
                        { ByteManager.ByteWriter(value16, EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset); } //First 2 byte save


                    }
                    if (EntryClass.EntryByteSize == "4L")
                    {
                        UInt32.TryParse(EntryClass.EntryByteDecimal, out uint value32);
                        byte[] valueBytes = BitConverter.GetBytes(value32); // Swap the endianness
                        Array.Reverse(valueBytes); // Swap the endianness
                        value32 = BitConverter.ToUInt32(valueBytes, 0); // Swap the endianness
                        { ByteManager.ByteWriter(value32, EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset); } //First 4 byte save

                    }
                    if (EntryClass.EntryByteSize == "2B")
                    {
                        UInt16.TryParse(EntryClass.EntryByteDecimal, out ushort value16);
                        { ByteManager.ByteWriter(value16, EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset); } //First 2 byte save
                    }
                    if (EntryClass.EntryByteSize == "4B")
                    {
                        UInt32.TryParse(EntryClass.EntryByteDecimal, out uint value32);
                        { ByteManager.ByteWriter(value32, EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset); } //First 4 byte save
                    }
                }    



            }//End IF ByteSize !=0
        }


        public void LoadNumberBox(Entry EntryClass)
        {
            EntryClass.EntryTypeNumberBox.NumberBoxTextBox.Text = EntryClass.EntryByteDecimal;

            if (EntryClass.EntryTypeNumberBox.NumberSign == "Negative")
            {
                long longValue;
                if (long.TryParse(EntryClass.EntryByteDecimal, out longValue))
                {
                    if (EntryClass.EntryByteSizeNum == 1 && longValue > 127)
                    {
                        EntryClass.EntryTypeNumberBox.NumberBoxTextBox.Text = (longValue - 256).ToString();
                    }
                    if (EntryClass.EntryByteSizeNum == 2 && longValue > 32767)
                    {
                        EntryClass.EntryTypeNumberBox.NumberBoxTextBox.Text = (longValue - 65536).ToString();
                    }
                    if (EntryClass.EntryByteSizeNum == 4 && longValue > 2147483647)
                    {
                        EntryClass.EntryTypeNumberBox.NumberBoxTextBox.Text = (longValue - 4294967296).ToString();
                    }
                }
            }
        }


        public void LoadCheckBox(Entry EntryClass) 
        {
            if (EntryClass.EntryByteDecimal == EntryClass.EntryTypeCheckBox.CheckBoxTrueValue.ToString())
            {
                EntryClass.EntryTypeCheckBox.CheckBoxButton.Content = EntryClass.EntryTypeCheckBox.CheckBoxTrueText; // = UserCheck
            }
            else if (EntryClass.EntryByteDecimal == EntryClass.EntryTypeCheckBox.CheckBoxFalseValue.ToString())
            {
                EntryClass.EntryTypeCheckBox.CheckBoxButton.Content = EntryClass.EntryTypeCheckBox.CheckBoxFalseText; //= UserUncheck
            }
            else
            {
                EntryClass.EntryTypeCheckBox.CheckBoxButton.Content = "??? " + EntryClass.EntryByteDecimal + " ";
            }
        }

        public void LoadBitFlag(Entry EntryClass)
        {
            int Num = Int32.Parse(EntryClass.EntryByteDecimal);

            if (Num > 127) { EntryClass.EntryTypeBitFlag.BitFlag8CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag8CheckText; Num = Num - 128; } else { EntryClass.EntryTypeBitFlag.BitFlag8CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag8UncheckText; } //Flag 0/128
            if (Num > 63) { EntryClass.EntryTypeBitFlag.BitFlag7CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag7CheckText; Num = Num - 64; } else { EntryClass.EntryTypeBitFlag.BitFlag7CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag7UncheckText; }  //Flag 0/64
            if (Num > 31) { EntryClass.EntryTypeBitFlag.BitFlag6CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag6CheckText; Num = Num - 32; } else { EntryClass.EntryTypeBitFlag.BitFlag6CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag6UncheckText; }  //Flag 0/32
            if (Num > 15) { EntryClass.EntryTypeBitFlag.BitFlag5CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag5CheckText; Num = Num - 16; } else { EntryClass.EntryTypeBitFlag.BitFlag5CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag5UncheckText; }  //Flag 0/16
            if (Num > 7) { EntryClass.EntryTypeBitFlag.BitFlag4CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag4CheckText; Num = Num - 8; } else { EntryClass.EntryTypeBitFlag.BitFlag4CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag4UncheckText; }  //Flag 0/8
            if (Num > 3) { EntryClass.EntryTypeBitFlag.BitFlag3CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag3CheckText; Num = Num - 4; } else { EntryClass.EntryTypeBitFlag.BitFlag3CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag3UncheckText; }  //Flag 0/4
            if (Num > 1) { EntryClass.EntryTypeBitFlag.BitFlag2CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag2CheckText; Num = Num - 2; } else { EntryClass.EntryTypeBitFlag.BitFlag2CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag2UncheckText; }  //Flag 0/2
            if (Num > 0) { EntryClass.EntryTypeBitFlag.BitFlag1CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag1CheckText; Num = Num - 1; } else { EntryClass.EntryTypeBitFlag.BitFlag1CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag1UncheckText; }  //Flag 0/1

            EntryClass.EntryTypeBitFlag.BitFlag1Label.Content = EntryClass.EntryTypeBitFlag.BitFlag1Name;
            EntryClass.EntryTypeBitFlag.BitFlag2Label.Content = EntryClass.EntryTypeBitFlag.BitFlag2Name;
            EntryClass.EntryTypeBitFlag.BitFlag3Label.Content = EntryClass.EntryTypeBitFlag.BitFlag3Name;
            EntryClass.EntryTypeBitFlag.BitFlag4Label.Content = EntryClass.EntryTypeBitFlag.BitFlag4Name;
            EntryClass.EntryTypeBitFlag.BitFlag5Label.Content = EntryClass.EntryTypeBitFlag.BitFlag5Name;
            EntryClass.EntryTypeBitFlag.BitFlag6Label.Content = EntryClass.EntryTypeBitFlag.BitFlag6Name;
            EntryClass.EntryTypeBitFlag.BitFlag7Label.Content = EntryClass.EntryTypeBitFlag.BitFlag7Name;
            EntryClass.EntryTypeBitFlag.BitFlag8Label.Content = EntryClass.EntryTypeBitFlag.BitFlag8Name;
        }


        public void LoadList(Entry EntryClass)
        {
            EntryClass.EntryTypeList.ListButton.Content = EntryClass.EntryByteDecimal + ": " + EntryClass.EntryTypeList.ListItems[Int32.Parse(EntryClass.EntryByteDecimal)];
        }




        //public void SaveNumberBox(Entry EntryClass)
        //{            
        //    EntryClass.EntryByteDecimal = EntryClass.NumberBox.Text;

        //    if (EntryClass.NumberSign == "Negative")
        //    {
        //        if (EntryClass.ByteSizeNum == 1) { if(Int32.Parse(EntryClass.NumberBox.Text) < 0) {EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.NumberBox.Text) + 256).ToString()       ;}}
        //        if (EntryClass.ByteSizeNum == 2) { if(Int32.Parse(EntryClass.NumberBox.Text) < 0) {EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.NumberBox.Text) + 65536).ToString()     ;}}
        //        if (EntryClass.ByteSizeNum == 4) { if(Int32.Parse(EntryClass.NumberBox.Text) < 0) {EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.NumberBox.Text) + 4294967296).ToString();}}
        //    }
        //}




        //public void SaveCheckBox(Entry EntryClass) 
        //{
        //    if (EntryClass.CheckBox.Content == EntryClass.CheckBoxTrueText)
        //    {                
        //        EntryClass.EntryByteDecimal = EntryClass.CheckBoxTrueValue.ToString();
        //    }
        //    if (EntryClass.CheckBox.Content == EntryClass.CheckBoxFalseText)
        //    {                
        //        EntryClass.EntryByteDecimal = EntryClass.CheckBoxFalseValue.ToString();
        //    }
        //}



        //public void SaveBitFlag(Entry EntryClass)
        //{
        //    int Num = 0;
        //    if (EntryClass.BitFlag1CheckBox.Content == EntryClass.BitFlag1CheckText) { Num = Num + 1; }
        //    if (EntryClass.BitFlag2CheckBox.Content == EntryClass.BitFlag2CheckText) { Num = Num + 2; }
        //    if (EntryClass.BitFlag3CheckBox.Content == EntryClass.BitFlag3CheckText) { Num = Num + 4; }
        //    if (EntryClass.BitFlag4CheckBox.Content == EntryClass.BitFlag4CheckText) { Num = Num + 8; }
        //    if (EntryClass.BitFlag5CheckBox.Content == EntryClass.BitFlag5CheckText) { Num = Num + 16; }
        //    if (EntryClass.BitFlag6CheckBox.Content == EntryClass.BitFlag6CheckText) { Num = Num + 32; }
        //    if (EntryClass.BitFlag7CheckBox.Content == EntryClass.BitFlag7CheckText) { Num = Num + 64; }
        //    if (EntryClass.BitFlag8CheckBox.Content == EntryClass.BitFlag8CheckText) { Num = Num + 128; }
        //    EntryClass.EntryByteDecimal = Num.ToString();

        //}

        //public void SaveList(Entry EntryClass) 
        //{
        //    string input = (string)EntryClass.ListButton.Content;
        //    string[] parts = input.Split(':');
        //    string number = parts[0].Trim();
        //    EntryClass.EntryByteDecimal = number; // Console.WriteLine(number); // Output: 24

        //}












        //////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////Create Entrys///////////////////////////////////////







        public void CreateNumberBox(Workshop TheWorkshop, Entry EntryClass)
        {
            // Default properties if new
            if (EntryClass.EntryTypeNumberBox == null)
            {
                EntryClass.EntryTypeNumberBox = new();
                EntryClass.EntryTypeNumberBox.NumberSign = "Positive";
            }

            // Default properties end

            bool FirstTime = true;

            TextBox NumberBox = new TextBox();
            NumberBox.Height = 25;
            NumberBox.MinWidth = 50;
            NumberBox.FontSize = 17;
            NumberBox.Margin = new Thickness(0, 0, 3, 0); // Left Top Right Bottom
            NumberBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            EntryClass.EntryDockPanel.Children.Add(NumberBox);
            EntryClass.EntryTypeNumberBox.NumberBoxTextBox = NumberBox;
            if (EntryClass.EntrySaveState == "Disabled")
            {
                NumberBox.IsEnabled = false;
            }

            NumberBox.PreviewMouseDown += (sender, e) =>
            {
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);
            };
            NumberBox.PreviewTextInput += (sender, e) =>
            {
                // Check if the entered character is a number
                if (!char.IsDigit(e.Text, e.Text.Length - 1))
                {
                    e.Handled = true; // If not a number, mark the event as handled
                }
            };

            NumberBox.TextChanged += (sender, e) =>
            {
                if (FirstTime == false)
                {
                    if (EntryClass.EntryTypeNumberBox.NumberBoxCanSave == true)
                    {
                        long longValue;
                        if (long.TryParse(NumberBox.Text, out longValue))
                        {
                            if (EntryClass.EntryTypeNumberBox.NumberSign == "Positive")
                            {
                                if (EntryClass.EntryByteSizeNum == 1)
                                {
                                    longValue = Math.Clamp(longValue, 0, 255);
                                }
                                else if (EntryClass.EntryByteSizeNum == 2)
                                {
                                    longValue = Math.Clamp(longValue, 0, 65535);
                                }
                                else if (EntryClass.EntryByteSizeNum == 4)
                                {
                                    longValue = Math.Clamp(longValue, 0, uint.MaxValue);
                                }
                            }
                            else if (EntryClass.EntryTypeNumberBox.NumberSign == "Negative")
                            {
                                if (EntryClass.EntryByteSizeNum == 1)
                                {
                                    longValue = Math.Clamp(longValue, -128, 127);
                                }
                                else if (EntryClass.EntryByteSizeNum == 2)
                                {
                                    longValue = Math.Clamp(longValue, -32768, 32767);
                                }
                                else if (EntryClass.EntryByteSizeNum == 4)
                                {
                                    longValue = Math.Clamp(longValue, int.MinValue, int.MaxValue);
                                }
                            }

                            NumberBox.Text = longValue.ToString();
                            EntryClass.EntryByteDecimal = longValue.ToString();
                            TheWorkshop.DebugBox.Text = EntryClass.EntryByteDecimal.ToString();

                            if (EntryClass == EntryClass.EntryEditor.SelectedEntry)
                            {
                                SaveEntry(EntryClass.EntryEditor, EntryClass);
                                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);
                            }
                        }
                    }
                }
                else
                {
                    FirstTime = false;
                }
            };

            NumberBox.TextInput += (sender, e) =>
            {
                // TheWorkshop.DebugBox.Text = "WTF";
            };
        }


        

        public void CreateCheckBox(Workshop TheWorkshop, Entry EntryClass)
        {

            //Default properties if new
            if (EntryClass.EntryTypeCheckBox == null)
            {
                EntryClass.EntryTypeCheckBox = new();
                EntryClass.EntryTypeCheckBox.CheckBoxTrueValue = 1;
                EntryClass.EntryTypeCheckBox.CheckBoxFalseValue = 0;
                EntryClass.EntryTypeCheckBox.CheckBoxTrueText = "✔";
                EntryClass.EntryTypeCheckBox.CheckBoxFalseText = " ";
            }
            //Default properties end

            Button CheckBox = new Button();
            CheckBox.MinWidth = 30;
            CheckBox.Height = 24;
            CheckBox.Margin = new Thickness(0, 0, 3, 0); // Left Top Right Bottom 
            CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            EntryClass.EntryDockPanel.Children.Add(CheckBox);
            if (EntryClass.EntrySaveState == "Disabled")
            {
                CheckBox.IsEnabled = false;
            }
            CheckBox.Click += delegate
            {


                if (EntryClass.EntryByteDecimal == EntryClass.EntryTypeCheckBox.CheckBoxTrueValue.ToString())
                {
                    CheckBox.Content = EntryClass.EntryTypeCheckBox.CheckBoxFalseText;
                    EntryClass.EntryByteDecimal = EntryClass.EntryTypeCheckBox.CheckBoxFalseValue.ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                else if (EntryClass.EntryByteDecimal == EntryClass.EntryTypeCheckBox.CheckBoxFalseValue.ToString())
                {
                    CheckBox.Content = EntryClass.EntryTypeCheckBox.CheckBoxTrueText;
                    EntryClass.EntryByteDecimal = EntryClass.EntryTypeCheckBox.CheckBoxTrueValue.ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }

                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

            };
            EntryClass.EntryTypeCheckBox.CheckBoxButton = CheckBox;
            //EntryClass.EntryType
            //Add a new property option, to deicde if text used says On/Off or Yes/No or Custom?
            //User can also customize the color of the button (+ based on user color mode?).
            //A few options like this go a long way for user flexability!
        }

        public void CreateBitFlag(Workshop TheWorkshop, Entry EntryClass)
        {

            //Default properties if new
            if (EntryClass.EntryTypeBitFlag == null)
            {
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
            }
            //Default properties end


            DockPanel BitFlags = new();
            //BitFlags.Background = Brushes.Crimson;
            int BitFlagBoxHeight = 24;
            int BitMinWidth = 33;
            var BitMargin = new Thickness(0, 0, 3, 0); // Left Top Right Bottom 
            var DockMargin = new Thickness(0, 0, 0, 3); // Left Top Right Bottom 
            ////////////////////////////////////////////////
            DockPanel BitFlag1 = new();
            DockPanel.SetDock(BitFlag1, Dock.Top);
            BitFlag1.Margin = DockMargin;

            Label BitFlag1Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag1Name == null) { EntryClass.EntryTypeBitFlag.BitFlag1Name = "Bit 1"; }
            BitFlag1Label.Content = EntryClass.EntryTypeBitFlag.BitFlag1Name;
            BitFlag1Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag1CheckBox = new Button();
            BitFlag1CheckBox.MinWidth = BitMinWidth;
            BitFlag1CheckBox.Height = BitFlagBoxHeight;
            BitFlag1CheckBox.Margin = BitMargin;
            BitFlag1CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.EntrySaveState == "Disabled")
            {
                BitFlag1CheckBox.IsEnabled = false;
            }
            BitFlag1CheckBox.Click += delegate
            {


                if (BitFlag1CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag1CheckText.ToString())
                {
                    BitFlag1CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag1UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 1).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                else if (BitFlag1CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag1UncheckText.ToString())
                {
                    BitFlag1CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag1CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 1).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }

                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag2 = new();
            DockPanel.SetDock(BitFlag2, Dock.Top);
            BitFlag2.Margin = DockMargin;

            Label BitFlag2Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag2Name == null) { EntryClass.EntryTypeBitFlag.BitFlag2Name = "Bit 2"; }
            BitFlag2Label.Content = EntryClass.EntryTypeBitFlag.BitFlag2Name;
            BitFlag2Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag2CheckBox = new Button();
            BitFlag2CheckBox.MinWidth = BitMinWidth;
            BitFlag2CheckBox.Height = BitFlagBoxHeight;
            BitFlag2CheckBox.Margin = BitMargin;
            BitFlag2CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.EntrySaveState == "Disabled")
            {
                BitFlag2CheckBox.IsEnabled = false;
            }
            BitFlag2CheckBox.Click += delegate
            {


                if (BitFlag2CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag2CheckText.ToString())
                {
                    BitFlag2CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag2UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 2).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);                    
                }
                else if (BitFlag2CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag2UncheckText.ToString())
                {
                    BitFlag2CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag2CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 2).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag3 = new();
            DockPanel.SetDock(BitFlag3, Dock.Top);
            BitFlag3.Margin = DockMargin;

            Label BitFlag3Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag3Name == null) { EntryClass.EntryTypeBitFlag.BitFlag3Name = "Bit 3"; }
            BitFlag3Label.Content = EntryClass.EntryTypeBitFlag.BitFlag3Name;
            BitFlag3Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag3CheckBox = new Button();
            BitFlag3CheckBox.MinWidth = BitMinWidth;
            BitFlag3CheckBox.Height = BitFlagBoxHeight;
            BitFlag3CheckBox.Margin = BitMargin; 
            BitFlag3CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.EntrySaveState == "Disabled")
            {
                BitFlag3CheckBox.IsEnabled = false;
            }
            BitFlag3CheckBox.Click += delegate
            {


                if (BitFlag3CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag3CheckText.ToString())
                {
                    BitFlag3CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag3UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 4).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                else if (BitFlag3CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag3UncheckText.ToString())
                {
                    BitFlag3CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag3CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 4).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag4 = new();
            DockPanel.SetDock(BitFlag4, Dock.Top);
            BitFlag4.Margin = DockMargin;

            Label BitFlag4Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag4Name == null) { EntryClass.EntryTypeBitFlag.BitFlag4Name = "Bit 4"; }
            BitFlag4Label.Content = EntryClass.EntryTypeBitFlag.BitFlag4Name;
            BitFlag4Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag4CheckBox = new Button();
            BitFlag4CheckBox.MinWidth = BitMinWidth;
            BitFlag4CheckBox.Height = BitFlagBoxHeight;
            BitFlag4CheckBox.Margin = BitMargin;
            BitFlag4CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.EntrySaveState == "Disabled")
            {
                BitFlag4CheckBox.IsEnabled = false;
            }
            BitFlag4CheckBox.Click += delegate
            {


                if (BitFlag4CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag4CheckText.ToString())
                {
                    BitFlag4CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag4UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 8).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                else if (BitFlag4CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag4UncheckText.ToString())
                {
                    BitFlag4CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag4CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 8).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag5 = new();
            DockPanel.SetDock(BitFlag5, Dock.Top);
            BitFlag5.Margin = DockMargin;

            Label BitFlag5Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag5Name == null) { EntryClass.EntryTypeBitFlag.BitFlag5Name = "Bit 5"; }
            BitFlag5Label.Content = EntryClass.EntryTypeBitFlag.BitFlag5Name;
            BitFlag5Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag5CheckBox = new Button();
            BitFlag5CheckBox.MinWidth = BitMinWidth;
            BitFlag5CheckBox.Height = BitFlagBoxHeight;
            BitFlag5CheckBox.Margin = BitMargin;
            BitFlag5CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.EntrySaveState == "Disabled")
            {
                BitFlag5CheckBox.IsEnabled = false;
            }
            BitFlag5CheckBox.Click += delegate
            {


                if (BitFlag5CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag5CheckText.ToString())
                {
                    BitFlag5CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag5UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 16).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                else if (BitFlag5CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag5UncheckText.ToString())
                {
                    BitFlag5CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag5CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 16).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag6 = new();
            DockPanel.SetDock(BitFlag6, Dock.Top);
            BitFlag6.Margin = DockMargin;

            Label BitFlag6Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag6Name == null) { EntryClass.EntryTypeBitFlag.BitFlag6Name = "Bit 6"; }
            BitFlag6Label.Content = EntryClass.EntryTypeBitFlag.BitFlag6Name;
            BitFlag6Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag6CheckBox = new Button();
            BitFlag6CheckBox.MinWidth = BitMinWidth;
            BitFlag6CheckBox.Height = BitFlagBoxHeight;
            BitFlag6CheckBox.Margin = BitMargin;
            BitFlag6CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.EntrySaveState == "Disabled")
            {
                BitFlag6CheckBox.IsEnabled = false;
            }
            BitFlag6CheckBox.Click += delegate
            {


                if (BitFlag6CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag6CheckText.ToString())
                {
                    BitFlag6CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag6UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 32).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                else if (BitFlag6CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag6UncheckText.ToString())
                {
                    BitFlag6CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag6CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 32).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag7 = new();
            DockPanel.SetDock(BitFlag7, Dock.Top);
            BitFlag7.Margin = DockMargin;

            Label BitFlag7Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag7Name == null) { EntryClass.EntryTypeBitFlag.BitFlag7Name = "Bit 7"; }
            BitFlag7Label.Content = EntryClass.EntryTypeBitFlag.BitFlag7Name;
            BitFlag7Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag7CheckBox = new Button();
            BitFlag7CheckBox.MinWidth = BitMinWidth;
            BitFlag7CheckBox.Height = BitFlagBoxHeight;
            BitFlag7CheckBox.Margin = BitMargin;
            BitFlag7CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.EntrySaveState == "Disabled")
            {
                BitFlag7CheckBox.IsEnabled = false;
            }
            BitFlag7CheckBox.Click += delegate
            {


                if (BitFlag7CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag7CheckText.ToString())
                {
                    BitFlag7CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag7UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 64).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                else if (BitFlag7CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag7UncheckText.ToString())
                {
                    BitFlag7CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag7CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 64).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

            };
            ////////////////////////////////////////////////
            DockPanel BitFlag8 = new();
            DockPanel.SetDock(BitFlag8, Dock.Top);
            BitFlag8.Margin = DockMargin;

            Label BitFlag8Label = new();
            if (EntryClass.EntryTypeBitFlag.BitFlag8Name == null) { EntryClass.EntryTypeBitFlag.BitFlag8Name = "Bit 8"; }
            BitFlag8Label.Content = EntryClass.EntryTypeBitFlag.BitFlag8Name;
            BitFlag8Label.HorizontalAlignment = HorizontalAlignment.Left;

            Button BitFlag8CheckBox = new Button();
            BitFlag8CheckBox.MinWidth = BitMinWidth;
            BitFlag8CheckBox.Height = BitFlagBoxHeight;
            BitFlag8CheckBox.Margin = BitMargin;
            BitFlag8CheckBox.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.EntrySaveState == "Disabled")
            {
                BitFlag8CheckBox.IsEnabled = false;
            }
            BitFlag8CheckBox.Click += delegate
            {


                if (BitFlag8CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag8CheckText.ToString())
                {
                    BitFlag8CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag8UncheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) - 128).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                else if (BitFlag8CheckBox.Content == EntryClass.EntryTypeBitFlag.BitFlag8UncheckText.ToString())
                {
                    BitFlag8CheckBox.Content = EntryClass.EntryTypeBitFlag.BitFlag8CheckText;
                    EntryClass.EntryByteDecimal = (Int32.Parse(EntryClass.EntryByteDecimal) + 128).ToString();
                    SaveEntry(EntryClass.EntryEditor, EntryClass);
                }
                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);
            };
            ////////////////////////////////////////////////

            EntryClass.EntryDockPanel.Children.Add(BitFlags);

            EntryClass.EntryTypeBitFlag.BitFlagsDockPanel = BitFlags;
            EntryClass.EntryTypeBitFlag.BitFlag1 = BitFlag1;
            EntryClass.EntryTypeBitFlag.BitFlag2 = BitFlag2;
            EntryClass.EntryTypeBitFlag.BitFlag3 = BitFlag3;
            EntryClass.EntryTypeBitFlag.BitFlag4 = BitFlag4;
            EntryClass.EntryTypeBitFlag.BitFlag5 = BitFlag5;
            EntryClass.EntryTypeBitFlag.BitFlag6 = BitFlag6;
            EntryClass.EntryTypeBitFlag.BitFlag7 = BitFlag7;
            EntryClass.EntryTypeBitFlag.BitFlag8 = BitFlag8;

            EntryClass.EntryTypeBitFlag.BitFlag1Label = BitFlag1Label;
            EntryClass.EntryTypeBitFlag.BitFlag2Label = BitFlag2Label;
            EntryClass.EntryTypeBitFlag.BitFlag3Label = BitFlag3Label;
            EntryClass.EntryTypeBitFlag.BitFlag4Label = BitFlag4Label;
            EntryClass.EntryTypeBitFlag.BitFlag5Label = BitFlag5Label;
            EntryClass.EntryTypeBitFlag.BitFlag6Label = BitFlag6Label;
            EntryClass.EntryTypeBitFlag.BitFlag7Label = BitFlag7Label;
            EntryClass.EntryTypeBitFlag.BitFlag8Label = BitFlag8Label;

            EntryClass.EntryTypeBitFlag.BitFlag1CheckBox = BitFlag1CheckBox;
            EntryClass.EntryTypeBitFlag.BitFlag2CheckBox = BitFlag2CheckBox;
            EntryClass.EntryTypeBitFlag.BitFlag3CheckBox = BitFlag3CheckBox;
            EntryClass.EntryTypeBitFlag.BitFlag4CheckBox = BitFlag4CheckBox;
            EntryClass.EntryTypeBitFlag.BitFlag5CheckBox = BitFlag5CheckBox;
            EntryClass.EntryTypeBitFlag.BitFlag6CheckBox = BitFlag6CheckBox;
            EntryClass.EntryTypeBitFlag.BitFlag7CheckBox = BitFlag7CheckBox;
            EntryClass.EntryTypeBitFlag.BitFlag8CheckBox = BitFlag8CheckBox;

            BitFlags.Children.Add(BitFlag1);
            BitFlag1.Children.Add(BitFlag1Label);
            BitFlag1.Children.Add(BitFlag1CheckBox);

            BitFlags.Children.Add(BitFlag2);
            BitFlag2.Children.Add(BitFlag2Label);
            BitFlag2.Children.Add(BitFlag2CheckBox);

            BitFlags.Children.Add(BitFlag3);
            BitFlag3.Children.Add(BitFlag3Label);
            BitFlag3.Children.Add(BitFlag3CheckBox);

            BitFlags.Children.Add(BitFlag4);
            BitFlag4.Children.Add(BitFlag4Label);
            BitFlag4.Children.Add(BitFlag4CheckBox);

            BitFlags.Children.Add(BitFlag5);
            BitFlag5.Children.Add(BitFlag5Label);
            BitFlag5.Children.Add(BitFlag5CheckBox);

            BitFlags.Children.Add(BitFlag6);
            BitFlag6.Children.Add(BitFlag6Label);
            BitFlag6.Children.Add(BitFlag6CheckBox);

            BitFlags.Children.Add(BitFlag7);
            BitFlag7.Children.Add(BitFlag7Label);
            BitFlag7.Children.Add(BitFlag7CheckBox);

            BitFlags.Children.Add(BitFlag8);
            BitFlag8.Children.Add(BitFlag8Label);
            BitFlag8.Children.Add(BitFlag8CheckBox);

            //BitFlag4.Width = BitFlags.Width;
            //Create a right aligned dockpanel, then Grids inside it going down, that themself have the label and Checkbox of each bit.

            //Note: Only a 1 byte MyEntry can turn into a bitflag

            //Editing it goes into properties. (As in, the user can change the name label of each flag
            //Editing might also include a option, for a custom flag graphic for each?

            //A Bitflag is 8 Flags (Checkboxes) in 1 Column, Docked Top, alignment right. (So 8 of these checkboxes are going down the right side)                
            //Thats it? Due to a bitflag always being the same checked/unchecked values, this seems kinda easy.
            //(Other then needing a good looking default checked / unchecked graphic)


            //A bunch of IF statements can exist, working backwards.
            //0: int Num = Byte Hex2Dec;
            //1: If Num >= 128, Flag 8 = on, and Num -128
            //2: If Num >= 64,  Flag 7 = on, and Num -64
            //3: If Num >= 32,  Flag 6 = on, and Num -32
            //4: If Num >= 16,  Flag 5 = on, and Num -16
            //5: If Num >= 8,   Flag 4 = on, and Num -8
            //6: If Num >= 4,   Flag 3 = on, and Num -4
            //7: If Num >= 2,   Flag 2 = on, and Num -2
            //8: If Num >= 1,   Flag 1 = on, and Num -1
            //Tada! the flags are not properly set for the user! 
            //Clicking a flag changes Num and the Bytes value up or down, based on if it's turning On or Off.
            //and thats it!
        }

        public void CreateDropDown(Entry EntryClass)
        {
            //Editing it goes into properties
            //This might be really hard, i don't know how to change a combo boxes item values dynamically!

            //I guess this would function similuarly to lists, but the user selects from a dropdown menu, so refer to lists to get the idea?
        }

        public void CreateList(Entry EntryClass, Workshop TheWorkshop)
        {

            //Default properties if new
            if (EntryClass.EntryTypeList == null)
            {
                EntryClass.EntryTypeList = new();
            }
            //Default properties end
            

            if (EntryClass.EntryTypeList.ListItems != null)
            {
                if (EntryClass.EntryByteSizeNum == 1)
                {
                    string[] items = EntryClass.EntryTypeList.ListItems;
                    Array.Resize(ref items, 256);
                    EntryClass.EntryTypeList.ListItems = items;
                    EntryClass.EntryTypeList.ListSize = 256;
                }
                if (EntryClass.EntryByteSizeNum == 2)
                {
                    string[] items = EntryClass.EntryTypeList.ListItems;
                    Array.Resize(ref items, 65536);
                    EntryClass.EntryTypeList.ListItems = items;
                    EntryClass.EntryTypeList.ListSize = 65536;
                }
            }

            if (EntryClass.EntryTypeList.ListItems == null) 
            {
                if (EntryClass.EntryByteSizeNum == 1) 
                {
                    EntryClass.EntryTypeList.ListItems = new string[256];
                    EntryClass.EntryTypeList.ListSize = 256;
                }
                if (EntryClass.EntryByteSizeNum == 2)
                {
                    EntryClass.EntryTypeList.ListItems = new string[65536];
                    EntryClass.EntryTypeList.ListSize = 65536;
                }
                //ListItems = new string[256],
            }

            

            Button Button = new();
            Button.MinWidth = 100;
            Button.Height = 24;
            Button.Margin = new Thickness(0, 0, 3, 0); // Left Top Right Bottom 
            Button.HorizontalAlignment = HorizontalAlignment.Right;
            if (EntryClass.EntrySaveState == "Disabled")
            {
                Button.IsEnabled = false;
            }
            EntryClass.EntryDockPanel.Children.Add(Button);
            EntryClass.EntryTypeList.ListButton = Button;
            Button.Click += (sender, e) =>
            {
                
                TheWorkshop.EntryClass = EntryClass;
                TheWorkshop.ButtonListEditSave.Content = "Edit";// Update button content and visibility
                TheWorkshop.ListPanelEdit.Visibility = Visibility.Collapsed;

                EntryBecomeActive(EntryClass);
                UpdateEntryProperties(TheWorkshop, EntryClass.EntryEditor);

                TheWorkshop.EntryListBox.SelectionChanged -= TheWorkshop.EntryListBox_SelectionChanged; // Remove event handler      
                TheWorkshop.EntryListBox.Items.Clear(); // Clear items
                for (int i = 0; i < EntryClass.EntryTypeList.ListItems.Length; i++)
                {
                    if (!string.IsNullOrEmpty(EntryClass.EntryTypeList.ListItems[i]))
                    {
                        string itemText = i + ": " + EntryClass.EntryTypeList.ListItems[i];
                        TheWorkshop.EntryListBox.Items.Add(itemText);
                    }
                }
                TheWorkshop.EntryListBox.SelectionChanged += TheWorkshop.EntryListBox_SelectionChanged; // Re-attach event handler                
                

                Window parentWindow = Window.GetWindow(Button); //Open the list tab for the user (obviously the user wants this)
                TabControl tabControl = parentWindow.FindName("TabSet") as TabControl;
                foreach (TabItem tabItem in tabControl.Items)
                {
                    if (tabItem.Header != null && tabItem.Header.ToString() == "Lists")
                    {
                        tabItem.IsSelected = true;
                        //TheWorkshop.DebugBox.Text = "Hai";
                        break;
                    }
                }

                

            };
        }





        ///////////////////////////////////////////////////END OF ENTRYS//////////////////////////////////////////////////////////














        //////////////////////////////////////////////START OF ENTRY TYPE SWAP////////////////////////////////////////////////////


        public void EntryChange(Database Database, string NewEntryType, Workshop TheWorkshop, Entry EntryClass)
        {

            //This chunk is commented out as a lazy way to ensure disabled entrys actually become disabled.
            //if (EntryClass.EntrySubType == NewEntryType) //This check isn't to stop anything, just save processing power, but it could be deleted if desired.
            //{
            //    return;
            //}

            
            //I may crash as i build this, as i attempt to delete a thing that already doesn't exist?
            if (EntryClass.EntrySubType == "NumberBox")
            {
                EntryClass.EntryDockPanel.Children.Remove(EntryClass.EntryTypeNumberBox.NumberBoxTextBox);

            }

            if (EntryClass.EntrySubType == "CheckBox")
            {
                EntryClass.EntryDockPanel.Children.Remove(EntryClass.EntryTypeCheckBox.CheckBoxButton);

            }

            if (EntryClass.EntrySubType == "BitFlag")
            {
                EntryClass.EntryDockPanel.Children.Remove(EntryClass.EntryTypeBitFlag.BitFlagsDockPanel);

            }

            if (EntryClass.EntrySubType == "List")
            {
                EntryClass.EntryDockPanel.Children.Remove(EntryClass.EntryTypeList.ListButton);

            }

            //How do i actually destroy something, such that it also isn't in memory, and isn't just "removed"?
            //Does the garbage collector automatically get it? or not?
            //NumBox.Parent.Children.Remove(NumBox);
            //NumBox.Dispose();
            //NumBox = null; 

            //////////////////////Deleting MyEntry modules//////////////////////////////
            //////////////////////Creating MyEntry modules//////////////////////////////




            if (NewEntryType == "NumberBox") //Step X: Create new Entry Module using Entry.ByteD and any other data relevant to this.
            {
                CreateNumberBox(TheWorkshop, EntryClass);
                LoadNumberBox(EntryClass);
                EntryClass.EntrySubType = "NumberBox";

            }


            if (NewEntryType == "CheckBox")
            {
                CreateCheckBox(TheWorkshop, EntryClass);
                LoadCheckBox(EntryClass);
                EntryClass.EntrySubType = "CheckBox";
            }



            if (NewEntryType == "BitFlag")
            {
                CreateBitFlag(TheWorkshop, EntryClass);
                LoadBitFlag(EntryClass);
                EntryClass.EntrySubType = "BitFlag";
            }



            if (NewEntryType == "DropDown")
            {
                CreateDropDown(EntryClass);
                EntryClass.EntrySubType = "DropDown";
            }



            if (NewEntryType == "List")
            {
                CreateList(EntryClass, TheWorkshop);
                LoadList(EntryClass);
                EntryClass.EntrySubType = "List";
            }
            
        }




        public void EntryBecomeActive(Entry EntryClass) 
        {
            Editor EditorClass = EntryClass.EntryEditor;
            if (Properties.Settings.Default.DeveloperMode == "Developer") //If dev mode is on, Open properties, and Uncolor previous entry.
            {
                Window parentWindow = Window.GetWindow(EntryClass.EntryDockPanel);
                TabControl tabControl = parentWindow.FindName("TabSet") as TabControl;
                foreach (TabItem tabItem in tabControl.Items)
                {
                    if (tabItem.Header != null && tabItem.Header.ToString() == "Properties")
                    {
                        tabItem.IsSelected = true;
                        break;
                    }
                }

                if (EditorClass.SelectedEntry != null)
                {
                    EditorClass.SelectedEntry.EntryBorder.ClearValue(Border.BackgroundProperty);
                    EditorClass.SelectedEntry.EntryBorder.ClearValue(Border.BorderBrushProperty);
                    //EditorClass.SelectedEntry.EntryDockPanel.Background = null;
                }
            }
            EditorClass.SelectedEntry = EntryClass; //Note: This MUST be here, After clear EntryClass color, and before set EntryClass Color.
            if (Properties.Settings.Default.DeveloperMode == "Developer") //If dev mode is on, Color this entry.
            {
                EditorClass.SelectedEntry.EntryBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#35383C")); //364448  232528
                EditorClass.SelectedEntry.EntryBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999")); //999999  3E8B8E
                if (EntryClass.EntrySaveState == "Disabled") //If dev mode is on, Color this entry.
                {
                    EditorClass.SelectedEntry.EntryBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A272B")); //483137   2C1C20
                    EditorClass.SelectedEntry.EntryBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#793F72")); //915968
                }
            }
        }


        public void UpdateEntryProperties(Workshop TheWorkshop ,Editor EditorClass) 
        {
            if (TheWorkshop.IsPreviewMode == true) { return; }

            Entry EntryClass = EditorClass.SelectedEntry;


            TheWorkshop.PropertiesHide();
            TheWorkshop.EntryProperties.Visibility = Visibility.Visible;
            TheWorkshop.PropertiesNameBox.Text = EntryClass.EntryName;
            TheWorkshop.EditorClass = EntryClass.EntryEditor;
            TheWorkshop.PageClass = EntryClass.EntryPage;
            TheWorkshop.RowClass = EntryClass.EntryRow;
            TheWorkshop.ColumnClass = EntryClass.EntryColumn;
            TheWorkshop.EntryClass = EntryClass;
            


            


            /////////////////////Data Analyzer/////////////////////
            TheWorkshop.PropertiesEntry1Byte.Text = EditorClass.EditorFile.FileBytes[EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset].ToString("D");
            TheWorkshop.PropertiesEntryHex1Byte.Text = EditorClass.EditorFile.FileBytes[EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset].ToString("X2");

            TheWorkshop.PropertiesEntry2ByteB.Text = BitConverter.ToUInt16(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset).ToString("D");
            TheWorkshop.PropertiesEntryHex2ByteB.Text = BitConverter.ToUInt16(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset).ToString("X4");
            TheWorkshop.PropertiesEntry4ByteB.Text = BitConverter.ToUInt32(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset).ToString("D");
            TheWorkshop.PropertiesEntryHex4ByteB.Text = BitConverter.ToUInt32(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset).ToString("X8");
                       

            ushort value2 = BitConverter.ToUInt16(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset);
            ushort swappedValue2 = (ushort)IPAddress.HostToNetworkOrder((short)value2); // Swap the endianness
            TheWorkshop.PropertiesEntry2ByteL.Text = swappedValue2.ToString("D"); // Convert the swapped value4 to a string using the desired format
            TheWorkshop.PropertiesEntryHex2ByteL.Text = swappedValue2.ToString("X4"); // Convert the swapped value4 to a string using the desired format

            uint value = BitConverter.ToUInt32(EditorClass.EditorFile.FileBytes, EditorClass.EditorTableStart + (EditorClass.TableRowIndex * EntryClass.EntryGameTableSize) + EntryClass.EntryByteOffset);
            byte[] valueBytes = BitConverter.GetBytes(value);
            Array.Reverse(valueBytes);
            uint swappedValue = BitConverter.ToUInt32(valueBytes, 0);
            TheWorkshop.PropertiesEntry4ByteL.Text = swappedValue.ToString("D");
            TheWorkshop.PropertiesEntryHex4ByteL.Text = swappedValue.ToString("X8");


            /////////////////////Checkboxes and Bitflags/////////////////////
            

            
            //The Editor.SelectedEntry 







            //////////////////////////////////////Various Dropdown Menus//////////////////////////////////////////


            string FindEntryLabelView = EntryClass.EntryLabelShown;  //Entry Label Shown Dropdown Menu.
            foreach (ComboBoxItem item in TheWorkshop.PropertiesEntryLabelShow.Items)
            {
                if (item.Content.ToString() == FindEntryLabelView)
                {
                    TheWorkshop.PropertiesEntryLabelShow.SelectedItem = item;
                    break;
                }
            }


            string FindEntryType = EntryClass.EntrySubType;  //Entry Type Dropdown Menu.
            foreach (ComboBoxItem item in TheWorkshop.PropertiesEntryType.Items)
            {
                if (item.Content.ToString() == FindEntryType)
                {
                    TheWorkshop.PropertiesEntryType.SelectedItem = item;
                    break;
                }
            }


            string FindEntryByteSize = "Dummy"; //Entry Size Dropdown Menu.
            if (EntryClass.EntryByteSize == "1") { FindEntryByteSize = "1 Byte"; } 
            if (EntryClass.EntryByteSize == "2L") { FindEntryByteSize = "2 Bytes Little Endian"; }
            if (EntryClass.EntryByteSize == "4L") { FindEntryByteSize = "4 Bytes Little Endian"; }
            if (EntryClass.EntryByteSize == "2B") { FindEntryByteSize = "2 Bytes Big Endian"; }
            if (EntryClass.EntryByteSize == "4B") { FindEntryByteSize = "4 Bytes Big Endian"; }
            foreach (ComboBoxItem item in TheWorkshop.PropertiesEntryByteSizeComboBox.Items)
            {
                if (item.Content.ToString() == FindEntryByteSize)
                {
                    TheWorkshop.PropertiesEntryByteSizeComboBox.SelectedItem = item;
                    break;
                }
            }

            
            


            string FindState = "Dummy"; //IS Entry Enabled 
            if (EntryClass.EntrySaveState == "Enabled") { FindState = "Saving Enabled"; }
            if (EntryClass.EntrySaveState == "Disabled") { FindState = "Saving Disabled"; }
            foreach (ComboBoxItem item in TheWorkshop.PropertiesEntrySaveState.Items)
            {
                if (item.Content.ToString() == FindState)
                {
                    TheWorkshop.PropertiesEntrySaveState.SelectedItem = item;
                    break;
                }
            }
            if (EntryClass.EntrySaveState == "AutoDisabled") { TheWorkshop.PropertiesEntrySaveState.Text = "AutoDisabled"; }



            //////////////////////////////////////Settings Per Entry Type//////////////////////////////////////////

            if (EntryClass.EntrySubType == "NumberBox")
            {
                string FindSign = "Dummy"; //Numberbox "Sign" Dropdown Menu. (If numberbox accepts negative numbers or not.)
                if (EntryClass.EntryTypeNumberBox.NumberSign == "Positive") { FindSign = "Positive Only"; }
                if (EntryClass.EntryTypeNumberBox.NumberSign == "Negative") { FindSign = "Positive and Negative"; }
                foreach (ComboBoxItem item in TheWorkshop.PropertiesEntryNumberBoxSign.Items)
                {
                    if (item.Content.ToString() == FindSign)
                    {
                        TheWorkshop.PropertiesEntryNumberBoxSign.SelectedItem = item;
                        break;
                    }
                }
            }




            if (EntryClass.EntrySubType == "CheckBox") 
            {
                TheWorkshop.PropertiesEntryCheckText.Text = EntryClass.EntryTypeCheckBox.CheckBoxTrueText;
                TheWorkshop.PropertiesEntryUncheckText.Text = EntryClass.EntryTypeCheckBox.CheckBoxFalseText;

            }


            if (EntryClass.EntrySubType == "BitFlag") 
            {
                TheWorkshop.PropertiesEntryBitFlag1Name.Text = EntryClass.EntryTypeBitFlag.BitFlag1Name;
                TheWorkshop.PropertiesEntryBitFlag1CheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag1CheckText;
                TheWorkshop.PropertiesEntryBitFlag1UncheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag1UncheckText;
                TheWorkshop.PropertiesEntryBitFlag2Name.Text = EntryClass.EntryTypeBitFlag.BitFlag2Name;
                TheWorkshop.PropertiesEntryBitFlag2CheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag2CheckText;
                TheWorkshop.PropertiesEntryBitFlag2UncheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag2UncheckText;
                TheWorkshop.PropertiesEntryBitFlag3Name.Text = EntryClass.EntryTypeBitFlag.BitFlag3Name;
                TheWorkshop.PropertiesEntryBitFlag3CheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag3CheckText;
                TheWorkshop.PropertiesEntryBitFlag3UncheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag3UncheckText;
                TheWorkshop.PropertiesEntryBitFlag4Name.Text = EntryClass.EntryTypeBitFlag.BitFlag4Name;
                TheWorkshop.PropertiesEntryBitFlag4CheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag4CheckText;
                TheWorkshop.PropertiesEntryBitFlag4UncheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag4UncheckText;
                TheWorkshop.PropertiesEntryBitFlag5Name.Text = EntryClass.EntryTypeBitFlag.BitFlag5Name;
                TheWorkshop.PropertiesEntryBitFlag5CheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag5CheckText;
                TheWorkshop.PropertiesEntryBitFlag5UncheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag5UncheckText;
                TheWorkshop.PropertiesEntryBitFlag6Name.Text = EntryClass.EntryTypeBitFlag.BitFlag6Name;
                TheWorkshop.PropertiesEntryBitFlag6CheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag6CheckText;
                TheWorkshop.PropertiesEntryBitFlag6UncheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag6UncheckText;
                TheWorkshop.PropertiesEntryBitFlag7Name.Text = EntryClass.EntryTypeBitFlag.BitFlag7Name;
                TheWorkshop.PropertiesEntryBitFlag7CheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag7CheckText;
                TheWorkshop.PropertiesEntryBitFlag7UncheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag7UncheckText;
                TheWorkshop.PropertiesEntryBitFlag8Name.Text = EntryClass.EntryTypeBitFlag.BitFlag8Name;
                TheWorkshop.PropertiesEntryBitFlag8CheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag8CheckText;
                TheWorkshop.PropertiesEntryBitFlag8UncheckText.Text = EntryClass.EntryTypeBitFlag.BitFlag8UncheckText;
            }






        } //End of UpdateEntryProperties Method





    } //End of Class
}

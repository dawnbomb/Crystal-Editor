using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Crystal_Editor
{
    internal class ExportToGoogleSheets
    {

        //This file has functions to convert an editor into a csv, that can be imported to google sheets.
        //this makes it easy to share editors online, and crowdsource editor creation / labeling entrys.
        //It does nothing else, and is not interconnected to anything else other then the 2 convert to google sheet buttons in a workshop.

        public void ToGoogleSheetHex(Workshop TheWorkshop, Editor EditorClass) 
        {
            string EditorData = "";
            int Rows = EditorClass.NameTableItemCount; //causes problems
            int Columns = EditorClass.EditorTableRowSize;
            var TheFile = EditorClass.EditorFile.FileBytes;

            if (Rows == 0) //Makes editors that don't get item names from a file work with sheet exports.
            {
                foreach (var Item in EditorClass.LeftBar.ItemList)
                {
                    if (Item.ItemType != "Folder")
                    {
                        Rows++;
                    }
                }
            }            

            

            EditorData = EditorData + " ,"; //Skip the very top left of the google sheet
            for (int ColumnInRow1 = 0; ColumnInRow1 != Columns; ColumnInRow1++) //Setting up the first row, to get Column / Entry names.
            {
                foreach (var page in EditorClass.PageList)
                {
                    foreach (var row in page.RowList)
                    {
                        foreach (var column in row.ColumnList)
                        {
                            foreach (var entry in column.EntryList)
                            {
                                if (entry.EntryByteOffset == ColumnInRow1) { EditorData = EditorData + entry.EntryName + ","; }

                            }
                        }
                    }
                }


                
            }
            EditorData = EditorData + "\r\n";


            for (int row = 0; row != Rows; row++ ) 
            {
                //Content at the start of a row
                EditorData = EditorData + EditorClass.LeftBar.ItemList[row].ItemName + ","; //The names of each item. Item folders are ID 0 so the first item name might be a folder lol

                

                


                for (int c = 0; c != Columns; c++)
                {
                    EditorData = EditorData + TheFile[EditorClass.EditorTableStart + (row * Columns) + c].ToString("X2") + ",";
                }
                EditorData = EditorData + "\r\n";
            }
            foreach (byte Byte in EditorClass.EditorFile.FileBytes) 
            {
                
            }



            System.IO.File.WriteAllText(TheWorkshop.ExePath + "\\" + EditorClass.EditorName + ".csv", EditorData); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
        }






        public void ToGoogleSheetDecimal(Workshop TheWorkshop, Editor EditorClass)
        {
            string EditorData = "";
            int Rows = EditorClass.NameTableItemCount; //causes problems
            int Columns = EditorClass.EditorTableRowSize;
            var TheFile = EditorClass.EditorFile.FileBytes;

            if (Rows == 0) //Makes editors that don't get item names from a file work with sheet exports.
            {
                foreach (var Item in EditorClass.LeftBar.ItemList)
                {
                    if (Item.ItemType != "Folder")
                    {
                        Rows++;
                    }
                }
            }


            EditorData = EditorData + " ,"; //Skip the very top left of the google sheet
            for (int ColumnInRow1 = 0; ColumnInRow1 != Columns; ColumnInRow1++) //Setting up the first row, to get Column / Entry names.
            {
                foreach (var page in EditorClass.PageList)
                {
                    foreach (var row in page.RowList)
                    {
                        foreach (var column in row.ColumnList)
                        {
                            foreach (var entry in column.EntryList)
                            {
                                if (entry.EntryByteOffset == ColumnInRow1) { EditorData = EditorData + entry.EntryName + ","; }

                            }
                        }
                    }
                }



            }
            EditorData = EditorData + "\r\n";


            for (int row = 0; row != Rows; row++)
            {
                //Content at the start of a row
                EditorData = EditorData + EditorClass.LeftBar.ItemList[row].ItemName + ","; //The names of each item. Item folders are ID 0 so the first item name might be a folder lol






                for (int c = 0; c != Columns; c++)
                {
                    string TheByte = TheFile[EditorClass.EditorTableStart + (row * Columns) + c].ToString("X2");
                    int decimalValue = Convert.ToInt32(TheByte, 16);
                    EditorData = EditorData + decimalValue.ToString() + ",";
                }
                EditorData = EditorData + "\r\n";
            }
            foreach (byte Byte in EditorClass.EditorFile.FileBytes)
            {

            }



            System.IO.File.WriteAllText(TheWorkshop.ExePath + "\\" + EditorClass.EditorName + ".csv", EditorData); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
        }










        public void ToGoogleSheetHexBackup(Workshop TheWorkshop, Editor EditorClass)
        {
            string EditorData = "";
            int Rows = EditorClass.NameTableItemCount;
            int Columns = EditorClass.EditorTableRowSize;
            var TheFile = EditorClass.EditorFile.FileBytes;



            for (int r = 0; r != Rows; r++)
            {
                for (int c = 0; c != Columns; c++)
                {
                    EditorData = EditorData + TheFile[EditorClass.EditorTableStart + (r * Columns) + c].ToString("X2") + ",";
                }
                EditorData = EditorData + "\r\n";
            }
            foreach (byte Byte in EditorClass.EditorFile.FileBytes)
            {

            }



            System.IO.File.WriteAllText(TheWorkshop.ExePath + "\\ExportToSheets.csv", EditorData); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
        }




    }
}

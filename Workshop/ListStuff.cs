using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Crystal_Editor
{
    public partial class Workshop
    {
        //This file handles the "List" function of an entry. Lists are a type of byte option, where they can be read as a list of options.
        //I plan to remake / overhaul this, to 3 main options.
        //Link to File
        //Link to Editor
        //Link to Nothing (Custom user name list)
        //This is a goal i think i will struggle with and would love help.

        //In addition, i would like to be able to make an entry automatically appear as a dropdown instead of a list, if it has less then 35 options.

        public void EntryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedItem = (string)EntryListBox.SelectedItem;

            EntryClass.EntryTypeList.ListButton.Content = selectedItem;

            //Emanager.SaveList(EntryClass);
            string input = (string)EntryClass.EntryTypeList.ListButton.Content;
            string[] parts = input.Split(':');
            string number = parts[0].Trim();
            EntryClass.EntryByteDecimal = number; // Console.WriteLine(number); // Output: 24


            EntryManager.SaveEntry(EntryClass.EntryEditor, EntryClass);
            EntryManager.UpdateEntryProperties(this, EntryClass.EntryEditor);

        }

        private void ButtonListEditSave_Click(object sender, RoutedEventArgs e)
        {
            ItemsNumBox.Clear();
            ItemsEditBox.Clear();
            //ItemsNumBox.Text = "0";
            StringBuilder NumsText = new StringBuilder("0");
            StringBuilder itemsText = new StringBuilder(EntryClass.EntryTypeList.ListItems[0]);
            for (int i = 1; i < EntryClass.EntryTypeList.ListItems.Length; i++)
            {
                NumsText.Append("\r");
                NumsText.Append(i);
                itemsText.Append("\r");
                itemsText.Append(EntryClass.EntryTypeList.ListItems[i]);
            }
            ItemsNumBox.Text = NumsText.ToString();
            ItemsEditBox.Text = itemsText.ToString();

            //ButtonListEditSave.Content = "Save";
            HIDEALL();
            EditorListTableWindow.Visibility = Visibility.Visible;
            ListPanelEdit.Visibility = Visibility.Collapsed;
            GridListFromTable.Visibility = Visibility.Collapsed;
            ComboBoxListType.Text = "";


            if (ButtonListEditSave.Content == "Edit") //stringbuilder Numbox+Editbox load time: 6 seconds
            {
                
            }
            else if (ButtonListEditSave.Content == "Save")
            {
                
            }


        }












        private void ComboBoxListTypeClosed(object sender, EventArgs e)
        {
            if (ComboBoxListType.Text == "Link to File")
            {
                
                ListPanelEdit.Visibility = Visibility.Collapsed;
                GridListFromTable.Visibility = Visibility.Visible;
            }

            if (ComboBoxListType.Text == "Link to Nothing")
            {
                ListPanelEdit.Visibility = Visibility.Visible;
                GridListFromTable.Visibility = Visibility.Collapsed;
            }


        }

        private void CancelListEdit(object sender, RoutedEventArgs e)
        {
            EditorListTableWindow.Visibility = Visibility.Collapsed;
        }


        private void SaveList(object sender, RoutedEventArgs e)
        {
            EditorListTableWindow.Visibility = Visibility.Collapsed;

            if (ComboBoxListType.Text == "")
            {
                return;
            }

            if (ComboBoxListType.Text == "Link to File")
            {

                
            }

            if (ComboBoxListType.Text == "Link to Nothing")
            {
                string[] lines = ItemsEditBox.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i < EntryClass.EntryTypeList.ListItems.Length)
                    {
                        EntryClass.EntryTypeList.ListItems[i] = lines[i].Trim();
                    }
                    else
                    {
                        // Handle case where there are more lines than array elements
                        break;
                    }
                }
                //ButtonListEditSave.Content = "Save";
                EntryListBox.SelectionChanged -= EntryListBox_SelectionChanged; // Remove event handler   
                EntryListBox.Items.Clear();
                for (int i = 0; i < EntryClass.EntryTypeList.ListItems.Length; i++)
                {
                    if (!string.IsNullOrEmpty(EntryClass.EntryTypeList.ListItems[i]))
                    {
                        string itemText = i + ": " + EntryClass.EntryTypeList.ListItems[i];
                        EntryListBox.Items.Add(itemText);
                    }
                }
                EntryListBox.SelectionChanged += EntryListBox_SelectionChanged; // Re-attach event handler  

                EntryManager.LoadList(EntryClass);

                //ButtonListEditSave.Content = "Edit";
                ListPanelEdit.Visibility = Visibility.Collapsed;
            }



        }

















    }
}

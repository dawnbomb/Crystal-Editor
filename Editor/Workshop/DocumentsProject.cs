using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal_Editor
{
    // This file handles "Project Documentation". 
    // Project documents are a type of text documentation built directly into the program.
    // Crystal editor seeks to have an *extreme* amount of documentation / notation systems to make it as good as possible to make notes for every possible thing.
    // Project Documentation, is for notes on your personal project, whatever your working on. 
    // For "offical documentation", see DocumentsWorkshop.cs
    // when a workshop is shared, Project documents are not shared, but workshop documents are.

    public partial class Workshop 
    {
        DocumentsProject DocumentsProject = new();

        private void MenuSaveProjectDocuments(object sender, RoutedEventArgs e)
        {
            DocumentsProject.SaveProjectDocumentation(this);
        }

        private void NewProjectDocument(object sender, RoutedEventArgs e)
        {

            DocumentsProject.CreateProjectDocument(this, "New Document", "");
        }

        private void SaveProjectDocumentName(object sender, RoutedEventArgs e)
        {
            //TreeProjectDocuments

            if (TreeProjectDocuments.SelectedItem == null) { return; }

            if (ProjectDocumentNameBox.Text != "" || ProjectDocumentNameBox.Text != null)
            {
                foreach (TreeViewItem TreeViewItem in TreeProjectDocuments.Items)
                {
                    if (TreeViewItem.Header as string == ProjectDocumentNameBox.Text)
                    {
                        return;
                    }
                }
                ((TreeViewItem)TreeProjectDocuments.SelectedItem).Header = ProjectDocumentNameBox.Text;
            }
        }

        private void DeleteProjectDocument(object sender, RoutedEventArgs e)
        {
            if (TreeProjectDocuments.SelectedItem == null) { return; }
            TreeProjectDocuments.Items.Remove(TreeProjectDocuments.SelectedItem);
        }

    }


    class DocumentsProject
    {
        

        //TreeProjectDocuments
        TreeViewItem CurrentItem;
        //DocumentsTree = TreeProjectDocuments
        //DocumentTextBox = ProjectDocumentTextBox
        //DocumentNameBox = ProjectDocumentNameBox


        public void LoadDocumentation(Workshop TheWorkshop)
        {
            if (TheWorkshop.IsPreviewMode == true) { return; }

            string[] ProjectDocumentFolderNames = Directory.GetDirectories(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\Documentation", "*", SearchOption.TopDirectoryOnly).Select(x => new DirectoryInfo(x).Name).ToArray();
            string[] ProjectDocumentOrder = File.ReadLines(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\Documentation\\" + "LoadOrder.txt").ToArray();


            foreach (string name in ProjectDocumentOrder)//The last known list of documents for this workshop, in the order they were saved in.
            {
                if (ProjectDocumentFolderNames.Contains(name))
                {
                    string Tag = System.IO.File.ReadAllText(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\Documentation\\" + name + "\\Text.txt");
                    CreateProjectDocument(TheWorkshop, name, Tag);

                }
            }

            foreach (string name in ProjectDocumentFolderNames)//The, add any new documents to the document tree in alphabetical order.
            {
                if (!ProjectDocumentOrder.Contains(name))
                {
                    string Tag = System.IO.File.ReadAllText(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\Documentation\\" + name + "\\Text.txt");
                    CreateProjectDocument(TheWorkshop, name, Tag);

                }
            }


        }

        public void CreateProjectDocument(Workshop TheWorkshop, string name, string Tag)
        {
            TreeViewItem TreeViewItem = new();

            int i = 0; //This chunk makes sure a new document will never have the same name as an existing document.
            int goal = 1; //without the user getting an error, interrupting the flow of concentration.
            for (; i < goal; i++)
            {
                foreach (TreeViewItem Item in TheWorkshop.TreeProjectDocuments.Items)
                {
                    if (Item.Header as string == name)
                    {
                        name = name + "2";
                        goal++;
                    }
                }

            }

            TreeViewItem.Header = name;
            TreeViewItem.Tag = Tag;
            TheWorkshop.TreeProjectDocuments.Items.Add(TreeViewItem);

            TreeViewItem.Selected += (sender, e) =>
            {
                if (CurrentItem != null)
                {
                    CurrentItem.Tag = TheWorkshop.ProjectDocumentTextBox.Text;
                }

                TheWorkshop.ProjectDocumentNameBox.Text = TreeViewItem.Header.ToString();
                TheWorkshop.ProjectDocumentTextBox.Text = TreeViewItem.Tag as string;
                CurrentItem = TreeViewItem;

            };



            //This chunk handles moving the documents via mouse click and drag.
            TreeViewItem.AllowDrop = true;
            TreeViewItem.Drop += DocumentDrop;
            void DocumentDrop(object sender, DragEventArgs e)
            {

                if (e.Data.GetDataPresent("MoveDocumentItem") && Keyboard.IsKeyUp(Key.LeftShift)) //Single Entry Drop
                {
                    TreeViewItem DropTreeViewItem = (TreeViewItem)e.Data.GetData("MoveDocumentItem");

                    if (TreeViewItem != DropTreeViewItem)
                    {
                        TheWorkshop.TreeProjectDocuments.Items.Remove(DropTreeViewItem);
                        int ToIndex = TheWorkshop.TreeProjectDocuments.Items.IndexOf(TreeViewItem) + 1;

                        if (ToIndex == TheWorkshop.TreeProjectDocuments.Items.Count && DropTreeViewItem != TreeViewItem)
                        {

                            TheWorkshop.TreeProjectDocuments.Items.Add(DropTreeViewItem);
                            DropTreeViewItem.IsSelected = true;
                        }
                        else if (DropTreeViewItem != TreeViewItem)
                        {
                            TheWorkshop.TreeProjectDocuments.Items.Insert(ToIndex, DropTreeViewItem);
                            DropTreeViewItem.IsSelected = true;
                        }
                    }
                }
            }
        }



        



        public void SaveProjectDocumentation(Workshop TheWorkshop)
        {
            if (CurrentItem != null)
            {
                CurrentItem.Tag = TheWorkshop.ProjectDocumentTextBox.Text;
            }

            //1: Save all to new location.
            //2: Delete all in old location.
            //3: Move all to new location.
            //4: order.txt?
            try
            {
                //This chunk checks for any problems / errors with saving documents.
                Directory.CreateDirectory(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\ProjectFolderTestSave");
                foreach (TreeViewItem TreeViewItem in TheWorkshop.TreeProjectDocuments.Items)
                {
                    Directory.CreateDirectory(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\ProjectFolderTestSave\\" + TreeViewItem.Header);
                    File.WriteAllText(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\ProjectFolderTestSave\\" + TreeViewItem.Header + "\\Text.txt", TreeViewItem.Tag as string); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.

                }
                Directory.Delete(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\ProjectFolderTestSave", true);

                //Assuming there was no errors, the next chunk actually saves the documents.
                //Instead of trying to properly deal with the logistics of renamed document folders, we just blow up the entire documentation folder and recreate it.       

                if (Directory.Exists(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\Documentation")) 
                {
                    string folderPath = TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\Documentation";
                    DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

                    foreach (FileInfo file in directoryInfo.GetFiles())
                    {
                        file.Delete();
                    }

                    foreach (DirectoryInfo subDirectory in directoryInfo.GetDirectories())
                    {
                        subDirectory.Delete(true);
                    }
                }
                






                //Directory.CreateDirectory(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documentation");
                string DocumentOrder = "";
                foreach (TreeViewItem TreeViewItem in TheWorkshop.TreeProjectDocuments.Items)
                {
                    Directory.CreateDirectory(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\Documentation\\" + TreeViewItem.Header);
                    File.WriteAllText(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\Documentation\\" + TreeViewItem.Header + "\\Text.txt", TreeViewItem.Tag as string); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
                    DocumentOrder = DocumentOrder + TreeViewItem.Header + "\n";

                }
                File.WriteAllText(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\Documentation\\" + "LoadOrder.txt", DocumentOrder);
            }
            catch
            {

                if (Directory.Exists(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\ProjectFolderTestSave"))
                {
                    string folderPath = TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\ProjectFolderTestSave";
                    DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

                    foreach (FileInfo file in directoryInfo.GetFiles())
                    {
                        file.Delete();
                    }

                    foreach (DirectoryInfo subDirectory in directoryInfo.GetDirectories())
                    {
                        subDirectory.Delete(true);
                    }
                    Directory.Delete(TheWorkshop.ExePath + "\\Projects\\" + TheWorkshop.WorkshopName + "\\" + TheWorkshop.ProjectName + "\\ProjectFolderTestSave");
                }


                string Error = "Error: Project Documentation not saved." +
                    "\n" +
                    "\nAn error occured during the \"Saving Project Documentation\" step of the save operation that just happened. Nothing has been corrupted, don't panic! :)" +
                    "\n" +
                    "\nAs you were probably saving more then only your documentation, you'll be happy to hear that each part of saving is handled seperately. " +
                    "This means there is NO CHANCE that any other parts of the saving operation are affected, such as when also saving editors or saving workshop files. " +
                    "\n" +
                    "\nAnyway as for documentation, To help users know which documents are which on their computer, we save documents using the names you give them to actual folders. " +
                    "Each operating system has a diffrent list of symbols it doesn't allow folder names to use. " +
                    "To deal with this problem the program first runs a simulation of what would happen IF it actually saved anything, " +
                    "by creating a temporary dummy folder and saving everything to that temporary folder. " +
                    "This way there is no chance your actual document folder will get corrupted or result in any other serious error. :)" +
                    "\n" +
                    "\nAs your seeing this error, it means your operating system doesn't like atleast one of the symbols you tried using in a documents name. " +
                    "Try to avoid symbols like @, #, $, %, &, *, \\, /, :, ;, etc. Also most operating systems DO allow spaces in folder names, so the problem isn't that. " +
                    "\n" +
                    "\nTry changing the names of any documents you think might have caused the error, and then try saving your documents again. ";
                Notification f2 = new(Error);
                f2.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
            }



        }


















    }
}

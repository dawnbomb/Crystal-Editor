using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.TextFormatting;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crystal_Editor
{
    // This file handles "Workshop Documentation". 
    // Workshop documents are a type of text documentation built directly into the program.
    // Crystal editor seeks to have an *extreme* amount of documentation / notation systems to make it as good as possible to make notes for every possible thing.
    // Workshop Documentation, is for "offical documentation" by a person or community about whatever the workshop is about.
    // It is not for actual projects, instead that is in DocumentsProjects.cs
    // When a workshop is shared, all workshop documents are shared with it.

    public partial class Workshop 
    {
        private void MenuSaveDocuments(object sender, RoutedEventArgs e) //Button: Saves ALL Document stuff
        {
            CSDocuments.SaveDocumentation(this);
        }


        private void ButtonNewDocument_Click(object sender, RoutedEventArgs e)
        {
            CSDocuments.CreateTreeItem(this, "New Document", "");
        }        

        private void SaveDocumentName(object sender, RoutedEventArgs e)
        {
            if (DocumentsTree.SelectedItem == null) { return; }

            if (DocumentNameBox.Text != "" || DocumentNameBox.Text != null)
            {
                foreach (TreeViewItem TreeViewItem in DocumentsTree.Items)
                {
                    if (TreeViewItem.Header as string == DocumentNameBox.Text)
                    {
                        return;
                    }
                }
                ((TreeViewItem)DocumentsTree.SelectedItem).Header = DocumentNameBox.Text;
            }

        }

        private void ButtonDeleteDocument_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentsTree.SelectedItem == null) { return; }
            DocumentsTree.Items.Remove(DocumentsTree.SelectedItem);
        }


        private void DocumentTreePreviewMouseMove(object sender, MouseEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyUp(Key.LeftShift))
            {
                var treeView = (TreeView)sender;
                var treeViewItem = GetTreeViewItemAtPoint(treeView, e.GetPosition(treeView));
                if (treeViewItem != null)
                {
                    var data = new DataObject("MoveDocumentItem", treeViewItem);
                    DragDrop.DoDragDrop(treeViewItem, data, DragDropEffects.Move);
                }
            }

            TreeViewItem GetTreeViewItemAtPoint(ItemsControl control, Point point)
            {
                var hitTestResult = VisualTreeHelper.HitTest(control, point);
                var visualHit = hitTestResult?.VisualHit;
                while (visualHit != null)
                {
                    if (visualHit is TreeViewItem treeViewItem)
                    {
                        return treeViewItem;
                    }

                    visualHit = VisualTreeHelper.GetParent(visualHit);
                }

                return null;
            }
        }




    }















    class DocumentsWorkshop
    {        

        TreeViewItem CurrentItem;
                

        public void LoadDocumentation(Workshop TheWorkshop)
        {
            string[] TheDocumentFolderNames = Directory.GetDirectories(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documentation", "*", SearchOption.TopDirectoryOnly).Select(x => new DirectoryInfo(x).Name).ToArray();
            string[] TheDocumentOrder = File.ReadLines(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documentation\\" + "LoadOrder.txt").ToArray();


            foreach (string name in TheDocumentOrder)//The last known list of documents for this workshop, in the order they were saved in.
            {
                if (TheDocumentFolderNames.Contains(name))
                {
                    string Tag = System.IO.File.ReadAllText(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documentation\\" + name + "\\Text.txt");
                    CreateTreeItem(TheWorkshop, name, Tag);

                }
            }

            foreach (string name in TheDocumentFolderNames)//The, add any new documents to the document tree in alphabetical order.
            {
                if (!TheDocumentOrder.Contains(name))
                {
                    string Tag = System.IO.File.ReadAllText(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documentation\\" + name + "\\Text.txt");
                    CreateTreeItem(TheWorkshop, name, Tag);

                }
            }


        }

        public void CreateTreeItem(Workshop TheWorkshop, string name, string Tag)
        {
            TreeViewItem TreeViewItem = new();

            int i = 0; //This chunk makes sure a new document will never have the same name as an existing document.
            int goal = 1; //without the user getting an error, interrupting the flow of concentration.
            for (;i<goal;i++ ) 
            {
                foreach (TreeViewItem Item in TheWorkshop.DocumentsTree.Items) 
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
            TheWorkshop.DocumentsTree.Items.Add(TreeViewItem);


            TreeViewItem.Selected += (sender, e) =>
            {
                if (CurrentItem != null)
                {
                    SaveItem(TheWorkshop);
                }


                TheWorkshop.DocumentNameBox.Text = TreeViewItem.Header.ToString();
                TheWorkshop.DocumentTextBox.Text = TreeViewItem.Tag as string;                
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
                        TheWorkshop.DocumentsTree.Items.Remove(DropTreeViewItem);
                        int ToIndex = TheWorkshop.DocumentsTree.Items.IndexOf(TreeViewItem) + 1;

                        if (ToIndex == TheWorkshop.DocumentsTree.Items.Count && DropTreeViewItem != TreeViewItem)
                        {

                            TheWorkshop.DocumentsTree.Items.Add(DropTreeViewItem);
                            DropTreeViewItem.IsSelected = true;
                        }
                        else if (DropTreeViewItem != TreeViewItem)
                        {
                            TheWorkshop.DocumentsTree.Items.Insert(ToIndex, DropTreeViewItem);
                            DropTreeViewItem.IsSelected = true;
                        }
                    }
                    


                }  


            }


        }

        public void SaveItem(Workshop TheWorkshop) 
        {
            CurrentItem.Tag = TheWorkshop.DocumentTextBox.Text;
        }
        
        

        public void SaveDocumentation(Workshop TheWorkshop) 
        {
            if (CurrentItem != null)
            {
                SaveItem(TheWorkshop);
            }

            //1: Save all to new location.
            //2: Delete all in old location.
            //3: Move all to new location.
            //4: LoadOrder.txt?
            try
            {
                //This chunk checks for any problems / errors with saving documents.
                Directory.CreateDirectory(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\DOCocumentationFolderTest");
                foreach (TreeViewItem TreeViewItem in TheWorkshop.DocumentsTree.Items)
                {
                    Directory.CreateDirectory(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\DOCocumentationFolderTest\\" + TreeViewItem.Header);
                    System.IO.File.WriteAllText(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\DOCocumentationFolderTest\\" + TreeViewItem.Header + "\\Text.txt", TreeViewItem.Tag as string); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.

                }
                Directory.Delete(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\DOCocumentationFolderTest", true);

                //Assuming there was no errors, the next chunk actually saves the documents.
                //Instead of trying to properly deal with the logistics of renamed document folders, we just blow up the entire documentation folder and recreate it.       

                if (Directory.Exists(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documentation")) 
                {
                    string folderPath = TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documentation";
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
                foreach (TreeViewItem TreeViewItem in TheWorkshop.DocumentsTree.Items)
                {
                    Directory.CreateDirectory(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documentation\\" + TreeViewItem.Header);
                    System.IO.File.WriteAllText(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documentation\\" + TreeViewItem.Header + "\\Text.txt", TreeViewItem.Tag as string); //Overwrites, Or creates file if it does not exist. Needs location permissions for admin folders.
                    DocumentOrder = DocumentOrder + TreeViewItem.Header + "\n";

                }
                System.IO.File.WriteAllText(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\Documentation\\" + "LoadOrder.txt", DocumentOrder);
            }
            catch 
            {

                if (Directory.Exists(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\DOCocumentationFolderTest"))
                {
                    string folderPath = TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\DOCocumentationFolderTest";
                    DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

                    foreach (FileInfo file in directoryInfo.GetFiles())
                    {
                        file.Delete();
                    }

                    foreach (DirectoryInfo subDirectory in directoryInfo.GetDirectories())
                    {
                        subDirectory.Delete(true);
                    }
                    Directory.Delete(TheWorkshop.ExePath + "\\Workshops\\" + TheWorkshop.WorkshopName + "\\DOCocumentationFolderTest");
                }                
                

                string Error = "Error: Documentation not saved." +
                    "\n" +
                    "\nAn error occured during the \"Saving Documentation\" step of the save operation that just happened. Nothing has been corrupted, don't panic! :)" +
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
                f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                f2.ShowDialog();
            }

            

        }

        








    }
}

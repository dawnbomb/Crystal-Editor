using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Crystal_Editor
{
    /// <summary>
    /// Interaction logic for Tutorial.xaml
    /// </summary>
    public partial class Tutorial : Window
    {
        Chapter_1 Chapter1 = new();
        Terminology Terms = new();
        Dictionary<string, string> Links = new();
        List<Run> Tutorials = new();

        public Tutorial()
        {
            InitializeComponent();            
        }
                

        

        private void BasicTutorials(object sender, RoutedEventArgs e)
        {
            HideAllTrees();
            TreeBasic.Visibility = Visibility.Visible;
        }

        private void AdvancedTutorials(object sender, RoutedEventArgs e)
        {
            HideAllTrees();
            TreeAdvanced.Visibility = Visibility.Visible;
        }
        private void Terminology(object sender, RoutedEventArgs e)
        {
            HideAllTrees();
            TreeTerms.Visibility = Visibility.Visible;
        }

        private void ThirdPartyTools(object sender, RoutedEventArgs e)
        {
            HideAllTrees();
            TreeTools.Visibility = Visibility.Visible;
        }
        private void EmulatorList(object sender, RoutedEventArgs e)
        {
            HideAllTrees();
            TreeEmulators.Visibility = Visibility.Visible;
        }
        private void ModAssist(object sender, RoutedEventArgs e)
        {
            HideAllTrees();
            TreeModAssist.Visibility = Visibility.Visible;            
        }
        private void EditorAssist(object sender, RoutedEventArgs e)
        {
            HideAllTrees();
            TreeEditor.Visibility = Visibility.Visible;
        }

        
        
        private void HideAllTrees() 
        {
            TreeBasic.Visibility = Visibility.Collapsed;
            TreeAdvanced.Visibility = Visibility.Collapsed;
            TreeTools.Visibility = Visibility.Collapsed;
            TreeEmulators.Visibility = Visibility.Collapsed;
            TreeTerms.Visibility = Visibility.Collapsed;
            TreeModAssist.Visibility = Visibility.Collapsed;
            TreeEditor.Visibility = Visibility.Collapsed;
        }

        
        private void Setup() 
        {
            RichTextBox1.Visibility = Visibility.Collapsed;
            RichTextBox2.Visibility = Visibility.Collapsed;
            RichTextBox3.Visibility = Visibility.Collapsed;
            RichTextBox4.Visibility = Visibility.Collapsed;
            RichTextBox5.Visibility = Visibility.Collapsed;
            RichTextBox6.Visibility = Visibility.Collapsed;

            ImageSet1.Visibility = Visibility.Collapsed;                      
            ImageSet2.Visibility = Visibility.Collapsed;            
            ImageSet3.Visibility = Visibility.Collapsed;            
            ImageSet4.Visibility = Visibility.Collapsed;
            ImageSet5.Visibility = Visibility.Collapsed;

            RichTextBox1.Document.Blocks.Clear();
            RichTextBox2.Document.Blocks.Clear();
            RichTextBox3.Document.Blocks.Clear();
            RichTextBox4.Document.Blocks.Clear();
            RichTextBox5.Document.Blocks.Clear();
            RichTextBox6.Document.Blocks.Clear();

            Tutorials.Clear();
            Links.Clear();
            
        }

        public void Preperations()
        {
            Paragraph ColorLinks(Run basicText)
            {
                Paragraph paragraph = new();
                string text = basicText.Text;
                int startIndex, endIndex;

                while ((startIndex = text.IndexOf('{')) != -1 && (endIndex = text.IndexOf('}')) != -1)
                {
                    // Add text before the curly braces
                    paragraph.Inlines.Add(new Run(text.Substring(0, startIndex)));

                    // Add text inside the curly braces with blue formatting
                    Run blueText = new Run(text.Substring(startIndex + 1, endIndex - startIndex - 1));
                    blueText.Foreground = Brushes.DeepSkyBlue;
                    paragraph.Inlines.Add(blueText);

                    // Update the text to process the remaining part
                    text = text.Substring(endIndex + 1);
                }

                // Add any remaining text
                paragraph.Inlines.Add(new Run(text));

                return paragraph;
            }

            List<Paragraph> paragraphs = new List<Paragraph>();
            foreach (var tutorial in Tutorials)
            {
                paragraphs.Add(ColorLinks(tutorial));
            }

            List<RichTextBox> richTextBoxes = new List<RichTextBox> { RichTextBox1, RichTextBox2, RichTextBox3, RichTextBox4, RichTextBox5, RichTextBox6 };

            for (int i = 0; i < paragraphs.Count; i++)
            {
                richTextBoxes[i].Document.Blocks.Add(paragraphs[i]);
                if (Tutorials[i] != null && Tutorials[i].Text != "")
                {
                    richTextBoxes[i].Visibility = Visibility.Visible;
                }
            }
        }

        private void CheckForLinks(object sender, MouseButtonEventArgs e)
        {
            TextPointer tp = RichTextBox1.GetPositionFromPoint(e.GetPosition(RichTextBox1), true);
            if (tp != null)
            {
                Run run = tp.Parent as Run; if (run != null)
                {
                    foreach (KeyValuePair<string, string> pair in Links)
                    {
                        string Key = pair.Key; //aka the link
                        string Value = pair.Value; //aka text
                        int index = run.Text.IndexOf(Value);
                        if (index > -1)
                        {
                            TextPointer start = run.ContentStart.GetPositionAtOffset(index);
                            TextPointer end = start.GetPositionAtOffset(Value.Length);
                            if (tp.CompareTo(start) >= 0 && tp.CompareTo(end) <= 0)
                            {
                                LookForLink(Key);
                            }
                        }
                    }
                    
                }
            }
        }

        private void SetupMenu(string Header)
        {
            foreach (TreeViewItem Item in TreeBasic.Items)     { if (Item.Header as string != Header) { Item.IsSelected = false; } else if (Item.Header as string == Header) { Item.IsSelected = true; } }
            foreach (TreeViewItem Item in TreeAdvanced.Items)  { if (Item.Header as string != Header) { Item.IsSelected = false; } else if (Item.Header as string == Header) { Item.IsSelected = true; } }
            foreach (TreeViewItem Item in TreeTools.Items)     { if (Item.Header as string != Header) { Item.IsSelected = false; } else if (Item.Header as string == Header) { Item.IsSelected = true; } }
            foreach (TreeViewItem Item in TreeEmulators.Items) { if (Item.Header as string != Header) { Item.IsSelected = false; } else if (Item.Header as string == Header) { Item.IsSelected = true; } }
            foreach (TreeViewItem Item in TreeTerms.Items)     { if (Item.Header as string != Header) { Item.IsSelected = false; } else if (Item.Header as string == Header) { Item.IsSelected = true; } }
            foreach (TreeViewItem Item in TreeModAssist.Items) { if (Item.Header as string != Header) { Item.IsSelected = false; } else if (Item.Header as string == Header) { Item.IsSelected = true; } }
            foreach (TreeViewItem Item in TreeEditor.Items)    { if (Item.Header as string != Header) { Item.IsSelected = false; } else if (Item.Header as string == Header) { Item.IsSelected = true; } }

        }









        public void Demo(Tutorial Tutorial, List<Run> Tutorials, Dictionary<string, string> Links)
        {
            Run Text1 = new(""); Tutorials.Add(Text1);
            Run Text2 = new(""); Tutorials.Add(Text2);
            Run Text3 = new(""); Tutorials.Add(Text3);

            Links.Add("Key1", "Trigger1");
            Links.Add("Key2", "Trigger2");
            Links.Add("Key3", "Trigger3");
            Tutorial.Preperations();
        }

        private void LookForLink(string Key)
        {
            Setup();
            //Basic Tutorials
            if (Key == "Welcome") { SetupMenu("Chapter 1:  Welcome to Crystal Editor!"); Chapter1.Welcome(this, Tutorials, Links); }
            if (Key == "Catagories") { SetupMenu("Chapter 1:  Catagories"); Chapter1.Catagories(this, Tutorials, Links); }
            if (Key == "Emulators") { SetupMenu("Chapter 1:  Emulators"); Chapter1.Emulators(this, Tutorials, Links); }
            if (Key == "ModsExplained") { SetupMenu("Chapter 1:  Mods Explained"); Chapter1.ModsExplained(this, Tutorials, Links); }

            //Terminology
            if (Key == "CommandPrompt") { SetupMenu("Command Prompt (aka 'cmd')"); Terms.CommandPrompt(this, Tutorials, Links); }
            if (Key == "CodeAndProgramming") { SetupMenu("Code and Programming"); Terms.CodeAndProgramming(this, Tutorials, Links); }

        }
           


        //The BASIC Tutorials
        private void Welcome(object sender, RoutedEventArgs e)              {           LookForLink("Welcome");        }  
        private void Catagories(object sender, RoutedEventArgs e)           {           LookForLink("Catagories");        }
        private void Emulators(object sender, RoutedEventArgs e)            {           LookForLink("Emulators");        }
        private void ModsExplained(object sender, RoutedEventArgs e)        {           LookForLink("ModsExplained");      }

        //Terminology
        private void CommandPrompt(object sender, RoutedEventArgs e)        {           LookForLink("CommandPrompt");        }

        private void CodeAndProgramming(object sender, RoutedEventArgs e)   {           LookForLink("CodeAndProgramming"); }
        
    }
}

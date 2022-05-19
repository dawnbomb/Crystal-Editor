using static Crystal_Editor.CoreCommonEvent;

namespace Crystal_Editor
{
    public partial class EnemyTemplateForm : Form
    {
        private CoreCommonEvent events;
        List<ComboBox> Arteboxes = new List<ComboBox>();  //Puts the combo boxes in an array called boxes so i can refernce this array later (to make the code pretty and save time)
        string UnitClass;



        public EnemyTemplateForm()
        {
            InitializeComponent();
            events = new CoreCommonEvent(fileLocation: Properties.Settings.Default.SpotTemplateFolder + "\\Template Editor\\EnemyTemplate", start: 37, row: 38, tree: Tree, controls: Controls);

            //Here is a list of names for what this editor is primarily editing.
            Tree.Nodes.Add("Goblin");
            Tree.Nodes.Add("Slime");
            Tree.Nodes.Add("Enemy Mage M");
            Tree.Nodes.Add("Enemy Mage F");
            Tree.Nodes.Add("Berserker");
            Tree.Nodes.Add("King Slime");
            Tree.Nodes.Add("King Slime Baby");
            Tree.Nodes.Add("Mimic");
            Tree.Nodes.Add("Reimu & Marisa (Duo-Battle)");
            Tree.Nodes.Add("Ezreal");
            Tree.Nodes.Add("Blue Eyes White Dragon");
            Tree.Nodes.Add("Dark Magician");
            Tree.Nodes.Add("Werewolf");
            Tree.Nodes.Add("Attack Helicopter");
            Tree.Nodes.Add("Ultimate God of Destruction");
            Tree.Nodes.Add("Ultimate God of Destruction (Post-game)");

            //Here is a list of names for a dropdown menu (so people can edit some things by selecting names from a list, instead of using numbers).
            Arteboxes.Add(comboBoxClass);
            string[] PlayerArteUserList = new[] {
            "00 None / from another arte",
            "01 Yuri",
            "02 Estelle",
            "03 Karol",
            //"04 Rita",
            "05 Raven",
            "06 Judith",
            "07 Repede",
            "08 Flynn",
            "09 Patty"
            };
            comboBoxClass.Items.AddRange(PlayerArteUserList);

            TreeNodeCollection nodeCollect = Tree.Nodes;
            Tree.SelectedNode = nodeCollect[0];
        }

        private int GetIndexForValueOrNeg1IfNonExistent(ComboBox comboBox, string value)
        {
            for (int x = 0; x < comboBox.Items.Count; x++)
                if (comboBox.Items[x].ToString()?.Split(' ')[0].Contains(value) ?? false)
                    return x;
            return -1;
        }

        private void Tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Editor(MoveRequest.Load);

            

            string VarByte = BitConverter.ToUInt32(events.data_array, 37 + (Tree.SelectedNode.Index * 38) + 1).ToString("D"); //We put the hex into this string, and if the string is read, we make the text appear in the combo box.
            nameOfFlags(0);

            void nameOfFlags(int box_id)
            {
                //skillTree.Nodes.Add("Function Is Working!" + VarcomboBoxSkillFlag);
                //Arteboxes[box_id].SelectedIndex = Arteboxes[box_id].FindStringExact("Unknown Flag");
                //if (VarSkillFlag == "0") { Arteboxes[box_id].SelectedIndex = Arteboxes[box_id].FindStringExact("00 None / from another arte"); }
                //if (VarSkillFlag == "1") { Arteboxes[box_id].SelectedIndex = Arteboxes[box_id].FindStringExact("01 Yuri"); }
                Arteboxes[box_id].SelectedIndex = -1;
                comboBoxClass.Text = "Unknown Flag TestDummy";

                Arteboxes[box_id].SelectedIndex = GetIndexForValueOrNeg1IfNonExistent(Arteboxes[box_id], VarByte);

                //if (VarByte == "0") { Arteboxes[box_id].SelectedIndex = 0; }
                //if (VarByte == "1") { Arteboxes[box_id].SelectedIndex = 1; }
                //if (VarByte == "2") { Arteboxes[box_id].SelectedIndex = 2; }
                //if (VarByte == "3") { Arteboxes[box_id].SelectedIndex = 3; }
                //if (VarByte == "4") { Arteboxes[box_id].SelectedIndex = 4; }
                ////if (VarByte == "5") { Arteboxes[box_id].SelectedIndex = 5; }
                //if (VarByte == "5") { Arteboxes[box_id].SelectedValue = GetIndexForValueOrNeg1IfNonExistent }
                //if (VarByte == "6") { Arteboxes[box_id].SelectedIndex = 6; }
                //if (VarByte == "7") { Arteboxes[box_id].SelectedIndex = 7; }
                //if (VarByte == "8") { Arteboxes[box_id].SelectedIndex = 8; }
                //if (VarByte == "9") { Arteboxes[box_id].SelectedIndex = 9; }
            }
            UnitClass = VarByte;
        }
        private void Button6_Click(object sender, EventArgs e) //Load Button
        {
            Editor(MoveRequest.Load);
        }
        private void Button5_Click(object sender, EventArgs e) //Save Button
        {
            Editor(MoveRequest.Save);

            if (comboBoxClass.Text == "00 None / from another arte") { UnitClass = "0"; }
            if (comboBoxClass.Text == "01 Yuri") { UnitClass = "1"; }
            if (comboBoxClass.Text == "02 Estelle") { UnitClass = "2"; }
            if (comboBoxClass.Text == "03 Karol") { UnitClass = "3"; }
            //if (comboBoxClass.Text == "04 Rita") { UnitClass = "4"; }
            if (comboBoxClass.Text == "05 Raven") { UnitClass = "5"; }
            if (comboBoxClass.Text == "06 Judith") { UnitClass = "6"; }
            if (comboBoxClass.Text == "07 Repede") { UnitClass = "7"; }
            if (comboBoxClass.Text == "08 Flynn") { UnitClass = "8"; }
            if (comboBoxClass.Text == "09 Patty") { UnitClass = "9"; }
            UInt32.TryParse(UnitClass, out uint value32); { TitleForm.ByteWriter(value32, events.data_array, 37 + (Tree.SelectedNode.Index * 38) + 1); }
        }
        private void SaveAllButton_Click(object sender, EventArgs e) //Save ALL Button
        {
            Editor(MoveRequest.Save);
        }

        public void Editor(MoveRequest requestType)
        {
            //Numbers start from Left and read to right Right (normal?) Endianese.
            //Final number is the column the data is from / how many bytes into a row the data is from. The first byte is byte 1 not byte 0.

            events.MoveData(textName: "richTextBoxID", column: 1, requestType); //1 Byte
            events.MoveData(textName: "richTextBoxStr", column: 17, requestType); //4 Byte
            events.MoveData(textName: "richTextBoxMag", column: 21, requestType); //4 Byte
            events.MoveData(textName: "richTextBoxDef", column: 25, requestType); //2 Byte
            events.MoveData(textName: "richTextBoxRes", column: 27, requestType); //2 Byte
            events.MoveData(textName: "richTextBoxTP", column: 15, requestType); //1 Byte

            events.MoveDataReverse(textName: "richTextBoxRev4", column: 2, requestType, length: 4); //4R Byte
            events.MoveDataReverse(textName: "richTextBoxRev2", column: 10, requestType, length: 2); //2R Byte
        }

        /*

        public void EditorReverse()
        {
            //Reverese Right to Left Endianese.
            //Semi-Final number is the column the data is from / how many bytes into a row the data is from. The first byte is byte 1 not byte 0.   The final number for array reverses is how many bytes to reverse.
            //4 Byte Reverse
            if (SaveLoad == "Load") {
                Array.Reverse(data_array, Start + (Tree.SelectedNode.Index * Row) + 2, 4);
                richTextBoxRev4.Text = BitConverter.ToUInt32(data_array, Start + (Tree.SelectedNode.Index * Row) + 2).ToString("D"); }  
                Array.Reverse(data_array, Start + (Tree.SelectedNode.Index * Row) + 2, 4);
            if (SaveLoad == "Save") { UInt32.TryParse(richTextBoxRev4.Text, out uint value32); { TitleForm.ByteWriter(value32, data_array, Start + (Tree.SelectedNode.Index * Row) + 2);
                Array.Reverse(data_array, Start + (Tree.SelectedNode.Index * Row) + 2, 4); } }

            //2 Byte Reverse
            if (SaveLoad == "Load") {
                Array.Reverse(data_array, Start + (Tree.SelectedNode.Index * Row) + 10, 2);
                richTextBoxRev2.Text = BitConverter.ToUInt16(data_array, Start + (Tree.SelectedNode.Index * Row) + 10).ToString("D"); }  // Read 4 Byte       //NameOfArray  //IgnoreFirstXBytes   //RowLeagth  //ByteInRow
                Array.Reverse(data_array, Start + (Tree.SelectedNode.Index * Row) + 10, 2);
            if (SaveLoad == "Save") { UInt32.TryParse(richTextBoxRev2.Text, out uint value16); { TitleForm.ByteWriter(value16, data_array, Start + (Tree.SelectedNode.Index * Row) + 10);
                Array.Reverse(data_array, Start + (Tree.SelectedNode.Index * Row) + 10, 2); } } 
        }

        */

        private void Tree_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {

        }

        private void EnemyTemplateForm_Load(object sender, EventArgs e)
        {

        }

        
    }
}

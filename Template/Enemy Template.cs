using static Crystal_Editor.CoreCommonEvent;

namespace Crystal_Editor
{
    

    public partial class EnemyTemplateForm : Form
    {
        private CoreCommonEvent events;
        List<ComboBox> ClassList = new List<ComboBox>();  //Puts the combo boxes in an array called boxes so i can refernce this array later (to make the code pretty and save time)
        string UnitClass;



        public EnemyTemplateForm()
        {
            InitializeComponent();
            events = new CoreCommonEvent(fileLocation: Properties.Settings.Default.SpotTemplateFolder + "\\Template Editor\\EnemyTemplate", start: 37, row: 38, tree: Tree, controls: Controls);

            picTrans();
                       
            
            

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
            ClassList.Add(comboBoxClass);
            string[] ClassNameList = new[] {
            "00 None / from another arte",
            "01 Yuri",
            "02 Estelle",
            "03 Karol",
            "04 Rita",
            "05 Raven",
            "06 Judith",
            "07 Repede",
            "08 Flynn",
            "09 Patty"
            };
            comboBoxClass.Items.AddRange(ClassNameList);

            TreeNodeCollection nodeCollect = Tree.Nodes;
            Tree.SelectedNode = nodeCollect[0];
        }

        public void picTrans()
        {
            //pictureBoxIconOne.Location = pictureBoxMainOne.PointToClient(pictureBoxIconOne.PointToScreen(Point.Empty));
            //pictureBoxIconOne.Parent = pictureBoxMainOne;

            pictureBoxIconLV.Location = pictureBoxMainLV.PointToClient(pictureBoxIconLV.PointToScreen(Point.Empty));
            pictureBoxIconLV.Parent = pictureBoxMainLV;
            pictureBoxIconEXP.Location = pictureBoxMainEXP.PointToClient(pictureBoxIconEXP.PointToScreen(Point.Empty));
            pictureBoxIconEXP.Parent = pictureBoxMainEXP;

            pictureBoxIconHP.Location = pictureBoxMainHP.PointToClient(pictureBoxIconHP.PointToScreen(Point.Empty));
            pictureBoxIconHP.Parent = pictureBoxMainHP;
            pictureBoxIconMP.Location = pictureBoxMainMP.PointToClient(pictureBoxIconMP.PointToScreen(Point.Empty));
            pictureBoxIconMP.Parent = pictureBoxMainMP;
            pictureBoxIconTP.Location = pictureBoxMainTP.PointToClient(pictureBoxIconTP.PointToScreen(Point.Empty));
            pictureBoxIconTP.Parent = pictureBoxMainTP;
            pictureBoxIconATK.Location = pictureBoxMainATK.PointToClient(pictureBoxIconATK.PointToScreen(Point.Empty));
            pictureBoxIconATK.Parent = pictureBoxMainATK;
            pictureBoxIconDEF.Location = pictureBoxMainDEF.PointToClient(pictureBoxIconDEF.PointToScreen(Point.Empty));
            pictureBoxIconDEF.Parent = pictureBoxMainDEF;
            pictureBoxIconMAG.Location = pictureBoxMainMAG.PointToClient(pictureBoxIconMAG.PointToScreen(Point.Empty));
            pictureBoxIconMAG.Parent = pictureBoxMainMAG;
            pictureBoxIconRES.Location = pictureBoxMainRES.PointToClient(pictureBoxIconRES.PointToScreen(Point.Empty));
            pictureBoxIconRES.Parent = pictureBoxMainRES;


            pictureBoxIconFire.Location = pictureBoxMainFire.PointToClient(pictureBoxIconFire.PointToScreen(Point.Empty));
            pictureBoxIconFire.Parent = pictureBoxMainFire;             
            pictureBoxIconIce.Location = pictureBoxMainIce.PointToClient(pictureBoxIconIce.PointToScreen(Point.Empty));
            pictureBoxIconIce.Parent = pictureBoxMainIce;
            pictureBoxIconWind.Location = pictureBoxMainWind.PointToClient(pictureBoxIconWind.PointToScreen(Point.Empty));
            pictureBoxIconWind.Parent = pictureBoxMainWind;
            pictureBoxIconElec.Location = pictureBoxMainElec.PointToClient(pictureBoxIconElec.PointToScreen(Point.Empty));
            pictureBoxIconElec.Parent = pictureBoxMainElec;
            pictureBoxIconHoly.Location = pictureBoxMainHoly.PointToClient(pictureBoxIconHoly.PointToScreen(Point.Empty));
            pictureBoxIconHoly.Parent = pictureBoxMainHoly;
            pictureBoxIconDark.Location = pictureBoxMainDark.PointToClient(pictureBoxIconDark.PointToScreen(Point.Empty));
            pictureBoxIconDark.Parent = pictureBoxMainDark;

            pictureBoxIconPsn.Location = pictureBoxMainPsn.PointToClient(pictureBoxIconPsn.PointToScreen(Point.Empty));
            pictureBoxIconPsn.Parent = pictureBoxMainPsn;
            pictureBoxIconSlp.Location = pictureBoxMainSlp.PointToClient(pictureBoxIconSlp.PointToScreen(Point.Empty));
            pictureBoxIconSlp.Parent = pictureBoxMainSlp;
            pictureBoxIconCnfs.Location = pictureBoxMainCnfs.PointToClient(pictureBoxIconCnfs.PointToScreen(Point.Empty));
            pictureBoxIconCnfs.Parent = pictureBoxMainCnfs;
            pictureBoxIconBurn.Location = pictureBoxMainBurn.PointToClient(pictureBoxIconBurn.PointToScreen(Point.Empty));
            pictureBoxIconBurn.Parent = pictureBoxMainBurn;
            pictureBoxIconChrm.Location = pictureBoxMainChrm.PointToClient(pictureBoxIconChrm.PointToScreen(Point.Empty));
            pictureBoxIconChrm.Parent = pictureBoxMainChrm;

            pictureBoxIconBLND.Location = pictureBoxMainBLND.PointToClient(pictureBoxIconBLND.PointToScreen(Point.Empty));
            pictureBoxIconBLND.Parent = pictureBoxMainBLND;
            pictureBoxIconPAR.Location = pictureBoxMainPAR.PointToClient(pictureBoxIconPAR.PointToScreen(Point.Empty));
            pictureBoxIconPAR.Parent = pictureBoxMainPAR;
            pictureBoxIconSTN.Location = pictureBoxMainSTN.PointToClient(pictureBoxIconSTN.PointToScreen(Point.Empty));
            pictureBoxIconSTN.Parent = pictureBoxMainSTN;

            pictureBoxIconSIL.Location = pictureBoxMainSIL.PointToClient(pictureBoxIconSIL.PointToScreen(Point.Empty));
            pictureBoxIconSIL.Parent = pictureBoxMainSIL;
            pictureBoxIconFRZ.Location = pictureBoxMainFRZ.PointToClient(pictureBoxIconFRZ.PointToScreen(Point.Empty));
            pictureBoxIconFRZ.Parent = pictureBoxMainFRZ;
            pictureBoxIconRAGE.Location = pictureBoxMainRAGE.PointToClient(pictureBoxIconRAGE.PointToScreen(Point.Empty));
            pictureBoxIconRAGE.Parent = pictureBoxMainRAGE;

            pictureBoxIconHEAD.Location = pictureBoxMainHEAD.PointToClient(pictureBoxIconHEAD.PointToScreen(Point.Empty));
            pictureBoxIconHEAD.Parent = pictureBoxMainHEAD;
            pictureBoxIconARM.Location = pictureBoxMainARM.PointToClient(pictureBoxIconARM.PointToScreen(Point.Empty));
            pictureBoxIconARM.Parent = pictureBoxMainARM;
            pictureBoxIconLEG.Location = pictureBoxMainLEG.PointToClient(pictureBoxIconLEG.PointToScreen(Point.Empty));
            pictureBoxIconLEG.Parent = pictureBoxMainLEG;
        }

        private void Tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Editor(MoveRequest.Load);
                        
            
            //string comboBox1Hex = BitConverter.ToUInt32(events.data_array, 37 + (Tree.SelectedNode.Index * 38) + 1).ToString("D"); //We put the hex into this string, and if the string is read, we make the text appear in the combo box.
            nameOfFlags(0);

            void nameOfFlags(int box_id)
            {
                events.DisplayComboboxIndex(ClassList, comboBoxClass, box_id, events.comboBox1Hex, column: 1);
            }
            UnitClass = events.comboBox1Hex;
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
            if (comboBoxClass.Text == "04 Rita") { UnitClass = "4"; }
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

            events.MoveData(textName: "comboBoxClass", column: 1, requestType); //1 Byte
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

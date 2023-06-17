using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Crystal_Editor
{
    public partial class Workshop
    {
        //I was going to organize more, this is literally just stuff about page from workshop.xaml.cs in a partial class. I never finished organizing, and so this is actually more confusing then organized. :(
        public void NewPageRight(Page PageClass)
        {
            Page NewPage = new Page { PageName = "New Page" };
            EditorClass.PageList.Add(NewPage);
            NewPage.RowList = new List<Row>();
            string IsFirstPage = "New";
            EditorCreate.CreatePage(EditorClass, NewPage, ref IsFirstPage, this, Database);

            Row RowClass = new Row { RowName = "New Row" }; //+ Database.GameEditors[ThisEditorName].PageNumber.ToString() };
            NewPage.RowList.Add(RowClass);
            RowClass.ColumnList = new List<Column>();
            EditorCreate.CreateRow(NewPage, RowClass, this, Database, -1);

            Column ColumnClass = new Column { ColumnName = "New Column" };
            RowClass.ColumnList.Add(ColumnClass);
            ColumnClass.EntryList = new List<Entry>();
            ColumnClass.ColumnRow = RowClass;
            EditorCreate.CreateColumn(RowClass, ColumnClass, this, Database, -1);
        }

        public void NewPageLeft(Page PageClass)
        {

        }

        private void PagePropertiesButtonNewRow_Click(object sender, RoutedEventArgs e)
        {
            Row RowClass = new Row { RowName = "New Row" }; //+ Database.GameEditors[ThisEditorName].PageNumber.ToString() };
            PageClass.RowList.Add(RowClass);
            RowClass.ColumnList = new List<Column>();
            EditorCreate.CreateRow(PageClass, RowClass, this, Database, -1);

            Column ColumnClass = new Column { ColumnName = "New Column" };
            RowClass.ColumnList.Add(ColumnClass);
            ColumnClass.EntryList = new List<Entry>();
            ColumnClass.ColumnRow = RowClass;
            EditorCreate.CreateColumn(RowClass, ColumnClass, this, Database, -1);
        }
    }
}

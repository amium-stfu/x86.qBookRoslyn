//using qb.Helpers;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;


namespace QB.Controls
{
    public class TableView : Control
    {
        /*
        /// <summary>
        /// Creates a new Table-Widget
        /// </summary>
        /// 
        public TableView(string name) : base(name)
        {
            this.Border = "gray:0.5";
        }

        /// <summary>
        /// Creates a new Table-Widget
        /// </summary>
        /// <param name="x">The x-location (left) of the Widget in mm</param>
        /// <param name="y">The y-location (top) of the Widget in mm</param>
        /// <param name="w">The width of the Widget in mm</param>
        /// <param name="h">The height of the Widget in mm</param>
        public TableView(string name, double x, double y, double w, double h)
            : this(name)
        {
            this.Bounds = new Rectangle(x, y, w, h);
        }
        */
        public TableView(string name, double x = 0, double y = 0, double w = 30, double h = 30) : base(name, x: x, y: y, w: w, h: h)
        {
            this.Border = "gray:0.1";
            //   this.AllowDrop = true;
            //   this.DragEnter += new DragEventHandler(Form1_DragEnter);
            //   this.DragDrop += new DragEventHandler(Form1_DragDrop);
        }


        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        void Form1_DragDrop(object sender, DragEventArgs e)
        {
        }

        /// <summary>
        /// Sets (or gets) the accociated QB.Table-Item
        /// </summary>
        public QB.Table Table { get; set; } = null;
        /// <summary>
        /// !!!DEPRECATED!!! TableView.Connect() is deprecated, please use TableView.Table = ... instead
        /// Links the Table-Widget with a Table-Object
        /// </summary>
        /// <param name="sourceTable">The Table-Object which should be visualized</param>
        /// [Obsolete("TableView.Connect() is deprecated, please use TableView.Table = ... instead")]
        public void Connect(QB.Table sourceTable)//, object set, object @out)
        {
            //MIGRATION
            //if (read is WrapperObject)
            //{
            //    if ((read as WrapperObject).Content is qb.Table)
            //        table = (read as WrapperObject).Content as qb.Table;
            //}
            Table = sourceTable;
        }

        public double ColWidth = 20.0;
        public double RowHeight = 5.0;

        public bool AlternateRowColors = true;

        internal override void Render(Control parent)
        {
            //base.Render(parent);

            try
            {
                if (Table == null)
                {
                    Draw.Rectangle(Draw.penMajorTicks, Bounds.X + 0.5f, Bounds.Y + 0.5f, Bounds.W - 1, Bounds.H - 1);
                    Draw.Text("Table '" + this.Name + "'\n<source-table not set>", Bounds.X + Bounds.W / 2, Bounds.Y + Bounds.H / 2 - 2.5f, 0, Draw.fontTextFixed, Draw.penMajorTicks.Color, System.Drawing.ContentAlignment.MiddleCenter);
                    return;
                }


                //var rows = table.Rows; //.getCsvLines();
                //if (table.Rows.ContainsKey(qbTables.ColHeaderRowId))
                //{
                //    foreach (var cell in table.Rows[qbTables.ColHeaderRowId].Cells)
                //    {
                //        var colFormat = cell.format;
                //        Draw.FillRectangle(Draw.GetBrush2(Misc.ParseColor(format.backcolor)), bounds.X + c * colWidth bounds.Y + r * rowHeight, colWidth, rowHeight);
                //        Draw.Text(text, bounds.X + c * colWidth, bounds.Y + r * rowHeight, colWidth, font, Misc.ParseColor(format.forecolor), align);
                //    }
                //}

                if (QB.Book.CompactView)
                    RowHeight = 3.5;

                int r = 0;
                lock (Table.Rows)
                {
                    foreach (var row in Table.Rows.ToList())
                    {
                        object rowId = row.Key;

                        //@SCAN 2023-07
                        if (AlternateRowColors)
                        {
                            if (rowId.ToString() == QB.Table.ColHeaderRowId)
                            {
                                //  Draw.FillRectangle((SolidBrush)Brushes.LightSteelBlue, Bounds.X + colLeft, Bounds.Y + r * RowHeight, colWidth, RowHeight);
                            }
                            else
                            {
                                if (r % 2 == 0)
                                {
                                    Draw.FillRectangle((SolidBrush)Brushes.AliceBlue, Bounds.X, Bounds.Y + r * RowHeight, Bounds.W, RowHeight);
                                }
                                else
                                {
                                    //Draw.Rectangle(Pens.Transparent, bounds.X + colLeft, bounds.Y + r * RowHeight, colWidth, RowHeight);
                                }
                            }
                        }


                        var cells = (row.Value as QB.Table.TableRow).Cells;
                        //var cols = row.Split(Table.csvDelimiter);
                        int c = 0;
                        double colLeft = 0;
                        lock (cells)
                        {
                            foreach (var cell in cells)
                            {
                                try
                                {
                                    double colWidth = ColWidth;

                                    Format format = new Format();
                                    if (Table.Rows.ContainsKey(QB.Table.ColHeaderRowId)
                                        && Table.Rows[QB.Table.ColHeaderRowId].Cells.ContainsKey(cell.Key))
                                    {
                                        var colFormat = Table.Rows[QB.Table.ColHeaderRowId].Cells[cell.Key].Format;
                                        format.AllowEdit = colFormat.AllowEdit;
                                        format.DisplayFormat = colFormat.DisplayFormat;
                                        format.EditMask = colFormat.EditMask;
                                        format.Alignment = colFormat.Alignment;
                                        format.BackColor = colFormat.BackColor;
                                        format.ForeColor = colFormat.ForeColor;
                                        format.Border = colFormat.Border;
                                        format.Font = colFormat.Font;
                                        format.Width = colFormat.Width;

                                        if (!double.IsNaN(format.Width))
                                            colWidth = format.Width;
                                    }
                                    if (cell.Value.Format != null)
                                    {
                                        if (format == null)
                                        {
                                            format = cell.Value.Format;
                                        }
                                        else
                                        {
                                            //format = cell.Value.format; //TODO: merge format-properties
                                            format.AllowEdit = cell.Value.Format.AllowEdit ? true : format.AllowEdit;
                                            format.DisplayFormat = cell.Value.Format.DisplayFormat ?? format.DisplayFormat;
                                            format.EditMask = cell.Value.Format.EditMask ?? format.EditMask;
                                            format.Alignment = cell.Value.Format.Alignment ?? format.Alignment;
                                            format.BackBrush = cell.Value.Format.BackBrush ?? format.BackBrush;
                                            format.ForeBrush = cell.Value.Format.ForeBrush ?? format.ForeBrush;
                                            format.Border = cell.Value.Format.Border ?? format.Border;
                                            format.Font = cell.Value.Format.Font ?? format.Font;
                                            format.Width = double.IsNaN(cell.Value.Format.Width) ? cell.Value.Format.Width : format.Width;
                                        }
                                    }

                                    string displayformat = format.DisplayFormat ?? "F3";
                                    System.Drawing.ContentAlignment align = System.Drawing.ContentAlignment.MiddleLeft;
                                    string text = null;
                                    if (cell.Value != null)
                                    {

                                        if (cell.Value.Value is double || cell.Value.Value is int)
                                        {
                                            if (displayformat.ToLower().StartsWith("x"))
                                            {
                                                Int32 x = cell.Value.Value.ToInt32();
                                                text = x.ToString(displayformat);
                                            }
                                            else
                                            {
                                                if (cell.Value.Value is double)
                                                {
                                                    if (double.IsNaN((double)cell.Value.Value))
                                                        text = "#";
                                                    else
                                                        text = ((double)cell.Value.Value).ToString(displayformat ?? "F3");
                                                }
                                                else if (cell.Value.Value is int)
                                                    text = ((int)cell.Value.Value).ToString(displayformat ?? "F3");
                                                else
                                                    text = cell.Value.Value.ToString();
                                            }
                                            align = System.Drawing.ContentAlignment.MiddleRight;
                                        }
                                        else if (cell.Value.Value is Signal)
                                        {
                                            text = (cell.Value.Value as Signal).Value.ToString(displayformat ?? "F3");
                                            align = System.Drawing.ContentAlignment.MiddleRight;
                                        }
                                        else if (cell.Value.Value is Module)
                                        {
                                            text = (cell.Value.Value as Module).Value.ToString(displayformat ?? "F3");
                                            align = System.Drawing.ContentAlignment.MiddleRight;
                                        }
                                        else if (cell.Value.Value is Boolean)
                                        {
                                            string strTrue = "True";
                                            string strFalse = "False";
                                            string strNull = "Null";
                                            if (displayformat.Contains(';'))
                                            {
                                                string[] splits = displayformat.Split(';');
                                                if (splits.Length > 1)
                                                {
                                                    strFalse = splits[0];
                                                    strTrue = splits[1];
                                                }
                                                if (splits.Length > 2)
                                                    strNull = splits[2];
                                            }
                                            if (cell.Value.Value == null)
                                                text = strNull;
                                            text = ((bool)cell.Value.Value) ? strTrue : strFalse;
                                            align = System.Drawing.ContentAlignment.MiddleLeft;
                                        }
                                        else
                                        {
                                            if (cell.Value.Value != null && double.TryParse(cell.Value.Value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
                                            {
                                                text = d.ToString(displayformat ?? "F3");
                                                align = System.Drawing.ContentAlignment.MiddleRight;
                                            }
                                            else
                                            {
                                                text = cell.Value.Value?.ToString();
                                                align = System.Drawing.ContentAlignment.MiddleLeft;
                                            }
                                        }
                                        if (format.Alignment != null)
                                        {
                                            align = (System.Drawing.ContentAlignment)format.Alignment;
                                        }
                                    }

                                    Font font = Draw.fontFootnoteFixed;
                                    if (format.Font != null)
                                    {
                                        if (format.FontConfig.Contains(":B"))
                                            font = new Font(Draw.fontFootnoteFixed, FontStyle.Bold);
                                        if (format.FontConfig.Contains(":I"))
                                            font = new Font(Draw.fontFootnoteFixed, FontStyle.Italic);
                                        if (format.FontConfig.Contains(":U"))
                                            font = new Font(Draw.fontFootnoteFixed, FontStyle.Underline);
                                        if (format.FontConfig.Contains(":S"))
                                            font = new Font(Draw.fontFootnoteFixed, FontStyle.Strikeout);
                                    }

                                    if (AlternateRowColors)
                                    {
                                        if (rowId.ToString() == QB.Table.ColHeaderRowId)
                                        {
                                            Draw.FillRectangle((SolidBrush)Brushes.LightSteelBlue, Bounds.X + colLeft, Bounds.Y + r * RowHeight, colWidth, RowHeight);
                                        }
                                        else
                                        {
                                            if (r % 2 == 0)
                                            {
                                                //    Draw.FillRectangle((SolidBrush)Brushes.Gainsboro, Bounds.X + colLeft, Bounds.Y + r * RowHeight, colWidth, RowHeight);
                                            }
                                            else
                                            {
                                                //Draw.Rectangle(Pens.Transparent, bounds.X + colLeft, bounds.Y + r * RowHeight, colWidth, RowHeight);
                                            }
                                        }
                                    }
                                    if (format.BackBrush != null)
                                        //Draw.FillRectangle(Draw.GetBrush2(Misc.ParseColor(format.BackColor)), Bounds.X + colLeft, Bounds.Y + r * RowHeight, colWidth, RowHeight);
                                        Draw.FillRectangle(format.BackBrush as SolidBrush ?? new SolidBrush(format.BackColor), Bounds.X + colLeft, Bounds.Y + r * RowHeight, colWidth, RowHeight);



                                    //Draw.Text(text, Bounds.X + colLeft, Bounds.Y + r * RowHeight, colWidth, font, format.ForeColor, align);


                                  //  align = ContentAlignment.MiddleLeft;
                                    if (align == ContentAlignment.MiddleRight)
                                        Draw.Text(text, Bounds.X + colLeft+ colWidth, Bounds.Y + r * RowHeight, 0, font, format.ForeColor, align);
                                    else
                                        Draw.Text(text, Bounds.X + colLeft, Bounds.Y + r * RowHeight, 0, font, format.ForeColor, align);

                                    if (rowId.ToString() == QB.Table.ColHeaderRowId) //is header-row?
                                    {
                                        Draw.Rectangle(Draw.GetPen2(System.Drawing.Color.Gray, 0.5), Bounds.X + colLeft, Bounds.Y + r * RowHeight, colWidth, RowHeight);
                                    }
                                    c++;
                                    colLeft += colWidth;
                                }
                                catch (Exception ex)
                                {
                                    Draw.Text("#EX", Bounds.X + colLeft, Bounds.Y + r * RowHeight, ColWidth, Draw.fontTextFixed, System.Drawing.Color.Red, System.Drawing.ContentAlignment.MiddleCenter);
                                }
                            }
                        }

                        //draw missing header-cells
                        while (c < Table.ColCount)
                        {
                            if (rowId.ToString() == QB.Table.ColHeaderRowId) //is header-row?
                            {
                                Draw.Rectangle(Draw.GetPen2(System.Drawing.Color.Gray, 0.5), Bounds.X + colLeft, Bounds.Y + r * RowHeight, ColWidth, RowHeight);
                            }
                            c++;
                        }

                        r++;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            base.Render(parent); //border, etc.
        }



        public void HandleClick(PointF point)
        {
            try
            {
                double deltaX = point.X - Bounds.X;
                double deltaY = point.Y - Bounds.Y;
                Table.SelectedRowNumber = (int)((deltaY) / RowHeight);
                double xPos = 0;
                //int colNr = (int)(point.X / ColWidth);
                int colNr = 0;
                string colId = null;
                bool colFound = false;
                if (!Table.Rows.ContainsKey(QB.Table.ColHeaderRowId))
                {
                    colNr = (int)((deltaX) / ColWidth);
                    colFound = true;
                }
                else
                {
                    var headerRow = Table.Rows[QB.Table.ColHeaderRowId];
                    foreach (var cell in headerRow.Cells)
                    {
                        if (!double.IsNaN(cell.Value.Format.Width))
                            xPos += cell.Value.Format.Width;
                        else
                            xPos += ColWidth;

                        if (xPos > deltaX)
                        {
                            colFound = true;
                            colId = cell.Key;
                            break;
                        }
                        colNr++;
                    }

                    if (!colFound)
                    {
                        while (colNr < Table.ColCount)
                        {
                            xPos += ColWidth;
                            if (xPos > deltaX)
                            {
                                colFound = true;
                                break;
                            }
                            colNr++;
                        }
                    }
                }

                if (colFound && colNr < Table.ColCount)
                {
                    string rowId = null;
                    if (Table.SelectedRowNumber == 0 && Table.Rows.ContainsKey(QB.Table.ColHeaderRowId))
                        rowId = QB.Table.ColHeaderRowId; //table.Rows.First().Key;
                    else
                        rowId = Table.Rows.Skip(Table.SelectedRowNumber).First().Key;

                    if (colId == null)
                    {
                        if (colNr < Table.Rows[rowId].Cells.Count)
                            colId = Table.Rows[rowId].Cells.Skip(colNr).First().Key;
                    }

                    if (rowId == null || colId == null)
                    {
                        return;
                    }


                    if (OnClick != null)
                    {
                        int.TryParse(rowId, out int row);
                        int.TryParse(colId, out int col);
                        string excelRef = null;
                        if (col > 0 && row > 0)
                            excelRef = StringExtensions.ExcelIndexToColumnLetters(col) + row;
                        OnClick(this, new TableViewClickEventArgs()
                        {
                            RowId = rowId,
                            ColumnId = colId,
                            Row = row,
                            Column = col,
                            ExcelRef = excelRef
                        });
                        return;
                    }


                    //MessageBox.Show($"clicked: {rowId}/{colId}");
                    if (Table[rowId, colId].Format.AllowEdit)
                    {
                        object value = Table[rowId, colId].Value;
                        QB.UI.InputDialog dialog = new QB.UI.InputDialog();
                        dialog.Title = "CELL VALUE";
                        dialog.Info = $"Change Value at {rowId},{colId} from\r\n   {value}\r\nto:";
                        dialog.Value = value?.ToString();
                        //dialog.Width = width;
                        //dialog.Height = heigth;

                        dialog.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                        var parentForm = System.Windows.Forms.Application.OpenForms[0];
                        dialog.Location = new System.Drawing.Point(parentForm.Left + (parentForm.Width - dialog.Width) / 2, parentForm.Top + (parentForm.Height - dialog.Height) / 2);

                        var dr = dialog.ShowDialog();
                        if (dr == System.Windows.Forms.DialogResult.OK)
                        {
                            if (double.TryParse(dialog.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
                                Table[rowId, colId].Value = d;
                            else
                                Table[rowId, colId].Value = dialog.Value;
                        }
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"#EX: " + ex.Message, "ERROR");
            }
        }

        public class TableViewClickEventArgs
        {
            public int Row = -1;
            public int Column = -1;
            public string RowId = null;
            public string ColumnId = null;
            public string ExcelRef = null;
        }

        public delegate void OnClickDelegate(TableView sender, TableViewClickEventArgs tvcea);
        public OnClickDelegate OnClick;


      //  public string DroppedFiles = "";

        internal virtual void Cleanup()
        {
            this.OnClick = null;
        }
    }
}

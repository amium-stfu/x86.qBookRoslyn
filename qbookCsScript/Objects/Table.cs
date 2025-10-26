using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace QB
{
    //HALE: Cow is just for some testing...
    public class Cow
    {
        public int Age = 12;
        public int Weight = 800;

        public void Grow(int amount)
        {
            Weight += amount;
        }
    }

    public class Table : Item
    {
        public static string ColHeaderRowId = "0"; //"$$$colHeaders"; //or rowId="0"?

        public class Cell
        {
            public Cell()
            {
            }

            private StringBuilder _sbValue = null;
            private object _value = null;
            public object Value
            {
                get
                {
                    if (_value is Delegate)
                    {
                        return ((Delegate)_value).DynamicInvoke();
                    }
                    else
                        return _value;
                }
                set
                {
                    if (value is StringBuilder)
                    {
                        _sbValue = value as StringBuilder;
                        _value = value.ToString();
                    }
                    else
                    {
                        _value = value;
                        if (_sbValue != null)
                        {
                            _sbValue.Clear();
                            _sbValue.Append(value);
                        }
                    }
                    if (value is double && double.IsNaN((double)value))
                    {

                    }
                }
            }

            public object Test;


            private QB.Format _format = new QB.Format();
            public QB.Format Format
            {
                get
                {
                    return _format;
                }
                set
                {
                    _format = value;
                }
            }
        }

        private class StringAndDoubleComp : IComparer<string>
        {
            public int Compare(string s1, string s2)
            {
                bool isNum1 = double.TryParse(s1, NumberStyles.Any, CultureInfo.InvariantCulture, out double d1);
                bool isNum2 = double.TryParse(s2, NumberStyles.Any, CultureInfo.InvariantCulture, out double d2);
                if (isNum1 && isNum2)
                {
                    if (d1 == d2)
                        return 0;
                    else
                        return d1 > d2 ? 1 : -1;
                }
                else
                {
                    if (isNum1)
                        return -1;
                    else if (isNum2)
                        return 1;
                    else
                    {
                        if (s1 == s2)
                            return 0;
                        else
                            return string.Compare(s1, s2);
                    }
                }
            }
        }
        internal class TableRow
        {
            internal TableRow()
            {
            }
            internal SortedDictionary<string, Cell> Cells { get; set; } = new SortedDictionary<string, Cell>(new StringAndDoubleComp());

            public override string ToString()
            {
                return (string.Join(Table.csvDelimiter.ToString(), this.Cells.Select(c => c.Value.Value?.ToString())));
            }
        }

        //public Table()
        //{
        //}

        public Table(string text = null, string id = null) : base(text, id)
        {

        }

        internal SortedDictionary<string, TableRow> Rows { get; set; } = new SortedDictionary<string, TableRow>(new StringAndDoubleComp());

        public string Name { get; set; } = "noname";

        public int RowCount
        {
            get
            {
                return Rows.Count;
            }
        }
        public int ColCount
        {
            get
            {
                lock (Rows)
                {
                    return Rows.Max(r => r.Value.Cells.Count);
                }
            }
        }




        class ColumnInfo
        {
            public string Title { get; set; }
            public double Width { get; set; }
        }

        Dictionary<string, ColumnInfo> ColumnHeaderDict = new Dictionary<string, ColumnInfo>();

        internal static char csvDelimiter = ',';
        internal static char csvComma = '.';
        internal static string csvNewLine = "\r\n";

        internal List<string> getCsvLines(bool addRowId = false, int maxRows = -1)
        {
            List<string> lines = new List<string>();
            foreach (var row in Rows)
            {
                if (row.Key.StartsWith("$$$")) //do NOT export virtual rows like '$$$header'
                    continue;

                string line = "";
                if (addRowId)
                    line = row.Key + csvDelimiter;
                foreach (var cell in row.Value.Cells)
                {
                    line += cell.Value.Value?.ToString() + csvDelimiter;
                }

                lines.Add(line.TrimEnd(csvDelimiter));
                if (maxRows > 0 && lines.Count >= maxRows)
                    break;
            }
            return lines;
        }

        internal List<string> getCsfLines(bool addRowId = false, int maxRows = -1)
        {
            List<string> lines = new List<string>();
            foreach (var row in Rows)
            {
                string line = "";
                if (addRowId)
                    line = row.Key + csvDelimiter;
                foreach (var cell in row.Value.Cells)
                {
                    line += cell.Value.Format?.ToString() + csvDelimiter;
                }

                lines.Add(line.TrimEnd(csvDelimiter));
                if (maxRows > 0 && lines.Count >= maxRows)
                    break;
            }
            return lines;
        }


        public string Csv
        {
            get
            {
                var csvLines = getCsvLines();
                if (csvLines != null && csvLines.Count > 0)
                    return string.Join(csvNewLine, csvLines);
                else
                    return "";
            }
            set
            {
                this.Clear();
                //this.Rows = new SortedDictionary<string, TableRow>();
                //var testRows = new SortedDictionary<string, TableRow>();

                string[] lines = value.Replace("\r", "").Split('\n'); //TODO: use csvNewLine -> regex-split? 
                int row = 0;
                foreach (var line in lines)
                {
                    row++;
                    string[] cells = line.Split(csvDelimiter);
                    int col = 0;
                    foreach (var cell in cells)
                    {
                        col++;
                        var newCell = new Cell() { Value = cell };
                        this[row.ToString(), col.ToString()] = newCell;
                        var checkCell = this[row.ToString(), col.ToString()];
                    }
                }
            }
        }
        public string CsvRowHeaders
        {
            get
            {
                var csvLines = getCsvLines(true);
                if (csvLines != null && csvLines.Count > 0)
                    return string.Join(csvNewLine, csvLines);
                else
                    return "";
            }
        }

        public string Csf
        {
            get
            {
                var csvLines = getCsvLines();
                if (csvLines != null && csvLines.Count > 0)
                    return string.Join(csvNewLine, csvLines);
                else
                    return "";
            }
            //TODO: set is missing
        }

        public string CsfRowHeaders
        {
            get
            {
                var csvLines = getCsfLines();
                if (csvLines != null && csvLines.Count > 0)
                    return string.Join(csvNewLine, csvLines);
                else
                    return "";
            }
        }

        public void Clear()
        {
            lock (Rows)
            {
                this.Rows.Clear();
            }

        }

        //[AutocompleteParamInfoAttribute("csv", "a comma separated list of items")]
        public string Add(string csv)
        {
            //insert at end
            string newRowID = AddRow();
            this.Rows[newRowID] = new TableRow();
            string[] cells = csv.Split(',');
            int col = 0;
            foreach (var cell in cells)
            {
                col++;
                this.Rows[newRowID].Cells[col.ToString()] = new Cell() { Value = cell };
            }
            return null;
        }

        public string Add(string csv, char separator)
        {
            //insert at end
            string newRowID = AddRow();
            this.Rows[newRowID] = new TableRow();
            string[] cells = csv.Split(separator);
            int col = 0;
            foreach (var cell in cells)
            {
                col++;
                this.Rows[newRowID].Cells[col.ToString()] = new Cell() { Value = cell };
            }
            return null;
        }

        //[AutocompleteParamInfoAttribute("row", "row-id of the table-row to be added")]
        //[AutocompleteParamInfoAttribute("csv", "a comma separated list of items")]
        public string Add(object rowId, string csv)
        {
            //insert at end
            string newRowID = rowId.ToString();
            this.Rows[newRowID] = new TableRow();
            string[] cells = csv.Split(',');
            int col = 0;
            foreach (var cell in cells)
            {
                col++;
                this.Rows[newRowID].Cells[col.ToString()] = new Cell() { Value = cell };
            }
            return null;
        }

        public string Row(object rowId)
        {
            return this.Rows[rowId.ToString()].ToString();
        }

        public string RowId(object colId, object value)
        {
            for (int row = 1; row <= RowCount; row++)
            {
                Table.Cell idCell = this.Rows["" + row].Cells[colId.ToString()];
                if ((idCell != null) && (idCell.Value != null) && (idCell.Value.ToString().Trim().ToUpper() == value.ToString().Trim().ToUpper()))
                    return "" + row;
            }
            return null;
        }


        public string Col(object colId)
        {
            List<object> values = new List<object>();
            lock (Rows)
            {
                foreach (var row in this.Rows)
                {
                    lock (row.Value.Cells)
                    {
                        if (row.Value.Cells.ContainsKey(colId.ToString()))
                            values.Add(row.Value.Cells[colId.ToString()].Value);
                        else
                            values.Add(null);
                    }
                }
            }
            if (values.Count > 0)
                return string.Join(csvDelimiter.ToString(), values);
            else
                return null;
        }





        static Regex trailingNumber = new Regex(@"(?<pre>[^0-9\.])(?<number>([0-9]*\.?[0-9]+))$", RegexOptions.Compiled);
        public string AddRow()
        {
            //insert at end
            //if last row is numeric, this row is last rowID+1; and return new rowID
            //  else use guid?
            string lastRowID = "0";
            if (this.Rows != null && this.Rows.Count > 0)
                lastRowID = this.Rows.Last().Key;
            if (double.TryParse(lastRowID, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double d))
            {
                return (d + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                //return Guid.NewGuid().ToString();
                Match m = trailingNumber.Match(lastRowID);
                if (m.Success)
                {
                    double.TryParse(m.Groups["number"].Value, out double number);
                    return m.Groups["pre"].Value + (number + 1.0);
                }
                else
                {
                    return lastRowID + "0001";
                }
            }
        }

        //public void insertRow(string row)
        //{
        //    //if row (numeric!) exists, increment all following rowIDs by +1
        //}

        public void DeleteRow(string rowId)
        {
            //if row exists, remove it
            if (this.Rows.ContainsKey(rowId))
                this.Rows.Remove(rowId);
        }

        /// <summary>
        /// Save the Table's contents as CSV to the default location, i.e. .\csv\<filename>_<date.time>.csv
        /// </summary>
        /// <returns></returns>
        public System.Object SaveCsv()
        {
            //use qbook's name (if known) as a prefix for the filename
            string qbookName = "qb";
            if (MyQbook != null && MyQbook.Filename != null)
                qbookName = MyQbook.Filename;

            string dir = Path.Combine(".", "csv");
            string path = Path.Combine(dir, qbookName + "_" + DateTime.Now.ToString("yyyy-MM-dd.HHmmss") + ".csv");
            return SaveCsv(path);
        }

        /// <summary>
        /// Save the Table's contents as CSV using the given path/filename
        /// </summary>
        /// <param name="fullPath">Filename for saving the CSV</param>
        /// <returns></returns>
        public System.Object SaveCsv(string fullPath)
        {
            try
            {
                QB.Logger.Debug($"Table'{this.Name}' -> SaveCsv() to " + fullPath);
                fullPath = Path.GetFullPath(fullPath);
                string dir = Path.GetDirectoryName(fullPath);
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
                //string csv = (string)toCsv();
                System.IO.File.WriteAllLines(fullPath, getCsvLines());
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        /// <summary>
        /// Loads the most recent CSV from the default directory matching <filename>_xyz.csv
        /// </summary>
        /// <returns></returns>
        public System.Object LoadCsv(bool firstRowIsHeader = false)
        {
            try
            {
                //use qbook's name (if known) as a prefix for the filename
                string qbookName = "qb";
                if (MyQbook != null && MyQbook.Filename != null)
                    qbookName = MyQbook.Filename;

                string dir = Path.Combine(".", "csv");
                string path = Path.Combine(dir, qbookName + "_" + DateTime.Now.ToString("yyyy-MM-dd.HHmmss") + ".csv");

                var latestFilename = System.IO.Directory.GetFiles(dir, qbookName + "_*" + ".csv").OrderByDescending(f => f).FirstOrDefault();
                if (latestFilename != null)
                    return LoadCsv(latestFilename, firstRowIsHeader);
                else
                    return false;
            }
            catch (Exception ex)
            {
                QB.Logger.Error("#EX in LoadCsv: " + ex.Message + (QB.Logger.ShowStackTrace ? ex.StackTrace : ""));
                return false;
            }
        }

        //public System.Object LoadCsv(bool firstRowIsHeader = false)
        //{
        //    try
        //    {
        //        QB.Logger.Debug($"Table'{this.Name}' -> LoadCsv() to " + fullPath);

        //        string dir = Path.Combine(".", "csv");
        //        string path = System.IO.Directory.GetFiles(dir, /*Qb.Book.Main.Name +*/ "_" + "*" + ".csv", SearchOption.TopDirectoryOnly).OrderBy(s => s).Last();
        //        if (path != null)
        //        {
        //            return LoadCsv(path, firstRowIsHeader);
        //        }
        //        else
        //            return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        public int SelectedRowNumber = -1;


        public System.Object LoadCsv(string fullPath, bool firstRowIsHeader = false)
        {
            try
            {
                QB.Logger.Debug($"Table'{this.Name}' -> LoadCsv() to " + fullPath);

                if (!File.Exists(fullPath))
                    return false;

                int r = 0;
                string[] lines = File.ReadAllLines(fullPath);
                if (firstRowIsHeader)
                {
                    string[] headers = lines[0].SplitOutsideQuotes(csvDelimiter);
                    foreach (var name in headers)
                    {
                        this["$$$header", name] = new Cell() { Value = name }; //Virtual row"$$$header" contains headers
                    }
                    //r++;
                }

                while (r < lines.Length)
                {
                    this.Add(lines[r]);
                    r++;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public override string ToString()
        {
            var csvLines = getCsvLines(true, 10);
            return string.Join("|", csvLines);
        }


        Regex a1Regex = new Regex(@"([A-Z]+)([0-9]+)");
        public Cell this[string a1]
        {
            get
            {
                a1 = a1.ToString().ToUpper();
                Match m = a1Regex.Match(a1);
                if (m.Success)
                {
                    string row = m.Groups[2].ToString();
                    string col = StringExtensions.ExcelColumnLettersToIndex(m.Groups[1].ToString()).ToString();
                    return this[row, col];
                }
                else
                {
                    return null;
                    //return (string)(string.Join(csvDelimiter.ToString(), this.Rows[a1].Cells.Select(c => c.Value.value.ToString())));
                    //return this[a1, "1"]; //list, no table
                }
            }
            set
            {
                a1 = a1.ToString().ToUpper();
                Match m = a1Regex.Match(a1);
                if (m.Success)
                {
                    string row = m.Groups[2].ToString();
                    string col = StringExtensions.ExcelColumnLettersToIndex(m.Groups[1].ToString()).ToString();
                    this[row, col] = value;
                }
                else
                {
                    this[a1, "1"] = value; //list, no table
                }
            }
        }

        public Cell this[object r, object c]
        {
            get
            {
                return this[r.ToString(), c.ToString()];
            }
            set
            {
                this[r.ToString(), c.ToString()] = value;
            }
        }

        public Cell this[string r, string c]
        {
            get
            {
                TableRow row = null;
                Cell cell = null;
                lock (Rows)
                {
                    if (this.Rows.ContainsKey(r))
                        row = this.Rows[r];
                    else
                    {
                        row = new TableRow();
                        lock (Rows)
                        {
                            this.Rows.Add(r, row);
                        }
                    }
                }

                lock (row.Cells)
                {
                    if (row.Cells.ContainsKey(c))
                        cell = row.Cells[c];
                    else
                    {
                        cell = new Cell();
                        row.Cells.Add(c, cell);
                    }
                }

                ////create column headers on the fly
                //if (r == ColHeaderRowId)
                //{
                //    if (!ColumnHeaderDict.ContainsKey(c))
                //        ColumnHeaderDict.Add(c, new ColumnInfo());
                //}

                //cell.Value = () => DateTime.Now.Second; //TEST
                return cell;
            }
            set
            {
                TableRow row = null;
                Cell cell = null;
                lock (Rows)
                {
                    if (this.Rows.ContainsKey(r))
                        row = this.Rows[r];
                    else
                    {
                        row = new TableRow();
                        lock (Rows)
                        {

                            this.Rows.Add(r, row);
                        }
                    }
                }

                lock (row.Cells)
                {
                    if (!row.Cells.ContainsKey(c))
                    {
                        cell = new Cell();
                        lock (row.Cells)
                        {

                            row.Cells.Add(c, cell);
                        }
                    }

                    //cell = value;
                    row.Cells[c] = value as Cell;
                }

            }
        }



        public Cell this[string idCol, string idValue, string valueId]
        {
            get
            {
                for (int row = 1; row <= RowCount; row++)
                {
                    Table.Cell idCell = this.Rows["" + row].Cells[idCol];
                    if ((idCell != null) && (idCell.Value != null) && (idCell.Value.ToString().Trim().ToUpper() == idValue.Trim().ToUpper()))
                        return this.Rows["" + row].Cells[valueId];
                }

                string newRowID = AddRow();
                this.Rows[newRowID] = new TableRow();
                for (int c = 1; c < 100; c++)
                {
                    this.Rows[newRowID].Cells["" + c] = new Cell();
                    if ("" + c == valueId.Trim())
                        break;
                }

                this.Rows[newRowID].Cells[idCol].Value = idValue;
                return this.Rows[newRowID].Cells[valueId];
            }
        }
    }
}

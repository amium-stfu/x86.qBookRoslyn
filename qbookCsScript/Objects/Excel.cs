using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace QB
{
    /// <summary>
    /// Provides write access to an XLSX copy while preserving the existing cell formatting.
    /// </summary>
    /// <remarks>
    /// <para>Typical usage with automatic disposal:</para>
    /// <code>using (var excel = Excel.Open(sourcePath, targetPath))
    /// {
    ///     excel["A1"] = "Text";
    ///     excel[1, 2] = 123;
    /// }</code>
    /// <para>Worksheet selection can be done during open or later:</para>
    /// <code>using (var excel = Excel.Open(sourcePath, targetPath, sheetName: "Report"))
    /// {
    ///     excel.SelectWorksheet("Summary");
    ///     excel.Set(col: 3, row: 5, value: DateTime.Now);
    /// }</code>
    /// <para>Writes to merged cells are not redirected. If a requested cell belongs to a merged range but is not the top-left anchor cell, an exception is thrown.</para>
    /// </remarks>
    public sealed class Excel : IDisposable
    {
        private static readonly Regex CellReferenceRegex = new Regex(@"^[A-Za-z]+[1-9][0-9]*$", RegexOptions.Compiled);

        private readonly SpreadsheetDocument _document;
        private readonly WorkbookPart _workbookPart;
        private WorksheetPart _worksheetPart;
        private Sheet _sheet;
        private bool _disposed;
        private bool _isDirty;

        /// <summary>
        /// Initializes a new instance for the copied target workbook.
        /// </summary>
        /// <param name="sourcePath">The source XLSX file that acts as the template.</param>
        /// <param name="targetPath">The writable target XLSX file that will be created or overwritten.</param>
        /// <param name="sheetName">The optional worksheet name. If omitted, the first worksheet is used.</param>
        public Excel(string sourcePath, string targetPath, string sheetName = null)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("Source path is required.", nameof(sourcePath));

            if (string.IsNullOrWhiteSpace(targetPath))
                throw new ArgumentException("Target path is required.", nameof(targetPath));

            sourcePath = Path.GetFullPath(sourcePath);
            targetPath = Path.GetFullPath(targetPath);

            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Source XLSX file was not found.", sourcePath);

            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.Copy(sourcePath, targetPath, true);

            SourcePath = sourcePath;
            TargetPath = targetPath;
            _document = SpreadsheetDocument.Open(targetPath, true);
            _workbookPart = _document.WorkbookPart ?? throw new InvalidOperationException("Workbook part is missing.");

            SelectWorksheet(sheetName);
        }

        /// <summary>
        /// Gets the source XLSX path.
        /// </summary>
        public string SourcePath { get; }

        /// <summary>
        /// Gets the writable target XLSX path.
        /// </summary>
        public string TargetPath { get; }

        /// <summary>
        /// Gets the active worksheet name.
        /// </summary>
        public string WorksheetName
        {
            get { return _sheet != null && _sheet.Name != null ? _sheet.Name.Value : null; }
        }

        /// <summary>
        /// Opens an XLSX template and creates a writable copy.
        /// </summary>
        /// <param name="sourcePath">The source XLSX file that acts as the template.</param>
        /// <param name="targetPath">The writable target XLSX file that will be created or overwritten.</param>
        /// <param name="sheetName">The optional worksheet name. If omitted, the first worksheet is used.</param>
        /// <returns>A writable workbook wrapper.</returns>
        /// <remarks>
        /// Pass <paramref name="sheetName" /> to select the initial worksheet directly when opening the workbook copy.
        /// </remarks>
        public static Excel Open(string sourcePath, string targetPath, string sheetName = null)
        {
            return new Excel(sourcePath, targetPath, sheetName);
        }

        /// <summary>
        /// Gets or sets a cell by Excel address such as <c>A1</c>.
        /// </summary>
        /// <param name="cellReference">The Excel cell reference.</param>
        /// <returns>The raw cell value.</returns>
        public object this[string cellReference]
        {
            get { return Get(cellReference); }
            set { Set(cellReference, value); }
        }

        /// <summary>
        /// Gets or sets a cell by one-based column and row.
        /// </summary>
        /// <param name="col">The one-based column index.</param>
        /// <param name="row">The one-based row index.</param>
        /// <returns>The raw cell value.</returns>
        public object this[int col, int row]
        {
            get { return Get(col, row); }
            set { Set(col, row, value); }
        }

        /// <summary>
        /// Changes the active worksheet.
        /// </summary>
        /// <param name="sheetName">The worksheet name. If omitted, the first worksheet is selected.</param>
        /// <remarks>
        /// This method changes the worksheet used by all following <c>Get</c>, <c>Set</c> and indexer operations.
        /// </remarks>
        public void SelectWorksheet(string sheetName = null)
        {
            ThrowIfDisposed();

            var sheets = _workbookPart.Workbook.Descendants<Sheet>().ToList();
            if (sheets.Count == 0)
                throw new InvalidOperationException("Workbook does not contain any worksheets.");

            Sheet sheet;
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                sheet = sheets.First();
            }
            else
            {
                sheet = sheets.FirstOrDefault(s => string.Equals(s.Name != null ? s.Name.Value : null, sheetName, StringComparison.OrdinalIgnoreCase));
                if (sheet == null)
                    throw new ArgumentException("Worksheet was not found.", nameof(sheetName));
            }

            var relationshipId = sheet.Id != null ? sheet.Id.Value : null;
            if (string.IsNullOrWhiteSpace(relationshipId))
                throw new InvalidOperationException("Worksheet relationship is missing.");

            _sheet = sheet;
            _worksheetPart = (WorksheetPart)_workbookPart.GetPartById(relationshipId);
        }

        /// <summary>
        /// Gets the raw value of a cell by Excel address.
        /// </summary>
        /// <param name="cellReference">The Excel cell reference.</param>
        /// <returns>The raw cell value or <c>null</c>.</returns>
        public object Get(string cellReference)
        {
            ThrowIfDisposed();
            cellReference = NormalizeCellReference(cellReference);
            var cell = FindCell(cellReference);
            return ReadCellValue(cell);
        }

        /// <summary>
        /// Gets the raw value of a cell by one-based column and row.
        /// </summary>
        /// <param name="col">The one-based column index.</param>
        /// <param name="row">The one-based row index.</param>
        /// <returns>The raw cell value or <c>null</c>.</returns>
        public object Get(int col, int row)
        {
            return Get(ToCellReference(col, row));
        }

        /// <summary>
        /// Writes a value into a cell by Excel address.
        /// </summary>
        /// <param name="cellReference">The Excel cell reference.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="cellReference" /> belongs to a merged range but is not the top-left anchor cell.
        /// </exception>
        public void Set(string cellReference, object value)
        {
            ThrowIfDisposed();
            cellReference = NormalizeCellReference(cellReference);
            cellReference = ResolveWritableCellReference(cellReference);

            var cell = GetOrCreateCell(cellReference);
            WriteCellValue(cell, value);
            MarkWorkbookForRecalculation();
            _worksheetPart.Worksheet.Save();
            _isDirty = true;
        }

        /// <summary>
        /// Writes a value into a cell by one-based column and row.
        /// </summary>
        /// <param name="col">The one-based column index.</param>
        /// <param name="row">The one-based row index.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the addressed cell belongs to a merged range but is not the top-left anchor cell.
        /// </exception>
        public void Set(int col, int row, object value)
        {
            Set(ToCellReference(col, row), value);
        }

        /// <summary>
        /// Saves the modified workbook copy.
        /// </summary>
        public void Save()
        {
            ThrowIfDisposed();

            if (!_isDirty)
                return;

            MarkWorkbookForRecalculation();
            _worksheetPart.Worksheet.Save();
            _workbookPart.Workbook.Save();
            _isDirty = false;
        }

        /// <summary>
        /// Saves pending changes and releases the workbook.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            Save();
            _document.Dispose();
            _disposed = true;
        }

        private void MarkWorkbookForRecalculation()
        {
            var workbook = _workbookPart.Workbook;
            var calculationProperties = workbook.GetFirstChild<CalculationProperties>();
            if (calculationProperties == null)
            {
                calculationProperties = new CalculationProperties();
                workbook.Append(calculationProperties);
            }

            calculationProperties.CalculationMode = CalculateModeValues.Auto;
            calculationProperties.ForceFullCalculation = true;
            calculationProperties.FullCalculationOnLoad = true;

            if (_workbookPart.CalculationChainPart != null)
                _workbookPart.DeletePart(_workbookPart.CalculationChainPart);
        }

        private static string NormalizeCellReference(string cellReference)
        {
            if (string.IsNullOrWhiteSpace(cellReference))
                throw new ArgumentException("Cell reference is required.", nameof(cellReference));

            cellReference = cellReference.Trim().ToUpperInvariant();
            if (!CellReferenceRegex.IsMatch(cellReference))
                throw new ArgumentException("Cell reference must look like A1.", nameof(cellReference));

            return cellReference;
        }

        private static string ToCellReference(int col, int row)
        {
            if (col < 1)
                throw new ArgumentOutOfRangeException(nameof(col), "Column index must be greater than zero.");

            if (row < 1)
                throw new ArgumentOutOfRangeException(nameof(row), "Row index must be greater than zero.");

            return GetColumnName(col) + row.ToString(CultureInfo.InvariantCulture);
        }

        private string ResolveWritableCellReference(string requestedReference)
        {
            var mergedRange = FindMergedRange(requestedReference);
            if (mergedRange == null)
                return requestedReference;

            if (!string.Equals(mergedRange.StartReference, requestedReference, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Cell '{0}' belongs to merged range '{1}'. Write to anchor cell '{2}' instead.",
                        requestedReference,
                        mergedRange.RangeReference,
                        mergedRange.StartReference));
            }

            return mergedRange.StartReference;
        }

        private MergedRangeInfo FindMergedRange(string cellReference)
        {
            var worksheet = _worksheetPart.Worksheet;
            var mergeCells = worksheet.Elements<MergeCells>().FirstOrDefault();
            if (mergeCells == null)
                return null;

            CellAddress requestedAddress;
            if (!TryParseCellAddress(cellReference, out requestedAddress))
                return null;

            foreach (var mergeCell in mergeCells.Elements<MergeCell>())
            {
                var rangeReference = mergeCell.Reference != null ? mergeCell.Reference.Value : null;
                if (string.IsNullOrWhiteSpace(rangeReference))
                    continue;

                var boundaries = rangeReference.Split(':');
                if (boundaries.Length != 2)
                    continue;

                CellAddress start;
                CellAddress end;
                if (!TryParseCellAddress(boundaries[0], out start) || !TryParseCellAddress(boundaries[1], out end))
                    continue;

                if (requestedAddress.ColumnIndex >= start.ColumnIndex && requestedAddress.ColumnIndex <= end.ColumnIndex &&
                    requestedAddress.RowIndex >= start.RowIndex && requestedAddress.RowIndex <= end.RowIndex)
                {
                    return new MergedRangeInfo
                    {
                        RangeReference = rangeReference.ToUpperInvariant(),
                        StartReference = start.Reference,
                    };
                }
            }

            return null;
        }

        private Cell FindCell(string cellReference)
        {
            CellAddress address;
            if (!TryParseCellAddress(cellReference, out address))
                return null;

            var sheetData = _worksheetPart.Worksheet.GetFirstChild<SheetData>();
            if (sheetData == null)
                return null;

            var row = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex != null && r.RowIndex.Value == address.RowIndex);
            if (row == null)
                return null;

            return row.Elements<Cell>().FirstOrDefault(c => string.Equals(c.CellReference != null ? c.CellReference.Value : null, cellReference, StringComparison.OrdinalIgnoreCase));
        }

        private Cell GetOrCreateCell(string cellReference)
        {
            CellAddress address;
            if (!TryParseCellAddress(cellReference, out address))
                throw new ArgumentException("Cell reference must look like A1.", nameof(cellReference));

            var sheetData = _worksheetPart.Worksheet.GetFirstChild<SheetData>();
            if (sheetData == null)
            {
                sheetData = new SheetData();
                _worksheetPart.Worksheet.Append(sheetData);
            }

            var row = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex != null && r.RowIndex.Value == address.RowIndex);
            if (row == null)
            {
                row = new Row { RowIndex = address.RowIndex };
                var referenceRow = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex != null && r.RowIndex.Value > address.RowIndex);
                if (referenceRow == null)
                    sheetData.Append(row);
                else
                    sheetData.InsertBefore(row, referenceRow);
            }

            var cell = row.Elements<Cell>().FirstOrDefault(c => string.Equals(c.CellReference != null ? c.CellReference.Value : null, cellReference, StringComparison.OrdinalIgnoreCase));
            if (cell != null)
                return cell;

            cell = new Cell { CellReference = cellReference };
            var nextCell = row.Elements<Cell>()
                .FirstOrDefault(c => string.Compare(c.CellReference != null ? c.CellReference.Value : null, cellReference, StringComparison.OrdinalIgnoreCase) > 0);

            if (nextCell == null)
                row.Append(cell);
            else
                row.InsertBefore(cell, nextCell);

            return cell;
        }

        private object ReadCellValue(Cell cell)
        {
            if (cell == null)
                return null;

            if (cell.DataType == null)
                return cell.CellValue != null ? (object)cell.CellValue.Text : cell.InnerText;

            switch (cell.DataType.Value)
            {
                case CellValues.SharedString:
                    return ReadSharedString(cell);
                case CellValues.Boolean:
                    return cell.CellValue != null && cell.CellValue.Text == "1";
                case CellValues.InlineString:
                    return cell.InlineString != null ? cell.InlineString.InnerText : cell.InnerText;
                default:
                    return cell.CellValue != null ? (object)cell.CellValue.Text : cell.InnerText;
            }
        }

        private string ReadSharedString(Cell cell)
        {
            if (cell.CellValue == null || _workbookPart.SharedStringTablePart == null || _workbookPart.SharedStringTablePart.SharedStringTable == null)
                return cell.InnerText;

            int index;
            if (!int.TryParse(cell.CellValue.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                return cell.CellValue.Text;

            return _workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAtOrDefault(index)?.InnerText;
        }

        private static void WriteCellValue(Cell cell, object value)
        {
            cell.CellFormula = null;
            cell.InlineString = null;

            if (value == null || value == DBNull.Value)
            {
                cell.CellValue = null;
                cell.DataType = null;
                return;
            }

            if (value is bool)
            {
                cell.DataType = CellValues.Boolean;
                cell.CellValue = new CellValue((bool)value ? "1" : "0");
                return;
            }

            if (value is DateTime)
            {
                cell.DataType = null;
                cell.CellValue = new CellValue(((DateTime)value).ToOADate().ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (value is DateTimeOffset)
            {
                cell.DataType = null;
                cell.CellValue = new CellValue(((DateTimeOffset)value).DateTime.ToOADate().ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (IsNumericType(value.GetType()))
            {
                cell.DataType = null;
                cell.CellValue = new CellValue(Convert.ToString(value, CultureInfo.InvariantCulture));
                return;
            }

            cell.DataType = CellValues.String;
            cell.CellValue = new CellValue(Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        private static bool IsNumericType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type == typeof(byte)
                || type == typeof(sbyte)
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        private static bool TryParseCellAddress(string cellReference, out CellAddress address)
        {
            address = null;
            if (string.IsNullOrWhiteSpace(cellReference))
                return false;

            cellReference = cellReference.Trim().ToUpperInvariant();
            if (!CellReferenceRegex.IsMatch(cellReference))
                return false;

            var letters = new string(cellReference.TakeWhile(char.IsLetter).ToArray());
            var digits = new string(cellReference.SkipWhile(char.IsLetter).ToArray());

            uint rowIndex;
            if (!uint.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out rowIndex))
                return false;

            address = new CellAddress
            {
                Reference = cellReference,
                ColumnIndex = GetColumnIndex(letters),
                RowIndex = rowIndex,
            };
            return true;
        }

        private static int GetColumnIndex(string columnName)
        {
            var columnIndex = 0;
            foreach (var character in columnName)
                columnIndex = (columnIndex * 26) + (character - 'A' + 1);

            return columnIndex;
        }

        private static string GetColumnName(int columnIndex)
        {
            var characters = new Stack<char>();
            while (columnIndex > 0)
            {
                columnIndex--;
                characters.Push((char)('A' + (columnIndex % 26)));
                columnIndex /= 26;
            }

            return new string(characters.ToArray());
        }

        private sealed class CellAddress
        {
            public string Reference { get; set; }

            public int ColumnIndex { get; set; }

            public uint RowIndex { get; set; }
        }

        private sealed class MergedRangeInfo
        {
            public string RangeReference { get; set; }

            public string StartReference { get; set; }
        }
    }
}

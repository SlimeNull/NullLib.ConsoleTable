using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NullLib.ConsoleEx;

namespace NullLib.ConsoleTable
{
    public class ConsoleTable
    {
        List<object> columns;
        List<List<object>> rows;
        List<ColumnAlignment> alignments;

        private int tableMinWidth = 0;
        private int tableMaxWidth = int.MaxValue;

        public static TableFormatOption DefaultFormat = new TableFormatOption('-', " --", "---", "-- ", " | ", " | ", " | ", true, true, true, true);
        public static TableFormatOption MarkdownFormat = new TableFormatOption('-', "|-", "-|-", "-|", "| ", " | ", " |", false, false, false, true);
        public static TableFormatOption AlternativeFormat = new TableFormatOption('-', "+-", "-+-", "-+", "| ", " | ", " |", true, true, true, true);
        public static TableFormatOption MinimalFormat = new TableFormatOption('-', "", "-", "", "", " ", "", false, false, false, true);

        public int TableMinimiumWidth
        {
            get => tableMinWidth; set
            {
                if (value > tableMaxWidth)
                    throw new ArgumentOutOfRangeException(nameof(value), "MaximiumWidth must greater than MainimiumWidth");
                tableMinWidth = value;
            }
        }
        public int TableMaximiumWidth
        {
            get => tableMaxWidth; set
            {
                if (value < tableMinWidth)
                    throw new ArgumentOutOfRangeException(nameof(value), "MaximiumWidth must greater than MainimiumWidth");
                tableMaxWidth = value;
            }
        }

        public ConsoleTable()
        {
            columns = new List<object>();
            rows = new List<List<object>>();
            alignments = new List<ColumnAlignment>();
        }
        public ConsoleTable(params object[] headers) : this()
        {
            foreach (var i in headers)
            {
                this.columns.Add(i);
                alignments.Add(ColumnAlignment.Left);
            }
        }

        public ConsoleTable AddColumn(string header)
        {
            columns.Add(header);
            alignments.Add(ColumnAlignment.Left);
            return this;
        }
        public ConsoleTable AddColumn(string header, ColumnAlignment align)
        {
            columns.Add(header);
            alignments.Add(align);
            return this;
        }
        public ConsoleTable SetColumn(int index, string header)
        {
            columns[index] = header;
            return this;
        }
        public ConsoleTable SetColumnAlignment(int index, ColumnAlignment align)
        {
            alignments[index] = align;
            return this;
        }
        public ConsoleTable RemoveColumn(int index)
        {
            columns.RemoveAt(index);
            alignments.RemoveAt(index);
            foreach (var row in rows)
                row.RemoveAt(index);
            return this;
        }
        public ConsoleTable AddRow(params object[] values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            if (columns.Count != values.Length)
                throw new Exception(
                    $"The number columns in the row ({columns.Count}) does not match the values ({values.Length})");

            rows.Add(new List<object>(values));
            return this;
        }
        public ConsoleTable SetRow(int index, params object[] values)
        {
            if (values is null)
                throw new ArgumentNullException(nameof(values));

            if (columns.Count != values.Length)
                throw new Exception(
                    $"The number columns in the row ({columns.Count}) does not match the values ({values.Length})");

            rows[index] = new List<object>(values);
            return this;
        }
        public ConsoleTable RemoveRow(int index)
        {
            rows.RemoveAt(index);
            return this;
        }

        private static void DealMinimiumWidth(ref int[] widths, int borderWidth, int contentWidth, int minWidth)
        {
            int additionWidth = minWidth - borderWidth - contentWidth;
            int additionWidthFinal = additionWidth;
            if (additionWidth > 0)
            {
                for (int i = 0, len = widths.Length; i < len; i++)
                {
                    int ii = (int)Math.Round(((float)widths[i] / contentWidth) * additionWidthFinal);
                    widths[i] += ii;
                    additionWidth -= ii;
                    if (additionWidth < 0)
                    {
                        widths[i] += additionWidth;
                        break;
                    }
                }
            }
        }
        private static void DealMaximiumWidth(ref int[] widths, int borderWidth, int contentWidth, int maxWidth)
        {
            int reducedWidth = borderWidth + contentWidth - maxWidth;
            int reducedWidthFinal = reducedWidth;
            if (reducedWidth > 0)
            {
                for (int i = 0, len = widths.Length; i < len; i++)
                {
                    int ii = (int)Math.Round(((float)widths[i] / contentWidth) * reducedWidthFinal);
                    widths[i] -= ii;
                    reducedWidth -= ii;
                    if (reducedWidth < 0)
                    {
                        widths[i] -= reducedWidth;
                        break;
                    }
                }
            }
        }
        private static string FormatString(ColumnAlignment align, int length, string str)
        {
            return align switch
            {
                ColumnAlignment.Left => str.PadRight(length - (ConsoleText.CalcStringLength(str) - str.Length)),
                ColumnAlignment.Right => str.PadLeft(length - (ConsoleText.CalcStringLength(str) - str.Length)),
                _ => string.Empty
            };
        }
        private static IEnumerable<string> FormatCell(ColumnAlignment align, int length, object cell)
        {
            StringBuilder sb = new StringBuilder();
            int len = 0;
            foreach (char i in cell.ToString())
            {
                int charLen = ConsoleText.CalcCharLength(i);
                if (len + charLen > length)
                {
                    yield return FormatString(align, length, sb.ToString());
                    sb.Clear();
                    len = 0;
                }
                sb.Append(i);
                len += charLen;
            }
            if (sb.Length > 0)
                yield return FormatString(align, length, sb.ToString());
        }
        private static IEnumerable<IEnumerable<string>> FormatCells(IList<ColumnAlignment> aligns, IList<int> lengths, IList<object> row)
        {
            return row.Select((v, i) => FormatCell(aligns[i], lengths[i], v));
        }
        private string FormatRow(IList<ColumnAlignment> aligns, IList<int> lengths, IList<object> row, TableFormatOption option)
        {
            string[][] formatedCells = FormatCells(aligns, lengths, row).Select(v => v.ToArray()).ToArray();
            int width = row.Count;
            int height = formatedCells.Select(v => v.Length).Max();
            string[,] result = new string[height, width];

            for (int i = 0, j = 0; i < width; i++, j = 0)
            {
                for (int len2 = formatedCells[i].Length; j < len2; j++)
                    result[j, i] = formatedCells[i][j];
                for (; j < height; j++)
                    result[j, i] = FormatString(aligns[i], lengths[i], "");
            }

            return option.RowStart + string.Join(option.RowEnd + Environment.NewLine + option.RowStart,
                Enumerable
                    .Range(0, height)
                    .Select(t => string.Join(option.RowStep,
                                                Enumerable
                                                    .Range(0, width)
                                                    .Select(k => result[t, k])))) + option.RowEnd;
        }

        private int[] GetMaxColumnLengths()
        {
            return columns
                .Select((t, i) => rows.Select(v => v[i])
                    .Union(new[] { columns[i] })
                    .Where(x => x != null)
                    .Select(x => ConsoleText.CalcStringLength(x.ToString()))
                    .Max())
                .ToArray();
        }
        private void DealTableWidth(ref int[] widths, TableFormatOption option)
        {
            int columnCount = widths.Length;
            int totalColumnWidth = widths.Sum();
            int frameworkWidth =
                Math.Max(ConsoleText.CalcStringLength(option.DividerStart), ConsoleText.CalcStringLength(option.RowStart)) +
                Math.Max(ConsoleText.CalcStringLength(option.DividerEnd), ConsoleText.CalcStringLength(option.RowEnd)) +
                Math.Max(ConsoleText.CalcStringLength(option.DividerStep), ConsoleText.CalcStringLength(option.RowStep)) * (columnCount - 1);

            DealMinimiumWidth(ref widths, frameworkWidth, totalColumnWidth, tableMinWidth);
            DealMaximiumWidth(ref widths, frameworkWidth, totalColumnWidth, tableMaxWidth);
        }

        public string FormatTable(TableFormatOption option)
        {
            StringBuilder sb = new StringBuilder();
            int[] lens = GetMaxColumnLengths();
            DealTableWidth(ref lens, option);

            string divider = option.DividerStart + string.Join(option.DividerStep, lens.Select(v => new string(option.DividerChar, v))) + option.DividerEnd;
            string headers = FormatRow(alignments, lens, columns, option);

            IEnumerable<string> rowstrs = rows.Select(v => FormatRow(alignments, lens, v, option));

            if (option.TableTopLine)
                sb.AppendLine(divider);
            sb.AppendLine(headers);
            if (option.HeaderUnderLine)
                sb.AppendLine(divider);

            if (option.RowStepLine)
                sb.AppendLine(string.Join(Environment.NewLine + divider + Environment.NewLine, rowstrs));
            else
                sb.AppendLine(string.Join(Environment.NewLine, rowstrs));

            if (option.TableBottomLine)
                sb.Append(divider);


            return sb.ToString();
        }

        public string ToDefaultString()
        {
            return FormatTable(DefaultFormat);
        }
        public string ToMarkdownString()
        {
            return FormatTable(MarkdownFormat);
        }
        public string ToAlternativeString()
        {
            return FormatTable(AlternativeFormat);
        }
        public string ToMinimalString()
        {
            return FormatTable(MinimalFormat);
        }
    }
    public struct TableFormatOption
    {
        public char DividerChar;
        public string DividerStart;
        public string DividerStep;
        public string DividerEnd;
        public string RowStart;
        public string RowStep;
        public string RowEnd;
        public bool TableTopLine;
        public bool TableBottomLine;
        public bool RowStepLine;
        public bool HeaderUnderLine;

        public TableFormatOption(
            char dividerChar,
            string dividerStart,
            string dividerStep,
            string dividerEnd,
            string rowStart,
            string rowStep,
            string rowEnd,
            bool tableTopLine,
            bool tableBottomLine,
            bool rowStepLine,
            bool headerUnderLine)
        {
            this.DividerChar = dividerChar;
            this.DividerStart = dividerStart;
            this.DividerStep = dividerStep;
            this.DividerEnd = dividerEnd;
            this.RowStart = rowStart;
            this.RowStep = rowStep;
            this.RowEnd = rowEnd;
            this.TableTopLine = tableTopLine;
            this.TableBottomLine = tableBottomLine;
            this.RowStepLine = rowStepLine;
            this.HeaderUnderLine = headerUnderLine;
        }
    }
}

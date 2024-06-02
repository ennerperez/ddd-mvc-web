using System;
using System.Diagnostics;
#if !USING_SPECFLOW
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#endif

namespace Test.Framework.Extended
{
    [DebuggerStepThrough]
    internal static class Assert
    {
#if XUNIT
        public static void Result(bool value, string message = "") => Xunit.Assert.True(!value, message);

        public static void Fail(string message = "") => Xunit.Assert.Fail(message);

        public static void Fail(Exception exception, bool stack = false)
        {
            var message = $"{exception.Message}{(stack ? $"\r\n\r\n{exception.StackTrace}" : "")}";
            Fail(message);
        }

        public static void Pass(string message = "") => Xunit.Assert.True(true, message);
#else
        public static void Result(bool value, string message = "")
        {
            if (value) Pass(message);
            else Fail(message);
        }

        public static void Fail(string message = "")
        {
            NUnit.Framework.Assert.Fail(message);
        }

        public static void Fail(Exception exception, bool stack = false)
        {
            var message = $"{exception.Message}{(stack ? $"\r\n\r\n{exception.StackTrace}" : "")}";
            Fail(message);
        }

        public static void Pass(string message = "")
        {
            NUnit.Framework.Assert.Pass(message);
        }
#endif
    }

#if !USING_SPECFLOW
    [Serializable]
    public class TableRow : IDictionary<string, string>
    {
        private readonly Table _table;
        private readonly string[] _items;

        internal TableRow(Table table, string[] items)
        {
            for (var colIndex = 0; colIndex < items.Length; colIndex++)
                items[colIndex] ??= string.Empty;

            _table = table;
            _items = items;
        }

        public string this[string header]
        {
            get
            {
                var itemIndex = _table.GetHeaderIndex(header);
                return _items[itemIndex];
            }
            set
            {
                var keyIndex = _table.GetHeaderIndex(header);
                _items[keyIndex] = value;
            }
        }

        public string this[int index] => _items[index];
        public int Count => _items.Length;

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            Debug.Assert(_items.Length == _table.Header.Count);
            var itemIndex = 0;
            foreach (var header in _table.Header)
            {
                yield return new KeyValuePair<string, string>(header, _items[itemIndex]);
                itemIndex++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

#pragma warning disable CA1822
        private Exception ThrowTableStructureCannotBeModified()
#pragma warning restore CA1822
        {
            return new Exception("The table rows must contain the same number of items as the header count of the table. The structure cannot be modified.");
        }

    #region Implementation of ICollection<KeyValuePair<string,string>>

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
        {
            throw ThrowTableStructureCannotBeModified();
        }

        void ICollection<KeyValuePair<string, string>>.Clear()
        {
            throw ThrowTableStructureCannotBeModified();
        }

        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
        {
            var keyIndex = _table.GetHeaderIndex(item.Key, false);
            if (keyIndex < 0)
                return false;
            return _items[keyIndex].Equals(item.Value);
        }

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw ThrowTableStructureCannotBeModified();
        }

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
        {
            throw ThrowTableStructureCannotBeModified();
        }

        bool ICollection<KeyValuePair<string, string>>.IsReadOnly => false;

    #endregion

    #region Implementation of IDictionary<string,string>

        public bool ContainsKey(string key)
        {
            return _table.Header.Contains(key);
        }

        void IDictionary<string, string>.Add(string key, string value)
        {
            throw ThrowTableStructureCannotBeModified();
        }

        bool IDictionary<string, string>.Remove(string key)
        {
            throw ThrowTableStructureCannotBeModified();
        }

        public bool TryGetValue(string key, out string value)
        {
            var keyIndex = _table.GetHeaderIndex(key, false);
            if (keyIndex < 0)
            {
                value = null;
                return false;
            }

            value = _items[keyIndex];
            return true;
        }

        public ICollection<string> Keys => _table.Header;

        public ICollection<string> Values => _items;

    #endregion
    }

    [Serializable]
    public class TableRows : IEnumerable<TableRow>
    {
        private readonly List<TableRow> _innerList = new();

        public int Count => _innerList.Count;

        public TableRow this[int index] => _innerList[index];

        public IEnumerator<TableRow> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal void Add(TableRow row)
        {
            _innerList.Add(row);
        }
    }

    [Serializable]
    public class Table
    {
        internal const string ErrorNoCellsToAdd = "No cells to add";
        internal const string ErrorNoHeaderToAdd = "No headers to add";
        internal const string ErrorColumnNameNotFound = "Could not find a column named '{0}' in the table.";
        internal const string ErrorCellsNotMatchingHeaders = "The number of cells ({0}) you are trying to add doesn't match the number of columns ({1})";

        private readonly string[] _header;
        private readonly TableRows _rows = new();

        public ICollection<string> Header => _header;

        public TableRows Rows => _rows;

        public int RowCount => _rows.Count;

        public Table(params string[] header)
        {
            if (header == null || header.Length == 0)
            {
                throw new ArgumentException(ErrorNoHeaderToAdd, nameof(header));
            }

            for (var colIndex = 0; colIndex < header.Length; colIndex++)
                header[colIndex] ??= string.Empty;
            _header = header;
        }

        public bool ContainsColumn(string column)
        {
            return GetHeaderIndex(column, false) >= 0;
        }

        internal int GetHeaderIndex(string column, bool throwIfNotFound = true)
        {
            var index = Array.IndexOf(_header, column);
            if (!throwIfNotFound)
                return index;
            if (index < 0)
            {
                var mess = string.Format(
                    ErrorColumnNameNotFound + "\nThe table looks like this:\n{1}",
                    column,
                    this);
                throw new IndexOutOfRangeException(mess);
            }

            return index;
        }

        public void AddRow(IDictionary<string, string> values)
        {
            var cells = new string[_header.Length];
            foreach (var value in values)
            {
                var headerIndex = GetHeaderIndex(value.Key);
                cells[headerIndex] = value.Value;
            }

            AddRow(cells);
        }

        public void AddRow(params string[] cells)
        {
            if (cells == null)
                throw new Exception(ErrorNoCellsToAdd);

            if (cells.Length != _header.Length)
            {
                var mess =
                    string.Format(
                        ErrorCellsNotMatchingHeaders + ".\nThe table looks like this\n{2}",
                        cells.Length,
                        _header.Length,
                        this);
                throw new ArgumentException(mess);
            }

            var row = new TableRow(this, cells);
            _rows.Add(row);
        }

        public void RenameColumn(string oldColumn, string newColumn)
        {
            var colIndex = GetHeaderIndex(oldColumn);
            _header[colIndex] = newColumn;
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool headersOnly, bool withNewline = true)
        {
            var columnWidths = new int[_header.Length];
            for (var colIndex = 0; colIndex < _header.Length; colIndex++)
                columnWidths[colIndex] = _header[colIndex].Length;

            if (!headersOnly)
            {
                foreach (var row in _rows)
                {
                    for (var colIndex = 0; colIndex < _header.Length; colIndex++)
                        columnWidths[colIndex] = Math.Max(columnWidths[colIndex], row[colIndex].Length);
                }
            }

            var builder = new StringBuilder();
            AddTableRow(builder, _header, columnWidths);

            if (!headersOnly)
            {
                foreach (var row in _rows)
                    AddTableRow(builder, row.Select(pair => pair.Value), columnWidths);
            }

            if (!withNewline)
            {
                var newlineLength = Environment.NewLine.Length;
                builder.Remove(builder.Length - newlineLength, newlineLength);
            }

            return builder.ToString();
        }

#pragma warning disable CA1822
        private void AddTableRow(StringBuilder builder, IEnumerable<string> cells, int[] widths)
#pragma warning restore CA1822
        {
            const string margin = " ";
            const string separator = "|";
            var colIndex = 0;

            builder.Append(separator);
            foreach (var cell in cells)
            {
                builder.Append(margin);

                builder.Append(cell);
                builder.Append(' ', widths[colIndex] - cell.Length);

                builder.Append(margin);
                builder.Append(separator);

                colIndex++;
            }

            builder.AppendLine();
        }

        /* Extensions */

        public T GetValue<T>(string key, bool ignoreCase = true, string fieldColum = "Field", string valueColum = "Value")
        {
            var table = this;

            var type = typeof(T);
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition();

            if (type.IsEnum || isNullable && Nullable.GetUnderlyingType(type)!.IsEnum)
            {
                if (!isNullable)
                {
                    return table.GetValue<string>(key).AsEnum<T>();
                }
                else
                {
                    return (T)table.GetValue<string>(key).AsEnum(Nullable.GetUnderlyingType(type));
                }
            }

            var stringComparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.Ordinal;
            if (table.Rows.Any(m => m[fieldColum].Equals(key, stringComparison)))
            {
                var value = table.Rows.FirstOrDefault(m => m[fieldColum].Equals(key, stringComparison))?[valueColum];

                if (isNullable)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return default(T);
                    }

                    return (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? throw new InvalidOperationException());
                }

                return (T)Convert.ChangeType(value, type);
            }

            return default(T);
        }

        public object GetValue(string key, Type type, bool ignoreCase = true, string fieldColum = "Field", string valueColum = "Value")
        {
            var table = this;
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition();

            if (type.IsEnum || isNullable && Nullable.GetUnderlyingType(type)!.IsEnum)
            {
                if (!isNullable)
                {
                    return table.GetValue<string>(key).AsEnum(type);
                }
                else
                {
                    return table.GetValue<string>(key).AsEnum(Nullable.GetUnderlyingType(type));
                }
            }

            var stringComparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.Ordinal;
            if (table.Rows.Any(m => m[fieldColum].Equals(key, stringComparison)))
            {
                var value = table.Rows.FirstOrDefault(m => m[fieldColum].Equals(key, stringComparison))?[valueColum];
                if (isNullable)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return default;
                    }

                    return Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? throw new InvalidOperationException());
                }

                return Convert.ChangeType(value, type);
            }

            return default;
        }

        public  T CastTo<T>(string fieldColum = "Field")
        {
            var table = this;
            var result = Activator.CreateInstance<T>();
            var props = typeof(T).GetProperties().Where(m => m.CanWrite).ToArray();
            string[] fields;
            if (!string.IsNullOrWhiteSpace(fieldColum))
            {
                fields = table.Rows.Select(m => m[fieldColum].ToLowerInvariant()).ToArray();
            }
            else
            {
                fields = table.Rows.Select(m => m[0].ToLowerInvariant()).ToArray();
            }

            var joint = props.Where(m => fields.Contains(m.Name.ToLowerInvariant())).ToArray();
            foreach (var prop in joint)
            {
                try
                {
                    var value = table.GetValue(prop.Name, prop.PropertyType);
                    if (value == null)
                    {
                        continue;
                    }

                    if (prop.PropertyType.IsEnum)
                    {
                        value = value.ToString().AsEnum(prop.PropertyType);
                    }

                    prop.SetValue(result, value);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return result;
        }

    }

#endif
}

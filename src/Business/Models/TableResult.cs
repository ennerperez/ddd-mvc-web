using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace Business.Models
{
    public class TableResult : TableResult<object>;

    public class TableResult<T> where T : class
    {
        public int iTotalRecords { get; set; }
        public int iTotalDisplayRecords { get; set; }
        public List<T> aaData { get; set; }
    }

    public class TableInfo
    {
        public int Draw { get; set; }

        public Column[] Columns { get; set; }

        public ColumnOrder[] Order { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        public Search Search { get; set; }

        public ColumnFilter[] Filters { get; set; }
    }

    public class Column
    {
        public string Data { get; set; }

        public string Name { get; set; }

        public bool Searchable { get; set; }

        public bool Orderable { get; set; }

        public Search Search { get; set; }
    }

    public class Search
    {
        public string Value { get; set; }

        public bool Regex { get; set; }
    }

    public class ColumnOrder
    {
        public int Column { get; set; }

        public string Dir { get; set; }
    }

    public class ColumnFilter
    {
        public string Column { get; set; }

        public string Value { get; set; }
    }
}

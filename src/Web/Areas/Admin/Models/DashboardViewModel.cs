using System;
using System.Collections.Generic;

namespace Web.Areas.Admin.Models
{
    public class DashboardItemViewModel<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {
        public TKey Key { get; set; }
        public string DisplayName { get; set; }

        public dynamic Metadata { get; set; }
    }

    public class DashboardCounterViewModel
    {
        public string DisplayName { get; set; }
        public long Counter { get; set; }

        public dynamic Metadata { get; set; }
    }

    public class DashboardViewModel
    {
        public IEnumerable<DashboardCounterViewModel> Counters { get; set; }
    }
}

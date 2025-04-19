using System;
using System.Collections.Generic;
using Domain.Abstractions;
using Domain.Enums;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class Setting : Entity<int>, IAuditable
    {
        public Setting()
        {
            CreatedAt = DateTime.Now;
            Type = Data.Text;
        }

        public Setting(string key, string value = null) : this()
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public Data Type { get; set; }
        public string Value { get; set; }

        #region IAuditable

        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        #endregion IAuditable
    }
}

using System;
using System.Collections.Generic;
using Domain.Enums;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class Setting : IEntity, IAuditable
    {
        public Setting()
        {
            Created = DateTime.Now;
            Type = Data.Text;
        }
        
        public Setting(string key) : this(key, null)
        {
        }

        public Setting(KeyValuePair<string, string> pair) : this(pair.Key, pair.Value)
        {
        }
        
        public Setting(string key, string value) : this()
        {
            Key = key;
            Value = value;
        }

        public int Id { get; set; }
        public string Key { get; set; }
        public Data Type { get; set; }
        public string Value { get; set; }

        #region IAuditable

        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }

        #endregion IAuditable
    }
}

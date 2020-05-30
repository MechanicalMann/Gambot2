using System;

namespace Gambot.Data
{
    public class DataStoreValue
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public DataStoreValue() {}

        public DataStoreValue(int id, string key, string value)
        {
            Id = id;
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }

        public string ToFullString() => $"{Key}: (#{Id}) {Value}";
    }
}
using System;

namespace Gambot.Data
{
    public class DataStoreValue
    {
        public int Id { get; protected set; }
        public string Key { get; protected set; }
        public string Value { get; protected set; }

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
    }
}
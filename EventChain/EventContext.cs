using System;
using System.Collections.Generic;

namespace EventChain
{
    public class EventContext
    {
        public Dictionary<string, object> Data { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        
        public T GetData<T>(string key)
        {
            if (Data.TryGetValue(key, out var value) && value is T typed)
            {
                return typed;
            }
            return default;
        }
        
        public void SetData<T>(string key, T value)
        {
            Data[key] = value;
        }
    }
}
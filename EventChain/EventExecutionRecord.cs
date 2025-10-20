using System;

namespace EventChain
{
    public class EventExecutionRecord
    {
        public string EventName { get; set; }
        public bool Success { get; set; }
        public float Precision { get; set; }
        public TimeSpan Duration { get; set; }
        public string Message { get; set; }
    }
}
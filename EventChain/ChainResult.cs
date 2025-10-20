using System;
using System.Collections.Generic;

namespace EventChain
{
    public class ChainResult<TContext> where TContext : EventContext
    {
        public bool Success { get; set; }
        public TContext Context { get; set; }
        public List<EventExecutionRecord> Events { get; set; } = new();
        public float Precision { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public string FailureReason { get; set; }
    }
}
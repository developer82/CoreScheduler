using System;
using System.Threading;

namespace CoreScheduler
{
    public class ScheduledTask
    {
        internal Timer Timer { get; set; }
        internal TimeSpan Period { get; set; }
        internal ITypedHandlerContainer Callback { get; set; }
        internal ITypedHandlerContainer ErrorCallback { get; set; }

        public string TaskId { get; private set; }
        public DateTime NextRun { get; set; }

        public ScheduledTask()
        {
            TaskId = new Guid().ToString().Substring(0, 5);
        }
    }
}

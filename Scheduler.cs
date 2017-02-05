using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CoreScheduler
{
    public class Scheduler
    {
        public static ConcurrentDictionary<string, ScheduledTask> Tasks { get; private set; } =
            new ConcurrentDictionary<string, ScheduledTask>();

        public enum Repeat
        {
            Never,
            EveryMinute,
            EveryTenMinutes,
            EveryHour,
            EveryDay,
            EveryWeek,
            NextMonth,
            NextYear
        }

        public Scheduler()
        {
        }

        /// <summary>
        /// Schedules a new task to be run at a specific time.
        /// </summary>
        /// <typeparam name="T">Type of state object being sent to the task</typeparam>
        /// <param name="callback">The task code you wish to be executed at the desired time.</param>
        /// <param name="when">Specific time to run the task.</param>
        /// <returns>ScheduledTask represanting the schduled task.</returns>
        public static ScheduledTask Schedule<T>(Action<T> callback, DateTime when)
        {
           return Schedule<T>(callback, null, default(T), when, TimeSpan.Zero);
        }

        /// <summary>
        /// Schedules a new task to be run at a specific time.
        /// </summary>
        /// <typeparam name="T">Type of state object being sent to the task</typeparam>
        /// <param name="callback">The task code you wish to be executed at the desired time.</param>
        /// <param name="when">Specific time to run the task.</param>
        /// <param name="repeat">Task repeat policy.</param>
        /// <returns>ScheduledTask represanting the schduled task.</returns>
        public static ScheduledTask Schedule<T>(Action<T> callback, DateTime when, Repeat repeat)
        {
            return Schedule<T>(callback, null, default(T), when, repeat);
        }

        /// <summary>
        /// Schedules a new task to be run at a specific time.
        /// </summary>
        /// <typeparam name="T">Type of state object being sent to the task</typeparam>
        /// <param name="callback">The task code you wish to be executed at the desired time.</param>
        /// <param name="onError">The code you with to execute in case of an exception in the scheduled task. The original T state object you sent and the exception will be sent you your callback function.</param>
        /// <param name="state">An object of type T to pass to the scheduled task.</param>
        /// <param name="when">Specific time to run the task.</param>
        /// <param name="repeat">Task repeat policy.</param>
        /// <returns>ScheduledTask represanting the schduled task.</returns>
        public static ScheduledTask Schedule<T>(Action<T> callback, Action<T, Exception> onError, T state, DateTime when, Repeat repeat)
        {
            TimeSpan period = GetTimeSpanFromRepeatPolicy(repeat);
            return Schedule<T>(callback, onError, state, when, period);
        }

        /// <summary>
        /// Schedules a new task to be run at a specific time.
        /// </summary>
        /// <typeparam name="T">Type of state object being sent to the task</typeparam>
        /// <param name="callback">The task code you wish to be executed at the desired time.</param>
        /// <param name="onError">The code you with to execute in case of an exception in the scheduled task. The original T state object you sent and the exception will be sent you your callback function.</param>
        /// <param name="state">An object of type T to pass to the scheduled task.</param>
        /// <param name="when">Specific time to run the task.</param>
        /// <param name="period">Timespan for automatic repetitaion of the task.</param>
        /// <returns>ScheduledTask represanting the schduled task.</returns>
        public static ScheduledTask Schedule<T>(Action<T> callback, Action<T, Exception> onError, T state, DateTime when, TimeSpan period)
        {
            TimeSpan dueTime = when - DateTime.Now;
            
            DateTime nextRun = DateTime.MinValue;
            if (period.TotalMilliseconds > 0)
            {
                nextRun = DateTime.Now.Add(period);
            }

            ScheduledTask scheduledTask = new ScheduledTask
            {
                NextRun = nextRun,
                Period = period,
                Callback = new TypedHandlerContainer<T>(callback),
                ErrorCallback = new DoubleTypedHandlerContainer<T, Exception>(onError)
            };
            Tasks.TryAdd(scheduledTask.TaskId, scheduledTask);
            
            scheduledTask.Timer = new Timer(timerTask =>
            {
                ScheduledTask taskState = (ScheduledTask)timerTask;

                try
                {
                    taskState.Callback?.Invoke(taskState);
                }
                catch (Exception ex)
                {
                    taskState.ErrorCallback?.Invoke(taskState, ex);
                }
                
                if (taskState.NextRun == DateTime.MinValue)
                {
                    RemoveScheduledTask(taskState);
                }
                else
                {
                    taskState.NextRun = taskState.NextRun.Add(taskState.Period);
                }
            }, scheduledTask, dueTime, period);

            return scheduledTask;
        }

        /// <summary>
        /// Cancels a scheduled task.
        /// </summary>
        /// <param name="taskId">The task identifier.</param>
        public static void Cancel(string taskId)
        {
            ScheduledTask taskState;
            if (Tasks.TryGetValue(taskId, out taskState))
            {
                RemoveScheduledTask(taskState);
            }
        }

        private static void RemoveScheduledTask(ScheduledTask taskState)
        {
            taskState.Timer.Dispose();
            Tasks.TryRemove(taskState.TaskId, out taskState);
        }

        private static TimeSpan GetTimeSpanFromRepeatPolicy(Repeat repeat)
        {
            switch (repeat)
            {
                case Repeat.EveryDay:
                    return new TimeSpan(1, 0, 0, 0);

                case Repeat.EveryHour:
                    return new TimeSpan(1, 0, 0);

                case Repeat.EveryMinute:
                    return new TimeSpan(0, 1, 0);

                case Repeat.EveryTenMinutes:
                    return new TimeSpan(0, 10, 0);
                
                case Repeat.EveryWeek:
                    return new TimeSpan(7, 0, 0, 0);
                
                case Repeat.NextMonth:
                    DateTime nextMonth = DateTime.Now.AddMonths(1);
                    return nextMonth - DateTime.Now;

                case Repeat.NextYear:
                    DateTime nextYear = DateTime.Now.AddYears(1);
                    return nextYear - DateTime.Now;

                default:
                    return TimeSpan.Zero;
            }
        }
    }
}

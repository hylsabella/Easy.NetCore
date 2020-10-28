using Quartz;
using System;
using System.Collections.Generic;

namespace Easy.Common.NetCore.Extentions
{
    public static class SchedulerExt
    {
        /// <summary>
        /// 添加定时任务
        /// </summary>
        /// <param name="intervalTs">间隔时间</param>
        /// <param name="startAt">第一次执行时间</param>
        /// <param name="repeatCount">重复次数，不填或者为0：不限次</param>
        public static void AddJobExt<T>(this IScheduler scheduler, TimeSpan intervalTs, DateTime? startAt = null, int? repeatCount = null, IDictionary<string, object> jobDataDic = null) where T : IJob
        {
            string jobName = typeof(T).Name;
            string groupName = jobName + "Group";
            string triggerName = jobName + "Trigger";

            JobBuilder jobBuilder = JobBuilder.Create<T>()
                                    .WithIdentity(jobName, groupName);

            if (jobDataDic != null)
            {
                JobDataMap jobDataMap = new JobDataMap(jobDataDic);

                jobBuilder.UsingJobData(jobDataMap);
            }

            IJobDetail job = jobBuilder.Build();

            AddJob(scheduler, intervalTs, startAt, repeatCount, groupName, triggerName, job);
        }

        /// <summary>
        /// 添加定时任务
        /// </summary>
        /// <param name="type">类型：必须继承至IJob</param>
        /// <param name="intervalTs">间隔时间</param>
        /// <param name="startAt">第一次执行时间</param>
        /// <param name="repeatCount">重复次数，不填或者为0：不限次</param>
        public static void AddJobExt(this IScheduler scheduler, Type type, TimeSpan intervalTs, DateTime? startAt = null, int? repeatCount = null, IDictionary<string, object> jobDataDic = null)
        {
            if (!typeof(IJob).IsAssignableFrom(type))
            {
                throw new ArgumentException("传入的类型必须是IJob的派生类！");
            }

            string jobName = type.Name;
            string groupName = jobName + "Group";
            string triggerName = jobName + "Trigger";

            JobBuilder jobBuilder = JobBuilder.Create(type)
                                    .WithIdentity(jobName, groupName);

            if (jobDataDic != null)
            {
                JobDataMap jobDataMap = new JobDataMap(jobDataDic);

                jobBuilder.UsingJobData(jobDataMap);
            }

            IJobDetail job = jobBuilder.Build();

            AddJob(scheduler, intervalTs, startAt, repeatCount, groupName, triggerName, job);
        }

        private static async void AddJob(IScheduler scheduler, TimeSpan intervalTs, DateTime? startAt, int? repeatCount, string groupName, string triggerName, IJobDetail job)
        {
            TriggerBuilder triggerBuilder = null;

            if (repeatCount.HasValue)
            {
                repeatCount -= 1;

                triggerBuilder = TriggerBuilder.Create()
                   .WithIdentity(triggerName, groupName)
                   .StartNow()
                   .WithSimpleSchedule(x => x
                       .WithInterval(intervalTs)
                       .WithRepeatCount(repeatCount.Value));
            }
            else
            {
                triggerBuilder = TriggerBuilder.Create()
                   .WithIdentity(triggerName, groupName)
                   .StartNow()
                   .WithSimpleSchedule(x => x
                       .WithInterval(intervalTs)
                       .RepeatForever());
            }

            if (!startAt.HasValue)
            {
                startAt = DateTime.Now + TimeSpan.FromSeconds(10);
            }

            triggerBuilder.StartAt(startAt.Value);

            ITrigger trigger = triggerBuilder.Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}

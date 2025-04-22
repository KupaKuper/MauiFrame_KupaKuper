using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiHmiFrame_KupaKuper.Help
{
    public static class TimerHelp
    {
        /// <summary>
        /// 延时对应时间后执行
        /// </summary>
        /// <param name="delayTime"> 延时的时间 </param>
        /// <param name="action"> 执行的方法 </param>
        public static async void DelayRun(TimeSpan delayTime, Action action)
        {
            await Task.Delay(delayTime);
            action();
        }
        /// <summary>
        /// 延时对应时间后执行
        /// </summary>
        /// <param name="delayTime"> 延时的毫秒数 </param>
        /// <param name="action"> 执行的方法 </param>
        public static async void DelayRun(int delayTime, Action action)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(delayTime));
            action();
        }
        /// <summary>
        /// 创建一个指定时间的周期执行任务
        /// </summary>
        /// <param name="durationTime"> 持续时间(ms) </param>
        /// <param name="cycleTime"> 执行周期(ms) </param>
        /// <param name="action"> 执行方法 </param>
        public static void CycleRun(TimeSpan durationTime, TimeSpan cycleTime, Action action)
        {
            CycleRun(durationTime, cycleTime, action, () => { });
        }
        /// <summary>
        /// 创建一个指定时间的周期执行任务
        /// </summary>
        /// <param name="durationTime"> 持续时间(ms) </param>
        /// <param name="cycleTime"> 执行周期(ms) </param>
        /// <param name="action"> 执行方法 </param>
        public static void CycleRun(int durationTime, int cycleTime, Action action)
        {
            CycleRun(durationTime, cycleTime, action, () => { });
        }
        /// <summary>
        /// 创建一个指定时间的周期执行任务
        /// </summary>
        /// <param name="durationTime"> 持续时间(ms) </param>
        /// <param name="cycleTime"> 执行周期(ms) </param>
        /// <param name="action"> 执行方法 </param>
        /// <param name="cycleEndAction"> 循环执行结束后执行的方法 </param>
        public static async void CycleRun(TimeSpan durationTime, TimeSpan cycleTime, Action action, Action cycleEndAction)
        {
            await Task.Run(() =>
            {
                TimeSpan time = new TimeSpan(0);
                while (time < durationTime)
                {
                    action();
                    Task.Delay(cycleTime);
                    time += cycleTime;
                }
            });
            cycleEndAction();
        }
        /// <summary>
        /// 创建一个指定时间的周期执行任务
        /// </summary>
        /// <param name="durationTime"> 持续时间(ms) </param>
        /// <param name="cycleTime"> 执行周期(ms) </param>
        /// <param name="action"> 执行方法 </param>
        /// <param name="cycleEndAction"> 循环执行结束后执行的方法 </param>
        public static async void CycleRun(int durationTime, int cycleTime, Action action, Action cycleEndAction)
        {
            await Task.Run(() =>
            {
                TimeSpan _durationTime = TimeSpan.FromMilliseconds(durationTime);
                TimeSpan _cycleTime = TimeSpan.FromMilliseconds(cycleTime);
                TimeSpan time = new TimeSpan(0);
                while (time < _durationTime)
                {
                    action();
                    Task.Delay(cycleTime);
                    time += _cycleTime;
                }
            });
            cycleEndAction();
        }
    }
}

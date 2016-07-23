
using System.Collections.Generic;
using System.Threading;
using System;
namespace snw.core
{
    public class scheduler : Dictionary<string, schedule>, IDisposable
    {
        public static scheduler Current;
        private int interval = 100;
        private Thread ch_Thread;
        private bool ThreadDie = false;

        public void Start(int checkinterval)
        {
            interval = checkinterval;
            ch_Thread = new Thread(new ThreadStart(CheckSchedules));
            ch_Thread.Start();
        }
        private void CheckSchedules()
        {
            try
            {
                while (!ThreadDie)
                {
                    foreach (KeyValuePair<string, schedule> s in this)
                    {
                        if (DateTime.Now > s.Value.CreationTime.AddMinutes(s.Value.ExpirationMinutes))
                        {
                            Remove(s.Key);
                        }
                    }
                    Thread.Sleep(interval * 60000);
                }
            }
            catch
            {
                return;
            } 
        }

        public void Dispose()
        {
            ThreadDie = true;
        }
    }

}
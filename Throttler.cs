using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Monoxide.Dishes
{
    public class Throttler
    {
        Timer timer;

        public bool Throttle(TimeSpan span)
        {
            System.Threading.Monitor.Enter(this);
            bool shouldExit = true;
            try
            {
                if (timer == null)
                {
                    timer = new Timer(span.TotalMilliseconds);
                    timer.AutoReset = false;
                    timer.Elapsed += (o, e) =>
                    {
                        timer.Stop();
                        timer.Close();
                        timer = null;
                    };
                    timer.Start();
                    System.Threading.Monitor.Exit(this);
                    shouldExit = false;
                    return true;
                }
            }
            finally
            {
                if (shouldExit)
                    System.Threading.Monitor.Exit(this);
            }
            return false;
        }
    }
}

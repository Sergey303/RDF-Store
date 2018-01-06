using System.Diagnostics;

namespace ConsoleSparqlCore
{
    public static class StopwatchExtension
    {
        public static double GetTimeWthLast2Digits(this Stopwatch timer)
        {
            return timer.ElapsedMilliseconds > 10
                ? timer.ElapsedMilliseconds
                : ((double) ((int) (timer.ElapsedTicks / 100))) / 100;
        }
    }

}

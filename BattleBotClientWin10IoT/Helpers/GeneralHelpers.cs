using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleBotClientWin10IoT.Helpers
{
    public static class GeneralHelpers
    {
        public static int MapIntToValue(int value, int inMin, int inMax, int outMin, int outMax)
        {
            int returnVal = (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
            if (returnVal > outMax) return outMax;
            if (returnVal < outMin) return outMin;
            return returnVal;
        }

        public static bool Between(int value, int min, int max)
        {
            return (value < max && value > min);
        }
        public static bool IsBetween<T>(this T item, T start, T end)
        {
            return Comparer<T>.Default.Compare(item, start) >= 0
                && Comparer<T>.Default.Compare(item, end) <= 0;
        }
    }
}

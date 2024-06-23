using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirginiaTech.Fuse.Record
{
    /// <summary>
    /// Can be used to count up to or down to some number.
    /// </summary>
    class Counter
    {
        private int currentCount;
        private int maxCount;

        /// <summary>
        /// Create a new counter object with the following max count.
        /// </summary>
        /// <param name="max">The maximum times this Counter can count.</param>
        public Counter(int max)
        {
            maxCount = max;
        }

        /// <summary>
        /// Increment the counter.
        /// </summary>
        public void Count()
        {
            if (currentCount < maxCount)
            {
                currentCount++;
            }
        }

        /// <summary>
        /// Reset the counter back to zero.
        /// </summary>
        public void Reset()
        {
            currentCount = 0;
        }

        /// <summary>
        /// Return true if the counter is at its maximum count.
        /// </summary>
        /// <returns></returns>
        public bool Complete()
        {
            return currentCount == maxCount;
        }

        /// <summary>
        /// The amount of increments left to reach the maximum count.
        /// </summary>
        public int CountLeft { get { return maxCount - currentCount; } }

        /// <summary>
        /// The amount of increments.
        /// </summary>
        public int CountAmount { get { return currentCount; } }
    }
}

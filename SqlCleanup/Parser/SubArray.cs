using System;
using System.Diagnostics;
using System.Linq;

namespace SqlCleanup.Parser
{
    [DebuggerDisplay("[{offset}-{offset+count}:{count} {ToString()}")]
    public struct SubArray<T>
    {
        private T[] array;
        private int offset;
        private int count;

        public T[] Array { get { return array; } }
        public int Offset { get { return offset; } }
        public int Count { get { return count; } }

        /// <summary>
        /// Initializes a new instance of the SubArray<T> structure that delimits
        /// all the elements in the specified array.
        /// </summary>
        /// <param name="array">The array to wrap.</param>
        public SubArray(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            this.array = array;
            this.offset = 0;
            this.count = array.Length;
        }

        /// <summary>
        /// Initializes a new instance of the System.ArraySegment<T> structure that delimits
        /// the specified range of the elements in the specified array.
        /// </summary>
        /// <param name="array">The array containing the range of elements to delimit.</param>
        /// <param name="offset">The zero-based index of the first element in the range.</param>
        /// <param name="count">The number of elements in the range.</param>
        public SubArray(T[] array, int offset, int count)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            this.array = array;
            this.offset = offset;
            this.count = count;
        }

        /// <summary>
        /// Returns a copy of only the specified part of the array.
        /// </summary>
        /// <returns>A copy of the specified part of the array.</returns>
        public T[] Copy()
        {
            var subArray = new T[count];
            Buffer.BlockCopy(array, offset * sizeof(char), subArray, 0, count * sizeof(char));
            return subArray;
        }

        public override string ToString()
        {
            return string.Join("", (Copy().Select(x => x.ToString())));
        }
    }
}

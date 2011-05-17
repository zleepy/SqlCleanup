using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlCleanup.SqlParser
{
    struct State
    {
        //public readonly bool IsMatch;
        public readonly bool HasError;
        public readonly int Start;
        public readonly int Count;
        public readonly int End;

        public State(int start = 0, int count = 0, bool hasError = false)
        {
            //IsMatch = isMatch;
            Start = start;
            Count = count;
            End = start + count;
            HasError = hasError;
        }
    }
}

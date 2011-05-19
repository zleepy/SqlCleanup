
namespace SqlCleanup.SqlParser
{
    struct State
    {
        //public readonly bool IsMatch;
        public readonly bool HasError;
        public readonly int Start;
        public readonly int Count;
        public readonly int End;
        public readonly TokenType Type;

        public State(TokenType tokenType, int start = 0, int count = 0, bool hasError = false)
        {
            //IsMatch = isMatch;
            Type = tokenType;
            Start = start;
            Count = count;
            End = start + count;
            HasError = hasError;
        }
    }
}

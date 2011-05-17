using System;

namespace SqlCleanup.SqlParser
{
    class MatchingStartEndRule : Rule
    {
        public string Start { get; private set; }
        public string End { get; private set; }
        public char? EscapeCharacter { get; private set; }

        public MatchingStartEndRule(string start, string end, TokenType tokenType)
            : base(tokenType)
        {
            if (string.IsNullOrWhiteSpace(start))
                throw new ArgumentNullException("start");
            if (string.IsNullOrWhiteSpace(end))
                throw new ArgumentNullException("end");

            Start = start;
            End = end;
            EscapeCharacter = null;
        }

        public MatchingStartEndRule(string start, string end, char escapeCharacter, TokenType tokenType)
            : base(tokenType)
        {
            if (string.IsNullOrWhiteSpace(start))
                throw new ArgumentNullException("start");
            if (string.IsNullOrWhiteSpace(end))
                throw new ArgumentNullException("end");

            Start = start;
            End = end;
            EscapeCharacter = escapeCharacter;
        }

        public override bool Match(Lexer lexer, out State result)
        {
            int startPos = lexer.Pos;

            if (IsMatchingString(lexer, Start))
            {
                if (EscapeCharacter == null)
                {
                    do
                    {
                        if (IsMatchingString(lexer, End))
                        {
                            result = new State(startPos, lexer.Pos - startPos);
                            return true;
                        }
                    } while (lexer.Next());

                    // Det här är specialfall som uppstår om man inte har något sluttecken och vi kommer till slutet på texten.
                    // I teorin är det inte en träff, men jag vill ändå räkna det som en träff fast som inte är komplett.
                    result = new State(startPos, lexer.Pos - startPos, true);
                    return true;
                }
                else
                {
                    do
                    {
                        if (lexer.Current == EscapeCharacter)
                        {
                            // Är det nuvarande tecknet samma som escape så hoppa över det och nästa.
                            lexer.Next();
                        }
                        else if (IsMatchingString(lexer, End))
                        {
                            result = new State(startPos, lexer.Pos - startPos);
                            return true;
                        }
                    } while (lexer.Next());

                    // Det här är specialfall som uppstår om man inte har något sluttecken och vi kommer till slutet på texten.
                    // I teorin är det inte en träff, men jag vill ändå räkna det som en träff fast som inte är komplett.
                    result = new State(startPos, lexer.Pos - startPos, true);
                    return true;
                }
            }

            result = new State();
            return false;
        }
    }
}

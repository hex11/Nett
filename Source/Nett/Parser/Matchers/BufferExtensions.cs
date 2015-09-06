﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett.Parser.Matchers
{
    internal static class BufferExtensions
    {
        private static readonly char[] WhitspaceCharSet =
        {
            '\u0009', '\u000A', '\u000B', '\u000D', '\u0020', '\u0085', '\u00A0',
            '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006',
            '\u2007', '\u2008', '\u2009', '\u200A', '\u2028', '\u2029', '\u202F', '\u205F',
            '\u3000',
        };

        public static bool PeekInRange(this LookaheadBuffer<char> buffer, char min, char max)
        {
            char pv = buffer.Peek();
            return pv >= min && pv <= max;
        }

        public static bool PeekIsWhitespace(this LookaheadBuffer<char> buffer)
        {
            if (buffer.End)
            {
                return false;
            }

            char pv = buffer.Peek();
            return WhitspaceCharSet.Contains(pv);
        }

        public static bool TokenDone(this LookaheadBuffer<char> buffer)
        {
            return buffer.End || buffer.PeekIsWhitespace();
        }

        public static bool LaSequenceIs(this LookaheadBuffer<char> buffer, string seq)
        {
            for (int i = 0; i < seq.Length; i++)
            {
                if (buffer.La(i) != seq[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static IList<char> Consume(this LookaheadBuffer<char> buffer, int count)
        {
            List<char> c = new List<char>();
            for (int i = 0; i < count; i++)
            {
                c.Add(buffer.Consume());
            }

            return c;
        }

        public static bool PeekIsDigit(this LookaheadBuffer<char> buffer)
        {
            return buffer.PeekInRange('0', '9');
        }

        public static string ConsumeTillWhitespaceOrEnd(this LookaheadBuffer<char> buffer)
        {
            StringBuilder sb = new StringBuilder();

            while (!buffer.End && !buffer.PeekIsWhitespace())
            {
                sb.Append(buffer.Consume());
            }

            return sb.ToString();
        }
    }
}

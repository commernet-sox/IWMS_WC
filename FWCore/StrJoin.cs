using System;
using System.Text;

namespace FWCore
{
    /// <summary>
    /// 封装StringBuilder对象，实现操作符重载(+=)
    /// </summary>
    public class StrJoin
    {
        private readonly StringBuilder _sb;

        public StrJoin()
        {
            _sb = new StringBuilder(0x40);
        }

        public StrJoin(int cap)
        {
            _sb = new StringBuilder(cap);
        }

        public int Lengh
        {
            get
            {
                return _sb.Length;
            }
        }

        public StrJoin Append(bool v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(byte v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(char v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(decimal v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(double v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(short v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(int v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(long v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(object v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(sbyte v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(float v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(char[] v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(string v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(ushort v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(uint v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(ulong v)
        {
            _sb.Append(v);
            return this;
        }

        public StrJoin Append(char v, int rc)
        {
            _sb.Append(v, rc);
            return this;
        }

        public StrJoin AppendFormat(string format, params object[] args)
        {
            _sb.AppendFormat(format, args);
            return this;
        }

        public static StrJoin operator +(StrJoin c1, StrJoin c2)
        {
            return c1.Append(c2.ToString());
        }

        public static StrJoin operator +(StrJoin c1, bool c2)
        {
            return c1.Append(c2);
        }

        public static StrJoin operator +(StrJoin c1, byte c2)
        {
            return c1.Append(c2);
        }

        public static StrJoin operator +(StrJoin c1, char c2)
        {
            return c1.Append(c2);
        }

        public static StrJoin operator +(StrJoin c1, decimal c2)
        {
            return c1.Append(c2);
        }

        public static StrJoin operator +(StrJoin c1, double c2)
        {
            return c1.Append(c2);
        }

        public static StrJoin operator +(StrJoin c1, short c2)
        {
            return c1.Append(c2);
        }

        public static StrJoin operator +(StrJoin c1, int c2)
        {
            return c1.Append(c2);
        }

        public static StrJoin operator +(StrJoin c1, sbyte c2)
        {
            return c1.Append(c2);
        }

        public static StrJoin operator +(StrJoin c1, float c2)
        {
            return c1.Append(c2);
        }

        public static StrJoin operator +(StrJoin c1, char[] c2)
        {
            return c1.Append(c2);
        }

        public static StrJoin operator +(StrJoin c1, long c2)
        {
            return c1.Append(c2);
        }

        public static StrJoin operator +(StrJoin c1, string c2)
        {
            return c1.Append(c2);
        }

        public static StrJoin operator +(StrJoin c1, ushort c2)
        {
            return c1.Append(c2);
        }

        public static StrJoin operator +(StrJoin c1, uint c2)
        {
            return c1.Append(c2);
        }

        public static StrJoin operator +(StrJoin c1, ulong c2)
        {
            return c1.Append(c2);
        }

        public static implicit operator string(StrJoin v)
        {
            return v.ToString();
        }

        public static implicit operator StrJoin(string v)
        {
            var join = new StrJoin();
            return join.Append(v);
        }

        public void Remove(int startIndex, int length)
        {
            _sb.Remove(startIndex, length);
        }

        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}

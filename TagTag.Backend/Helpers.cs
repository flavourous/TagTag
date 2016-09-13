using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagTag.Backend
{
    public static class Helpers
    {
        public static T PopNull<T>(this Stack<T> st) where T : class
        {
            if (st.Count == 0) return null;
            return st.Pop();
        }
        public static T PeekNull<T>(this Stack<T> st) where T : class
        {
            if (st.Count == 0) return null;
            return st.Peek();
        }
    }
}

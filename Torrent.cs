/*
TorrentCleanup
Copyright (C) 2016 Michael Drüing

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace TorrentCleanup
{
    public class TException : Exception
    {
        public TException(string what) : base(what) { }
    }

    /// <summary>
    /// The base class for all BitTorrent objects
    /// </summary>
    public abstract class TObject
    {
        /// <summary>
        /// Parse an object from a given stream. Returns the number of bytes consumed from the stream
        /// </summary>
        /// <param name="sr">The streamreader to parse</param>
        public abstract void ParseFromStream(System.IO.TextReader sr);

        /// <summary>
        /// Returns a "pretty-printed" representation of the object, indented by a few spaces
        /// </summary>
        /// <param name="indent">The number of spaces to insert in front of each line</param>
        /// <returns>the pretty-printed string</returns>
        public abstract string PrettyPrint(int indent);

        public static TObject ParseObject(System.IO.TextReader sr)
        {
            int ch = sr.Peek();
            TObject result;

            if (Char.IsDigit((char)ch))
            {
                result = new TString();
                result.ParseFromStream(sr);
                return result;
            }

            switch (ch)
            {
                case 'i':
                    result = new TInteger();
                    result.ParseFromStream(sr);
                    return result;
                case 'l':
                    result = new TList();
                    result.ParseFromStream(sr);
                    return result;
                case 'd':
                    result = new TDictionary();
                    result.ParseFromStream(sr);
                    return result;
                default:
                    throw new TException("invalid datatype");
            }
        }

        public static implicit operator TObject(string s)
        {
            return new TString(s);
        }

        public static implicit operator TObject(long i)
        {
            return new TInteger(i);
        }

        public static bool operator ==(TObject a, object b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TObject a, object b)
        {
            return !a.Equals(b);
        }

        public abstract override bool Equals(object o);
        public abstract override int GetHashCode();
    }

    /// <summary>
    /// A <c>TString</c> encodes a BitTorrent style string
    /// </summary>
    public class TString : TObject, IComparable, IEquatable<object>
    {
        protected string m_stringValue = null;

        public string Value
        {
            get
            {
                return m_stringValue;
            }
            set
            {
                m_stringValue = value;
            }
        }

        public TString(string s)
        {
            m_stringValue = s;
        }

        public TString()
        {
            m_stringValue = "";
        }

        public override void ParseFromStream(System.IO.TextReader sr)
        {
            string len = "";
            int length;
            int ch;

            while (true)
            {
                ch = sr.Read();
                if (ch == -1)
                    throw new TException("unexpected end of stream");

                if (!Char.IsDigit((char)ch))
                    break;

                len += (char)ch;
            }
            if (ch != ':')
                throw new TException("Expected ':' after string length");
            
            length = Convert.ToInt32(len);
            byte[] buf = new byte[length];
            for (int i = 0; i < length; i++)
            {
                ch = sr.Read();
                if (ch == -1)
                    throw new TException("unexpected end of stream");
                buf[i] = (byte)(ch & 0xff);
            }
            m_stringValue = Encoding.UTF8.GetString(buf);
        }

        public override string ToString()
        {
            return String.Format("{0}:{1}", m_stringValue.Length, m_stringValue);
        }

        public override string PrettyPrint(int indent)
        {
            return String.Format("{0}'{1}'{2}", new string(' ', indent), m_stringValue, Environment.NewLine);
        }

        public int CompareTo(object obj)
        {
            if (obj is TString)
                return m_stringValue.CompareTo((obj as TString).m_stringValue);

            return -1;
        }

        public override bool Equals(object other)
        {
            if (other is TString)
                return (other as TString).m_stringValue.Equals(m_stringValue);
            if (other is string)
                return m_stringValue == (string)other;

            return false;
        }

        public override int GetHashCode()
        {
            return m_stringValue.GetHashCode();
        }
    }

    public class TInteger : TObject, IComparable, IEquatable<object>
    {
        private long m_intValue;

        public long  Value
        {
            get
            {
                return m_intValue;
            }
            set
            {
                m_intValue = value;
            }
        }

        public TInteger(long i)
        {
            m_intValue = i;
        }

        public TInteger()
        {
            m_intValue = 0;
        }

        public override void ParseFromStream(System.IO.TextReader sr)
        {
            StringBuilder sb = new StringBuilder();
            int ch;

            if (sr.Read() != 'i')
                throw new TException("'i' expected");

            while (true) 
            {
                ch = sr.Read();
                if (ch == -1)
                    throw new TException("unexpected end of stream");

                if (ch == 'e')
                    break;

                if (Char.IsDigit((char)ch))
                    sb.Append((char)ch);
                else
                    throw new TException("invalid integer digit");
            }

            m_intValue = Convert.ToInt64(sb.ToString());
        }

        public override string ToString()
        {
            return String.Format("i{0}e", m_intValue);
        }

        public override string PrettyPrint(int indent)
        {
            return String.Format("{0}{1}{2}", new string(' ', indent), m_intValue, Environment.NewLine);
        }

        public int CompareTo(object obj)
        {
            if (obj is TInteger)
                return m_intValue.CompareTo((obj as TInteger).m_intValue);
            try
            {
                long val = Convert.ToInt64(obj);
                return m_intValue.CompareTo(val);
            }
            catch
            {
                return -1;
            }
        }

        public override bool Equals(object other)
        {
            if (other is TInteger)
                return (other as TInteger).m_intValue == m_intValue;
            try
            {
                long val = Convert.ToInt64(other);
                return m_intValue == val;
            }
            catch
            {
                // not convertible to INT
                return false;
            }
        }

        public override int GetHashCode()
        {
            return m_intValue.GetHashCode();
        }
    }

    public class TList : TObject, IList<TObject>, IEquatable<object>
    {
        protected List<TObject> m_theList = new List<TObject>();

        public IEnumerator<TObject> GetEnumerator()
        {
            return m_theList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_theList.GetEnumerator();
        }

        public override void ParseFromStream(System.IO.TextReader sr)
        {
            if (sr.Read() != 'l')
                throw new TException("'l' expected");

            m_theList = new List<TObject>();
            while (sr.Peek() != 'e')
            {
                m_theList.Add(TObject.ParseObject(sr));
            }
            sr.Read(); // skip the final 'e'
        }

        public override string PrettyPrint(int indent)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("{0}({1}", new string(' ', indent), Environment.NewLine));
            foreach (TObject o in m_theList)
            {
                sb.Append(o.PrettyPrint(indent + 2));
            }
            sb.Append(String.Format("{0}){1}", new string(' ', indent), Environment.NewLine));

            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('l');
            foreach (TObject o in m_theList)
                sb.Append(o.ToString());
            sb.Append('e');

            return sb.ToString();
        }

        public override int GetHashCode()
        {
            int hash = 0;

            foreach (TObject o in m_theList)
            {
                hash <<= 4;
                hash ^= o.GetHashCode();
            }

            return hash;
        }

        public int IndexOf(TObject item)
        {
            return m_theList.IndexOf(item);
        }

        public void Insert(int index, TObject item)
        {
            m_theList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            m_theList.RemoveAt(index);
        }

        public TObject this[int index]
        {
            get
            {
                return m_theList[index];
            }
            set
            {
                m_theList[index] = value;
            }
        }

        public void Add(TObject item)
        {
            m_theList.Add(item);
        }

        public void Clear()
        {
            m_theList.Clear();
        }

        public bool Contains(TObject item)
        {
            return m_theList.Contains(item);
        }

        public void CopyTo(TObject[] array, int arrayIndex)
        {
            m_theList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return m_theList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(TObject item)
        {
            return m_theList.Remove(item);
        }

        public override bool Equals(object other)
        {
            if (other is TList)
            {
                TList o = other as TList;

                if (o.Count != m_theList.Count)
                    return false;

                IEnumerator<TObject> it1 = o.m_theList.GetEnumerator();
                IEnumerator<TObject> it2 = m_theList.GetEnumerator();
                for (int i = 0; i < m_theList.Count; i++)
                {
                    it1.MoveNext();
                    it2.MoveNext();

                    if (!it1.Current.Equals(it2.Current))
                        return false;
                }

                return true;
            }

            return false;
        }
    }

    public class TDictionary : TObject, IDictionary<TObject, TObject>, IEquatable<object>
    {
        private Dictionary<TObject, TObject> m_theDictionary = new Dictionary<TObject, TObject>();

        public override void ParseFromStream(System.IO.TextReader sr)
        {
            if (sr.Read() != 'd')
                throw new TException("'d' expected");

            while (sr.Peek() != 'e')
            {
                TObject key = TObject.ParseObject(sr);
                TObject val = TObject.ParseObject(sr);
                m_theDictionary.Add(key, val);
            }

            sr.Read(); // skip the final 'e'
        }

        public override string PrettyPrint(int indent)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(String.Format("{0}[{1}", new string(' ', indent), Environment.NewLine));
            foreach (TObject k in m_theDictionary.Keys)
            {
                sb.Append(k.PrettyPrint(indent + 2));
                sb.Append(m_theDictionary[k].PrettyPrint(indent + 2));
                sb.Append(String.Format("{0}----{1}", new string(' ', indent + 2), Environment.NewLine));
            }
            sb.Append(String.Format("{0}]{1}", new string(' ', indent), Environment.NewLine));
            return sb.ToString();
        }

        public void Add(TObject key, TObject value)
        {
            m_theDictionary.Add(key, value);
        }

        public bool ContainsKey(TObject key)
        {
            return m_theDictionary.ContainsKey(key);
        }

        public ICollection<TObject> Keys
        {
            get { return m_theDictionary.Keys; }
        }

        public bool Remove(TObject key)
        {
            return m_theDictionary.Remove(key);
        }

        public bool TryGetValue(TObject key, out TObject value)
        {
            return m_theDictionary.TryGetValue(key, out value);
        }

        public ICollection<TObject> Values
        {
            get { return m_theDictionary.Values; }
        }

        public TObject this[TObject key]
        {
            get
            {
                return m_theDictionary[key];
            }
            set
            {
                m_theDictionary[key] = value;
            }
        }

        public void Add(KeyValuePair<TObject, TObject> item)
        {
            m_theDictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            m_theDictionary.Clear();
        }

        public bool Contains(KeyValuePair<TObject, TObject> item)
        {
            return ((IDictionary<TObject, TObject>)m_theDictionary).Contains(item);
        }

        public int Count
        {
            get { return m_theDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TObject, TObject> item)
        {
            return m_theDictionary.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TObject, TObject>> GetEnumerator()
        {
            return m_theDictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_theDictionary.GetEnumerator();
        }

        public void CopyTo(KeyValuePair<TObject, TObject>[] array, int arrayIndex)
        {
            foreach (var x in m_theDictionary)
            {
                array[arrayIndex++] = x;
            }
        }

        public override int GetHashCode()
        {
            int hash = 0;

            foreach (TObject k in m_theDictionary.Keys)
            {
                hash <<= 4;
                hash ^= k.GetHashCode();
                hash <<= 4;
                hash ^= m_theDictionary[k].GetHashCode();
            }

            return hash;
        }

        // deep equality
        public override bool Equals(object other)
        {
            if (other is TDictionary)
            {
                TDictionary o = other as TDictionary;

                if (o.m_theDictionary.Count != m_theDictionary.Count)
                    return false;

                foreach (TObject k in m_theDictionary.Keys)
                {
                    // compare keys
                    if (!o.ContainsKey(k))
                        return false;

                    // compare values
                    if (!m_theDictionary[k].Equals(o.m_theDictionary[k]))
                        return false;
                }

                return true;
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('d');
            foreach (TObject k in m_theDictionary.Keys)
            {
                sb.Append(k.ToString());
                sb.Append(m_theDictionary[k].ToString());
            }
            sb.Append('e');

            return sb.ToString();
        }
    }
}

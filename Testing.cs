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

namespace TorrentCleanup
{
    public class Testing
    {
        private static void PASS(bool test, string message)
        {
            if (!test)
                Console.WriteLine("Unexpected FAIL: {0}", message);
        }

        private static void FAIL(bool test, string message)
        {
            if (test)
                Console.WriteLine("Unexpected PASS: {0}", message);
        }

        public static void DoTests()
        {
            DoIntegerTests();
            DoStringTests();
            DoCrossTests();
            DoListTests();
            DoDictTests();
        }

        public static void DoIntegerTests()
        {
            TInteger i1 = new TInteger(23);
            TInteger i2 = new TInteger(42);
            TInteger i3 = new TInteger(23);


            FAIL(i1 == i2, "Int == 1");
            PASS(i1 == i3, "Int == 2");
            PASS(i1 != i2, "Int == 3");
            FAIL(i1 != i3, "Int == 4");

            FAIL(i1.Equals(i2), "Int Eq 1");
            PASS(i1.Equals(i3), "Int Eq 2");

            PASS(i1.CompareTo(i2) != 0, "Int Ct 1");
            PASS(i1.CompareTo(i1) == 0, "Int Ct 2");
            PASS(i1.CompareTo(i2) < 0, "Int Ct 3");
            PASS(i2.CompareTo(i1) > 0, "Int Ct 4");

            FAIL(i1.CompareTo(null) == 0, "Int null 1");
            FAIL(i1.Equals(null), "Int null 2");

            PASS(i1.Equals(23), "Int conv 1");
            FAIL(i1.Equals(42), "Int conv 2");
            PASS(i1.CompareTo(23) == 0, "Int conv 3");
            PASS(i1 == 23, "Int conv 4");
            FAIL(i2 == 23, "Int conv 5");
        }

        public static void DoStringTests()
        {
            TString s1 = new TString("foo");
            TString s2 = new TString("bar");
            TString s3 = new TString("foo");

            FAIL(s1 == s2, "Str == 1");
            PASS(s1 == s3, "Str == 2");

            PASS(s1 != s2, "Str != 1");
            FAIL(s1 != s3, "Str != 2");

            PASS(s1.Equals(s1), "Str Eq 1");
            FAIL(s1.Equals(s2), "Str Eq 2");
            PASS(s1.Equals(s3), "Str Eq 3");

            PASS(s1.CompareTo(s1) == 0, "Str Ct 1");
            PASS(s1.CompareTo(s2) > 0, "Str Ct 2");
            PASS(s1.CompareTo(s3) == 0, "Str Ct 3");

            FAIL(s1.CompareTo(null) == 0, "Str null 1");
            FAIL(s1.Equals(null), "Str null 2");

            PASS(s1 == "foo", "Str conv 1");
            PASS(s1.Equals("foo"), "Str conv 2");
            FAIL(s1 == "bar", "Str conv 3");
            FAIL(s1.Equals("bar"), "Str conv 4");
        }

        public static void DoCrossTests()
        {
            TString s = new TString("foo");
            TInteger i = new TInteger(23);

            FAIL(s.Equals(i), "X Eq 1");
            FAIL(i.Equals(s), "X Eq 2");
        }

        public static void DoListTests()
        {
            TList l1 = new TList();
            TList l2 = new TList();
            TList l3 = new TList();
            TList l4 = new TList();

            l1.Add(new TString("foo"));
            l1.Add(new TString("bar"));

            l2.Add(new TInteger(23));
            l2.Add(new TInteger(42));

            l3.Add(new TString("foo"));
            l3.Add(new TString("bar"));

            l4.Add(new TString("bar"));
            l4.Add(new TString("foo"));

            FAIL(l1 == l2, "Lst == 1");
            PASS(l1 == l3, "Lst == 2");

            PASS(l1 != l2, "Lst != 1");
            FAIL(l1 != l3, "Lst != 2");

            PASS(l1.Equals(l1), "Lst Eq 1");
            FAIL(l1.Equals(l2), "Lst Eq 2");
            PASS(l1.Equals(l3), "Lst Eq 3");

            PASS(l1.Contains(new TString("foo")), "Lst cont 1");
            FAIL(l1.Contains(new TInteger(23)), "Lst cont 2");

            FAIL(l1.Equals(null), "Lst null 1");
            FAIL(l1.Contains(null), "Lst null 2");

            PASS(l1.GetHashCode() == l3.GetHashCode(), "Lst hash 1");
            FAIL(l1.GetHashCode() == l4.GetHashCode(), "Lst hash 2");
        }

        public static void DoDictTests()
        {
            TDictionary d1 = new TDictionary();
            TDictionary d2 = new TDictionary();
            TDictionary d3 = new TDictionary();
            TDictionary d4 = new TDictionary();
            TString k1 = new TString("foo");
            TString v1 = new TString("bar");

            // populate dictionaries
            d1[k1] = v1;
            d2[k1] = v1;
            d3[k1] = new TString("bar");
            d4[new TString("foo")] = new TString("bar");

            // now all dictionaries should contain only 1 k/v pair
            PASS(d1 == d2, "Dict == 1");
            PASS(d1 == d3, "Dict == 1");
            PASS(d1 == d4, "Dict == 1");

            FAIL(d1 != d2, "Dict != 1");
            FAIL(d1 != d3, "Dict != 1");
            FAIL(d1 != d4, "Dict != 1");

            PASS(d1.Equals(d2), "Dict Eq 1");
            PASS(d1.Equals(d3), "Dict Eq 2");
            PASS(d1.Equals(d4), "Dict Eq 3");

            PASS(d1.ContainsKey(new TString("foo")), "Dict cont 1");
            PASS(d1.Contains(new KeyValuePair<TObject,TObject>(new TString("foo"), new TString("bar"))), "Dict cont 2");

            PASS(d1.ContainsKey("foo"), "Dict conv 1");
            PASS(d1["foo"] == "bar", "Dict conv 2");
        }
    }
}

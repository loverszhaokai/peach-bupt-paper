// Add by zhaokai
// 
// Sotre The mutators and mutations of current iteration
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.Core
{
    public static class CurrentMutators
    {
        static List<Tuple<string, uint>> mutations = new List<Tuple<string, uint>>();

        public static void Clear()
        {
            mutations.Clear();
        }

        public static void Add(string mutatorName, uint position)
        {
            mutations.Add(new Tuple<string, uint>(mutatorName, position));
        }

        public static void AddToDB()
        {
            foreach (Tuple<string, uint> it in mutations)
            {
                Console.WriteLine("mutatorName=" + it.Item1);
                Console.WriteLine("position=" + it.Item2);

                MySQLHelper.MySQLHelper.Add(it.Item1, it.Item2, 1);
            }
        }

        public static void Connect()
        {
            MySQLHelper.MySQLHelper.Connect();
        }

        public static void DisConnect()
        {
            MySQLHelper.MySQLHelper.DisConnect();
        }
    }
}

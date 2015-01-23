
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;

namespace Peach.Core
{
	/// <summary>
	/// Base class for Mutators.
	/// </summary>
	public abstract class Mutator : IWeighted
	{
		/// <summary>
		/// Instance of current mutation strategy
		/// </summary>
		public MutationStrategy context = null;

		/// <summary>
		/// Weight this mutator will get chosen in random mutation mode.
		/// </summary>
		public int weight = 1;

		/// <summary>
		/// Name of this mutator
		/// </summary>
		public string name = "Unknown";

		public Mutator()
		{
		}

		public Mutator(DataElement obj)
		{
		}

		public Mutator(State obj)
		{
		}

		/// <summary>
		/// Check to see if DataElement is supported by this 
		/// mutator.
		/// </summary>
		/// <param name="obj">DataElement to check</param>
		/// <returns>True if object is supported, else False</returns>
		public static bool supportedDataElement(DataElement obj)
		{
			return false;
		}

		/// <summary>
		/// Check to see if State is supported by this 
		/// mutator.
		/// </summary>
		/// <param name="obj">State to check</param>
		/// <returns>True if object is supported, else False</returns>
		public static bool supportedState(State obj)
		{
			return false;
		}

		/// <summary>
		/// Returns the total number of mutations this
		/// mutator is able to perform.
		/// </summary>
		/// <returns>Returns number of mutations mutater can generate.</returns>
		public abstract int count
		{
			get;
		}

		public abstract uint mutation
		{
			get;
			set;
		}

		/// <summary>
		/// Perform a sequential mutation.
		/// </summary>
		/// <param name="obj"></param>
		public abstract void sequentialMutation(DataElement obj);

		/// <summary>
		/// Perform a random mutation.
		/// </summary>
		/// <param name="obj"></param>
		public abstract void randomMutation(DataElement obj);

		/// <summary>
		/// Allow changing which state we change to.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public virtual State changeState(State obj)
		{
			throw new NotImplementedException();
		}

		#region IWeighted Members

		public int SelectionWeight
		{
			get { return weight; }
		}

		#endregion


        // 
        // Pos  Times
        // 0    0
        // 1    3
        // 2    5
        // 3    1
        //
        protected uint[][] allPosTimes;

        // 
        // Pos  Times
        // 0    0
        // 1    3
        // 2    5
        // 3    1
        //
        void InitAllPosTimes()
        {
            for (uint i = 0; i < allPosTimes.Length; i++)
            {
                allPosTimes[i] = new uint[2];
                allPosTimes[i][0] = i;
                allPosTimes[i][1] = 0;
            }
        }


        // 
        // Pos  Times
        // 0    0
        // 1    3
        // 2    5
        // 3    1
        // 
        // Change to
        //
        // Pos  Times
        // 2    5
        // 1    3
        // 3    1
        // 0    0
        // 
        void SortAllPosTimes()
        {
            Sort<uint>(allPosTimes, 1);
        }

        static void Sort<T>(T[][] data, int col)
        {
            Comparer<T> comparer = Comparer<T>.Default;
            System.Array.Sort<T[]>(data, (x, y) => comparer.Compare(y[col], x[col]));
        }

        void SortValuesBenefit<T>(string mutatorName, T[] values, T[] values_benefit, uint top)
        {
            allPosTimes = new uint[values.Length][];
            // Init allPosTimes with times = 0
            InitAllPosTimes();

            // Get postions times 
            List<Tuple<uint, uint>> posTimes = new List<Tuple<uint, uint>>();
            MySQLHelper.MySQLHelper.GetMutationTimes(mutatorName, ref posTimes);

            // Update allPosTimes's times
            foreach (Tuple<uint, uint> it in posTimes)
            {
                allPosTimes[it.Item1][1] = it.Item2;
            }

            // Sort allPosTimes
            SortAllPosTimes();
        }

        protected void InitValuesBenefit<T>(string mutatorName, T[] values, ref T[] values_benefit)
        {
            if (Sample.IsUseSample)
            {
                // Change values_benefit
                uint size = Convert.ToUInt32(values.Length * Sample.VRC);
                values_benefit = new T[size];

                // Sort values_benefit
                SortValuesBenefit(mutatorName, values, values_benefit, size);

                // Random the order of array from 1 + top to end
                uint top = Convert.ToUInt32(size * Sample.VTOP);
                RandomArray<uint>(allPosTimes, (int)top, (int)size);

                // Set the top values
                for (uint i = 0; i < size; ++i)
                    values_benefit[i] = values[allPosTimes[i][0]];
            }
            else
            {
                values_benefit = new T[values.Length];

                // Copy each values to values_benefit
                for (int i = 0; i < values.Length; ++i)
                    values_benefit[i] = values[i];
            }
        }


        protected static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        protected static void Swap<T>(ref T[] lhs, ref T[] rhs)
        {
            T[] temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        protected static void RandomArray<T>(T[] array, int start, int end)
        {
            for (int i = start; 1 + i < end; ++i)
            {
                System.Random rand = new System.Random();
                int randPos = rand.Next(1 + i, end);
                Swap<T>(ref array[i], ref array[randPos]);
            }
        }

        protected static void RandomArray<T>(T[][] array, int start, int end)
        {
            for (int i = start; 1 + i < end; ++i)
            {
                System.Random rand = new System.Random();
                int randPos = rand.Next(1 + i, end);
                Swap<T>(ref array[i], ref array[randPos]);
            }
        }

 
        // Two dimension

        void SortValuesBenefit<T>(string mutatorName, T[][] values, T[][] values_benefit, uint top)
        {
            allPosTimes = new uint[values.Length][];
            // Init allPosTimes with times = 0
            InitAllPosTimes();

            // Get postions times 
            List<Tuple<uint, uint>> posTimes = new List<Tuple<uint, uint>>();
            MySQLHelper.MySQLHelper.GetMutationTimes(mutatorName, ref posTimes);

            // Update allPosTimes's times
            foreach (Tuple<uint, uint> it in posTimes)
            {
                allPosTimes[it.Item1][1] = it.Item2;
            }

            // Sort allPosTimes
            SortAllPosTimes();
        }


        protected void InitValuesBenefit<T>(string mutatorName, T[][] values, ref T[][] values_benefit)
        {
            if (Sample.IsUseSample)
            {
                // Change values_benefit
                uint size = Convert.ToUInt32(values.Length * Sample.VRC);
                values_benefit = new T[size][];

                // Sort values_benefit
                SortValuesBenefit(mutatorName, values, values_benefit, size);

                // Random the order of array from 1 + top to end
                uint top = Convert.ToUInt32(size * Sample.VTOP);
                RandomArray<uint>(allPosTimes, (int)top, (int)size);

                // Set the values_benefit
                for (uint i = 0; i < size; ++i)
                    values_benefit[i] = values[allPosTimes[i][0]];
            }
            else
            {
                values_benefit = new T[values.Length][];

                // Copy each values to values_benefit
                for (int i = 0; i < values.Length; ++i)
                    values_benefit[i] = values[i];
            }
        }


        // SimpleRandomSample and SystemRandomSample


        private void SimpleRandom(uint _size, ref uint[] _sample_pos)
        {
            int [] pos = new int[_size];
            for (int i = 0; i < _size; ++i)
            {
                pos[i] = i;
            }

            // pos = { 0, 1, 2, 3, 4, 5 ...}

            for (int i = 0; i < _sample_pos.Length; ++i)
            {
                System.Random rand = new System.Random();
                int p = rand.Next(i, (int)_size);
                _sample_pos[i] = (uint)pos[p];
                pos[p] = i;
            }
        }

        private void SystemRandom(uint _size, ref uint[] _sample_pos)
        {
            int step = (int)_size / _sample_pos.Length;

            System.Random rand = new System.Random();
            int first_pos = rand.Next(0, step);

            _sample_pos[0] = (uint)first_pos;

            for (int i = 1; i < _sample_pos.Length; ++i)
            {
                _sample_pos[i] = _sample_pos[i - 1] + (uint)step;
            }
        }

        protected void RandomSample(uint _size, ref uint[] _sample_pos)
        {
            Console.WriteLine("RandomSample(" + _size + ")");
            Console.WriteLine("vrc=" + Sample.VRC);

            uint sample_pos_size = (uint)(_size * Sample.VRC);
            _sample_pos = new uint[sample_pos_size];

            if ("SimpleRandomSample" == Sample.Name)
            {
                SimpleRandom(_size, ref _sample_pos);
            }
            else if ("SystemRandomSample" == Sample.Name)
            {
                SystemRandom(_size, ref _sample_pos);
            }
            else
            {
                Console.WriteLine("Wrong sample class:" + Sample.Name);
            }
        }
	}
}

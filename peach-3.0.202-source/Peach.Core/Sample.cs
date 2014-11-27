
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
using System.Reflection;

using Peach.Core.MutationStrategies;
using Peach.Core.Dom;

using NLog;

namespace Peach.Core
{
    //
    // Benefit Sample
    //
	public static class Sample
	{
        // Is use Sample
        static bool isUseSample;

        static double erc;
        static double mrc;
        static double vrc;

        static double eTop;
        static double mTop;
        static double vTop;

        public static bool IsUseSample
        {
            get { return isUseSample; }
            set { isUseSample = value; }
        }

        public static double ERC
        {
            get { return erc; }
            set { erc = value; }
        }

        public static double MRC
        {
            get { return mrc; }
            set { mrc = value; }
        }

        public static double VRC
        {
            get { return vrc; }
            set { vrc = value; }
        }

        public static double ETOP
        {
            get { return eTop; }
            set { eTop = value; }
        }

        public static double MTOP
        {
            get { return mTop; }
            set { mTop = value; }
        }

        public static double VTOP
        {
            get { return vTop; }
            set { vTop = value; }
        }
    }
}

// end

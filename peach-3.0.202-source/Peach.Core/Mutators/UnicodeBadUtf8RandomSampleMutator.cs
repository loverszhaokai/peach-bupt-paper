﻿
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

namespace Peach.Core.Mutators
{
    [Mutator("StringSimpleRandomSampleMutator using SimpleRandomSample")]
    public partial class UnicodeBadUtf8RandomSampleMutator : Mutator
    {
        uint pos = 0;

        uint[] sample_pos;

        public UnicodeBadUtf8RandomSampleMutator(DataElement obj)
        {
            pos = 0;
            name = "UnicodeBadUtf8RandomSampleMutator";

            RandomSample((uint)values.Length, ref sample_pos);
        }
		
	    public override uint mutation
        {
            get { return pos; }
            set { pos = value; }
        }
		
        public override int count
        {
            get { return sample_pos.Length; }
        }
		
		public new static bool supportedDataElement(DataElement obj)
        {
            if (obj is Dom.String && obj.isMutable)
                return true;

            return false;
		}

        public override void sequentialMutation(DataElement obj)
        {
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
            uint p = sample_pos[pos];
            obj.MutatedValue = new Variant(values[p]);
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
        }

        public override void randomMutation(DataElement obj)
        { }
    }
}

// end

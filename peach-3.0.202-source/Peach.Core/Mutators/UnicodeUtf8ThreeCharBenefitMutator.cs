
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
    [Mutator("Generates bad long UTF-8 three byte strings")]
    public partial class UnicodeUtf8ThreeCharBenefitMutator : Mutator
    {
        // members
        //
        uint pos = 0;
        byte[][] values_benefit;

        // CTOR
        //
        public UnicodeUtf8ThreeCharBenefitMutator(DataElement obj)
        {
            pos = 0;
            name = "UnicodeUtf8ThreeCharBenefitMutator";

            InitValuesBenefit<byte>(name, values, ref values_benefit);
        }

        // MUTATION
        //
        public override uint mutation
        {
            get { return pos; }
            set { pos = value; }
        }

        // COUNT
        //
        public override int count
        {
            get { return values.Length; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            if (obj is Dom.String && obj.isMutable)
                return true;

            return false;
        }

        // SEQUENTIAL_MUTATION
        //
        public override void sequentialMutation(DataElement obj)
        {
            obj.MutatedValue = new Variant(values[pos]);
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;

            // values_pos: the postion in values
            uint values_pos = pos;

            if (Sample.IsUseSample)
                values_pos = allPosTimes[pos][0];
            CurrentMutators.Add("UnicodeUtf8ThreeCharBenefitMutator", values_pos);
        }

        // RANDOM_MUTATION
        //
        public override void randomMutation(DataElement obj)
        {
            uint randPos = context.Random.NextUInt32() % (uint)values.Length;

            obj.MutatedValue = new Variant(values[randPos]);
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;

            // values_pos: the postion in values
            uint values_pos = randPos;

            if (Sample.IsUseSample)
                values_pos = allPosTimes[randPos][0];
            CurrentMutators.Add("UnicodeUtf8ThreeCharBenefitMutator", values_pos);
        }
    }
}

// end

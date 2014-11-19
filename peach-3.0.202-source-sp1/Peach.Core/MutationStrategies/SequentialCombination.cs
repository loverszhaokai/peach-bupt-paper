
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
using System.Reflection;

using NLog;

namespace Peach.Core.MutationStrategies
{

    [MutationStrategy("SequentialCombination", true)]
    [Serializable]
    [Parameter("MaxGroup", typeof(ushort), "max fields to be combined", "3")]
    [Parameter("MinGroup", typeof(ushort), "min fields to be combined", "1")]
    public class SequentialCombination : MutationStrategy
    {

        protected class Iterations : List<Tuple<string, Mutator, string>> { }

        // DataElementMutatorList 2 dimension list
        // {
        //     { "SendDataModel.DataElement_2", Peach.Core.Mutators.DataElementDuplicateMutator, "Run 1.State1.OutputAction.SendDataModel" },
        //     { "SendDataModel.DataElement_2", Peach.Core.Mutators.DataElementDuplicateMutator, "Run 1.State1.OutputAction.SendDataModel" },
        //     { "SendDataModel.DataElement_2", Peach.Core.Mutators.DataElementDuplicateMutator, "Run 1.State1.OutputAction.SendDataModel" },
        //     { "SendDataModel.DataElement_2", Peach.Core.Mutators.DataElementDuplicateMutator, "Run 1.State1.OutputAction.SendDataModel" },
        // }
        protected class DataElementMutatorList : List<List<Tuple<string, Mutator, string>>> { }


        //  CombinationIterations
        //  {
        //      1 combination
        //      {
        //          {   DataElement_0, DataElementDuplicateMutator },
        //      },
        //      {
        //          {   DataElement_0, DataElementRemoveMutator },
        //      },
        //      {
        //          {   DataElement_1, DataElementDuplicateMutator },
        //      },
        //      {
        //          {   DataElement_1, DataElementRemoveMutator },
        //      },
        //      2 combination
        //      {
        //          {   DataElement_0, DataElementDuplicateMutator },
        //          {   DataElement_1, DataElementDuplicateMutator },
        //      },
        //      {
        //          {   DataElement_0, DataElementDuplicateMutator },
        //          {   DataElement_1, DataElementRemoveMutator },
        //      },
        //      {
        //          {   DataElement_0, DataElementRemoveMutator },
        //          {   DataElement_1, DataElementDuplicateMutator },
        //      },
        //      {
        //          {   DataElement_0, DataElementRemoveMutator },
        //          {   DataElement_1, DataElementRemoveMutator },
        //      },
        //  }
        //
        protected class CombinationIterations : List<List<Tuple<DataElement, Mutator>>> { }

        protected class MutatorListEnumerator : List<IEnumerator<Tuple<string, Mutator, string>>> { }


        /// <summary>
        /// pairwise
        /// </summary>
        protected class GroupList : List<List<int>> { }


        //  ems contains DataElement and it's mutators
        //  ems = 
        //  {
        //      { 
        //          DataElement_0, 
        //          {
        //              DataElementDuplicateMutator,
        //              DataElementRemoveMutator
        //          },
        //      }
        //      { 
        //          DataElement_1, 
        //          {
        //              DataElementDuplicateMutator,
        //              DataElementRemoveMutator
        //          },
        //      }
        //      { 
        //          DataElement_2, 
        //          {
        //              DataElementDuplicateMutator,
        //              DataElementRemoveMutator
        //          },
        //      }
        //  }
        protected List<Tuple<DataElement, List<Mutator>>> ems = new List<Tuple<DataElement, List<Mutator>>>();

        [NonSerialized]
        protected static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        [NonSerialized]
        protected IEnumerator<Tuple<string, Mutator, string>> _enumerator;
        [NonSerialized]
        //protected IEnumerator<Tuple<DataElementMutatorList>> combination_enumerator;
        protected IEnumerator<List<Tuple<DataElement, Mutator>>> combination_enumerator;

        [NonSerialized]
        protected Iterations _iterations = new Iterations();

        protected CombinationIterations combination_iterations = new CombinationIterations();
        protected MutatorListEnumerator mutator_list_enumerator = new MutatorListEnumerator();

        private List<Type> _mutators = null;
        private uint _count = 1;
        private uint _iteration = 1;
        /// <summary>
        /// if there is <param name="2_fields_max" value="100"/> and current mutant has 2 fields, 
        ///     then the max_mutants_count = 100
        /// else 
        ///     the max_mutants_count = 0
        /// </summary>
        private int max_mutants_count = 0;

        public SequentialCombination(Dictionary<string, Variant> args)
            : base(args)
        {
            //InitMaxMutantList();
        }

        public override void Initialize(RunContext context, Engine engine)
        {
            base.Initialize(context, engine);

            // Force seed to always be the same
            context.config.randomSeed = 31337;

            Core.Dom.Action.Starting += new ActionStartingEventHandler(Action_Starting);
            Core.Dom.State.Starting += new StateStartingEventHandler(State_Starting);
            context.engine.IterationFinished += new Engine.IterationFinishedEventHandler(engine_IterationFinished);
            context.engine.IterationStarting += new Engine.IterationStartingEventHandler(engine_IterationStarting);
            _mutators = new List<Type>();
            _mutators.AddRange(EnumerateValidMutators());
        }

        void engine_IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
        {
            if (context.controlIteration && context.controlRecordingIteration)
            {
                // Starting to record
                _iterations = new Iterations();
                _count = 0;
            }
        }

        void engine_IterationFinished(RunContext context, uint currentIteration)
        {
            // If we were recording, end of iteration is end of recording
            if (context.controlIteration && context.controlRecordingIteration)
                OnDataModelRecorded();
        }

        public override void Finalize(RunContext context, Engine engine)
        {
            base.Finalize(context, engine);

            Core.Dom.Action.Starting -= Action_Starting;
            Core.Dom.State.Starting -= State_Starting;
            context.engine.IterationStarting -= engine_IterationStarting;
            context.engine.IterationFinished -= engine_IterationFinished;
        }

        protected virtual void OnDataModelRecorded()
        {
        }

        public override bool IsDeterministic
        {
            get
            {
                return true;
            }
        }

        public override uint Iteration
        {
            get
            {
                return _iteration;
            }
            set
            {
                SetIteration(value);
                SeedRandom();
            }
        }

        private void SetIteration(uint value)
        {
            System.Diagnostics.Debug.Assert(value > 0);

            if (_context.controlIteration && _context.controlRecordingIteration)
            {
                return;
            }

            if (_iteration == 1 || value < _iteration)
            {
                // The first time, init _enumerator
                _iteration = 1;
                combination_enumerator = combination_iterations.GetEnumerator();
                combination_enumerator.MoveNext();
            }

            // Value-- when reproduce, so can not always goto next
            JumpToIteration(value);
        }

        private void JumpToIteration(uint iter)
        {
            while (true)
            {
                uint current_count = 1;

                for (int i = 0; i < combination_enumerator.Current.Count; i++)
                    current_count *= (uint)combination_enumerator.Current[i].Item2.count;

                if (iter <= current_count)
                {
                    // Find it
                    iter--;
                    uint m = current_count;
                    for (int i = 0; i < combination_enumerator.Current.Count; i++)
                    {
                        m /= (uint)combination_enumerator.Current[i].Item2.count;
                        combination_enumerator.Current[i].Item2.mutation = iter / m;
                        iter %= m;
                    }

                    break;
                }

                iter -= current_count;
                combination_enumerator.MoveNext();
            }
        }

        private void MoveToNext()
        {
            bool is_current_iteration_finish = true;
            for (int i = combination_enumerator.Current.Count - 1; i >= 0; i++ )
            {
                if (combination_enumerator.Current[i].Item2.mutation != combination_enumerator.Current[i].Item2.count - 1)
                {
                    // Current Mutator has new mutation
                    combination_enumerator.Current[i].Item2.mutation++;
                    for (int j = i + 1; j < combination_enumerator.Current.Count; j++)
                        combination_enumerator.Current[j].Item2.mutation = 0;
                }
            }

            if (is_current_iteration_finish)
            {
                combination_enumerator.MoveNext();

                // Set all the mutation = 0
                for (int i = 0; i < combination_enumerator.Current.Count; i++)
                    combination_enumerator.Current[i].Item2.mutation = 0;
            }
        }

        private void Action_Starting(Core.Dom.Action action)
        {
            // Is this a supported action?
            if (!(action.type == ActionType.Output || action.type == ActionType.SetProperty || action.type == ActionType.Call))
                return;

            if (!_context.controlIteration)
                MutateDataModel(action);

            else if (_context.controlIteration && _context.controlRecordingIteration)
                RecordDataModel(action);
        }

        void State_Starting(State state)
        {
            if (_context.controlIteration && _context.controlRecordingIteration)
            {
                foreach (Type t in _mutators)
                {
                    // can add specific mutators here
                    if (SupportedState(t, state))
                    {
                        var mutator = GetMutatorInstance(t, state);
                        _iterations.Add(new Tuple<string, Mutator, string>("STATE_" + state.name, mutator, null));
                        //_count += (uint)mutator.count;
                    }
                }
            }
        }

        // Recursivly walk all DataElements in a container.
        // Add the element and accumulate any supported mutators.
        private void GatherMutators(string modelName, DataElementContainer cont)
        {
            // allElements contains all the elements
            List<DataElement> allElements = new List<DataElement>();

            // Get all elements
            RecursevlyGetElements(cont, allElements);

            foreach (DataElement elem in allElements)
            {
                bool is_mutable = false;
                List<Mutator> mutators = new List<Mutator>();

                foreach (Type t in _mutators)
                {
                    // We can add specific mutators here
                    if (SupportedDataElement(t, elem))
                    {
                        var mutator = GetMutatorInstance(t, elem);
                        mutators.Add(mutator);
                        is_mutable = true;
                    }
                }

                if (is_mutable)
                {
                    elem.ModelName = modelName;
                    ems.Add(new Tuple<DataElement, List<Mutator>>(elem, mutators));
                }
            }

            SetCombinationIterations();
        }

        private void RecordDataModel(Core.Dom.Action action)
        {
            // ParseDataModel should only be called during iteration 0
            System.Diagnostics.Debug.Assert(_context.controlIteration && _context.controlRecordingIteration);

            if (action.dataModel != null)
            {
                string modelName = GetDataModelName(action);
                GatherMutators(modelName, action.dataModel as DataElementContainer);
            }
            else if (action.parameters != null && action.parameters.Count > 0)
            {
                foreach (ActionParameter param in action.parameters)
                {
                    if (param.dataModel != null)
                    {
                        string modelName = GetDataModelName(action, param);
                        GatherMutators(modelName, param.dataModel as DataElementContainer);
                    }
                }
            }
        }

        /// <summary>
        /// Allows mutation strategy to affect state change.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public override State MutateChangingState(State state)
        {
            if (_context.controlIteration)
                return state;

            if ("STATE_" + state.name == _enumerator.Current.Item1)
            {
                OnMutating(state.name, _enumerator.Current.Item2.name);
                logger.Debug("MutateChangingState: Fuzzing state change: " + state.name);
                logger.Debug("MutateChangingState: Mutator: " + _enumerator.Current.Item2.name);
                return _enumerator.Current.Item2.changeState(state);
            }

            return state;
        }

        private void ApplyMutation(string modelName, DataModel dataModel)
        {

            if (combination_enumerator.Current[0].Item1.ModelName != modelName)
                return;

            for (int i = 0; i < combination_enumerator.Current.Count; ++i)
            {
                var fullName = combination_enumerator.Current[i].Item1.fullName;
                var dataElement = dataModel.find(fullName);

                if (dataElement != null)
                {
                    Mutator mutator = combination_enumerator.Current[i].Item2;
                    OnMutating(fullName, mutator.name);

                    logger.Debug("ApplyMutation: Fuzzing: " + fullName);
                    logger.Debug("ApplyMutation: Mutator: " + mutator.name);

                    mutator.sequentialMutation(dataElement);
                }
            }
        }

        private void MutateDataModel(Core.Dom.Action action)
        {
            // MutateDataModel should only be called after ParseDataModel
            System.Diagnostics.Debug.Assert(_count >= 1);
            System.Diagnostics.Debug.Assert(_iteration > 0);
            System.Diagnostics.Debug.Assert(!_context.controlIteration);

            if (action.dataModel != null)
            {
                string modelName = GetDataModelName(action);
                ApplyMutation(modelName, action.dataModel);
            }
            else if (action.parameters != null && action.parameters.Count > 0)
            {
                foreach (ActionParameter param in action.parameters)
                {
                    if (param.dataModel != null)
                    {
                        string modelName = GetDataModelName(action, param);
                        ApplyMutation(modelName, param.dataModel);
                    }
                }
            }
        }

        public override uint Count
        {
            get
            {
                return _count;
            }
        }

        /*
        * add by zhaokai
        */

        // Set CombinationIterations by ems
        //  {
        //      1 combination
        //      {
        //          {   DataElement_0, DataElementDuplicateMutator },
        //      },
        //      {
        //          {   DataElement_0, DataElementRemoveMutator },
        //      },
        //      {
        //          {   DataElement_1, DataElementDuplicateMutator },
        //      },
        //      {
        //          {   DataElement_1, DataElementRemoveMutator },
        //      },
        //      2 combination
        //      {
        //          {   DataElement_0, DataElementDuplicateMutator },
        //          {   DataElement_1, DataElementDuplicateMutator },
        //      },
        //      {
        //          {   DataElement_0, DataElementDuplicateMutator },
        //          {   DataElement_1, DataElementRemoveMutator },
        //      },
        //      {
        //          {   DataElement_0, DataElementRemoveMutator },
        //          {   DataElement_1, DataElementDuplicateMutator },
        //      },
        //      {
        //          {   DataElement_0, DataElementRemoveMutator },
        //          {   DataElement_1, DataElementRemoveMutator },
        //      },
        //  }
        //
        private void SetCombinationIterations()
        {
            int elementCount = ems.Count;
            // Change MaxGroup in case that the number of elements is less than MaxGroup
            if ((int)MaxGroup > elementCount)
                MaxGroup = (ushort)elementCount;

            for (int combinationNum = MinGroup; combinationNum <= MaxGroup; ++combinationNum)
            {
                List<int> id_list = InitList(combinationNum);

                while (true)
                {
                    // 1. Add it to combination_iterations
                    // 2. Check is finish
                    // 3. Move to next

                    // 1. Add it to combination_iterations
                    AddCombinationIterations(id_list);

                    // 2. Check is finish
                    int cn = id_list.Count;
                    bool is_finish = true;
                    for (int j = cn - 1; j >= 0; j--)
                    {
                        if (id_list[j] < elementCount - (cn - j))
                        {
                            is_finish = false;
                            break;
                        }
                    }
                    if (is_finish)
                        break;

                    // 3. Move to next
                    if (id_list[cn - 1] < elementCount - 1)
                    {
                        // Last id has not been TOP
                        id_list[cn - 1]++;
                    }
                    else
                    {
                        for (int j = cn - 2; j >= 0; j--)
                        {
                            if (id_list[j] < elementCount - (cn - j))
                            {
                                id_list[j]++;
                                for (int k = j + 1; k <= cn - 1; k++)
                                    id_list[k] = id_list[k - 1] + 1;
                                break;
                            }
                        }
                    }
                }
            }
        }

        struct IterationInfo
        {
            public IterationInfo(DataElement _de, Mutator _m, int _de_index, int _m_index)
            {
                de = _de;
                m = _m;
                de_index = _de_index;
                m_index = _m_index;
            }

            public DataElement de;
            public Mutator m;
            public int de_index; // The index of de in ems
            public int m_index; // The index of m in List<Mutator>
        };


        // Add to combination_iterations
        // id_list = { 0, 1 }
        // 
        //      2 combination
        //      {
        //          {   DataElement_0, DataElementDuplicateMutator },
        //          {   DataElement_1, DataElementDuplicateMutator },
        //      },
        //      {
        //          {   DataElement_0, DataElementDuplicateMutator },
        //          {   DataElement_1, DataElementRemoveMutator },
        //      },
        //      {
        //          {   DataElement_0, DataElementRemoveMutator },
        //          {   DataElement_1, DataElementDuplicateMutator },
        //      },
        //      {
        //          {   DataElement_0, DataElementRemoveMutator },
        //          {   DataElement_1, DataElementRemoveMutator },
        //      }
        //
        private void AddCombinationIterations(List<int> id_list)
        {
            List<IterationInfo> eml = new List<IterationInfo>();

            // init eml
            for (int i = 0; i < id_list.Count; ++i)
            {
                int current_id = id_list[i];
                DataElement current_e = ems[current_id].Item1;
                List<Mutator> current_m = ems[current_id].Item2;
                eml.Add(new IterationInfo(current_e, current_m[0], current_id, 0));
            }

            // Add all the cases to combination_iterations
            while (true)
            {
                // 1. Add eml to combination_iterations
                // 2. Whether eml is the last
                // 3. Move to next eml

                // 1. Add eml to combination_iterations
                AddEmlToCombinationIterations(eml);

                // 2. Whether eml is the last
                if (IsLastEml(eml))
                    break;

                // 3. Move to next eml
                EmlMoveNext(ref eml);
            }
        }

        private void AddEmlToCombinationIterations(List<IterationInfo> eml)
        {
            List<Tuple<DataElement, Mutator>> temp = new List<Tuple<DataElement, Mutator>>();

            uint add = 1;

            // 1. Set temp
            for (int i = 0; i < eml.Count; ++i)
            {
                add *= (uint)eml[i].m.count;
                temp.Add(new Tuple<DataElement, Mutator>(eml[i].de, eml[i].m));
            }

            _count += add;

            // 2. Add temp to combination_iterations
            combination_iterations.Add(temp);
        }

        private bool IsLastEml(List<IterationInfo> eml)
        {
            bool is_finish = true;

            for (int i = 0; i < eml.Count; ++i)
            {
                int cnt_element_id = eml[i].de_index;
                int cnt_mutator_id = eml[i].m_index;
                int last_mutator_id = ems[cnt_element_id].Item2.Count - 1;

                if (cnt_mutator_id != last_mutator_id)
                {
                    is_finish = false;
                    break;
                }
            }

            return is_finish;
        }

        private void EmlMoveNext(ref List<IterationInfo> eml)
        {
            int element_id = 0;
            int mutator_id = 0;
            int last_mutator_id = 0;

            for (int i = eml.Count - 1; i >= 0; i--)
            {
                element_id = eml[i].de_index;
                mutator_id = eml[i].m_index;
                last_mutator_id = ems[element_id].Item2.Count - 1;

                if (mutator_id != last_mutator_id)
                {
                    IterationInfo itfo = eml[i];
                    itfo.m = ems[element_id].Item2[mutator_id + 1];
                    itfo.m_index = mutator_id + 1;
                    eml[i] = itfo;

                    // Change mutators
                    for (int j = i + 1; j < eml.Count; j++)
                    {
                        element_id = eml[j].de_index;

                        IterationInfo itfo_l = eml[j];
                        itfo_l.m = ems[element_id].Item2[0];
                        itfo_l.m_index = 0;
                        eml[j] = itfo_l;
                    }

                    break;
                }
            }
        }

        // Init list
        // return { 0, 1, 2 } when combinationNum = 3
        private List<int> InitList(int combinationNum)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < combinationNum; ++i)
                list.Add(i);
            return list;
        }
    }
}

//// end

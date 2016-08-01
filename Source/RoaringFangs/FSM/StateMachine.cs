/*
The MIT License (MIT)

Copyright (c) 2016 Roaring Fangs Entertainment

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using RoaringFangs.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace RoaringFangs.FSM
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    [Serializable]
    public abstract class StateMachine<TStateEnum> :
        MonoBehaviour, IStateManager<TStateEnum>, ISerializationCallbackReceiver
        where TStateEnum : struct, IConvertible
    {
        private static readonly Dictionary<TStateEnum, string> EnumLUT;
        private static readonly Dictionary<string, TStateEnum> StateTypeLUT;

        public static IEnumerable<TStateEnum> GetEnums()
        {
            return Enum.GetValues(typeof(TStateEnum)).Cast<TStateEnum>();
        }

        static StateMachine()
        {
            var enums = GetEnums();
            EnumLUT = enums.ToDictionary(e => e, e => e.ToString());
            StateTypeLUT = enums.ToDictionary(e => e.ToString(), e => e);
        }

        #region Serialization

        [SerializeField]
        private List<StateInfoEntry> _StateInfoList;

        [SerializeField]
        private List<TransitionEntry> _TransitionsList;

        // Converting between sealed types...
        private static TStateEnum ParseEnum(string enum_str)
        {
            try
            {
                return (TStateEnum)Enum.Parse(typeof(TStateEnum), enum_str);
            }
            catch (ArgumentException)
            {
                return default(TStateEnum);
            }
        }

        public void OnBeforeSerialize()
        {
            _StateInfoList = States
                .Select(
                    e => new StateInfoEntry(
                        e.Key.ToString(),
                        e.Value != null ?
                            e.Value.ToSurrogate() :
                            default(StateInfo.Surrogate)))
                .ToList();
            _TransitionsList = Transitions
                .SelectMany(
                    e => e.Value,
                    (from, to) => new TransitionEntry(EnumLUT[from.Key], EnumLUT[to]))
                .ToList();
        }

        public void OnAfterDeserialize()
        {
            if (_StateInfoList != null)
            {
                States = _StateInfoList
                    .ToDictionary(
                        e => ParseEnum(e.State),
                        e => (StateInfo)e.Info);
            }
            if (_TransitionsList != null)
            {
                var transitions_valid = _TransitionsList
                    .Where(e => e.From != null && e.To != null)
                    .Select(e => new KeyValuePair<TStateEnum, TStateEnum>(StateTypeLUT[e.From], StateTypeLUT[e.To]))
                    .ToArray();
                var transitions_lookup = transitions_valid
                    .ToLookup(e => e.Key, e => e.Value);
                var transitions_distinct = transitions_valid
                    .Select(e => e.Key)
                    .Distinct()
                    .ToArray();
                Transitions = transitions_distinct
                    .ToDictionary(
                        e => e,
                        e => new HashSet<TStateEnum>(transitions_lookup[e]));
            }
        }

        #endregion Serialization

        private Dictionary<TStateEnum, StateInfo> _States;

        protected Dictionary<TStateEnum, StateInfo> States
        {
            get
            {
                if (_States == null)
                {
                    _States = new Dictionary<TStateEnum, StateInfo>();
                }
                if (_States.Count != GetEnums().LongCount())
                {
                    PopulateStates(ref _States);
                }
                return _States;
            }
            set { _States = value; }
        }

        public StateInfo GetStateInfo(TStateEnum state)
        {
            return States[state];
        }

        protected void SetStateInfo(TStateEnum state, StateInfo info)
        {
            States[state] = info;
        }

        private void PopulateStates(ref Dictionary<TStateEnum, StateInfo> state_table)
        {
            var enum_values = Enum.GetValues(typeof(TStateEnum)).Cast<TStateEnum>();
            foreach (TStateEnum state in enum_values)
            {
                if (!state_table.ContainsKey(state))
                    state_table[state] = new StateInfo();
            }
        }

        private Dictionary<TStateEnum, HashSet<TStateEnum>> _Transitions;

        protected Dictionary<TStateEnum, HashSet<TStateEnum>> Transitions
        {
            get
            {
                if (_Transitions == null)
                {
                    _Transitions = new Dictionary<TStateEnum, HashSet<TStateEnum>>();
                }
                return _Transitions;
            }
            set { _Transitions = value; }
        }

        public void AddTransition(TStateEnum from, TStateEnum to)
        {
            if (!Transitions.ContainsKey(from))
                Transitions[from] = new HashSet<TStateEnum>();
            Transitions[from].Add(to);
        }

        public void RemoveTransition(TStateEnum from, TStateEnum to)
        {
            if (Transitions.ContainsKey(from))
            {
                Transitions[from].Remove(to);
                if (Transitions[from].Count == 0)
                    Transitions.Remove(from);
            }
        }

        protected virtual void Start()
        {
            CurrentState = default(TStateEnum);
            PopulateStates(ref _States);
        }

        private TStateEnum _CurrentState = default(TStateEnum);

        public TStateEnum CurrentState
        {
            get { return _CurrentState; }
            protected set { _CurrentState = value; }
        }

        public virtual void ChangeState(TStateEnum to_state)
        {
            StateInfo current_state_info = GetStateInfo(CurrentState);
            if (current_state_info.ExitAction != null)
                current_state_info.ExitAction.Invoke(this);

            var destinations = Transitions[CurrentState];

            if (destinations.Contains(to_state))
            {
                StateInfo next_state_info = GetStateInfo(to_state);
                if (next_state_info.EntryAction != null)
                    next_state_info.EntryAction.Invoke(this);
                CurrentState = to_state;
            }
            else
                throw new InvalidOperationException(
                    "Cannot transition from state " + CurrentState + " to state " + to_state);
        }

        public virtual void Update()
        {
            StateInfo active_state_info;
            active_state_info = GetStateInfo(CurrentState);
            if (active_state_info.BodyAction != null)
                active_state_info.BodyAction.Invoke(this);
        }

        /*
        public virtual bool CheckState(int id)
        {
            return _transitions.ContainsKey(TransitionKey.GetHashCode(_activeState, id));
        }
        */

        public virtual void Dispose()
        {
        }
    }
}
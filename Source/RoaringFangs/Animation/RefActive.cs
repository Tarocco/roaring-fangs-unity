﻿/*
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

using System;
using RoaringFangs.Attributes;
using UnityEngine;

namespace RoaringFangs.Animation
{
    [ExecuteInEditMode]
    public class RefActive : RefBoolBehaviorBase, IActiveStateProperty
    {
        [SerializeField, AutoProperty("Target")]
        private MonoBehaviour _TargetBehavior;

        public IActiveStateProperty Target
        {
            get { return (IActiveStateProperty)_TargetBehavior; }
            set { _TargetBehavior = (MonoBehaviour)value; }
        }

        public bool Active
        {
            get
            {
                if (Value.HasValue)
                    return Value.Value;
                else
                    return Target.Active;
            }

            set
            {
                Value = value;
                Target.Active = value;
            }
        }

        private void LateUpdate()
        {
            if (Target != null)
            {
                if (!Value.HasValue)
                    Value = Target.Active;
                Target.Active = Value.Value;
            }
        }

        [ContextMenu("Configure")]
        public void Configure()
        {
            Target = Target ?? GetComponent<IActiveStateProperty>();
        }
    }
}
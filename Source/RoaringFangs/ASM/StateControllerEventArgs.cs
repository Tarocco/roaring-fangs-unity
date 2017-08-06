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
using UnityEngine;

namespace RoaringFangs.ASM
{
    /// <summary>
    /// Note that these are not used with any "real" event type but are used
    /// with direct calls from <see cref="StateController"/> to
    /// <see cref="ControlledStateManager"/>
    /// </summary>
    public class StateControllerEventArgs : EventArgs
    {
        public readonly Animator Animator;
        public readonly AnimatorStateInfo AnimatorStateInfo;
        public readonly int LayerIndex;

        public StateControllerEventArgs(
            Animator animator,
            AnimatorStateInfo animator_state_info,
            int layer_index)
        {
            Animator = animator;
            AnimatorStateInfo = animator_state_info;
            LayerIndex = layer_index;
        }
    }
}
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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoaringFangs.Inputs
{
    [System.Serializable]
    public struct IntRule
    {
        [SerializeField]
        private bool _Bypass;

        public bool Bypass
        {
            get { return _Bypass; }
            set { _Bypass = value; }
        }

        [SerializeField]
        private int _Id;

        public int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        [SerializeField]
        private InputSelection _Source;

        public InputSelection Source
        {
            get { return _Source; }
            set { _Source = value; }
        }

        public IntRule(InputSelection src, int dst)
        {
            _Bypass = true;
            _Id = dst;
            _Source = src;
        }
    }
}
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

namespace RoaringFangs.GSR
{
    [Serializable]
    public class ConfigurableRenderTexture : ITexturable<RenderTexture>
    {
        public bool AutoResize;

        [Range(1, 16)]
        public int SquashPower = 8;

        [Range(1, 16)]
        public int StretchPower = 6;

        public RenderTexture Reference;

        private RenderTexture _Texture = null;

        public RenderTexture Texture
        {
            get { return _Texture; }
            set { _Texture = value; }
        }

        public void Resize(int target_width, int target_height)
        {
            StretchPower = Mathf.Min(SquashPower, StretchPower);
            int width_rd = (target_width >> SquashPower) << StretchPower;
            int height_rd = (target_height >> SquashPower) << StretchPower;

            bool dirty =
                Texture == null ||
                Texture.width != width_rd ||
                Texture.height != height_rd;
            if (dirty)
            {
                RenderTexture.ReleaseTemporary(Texture);
                Texture = RenderTexture.GetTemporary(
                    width_rd,
                    height_rd,
                    Reference.depth,
                    Reference.format,
                    RenderTextureReadWrite.Default);
            }
        }
    }
}
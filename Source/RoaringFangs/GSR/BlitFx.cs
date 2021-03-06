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

using System.Linq;
using UnityEngine;

namespace RoaringFangs.GSR
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class BlitFx : BlitFxBase, ISerializationCallbackReceiver
    {
        [SerializeField]
        //[HideInInspector]
        private PreBlitFx[] _PreBlitFx;

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            foreach (var fx in _PreBlitFx)
            {
                if (fx.enabled && fx.Texture != null && fx.Material != null)
                {
                    Graphics.Blit(fx.GetBufferedCopyOfTexture(), fx.Texture, fx.Material);
                }
            }
            if(Material != null)
                Graphics.Blit(src, dest, Material);
        }

        public void OnBeforeSerialize()
        {
            // Get all of the PreBlitFx components before this component
            var components = GetComponents<MonoBehaviour>();
            var components_before_this = components.TakeWhile(c => this != c);
            var components_between_previous_blitfx = components_before_this
                .Reverse()
                .TakeWhile(c => !(c is BlitFx))
                .Reverse();
            var pre_blit_fx = components_between_previous_blitfx.OfType<PreBlitFx>();
            _PreBlitFx = pre_blit_fx.ToArray();
        }

        public void OnAfterDeserialize()
        {
        }
    }
}
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

using RoaringFangs.GSR;
using UnityEngine;

namespace RoaringFangs.CameraBehavior
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class CameraMimick : MonoBehaviour, ITexturable, ISerializationCallbackReceiver
    {
        [SerializeField]
        private Camera _Camera;

        [SerializeField]
        private Camera _Source;

        public bool PreserveTexture;
        public bool PreserveCullingMask;

        public Texture Texture
        {
            get
            {
                return _Camera.targetTexture;
            }
            set
            {
                _Camera.targetTexture = (RenderTexture)value;
            }
        }

        private void OnPreCull()
        {
            var texture = _Camera.targetTexture;
            var culling_mask = _Camera.cullingMask;
            _Camera.CopyFrom(_Source);
            if (PreserveTexture)
                _Camera.targetTexture = texture;
            if (PreserveCullingMask)
                _Camera.cullingMask = culling_mask;
        }

        public void OnBeforeSerialize()
        {
            if (_Camera == null)
                _Camera = GetComponent<Camera>();
            if (_Source == null && transform.parent != null)
                _Source = transform.parent.GetComponentInParent<Camera>();
            //EditorUtilities.OnBeforeSerializeAutoProperties(this);
        }

        public void OnAfterDeserialize()
        {
        }
    }
}
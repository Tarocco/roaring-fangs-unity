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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoaringFangs.Utility
{
    public static class TransformUtils
    {
        #region Delegates

        public delegate T AddComponentDelegate<T>(GameObject game_object) where T : Component;

        public delegate void SetTransformParentDelegate(Transform t, Transform parent, bool worldPositionStays = true);

        #endregion Delegates

        #region Static Methods

        private static T AddComponent<T>(GameObject game_object) where T : Component
        {
            return game_object.AddComponent<T>();
        }

        public static IEnumerable<T> AddComponents<T>(
            IEnumerable<GameObject> game_objects,
            AddComponentDelegate<T> add_component = null)
            where T : Component
        {
            add_component = add_component ?? AddComponent<T>;
            foreach (GameObject game_object in game_objects)
                yield return add_component(game_object);
        }

        public static void SetTransformParent(Transform t, Transform parent, bool worldPositionStays)
        {
            t.SetParent(parent);
        }

        public static IEnumerable<Transform> GetAllDescendants(params Transform[] transforms)
        {
            return GetAllDescendants((IEnumerable<Transform>)transforms);
        }

        public static IEnumerable<Transform> GetAllDescendants(IEnumerable<Transform> transforms)
        {
            foreach (Transform t in transforms)
            {
                yield return t;
                foreach (Transform t2 in t)
                {
                    foreach (Transform d in GetAllDescendants(t2))
                        yield return d;
                }
            }
        }

        #region Interfaces

        public interface IHasDepth
        {
            int Depth { get; }
        }

        public interface IHasPath
        {
            string Path { get; }
        }

        public interface IHasTransform
        {
            Transform Transform { get; }
        }

        public interface ITransformD : IHasTransform, IHasDepth
        {
        }

        public interface ITransformDP : IHasTransform, IHasDepth, IHasPath
        {
        }

        #endregion Interfaces

        [Serializable]
        public struct TransformD : ITransformD
        {
            [SerializeField]
            private Transform _Transform;

            public Transform Transform
            {
                get { return _Transform; }
                private set { _Transform = value; }
            }

            [SerializeField]
            private int _Depth;

            public int Depth
            {
                get { return _Depth; }
                set { _Depth = value; }
            }

            public TransformD(Transform transform, int depth)
            {
                _Transform = transform;
                _Depth = depth;
            }
        }

        [Serializable]
        public struct TransformDP : ITransformDP
        {
            [SerializeField]
            private Transform _Transform;

            public Transform Transform
            {
                get { return _Transform; }
                private set { _Transform = value; }
            }

            [SerializeField]
            private string _Path;

            public string Path
            {
                get { return _Path; }
                private set { _Path = value; }
            }

            [SerializeField]
            private int _Depth;

            public int Depth
            {
                get { return _Depth; }
                set { _Depth = value; }
            }

            public TransformDP(Transform transform, int depth, string path)
            {
                _Transform = transform;
                _Depth = depth;
                _Path = path;
            }
        }

        public static string GetTransformPath(Transform parent, Transform child)
        {
            if (child.parent == null || child.parent == parent)
                return child.name;
            return GetTransformPath(parent, child.parent) + "/" + child.name;
        }

        public static IEnumerable<ITransformDP> GetAllDescendantsWithPaths(Transform root, Transform t)
        {
            foreach (ITransformDP tdp in GetAllDescendantsWithPaths(root, t, 0, GetTransformPath(root, t)))
            {
                yield return tdp;
            }
        }

        public static IEnumerable<ITransformDP> GetAllDescendantsWithPaths(Transform root, Transform t, IEnumerable<Transform> exclude)
        {
            foreach (ITransformDP tdp in GetAllDescendantsWithPaths(root, t, 0, GetTransformPath(root, t), exclude))
            {
                yield return tdp;
            }
        }

        public static IEnumerable<ITransformDP> GetAllDescendantsWithPaths(Transform root, Transform t, params Transform[] exclude)
        {
            return GetAllDescendantsWithPaths(root, t, (IEnumerable<Transform>)exclude);
        }

        private static IEnumerable<ITransformDP> GetAllDescendantsWithPaths(Transform root, Transform t, int depth, string path)
        {
            yield return new TransformDP(t, depth, path);
            path += "/";
            foreach (Transform t2 in t)
            {
                foreach (TransformDP tp in GetAllDescendantsWithPaths(root, t2, depth + 1, path + t2.name))
                    yield return tp;
            }
        }

        private static IEnumerable<ITransformDP> GetAllDescendantsWithPaths(Transform root, Transform t, int depth, string path, IEnumerable<Transform> exclude)
        {
            if (!exclude.Contains(t))
            {
                yield return new TransformDP(t, depth, path);
                path += "/";
                foreach (Transform t2 in t)
                {
                    foreach (TransformDP tp in GetAllDescendantsWithPaths(root, t2, depth + 1, path + t2.name, exclude))
                        yield return tp;
                }
            }
        }

        public static IEnumerable<Transform> GetAllChildren(IEnumerable<Transform> parents)
        {
            foreach (Transform parent in parents)
            {
                foreach (Transform child in parent)
                    yield return child;
            }
        }

        public static IEnumerable<Transform> GetChildren(Transform parent)
        {
            foreach (Transform child in parent)
                yield return child;
        }

        public static List<Transform> GetMatchingTransforms(Transform transform, string prefix)
        {
            List<Transform> matching = new List<Transform>();
            foreach (Transform t in GetAllDescendants(transform))
            {
                if (t.name.StartsWith(prefix))
                    matching.Add(t);
            }
            return matching;
        }

        public static IEnumerable<Transform> GetTransforms(IEnumerable<Component> components)
        {
            foreach (var @object in components)
                yield return @object.transform;
        }

        public static IEnumerable<Transform> GetTransforms(IEnumerable objects)
        {
            foreach (var @object in objects)
                yield return ((GameObject)@object).transform;
        }

        public static T GetComponentInChildrenExclusively<T>(Transform self) where T : class
        {
            foreach (Transform t in self)
            {
                var component = t.GetComponent<T>();
                if (component != null)
                    return component;
            }
            return null;
        }

        public static IEnumerable<T> GetComponentsInChildrenExclusively<T>(Transform self) where T : class
        {
            foreach (Transform t in self)
            {
                var components = t.GetComponents<T>();
                if (components != null)
                    foreach (T component in components)
                        yield return component;
            }
        }

        public static IEnumerable<T> GetComponents<T>(IEnumerable<Transform> transforms) where T : class
        {
            foreach (Transform t in transforms)
            {
                var components = t.GetComponents<T>();
                if (components != null)
                    foreach (T component in components)
                        yield return component;
            }
        }

        public static IEnumerable<T> GetComponentsInDescendants<T>(Transform root, bool include_all = false) where T : class
        {
            foreach (Transform t in root)
            {
                if (include_all || t.gameObject.activeSelf)
                {
                    var components = t.GetComponents<T>();
                    if (components != null)
                        foreach (T component in components)
                            yield return component;
                    foreach (T c in GetComponentsInDescendants<T>(t, include_all))
                        yield return c;
                }
            }
        }

        public static IEnumerable<T> GetComponentsInDescendants<T>(IEnumerable<Transform> transforms, bool include_all = false) where T : class
        {
            foreach (Transform t in transforms)
            {
                foreach (T c in GetComponentsInDescendants<T>(t, include_all))
                    yield return c;
            }
        }

        public static T GetComponentInParent<T>(Transform transform, bool include_all = false) where T : class
        {
            if (include_all || transform.gameObject.activeSelf)
            {
                T component = transform.GetComponent<T>();
                if (component != null)
                    return component;
                else if (transform.parent != null)
                    return GetComponentInParent<T>(transform.parent, include_all);
            }
            return null;
        }

        public class WithDepth<T> : IHasDepth
        {
            public readonly T Self;
            private int _Depth;

            public int Depth
            {
                get { return _Depth; }
                private set { _Depth = value; }
            }

            public WithDepth(T self, int depth)
            {
                Self = self;
                _Depth = depth;
            }
        }

        public static IEnumerable<WithDepth<T>> GetComponentsInDescendantsWithDepth<T>(Transform root, bool include_all = false) where T : class
        {
            return GetComponentsInDescendantsWithDepth<T>(root, include_all, 0);
        }

        public static IEnumerable<WithDepth<T>> GetComponentsInDescendantsWithDepth<T>(Transform root, bool include_all, int depth) where T : class
        {
            foreach (Transform t in root)
            {
                if (include_all || t.gameObject.activeSelf)
                {
                    T component = t.GetComponent<T>();
                    if (component != null)
                        yield return new WithDepth<T>(component, depth);
                    foreach (T c in GetComponentsInDescendants<T>(t, include_all))
                        yield return new WithDepth<T>(c, depth + 1);
                }
            }
        }

        public static IEnumerable<GameObject> FindAll(params string[] paths)
        {
            foreach (string path in paths)
                yield return GameObject.Find(path);
        }

        public static bool IsDescendant(Transform parent, Transform descendant)
        {
            if (descendant.parent == parent)
                return true;
            if (descendant.parent == null)
                return false;
            return IsDescendant(parent, descendant.parent);
        }

        public static Transform GetTransformAtPath(Transform from, string path)
        {
            if (from)
            {
                var path_from = GetTransformPath(null, from);
                var path_resolved = Paths.ResolvePath(path_from, path);
                return Find(path_resolved).transform;
            }
            else
            {
                return Find(path).transform;
            }
        }

        private static Transform Find(string path)
        {
            var game_object = GameObject.Find(path);
            if (game_object)
                return game_object.transform;
            else
                throw new ArgumentNullException(path, "Game object at path not found");
        }

        public static string GetTransformPathRelative(Transform from, Transform transform)
        {
            string path = GetTransformPath(null, transform);
            if (from)
            {
                var path_from = GetTransformPath(null, from);
                return Paths.GetRelativePath(path_from, path);
            }
            else
                return path;
        }

        public static string GetTransformPathRelative(Transform from, string path)
        {
            if (from)
            {
                var path_from = GetTransformPath(null, from);
                return Paths.ResolvePath(path_from, path);
            }
            else
                return path;
        }

        public static IEnumerable<Transform> Ancestors(Transform current)
        {
            while (current != null)
            {
                yield return current;
                current = current.parent;
            }
        }

        public static IEnumerable<Transform> Ancestors(IEnumerable<Transform> current)
        {
            foreach (Transform t in current)
            {
                foreach (Transform c in Ancestors(t))
                    yield return c;
            }
        }

        public static GameObject FindGameObject(Transform parent, string name)
        {
            if (parent.name == name)
                return parent.gameObject;
            foreach (Transform t in parent)
                return FindGameObject(t, name);
            return null;
        }

		// Very WET

		public static void SyncObjectWithPath<T>(
			Transform parent,
			ref T component,
			ref string path,
			Type type = null)
			where T: Component
		{
			if (parent == null)
				throw new ArgumentNullException ("Parent cannot be null");
			if (component == null)
			{
				if (!String.IsNullOrEmpty (path))
				{
					var transform_at_path = parent.Find (path);
					if (transform_at_path)
						component = (T)transform_at_path.GetComponent(type ?? typeof(T));
				}
			}
			else
			{
				path = GetTransformPathRelative (parent, component.transform);		
			}
		}

		public static void SyncObjectWithPath(Transform parent, ref GameObject game_object, ref string path)
		{
			if (parent == null)
				throw new ArgumentNullException ("Parent cannot be null");
			if (game_object == null)
			{
				var transform_at_path = parent.Find (path);
				if (transform_at_path)
					game_object = transform_at_path.gameObject;
			}
			else
			{
				path = GetTransformPathRelative (parent, game_object.transform);		
			}
		}



        #endregion Static Methods
    }
}
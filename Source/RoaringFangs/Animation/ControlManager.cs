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

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RoaringFangs.Utility;

namespace RoaringFangs.Animation
{
    [ExecuteInEditMode]
    public class ControlManager : MonoBehaviour
    {
        #region Subject
        [HideInInspector]
        private GameObject _Subject;
        public GameObject Subject
        {
            get
            {
                // If the subject is not set
                if (_Subject == null)
                {
                    // If the subject path is set, find the subject by its path and set the backing field reference
                    if(!String.IsNullOrEmpty(_SubjectPath))
                    {
                        
                        // Subject path is an absolute path
                        if (_SubjectPath.StartsWith("/"))
                        {
                            // Find in scene root
                            _Subject = GameObject.Find(_SubjectPath);
                        }
                        else
                        {
                            // Subject path is in parent-relative (right now only handling 1 level)
                            if (_SubjectPath.StartsWith("../"))
                            {
                                string subject_path_rel_to_parent = _SubjectPath.Substring(3);
                                if (transform.parent != null)
                                {
                                    // Find in parent transform
                                    _Subject = transform.parent.Find(subject_path_rel_to_parent).gameObject;
                                }
                                else
                                {
                                    // Find in scene root
                                    _Subject = GameObject.Find(subject_path_rel_to_parent);
                                }
                            }
                            // Subject path is relative
                            else
                            {
                                _Subject = transform.Find(_SubjectPath).gameObject;
                            }
                        }
                    }
                }
                return _Subject;
            }
            set
            {
                if (value != _Subject)
                {
                    _Subject = value;
                    if (value != null)
                    {
                        // TODO: is transform.parent a good idea here for the "root" argument?
                        // Right now this allows the subject to be a sibling, but not a parent
                        _SubjectPath = "../" + TransformUtils.GetTransformPath(transform.parent, value.transform);
                        SubjectPathAbs = TransformUtils.GetTransformPath(null, value.transform);
                    }
                    else
                    {
                        _SubjectPath = null;
                        SubjectPathAbs = null;
                    }
                    CachedSubjectDescendantsAndPaths = CollectSubjectDescendants();
                    NotifyControlGroupsOfSubjectDescendants();
                }
            }
        }
        #endregion
        #region Subject Paths
        [SerializeField, HideInInspector]
        private string _SubjectPath;
        public string SubjectPath
        {
            get { return _SubjectPath; }
            set
            {
                if (value != _SubjectPath)
                {
                    _SubjectPath = value;
                    if (String.IsNullOrEmpty(value))
                        Subject = null;
                }
            }
        }
        private string _SubjectPathAbs;
        public string SubjectPathAbs
        {
            get
            {
                if (String.IsNullOrEmpty(_SubjectPathAbs))
                {
                    var subject = Subject;
                    if (subject != null)
                        _SubjectPathAbs = TransformUtils.GetTransformPath(null, subject.transform);
                    else
                        return null;
                }
                return _SubjectPathAbs;
            }
            private set
            {
                _SubjectPathAbs = value;
            }
        }
        #endregion
        #region Cached Subject Descendants
        [SerializeField, HideInInspector]
        private TransformUtils.TransformDP[] _CachedSubjectDescendantsAndPaths;
        public IEnumerable<TransformUtils.ITransformDP> CachedSubjectDescendantsAndPaths
        {
            get
            {
                // Lazy initialization
                if (_CachedSubjectDescendantsAndPaths == null)
                {
                    CachedSubjectDescendantsAndPaths = CollectSubjectDescendants();
                }
                // Double null check for quality assurance
                if (_CachedSubjectDescendantsAndPaths != null)
                {
                    foreach (var tdp in _CachedSubjectDescendantsAndPaths)
                        yield return tdp;
                }
            }
            private set
            {
                // Ifsetting the cached subject descendants
                if (value != null)
                {
                    // Create a concrete struct array from the abstract (interface) enumerable
                    _CachedSubjectDescendantsAndPaths = value
                        .Select(t => new TransformUtils.TransformDP(t.Transform, t.Depth, t.Path))
                        .ToArray();
                    // Notify control groups with descendants
                    NotifyControlGroupsOfSubjectDescendants(value);
                }
                // else if clearing them and they aren't already cleared
                else if(_CachedSubjectDescendantsAndPaths != null)
                {
                    _CachedSubjectDescendantsAndPaths = null;
                    // Notify control groups to clear
                    NotifyControlGroupsOfSubjectDescendants(null);
                }
                
            }
        }

        public void NotifyControlGroupsOfSubjectDescendants()
        {
            NotifyControlGroupsOfSubjectDescendants(CachedSubjectDescendantsAndPaths);
        }

        private void NotifyControlGroupsOfSubjectDescendants(IEnumerable<TransformUtils.ITransformDP> cached_subject_descendants_and_paths)
        {
            var groups = TransformUtils.GetComponentsInDescendants<TargetGroupBehavior>(transform, true);
            foreach (var group in groups)
                group.OnSubjectChanged(cached_subject_descendants_and_paths);
        }

        protected IEnumerable<TransformUtils.ITransformDP> CollectSubjectDescendants()
        {
            if (Application.isPlaying)
                Debug.LogWarning("CollectSubjectDescendants called at runtime!");
            if (Subject == null)
                return null;
            return TransformUtils.GetAllDescendantsWithPaths(Subject.transform.parent, Subject.transform);
        }
        #endregion
        #region Targets
        private struct TargetInfo
        {
            public readonly int Depth;
            public readonly bool Active;
            public TargetInfo(int depth, bool active)
            {
                Depth = depth;
                Active = active;
            }
        }

        private struct TargetRule
        {
            public readonly Transform Transform;
            public readonly TargetInfo Info;
            public TargetRule(Transform transform, int depth, bool active)
            {
                Transform = transform;
                Info = new TargetInfo(depth, active);
            }
        }
        private Dictionary<Transform, TargetInfo> TargetDataPrevious = new Dictionary<Transform, TargetInfo>();
        #endregion

        private bool _FirstEnable = true;
        void OnEnable()
        {
            if (_FirstEnable)
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    CollectSubjectDescendants();
                    RoaringFangs.Editor.EditorHelper.HierarchyObjectPathChanged += HandleHierarchyObjectPathChanged;
                }
#endif
                _FirstEnable = false;
            }
        }

        // bug you also forgot to add this unsubscribe thing, that caused exceptions
        void OnDisable()
        {
#if UNITY_EDITOR
            RoaringFangs.Editor.EditorHelper.HierarchyObjectPathChanged -= HandleHierarchyObjectPathChanged;
#endif
        }

        private void HandleHierarchyObjectPathChanged(object sender, RoaringFangs.Editor.EditorHelper.HierarchyObjectPathChangedEventArgs args)
        {
            // If the change had anything to do with the subject
            if (!String.IsNullOrEmpty(SubjectPathAbs) &&
                ((!String.IsNullOrEmpty(args.NewPath) && args.NewPath.StartsWith(SubjectPathAbs) ||
                (!String.IsNullOrEmpty(args.OldPath) && args.OldPath.StartsWith(SubjectPathAbs)))))
            {
                // Lazily invalidate the cached subject descentants and paths
                //CachedSubjectDescendantsAndPaths = null;
                // bug how about reinitialize instead of invalidating ? this is what caused the issue
                // lazy initialization didn't seem to work
                CachedSubjectDescendantsAndPaths = CollectSubjectDescendants();
            }
        }

        void Update()
        {
            
            //var groups = TransformUtils.GetComponentsInDescendants<TargetGroupBehavior>(transform, true);
            // all those yields and enumerables are cool and everything, but i'd like to be safe at least for debugging
            var groups = transform.GetComponentsInChildren<TargetGroupBehavior>(true);

            //IEnumerable<TargetRule> targets = groups
            //    .Where(g => g.Targets != null)
            //    .SelectMany(g => g.Targets
            //        .Select(t => new TargetRule(t.Transform, t.Depth, g.gameObject.activeSelf)));
            //var targets_array = targets.ToArray();

            // all good, always correct value
            //Debug.Log(groups.Length);


            //var groupCount = 0;
            var targets_array = new List<TargetRule>();
            foreach (var group in groups) // replaced that non-debuggable linq statement to foreach loop
            {
                //var groupTargetCount = 0;

                if (group.Targets != null)
                {
                    foreach (var target in group.Targets)
                    {
                        //++groupTargetCount;
                        targets_array.Add(new TargetRule(target.Transform, target.Depth, group.gameObject.activeSelf));
                    }
                }

                //Debug.Log("groupNumber " + groupCount++ + " targetCount = " + groupTargetCount);
            }


            //var target_data_previous = new Dictionary<Transform, TargetInfo>();
            foreach (var target in targets_array)
            {
                if (target.Transform != null)
                {
                    TargetInfo target_info_previous;
                    bool have_previous = TargetDataPrevious.TryGetValue(target.Transform, out target_info_previous);
                    bool active_update = !have_previous ||
                        target.Info.Active != target_info_previous.Active ||
                        target.Info.Depth != target_info_previous.Depth;
                    if (active_update)
                    {
                        // Update the target game object
                        target.Transform.gameObject.SetActive(target.Info.Active);
                    }
                    // Add to previous target data dictionary
                    TargetDataPrevious[target.Transform] = target.Info;
                }
            }
            // Update previous value dictionary
            //TargetDataPrevious = target_data_previous; // dafuk are you doing m8, dictionary is not a struct


            //var target_data_previous = new Dictionary<Transform, TargetInfo>();
            //foreach (var target in targets_array)
            //{
            //    if (target.Transform != null)
            //    {
            //        TargetInfo target_info_previous;
            //        bool have_previous = TargetDataPrevious.TryGetValue(target.Transform, out target_info_previous);
            //        bool active_update = !have_previous ||
            //            target.Info.Active != target_info_previous.Active ||
            //            target.Info.Depth != target_info_previous.Depth;
            //        if (active_update)
            //        {
            //            // Update the target game object
            //            target.Transform.gameObject.SetActive(target.Info.Active);
            //        }
            //        // Add to previous target data dictionary
            //        target_data_previous[target.Transform] = target.Info;
            //    }
            //}
            //// Update previous value dictionary
            //TargetDataPrevious = target_data_previous;
        }
#if UNITY_EDITOR
        [MenuItem("Sprites And Bones/Animation/Control Manager", false, 0)]
        [MenuItem("GameObject/Sprites And Bones/Control Manager", false, 0)]
        public static ControlManager Create()
        {
            GameObject manager_object = new GameObject("Control Manager");
            Undo.RegisterCreatedObjectUndo(manager_object, "Add Control Manager");
            ControlManager manager = manager_object.AddComponent<ControlManager>();
            GameObject selected = Selection.activeGameObject;
            if (selected != null)
            {
                Undo.SetTransformParent(manager_object.transform, selected.transform.parent, "Add Control Manager");
                manager.Subject = selected;
            }
            return manager;
        }
#endif
    }
}

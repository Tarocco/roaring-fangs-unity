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

#if UNITY_EDITOR

using UnityEditor;

#endif

using System;
using System.Collections.Generic;
using System.Linq;

using RoaringFangs.Utility;
using RoaringFangs.Editor;

namespace RoaringFangs.Animation
{
    [ExecuteInEditMode]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class ControlManager : MonoBehaviour, IHasHierarchyIcons
    {
#if UNITY_EDITOR

        [HideInInspector, SerializeField]
        public bool Editor__ShowTargetGroups;

        [HideInInspector, SerializeField]
        public UnityEngine.Object[] Editor__TargetGroupsShown;

#endif
        public static Texture2D HIControlManager { get; protected set; }

        #region Subject

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
                    if (!String.IsNullOrEmpty(value))
                    {
                        try
                        {
                            var subject_transform = TransformUtils.GetTransformAtPath(transform, value);
                            _Subject = subject_transform.gameObject;
                            _SubjectPathAbs = TransformUtils.GetTransformPath(null, subject_transform);
                            _SubjectPath = value;
                        }
                        catch (ArgumentNullException ex)
                        {
                            Debug.LogWarning(ex, this);
                        }
                    }
                    else
                    {
                        _Subject = null;
                        _SubjectPath = null;
                        _SubjectPathAbs = null;
                    }
                }
            }
        }

        private string _SubjectPathAbs;

        public string SubjectPathAbs
        {
            get
            {
                if (_SubjectPathAbs == null)
                    _SubjectPathAbs = TransformUtils.GetTransformPathRelative(transform, _SubjectPath);
                return _SubjectPathAbs;
            }
            set
            {
                if (value != _SubjectPathAbs)
                {
                    if (!String.IsNullOrEmpty(value))
                    {
                        try
                        {
                            var subject_transform = TransformUtils.GetTransformAtPath(null, value);
                            _Subject = subject_transform.gameObject;
                            _SubjectPath = TransformUtils.GetTransformPath(transform, subject_transform);
                            _SubjectPathAbs = value;
                        }
                        catch (ArgumentNullException ex)
                        {
                            Debug.LogWarning(ex, this);
                        }
                    }
                    else
                    {
                        _Subject = null;
                        _SubjectPath = null;
                        _SubjectPathAbs = null;
                    }
                }
            }
        }

        #endregion Subject Paths

        [SerializeField, HideInInspector]
        private GameObject _Subject;

        public GameObject Subject
        {
            get
            {
                // If the subject is not set and the subject path is set, find the subject by its path and set the backing field reference
                if (_Subject == null && !String.IsNullOrEmpty(_SubjectPathAbs))
                {
                    try
                    {
                        _Subject = TransformUtils.GetTransformAtPath(null, _SubjectPathAbs).gameObject;
                    }
                    catch (ArgumentNullException ex)
                    {
                        Debug.LogWarning(ex, this);
                    }
                }
                return _Subject;
            }
            set
            {
                if (value != null)
                {
                    _SubjectPath = TransformUtils.GetTransformPathRelative(transform, value.transform);
                    _SubjectPathAbs = TransformUtils.GetTransformPathRelative(null, value.transform);
                }
                else
                {
                    try
                    {
                        // Only clear the subject path(s) if the subject is still there
                        // (i.e. if setting the subject to null was deliberate)
                        TransformUtils.GetTransformAtPath(null, _SubjectPathAbs);
                        _SubjectPath = null;
                        _SubjectPathAbs = null;
                    }
                    catch (ArgumentNullException ex)
                    {
                        // Pass
                    }
                }
                if (value != _Subject)
                {
                    _Subject = value;
                    CachedSubjectDescendantsAndPaths = GetSubjectDescendants();
                }
            }
        }

        #endregion Subject

        #region Cached Subject Descendants

        private TransformUtils.TransformDP[] _CachedSubjectDescendantsAndPaths;

        public IEnumerable<TransformUtils.ITransformDP> CachedSubjectDescendantsAndPaths
        {
            get
            {
                // Lazy initialization
                if (_CachedSubjectDescendantsAndPaths == null)
                {
                    CachedSubjectDescendantsAndPaths = GetSubjectDescendants();
                }
                // Double null check for quality assurance
                if (_CachedSubjectDescendantsAndPaths != null)
                {
                    return _CachedSubjectDescendantsAndPaths.Cast<TransformUtils.ITransformDP>().ToArray();
                }
                return null;
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
                    Debug.Log("Cached descendants", this);
                }
                // else if clearing them and they aren't already cleared
                else if (_CachedSubjectDescendantsAndPaths != null)
                {
                    _CachedSubjectDescendantsAndPaths = null;
                    // Notify control groups to clear
                    NotifyControlGroupsOfSubjectDescendants(null);
                    Debug.Log("Cleared Descendants", this);
                }
            }
        }

        public void NotifyControlGroupsOfSubjectDescendants()
        {
            NotifyControlGroupsOfSubjectDescendants(CachedSubjectDescendantsAndPaths);
        }

        private void NotifyControlGroupsOfSubjectDescendants(
            IEnumerable<TransformUtils.ITransformDP> cached_subject_descendants_and_paths)
        {
            var groups = TransformUtils.GetComponentsInDescendants<TargetGroupBase>(transform, true)
                .OfType<ITargetGroup>();
            foreach (var group in groups)
            {
                group.OnFindMatchingTargetsInDescendants(cached_subject_descendants_and_paths);
            }
        }

        protected IEnumerable<TransformUtils.ITransformDP> GetSubjectDescendants()
        {
            if (Application.isPlaying)
                Debug.LogWarning("CollectSubjectDescendants called at runtime!");
            if (Subject == null)
                return null;
            return TransformUtils.GetAllDescendantsWithPaths(
                Subject.transform.parent,
                Subject.transform,
                transform);
        }

        #endregion Cached Subject Descendants

        #region Targets

        private struct TargetInfo
        {
            public readonly int Depth;

            public TargetInfo(int depth)
            {
                Depth = depth;
            }
        }

        private struct TargetRule
        {
            public readonly Transform Transform;
            public readonly TargetInfo Info;

            public TargetRule(Transform transform, int depth)
            {
                Transform = transform;
                Info = new TargetInfo(depth);
            }
        }

        private struct GroupTargetKey
        {
            public readonly ITargetGroup Group;
            public readonly IActiveStateProperty ActiveStateProperty;
            public GroupTargetKey(ITargetGroup group, IActiveStateProperty active_state_property)
            {
                Group = group;
                ActiveStateProperty = active_state_property;
            }
        }

        private Dictionary<GroupTargetKey, TargetRule[]> _GroupedTargets;

        private Dictionary<GroupTargetKey, TargetRule[]> GroupedTargets
        {
            get
            {
                if (_GroupedTargets == null)
                    _GroupedTargets = GetAllGroupedTargets();
                return _GroupedTargets;
            }
            set
            {
                _GroupedTargets = value;
            }
        }

        private Dictionary<Transform, TargetInfo> TargetDataPrevious =
            new Dictionary<Transform, TargetInfo>();

        #endregion Targets

        private IEnumerable<ITargetGroup> GetAllTargetGroups()
        {
            return TransformUtils.GetComponentsInDescendants<TargetGroupBase>(transform, true)
                .OfType<ITargetGroup>();
        }

        private Dictionary<GroupTargetKey, TargetRule[]> GetAllGroupedTargets()
        {
            var groups = GetAllTargetGroups();
            // For each groups array, select valid target lists in all of the target groups and create
            // rules on whether to show or hide the targets based on the control group's active state
            return groups
                .Where(g => g.Targets != null)
                .ToDictionary(
                    g => new GroupTargetKey(g, (g as Component).GetComponent<IActiveStateProperty>()),
                    g => g.Targets.Select(t => new TargetRule(t.Transform, t.Depth)).ToArray()
                );
        }

        public void OnUpdateGroupTargets(ITargetGroup group)
        {
            var matching = GroupedTargets.Keys.Where(t => t.Group == group);
            foreach (var e in matching.ToArray())
            {
                var targets = e.Group.Targets ?? Enumerable.Empty<TransformUtils.ITransformD>();
                var target_rules = e.Group.Targets
                    .Select(t => new TargetRule(t.Transform, t.Depth)).ToArray();
                GroupedTargets[e] = target_rules;
            }
        }

        private void OnEnable()
        {
            GroupedTargets = GetAllGroupedTargets();
#if UNITY_EDITOR
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                CachedSubjectDescendantsAndPaths = GetSubjectDescendants();
                EditorHelper.HierarchyObjectPathChanged += HandleHierarchyObjectPathChanged;
            }
            else
            {
                EditorHelper.HierarchyObjectPathChanged -= HandleHierarchyObjectPathChanged;
            }
            FindIcons();
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorHelper.HierarchyObjectPathChanged -= HandleHierarchyObjectPathChanged;
#endif
        }

#if UNITY_EDITOR
        private bool PathChangeHandledOnceThisUpdate = false;

        private void HandleHierarchyObjectPathChanged(
            object sender,
            EditorHelper.HierarchyObjectPathChangedEventArgs args)
        {
            // If the change had anything to do with the subject, notify
            // control groups of the changes to the subject descendants
            if (!String.IsNullOrEmpty(SubjectPathAbs))
            {
                // Subject root was affected
                bool root_affected =
                    SubjectPathAbs == args.NewPath ||
                    SubjectPathAbs == args.OldPath;
                // Subject descendants affected
                bool descendants_affected =
                    Paths.IsSubPath(SubjectPathAbs, args.NewPath) ||
                    Paths.IsSubPath(SubjectPathAbs, args.OldPath);
                if (root_affected)
                {
                    // If the subject is not null (destroyed), set the subject
                    if (args.GameObject != null)
                        Subject = args.GameObject;
                }
                if (descendants_affected)
                {
                    // Handle invalidation
                    if (!PathChangeHandledOnceThisUpdate)
                    {
                        CachedSubjectDescendantsAndPaths = GetSubjectDescendants();
                        NotifyControlGroupsOfSubjectDescendants();
                        PathChangeHandledOnceThisUpdate = true;
                        GroupedTargets = GetAllGroupedTargets();
                    }
                }
            }
        }

#endif

        private void Update()
        {
#if UNITY_EDITOR
            PathChangeHandledOnceThisUpdate = false;
#endif
            // Update any of the ActiveStateProperty controllers
            foreach (var entry in GroupedTargets)
            {
                var group_info = entry.Key;
                var active_state_property = group_info.ActiveStateProperty;
                if (active_state_property != null)
                    active_state_property.ManagedUpdate();
                // Get the group and targets from the grouping object
                var group = group_info.Group;
                // Skip if group is not valid
                if (group.Equals(null))
                    continue;
                var targets = entry.Value;
                // Buffer the selection into an array
                var targets_array = targets;
                for (int i = 0; i < targets_array.Length; i++)
                {
                    var target = targets_array[i];
                    // Skip if the target transform is not valid
                    if (target.Transform == null)
                        continue;
                    // Skip if the target transform is this transform
                    if (transform.IsChildOf(target.Transform))
                        continue;

                    bool target_active = target.Transform.gameObject.activeSelf;
                    bool group_active = group.ActiveInHierarchy;
                    bool active;

                    switch (group.Mode)
                    {
                        default:
                        case TargetGroupMode.Set:
                            active = group_active;
                            break;

                        case TargetGroupMode.AND:
                            active = target_active && group_active;
                            break;

                        case TargetGroupMode.OR:
                            active = target_active || group_active;
                            break;

                        case TargetGroupMode.XOR:
                            active = target_active ^ group_active;
                            break;
                    }

                    target.Transform.gameObject.SetActive(active);
                }
            }
        }

#if UNITY_EDITOR

        [MenuItem("Roaring Fangs/Animation/Control Manager", false, 0)]
        [MenuItem("GameObject/Roaring Fangs/Animation/Control Manager", false, 0)]
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

        public void OnDrawHierarchyIcons(Rect icon_position)
        {
            UnityEngine.GUI.Label(icon_position, HIControlManager);
        }

        public static void FindIcons()
        {
            var icons = HierarchyIcons.GetIcons("ControlManager", HierarchyIcons.KeyMode.LowerCase);
            HIControlManager = icons.GetOrDefault("controlmanager.png");
        }

#endif
    }
}
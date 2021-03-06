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

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace RoaringFangs.Editor
{
    public static partial class EditorHelper
    {
        private const string EnableHierarchyObjectPathTrackingKey =
            "EditorHelperPreferences.EnableHierarchyObjectTracking";

        private static bool? _EnableHierarchyObjectPathTracking;

        public static bool EnableHierarchyObjectPathTracking
        {
            get
            {
                if (!_EnableHierarchyObjectPathTracking.HasValue)
                    _EnableHierarchyObjectPathTracking =
                        EditorPrefs.GetBool(
                            EnableHierarchyObjectPathTrackingKey,
                            false);
                return _EnableHierarchyObjectPathTracking.Value;
            }
            set
            {
                if (value != _EnableHierarchyObjectPathTracking)
                {
                    EditorPrefs.SetBool(
                        EnableHierarchyObjectPathTrackingKey,
                        value);
                    _EnableHierarchyObjectPathTracking = value;
                    EditorApplication.hierarchyWindowChanged -=
                        HandleHierarchyWindowChanged;
                    if (value)
                    {
                        EditorApplication.hierarchyWindowChanged +=
                            HandleHierarchyWindowChanged;
                        // Initialize by calling the handler once
                        HandleHierarchyWindowChanged();
                    }
                }
            }
        }

        private const string ShowParallelTasksMessageKey =
            "EditorHelperPreferences.ShowParallelTasksMessage";

        private static bool? _ShowParallelTasksMessage;

        public static bool ShowParallelTasksMessage
        {
            get
            {
                if (!_ShowParallelTasksMessage.HasValue)
                    _ShowParallelTasksMessage =
                        EditorPrefs.GetBool(
                            ShowParallelTasksMessageKey,
                            false);
                return _ShowParallelTasksMessage.Value;
            }
            set
            {
                if (value != _ShowParallelTasksMessage)
                {
                    EditorPrefs.SetBool(
                        ShowParallelTasksMessageKey,
                        value);
                    _ShowParallelTasksMessage = value;
                }
            }
        }

        private static GUISkin _DefaultSkin;

        private static GUILayoutOption _DebugLabelWidth =
            GUILayout.Width(192);

        private static GUILayoutOption _DebugLabelHeight =
            GUILayout.Height(EditorGUIUtility.singleLineHeight);

        private static void DrawLabeledReadout(
            string label_text,
            string readout_text)
        {
            using (new GUILayout.HorizontalScope())
            {
                UnityEngine.GUI.SetNextControlName(label_text + " Label");
                GUILayout.Label(
                    label_text,
                    _DebugLabelWidth);
                UnityEngine.GUI.SetNextControlName(label_text + " Readout");
                EditorGUILayout.SelectableLabel(
                    readout_text,
                    _DefaultSkin.textField,
                    _DebugLabelHeight);
            }
        }

        private static void DrawLabeledReadout(
            string label_text,
            object readout_object)
        {
            DrawLabeledReadout(label_text, readout_object.ToString());
        }

        [PreferenceItem("RF Editor Helper")]
        public static void PreferencesGUI()
        {
            _DefaultSkin = _DefaultSkin ?? UnityEngine.GUI.skin;
            var enable_hierarchy_object_path_tracking_label = new GUIContent(
                "Enable Hierarchy Object Path Tracking",
                "Enables the EditorHelper.HierarchyObjectPathChanged event " +
                "allowing scripts to know when an object has moved to a " +
                "different location in the hierarchy. Path change detection " +
                "is performed as a parallel operation.");
            EnableHierarchyObjectPathTracking = EditorGUILayout.Toggle(
                enable_hierarchy_object_path_tracking_label,
                EnableHierarchyObjectPathTracking);
            var show_parallel_tasks_message_label = new GUIContent(
                "Show Parallel Tasks Message",
                "Shows a non-modal progress indicator when parallel " +
                "tasks are taking a noticeable amount of time to complete.");
            ShowParallelTasksMessage = EditorGUILayout.Toggle(
                show_parallel_tasks_message_label,
                ShowParallelTasksMessage);

            GUILayout.Label("Debug Values");

            using (new GUILayout.VerticalScope(_DefaultSkin.box))
            {
                DrawLabeledReadout(
                    "MemberAttributeCacheSize",
                    EditorUtilities.MemberAttributeCacheSize);
                DrawLabeledReadout(
                    "AutoPropertyMemberValueHashCacheSize",
                    EditorUtilities.AutoPropertyMemberValueHashCacheSize);
                DrawLabeledReadout(
                    "StickyPropertyMemberValueHashCacheSize",
                    EditorUtilities.StickyPropertyMemberValueHashCacheSize);
            }
        }
    }
}

#endif
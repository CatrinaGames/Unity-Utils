using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializableGuid))]
public class SerializableGuidDrawer : PropertyDrawer {
    private const float ButtonWidth = 28f;

    static readonly string[] GuidParts = { "Part1", "Part2", "Part3", "Part4" };

    static SerializedProperty[] GetGuidParts(SerializedProperty property) {
        var values = new SerializedProperty[GuidParts.Length];
        for (int i = 0; i < GuidParts.Length; i++) {
            values[i] = property.FindPropertyRelative(GuidParts[i]);
        }
        return values;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        Rect contentPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        float totalButtonsWidth = ButtonWidth * 3;

        Rect textRect = new Rect(contentPosition.x, contentPosition.y, contentPosition.width - totalButtonsWidth - 2, contentPosition.height);

        Rect copyRect = new Rect(contentPosition.x + contentPosition.width - (ButtonWidth * 3), contentPosition.y, ButtonWidth, contentPosition.height);
        Rect regenRect = new Rect(contentPosition.x + contentPosition.width - (ButtonWidth * 2), contentPosition.y, ButtonWidth, contentPosition.height);
        Rect resetRect = new Rect(contentPosition.x + contentPosition.width - ButtonWidth, contentPosition.y, ButtonWidth, contentPosition.height);

        string guidString = "GUID Not Initialized";
        if (GetGuidParts(property).All(x => x != null)) {
            guidString = BuildGuidString(GetGuidParts(property));
        }
        EditorGUI.SelectableLabel(textRect, guidString, EditorStyles.textField);

        GUIContent copyIcon = GetIcon("TreeEditor.Duplicate", "Copy GUID to Clipboard");
        if (GUI.Button(copyRect, copyIcon, EditorStyles.miniButtonLeft)) {
            CopyGuid(property);
        }

        GUIContent regenIcon = GetIcon("Refresh", "Regenerate GUID");
        if (GUI.Button(regenRect, regenIcon, EditorStyles.miniButtonMid)) {
            RegenerateGuid(property);
        }

        GUIContent resetIcon = GetIcon("TreeEditor.Trash", "Reset GUID to Empty");
        if (GUI.Button(resetRect, resetIcon, EditorStyles.miniButtonRight)) {
            ResetGuid(property);
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    // --- Icons Helper ---
    GUIContent GetIcon(string iconName, string tooltip) {
        GUIContent content = EditorGUIUtility.IconContent(iconName);
        content.tooltip = tooltip;
        return content;
    }

    void CopyGuid(SerializedProperty property) {
        if (GetGuidParts(property).Any(x => x == null)) return;
        string guid = BuildGuidString(GetGuidParts(property));
        EditorGUIUtility.systemCopyBuffer = guid;
        Debug.Log($"GUID copied to clipboard: {guid}");
    }

    void ResetGuid(SerializedProperty property) {
        if (!EditorUtility.DisplayDialog("Reset GUID", "Are you sure you want to reset the GUID to empty?", "Yes", "No")) return;
        foreach (var part in GetGuidParts(property)) part.uintValue = 0;
        property.serializedObject.ApplyModifiedProperties();
    }

    void RegenerateGuid(SerializedProperty property) {
        if (!EditorUtility.DisplayDialog("Regenerate GUID", "Are you sure you want to regenerate the GUID?", "Yes", "No")) return;
        byte[] bytes = Guid.NewGuid().ToByteArray();
        SerializedProperty[] guidParts = GetGuidParts(property);
        for (int i = 0; i < GuidParts.Length; i++) guidParts[i].uintValue = BitConverter.ToUInt32(bytes, i * 4);
        property.serializedObject.ApplyModifiedProperties();
    }

    static string BuildGuidString(SerializedProperty[] guidParts) {
        return new StringBuilder()
            .AppendFormat("{0:X8}", guidParts[0].uintValue)
            .AppendFormat("{0:X8}", guidParts[1].uintValue)
            .AppendFormat("{0:X8}", guidParts[2].uintValue)
            .AppendFormat("{0:X8}", guidParts[3].uintValue)
            .ToString();
    }
}

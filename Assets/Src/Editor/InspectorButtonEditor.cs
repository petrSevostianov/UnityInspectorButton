using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
#nullable enable
public abstract class ButtonDrawerBase : PropertyDrawer {
    protected abstract bool ValidateMethod(MethodInfo method);

    protected abstract void InvokeMethod(object target, MethodInfo method);

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUIUtility.singleLineHeight;
    }

    private Action? delayedAction;
    public void invokeDelayedAction() {
        delayedAction?.Invoke();
        delayedAction = null;

    }

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
        SerializedObject O = (prop.GetType().GetField("m_SerializedObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(prop) as SerializedObject)!;
        var TargetObjects = O.targetObjects;

        string MethodName = prop.name;
        char[] charsToTrim = { '_' };
        MethodName = MethodName.Trim(charsToTrim);
        MethodInfo TargetMethod = TargetObjects[0].GetType().GetMethods()
            .Where(ValidateMethod)
            .FirstOrDefault(x => x.Name == MethodName);

        if (TargetMethod == null) {
            GUI.color = Color.red;
            GUI.Label(pos, "Method " + MethodName + " not found.");
            GUI.color = Color.white;
            return;
        }

        if (GUI.Button(pos, ObjectNames.NicifyVariableName(MethodName))) {
            delayedAction = () => {
                foreach (var o in TargetObjects) {
                    InvokeMethod(o, TargetMethod);
                }
            };
            EditorApplication.delayCall += invokeDelayedAction;
        }
    }
}

[CustomPropertyDrawer(typeof(InspectorButton))]
public class InspectorButtonDrawer : ButtonDrawerBase {
    protected override bool ValidateMethod(MethodInfo method) {
        return method.ReturnType == typeof(void) && method.GetParameters().Length == 0;
    }

    protected override void InvokeMethod(object target, MethodInfo method) {
        method.Invoke(target, null);
    }
}


[AttributeUsage(AttributeTargets.Method)]
public class InspectorFileDialogButtonInfoAttribute : Attribute {
    public enum FileDialogType {
        Open,
        Save,

    }

    public FileDialogType DialogType { get; } = FileDialogType.Open;
    public string? Title { get; }
    public string Extension { get; }
    public InspectorFileDialogButtonInfoAttribute(string extension, string? title = null, FileDialogType dialogType = FileDialogType.Open) {
        DialogType = dialogType;
        Extension = extension;
        Title = title;
    }
}


[CustomPropertyDrawer(typeof(InspectorFileSelectButton))]
public class InspectorFileDialogButtonDrawer : ButtonDrawerBase {
    protected override bool ValidateMethod(MethodInfo method) {
        return method.ReturnType == typeof(void) &&
               method.GetParameters().Length == 1 &&
               method.GetParameters().FirstOrDefault()?.ParameterType == typeof(string);
    }

    protected override void InvokeMethod(object target, MethodInfo method) {
        var extensionAttribute = method.GetCustomAttribute<InspectorFileDialogButtonInfoAttribute>(false);
        string? extension = extensionAttribute?.Extension;
        string? title = extensionAttribute?.Title;
        var dialogType = extensionAttribute?.DialogType ?? InspectorFileDialogButtonInfoAttribute.FileDialogType.Open;
        if (title == null) {
            title = ObjectNames.NicifyVariableName(method.Name);
        }

        string path = "";
        switch (dialogType) {
            case InspectorFileDialogButtonInfoAttribute.FileDialogType.Open:
                path = EditorUtility.OpenFilePanel(title, "", extension);
                break;
            case InspectorFileDialogButtonInfoAttribute.FileDialogType.Save:
                path = EditorUtility.SaveFilePanel(title, "", "", extension);
                break;
        }

        if (!string.IsNullOrEmpty(path)) {
            method.Invoke(target, new object[] { path });
        }
    }
}


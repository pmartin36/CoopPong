﻿//using UnityEngine;
//using UnityEditor;
//using System.Collections.Generic;

//[CustomPropertyDrawer(typeof(ButtonEffectedList))]
//public class ButtonEffectedListDrawer : PropertyDrawer {
//	private bool Foldout = false;
//	int indentAmount = 15;

//	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
//		EditorGUI.BeginProperty(position, label, property);

//		Rect propRect = new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight);
//		Foldout = EditorGUI.Foldout(propRect, Foldout, "Effected", true);
//		if (Foldout) {
//			propRect.position += new Vector2(0, EditorGUIUtility.singleLineHeight);
//			propRect.xMin += indentAmount;
//			EditorGUIUtility.labelWidth -= indentAmount;

//			var size = property.FindPropertyRelative("Count");
//			EditorGUI.PropertyField(propRect, size);

//			property = property.FindPropertyRelative("Effected");

//			//	//ButtonEffectedList bel = property.FindPropertyRelative("Effected");
//			//	for (int i = 0; i < size.intValue; i++) {
//			//		//propRect.position += new Vector2(0, EditorGUIUtility.singleLineHeight);
//			//		//EditorGUI.ObjectField(propRect, $"Element {i}", ,typeof(IButtonEffected));
//			//	}
//		}

//		EditorGUI.EndProperty();
//	}

//	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
//		// return 2 * EditorGUIUtility.singleLineHeight;
//		var p = property.FindPropertyRelative("Count");
//		return (p.intValue + 2) * EditorGUIUtility.singleLineHeight;
//	}
//}
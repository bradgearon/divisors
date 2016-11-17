using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

namespace I2.Loc
{
	#if !UNITY_5_0 && !UNITY_5_1

    [CustomEditor(typeof(LocalizeDropdown))]
    public class LocalizeDropdownInspector : Editor
	{
		private ReorderableList mList;

        private List<string> terms;

		private ReorderableList getList(SerializedObject serObject)
		{
			if (mList == null) {
                mList = new ReorderableList (serObject, serObject.FindProperty ("_Terms"), true, true, true, true);
				mList.drawElementCallback = drawElementCallback;
				mList.drawHeaderCallback = drawHeaderCallback;
                mList.onAddCallback = addElementCallback;
                mList.onRemoveCallback = removeElementCallback;
			} 
			else
			{
                mList.serializedProperty = serObject.FindProperty ("_Terms");
			}
			return mList;
		}

        private void addElementCallback( ReorderableList list )
        {
            serializedObject.ApplyModifiedProperties();

            var objParams = (target as LocalizeDropdown);
            objParams._Terms.Add(string.Empty);

            list.index = objParams._Terms.Count - 1;

            serializedObject.Update();
        }

        private void removeElementCallback( ReorderableList list )
        {
            if (list.index < 0)
                return;
            serializedObject.ApplyModifiedProperties();

            var objParams = (target as LocalizeDropdown);
            objParams._Terms.RemoveAt(list.index);

            serializedObject.Update();
        }

		private void drawHeaderCallback(Rect rect)
		{
            GUI.Label(rect, "Terms:");
		}

		private void drawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
		{
			var serializedElement = mList.serializedProperty.GetArrayElementAtIndex (index);

            EditorGUI.BeginChangeCheck ();

            var newIndex = EditorGUI.Popup (rect, terms.IndexOf (serializedElement.stringValue), terms.ToArray());

            if (EditorGUI.EndChangeCheck ())
                serializedElement.stringValue = (newIndex < 0 || newIndex == terms.Count - 1) ? string.Empty : terms [newIndex];
		}

        void OnEnable()
        {
            mList = getList(serializedObject);
        }

        public override void OnInspectorGUI()
		{
            serializedObject.UpdateIfDirtyOrScript();

            terms =  LocalizationManager.GetTermsList ();
            terms.Sort(System.StringComparer.OrdinalIgnoreCase);
            terms.Add ("<inferred from text>");

            GUI.backgroundColor = Color.Lerp (Color.black, Color.gray, 1);
            GUILayout.BeginVertical(LocalizeInspector.GUIStyle_Background);
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("Localize DropDown", LocalizeInspector.GUIStyle_Header))
            {
                Application.OpenURL(LocalizeInspector.HelpURL_Documentation);
            }


            GUILayout.Space(5);
            mList.DoLayoutList();

            GUILayout.Space (10);

            GUILayout.BeginHorizontal();
                if (GUILayout.Button("v"+LocalizationManager.GetVersion(), EditorStyles.miniLabel))
                    Application.OpenURL(LocalizeInspector.HelpURL_ReleaseNotes);

                GUILayout.FlexibleSpace ();
                if (GUILayout.Button("Tutorials", EditorStyles.miniLabel))
                    Application.OpenURL(LocalizeInspector.HelpURL_Tutorials);

                GUILayout.Space(10);

                if (GUILayout.Button("Ask a Question", EditorStyles.miniLabel))
                    Application.OpenURL(LocalizeInspector.HelpURL_forum);

                GUILayout.Space(10);

                if (GUILayout.Button("Documentation", EditorStyles.miniLabel))
                    Application.OpenURL(LocalizeInspector.HelpURL_Documentation);
            GUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 0;


            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
            terms = null;
		}
	}
	#endif
}
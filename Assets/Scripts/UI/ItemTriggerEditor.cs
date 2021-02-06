#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditor.EventSystems;


[CustomEditor(typeof(ItemButtonTrigger))]
public class ItemTriggerEditor : NamedButtonTriggerEditor
{
}
#endif



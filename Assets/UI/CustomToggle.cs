using UnityEditor;
using UnityEditor.UI;
using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using System;



public class CustomToggle : Selectable, IPointerClickHandler {

	[Serializable] public class ToggleEvent : UnityEvent<CustomToggle> {}
	[Serializable] public class ToggleValue : UnityEvent<bool> {}

	// Data

	[SerializeField] Image m_ImageFalse;
	[SerializeField] Image m_ImageTrue;
	[SerializeField] TextMeshProUGUI m_TextFalse;
	[SerializeField] TextMeshProUGUI m_TextTrue;
	[SerializeField] TextMeshProUGUI m_Label;
	[SerializeField] string m_description;

	[SerializeField] bool m_Value = false;

	[SerializeField] ToggleEvent m_OnUpdateNeeded = new ToggleEvent();
	[SerializeField] ToggleValue m_OnValueChanged = new ToggleValue();



	// Editor

	[CustomEditor(typeof(CustomToggle))]
	public class CustomToggleEditor : SelectableEditor {
		SerializedProperty imageFalseProp;
		SerializedProperty imageTrueProp;
		SerializedProperty textFalseProp;
		SerializedProperty textTrueProp;
		SerializedProperty labelProp;
		SerializedProperty descriptionProp;

		SerializedProperty valueProp;

		SerializedProperty onUpdateNeededProp;
		SerializedProperty onValueChangedProp;

		protected override void OnEnable() {
			base.OnEnable();
			imageFalseProp     = serializedObject.FindProperty("m_ImageFalse");
			imageTrueProp      = serializedObject.FindProperty("m_ImageTrue");
			textFalseProp      = serializedObject.FindProperty("m_TextFalse");
			textTrueProp       = serializedObject.FindProperty("m_TextTrue");
			labelProp          = serializedObject.FindProperty("m_Label");
			descriptionProp    = serializedObject.FindProperty("m_description");

			valueProp          = serializedObject.FindProperty("m_Value");

			onUpdateNeededProp = serializedObject.FindProperty("m_OnUpdateNeeded");
			onValueChangedProp = serializedObject.FindProperty("m_OnValueChanged");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(imageFalseProp);
			EditorGUILayout.PropertyField(imageTrueProp);
			EditorGUILayout.PropertyField(textFalseProp);
			EditorGUILayout.PropertyField(textTrueProp);
			EditorGUILayout.PropertyField(labelProp);
			EditorGUILayout.PropertyField(descriptionProp);
			
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(valueProp);

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(onUpdateNeededProp);
			EditorGUILayout.PropertyField(onValueChangedProp);

			serializedObject.ApplyModifiedProperties();
		}
	}



	// Properties

	public bool value {
		get => m_Value;
		set {
			if (m_Value == value) return;
			m_Value = value;
			Refresh();
		}
	}

	public ToggleEvent onUpdateNeeded => m_OnUpdateNeeded;
	public ToggleValue onValueChanged => m_OnValueChanged;



	// Methods

	public RectTransform rectTransform;
	ScrollRect scrollRect;

	void Refresh() {
		if (m_ImageFalse) m_ImageFalse.enabled = !m_Value;
		if (m_TextFalse ) m_TextFalse .enabled = !m_Value;
		if (m_ImageTrue ) m_ImageTrue .enabled =  m_Value;
		if (m_TextTrue  ) m_TextTrue  .enabled =  m_Value;
		onValueChanged?.Invoke(m_Value);
	}

	protected override void Start() {
		rectTransform = transform as RectTransform;
		scrollRect = GetComponentInParent<ScrollRect>();
		base.Start();
		Refresh();
	}

	public void OnPointerClick(PointerEventData eventData) {
		if (!interactable) return;
		value = !value;
		onValueChanged?.Invoke(value);
	}

	public override void OnMove(AxisEventData eventData) {
		bool flag = false;
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:
			case MoveDirection.Right: value = !value; flag = true; break;
		}
		if (flag) {
			DoStateTransition(SelectionState.Pressed, false);
			onValueChanged?.Invoke(value);
		}
		else base.OnMove(eventData);
	}

	public override void OnSelect(BaseEventData eventData) {
		base.OnSelect(eventData);
		if (eventData is AxisEventData && scrollRect) {
			float y = rectTransform.anchoredPosition.y - rectTransform.rect.height / 2;
			Vector2 anchoredPosition = scrollRect.content.anchoredPosition;
			anchoredPosition.y = -scrollRect.viewport.rect.height / 2 - y;
			scrollRect.content.anchoredPosition = anchoredPosition;
		}
	}
}

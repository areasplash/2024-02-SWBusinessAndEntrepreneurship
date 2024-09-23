using UnityEditor;
using UnityEditor.UI;
using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using System;



public class CustomButton : Selectable, IPointerClickHandler {

	[Serializable] public class ButtonEvent : UnityEvent<CustomButton> {}
	[Serializable] public class ButtonValue : UnityEvent {}



	// Data

	[SerializeField] TextMeshProUGUI m_Text;
	[SerializeField] TextMeshProUGUI m_Label;
	[SerializeField] string m_description;

	[SerializeField] ButtonEvent m_OnUpdateNeeded = new ButtonEvent();
	[SerializeField] ButtonValue m_OnValueChanged = new ButtonValue();



	// Editor

	[CustomEditor(typeof(CustomButton))]
	public class CustomButtonEditor : SelectableEditor {
		SerializedProperty textProp;
		SerializedProperty labelProp;
		SerializedProperty descriptionProp;

		SerializedProperty onUpdateNeededProp;
		SerializedProperty onValueChangedProp;

		protected override void OnEnable() {
			base.OnEnable();
			textProp           = serializedObject.FindProperty("m_Text");
			labelProp          = serializedObject.FindProperty("m_Label");
			descriptionProp    = serializedObject.FindProperty("m_description");
			
			onUpdateNeededProp = serializedObject.FindProperty("m_OnUpdateNeeded");
			onValueChangedProp = serializedObject.FindProperty("m_OnValueChanged");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(textProp);
			EditorGUILayout.PropertyField(labelProp);
			EditorGUILayout.PropertyField(descriptionProp);
			
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(onUpdateNeededProp);
			EditorGUILayout.PropertyField(onValueChangedProp);

			serializedObject.ApplyModifiedProperties();
		}
	}



	// Properties

	public string text {
		get => m_Text ? m_Text.text : "";
		set {
			if (m_Text) m_Text.text = value;
		}
	}

	public ButtonEvent onUpdateNeeded => m_OnUpdateNeeded;
	public ButtonValue onValueChanged => m_OnValueChanged;



	// Methods

	public RectTransform rectTransform;
	ScrollRect scrollRect;

	void Refresh() {
		onUpdateNeeded?.Invoke(this);
	}

	protected override void Start() {
		rectTransform = transform as RectTransform;
		scrollRect = GetComponentInParent<ScrollRect>();
		base.Start();
		Refresh();
	}

	public void OnPointerClick(PointerEventData eventData) {
		if (!interactable) return;
		onValueChanged?.Invoke();
	}

	public override void OnMove(AxisEventData eventData) {
		bool flag = false;
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:
			case MoveDirection.Right: flag = true; break;
		}
		if (flag) {
			DoStateTransition(SelectionState.Pressed, false);
			onValueChanged?.Invoke();
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

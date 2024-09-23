using UnityEditor;
using UnityEditor.UI;
using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using System;



public class CustomStepper : Selectable, IPointerClickHandler {

	[Serializable] public class StepperEvent : UnityEvent<CustomStepper> {}
	[Serializable] public class StepperValue : UnityEvent<int> {}



	// Data

	[SerializeField] Image m_LeftArrow;
	[SerializeField] Image m_RightArrow;
	[SerializeField] TextMeshProUGUI m_Text;
	[SerializeField] TextMeshProUGUI m_Label;
	[SerializeField] string m_description;

	[SerializeField] int  m_Length = 1;
	[SerializeField] int  m_Value  = 0;
	[SerializeField] bool m_Loop   = false;

	[SerializeField] StepperEvent m_OnUpdateNeeded = new StepperEvent();
	[SerializeField] StepperValue m_OnValueChanged = new StepperValue();



	// Editor

	[CustomEditor(typeof(CustomStepper))]
	public class CustomStepperEditor : SelectableEditor {
		SerializedProperty leftArrowProp;
		SerializedProperty rightArrowProp;
		SerializedProperty textProp;
		SerializedProperty labelProp;
		SerializedProperty descriptionProp;

		SerializedProperty lengthProp;
		SerializedProperty valueProp;
		SerializedProperty loopProp;

		SerializedProperty onUpdateNeededProp;
		SerializedProperty onValueChangedProp;

		protected override void OnEnable() {
			base.OnEnable();
			leftArrowProp      = serializedObject.FindProperty("m_LeftArrow");
			rightArrowProp     = serializedObject.FindProperty("m_RightArrow");
			textProp           = serializedObject.FindProperty("m_Text");
			labelProp          = serializedObject.FindProperty("m_Label");
			descriptionProp    = serializedObject.FindProperty("m_description");

			lengthProp         = serializedObject.FindProperty("m_Length");
			valueProp          = serializedObject.FindProperty("m_Value");
			loopProp		   = serializedObject.FindProperty("m_Loop");

			onUpdateNeededProp = serializedObject.FindProperty("m_OnUpdateNeeded");
			onValueChangedProp = serializedObject.FindProperty("m_OnValueChanged");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(leftArrowProp);
			EditorGUILayout.PropertyField(rightArrowProp);
			EditorGUILayout.PropertyField(textProp);
			EditorGUILayout.PropertyField(labelProp);
			EditorGUILayout.PropertyField(descriptionProp);

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(lengthProp);
			EditorGUILayout.IntSlider(valueProp, 0, lengthProp.intValue - 1);
			EditorGUILayout.PropertyField(loopProp);
			
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

	public int length {
		get => m_Length;
		set {
			if (m_Length == value) return;
			m_Length = Mathf.Max(1, value);
			Refresh();
		}
	}

	public int value {
		get => m_Value;
		set {
			if (m_Value == value) return;
			if (loop) {
				if (value < 0) value = length - 1;
				if (length - 1 < value) value = 0;
			}
			m_Value = Mathf.Clamp(value, 0, length - 1);
			Refresh();
		}
	}

	public bool loop {
		get => m_Loop;
		set {
			if (m_Loop == value) return;
			m_Loop = value;
			Refresh();
		}
	}

	public StepperEvent onUpdateNeeded => m_OnUpdateNeeded;
	public StepperValue onValueChanged => m_OnValueChanged;



	// Methods

	public RectTransform rectTransform;
	ScrollRect scrollRect;

	float bodyWidth => rectTransform.rect.width;

	void Refresh() {
		if (m_LeftArrow ) m_LeftArrow .enabled = loop || 0 < value;
		if (m_RightArrow) m_RightArrow.enabled = loop || value < length - 1;
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
		RectTransform rectTransform = transform as RectTransform;
		Vector2 point = rectTransform.InverseTransformPoint(eventData.position);
		bool flag = (0 <= point.x) && (point.x < bodyWidth / 3);
		value += flag ? -1 : 1;
		onValueChanged?.Invoke(value);
	}

	public override void OnMove(AxisEventData eventData) {
		bool flag = false;
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:  value--; flag = true; break;
			case MoveDirection.Right: value++; flag = true; break;
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

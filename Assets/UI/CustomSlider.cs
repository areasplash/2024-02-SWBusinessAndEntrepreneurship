using UnityEditor;
using UnityEditor.UI;
using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using System;



public class CustomSlider : Selectable, IPointerClickHandler, IDragHandler {

	[Serializable] public class SliderEvent : UnityEvent<CustomSlider> {}
	[Serializable] public class SliderValue : UnityEvent<float> {}



	// Data

	[SerializeField] Image m_Fill;
	[SerializeField] Image m_Handle;
	[SerializeField] TextMeshProUGUI m_Text;
	[SerializeField] TextMeshProUGUI m_Label;
	[SerializeField] string m_description;
	[SerializeField] string m_Format = "{0:P1}";

	[SerializeField] float m_ValueMin = 0;
	[SerializeField] float m_ValueMax = 1;
	[SerializeField] float m_Value    = 1;
	[SerializeField] float m_Step     = 0.10f;
	[SerializeField] float m_FineStep = 0.02f;

	[SerializeField] SliderEvent m_OnUpdateNeeded = new SliderEvent();
	[SerializeField] SliderValue m_OnValueChanged = new SliderValue();



	// Editor

	[CustomEditor(typeof(CustomSlider))]
	public class CustomSliderEditor : SelectableEditor {
		SerializedProperty fillProp;
		SerializedProperty handleProp;
		SerializedProperty textProp;
		SerializedProperty labelProp;
		SerializedProperty decsriptionProp;
		SerializedProperty formatProp;

		SerializedProperty valueMinProp;
		SerializedProperty valueMaxProp;
		SerializedProperty valueProp;
		SerializedProperty stepProp;
		SerializedProperty fineStepProp;
		
		SerializedProperty onUpdateNeededProp;
		SerializedProperty onValueChangedProp;

		protected override void OnEnable() {
			base.OnEnable();
			fillProp           = serializedObject.FindProperty("m_Fill");
			handleProp         = serializedObject.FindProperty("m_Handle");
			textProp           = serializedObject.FindProperty("m_Text");
			labelProp          = serializedObject.FindProperty("m_Label");
			decsriptionProp    = serializedObject.FindProperty("m_description");
			formatProp         = serializedObject.FindProperty("m_Format");

			valueMinProp       = serializedObject.FindProperty("m_ValueMin");
			valueMaxProp       = serializedObject.FindProperty("m_ValueMax");
			valueProp          = serializedObject.FindProperty("m_Value");
			stepProp           = serializedObject.FindProperty("m_Step");
			fineStepProp       = serializedObject.FindProperty("m_FineStep");
			
			onUpdateNeededProp = serializedObject.FindProperty("m_OnUpdateNeeded");
			onValueChangedProp = serializedObject.FindProperty("m_OnValueChanged");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			serializedObject.Update();

			float  valueMin = valueMinProp.floatValue;
			float  valueMax = valueMaxProp.floatValue;
			float  value    = valueProp.floatValue;
			float  step     = stepProp.floatValue;
			float  fineStep = fineStepProp.floatValue;
			string format   = formatProp.stringValue;

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(fillProp);
			EditorGUILayout.PropertyField(handleProp);
			EditorGUILayout.PropertyField(textProp);
			EditorGUILayout.PropertyField(labelProp);
			EditorGUILayout.PropertyField(decsriptionProp);
			EditorGUILayout.PropertyField(formatProp);
			EditorGUILayout.TextField(" ", "{0} = value, {1} = valueMin, {2} = valueMax");
			EditorGUILayout.TextField(" ", "" + string.Format(format, value, valueMin, valueMax));

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(valueMinProp);
			EditorGUILayout.PropertyField(valueMaxProp);
			EditorGUILayout.Slider(valueProp, valueMin, valueMax);
			EditorGUILayout.Slider(stepProp, 0, valueMax - valueMin);
			EditorGUILayout.Slider(fineStepProp, 0, step);

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(onUpdateNeededProp);
			EditorGUILayout.PropertyField(onValueChangedProp);

			serializedObject.ApplyModifiedProperties();
		}
	}



	// Properties

	public float valueMin {
		get => m_ValueMin;
		set {
			if (m_ValueMin == value) return;
			m_ValueMin = Mathf.Min(value, valueMax);
			Refresh();
		}
	}

	public float valueMax {
		get => m_ValueMax;
		set {
			if (m_ValueMax == value) return;
			m_ValueMax = Mathf.Max(value, valueMin);
			Refresh();
		}
	}

	public float value {
		get => m_Value;
		set {
			if (m_Value == value) return;
			m_Value = Mathf.Clamp(value, valueMin, valueMax);
			Refresh();
		}
	}

	public float step {
		get => m_Step;
		set => m_Step = Mathf.Clamp(value, 0, valueMax - valueMin);
	}

	public float fineStep {
		get => m_FineStep;
		set => m_FineStep = Mathf.Clamp(value, 0, step);
	}

	public SliderEvent onUpdateNeeded => m_OnUpdateNeeded;
	public SliderValue onValueChanged => m_OnValueChanged;



	// Methods

	public RectTransform rectTransform;
	ScrollRect scrollRect;

	float bodyWidth => rectTransform.rect.width;
	float handleWidth => m_Handle.rectTransform.rect.width;
	int x => Mathf.RoundToInt((value - valueMin) / (valueMax - valueMin) * (bodyWidth - handleWidth));
	bool  ctrl => Input.GetKey(KeyCode.LeftControl);

	void Refresh() {
		if (m_Fill) {
			Vector2 sizeDelta = new Vector2(handleWidth / 2 + x, m_Fill.rectTransform.sizeDelta.y);
			m_Fill.rectTransform.sizeDelta = sizeDelta;
		}
		if (m_Handle) {
			Vector2 anchoredPosition = new Vector2(x, m_Handle.rectTransform.anchoredPosition.y);
			m_Handle.rectTransform.anchoredPosition = anchoredPosition;
		}
		if (m_Text) m_Text.text = string.Format(m_Format, value, valueMin, valueMax);
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
		Vector2 point = rectTransform.InverseTransformPoint(eventData.position);
		if (point.x < x) value -= ctrl ? fineStep : step;
		if (x < point.x) value += ctrl ? fineStep : step;
		onValueChanged?.Invoke(value);
	}

	public void OnDrag(PointerEventData eventData) {
		if (!interactable) return;
		Vector2 point = rectTransform.InverseTransformPoint(eventData.position);
		value = Mathf.Lerp(valueMin, valueMax, Mathf.InverseLerp(0, bodyWidth, point.x));
		onValueChanged?.Invoke(value);
	}

	public override void OnMove(AxisEventData eventData) {
		bool flag = false;
		if (interactable) switch (eventData.moveDir) {
			case MoveDirection.Left:  value -= ctrl ? fineStep : step; flag = true; break;
			case MoveDirection.Right: value += ctrl ? fineStep : step; flag = true; break;
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

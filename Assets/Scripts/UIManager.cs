using UnityEditor;
using static UnityEditor.EditorGUILayout;

using UnityEngine;
using UnityEngine.Localization.Settings;

using System.Collections.Generic;



public class UIManager : MonoBehaviour {

	// Inspector GUI

	[CustomEditor(typeof(UIManager))]
	public class UIManagerEditor : Editor {
		public override void OnInspectorGUI() {
			DrawDefaultInspector();
			UIManager i = (UIManager)target;

			Space();
			LabelField("Canvas", EditorStyles.boldLabel);
			i.mainmenu = (Canvas)ObjectField("Main Menu", i.mainmenu, typeof(Canvas), true);
			i.game     = (Canvas)ObjectField("Game     ", i.game,     typeof(Canvas), true);
			i.menu     = (Canvas)ObjectField("Menu     ", i.menu,     typeof(Canvas), true);
			i.settings = (Canvas)ObjectField("Settings ", i.settings, typeof(Canvas), true);

			Space();
			LabelField("Development", EditorStyles.boldLabel);
			i.skipTitle = Toggle("Skip Title", i.skipTitle);

			if (GUI.changed) EditorUtility.SetDirty(i);
		}
	}


    
	// Instance

	[SerializeField, HideInInspector] UIManager instance;
	public static UIManager Instance { get; private set; }

	void Awake() {
		if (Instance == null) {
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else Destroy(gameObject);
	}



	// Data

	[SerializeField, HideInInspector] Canvas mainmenu;
	[SerializeField, HideInInspector] Canvas game;
	[SerializeField, HideInInspector] Canvas menu;
	[SerializeField, HideInInspector] Canvas settings;

	[SerializeField, HideInInspector] bool skipTitle;



	// Properties



	// Methods

	void Start() {
		mainmenu.gameObject.SetActive(!skipTitle);
		game    .gameObject.SetActive( skipTitle);
		menu    .gameObject.SetActive(false);
		settings.gameObject.SetActive(false);
	}



	public void SetSceneMainMenu() {
		mainmenu.gameObject.SetActive(true );
		game    .gameObject.SetActive(false);
		menu    .gameObject.SetActive(false);
		settings.gameObject.SetActive(false);
	}

	public void SetSceneGame() {
		mainmenu.gameObject.SetActive(false);
		game    .gameObject.SetActive(true );
		menu    .gameObject.SetActive(false);
		settings.gameObject.SetActive(false);
	}

	public void OpenMenu() {
		menu    .gameObject.SetActive(true );
	}

	public void OpenSettings() {
		settings.gameObject.SetActive(true );
	}
	
	void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) Back();
	}

	public void Back() {
		if      (settings.gameObject.activeSelf) settings.gameObject.SetActive(false);
		else if (menu    .gameObject.activeSelf) menu    .gameObject.SetActive(false);
		else if (game    .gameObject.activeSelf) menu    .gameObject.SetActive(true );
		else if (mainmenu.gameObject.activeSelf) {
			#if UNITY_EDITOR
				EditorApplication.isPlaying = false;
			#else
				Application.Quit();
			#endif
		}
	}




	public void UpdateLanguage(CustomStepper stepper) {
		stepper.length = LocalizationSettings.AvailableLocales.Locales.Count;
		stepper.text = LocalizationSettings.AvailableLocales.Locales[stepper.value].name;
	}

	public void SetLanguage(int value) {
		LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[value];
	}

	public void SetFullScreen(bool value) {
		Screen.fullScreen = value;
	}

	List<Vector2Int> screenResolutionList = new List<Vector2Int> {
		new Vector2Int(1280,  720),
		new Vector2Int(1920, 1080),
		new Vector2Int(2560, 1440),
		new Vector2Int(3840, 2160),
	};

	public void UpdateScreenResolution(CustomStepper stepper) {
		stepper.length = screenResolutionList.Count;
		int x = screenResolutionList[stepper.value].x;
		int y = screenResolutionList[stepper.value].y;
		stepper.text = x + "x" + y;
	}

	public void SetScreenResolution(int value) {
		int x = screenResolutionList[value].x;
		int y = screenResolutionList[value].y;
		Screen.SetResolution(x, y, Screen.fullScreen);
	}

	public static void FadeOut(float speed = 0.5f, float duration = 1.0f) {
		
	}
}

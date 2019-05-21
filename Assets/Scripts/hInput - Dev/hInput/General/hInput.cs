﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hInput : MonoBehaviour {
	//FEATURES
	// hAbstractInput : store lasFramePressed value, make justPressed non frame dependent
	// separer stickInput et stickVirtualButton

	//TESTING
	// Test build all on startup

	//SHIPPING
	// update doc
	// Proofread all
	// add tooltips to hAbstractStick & hAbstractInput properties

	// --------------------
	// SETTINGS
	// --------------------

	[SerializeField]
	[Tooltip ("If enabled, hInput will start tracking every control of every gamepad from startup. "
	+"Otherwise, each control will only start being registered the first time you ask for it.")]
	private bool _buildAllOnStartUp = false;
	public static bool buildAllOnStartUp {
		get { return instance._buildAllOnStartUp; }
		set { instance._buildAllOnStartUp = value; }
	}

	[SerializeField]
	[Range(0,1)]
	[Tooltip("The distance from the center beyond which stick and trigger inputs start being registered.")]
	private float _deadZone = 0.2f;
	public static float deadZone { 
		get { return instance._deadZone; } 
		set { instance._deadZone = value; } 
	}

	[SerializeField]
	[Range(0,1)]
	[Tooltip("The distance from the center beyond which stick and trigger inputs are considered pushed or activated.")]
	private float _triggerZone = 0.5f;
	public static float triggerZone { 
		get { return instance._triggerZone; } 
		set { instance._triggerZone = value; }  
	}

	[SerializeField]
	[Range(45,90)]
	[Tooltip("The size of the angle that defines a stick direction.\n\n"+
	"Note : if it is higher than 45 degrees, directions like (up) and (leftUp) will overlap. " 
	+"Likewise, if it is lower than 90 degrees, there will be a gap between directions like (up) and (left).")]
	private float _directionAngle = 90f;
	public static float directionAngle { 
		get { return instance._directionAngle; } 
		set { instance._directionAngle = value; }  
	}

	[SerializeField]
	[Range(0,2)]
	[Tooltip("The maximum duration between the start of two presses for them to be considered a double press.")]
	private float _doublePressDuration = 0.3f;
	public static float doublePressDuration { 
		get { return instance._doublePressDuration; } 
		set { instance._doublePressDuration = value; }  
	}

	[SerializeField]
	[Range(0,2)]
	[Tooltip("The minimum duration of a press for it to be considered a long press.")]
	private float _longPressDuration = 0.3f;
	public static float longPressDuration { 
		get { return instance._longPressDuration; } 
		set { instance._longPressDuration = value; }  
	}

	[SerializeField]
	[Tooltip("The camera on which the worldPosition property of hStick and hDPad should be calculated. If not set, hInput will try to find one on the scene.")]
	private Transform _worldCamera = null;
	public static Transform worldCamera { 
		get { 
			if (instance._worldCamera != null) return instance._worldCamera;
			else if (Camera.main != null) instance._worldCamera = Camera.main.transform;
			else if (GameObject.FindObjectOfType<Camera>() != null) instance._worldCamera = GameObject.FindObjectOfType<Camera>().transform;
			else { Debug.LogError ("hInput error : No camera found !"); return null; }
			return instance._worldCamera;
		} 
		set { instance._worldCamera = value; } 
	}


	// --------------------
	// ADDITIONAL SETTINGS
	// --------------------

	//By how much to increase diagonals (in %), because otherwise the max stick distance is sometimes less than 1.
	private float _diagonalIncrease = 0.01f;
	public static float diagonalIncrease { get { return instance._diagonalIncrease; } }

	//Maximum amount of gamepads supported by the game
	private float _maxGamepads = 4;
	public static float maxGamepads { get { return instance._maxGamepads; } }


	// --------------------
	// TIME
	// --------------------

	//By how much to increase deltaTime (in %)
	private float _deltaTimeEpsilon = 0.1f;
	public static float deltaTimeEpsilon { get { return instance._deltaTimeEpsilon; } }

	//We assume that next frame will be processed in less than this duration.
	public static float maxDeltaTime { get { return Time.deltaTime * (1+deltaTimeEpsilon); } }


	// --------------------
	// OPERATING SYSTEM
	// --------------------

	private string _os;
	public static string os { 
		get { 
			if (instance._os == null) {
				#if UNITY_EDITOR_WIN
					instance._os = "Windows";
				#elif UNITY_STANDALONE_WIN
					instance._os = "Windows";
				#elif UNITY_EDITOR_OSX
					instance._os = "Mac";
				#elif UNITY_STANDALONE_OSX
					instance._os = "Mac";
				#elif UNITY_EDITOR_LINUX
					instance._os = "Linux";
				#elif UNITY_STANDALONE_LINUX
					instance._os = "Linux";
				#else
					Debug.LogError("hInput Error : Unknown OS !");
				#endif
			}

			return instance._os;
		} 
	}


	// --------------------
	// SINGLETON PATTERN
	// --------------------

	private static hInput _instance;
	public static hInput instance { 
		get {
			if (_instance == null) {
				GameObject go = new GameObject();
				go.name = "hInput";
				_instance = go.AddComponent<hInput>();
			}
			
			return _instance;
		} 
	}

	private void Awake () {
		if (_instance == null) _instance = this;
		if (_instance != this) Destroy(this);
		DontDestroyOnLoad (this);

		if (buildAllOnStartUp) {
			anyGamepad.BuildAll();
			for (int i=0; i<maxGamepads; i++) gamepad[i].BuildAll();
		}
	}


	// --------------------
	// GAMEPADS
	// --------------------

	private hGamepad _anyGamepad;
	public static hGamepad anyGamepad { 
		get { 
			if (instance._anyGamepad == null) instance._anyGamepad = new hGamepad(os, -1); 
			return instance._anyGamepad; 
		}
	}

	private List<hGamepad> _gamepad;
	public static List<hGamepad> gamepad { 
		get {
			if (instance._gamepad == null) {
				instance._gamepad = new List<hGamepad>();
				for (int i=0; i<maxGamepads; i++) gamepad.Add(new hGamepad(os, i));
			}
			return instance._gamepad; 
		} 
	}


	// --------------------
	// UPDATE
	// --------------------

	private void Update () {
		anyGamepad.Update();
		for (int i=0; i<maxGamepads; i++) gamepad[i].Update ();
	}
}
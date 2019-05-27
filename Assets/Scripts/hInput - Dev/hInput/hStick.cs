﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hStick {
	// --------------------
	// NAME
	// --------------------

	private string _name;
	public string name { get { return _name; } }

	private string _fullName;
	public string fullName { get { return _fullName; } }

	private int _gamepadIndex;
	public int gamepadIndex { get { return _gamepadIndex; } }

	public hGamepad gamepad { 
		get { 
			if (gamepadIndex >= 0) return hInput.gamepad[gamepadIndex]; 
			else return hInput.anyGamepad;
		} 
	}

	private int _index;
	public int index { get { return _index; } }

	
	// --------------------
	// IMPLICIT CONVERSION
	// --------------------

	public static implicit operator Vector2 (hStick hStick) { return hStick.position; }


	// --------------------
	// CONSTRUCTORS
	// --------------------

	// For sticks
	public hStick (string name, hGamepad gamepad, int index) {
		this._name = name;
		this._gamepadIndex = gamepad.index;
		this._fullName = gamepad.fullName+"_"+name;
		this._index = index;

		horizontalAxis = new hAxis (fullName+"_Horizontal");
		verticalAxis = new hAxis (fullName+"_Vertical");
	}

	// For the D-pad
	public hStick (string name, hGamepad gamepad) {
		this._name = name;
		this._gamepadIndex = gamepad.index;
		this._fullName = gamepad.fullName+"_"+name;
		this._index = 2;

		horizontalAxis = new hAxis (fullName+"_Horizontal", fullName+"_Left", fullName+"_Right");
		verticalAxis = new hAxis (fullName+"_Vertical", fullName+"_Down", fullName+"_Up");
	}


	// --------------------
	// AXES
	// --------------------

	private hAxis horizontalAxis;
	private hAxis verticalAxis;


	// --------------------
	// DIRECTIONS
	// --------------------

	private hDirection _up;
	public hDirection up { 
		get {
			if (_up == null) _up = new hDirection ("Up", 90, this);
			return _up;
		} 
	}

	private hDirection _down;
	public hDirection down { 
		get {
			if (_down == null) _down = new hDirection ("Down", -90, this);
			return _down;
		} 
	}

	private hDirection _left;
	public hDirection left { 
		get {
			if (_left == null) _left = new hDirection ("Left", 180, this);
			return _left;
		} 
	}

	private hDirection _right;
	public hDirection right { 
		get {
			if (_right == null) _right = new hDirection ("Right", 0, this);
			return _right;
		} 
	}

	
	private hDirection _upLeft;
	public hDirection leftUp { get { return upLeft; } }
	public hDirection upLeft { 
		get {
			if (_upLeft == null) _upLeft = new hDirection ("UpLeft", 135, this);
			return _upLeft;
		} 
	}

	private hDirection _downLeft;
	public hDirection leftDown { get { return downLeft; } }
	public hDirection downLeft { 
		get {
			if (_downLeft == null) _downLeft = new hDirection ("DownLeft", -135, this);
			return _downLeft;
		} 
	}

	private hDirection _upRight;
	public hDirection rightUp { get { return upRight; } }
	public hDirection upRight { 
		get {
			if (_upRight == null) _upRight = new hDirection ("UpRight", 45, this);
			return _upRight;
		} 
	}

	private hDirection _downRight;
	public hDirection rightDown { get { return downRight; } }
	public hDirection downRight { 
		get {
			if (_downRight == null) _downRight = new hDirection ("DownRight", -45, this);
			return _downRight;
		} 
	}

	public void BuildDirections () {
		int indices = up.gamepadIndex;
		indices = down.gamepadIndex;
		indices = left.gamepadIndex;
		indices = right.gamepadIndex;
		indices = upLeft.gamepadIndex;
		indices = upRight.gamepadIndex;
		indices = downLeft.gamepadIndex;
		indices = downRight.gamepadIndex;
	}

	private void UpdateDirections () {
		if ((hDirection)_up != null) _up.Update();
		if ((hDirection)_down != null) _down.Update();
		if ((hDirection)_left != null) _left.Update();
		if ((hDirection)_right != null) _right.Update();
		
		if ((hDirection)_upLeft != null) _upLeft.Update();
		if ((hDirection)_downLeft != null) _downLeft.Update();
		if ((hDirection)_upRight != null) _upRight.Update();
		if ((hDirection)_downRight != null) _downRight.Update();
	}
	

	
	// --------------------
	// UPDATE
	// --------------------
	
	public void Update () {
		UpdateAxes ();
		UpdateDirections ();
	}

	
	// --------------------
	// PUBLIC PROPERTIES - RAW
	// --------------------

	private void UpdateAxes () {
		_horizontalRaw = horizontalAxis.positionRaw;
		_verticalRaw = -verticalAxis.positionRaw;
	}

	private float _horizontalRaw;
	public float horizontalRaw { get { return _horizontalRaw; } }

	private float _verticalRaw;
	public float verticalRaw { get { return _verticalRaw; } }

	public Vector2 positionRaw { get { return new Vector2 (horizontalRaw, verticalRaw); } }

	private float _distanceRaw;
	private float _distanceRawDate;
	public float distanceRaw { 
		get { 
			float time = Time.time;
			if (time == 0 || _distanceRawDate != time) {
				_distanceRaw = positionRaw.magnitude;
				_distanceRawDate = time;
			}
			return _distanceRaw; 
		} 
	}

	private float _angleRaw;
	private float _angleRawDate;
	public float angleRaw { 
		get { 
			float time = Time.time;
			if (time == 0 || _angleRawDate != time) {
				_angleRaw = Vector2.SignedAngle(Vector2.right, positionRaw);
				_angleRawDate = time;
			}
			return _angleRaw;
		} 
	}

	public Vector3 worldPositionCameraRaw { get { return (hInput.worldCamera.right*horizontalRaw + hInput.worldCamera.up*verticalRaw); } }

	public Vector3 worldPositionFlatRaw { get { return new Vector3 (horizontalRaw, 0, verticalRaw); } }

	
	// --------------------
	// PUBLIC PROPERTIES - DEADZONED
	// --------------------

	public bool inDeadZone { get { return distanceRaw < hInput.deadZone; } }

	private Vector2 _position;
	private float _positionDate;
	public Vector2 position { 
		get {
			float time = Time.time;
			if (time == 0 || _positionDate != time) {
				if (inDeadZone) _position = Vector2.zero;
				else {
					Vector2 deadZonedPos = ((1 + hInput.distanceIncrease)*(positionRaw - positionRaw.normalized*hInput.deadZone)/(1 - hInput.deadZone));
					_position = new Vector2 (Mathf.Clamp(deadZonedPos.x, -1, 1), Mathf.Clamp(deadZonedPos.y, -1, 1));
				}
				_positionDate = time;
			}
			return _position; 
		} 
	}

	public float horizontal { get { return position.x; } }
	public float vertical { get { return position.y; } }

	private float _distance;
	private float _distanceDate;
	public float distance { 
		get { 
			float time = Time.time;
			if (time == 0 || _distanceDate != time) {
				_distance = Mathf.Clamp01(position.magnitude);
				_distanceDate = time;
			}
			return _distance; 
		} 
	}

	public bool inTriggerZone { get { return distance >= hInput.triggerZone; } }

	private float _angle;
	private float _angleDate;
	public float angle { 
		get { 
			float time = Time.time;
			if (time == 0 || _angleDate != time) {
				_angle = Vector2.SignedAngle(Vector2.right, position);
				_angleDate = time;
			}
			return _angle;
		} 
	}

	public Vector3 worldPositionCamera { get { return (hInput.worldCamera.right*horizontal + hInput.worldCamera.up*vertical); } }

	public Vector3 worldPositionFlat { get { return new Vector3 (horizontal, 0, vertical); } }
}
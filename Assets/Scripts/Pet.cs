﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Pet: MonoBehaviour {

	private float timeSinceLastCommand;
	private Camera mainCamera;

	private TargetPosRot _target;
	public TargetPosRot Target { get => _target; private set => _target = value; }
	public CommandLocation Command { get; private set; }
	public float Amount { get; set; }

	private int numEffected;
	private List<IButtonEffected> upEffected;
	private List<IButtonEffected> downEffected;
	private IButtonEffected currentEffected;

	private float defaultRotationSign;

	public bool OnScreen {
		get {
			var viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
			return viewportPosition.y <= 1 || viewportPosition.y >= 0;
		}
	}


	public void Awake() {
		mainCamera = Camera.main;
		Target = new TargetPosRot(new Vector3(0, -12f), 90);
	}

	public void Start() {
		if (currentEffected != null) {
			SetTarget();
		}
	}

	public void Init(CommandLocation upCommand, CommandLocation downCommand, List<IButtonEffected> effected) {
		upEffected = new List<IButtonEffected>();
		downEffected = new List<IButtonEffected>();
		foreach(var e in effected) {
			// avoid stupid null state after switching to ceiling
			if(!e.Equals(null)) {
				bool up = e.IsEffectedByButtonLocation(upCommand);
				bool down = e.IsEffectedByButtonLocation(downCommand);
				if (up) {
					upEffected.Add(e);
				}
				if (down) {
					downEffected.Add(e);
				}
			}
		}
		defaultRotationSign = downCommand == CommandLocation.DownLeft ? -1f : 1f;

		SetDefaultEffected(effected, downEffected.Count > 0 ? downCommand : upCommand);
	}

	public void SetDefaultEffected(List<IButtonEffected> effected, CommandLocation defCommmand) {
		var activeEffected = effected.Where(g => !g.Equals(null) && g.GameObject.activeInHierarchy);
		numEffected = activeEffected.Count();
		if (numEffected == 1) {
			Command = defCommmand;
			currentEffected = activeEffected.First();
			currentEffected.AddActor(this, 0);
		}
	}

	public void Update() {
		if(numEffected > 1 && timeSinceLastCommand > 5 && OnScreen) {
			ClearCommand(true);
		}

		float distToTarget;
		Vector3 movement, direction;
		if(currentEffected != null) {
			SetTarget();
			movement = (Target.Position - transform.position);
			direction = movement.normalized;
			distToTarget = movement.magnitude;
			currentEffected.AddActor(this, distToTarget < 0.01f ? Amount : 0);
		} else {
			movement = (Target.Position - transform.position);
			direction = movement.normalized;
			distToTarget = movement.magnitude;
		}

		float distToMove = 15 * Time.deltaTime;
		if(distToMove > distToTarget) {
			distToMove = distToTarget;
		}
		transform.position += distToMove * direction;


		// TODO: Make each pet rotate different directions
		float targetAngle = distToTarget < 2f ? Target.Rotation : Utils.VectorToAngle(direction);
		targetAngle = (targetAngle + 360) % 360;
		float currentAngle = (transform.rotation.eulerAngles.z + 360) % 360;
		float angleToRotate = 540 * Time.deltaTime;
		float angleToTarget = (targetAngle - currentAngle);
		float absAngleToTarget = Mathf.Abs(angleToTarget);
		if (absAngleToTarget < Mathf.Abs(angleToRotate)) {
			angleToRotate = angleToTarget;
		}
		else if (absAngleToTarget > 180 || angleToTarget < 0) {
			angleToRotate *= -1f;
		}

		transform.rotation = Quaternion.Euler(0, 0, currentAngle + angleToRotate);
		

		timeSinceLastCommand += Time.deltaTime;
	}

	public void SetCommand(CommandLocation bl, float amount) {
		if(bl == CommandLocation.None) {
			if(bl != Command) {
				ClearCommand();
			}
		}
		else {
			if(Command != bl) {
				Command = bl;
				var tempEffected = currentEffected;
				currentEffected = (bl == CommandLocation.UpLeft || bl == CommandLocation.UpRight ? upEffected : downEffected)
					.FirstOrDefault(e => !e.Equals(null) && e.GameObject.activeInHierarchy);
				if(tempEffected != null && tempEffected != currentEffected) {
					tempEffected.RemoveActor(this);
				}
				SetTarget();
			}
			Amount = amount;	
			timeSinceLastCommand = 0;
		}
		
	}

	private void ClearCommand(bool flyOff = false) {
		currentEffected?.RemoveActor(this);
		currentEffected = null;
		Command = CommandLocation.None;
		Amount = 0;
		if(flyOff) {
			var sign = Mathf.Sign(transform.position.y);
			Target.Position = new Vector3(transform.position.x, sign * 12, -2f);
			Target.Rotation = sign * 90f;
		}
	}

	public void SetTarget() {
		currentEffected?.OccupantTargetTransform(Command, transform, ref _target);
		transform.position = new Vector3(Target.Position.x, transform.position.y, Target.Position.z);	
	}

	public void MoveFromPush(Vector3 moveAmount) {
		// play pushing animation
		transform.position += moveAmount;
		Target.Position += moveAmount;
	}
}

public class TargetPosRot {
	public Vector3 Position { get; set; }
	public float Rotation { get; set; }

	public TargetPosRot(Vector3 position, float rotation) {
		Position = position;
		Rotation = rotation;
	}
}

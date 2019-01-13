using System;
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

	private List<IButtonEffected> upEffected;
	private List<IButtonEffected> downEffected;
	private IButtonEffected currentEffected;

	public bool OnScreen {
		get {
			var viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
			return viewportPosition.y <= 1 || viewportPosition.y >= 0;
		}
	}
	

	public void Start() {
		mainCamera = Camera.main;
		Target = new TargetPosRot(new Vector3(0, -12f), 90);
	}

	public void Init(CommandLocation upCommand, CommandLocation downCommand, List<IButtonEffected> effected) {
		upEffected = new List<IButtonEffected>();
		downEffected = new List<IButtonEffected>();
		foreach(var e in effected) {
			if(e.IsEffectedByButtonLocation(upCommand)) {
				upEffected.Add(e);
			}
			if (e.IsEffectedByButtonLocation(downCommand)) {
				downEffected.Add(e);
			}
		}
	}

	public void Update() {
		if(timeSinceLastCommand > 5 && currentEffected != null && OnScreen) {
			ClearCommand();
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

		var distToMove = 15 * Time.deltaTime;
		if(distToMove > distToTarget) {
			distToMove = distToTarget;
		}
		transform.position += distToMove * direction;

		var angleToRotate = 360 * Time.deltaTime;
		var angleToTarget = Target.Rotation - transform.rotation.eulerAngles.z;
		if ( angleToTarget < angleToRotate) {
			angleToRotate = angleToTarget;
		}
		transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + angleToRotate);

		timeSinceLastCommand += Time.deltaTime;
	}

	public void SetCommand(CommandLocation bl, float amount) {
		if(bl == CommandLocation.None) {
			Amount = 0;
		}
		else {
			if(Command != bl) {
				var tempEffected = currentEffected;
				currentEffected = (bl == CommandLocation.TopLeft || bl == CommandLocation.TopRight ? upEffected : downEffected)
					.FirstOrDefault(e => e.GameObject.activeInHierarchy);
				if(tempEffected != null && tempEffected != currentEffected) {
					tempEffected.RemoveActor(this);
				}
				SetTarget();
			}
			Amount = amount;
			Command = bl;
			timeSinceLastCommand = 0;
		}
		
	}

	private void ClearCommand() {
		currentEffected?.RemoveActor(this);
		currentEffected = null;
		Command = CommandLocation.None;
		Amount = 0;
		var sign = Mathf.Sign(transform.position.y);
		Target.Position = new Vector2(transform.position.x, sign * 12);
		Target.Rotation = sign * 90f;
	}

	public void SetTarget() {
		currentEffected.OccupantTargetTransform(Command, transform, ref _target);
		transform.position = new Vector2(Target.Position.x, transform.position.y);	
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

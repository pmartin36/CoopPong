﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class SlideOnPress : SlidingObject, IButtonEffected {
	public float Amount { get; set; }
	public CommandLocation[] PositiveActors;
	public CommandLocation[] NegativeActors;

	private Vector3 pushPointDiffFromTransform;
	public GameObject GameObject { get => gameObject; }

	private HashSet<Pet> Actors;

	public ModifiableTResult<CommandLocation, Transform, TargetPosRot> OccupantTargetTransform =>
		(CommandLocation location, Transform otransform, ref TargetPosRot posRot) => {
			// TODO: Make smart based on direction of travel
			if (Mathf.Abs(TrackDirection.x - TrackDirection.y) < 0.1f) {
				//diagonal
			}
			else if (TrackDirection.x > TrackDirection.y) {
				//left and right
			}
			else {
				//up and down

			}
			float y_sign = location == CommandLocation.TopLeft || location == CommandLocation.TopRight ? 1 : -1;
			float x_sign = location == CommandLocation.TopRight || location == CommandLocation.BottomRight ? 1 : -1;
			Vector3 diff = new Vector2(
				Actors.Count > 1 ? otransform.lossyScale.x * x_sign / 2f : 0,
				pushPointDiffFromTransform.y * y_sign);
			posRot.Position = ObjectOnTrack.transform.position + diff;
			posRot.Rotation = y_sign * 90f;
		};

	public override void Start() {
		base.Start();
		Actors = new HashSet<Pet>();
		Direction = 0;


		Collider2D c = ObjectOnTrack.GetComponent<Collider2D>();
		if(c is CircleCollider2D) {
			pushPointDiffFromTransform = Vector2.down * (c as CircleCollider2D).radius;
		}
		else if(c is BoxCollider2D) {
			pushPointDiffFromTransform = Vector2.down * (c as BoxCollider2D).size.y/2f;
		}
		else {
			pushPointDiffFromTransform = Vector2.down * 1;
		}
	}

	public void AddActor(Pet p, float amount) {
		Actors.Add(p);
		var location = p.Command;
		if (PositiveActors.Any(a => a == location)) {
			Amount += amount;
		}
		else if (NegativeActors.Any(a => a == location)) {
			Amount -= amount;
		}
	}

	public void RemoveActor(Pet p) {
		Actors.Remove(p);
	}

	public override void LateUpdate() {
		var start = ObjectOnTrack.transform.position;
		Move(Speed * Amount * Time.deltaTime);
		Amount = 0;

		Vector3 amountMoved = ObjectOnTrack.transform.position - start;
		foreach (Pet p in Actors) {
			p.MoveFromPush(amountMoved);
		}
	}

	public Vector3 GetOccupantPosition(CommandLocation location) {
		return Vector3.one;
	}

	public bool IsEffectedByButtonLocation(CommandLocation bl) {
		return PositiveActors.Any(a => a == bl) || NegativeActors.Any(a => a == bl);
	}
}


﻿using UnityEngine;
using System.Collections;
using System;

public class ShortSword : Weapon {

	public override string Name {
		get {
			return "Short Sword";
		}
	}

	public override string DefaultModel {
		get {
			return "ShortSword";
		}
	}

	public override EquippableSlot Slot {
		get {
			return EquippableSlot.BOTH_HANDS;
		}
	}

	public override StatMod[] StatModifiers {
		get {
			return new StatMod[] {
				new StatMod(CharStat.REG, 5f)
			};
		}
	}

	public override WeaponHandiness Handiness {
		get {
			return WeaponHandiness.ONE_HANDED;
		}
	}

	GameObject RenderedModel { get; set; }
	BoxCollider ModelCollider { get; set; }


	public bool Animating;
	public float AnimationLength = 0.25f;
	float TimeSinceAnimate;

	public Vector3 StartingAnimPos = new Vector3(1.5f, 0f, 0f);
	public Vector3 StartingRotation = new Vector3(0f, 0f, 270f);

	public Vector3 InactivePosition = new Vector3(0.6f, -0.5f, -0.25f);
	public Vector3 InactiveRotation = new Vector3(240f, 0f, 0f);

	public float StaminaUsed = 25f;
	public float Damage = 10f;

	public override void OnStart() {
		RenderedModel = transform.FindChild("Render").gameObject;
		ModelCollider = RenderedModel.GetComponentInChildren<BoxCollider>();

		Physics.IgnoreLayerCollision(LayerMask.NameToLayer("LocalPlayer"), LayerMask.NameToLayer("Weapon"));
	}

	public override void OnUpdate() {
		if (Active && Owner != null && Owner.Alive) { // Weapon is equipped on hand and the owner is alive; the owner can use this weapon.

			if (Animating) {

				ModelCollider.enabled = true;

				if (TimeSinceAnimate > AnimationLength) {
					TimeSinceAnimate = 0f;
					Animating = false;
					Using = false;
				}
				else {
					Vector3 newRot = transform.parent.eulerAngles;
					newRot.y += -145f;
                    transform.rotation = Quaternion.Lerp(transform.parent.rotation, Quaternion.Euler(newRot), TimeSinceAnimate / AnimationLength);

					TimeSinceAnimate += Time.deltaTime;
				}
			}
			else {

				ModelCollider.enabled = false;

				transform.rotation = transform.parent.rotation;
				RenderedModel.transform.localPosition = InactivePosition;
				RenderedModel.transform.localRotation = Quaternion.Euler(InactiveRotation);

				// If the player is pressing the primary attack key, then check if stamina is required
				// -> Stamina is required if the Owner is a Player
				// -> if stamina is required, then ensure that stamina is used.
				// -> If stamina isn't required, then continue without checking for stamina usage
				if (vp_Input.GetButtonAny("Attack1") && (!Owner.IsPlayer || Owner.Player.UseStamina(StaminaUsed))) {

					RenderedModel.transform.localPosition = StartingAnimPos;
					RenderedModel.transform.localEulerAngles = StartingRotation;

					Animating = true;
					Using = true;
				}
			}
		}
	}

	public IEnumerator AnimateNormalAttack(Vector3 point, Vector3 axis,
					  float rotateAmount, float rotateTime) {

		float step = 0.0f; //non-smoothed
		float rate = 1.0f/rotateTime; //amount to increase non-smooth step by
		float smoothStep = 0.0f; //smooth step this time
		float lastStep = 0.0f; //smooth step last time
		while (step < 1.0) { // until we're done
			step += Time.deltaTime * rate; //increase the step
			smoothStep = Mathf.SmoothStep(0.0f, 1.0f, step); //get the smooth step
			transform.RotateAround(point, axis,
								   rotateAmount * (smoothStep - lastStep));
			lastStep = smoothStep; //store the smooth step
			yield return null;
		}
		//finish any left-over
		if (step > 1.0)
			transform.RotateAround(point, axis,
								   rotateAmount * (1.0f - lastStep));

		Animating = false;
		Using = false;
	}

	void OnTriggerEnter(Collider other) {
		
		if (other.tag != "Player") {
			if (other.GetComponent<vp_DamageHandler>() != null) other.SendMessage("Damage", new vp_DamageInfo(Damage, Owner.transform), SendMessageOptions.DontRequireReceiver);
			else if (other.GetComponent<Pawn>() != null) other.SendMessage("Damage", new DamageInfo(DamageType.PHYSICAL, DamageElement.NONE, Damage, Owner));
		}
	}
}

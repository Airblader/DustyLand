using System;
using System.Collections.Generic;
using KSP;
using UnityEngine;

public class EngineEmitter {
	private const int LAYER_MASK = 1 << 15;
	private readonly Part part;
	private readonly ParticleEmitter emitter;
	private readonly Transform thruster;

	public EngineEmitter( Part part, Transform thruster ) {
		this.part = part;
		this.thruster = thruster;

		this.emitter = CreateParticleEmitter();
	}

	public void Process() {
		// TODO Don't use infinity, but account for tilted ships
		// TODO consider distance, too
		RaycastHit thrustTargetOnSurface;
		bool hit = Physics.Raycast( part.transform.position, thruster.forward, out thrustTargetOnSurface,
			           Mathf.Infinity, LAYER_MASK );

		if( hit ) {
			emitter.emit = true;
			emitter.gameObject.SetActive( true );

			emitter.transform.parent = part.transform;
			emitter.transform.position = thrustTargetOnSurface.point - 0.5f * thruster.forward.normalized;

			// TODO reuse this when setting localVelocity here
			//float angle = Vector3.Angle( thrust.eulerAngles, thrustTargetOnSurface.normal );
			//emitter.transform.Rotate( 90, 90 - angle, 0 );

			// TODO currently has no effect because localVelocity = 0
			emitter.transform.LookAt( thruster.position );
			// TODO account for this when setting localVelocity
			emitter.transform.Rotate( 90, 0, 0 );
		} else {
			Deactivate();
		}
	}

	public void Deactivate() {
		emitter.emit = false;

		if( emitter.particleCount == 0 ) {
			emitter.gameObject.SetActive( false );
		}
	}

	private ParticleEmitter CreateParticleEmitter() {
		GameObject emitterGameObject = (GameObject) UnityEngine.Object.Instantiate( UnityEngine.Resources.Load( "Effects/fx_smokeTrail_light" ) );
		emitterGameObject.transform.parent = part.transform;
		emitterGameObject.transform.localRotation = Quaternion.identity;
		emitterGameObject.SetActive( false );

		ParticleEmitter emitter = emitterGameObject.GetComponent<ParticleEmitter>();
		emitter.maxEmission = 10;
		emitter.maxEnergy = 5;
		emitter.maxSize = 7;
		emitter.minEmission = 3;
		emitter.minEnergy = 1;
		emitter.minSize = 1;

		// TODO this can be updated in each update cycle to account for tilt
		emitter.localVelocity = new Vector3( 0, 0, 0 );

		return emitter;
	}
}
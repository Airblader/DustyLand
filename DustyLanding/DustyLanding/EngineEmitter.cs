using System;
using System.Collections.Generic;
using KSP;
using UnityEngine;

public class EngineEmitter {
	private const int LAYER_MASK = 1 << 15;
	private readonly Part part;
	private readonly ParticleEmitter emitter;
	private readonly Transform thruster;

	public EngineEmitter( Part part, ParticleEmitter emitter, Transform thruster ) {
		this.part = part;
		this.emitter = emitter;
		this.thruster = thruster;
	}

	public void Process() {
		emitter.emit = true;
		emitter.gameObject.SetActive( true );

		// TODO Don't use infinity, but account for tilted ships
		// TODO consider distance, too
		RaycastHit thrustTargetOnSurface;
		bool hit = Physics.Raycast( part.transform.position, thruster.forward, out thrustTargetOnSurface,
			           Mathf.Infinity, LAYER_MASK );

		if( hit ) {
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
}
using System;
using System.Collections.Generic;
using UnityEngine;
using KSP;

public class EngineEmitters {
	public readonly DualModuleEngines engine;
	private List<EngineEmitter> emitters = new List<EngineEmitter>();

	public EngineEmitters( Part part, DualModuleEngines engine ) {
		this.engine = engine;
		foreach( Transform thrust in engine.thrustTransforms ) {
			emitters.Add( new EngineEmitter( part, CreateParticleEmitter( engine.module.part ), thrust ) );
		}
	}

	public void Process() {
		if( !IsEngineActive() ) {
			emitters.ForEach( emitter => emitter.Deactivate() );
			return;
		}

		emitters.ForEach( emitter => emitter.Process() );
	}

	private bool IsEngineActive() {
		return engine.isEnabled && engine.isIgnited && !engine.isFlameout && engine.HasThrust();
	}

	private ParticleEmitter CreateParticleEmitter( Part part ) {
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
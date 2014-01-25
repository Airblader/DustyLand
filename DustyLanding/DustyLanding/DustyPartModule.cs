using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP;

public class DustyPartModule : PartModule {
	public const string MODULE_NAME = "DustyPartModule";
	private const int LAYER_MASK = 1 << 15;
	// TODO name
	private List<DustyEngineModule> engineModules = new List<DustyEngineModule>();
	// TODO name
	private class DustyEngineModule {
		public readonly DualModuleEngines engine;
		public List<ParticleEmitter> emitters = new List<ParticleEmitter>();

		public DustyEngineModule( DualModuleEngines engine ) {
			this.engine = engine;
			foreach( Transform thrust in engine.thrustTransforms ) {
				emitters.Add( CreateParticleEmitter( engine.module.part ) );
			}
		}

		private ParticleEmitter CreateParticleEmitter( Part part ) {
			GameObject emitterGameObject = (GameObject) UnityEngine.Object.Instantiate( UnityEngine.Resources.Load( "Effects/fx_smokeTrail_medium" ) );
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

	override public void OnAwake() {
		Logging.Trace( "Woke up for part = " + part.name ?? "<unknown>" );
		InvokeRepeating( "Process", 0.0f, 0.1f );
	}

	private void Process() {
		Logging.Trace( "Processing for part " + part.name ?? "<unknown>" );
		if( !FlightGlobals.ready ) {
			return;
		}

		UpdateCapturedModules();
		engineModules.ForEach( module => ProcessModule( module ) );
	}

	private void UpdateCapturedModules() {
		foreach( DualModuleEngines engine in part.GetDualModuleEngines() ) {
			if( engineModules.Any( m => m.engine.module.Equals( engine.module ) ) ) {
				continue;
			}

			Logging.Trace( "Found a new engine module, capturing it" );
			engineModules.Add( new DustyEngineModule( engine ) );
		}

		Logging.Trace( "Number of captures modules: " + engineModules.Count );
	}

	private void ProcessModule( DustyEngineModule module ) {
		Logging.Trace( "Processing module " + module.GetHashCode() );
		if( !module.engine.isEnabled || !module.engine.isIgnited || module.engine.isFlameout || !module.engine.HasThrust() ) {
			foreach( ParticleEmitter emitter in module.emitters ) {
				// TODO this will remove the emitters instantly, but it should just go away smoothly
				// (i.e. just stop emitting and set them to active once all particles are gone)
				emitter.gameObject.SetActive( false );
			}

			return;
		}

		// TODO rewrite this weird double-list construct into a separate DustyTransform class
		int i = 0;
		foreach( ParticleEmitter emitter in module.emitters ) {
			Transform thrust = module.engine.thrustTransforms[i];
			Logging.Trace( "current: " + thrust ?? "nothin" );
			// TODO Don't use infinity, but account for tilted ships
			RaycastHit thrustTargetOnSurface;
			bool hit = Physics.Raycast( part.transform.position, thrust.forward, out thrustTargetOnSurface,
				           Mathf.Infinity, LAYER_MASK );
			// TODO consider distance, too
			emitter.gameObject.SetActive( hit );

			if( hit ) {
				emitter.transform.parent = part.transform;
				emitter.transform.position = thrustTargetOnSurface.point - 0.5f * thrust.forward.normalized;

				// TODO reuse this when setting localVelocity here
				//float angle = Vector3.Angle( thrust.eulerAngles, thrustTargetOnSurface.normal );
				//emitter.transform.Rotate( 90, 90 - angle, 0 );

				// TODO currently has no effect because localVelocity = 0
				emitter.transform.LookAt( thrust.position );
				// TODO account for this when setting localVelocity
				emitter.transform.Rotate( 90, 0, 0 );
			}
		}
	}
}



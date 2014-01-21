using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP;

/// Plan:
/// -1. test thrustTransform length
///  1. Rename this to DustyEngineWatcher
/// 	- it creates a DustyEngine on each module if it doesn't exist
///  2. Create MonoBehaviour DustyEngine
/// 	- knows about the part it is attached to
/// 	- controls the emitters

public class DustyEngineModule : PartModule, IDisposable {
	public const string MODULE_NAME = "DustyEngineModule";
	private const int LAYER_MASK = 1 << 15;
	private FXGroup fxDust;

	override public void OnAwake() {
		if( !part.HasEngineModule() ) {
			throw new ApplicationException( "The part is not an engine" );
		}

		InitFx();
		Logging.Log( "Module woke up for " + part.name );
	}

	override public void OnStart( StartState state ) {
		// TODO will this ever be called?
		if( state.IsOneOf( StartState.None, StartState.Editor ) || HighLogic.LoadedScene != GameScenes.FLIGHT ) {
			Logging.Log( "Not in correct state or scene, aborting" );
			return;
		}

		if( vessel == null ) {
			throw new MissingComponentException( "Part is not attached to a vessel" );
		}
	}

	override public void OnFixedUpdate() {
		// TODO do Awake, OnStart here?
		if( !FlightGlobals.ready ) {
			return;
		}

		try {
			UpdateEmitters();
		} catch( Exception e ) {
			Logging.Error( "Exception while updating emitters", e );
		}
	}

	public void Dispose() {
		if( fxDust == null ) {
			return;
		}

		DestroyFx();
	}

	private void UpdateEmitters() {
		DualModuleEngines engine = part.GetDualModuleEngines()[0];
		if( engine == null ) {
			Logging.Log( "Engine module not found" );
			return;
		}

		if( !engine.isEnabled || !engine.isIgnited || engine.isFlameout || !engine.HasThrust() ) {
			foreach( ParticleEmitter emitter in fxDust.fxEmitters ) {
				// TODO this will remove the emitters instantly, but it should just go away smoothly
				// (i.e. just stop emitting and set them to active once all particles are gone)
				emitter.gameObject.SetActive( false );
			}

			return;
		}

		foreach( ParticleEmitter emitter in fxDust.fxEmitters ) {
			UpdateEmitter( emitter, engine );
		}
	}

	private void UpdateEmitter( ParticleEmitter emitter, DualModuleEngines engine ) {
		// TODO remove this and add emitters for all thrusters
		if( engine.thrustTransforms.Count == 0 ) {
			return;
		}

		// TODO Don't use infinity, but account for tilted ships
		RaycastHit thrustTargetOnSurface;
		bool hit = Physics.Raycast( part.transform.position, engine.thrustTransforms[0].forward, out thrustTargetOnSurface,
			           Mathf.Infinity, LAYER_MASK );
		// TODO consider distance, too
		emitter.gameObject.SetActive( hit );

		if( hit ) {
			emitter.transform.parent = part.transform;
			emitter.transform.position = thrustTargetOnSurface.point - 0.5f * engine.thrustTransforms[0].forward.normalized;

			// TODO reuse this when setting localVelocity here
			//float angle = Vector3.Angle( module.thrustTransforms[0].eulerAngles, thrustTargetOnSurface.normal );
			//emitter.transform.Rotate( 90, 90 - angle, 0 );

			// TODO currently has no effect because localVelocity = 0
			emitter.transform.LookAt( engine.thrustTransforms[0].position );
			// TODO account for this when setting localVelocity
			emitter.transform.Rotate( 90, 0, 0 );
		}
	}

	private void InitFx() {
		Logging.Log( "Initialize particle emitters" );

		if( fxDust != null ) {
			DestroyFx();
		}

		fxDust = new FXGroup( vessel.vesselName );
		// TODO maybe use a set of ~8 emitters arranged in a circle
		fxDust.fxEmitters.Add( CreateParticleEmitter( "fx_smokeTrail_medium" ) );
	}

	private ParticleEmitter CreateParticleEmitter( string fxName ) {
		GameObject emitterGameObject = (GameObject) UnityEngine.Object.Instantiate( UnityEngine.Resources.Load( "Effects/" + fxName ) );
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

	private void DestroyFx() {
		foreach( ParticleEmitter emitter in fxDust.fxEmitters ) {
			GameObject.DestroyImmediate( emitter );
		}
	}
}



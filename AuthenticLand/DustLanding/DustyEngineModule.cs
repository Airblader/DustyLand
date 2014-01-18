using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP;

public class DustyEngineModule : PartModule, IDisposable {
	public const string MODULE_NAME = "DustyEngineModule";
	private const int LAYER_MASK = 1 << 15;
	private FXGroup fxDust;

	override public void OnAwake() {
		if( !part.HasModule<ModuleEngines>() ) {
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
			Logging.Log( "Not yet ready, aborting" );
			return;
		}

		UpdateEmitters();
	}

	public void Dispose() {
		if( fxDust == null ) {
			return;
		}

		DestroyFx();
	}

	internal void UpdateEmitters() {
		ModuleEngines module = GetEngineModule();
		if( module == null ) {
			Logging.Log( "Engine module not found" );
			return;
		}

		// TODO check if there's any thrust
		if( !module.isEnabled ) {
			Logging.Log( "Module is not enabled, turning off dust effects" );

			foreach( ParticleEmitter emitter in fxDust.fxEmitters ) {
				emitter.gameObject.SetActive( false );
			}

			return;
		}

		Logging.Log( "Updating particle emitters" );

		foreach( ParticleEmitter emitter in fxDust.fxEmitters ) {
			// TODO remove this and add emitters for all thrusters
			if( module.thrustTransforms.Count == 0 ) {
				continue;
			}

			// TODO Don't use infinity, but account for tilted ships
			RaycastHit thrustTargetOnSurface;
			Physics.Raycast( part.transform.position, module.thrustTransforms[0].forward, out thrustTargetOnSurface,
				Mathf.Infinity, LAYER_MASK );

			emitter.transform.parent = part.transform;
			emitter.transform.position = thrustTargetOnSurface.point;
			emitter.transform.LookAt( module.thrustTransforms[0].position );
			emitter.transform.Rotate( 90, 0, 0 );

			emitter.gameObject.SetActive( true );
		}
	}

	internal ModuleEngines GetEngineModule() {
		return part.Modules.OfType<ModuleEngines>().First();
	}

	internal void InitFx() {
		Logging.Log( "Initialize particle emitters" );

		if( fxDust != null ) {
			DestroyFx();
		}

		fxDust = new FXGroup( vessel.vesselName );
		fxDust.fxEmitters.Add( CreateParticleEmitter( "fx_smokeTrail_medium" ).GetComponent<ParticleEmitter>() );
	}

	internal GameObject CreateParticleEmitter( string fxName ) {
		GameObject emitter = (GameObject) UnityEngine.Object.Instantiate( UnityEngine.Resources.Load( "Effects/" + fxName ) );
		emitter.transform.parent = part.transform;
		emitter.transform.localRotation = Quaternion.identity;
		emitter.SetActive( false );

		return emitter;
	}

	internal void DestroyFx() {
		foreach( ParticleEmitter emitter in fxDust.fxEmitters ) {
			GameObject.DestroyImmediate( emitter );
		}
	}
}



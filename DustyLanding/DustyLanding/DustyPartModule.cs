using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP;

public class DustyPartModule : PartModule {
	public const string MODULE_NAME = "DustyPartModule";
	private const int LAYER_MASK = 1 << 15;
	private List<EngineEmitters> engines = new List<EngineEmitters>();

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
		engines.ForEach( module => ProcessModule( module ) );
	}

	private void UpdateCapturedModules() {
		foreach( DualModuleEngines engine in part.GetDualModuleEngines() ) {
			if( engines.Any( m => m.engine.module.Equals( engine.module ) ) ) {
				continue;
			}

			Logging.Trace( "Found a new engine module, capturing it" );
			engines.Add( new EngineEmitters( engine ) );
		}

		Logging.Trace( "Number of captures modules: " + engines.Count );
	}

	private void ProcessModule( EngineEmitters module ) {
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



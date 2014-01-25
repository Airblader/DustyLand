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
		Logging.Log( String.Format( "Waking up for part {0} [{1}]", part.name, part.GetHashCode() ) );
		InvokeRepeating( "Process", 0.0f, 0.1f );
	}

	private void Process() {
		Logging.Trace( String.Format( "Processing part {0} [{1}]", part.name, part.GetHashCode() ) );
		if( !FlightGlobals.ready ) {
			return;
		}

		UpdateCapturedModules();
		engines.ForEach( module => ProcessModule( module ) );
	}

	private void UpdateCapturedModules() {
		bool foundNewModule = false;
		foreach( DualModuleEngines engine in part.GetDualModuleEngines() ) {
			if( engines.Any( m => m.engine.module.Equals( engine.module ) ) ) {
				continue;
			}

			Logging.Trace( "Engine module detected, attaching it" );
			engines.Add( new EngineEmitters( engine ) );
			foundNewModule = true;
		}

		if( foundNewModule ) {
			Logging.Trace( String.Format( "Part {0} [{1}] now has {2} detected engine modules", part.name,
				part.GetHashCode(), engines.Count ) );
		}
	}

	private void ProcessModule( EngineEmitters module ) {
		// TODO extract method
		if( !module.engine.isEnabled || !module.engine.isIgnited || module.engine.isFlameout || !module.engine.HasThrust() ) {
			foreach( ParticleEmitter emitter in module.emitters ) {
				emitter.emit = false;
				if( emitter.particleCount == 0 ) {
					emitter.gameObject.SetActive( false );
				}
			}

			return;
		}

		// TODO rewrite this weird double-list construct into a separate DustyTransform class
		int i = 0;
		foreach( ParticleEmitter emitter in module.emitters ) {
			emitter.emit = true;
			Transform thrust = module.engine.thrustTransforms[i];

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



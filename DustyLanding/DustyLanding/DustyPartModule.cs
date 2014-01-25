using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP;

[ReflectivelyUsed]
public class DustyPartModule : PartModule {
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
		engines.ForEach( engine => ProcessEngine( engine ) );
	}

	private void UpdateCapturedModules() {
		bool foundNewModule = false;
		foreach( DualModuleEngines engine in part.GetDualModuleEngines() ) {
			if( engines.Any( engineEmitter => engineEmitter.engine.module.Equals( engine.module ) ) ) {
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

	private void ProcessEngine( EngineEmitters engine ) {
		if( !IsEngineActive( engine.engine ) ) {
			DeactivateEmitters( engine );
			return;
		}

		foreach( EngineEmitter emitter in engine.emitters ) {
			ProcessEmitter( emitter );
		}
	}

	private void ProcessEmitter( EngineEmitter emitter ) {
		emitter.emitter.emit = true;
		emitter.emitter.gameObject.SetActive( true );

		// TODO Don't use infinity, but account for tilted ships
		// TODO consider distance, too
		RaycastHit thrustTargetOnSurface;
		bool hit = Physics.Raycast( part.transform.position, emitter.thruster.forward, out thrustTargetOnSurface,
			           Mathf.Infinity, LAYER_MASK );

		if( hit ) {
			emitter.emitter.transform.parent = part.transform;
			emitter.emitter.transform.position = thrustTargetOnSurface.point - 0.5f * emitter.thruster.forward.normalized;

			// TODO reuse this when setting localVelocity here
			//float angle = Vector3.Angle( thrust.eulerAngles, thrustTargetOnSurface.normal );
			//emitter.transform.Rotate( 90, 90 - angle, 0 );

			// TODO currently has no effect because localVelocity = 0
			emitter.emitter.transform.LookAt( emitter.thruster.position );
			// TODO account for this when setting localVelocity
			emitter.emitter.transform.Rotate( 90, 0, 0 );
		} else {
			DeactivateEmitter( emitter );
		}
	}

	private bool IsEngineActive( DualModuleEngines engineModule ) {
		return engineModule.isEnabled && engineModule.isIgnited && !engineModule.isFlameout && engineModule.HasThrust();
	}

	private void DeactivateEmitters( EngineEmitters engine ) {
		foreach( EngineEmitter emitter in engine.emitters ) {
			DeactivateEmitter( emitter );
		}
	}

	private void DeactivateEmitter( EngineEmitter emitter ) {
		emitter.emitter.emit = false;

		if( emitter.emitter.particleCount == 0 ) {
			emitter.emitter.gameObject.SetActive( false );
		}
	}
}



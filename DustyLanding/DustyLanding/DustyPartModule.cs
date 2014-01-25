using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP;

[ReflectivelyUsed]
public class DustyPartModule : PartModule {
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
		engines.ForEach( engine => engine.Process() );
	}

	private void UpdateCapturedModules() {
		bool foundNewModule = false;
		foreach( DualModuleEngines engine in part.GetDualModuleEngines() ) {
			if( engines.Any( engineEmitter => engineEmitter.engine.module.Equals( engine.module ) ) ) {
				continue;
			}

			Logging.Trace( "Engine module detected, attaching it" );
			engines.Add( new EngineEmitters( part, engine ) );
			foundNewModule = true;
		}

		if( foundNewModule ) {
			Logging.Trace( String.Format( "Part {0} [{1}] now has {2} detected engine modules", part.name,
				part.GetHashCode(), engines.Count ) );
		}
	}
}



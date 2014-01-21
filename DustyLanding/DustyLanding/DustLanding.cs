using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP;

[KSPAddon( KSPAddon.Startup.Flight, false )]
public class DustLanding : MonoBehaviour {
	private Vessel vessel {
		get {
			return FlightGlobals.fetch.activeVessel;
		}
	}

	public void Start() {
		InvokeRepeating( "ScanVesselParts", 0.0f, 1.0f );
	}

	private void ScanVesselParts() {
		if( !FlightGlobals.ready || vessel == null || !vessel.loaded ) {
			Logging.Log( "no vessel found or vessel not ready, aborting." );
			return;
		}

		Logging.Trace( "scanning vessel parts" );
		foreach( Part part in vessel.parts ) {
			if( !part.HasEngineModule() || part.HasModule<DustyPartModule>() ) {
				continue;
			}

			part.AddModule( DustyPartModule.MODULE_NAME );
		}
	}
}
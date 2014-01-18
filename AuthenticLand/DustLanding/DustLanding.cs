using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP;

[KSPAddon( KSPAddon.Startup.Flight, false )]
public class DustLanding : MonoBehaviour {
	private Vessel vessel;

	private void FixedUpdate() {
		if( !FlightGlobals.ready ) {
			return;
		}

		bool vesselChanged = UpdateVessel();
		if( vessel == null || !vessel.loaded ) {
			return;
		}

		if( vesselChanged ) {
			SetupEmitters();
		}
	}

	internal bool UpdateVessel() {
		if( FlightGlobals.fetch.activeVessel != vessel ) {
			vessel = FlightGlobals.fetch.activeVessel;
			return true;
		}

		return false;
	}

	internal void SetupEmitters() {
		foreach( Part part in vessel.parts ) {
			// TODO support ModuleEnginesFX (add a common interface)
			if( !part.HasModule<ModuleEngines>() || part.HasModule<DustyEngineModule>() ) {
				continue;
			}

			// TODO is Awaken still needed? Doesn't seem like it
			//Awaken( part.AddModule( DustyEngineModule.MODULE_NAME ) );
			part.AddModule( DustyEngineModule.MODULE_NAME );
		}
	}

	/// <summary>
	/// Taken from https://github.com/Ialdabaoth/ModuleManager/blob/master/moduleManager.cs
	/// </summary>
	internal bool Awaken( PartModule module ) {
		MethodInfo awakeMethod = typeof( PartModule ).GetMethod( "Awake", BindingFlags.Instance | BindingFlags.NonPublic );
		if( awakeMethod == null ) {
			return false;
		}

		awakeMethod.Invoke( module, new object[] { } );
		return true;
	}
}
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP;

[KSPAddon( KSPAddon.Startup.Flight, false )]
public class DustLanding : MonoBehaviour {
	private Vessel vessel;

	public void FixedUpdate() {
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

	private bool UpdateVessel() {
		if( FlightGlobals.fetch.activeVessel != vessel ) {
			vessel = FlightGlobals.fetch.activeVessel;
			return true;
		}

		return false;
	}

	private void SetupEmitters() {
		foreach( Part part in vessel.parts ) {
			if( !part.HasEngineModule() || part.HasModule<DustyPartModule>() ) {
				continue;
			}

			// TODO is Awaken still needed? Doesn't seem like it
			//Awaken( part.AddModule( DustyEngineModule.MODULE_NAME ) );
			part.AddModule( DustyPartModule.MODULE_NAME );
		}
	}

	/// <summary>
	/// Taken from https://github.com/Ialdabaoth/ModuleManager/blob/master/moduleManager.cs
	/// </summary>
	private bool Awaken( PartModule module ) {
		MethodInfo awakeMethod = typeof( PartModule ).GetMethod( "Awake", BindingFlags.Instance | BindingFlags.NonPublic );
		if( awakeMethod == null ) {
			return false;
		}

		awakeMethod.Invoke( module, new object[] { } );
		return true;
	}
}
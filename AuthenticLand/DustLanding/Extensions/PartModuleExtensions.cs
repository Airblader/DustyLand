using System;
using UnityEngine;
using KSP;

public static class PartModuleExtensions {
	// TODO ignores several factors (see MechJeb)
	public static double GetCurrentThrust( this ModuleEngines engine ) {
		throw new NotSupportedException();
		//return engine.maxThrust * engine.currentThrottle + engine.minThrust * ( 1 - engine.currentThrottle );
	}

	public static bool HasThrust( this ModuleEngines engine ) {
		return engine.currentThrottle > 0.01f;
	}
}
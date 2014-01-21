using System;
using UnityEngine;
using KSP;

public static class PartModuleExtensions {
	// TODO ignores several factors (see MechJeb)
	public static double GetCurrentThrust( this ModuleEngines engine ) {
		throw new NotSupportedException();
		//return engine.maxThrust * engine.currentThrottle + engine.minThrust * ( 1 - engine.currentThrottle );
	}
	// TODO is not a PartModule extension anymore (not directly, anyway)
	public static bool HasThrust( this DualModuleEngines engine ) {
		return engine.currentThrottle > 0.01f;
	}
}
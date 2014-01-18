using System;
using System.Collections.Generic;
using KSP;
using UnityEngine;

/// <summary>
/// DualModuleEngines wraps a PartModule which is either of type ModuleEngines or ModuleEnginesFX and
/// provides a consolidated API for both types.
/// 
/// Note that the consolidated API may not be complete. However, it is fairly easy to extend it :)
/// </summary>
public class DualModuleEngines {
	/// The wrapped PartModule instance.
	public readonly PartModule module;
	/// Whether the wrapped module is of type ModuleEnginesFX (true) or ModuleEngines (false).
	/// Note: This is only public for use in extension methods.
	public readonly bool isFx;

	/// Return the wrapped module as ModuleEngines.
	/// Note: This is only public for use in extension methods.
	public  ModuleEngines asNoFx {
		get {
			return module as ModuleEngines;
		}
	}

	#region DualModuleEngines

	/// Return the wrapped module as ModuleEnginesFX.
	/// Note: This is only public for use in extension methods.
	public  ModuleEnginesFX asFx {
		get {
			return module as ModuleEnginesFX;
		}
	}

	private DualModuleEngines( ModuleEngines module ) {
		this.module = module;
		this.isFx = false;
	}

	private DualModuleEngines( ModuleEnginesFX module ) {
		this.module = module;
		this.isFx = true;
	}

	/// Converts module into DualModuleEngines and returns null if
	/// module is neither of type ModuleEngines nor ModuleEnginesFX.
	public static DualModuleEngines From( PartModule module ) {
		if( module is ModuleEnginesFX ) {
			return new DualModuleEngines( module as ModuleEnginesFX );
		} else if( module is ModuleEngines ) {
			return new DualModuleEngines( module as ModuleEngines );
		}

		return null;
	}

	#endregion

	#region Consolidated API

	public bool isEnabled {
		get {
			return isFx ? asFx.isEnabled : asNoFx.isEnabled;
		}
	}

	public bool isIgnited {
		get {
			return isFx ? asFx.EngineIgnited : asNoFx.EngineIgnited;
		}
	}

	public bool isFlameout {
		get {
			return isFx ? asFx.getFlameoutState : asNoFx.getFlameoutState;
		}
	}

	public List<Transform> thrustTransforms {
		get {
			return isFx ? asFx.thrustTransforms : asNoFx.thrustTransforms;
		}
	}

	public float currentThrottle {
		get {
			return isFx ? asFx.currentThrottle : asNoFx.currentThrottle;
		}
	}

	#endregion

}

/// <summary>
/// Convenience methods for using DualModuleEngines.
/// </summary>
public static class DualModuleEnginesPartExtensions {
	public static List<DualModuleEngines> GetDualModuleEngines( this Part part ) {
		List<DualModuleEngines> modules = new List<DualModuleEngines>();
		foreach( PartModule module in part.Modules ) {
			DualModuleEngines engineModule = DualModuleEngines.From( module );
			if( engineModule != null ) {
				modules.Add( engineModule );
			}
		}

		return modules;
	}

	public static bool HasEngineModule( this Part part ) {
		return part.GetDualModuleEngines().Count != 0;
	}
}
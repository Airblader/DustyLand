using System;
using System.Linq;
using System.Collections.Generic;
using KSP;

public static class PartExtensions {
	public static bool HasModule<T>( this Part p ) where T : PartModule {
		return p.Modules != null && p.Modules.OfType<T>().Count() > 0;
	}
}


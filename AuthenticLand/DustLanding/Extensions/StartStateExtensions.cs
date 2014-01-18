using System;
using KSP;

public static class StartStateExtensions {
	public static bool IsOneOf( this PartModule.StartState self, params PartModule.StartState[] states ) {
		foreach( PartModule.StartState state in states ) {
			if( self == state ) {
				return true;
			}
		}

		return false;
	}
}


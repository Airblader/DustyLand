using System;
using UnityEngine;

public static class Logging {
	public static void Log( string message ) {
		Debug.Log( FormatMessage( message ) );
	}

	internal static string FormatMessage( string message ) {
		return "[DustyLander]: " + message;
	}
}



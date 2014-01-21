#define DEBUG

using System;
using UnityEngine;

public static class Logging {
	public static void Trace( string message ) {
		#if DEBUG
		Log( message );
		#endif
	}

	public static void Log( string message ) {
		Debug.Log( FormatMessage( message ) );
	}

	public static void Error( string message, Exception e ) {
		Debug.LogError( FormatException( FormatMessage( message ), e ) );
	}

	internal static string FormatMessage( string message ) {
		return "[DustyLander]: " + message;
	}

	internal static string FormatException( string message, Exception e ) {
		return message + " [" + e.GetType() + "] " + e.StackTrace;
	}
}



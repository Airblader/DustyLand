using System;

/// <summary>
/// Notes that the attributed element is being used via reflection. Has no effect otherwise.
/// </summary>
[System.AttributeUsage( System.AttributeTargets.All )]
public class ReflectivelyUsed : System.Attribute {
}

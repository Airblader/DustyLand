using System;
using System.Collections.Generic;
using KSP;
using UnityEngine;

public class EngineEmitter {
	public readonly ParticleEmitter emitter;
	public readonly Transform thruster;

	public EngineEmitter( ParticleEmitter emitter, Transform thruster ) {
		this.emitter = emitter;
		this.thruster = thruster;
	}
}
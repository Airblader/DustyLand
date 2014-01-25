using System;
using System.Collections.Generic;
using UnityEngine;
using KSP;

public class EngineEmitters {
	public readonly DualModuleEngines engine;
	private List<EngineEmitter> emitters = new List<EngineEmitter>();

	public EngineEmitters( Part part, DualModuleEngines engine ) {
		this.engine = engine;
		foreach( Transform thrust in engine.thrustTransforms ) {
			emitters.Add( new EngineEmitter( part, thrust ) );
		}
	}

	public void Process() {
		if( !IsEngineActive() ) {
			emitters.ForEach( emitter => emitter.Deactivate() );
			return;
		}

		emitters.ForEach( emitter => emitter.Process() );
	}

	private bool IsEngineActive() {
		return engine.isEnabled && engine.isIgnited && !engine.isFlameout && engine.HasThrust();
	}
}
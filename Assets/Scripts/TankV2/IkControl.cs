using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TankExtensions;
using IkSolver;
using ValueDriver;

namespace TankV2{

[System.Serializable]
public class IkControl{
	public Transform legRFTarget = null;
	public Transform legRBTarget = null;
	public Transform legLFTarget = null;
	public Transform legLBTarget = null;
	
	public Transform getLegIkTarget(int legIndex){
		switch(legIndex){
			case(0):
				return legLFTarget;
			case(1):
				return legRFTarget;
			case(2):
				return legLBTarget;
			case(3):
				return legRBTarget;
			default:
				return legLFTarget;
		}
	}
}

}
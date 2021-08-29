using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TankExtensions;
using IkSolver;
using ValueDriver;

namespace TankV2{

[System.Serializable]
public class DirectControl{
	public float turretControlAngle = 0.0f;
	public float barrelControlAngle = 0.0f;
	public LegControl legControlRF = new LegControl();
	public LegControl legControlLF = new LegControl();
	public LegControl legControlRB = new LegControl();
	public LegControl legControlLB = new LegControl();
	LegControl getLeg(int legIndex){
		switch(legIndex){
			case(0):
				return legControlLF;
			case(1):
				return legControlRF;
			case(2):
				return legControlLB;
			case(3):
				return legControlRB;
			default:
				throw new System.ArgumentOutOfRangeException();
		}
	}
}

}
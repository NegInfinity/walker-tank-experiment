using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TankExtensions;
using IkSolver;
using ValueDriver;

namespace TankV2{

[System.Serializable]
public class LegControl{
	public float legYaw = 0.0f;
	public float upperLeg = 0.0f;
	public float lowerLeg = 0.0f;
}

}
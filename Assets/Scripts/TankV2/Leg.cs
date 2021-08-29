using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TankExtensions;
using IkSolver;
using ValueDriver;

namespace TankV2{

[System.Serializable]
public struct Leg{
	public Part hip;
	public Part upper;
	public Part lower;
	public Part tip;
	public bool right;
	public bool front;
}

}
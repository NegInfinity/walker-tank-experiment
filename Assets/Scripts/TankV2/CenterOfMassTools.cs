using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TankExtensions;
using IkSolver;
using ValueDriver;

namespace TankV2{

public static class CenterOfMassTools{
	public static void addCenterOfMass(ref Vector3 center, ref float mass, Part part){
		if (!part.rigBody)
			return;
		var worldCenter = part.rigBody.worldCenterOfMass;
		var partMass = part.rigBody.mass;
		center += worldCenter * partMass;
		mass += partMass;
	}

	public static void addCenterOfMass(ref Vector3 center, ref float mass, Leg leg){
		addCenterOfMass(ref center, ref mass, leg.hip);
		addCenterOfMass(ref center, ref mass, leg.upper);
		addCenterOfMass(ref center, ref mass, leg.lower);
	}
}

}

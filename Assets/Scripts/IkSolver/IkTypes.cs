using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IkSolver{

[System.Serializable]
public class IkNode{
	public Coord defaultCoord = Coord.identity;
	public IkJointAxes localJointAxes = IkJointAxes.defaultAxes;

	public Coord local = Coord.identity;
	public Coord world = Coord.identity;

	public IkJointAxes defaultJointAxes{
		get => localJointAxes.transformedBy(defaultCoord);
	}
	public IkJointAxes jointAxes{
		get => localJointAxes.transformedBy(local);
	}

	public bool jointActive = true;
	public IkJointState jointState = IkJointState.zero;
	public IkJointLimits jointLimits = IkJointLimits.defaultNoLimit;

	public void reset(){
		local = defaultCoord;
	}

	public void update(IkNode parent){
		if (jointActive){
			var defAxes = defaultJointAxes;
			local = defaultCoord.rotatedAround(defAxes.anchor, defAxes.xAxis, jointState.xRot);
		}

		if (parent != null)
			world = parent.world.transformCoord(local);
		else
			world = local;
	}

	public void moveToParentSpace(IkNode parent, bool affectDefault){
		local = parent.world.inverseTransformCoord(world);
		if (affectDefault)
			defaultCoord = parent.world.inverseTransformCoord(defaultCoord);
	}

	public IkNode(Coord coord_, IkJointAxes jointAxes_){
		local = coord_;
		defaultCoord = coord_;
		world = coord_;
		localJointAxes = jointAxes_;
	}		
}


}
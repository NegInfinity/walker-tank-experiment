using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TankExtensions;
using IkSolver;
using ValueDriver;

namespace TankV2{

using static CenterOfMassTools;

public class TankControllerV2: MonoBehaviour{
	public Parts parts = new Parts();
	public DirectControl directControl = new DirectControl();
	public IkControl ikControl = new IkControl();

	public Cinemachine.CinemachineVirtualCamera followCam = null;
	public LayerMask groundMask = -1;

	void drawConnectionLine(Part a, Part b){
		drawConnectionLine(a.obj, b.obj);
	}

	void drawGizmoPoint(Vector3 pos, float size){
		var x = transform.right * size * 0.5f;
		var y = transform.up * size * 0.5f;
		var z = transform.forward * size * 0.5f;
		Gizmos.DrawLine(pos - x, pos + x);
		Gizmos.DrawLine(pos - y, pos + y);
		Gizmos.DrawLine(pos - z, pos + z);
	}

	void drawConnectionLine(GameObject a, GameObject b){
		if (!a || !b)
			return;
		Gizmos.DrawLine(a.transform.position, b.transform.position);
	}

	void drawCenterOfMass(Part part){
		if (!part.rigBody)
			return;
		drawGizmoPoint(part.rigBody.worldCenterOfMass, 0.25f);
	}

	void drawLegGizmo(Leg leg){
		drawConnectionLine(parts.body, leg.hip);
		drawConnectionLine(leg.hip, leg.upper);
		drawConnectionLine(leg.upper, leg.lower);
		drawConnectionLine(leg.lower, leg.tip);

		drawCenterOfMass(leg.hip);
		drawCenterOfMass(leg.upper);
		drawCenterOfMass(leg.lower);
	}

	void drawGizmos(Color c){
		var oldColor = Gizmos.color;
		Gizmos.color = c;

		drawLegGizmo(parts.legLF);
		drawLegGizmo(parts.legRF);
		drawLegGizmo(parts.legLB);
		drawLegGizmo(parts.legRB);

		drawConnectionLine(parts.body, parts.turret);
		drawConnectionLine(parts.turret, parts.barrel);
		drawCenterOfMass(parts.body);
		drawCenterOfMass(parts.turret);
		drawCenterOfMass(parts.barrel);

		var cmData = parts.getCenterOfMass();
		var mass = cmData.Item2;
		var centerOfMass = cmData.Item1;

		if (mass != 0.0f){
			drawGizmoPoint(centerOfMass, 1.0f);
			var massVec = Vector3.Project(centerOfMass - transform.position, Vector3.up);
			var groundMass = centerOfMass - massVec; 
			Gizmos.DrawLine(groundMass, centerOfMass);
			//drawGizmoPoint(groundMass, 1.0f);

			var groundPlane = Vector3.up;
			var projectedCenterOfMass = Vector3.ProjectOnPlane(centerOfMass, groundPlane);
			var projectedLF = Vector3.ProjectOnPlane(parts.legLF.tip.obj.transform.position, groundPlane);
			var projectedRF = Vector3.ProjectOnPlane(parts.legRF.tip.obj.transform.position, groundPlane);
			var projectedLB = Vector3.ProjectOnPlane(parts.legLB.tip.obj.transform.position, groundPlane);
			var projectedRB = Vector3.ProjectOnPlane(parts.legRB.tip.obj.transform.position, groundPlane);

			drawGizmoPoint(projectedCenterOfMass, 1.0f);
			Gizmos.DrawLine(projectedLB, projectedRB);
			Gizmos.DrawLine(projectedLF, projectedRF);
			Gizmos.DrawLine(projectedLB, projectedLF);
			Gizmos.DrawLine(projectedRB, projectedRF);
			Gizmos.DrawLine(projectedLB, projectedRF);
			Gizmos.DrawLine(projectedLF, projectedRB);
		}

		Gizmos.color = oldColor;
	}

	void OnDrawGizmos(){
		drawGizmos(Color.yellow);
	}

	void OnDrawSelected(){
		drawGizmos(Color.white);
	}

	Coroutine controlCoroutineObject = null;

	void Start(){
		gameObject.findTankPart(out parts.turret, "turret");
		gameObject.findTankPart(out parts.barrel, "barrels");
		//if (!findTankPart(out body, "body"))
		parts.body = new Part(gameObject);

		gameObject.findTankLeg(out parts.legRF, true, true, "rf");
		gameObject.findTankLeg(out parts.legLF, true, false, "lf");
		gameObject.findTankLeg(out parts.legRB, false, true, "rb");
		gameObject.findTankLeg(out parts.legLB, false, false, "lb");

		if (ikControl.legRFTarget && parts.legRF.tip.obj)
			ikControl.legRFTarget.transform.position = parts.legRF.tip.obj.transform.position;

		if (ikControl.legRBTarget && parts.legRB.tip.obj)
			ikControl.legRBTarget.transform.position = parts.legRB.tip.obj.transform.position;

		if (ikControl.legLFTarget && parts.legLF.tip.obj)
			ikControl.legLFTarget.transform.position = parts.legLF.tip.obj.transform.position;

		if (ikControl.legLBTarget && parts.legLB.tip.obj)
			ikControl.legLBTarget.transform.position = parts.legLB.tip.obj.transform.position;

		controlCoroutineObject = StartCoroutine(controlCoroutine());
	}

	void applyLegControl(Leg leg, LegControl legControl){
		leg.hip.applyHingeAngle(legControl.legYaw);
		leg.upper.applyHingeAngle(legControl.upperLeg);
		leg.lower.applyHingeAngle(legControl.lowerLeg);
	}


	void applyControl(){
		parts.turret.applyHingeAngle(directControl.turretControlAngle);
		parts.barrel.applyHingeAngle(directControl.barrelControlAngle);
		applyLegControl(parts.legRF, directControl.legControlRF);
		applyLegControl(parts.legLF, directControl.legControlLF);
		applyLegControl(parts.legRB, directControl.legControlRB);
		applyLegControl(parts.legLB, directControl.legControlLB);
	}	

	void solveLegKinematics(Leg leg, LegControl legControl, Vector3 worldTargetPos){
		if (!parts.body.obj || !leg.hip.obj || !leg.upper.obj || !leg.lower.obj || !leg.tip.obj)
			return;

		var origBodyCoord = Coord.fromTransform(parts.body.defaultTransform);
		var worldBodyCoord = Coord.fromTransform(parts.body.obj.transform, false);
		var hipCoord = Coord.fromTransform(leg.hip.defaultTransform);
		var upperCoord = Coord.fromTransform(leg.upper.defaultTransform);
		var lowerCoord = Coord.fromTransform(leg.lower.defaultTransform);
		var tipCoord = Coord.fromTransform(leg.tip.defaultTransform);

		hipCoord = origBodyCoord.inverseTransformCoord(hipCoord);
		upperCoord = origBodyCoord.inverseTransformCoord(upperCoord);
		lowerCoord = origBodyCoord.inverseTransformCoord(lowerCoord);
		tipCoord = origBodyCoord.inverseTransformCoord(tipCoord);

		var hipNode = new IkNode(hipCoord, leg.hip.hinge);
		var upperNode = new IkNode(upperCoord, leg.upper.hinge);
		var lowerNode = new IkNode(lowerCoord, leg.lower.hinge);
		var tipNode = new IkNode(tipCoord, leg.tip.hinge);

		var nodes = new List<IkNode>();
		nodes.Add(hipNode);
		nodes.Add(upperNode);
		nodes.Add(lowerNode);
		nodes.Add(tipNode);

		for(int i = nodes.Count-1; i > 0; i--){
			nodes[i].moveToParentSpace(nodes[i-1], true);
		}

		var targetPos = worldBodyCoord.inverseTransformPoint(worldTargetPos);
		Solver.solveIkChain(nodes, targetPos, true);

		//updateIkChain(nodes, 0, false, false);
		var debugCoord = worldBodyCoord;
		for(int i = 0; (i + 1) < nodes.Count; i++){
			var node = nodes[i];
			var nextNode = nodes[i+1];
			Debug.DrawLine(
				debugCoord.transformPoint(node.objWorld.pos), 
				debugCoord.transformPoint(nextNode.objWorld.pos)
			);
		}
		/*
		if (nodes.Count > 0){
			Debug.DrawLine(debugCoord.transformPoint(nodes[0].objWorld.pos), debugCoord.transformPoint(targetPos));
			Debug.DrawLine(debugCoord.transformPoint(nodes[nodes.Count-1].objWorld.pos), debugCoord.transformPoint(targetPos));
		}
		*/

		legControl.legYaw = hipNode.jointState.xRotDeg;
		legControl.upperLeg = upperNode.jointState.xRotDeg;
		legControl.lowerLeg = lowerNode.jointState.xRotDeg;
	}

	void solveKinematics(){
		if (ikControl.legRFTarget && ikControl.legRFTarget.gameObject.activeInHierarchy)
			solveLegKinematics(parts.legRF, directControl.legControlRF, ikControl.legRFTarget.position);
		if (ikControl.legRBTarget && ikControl.legRBTarget.gameObject.activeInHierarchy)
			solveLegKinematics(parts.legRB, directControl.legControlRB, ikControl.legRBTarget.position);
		if (ikControl.legLFTarget && ikControl.legLFTarget.gameObject.activeInHierarchy)
			solveLegKinematics(parts.legLF, directControl.legControlLF, ikControl.legLFTarget.position);
		if (ikControl.legLBTarget && ikControl.legLBTarget.gameObject.activeInHierarchy)
			solveLegKinematics(parts.legLB, directControl.legControlLB, ikControl.legLBTarget.position);
	}

	void setRelLegIk(int legIndex, float right, float forward, float height){
		var target = ikControl.getLegIkTarget(legIndex);
		var rightVec = parts.body.obj.transform.right;
		var forwardVec = parts.body.obj.transform.forward;
		var upVec = parts.body.obj.transform.up;
		var pos = parts.body.objWorldPos;

		target.transform.position = pos 
			+ right * rightVec + forward * forwardVec 
			+ upVec * height;
	}

	void setRelLegIk(int legIndex, Vector3 coord){
		setRelLegIk(legIndex, coord.x, coord.z, coord.y);
	}

	void setRelLegIk(Vector3 lf, Vector3 rf, Vector3 lb, Vector3 rb){
		setRelLegIk(0, lf);
		setRelLegIk(1, rf);
		setRelLegIk(2, lb);
		setRelLegIk(3, rb);
	}

	void setRelLegIk(LegRelIk relIk){
		setRelLegIk(relIk.lf, relIk.rf, relIk.lb, relIk.rb);
	}

	Vector3 invRelTransformNoScale(Transform t, Vector3 p){
		var diff = p - t.position;
		return new Vector3(
			Vector3.Dot(diff, t.right),
			Vector3.Dot(diff, t.up),
			Vector3.Dot(diff, t.forward)
		);
	}

	LegRelIk getLegRelIk(bool fromTargets){
		var result = new LegRelIk();
		result.rf = fromTargets ? ikControl.legRFTarget.position: parts.legRF.tip.objWorldPos;
		result.lf = fromTargets ? ikControl.legLFTarget.position: parts.legLF.tip.objWorldPos;
		result.rb = fromTargets ? ikControl.legRBTarget.position: parts.legRB.tip.objWorldPos;
		result.lb = fromTargets ? ikControl.legLBTarget.position: parts.legLB.tip.objWorldPos;

		var bodyT = parts.body.obj.transform;
		result.rf = invRelTransformNoScale(bodyT, result.rf);
		result.rb = invRelTransformNoScale(bodyT, result.rb);
		result.lf = invRelTransformNoScale(bodyT, result.lf);
		result.lb = invRelTransformNoScale(bodyT, result.lb);

		return result;
	}

	float sawFunc(float t){
		t = Mathf.Repeat(t, 1.0f);
		float t0 = 0.25f;
		if (t <= t0)
			return Mathf.Lerp(0.0f, 1.0f, Mathf.Clamp01(t/t0));
		return Mathf.Lerp(1.0f, 0.0f, Mathf.Clamp01((t - t0)/(1.0f - t0)));
	}

	void combineIks(LegRelIk result, LinkedList<LegRelIk> iks, LegRelIk untilIk){
		result.setVec(Vector3.zero);
		bool first = true;
		foreach(var cur in iks){
			if (cur == untilIk)
				break;
			if (first){
				result.assign(cur);
				first = false;
			}
			else
				result.addIk(cur);
		}
	}

	public GaitGenerator gaitGenerator = new GaitGenerator();

	IEnumerator walk01(){
		var legIk = getLegRelIk(false);

		var sideOffset = MathTools.seq(legIk.rf, legIk.rb, legIk.lf, legIk.lb).Select(v => Mathf.Abs(v.x)).Average();
		var forwardOffset = MathTools.seq(legIk.rf, legIk.lf).Select(v => v.z).Average();
		var backOffset = MathTools.seq(legIk.rb, legIk.lb).Select(v => v.z).Average();
		var height = MathTools.seq(legIk.rf, legIk.rb, legIk.lf, legIk.lb).Select(v => v.y).Average();

		legIk = new LegRelIk(sideOffset, forwardOffset, backOffset, height);
		LinkedList<LegRelIk> iks = new LinkedList<LegRelIk>();

		var baseIk = legIk;
		iks.AddLast(baseIk);

		var heightIk = new LegRelIk();
		iks.AddLast(heightIk);

		var extendIk = new LegRelIk();
		iks.AddLast(extendIk);

		var circleIk = new LegRelIk();
		iks.AddLast(circleIk);

		var legRaiseIk = new LegRelIk();
		iks.AddLast(legRaiseIk);

		var legMoveIk = new LegRelIk();
		iks.AddLast(legMoveIk);

		setRelLegIk(legIk);

		var combinedIk = new LegRelIk();
		System.Action updRelIk = () => {
			combineIks(combinedIk, iks, null);
			setRelLegIk(combinedIk);
		};

		yield return new WaitForSeconds(1.0f);

		yield return heightIk.shiftY(-5.0f, 2.0f, updRelIk);

		circleIk.weight = 0.0f;

		int[] quadrantToLeg = new int[]{
			1, 0, 2, 3
		};

		float rx = 1.0f;
		float rz = 1.0f;

		float stepHeight = 3.0f;

		circleIk.weight = 0.0f;
		legRaiseIk.weight = 0.0f;

		int lastQuadrant = -1;

		Vector3[] legPrev = new Vector3[gaitGenerator.numSectors];
		Vector3[] legNext = new Vector3[gaitGenerator.numSectors];
		bool[] legNextFlags = new bool[gaitGenerator.numSectors];
		float[] lastSawValue = new float[gaitGenerator.numSectors];
		for(int i = 0; i < gaitGenerator.numSectors; i++){
			lastSawValue[i] = 0.0f;
			legPrev[i] = legNext[i] = Vector3.zero;
			legNextFlags[i] = false;
		}

		float stepVal = 3.0f;
		float extValX = 2.0f;
		float extValZ = 2.0f;
		circleIk.weight = 0.0f;
		legRaiseIk.weight = 0.0f;
		while(true){
			circleIk.setVec(gaitGenerator.circleX * rx, 0.0f, gaitGenerator.circleY * rz);
			for(int secIndex = 0; secIndex < gaitGenerator.numSectors; secIndex++){
				int legIndex = quadrantToLeg[secIndex];
				legRaiseIk.setVec(legIndex, 0.0f, gaitGenerator.raisePulses[secIndex] * stepHeight, 0.0f);
			}
			if (circleIk.weight < 1.0f){
				circleIk.weight = Mathf.Clamp01(circleIk.weight + Time.deltaTime * 0.5f);
				updRelIk();
				yield return null;
				continue;
			}
			if (legRaiseIk.weight < 1.0f){
				legRaiseIk.weight = Mathf.Clamp01(legRaiseIk.weight + Time.deltaTime * 0.5f);
				updRelIk();
				yield return null;
				continue;
			}

			for(int secIndex = 0; secIndex < gaitGenerator.numSectors; secIndex++){
				int legIndex = quadrantToLeg[secIndex];
				//legRaiseIk.setVec(legIndex, 0.0f, gaitGenerator.raisePulses[secIndex] * stepHeight, 0.0f);
				//legRaiseIk.setVec(legIndex, 0.0f, gaitGenerator.lerpPulses[secIndex] * stepHeight, 0.0f);
				var sawValue = gaitGenerator.lerpPulses[secIndex];

				bool leftLeg = ((legIndex & 0x1) == 0);
				bool frontLeg= legIndex < 2;

				var legBaseCoord = new Vector3(
					(leftLeg ? -extValX: extValX),
					0.0f, 
					(frontLeg ? extValZ: -extValZ)
				);

				if ((lastSawValue[legIndex] < 0.0f) && (sawValue >= 0.0f)){
					if (!legNextFlags[legIndex])
						legPrev[legIndex] = legNext[legIndex];
					else{
						legPrev[legIndex] = legBaseCoord - Vector3.forward * stepVal;
					}
				}

				if ((lastSawValue[legIndex] > 0.0f) && (sawValue <= 0.0f)){
					legNext[legIndex] = legBaseCoord + Vector3.forward * stepVal;
					legNextFlags[legIndex] = true;
				}

				var lerpFactor = Mathf.Repeat(sawValue + 1.0f, 1.0f);
				var legValue = (sawValue < 0.0f) ?  
					Vector3.Lerp(legPrev[legIndex], legNext[legIndex], lerpFactor):
					Vector3.Lerp(legNext[legIndex], legPrev[legIndex], lerpFactor);
				
				legMoveIk.setVec(legIndex, legValue);

				lastSawValue[legIndex] = sawValue;
			}
			if (lastQuadrant != gaitGenerator.currentSector){
				lastQuadrant = gaitGenerator.currentSector;
			}

			updRelIk();
			yield return null;
		}
	}

	IEnumerator walk02(){
		var legIk = getLegRelIk(false);

		var sideOffset = MathTools.seq(legIk.rf, legIk.rb, legIk.lf, legIk.lb).Select(v => Mathf.Abs(v.x)).Average();
		var forwardOffset = MathTools.seq(legIk.rf, legIk.lf).Select(v => v.z).Average();
		var backOffset = MathTools.seq(legIk.rb, legIk.lb).Select(v => v.z).Average();
		var legHeight = MathTools.seq(legIk.rf, legIk.rb, legIk.lf, legIk.lb).Select(v => v.y).Average();

		legIk = new LegRelIk(sideOffset, forwardOffset, backOffset, legHeight);
		LinkedList<LegRelIk> iks = new LinkedList<LegRelIk>();

		var baseIk = legIk;
		iks.AddLast(baseIk);

		var legExtendIk = new LegRelIk();
		iks.AddLast(legExtendIk);

		var liftIk = new LegRelIk();
		iks.AddLast(liftIk);

		var legRaiseIk = new LegRelIk();
		iks.AddLast(legRaiseIk);

		var combinedIk = new LegRelIk();
		System.Action updRelIk = () => {
			combineIks(combinedIk, iks, null);
			setRelLegIk(combinedIk);
		};


		yield return new WaitForSeconds(1.0f);
		var extX = 2.5f;
		var extZ = 1.5f;
		legExtendIk.addVec(0, -extX, 0, extZ);
		legExtendIk.addVec(1, extX, 0, extZ);
		legExtendIk.addVec(2, -extX, 0, -extZ);
		legExtendIk.addVec(3, extX, 0, -extZ);
		legExtendIk.weight = 0.0f;

		var zeroPose = new BodyPose();
		zeroPose.legAddRel(0, -sideOffset, 0.0f, forwardOffset);
		zeroPose.legAddRel(1, sideOffset, 0.0f, forwardOffset);
		zeroPose.legAddRel(2, -sideOffset, 0.0f, backOffset);
		zeroPose.legAddRel(3, sideOffset, 0.0f, backOffset);		
		zeroPose.bodyPos = new Vector3(0.0f, -legHeight, 0.0f);

		var refPose = zeroPose.clone();		
		refPose.bodyAddRel(0.0f, 5.0f, 0.0f);
		refPose.legAddRel(0, -extX, 0, extZ);
		refPose.legAddRel(1, extX, 0, extZ);
		refPose.legAddRel(2, -extX, 0, -extZ);
		refPose.legAddRel(3, extX, 0, -extZ);

		yield return liftIk.shiftY(1.0f, 1.0f, updRelIk);

		yield return legExtendIk.driveWeight(1.0f, 1.0f, updRelIk);

		yield return liftIk.shiftY(-6.0f, 3.0f, updRelIk);

		if (followCam)
			followCam.Priority += 10;

		var startPose = refPose.clone();//getBodyPose();
		var endPose = startPose.clone();
		endPose.vecAddRel(0.0f, 0.0f, 5.0f);
		var lerpPose = startPose.clone();
		var worldPose = new BodyPose();

		var relPose = new LegRelIk();
		int[] legRemap = new int[]{0, 1, 2, 3};
		float legRaiseHeight = 2.0f;
		float heading = 0.0f;
		while(true){
			float t = 0;
			float duration = 4.0f;

			var move2D = new Vector2(
				Input.GetAxis("Horizontal"),
				Input.GetAxis("Vertical")
			);
			if (move2D.magnitude > 1.0f)
				move2D.Normalize();

			var move3d = Vector3.zero;
			if (followCam){
				var camForward = Vector3.ProjectOnPlane(followCam.transform.forward, Vector3.up).normalized;
				var camRight = Vector3.ProjectOnPlane(followCam.transform.right, Vector3.up).normalized;
				camRight = Vector3.ProjectOnPlane(camRight, camForward).normalized;
				Debug.Log($"cam forward: {camForward}");
				Debug.Log($"cam right: {camRight}");

				move3d += move2D.x * camRight;
				move3d += move2D.y * camForward;

				var bodyTransform = parts.body.obj.transform;
				var bodyForward = Vector3.ProjectOnPlane(bodyTransform.forward, Vector3.up).normalized;
				var bodyLeft = Vector3.ProjectOnPlane(-bodyTransform.right, Vector3.up);
				bodyLeft = Vector3.ProjectOnPlane(bodyLeft, bodyForward).normalized;

				var x = Vector3.Dot(bodyForward, camForward);
				var y = Vector3.Dot(bodyLeft, camForward);

				var angle = Mathf.Atan2(y, x)*Mathf.Rad2Deg;
				angle = Mathf.Clamp(angle, -45.0f, 45.0f);
				if (Mathf.Abs(angle) > 1.0f){
					heading -= angle;
				}
			}
			heading = Mathf.Repeat(heading, 360.0f);

			var moveLen = 5.0f;
			Vector3 deltaMove = move3d * moveLen;//new Vector3(move2D.x * moveLen, 0.0f, move2D.y * moveLen);//new Vector3(0.0f, 0.0f, 5.0f);
			parts.getBodyPose(worldPose);
			startPose.assign(refPose);
			startPose.moveTo(worldPose.bodyPos);
			startPose.rotateAroundBody(heading, Vector3.up);
			endPose.assign(startPose);
			startPose.assign(worldPose);
			endPose.vecAddWorld(deltaMove);

			startPose.adjustPositionsToWorld(groundMask, Vector3.up, legRaiseHeight * 1.5f, legRaiseHeight * 2.5f, 5.0f);
			endPose.adjustPositionsToWorld(groundMask, Vector3.up, legRaiseHeight * 1.5f, legRaiseHeight * 2.5f, 5.0f);

			while(t < duration){
				t += Time.deltaTime;
				float relT = Mathf.Clamp01(t/duration);
				var bodyLerp = relT;
				//var bodyLerp = -Mathf.Cos(Mathf.PI * relT) * 0.5f + 0.5f;
				lerpPose.assign(startPose);
				lerpPose.bodyPos = Vector3.Lerp(startPose.bodyPos, endPose.bodyPos, bodyLerp);
				lerpPose.bodyRot = Quaternion.Lerp(startPose.bodyRot, endPose.bodyRot, bodyLerp);

				var legT = bodyLerp * 4.0f;
				var legRelT = Mathf.Repeat(legT, 1.0f);
				/*
				https://easings.net/#easeInSine
				function easeInSine(x: number): number {
 					 return 1 - cos((x * PI) / 2);
				}
				*/
				var legLerp = Mathf.Clamp01(
					//legRelT * legRelT
					1.0f - Mathf.Cos((legRelT * Mathf.PI) * 0.5f)
				);
				var currentLeg = Mathf.Clamp(Mathf.FloorToInt(legT), 0, 3);
				int numLegs = 4;
				var raisePulse = Mathf.Sin(Mathf.PI * legRelT);

				for(int i = 0; i < currentLeg; i++){
					lerpPose.setLeg(
						legRemap[i], 
						endPose.getLeg(legRemap[i])
					);
				}
				lerpPose.setLeg(
					legRemap[currentLeg], 
					Vector3.Lerp(
						startPose.getLeg(legRemap[currentLeg]), 
						endPose.getLeg(legRemap[currentLeg]), 
						legLerp
					)
				);
				lerpPose.legAddWorld(legRemap[currentLeg], Vector3.up * legRaiseHeight * raisePulse);
				for(int i = currentLeg + 1; i < numLegs; i++){
					lerpPose.setLeg(
						legRemap[i], 
						startPose.getLeg(legRemap[i])
					);
				}

				lerpPose.getRelIk(relPose);
				setRelLegIk(relPose);
				yield return null;
			}

		}

		yield break;
	}


	IEnumerator controlCoroutine(){
		yield return walk02();
	}

	void Update(){
		gaitGenerator.update();
		solveKinematics();
		applyControl();		
	}
}

}
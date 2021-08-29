using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TankExtensions;
using IkSolver;
using ValueDriver;

namespace TankV2{

[System.Serializable]
public class GaitGenerator{
	public int numSectors{
		get => 4;
	}
	public float timer = 0.0f;
	public float period = 4.0f;
	public float relT = 0.0f;
	public float relSectorT = 0.0f;
	public float angleDeg = 0.0f;
	public float angleRad = 0.0f;
	public int currentSector = 0;
	public float[] raisePulses = new float[0];
	public float[] lerpPulses = new float[0];
	public float circleX = 0.0f;
	public float circleY = 0.0f;

	public float saw(float t, int numSectors){
		var sectorDur = 1.0f / (float)numSectors;
		t = Mathf.Repeat(t, 1.0f);
		if (t < sectorDur)
			return Mathf.Lerp(-1.0f, 0.0f, t/sectorDur);
		return Mathf.Lerp(0.0f, 1.0f, (t - sectorDur)/(1.0f - sectorDur));
	}

	public void update(){
		if ((raisePulses?.Length ?? 0) != numSectors)
			raisePulses = new float[numSectors];
		if ((lerpPulses?.Length ?? 0) != numSectors)
			lerpPulses = new float[numSectors];

		float sectorDur = 1/(float)numSectors;

		timer += Time.deltaTime;
		timer = Mathf.Repeat(timer, period);
		relT = timer/period;

		angleDeg = Mathf.Lerp(0.0f, 360.0f, relT);
		angleRad = angleDeg * Mathf.Deg2Rad;
		circleX = Mathf.Cos(angleRad);
		circleY = Mathf.Sin(angleRad);

		var pulseAngle = angleRad * 0.5f * (float)numSectors;
		var pulseValue = Mathf.Abs(Mathf.Sin(pulseAngle));
		
		relSectorT = Mathf.Repeat(relT/sectorDur, 1.0f);
		currentSector = Mathf.Clamp(Mathf.FloorToInt(relT/sectorDur), 0, numSectors - 1);

		for(int sectorIndex = 0; sectorIndex < numSectors; sectorIndex++){
			raisePulses[sectorIndex] = (sectorIndex == currentSector) ? pulseValue : 0.0f;
			lerpPulses[sectorIndex] = saw(relT - sectorDur * (float)sectorIndex, numSectors);
		}
	}
}

}
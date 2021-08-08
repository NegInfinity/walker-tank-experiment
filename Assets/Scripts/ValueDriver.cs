using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ValueDriver{

public static class ValDriver{
	public static IEnumerator driveFloat(float startVal, float add, float time, System.Action<float> onChange, System.Action<float> onFinish = null){
		float timer = 0.0f;
		float speed = add/time;
		while(timer < time){
			var val = startVal + Mathf.Clamp01(timer/time)*add;
			timer += Time.deltaTime;
			onChange(val);
			yield return null;
		}
		onChange(startVal + add);
		if (onFinish != null){
			onFinish(startVal + add);
		}
	}

	public static IEnumerator driveVector(Vector3 startVal, Vector3 add, float time, System.Action<Vector3> onChange, System.Action<Vector3> onFinish = null){
		float timer = 0.0f;
		var speed = add/time;
		while(timer < time){
			var val = startVal + Mathf.Clamp01(timer/time)*add;
			timer += Time.deltaTime;
			onChange(val);
			yield return null;
		}
		onChange(startVal + add);
		if (onFinish != null){
			onFinish(startVal + add);
		}
	}
}

}
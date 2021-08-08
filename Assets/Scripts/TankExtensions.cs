using UnityEngine;

namespace TankExtensions{
public static class Extensions{
	public static GameObject findObjectWithName(this GameObject parent, string name){
		if (parent.name == name)
			return parent;
		foreach(Transform cur in parent.transform){
			if (cur.gameObject.name.ToLower() == name)
				return cur.gameObject;
			GameObject tmpResult = findObjectWithName(cur.gameObject, name);
			if (tmpResult)
				return tmpResult;
		}
		return null;
	}

	public static GameObject findObjectWithLowerName(this GameObject parent, string name){
		if (parent.name.ToLower() == name)
			return parent;
		foreach(Transform cur in parent.transform){
			if (cur.gameObject.name.ToLower() == name)
				return cur.gameObject;
			GameObject tmpResult = findObjectWithLowerName(cur.gameObject, name);
			if (tmpResult)
				return tmpResult;
		}
		return null;
	}
}

}
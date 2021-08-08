using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RigidbodyHelpers{
	public static class Extensions{
		public static Val getValueOrDefault<Key, Val>(this Dictionary<Key, Val> dict, Key key, Val defVal = default){
			Val result;
			if (!dict.TryGetValue(key, out result)){
				return defVal;
			}
			return result;
		}

		public static Comp getOrCreateComponent<Comp>(this GameObject obj, System.Action<Comp> onInit = null) where Comp: Component{
			Comp result = obj.GetComponent<Comp>();
			if (!result){
				result = obj.AddComponent<Comp>();
				if (onInit != null)
					onInit(result);
			}
			return result;
		}
	}
}

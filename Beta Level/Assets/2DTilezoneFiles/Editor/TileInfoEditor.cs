using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TileInfo))]
public class TileInfoEditor : Editor {

	void OnEnable () {
		TileInfo ti = (TileInfo)serializedObject.targetObject;
		if( ti.GetComponent<MeshFilter>().sharedMesh == null ) {
			ti.mapHasChanged = true;
			ti.UpdateVisualMesh( true );
		}
	}
}

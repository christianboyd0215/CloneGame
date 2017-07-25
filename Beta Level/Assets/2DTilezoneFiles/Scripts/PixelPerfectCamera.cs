using UnityEngine;
using System.Collections;

[AddComponentMenu("2D/Pixel Perfect Camera")]
public class PixelPerfectCamera : MonoBehaviour {
	
	public float pixelsPerUnit = 16;
	static float _pixelsPerUnit = 16;
	public int zoomFactor = 1;
	public static int _zoomFactor;
	[Range(-2, 2)]public int offsetNumber;
	public bool debugMode;

	Vector3 offSet;
	
	void Start () {
		GetComponent<Camera>().orthographicSize = (float)Screen.height / 2f / pixelsPerUnit;
		_pixelsPerUnit = pixelsPerUnit;
		_zoomFactor = zoomFactor;
		if( zoomFactor > 1 )
			GetComponent<Camera>().orthographicSize /= zoomFactor;

		if( transform.parent != null )
			offSet = transform.position - transform.parent.position;
	}
	
	void LateUpdate () {
		if( transform.parent != null )
			transform.position = transform.parent.position + offSet;
		//make sure this is called after the camera has moved
		SnapCam();
	}
	
	public void SnapCam ( ) {
		Vector3 newPos = transform.position;
		newPos.x =  (Mathf.RoundToInt(newPos.x*pixelsPerUnit*zoomFactor) + ((float)offsetNumber/4) ) / (_pixelsPerUnit*zoomFactor);
		newPos.y =  (Mathf.RoundToInt(newPos.y*pixelsPerUnit*zoomFactor) + 0.0f) / (pixelsPerUnit*zoomFactor);
		transform.position = newPos;
	}

	public static void SnapToPix ( Transform transform ) {
		Vector3 newPos = transform.position;
		newPos.x =  Mathf.RoundToInt(newPos.x*_pixelsPerUnit * _zoomFactor) / (_pixelsPerUnit * _zoomFactor);
		newPos.y =  Mathf.RoundToInt(newPos.y*_pixelsPerUnit * _zoomFactor) / (_pixelsPerUnit * _zoomFactor);
		transform.position = newPos;
	}

	void OnGUI () {
		if( debugMode ) {
			GUILayout.Label( "Offset Number: " + offsetNumber );
			offsetNumber = (int)GUILayout.VerticalSlider( offsetNumber, -2, 2 );
		}
	}
}

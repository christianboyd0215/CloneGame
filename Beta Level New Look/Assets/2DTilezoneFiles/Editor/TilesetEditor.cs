using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
//using UnityEditorInternal;

public class TilesetEditor : EditorWindow {
	public enum TileTool {
		None,
		Draw,
		Erase,
		Box,
		Rotate90,
		Flip,
		FloodFill,
		Collisions
	};

	static int sheetWidth {
		get {
			return Mathf.RoundToInt((float)(mat.mainTexture.width-offsetX) / (tileSize + spacing));
		}
	}

	static int sheetHeight {
		get {
			return Mathf.RoundToInt((float)(mat.mainTexture.height-offsetY) / (tileSize + spacing));
		}
	}

	static int tileSize = 16;
	static int spacing = 0;
	static int offsetX = 0;
	static int offsetY = 0;
	public static int TileSize { get { return tileSize; } }
	public static int Spacing { get { return spacing; } }
	public static int OffsetX { get { return offsetX; } }
	public static int OffsetY { get { return offsetY; } }
	static Color backgroundColor = new Color( 194f / 255f, 194f / 255f, 194f / 255f );
	[SerializeField]
	static Material mat;
	public static Material Mat {
		get {
			return mat;
		}
		set {
			mat = value;
		}
	}

	static Vector2 fullSheetScroll;
	static Vector2 sideControlsScroll;
	Vector2 mousePos;
	bool mouseDownScene = false;
	Vector2 lastPosScene;
	Vector2 lastPosWindow;
	public static Tile[,] selectedTiles = { { new Tile(0, 0) } };
	static GameObject layerToEdit;
	static TileInfo layerTileInfo;

	public static TileInfo LayerTileInfo {
		get {
			return layerTileInfo;
		}
	}

	public static TileTool toolSelected;
	static float zoomScale = 1f;
	public static int autoTileSelected = -1;
	public static Vector2 posToCreateMesh;

	static SerializedObject meshRenderer;
	static SerializedProperty meshRendererSortingLayer;
	static SerializedProperty meshRendererSortingOrder;
	int meshRendererSelectedLayer;

	public static KeyCode noneShortcut = KeyCode.Alpha1;
	public static KeyCode drawShortcut = KeyCode.Alpha2;
	public static KeyCode eraseShortcut = KeyCode.Alpha3;
	public static KeyCode boxShortcut = KeyCode.Alpha4;
	public static KeyCode rotateShortcut = KeyCode.Alpha5;
	public static KeyCode flipShortcut = KeyCode.Alpha6;
	public static KeyCode fillShortcut = KeyCode.Alpha7;
	public static KeyCode collisionsShortcut = KeyCode.Alpha8;

	static Texture _autoTileGuide;
	List<int> autoTileY = new List<int>();

	static public Texture autoTileGuide {
		get {
			if (_autoTileGuide == null)
				_autoTileGuide = Resources.Load<Texture>("Tileguide");
			return _autoTileGuide;
		}
	}

	static Texture _gridIcon;
	static public Texture gridIcon {
		get {
			if (_gridIcon == null)
				_gridIcon = Resources.Load<Texture>("grid");
			return _gridIcon;
		}
	}
	static GUIContent[] _toolIcons;

	public static GUIContent[] toolIcons {
		get {
			if (_toolIcons == null) {
				_toolIcons = new GUIContent[]
				{
					new GUIContent("X", "None"),
					EditorGUIUtility.IconContent ("TerrainInspector.TerrainToolSplat", "Draw Tile" ),
					new GUIContent(Resources.Load<Texture>( "Eraser" ), "Erase Tile" ),
					new GUIContent(Resources.Load<Texture>( "Box" ), "Box Tool" ),
					EditorGUIUtility.IconContent ("RotateTool", "Rotate Tile"),
					new GUIContent(Resources.Load<Texture>( "Flip" ), "Flip Tile" ),
					new GUIContent(Resources.Load<Texture>( "floodfill" ), "Fill Area" ),
					new GUIContent(Resources.Load<Texture>( "Collisions" ), "Set up collisions" ),
				};
			}
			return _toolIcons;
		}
	}

	[MenuItem("Window/Tileset Editor")]
	static void Init() {
		// Get existing open window or if none, make a new one:
		TilesetEditor window = (TilesetEditor)EditorWindow.GetWindow(typeof(TilesetEditor));
		window.Show();
	}

	void OnUndoRedo() {
		if (layerTileInfo != null) {
			layerTileInfo.mapHasChanged = true;
			layerTileInfo.UpdateVisualMesh( true );

		}
		Repaint();
	}

	void OnFocus() {
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		SceneView.onSceneGUIDelegate += this.OnSceneGUI;

		Undo.undoRedoPerformed -= this.OnUndoRedo;
		Undo.undoRedoPerformed += this.OnUndoRedo;
	}

	void OnDestroy() {
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		Undo.undoRedoPerformed -= this.OnUndoRedo;
	}

	void OnEnable() {
		tileSize = EditorPrefs.GetInt("2DTilezoneTileSize", 16);
		spacing = EditorPrefs.GetInt("2DTilezoneSpacing", 0);
		offsetX = EditorPrefs.GetInt("2DTilezoneOffsetX", 0);
		offsetY = EditorPrefs.GetInt("2DTilezoneOffsetY", 0);
		zoomScale = EditorPrefs.GetFloat("2DTilezoneZoom", 1f);
		fullSheetScroll.x = EditorPrefs.GetFloat("2DTilezoneScrollX", 0);
		fullSheetScroll.y = EditorPrefs.GetFloat("2DTilezoneScrollY", 0);
		toolSelected = (TileTool)EditorPrefs.GetInt("2DTilezoneToolSelected", 0);
		layerToEdit = GameObject.Find(EditorPrefs.GetString("2DTilezoneLayerName"));

		noneShortcut = (KeyCode)EditorPrefs.GetInt("2DTilezoneNoneShortcut");
		drawShortcut = (KeyCode)EditorPrefs.GetInt("2DTilezoneDrawShortcut");
		eraseShortcut = (KeyCode)EditorPrefs.GetInt("2DTilezoneEraseShortcut");
		boxShortcut = (KeyCode)EditorPrefs.GetInt("2DTilezoneBoxShortcut");
		rotateShortcut = (KeyCode)EditorPrefs.GetInt("2DTilezoneRotateShortcut");
		flipShortcut = (KeyCode)EditorPrefs.GetInt("2DTilezoneFlipShortcut");
		fillShortcut = (KeyCode)EditorPrefs.GetInt("2DTilezoneFillShortcut");
		collisionsShortcut = (KeyCode)EditorPrefs.GetInt("2DTilezoneCollisionsShortcut");

		mat = (Material)AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("2DTilzoneMatPath"), typeof(Material));
		OnSelectionChange();
		if (layerToEdit == null)
			return;
		layerTileInfo = layerToEdit.GetComponent<TileInfo>();
		if (layerTileInfo == null)
			return;
		if (mat == null || mat.mainTexture == null && layerTileInfo.meshRenderer != null)
			mat = layerTileInfo.meshRenderer.sharedMaterial;
		InitLayerToEdit();
	}
		
	static void SetWireframeHidden ( bool isHidden ) {
		if( layerTileInfo == null )
			return;
		EditorUtility.SetSelectedWireframeHidden( layerTileInfo.meshRenderer, isHidden );
	}

	static void InitLayerToEdit () {
		SetWireframeHidden( toolSelected != TileTool.None );
		meshRenderer = new SerializedObject( layerToEdit.GetComponent<MeshRenderer>() );
		meshRendererSortingLayer = meshRenderer.FindProperty( "m_SortingLayerID" );
		meshRendererSortingOrder = meshRenderer.FindProperty( "m_SortingOrder" );
		//		meshRenderer = layerToEdit.GetComponent<MeshRenderer>();
		layerTileInfo.knownTilePoints = new TileInfo.TilePolygonPoints[layerTileInfo.width,layerTileInfo.height];
	}

	void OnDisable() {
		if( layerToEdit != null )
			SetWireframeHidden( false );
		EditorPrefs.SetInt("2DTilezoneTileSize", tileSize);
		EditorPrefs.SetInt("2DTilezoneSpacing", spacing);
		EditorPrefs.SetInt("2DTilezoneOffsetX", offsetX);
		EditorPrefs.SetInt("2DTilezoneOffsetY", offsetY);
		EditorPrefs.SetFloat("2DTilezoneZoom", zoomScale);
		EditorPrefs.SetFloat("2DTilezoneScrollX", fullSheetScroll.x);
		EditorPrefs.SetFloat("2DTilezoneScrollY", fullSheetScroll.y);
		EditorPrefs.SetInt("2DTilezoneToolSelected", (int)toolSelected);

		EditorPrefs.SetInt("2DTilezoneNoneShortcut", (int)noneShortcut );
		EditorPrefs.SetInt("2DTilezoneDrawShortcut", (int)drawShortcut );
		EditorPrefs.SetInt("2DTilezoneEraseShortcut", (int)eraseShortcut );
		EditorPrefs.SetInt("2DTilezoneBoxShortcut", (int)boxShortcut );
		EditorPrefs.SetInt("2DTilezoneRotateShortcut", (int)rotateShortcut );
		EditorPrefs.SetInt("2DTilezoneFlipShortcut", (int)flipShortcut );
		EditorPrefs.SetInt("2DTilezoneFillShortcut", (int)fillShortcut );
		EditorPrefs.SetInt("2DTilezoneCollisionsShortcut", (int)collisionsShortcut );

		if (layerToEdit != null) {
			EditorPrefs.SetString("2DTilezoneLayerName", layerToEdit.name);
		}
		else
			EditorPrefs.SetString("2DTilezoneLayerName", "");

		if (mat != null)
			EditorPrefs.SetString("2DTilzoneMatPath", AssetDatabase.GetAssetPath(mat));
		else
			EditorPrefs.SetString("2DTilzoneMatPath", "");
	}

	public static void ChangeTileLayer( TileInfo newTileLayer ) {
		if( newTileLayer == null )
			return;
		if( layerToEdit != null )
			SetWireframeHidden( false );
		layerToEdit = newTileLayer.gameObject;
		layerTileInfo = newTileLayer;
		tileSize = newTileLayer.tileSize;
		offsetX = newTileLayer.offsetX;
		offsetY = newTileLayer.offsetY;
		spacing = newTileLayer.spacing;
		mat = newTileLayer.meshRenderer.sharedMaterial;
		InitLayerToEdit();
		selectedTiles = new Tile[,] { { new Tile( 0, 0 ) } };
	}

	void OnSelectionChange() {
		if ( Selection.activeGameObject != null && PrefabUtility.GetPrefabType( Selection.activeGameObject ) != PrefabType.Prefab ) {
			TileInfo ti = Selection.activeGameObject.GetComponent<TileInfo>();
			if (ti != null) {
				if( layerToEdit != null )
					SetWireframeHidden( false );
				layerToEdit = ti.gameObject;
				layerTileInfo = ti;
				tileSize = ti.tileSize;
				offsetX = ti.offsetX;
				offsetY = ti.offsetY;
				spacing = ti.spacing;
				mat = ti.meshRenderer.sharedMaterial;
				InitLayerToEdit();
				selectedTiles = new Tile[,] { { new Tile( 0, 0 ) } };
				Repaint();
			}
		}
	}

	public static void CreateMesh( Rect pos, string goName ) {
		GameObject result;

		result = new GameObject(goName);
		result.transform.position = new Vector3( Mathf.Ceil( pos.x - pos.width / 2 ), Mathf.Ceil( pos.y - pos.height / 2 ), 0);
		result.AddComponent<MeshFilter>().sharedMesh = new Mesh();
		result.AddComponent<MeshRenderer>().material = mat;
		TileInfo ti = result.AddComponent<TileInfo>();
		ti.collisions = new TileInfo.CollisionType[(sheetWidth) * (sheetHeight)];
		for (int i = 0; i < (sheetWidth) * (sheetHeight); i++) {
			ti.collisions[i] = TileInfo.CollisionType.Full;
		}
		ti.tileSize = tileSize;
		ti.offsetX = offsetX;
		ti.offsetY = offsetY;
		ti.spacing = spacing;
		//result.GetComponent<TileInfo>().positionAtLastEdit = pos;
		ti.mapWidth = (int)pos.width;
		ti.mapHeight = (int)pos.height;
		ti.tiles = new Tile[(int)pos.width * (int)pos.height];
		for (int i = 0; i < ti.tiles.Length; i++) {
			ti.tiles[i] = Tile.empty;
		}

		if (layerTileInfo != null) {
			ti.collisions = new TileInfo.CollisionType[layerTileInfo.collisions.Length];
			for (int i = 0; i < layerTileInfo.collisions.Length; i++) {
				ti.collisions[i] = layerTileInfo.collisions[i];
			}
		}

		Undo.RegisterCreatedObjectUndo(result, "Create Object");
		layerToEdit = result;
		layerTileInfo = ti;
		InitLayerToEdit();
		Selection.activeGameObject = result;
		//return ti;
	}

	public void DrawBox(Vector2 position) {
		Handles.DrawLine(new Vector3(position.x, position.y, 0), new Vector3(position.x + 1, position.y, 0));
		Handles.DrawLine(new Vector3(position.x + 1, position.y, 0), new Vector3(position.x + 1, position.y + 1, 0));
		Handles.DrawLine(new Vector3(position.x + 1, position.y + 1, 0), new Vector3(position.x, position.y + 1, 0));
		Handles.DrawLine(new Vector3(position.x, position.y + 1, 0), new Vector3(position.x, position.y, 0));
	}

	public void DrawBox(Vector2 bottomLeft, Vector2 topRight) {
		Handles.DrawLine(new Vector3(bottomLeft.x, bottomLeft.y, 0), new Vector3(topRight.x, bottomLeft.y, 0));
		Handles.DrawLine(new Vector3(topRight.x, bottomLeft.y, 0), new Vector3(topRight.x, topRight.y, 0));
		Handles.DrawLine(new Vector3(topRight.x, topRight.y, 0), new Vector3(bottomLeft.x, topRight.y, 0));
		Handles.DrawLine(new Vector3(bottomLeft.x, topRight.y, 0), new Vector3(bottomLeft.x, bottomLeft.y, 0));
	}

	public void DrawTriangle(Rect p, Color c, bool right) {
		for (int y = 0; y < p.height; y++) {
			if (right)
				EditorGUI.DrawRect(new Rect(p.x + p.width - y, y + p.y, y, 1), c);
			else
				EditorGUI.DrawRect(new Rect(p.x, y + p.y, y, 1), c);
		}
	}

	void DrawGrid() {
		for (float x = (offsetX + tileSize) * zoomScale; x < mat.mainTexture.width * zoomScale; x += (tileSize + spacing) * zoomScale) {
			EditorGUI.DrawRect(new Rect(x, 0, Mathf.Max(spacing*zoomScale, 1), mat.mainTexture.height * zoomScale), Color.white);
		}

		for (float y = (offsetY + tileSize) * zoomScale; y < mat.mainTexture.height * zoomScale; y += (tileSize + spacing) * zoomScale) {
			EditorGUI.DrawRect(new Rect(0, y, mat.mainTexture.width * zoomScale, Mathf.Max(spacing*zoomScale, 1)), Color.white);
		}
	}

	void DrawSceneGrid() {
		if (layerTileInfo == null)
			return;
		
		Handles.color = new Color(1, 1, 1, 0.25f);
		
		for (int x = 0; x <= layerTileInfo.mapWidth; x++) {
			Vector3 p1 = layerTileInfo.transform.position;
			p1.x += x*layerTileInfo.zoomFactor;
			
			Vector3 p2 = p1;
			p2.y += layerTileInfo.mapHeight*layerTileInfo.zoomFactor;
			
			Handles.DrawLine(p1, p2);
		}
		
		for (int y = 0; y <= layerTileInfo.mapHeight; y++) {
			Vector3 p1 = layerTileInfo.transform.position;
			p1.y += y*layerTileInfo.zoomFactor;
			
			Vector3 p2 = p1;
			p2.x += layerTileInfo.mapWidth*layerTileInfo.zoomFactor;
			
			Handles.DrawLine(p1, p2);
		}

		Handles.color = new Color(1, 1, 1, 0.75f);
	}

	bool CheckForGridSnap() {
		if (layerTileInfo == null)
			return true;

		//        if (layerTileInfo.tileSize != tileSize || layerTileInfo.spacing != spacing) {
		//            if (EditorUtility.DisplayDialog("Create new layer?",
		//                                            "The tile size selected or spacing is different than what is on the selected layer. To continue with the selected tile size create a new layer.",
		//                                            "Create New Layer",
		//                                            "Cancel")) {
		//                posToCreateMesh = (Vector2)SceneView.lastActiveSceneView.camera.transform.position;
		//                CreateTileLayer.Init();
		//            }
		//            return false;
		//        }

		if (
			layerToEdit.transform.position.x == Mathf.FloorToInt(layerToEdit.transform.position.x)
			&& layerToEdit.transform.position.y == Mathf.FloorToInt(layerToEdit.transform.position.y)
			&& layerToEdit.transform.rotation == Quaternion.identity
		) {

			return true;
		}
		string title = "Align " + layerToEdit + " to grid?";
		string message = "To continue editing " + layerToEdit + " needs to be snapped to the grid.";
		if (EditorUtility.DisplayDialog(title, message, "Snap to grid", "Cancel")) {
			Undo.RecordObject(layerToEdit.transform, "Snap to grid");
			Vector3 snappedPosition = layerToEdit.transform.position;
			snappedPosition.x = Mathf.Round(snappedPosition.x);
			snappedPosition.y = Mathf.Round(snappedPosition.y);
			layerToEdit.transform.position = snappedPosition;
			layerToEdit.transform.rotation = Quaternion.identity;
		}
		mouseDownScene = false;
		return false;

	}

	void SetCursor(Rect pos, bool shift = false) {
		switch (toolSelected) {
		case TileTool.Box:
		case TileTool.Draw:
			if (shift)
				EditorGUIUtility.AddCursorRect(pos, MouseCursor.Text);
			else
				EditorGUIUtility.AddCursorRect(pos, MouseCursor.ArrowPlus);
			break;
		case TileTool.Erase:
			EditorGUIUtility.AddCursorRect(pos, MouseCursor.ArrowMinus);
			break;
		case TileTool.Rotate90:
			EditorGUIUtility.AddCursorRect(pos, MouseCursor.RotateArrow);
			break;
		case TileTool.Flip:
			EditorGUIUtility.AddCursorRect(pos, MouseCursor.SlideArrow);
			break;
		case TileTool.FloodFill:
			EditorGUIUtility.AddCursorRect(pos, MouseCursor.MoveArrow);
			break;
		}
	}

	void AddTile(Vector2 pos, Tile selectedTile) {
		int index = layerTileInfo.WorldPointToMapIndex(pos);
		if (index == -1)
			return;
		if ((layerTileInfo.tiles[index] == selectedTile && autoTileSelected < 0) || ( layerTileInfo.tiles[index].autoTileIndex == autoTileSelected && autoTileSelected >= 0 ))
			return;

		if (selectedTile.autoTileIndex >= layerTileInfo.numberOfAutotiles)
			selectedTile.autoTileIndex = -1;

		Undo.RecordObject(layerTileInfo, "Add Tile");
		if (autoTileSelected == -1)
			layerTileInfo.AddTile(pos, selectedTile);
		else
			layerTileInfo.AddTile(pos, autoTileSelected);
	}

	void FloodFill(Vector2 pos) {
		if (layerTileInfo.WorldPointToMapIndex(pos) == -1)
			return;
		Undo.RecordObject(layerTileInfo, "Flood Fill");
		if (autoTileSelected == -1)
			layerTileInfo.FloodFill(pos, selectedTiles, pos, new Tile(layerTileInfo.tiles[layerTileInfo.WorldPointToMapIndex(pos)]));
		else
			layerTileInfo.FloodFill(pos, autoTileSelected, pos, new Tile(layerTileInfo.tiles[layerTileInfo.WorldPointToMapIndex(pos)]));
	}

	void RemoveTile(Vector2 pos) {
		Undo.RecordObject(layerTileInfo, "Remove Tile");
		layerTileInfo.RemoveTile(pos);
	}

	void RotateTile(Vector2 pos) {
		Undo.RecordObject(layerTileInfo, "Rotate Tile");
		layerTileInfo.RotateTile(pos);
	}

	void FlipTile(Vector2 pos) {
		Undo.RecordObject(layerTileInfo, "Flip Tile");
		layerTileInfo.FlipTile(pos);
	}

	void CheckForKeyboardShortcuts ( Event current ) {
		if( current.type == EventType.keyDown ) {
			if( current.keyCode == noneShortcut )
				toolSelected = TileTool.None;
			if( current.keyCode == drawShortcut )
				toolSelected = TileTool.Draw;
			if( current.keyCode == eraseShortcut )
				toolSelected = TileTool.Erase;
			if( current.keyCode == boxShortcut )
				toolSelected = TileTool.Box;
			if( current.keyCode == rotateShortcut )
				toolSelected = TileTool.Rotate90;
			if( current.keyCode == flipShortcut )
				toolSelected = TileTool.Flip;
			if( current.keyCode == fillShortcut )
				toolSelected = TileTool.FloodFill;
			if( current.keyCode == collisionsShortcut )
				toolSelected = TileTool.Collisions;
			Repaint();
		}
	}

	public void OnSceneGUI(SceneView sceneView) {

		Event current = Event.current;
		Vector2 mousePos = Event.current.mousePosition;
		float zoomFactor = layerTileInfo == null ? 1 : layerTileInfo.zoomFactor;

		SetCursor(sceneView.position, current.shift);

		CheckForKeyboardShortcuts( current );

		if (toolSelected != TileTool.None && toolSelected != TileTool.Collisions ) {

			DrawSceneGrid();



			// This prevents us from selecting other objects in the scene
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			HandleUtility.AddDefaultControl(controlID);

			mousePos.y = sceneView.camera.pixelHeight - mousePos.y;
			mousePos = (Vector2)sceneView.camera.ScreenPointToRay(mousePos).origin;
			if( layerTileInfo == null ) {
				mousePos.x = Mathf.Floor(mousePos.x);
				mousePos.y = Mathf.Floor(mousePos.y);
			}
			else {
				mousePos -= (Vector2)layerTileInfo.transform.position;
				mousePos.x = Mathf.Floor( mousePos.x / zoomFactor ) * zoomFactor;
				mousePos.y = Mathf.Floor( mousePos.y / zoomFactor ) * zoomFactor;
				mousePos += (Vector2)layerTileInfo.transform.position;
			}

			if (mouseDownScene && lastPosScene != mousePos && (toolSelected != TileTool.Draw || current.shift)) {
				Vector2 bottomLeft = lastPosScene;
				Vector2 topRight = mousePos;
				if (mousePos.x < lastPosScene.x) {
					bottomLeft.x = mousePos.x;
					topRight.x = lastPosScene.x;
				}
				if (mousePos.y < lastPosScene.y) {
					bottomLeft.y = mousePos.y;
					topRight.y = lastPosScene.y;
				}
				for (float x = bottomLeft.x; x <= topRight.x; x += zoomFactor) {
					for (float y = bottomLeft.y; y <= topRight.y; y += zoomFactor) {
						DrawBox(new Vector2(x, y), new Vector2(x+zoomFactor, y+zoomFactor));
					}
				}
			}
			else if (selectedTiles.Length > 0) {
				for (int x = 0; x < selectedTiles.GetLength(0); x++) {
					for (int y = 0; y < selectedTiles.GetLength(1); y++) {
						DrawBox(new Vector2(mousePos.x + x*zoomFactor, mousePos.y - y*zoomFactor), new Vector2(mousePos.x + (x+1)*zoomFactor, mousePos.y - (y-1)*zoomFactor));
						if ((toolSelected != TileTool.Box && toolSelected != TileTool.FloodFill && toolSelected != TileTool.Draw) || autoTileSelected != -1)
							break;
					}
					if ((toolSelected != TileTool.Box && toolSelected != TileTool.FloodFill && toolSelected != TileTool.Draw) || autoTileSelected != -1)
						break;
				}
			}
			else {
				selectedTiles = new Tile[1, 1] { { new Tile(0, 0) } };
			}

		}

		if (mouseDownScene && toolSelected == TileTool.Draw && !current.shift ) {
			if ((layerToEdit == null || layerTileInfo == null) && mat != null) {
				if (EditorUtility.DisplayDialog("Create new layer?", "There is no layer selected. Would you like to create a new one?", "Create new layer", "Cancel")) {
					posToCreateMesh = (Vector2)SceneView.lastActiveSceneView.camera.transform.position;
					//layerTileInfo = layerToEdit.GetComponent<TileInfo>();
					CreateTileLayer.Init();
				}
				mouseDownScene = false;
				return;
			}
			else if( mat == null ) {
				mouseDownScene = false;
				return;
			}
			else {
				if (autoTileSelected == -1) {
					for (int x = 0; x < selectedTiles.GetLength(0); x++) {
						for (int y = 0; y < selectedTiles.GetLength(1); y++) {
							int xx = TileInfo.Mod(x + (int)(mousePos.x / zoomFactor - lastPosScene.x / zoomFactor), selectedTiles.GetLength(0));
							int yy = TileInfo.Mod(y + (int)(lastPosScene.y / zoomFactor - mousePos.y / zoomFactor), selectedTiles.GetLength(1));
							AddTile(new Vector2(x*zoomFactor + mousePos.x + 0.5f*zoomFactor, mousePos.y - y*zoomFactor + 0.5f*zoomFactor), selectedTiles[xx, yy]);
						}
					}
				}
				else
					AddTile(mousePos + new Vector2( zoomFactor, zoomFactor ) * 0.5f , layerTileInfo.autoTileData[48 * autoTileSelected]);
			}
			layerTileInfo.UpdateVisualMesh( true );
		}

		if (toolSelected != TileTool.None && toolSelected != TileTool.Collisions && current.isMouse && current.button == 0) {

			if (current.type == EventType.MouseDown) {
				if (layerTileInfo != null && autoTileSelected >= layerTileInfo.numberOfAutotiles)
					autoTileSelected = -1;

				mouseDownScene = true;
				lastPosScene = mousePos;
			}

			else if (current.type == EventType.MouseUp && mouseDownScene) {
				mouseDownScene = false;
				//                if (CheckForGridSnap()) {
				if ((toolSelected == TileTool.Box || toolSelected == TileTool.FloodFill || toolSelected == TileTool.Draw) && layerToEdit == null) {
					if (mat != null && mat.mainTexture != null) {
						if (EditorUtility.DisplayDialog("Create new layer?", "There is no layer selected. Would you like to create a new one?", "Create new layer", "Cancel")) {
							posToCreateMesh = (Vector2)SceneView.lastActiveSceneView.camera.transform.position;
							//layerTileInfo = layerToEdit.GetComponent<TileInfo>();
							CreateTileLayer.Init();
						}
						else {
							return;
						}
					}
				}
				if (layerToEdit == null)
					return;

				if ((toolSelected != TileTool.None && toolSelected != TileTool.Collisions && toolSelected != TileTool.Erase) && (layerTileInfo.WorldPointToMapIndex(mousePos) == -1 ||
					layerTileInfo.WorldPointToMapIndex(lastPosScene) == -1)) {

					if (EditorUtility.DisplayDialog("Resize map bounds?",
						"Resize the map bounds to include the tile clicked?",
						"Resize Map Bounds",
						"Cancel")) {
						Undo.RecordObjects(new UnityEngine.Object[2] { layerTileInfo, layerTileInfo.transform }, "Resize Bounds");
						layerTileInfo.ResizeBoundsToFitWorldPos(mousePos);
					}
				}

				Vector2 originalTilePos = lastPosScene;
				if (mousePos.x < lastPosScene.x) {
					lastPosScene.x = mousePos.x;
					mousePos.x = originalTilePos.x;
				}
				if (mousePos.y < lastPosScene.y) {
					lastPosScene.y = mousePos.y;
					mousePos.y = originalTilePos.y;
				}
				if (current.shift) {
					selectedTiles = new Tile[(int)((mousePos.x - lastPosScene.x) / zoomFactor) + 1, (int)((mousePos.y - lastPosScene.y) / zoomFactor) + 1];
					autoTileSelected = -1;
				}
				for (int xIndex = 0; xIndex <= Mathf.RoundToInt((mousePos.x - lastPosScene.x)/zoomFactor); xIndex++) {
					for (int yIndex = 0; yIndex <= Mathf.RoundToInt((mousePos.y - lastPosScene.y)/zoomFactor); yIndex++) {
						float x = lastPosScene.x + xIndex * zoomFactor + 0.5f * zoomFactor;
						float y = mousePos.y - yIndex * zoomFactor + 0.5f * zoomFactor;
						switch (toolSelected) {
						case TileTool.Box:
						case TileTool.FloodFill:
						case TileTool.Draw:
							if (current.shift) {
								selectedTiles[xIndex,yIndex] = layerTileInfo.tiles[layerTileInfo.WorldPointToMapIndex(x, y)];
								break;
							}
							//if no drag was performed stamp the whole selection
							if (lastPosScene == mousePos && selectedTiles.Length > 1 && autoTileSelected == -1 && toolSelected == TileTool.Box) {
								for (int xx = 0; xx < selectedTiles.GetLength(0); xx++) {
									for (int yy = 0; yy < selectedTiles.GetLength(1); yy++) {
										AddTile(new Vector2(x + xx*zoomFactor, y - yy*zoomFactor), selectedTiles[xx, yy % selectedTiles.GetLength(1)]);
									}
								}
							}
							else {
								if (toolSelected == TileTool.Box)
									AddTile(new Vector2(x, y), selectedTiles[xIndex % selectedTiles.GetLength(0), yIndex % selectedTiles.GetLength(1)]);
								else if (toolSelected == TileTool.FloodFill)
									FloodFill(new Vector2(x, y));
							}

							break;
						case TileTool.Erase:
							RemoveTile(new Vector2(x, y));
							break;
						case TileTool.Rotate90:
							RotateTile(new Vector2(x, y));
							break;
						case TileTool.Flip:
							FlipTile(new Vector2(x, y));
							break;
						}
					}
				}


				//					for (int x = (int)lastPosScene.x; x <= (int)mousePos.x; x+=zoomFactor) {
				//						for (int y = (int)lastPosScene.y; y <= (int)mousePos.y; y+=zoomFactor) {
				//                            switch (toolSelected) {
				//                                case TileTool.Box:
				//                                case TileTool.FloodFill:
				//                                case TileTool.Draw:
				//                                if (current.shift) {
				//                                    selectedTiles[x - (int)lastPosScene.x, (int)mousePos.y - y] = layerTileInfo.tiles[layerTileInfo.WorldPointToMapIndex(x, y)];
				//                                    break;
				//                                }
				//                                //if no drag was performed stamp the whole selection
				//                                if (lastPosScene == mousePos && selectedTiles.Length > 1 && autoTileSelected == -1 && toolSelected == TileTool.Box) {
				//                                    for (int xx = 0; xx < selectedTiles.GetLength(0); xx++) {
				//                                        for (int yy = 0; yy < selectedTiles.GetLength(1); yy++) {
				//											AddTile(new Vector2(x + xx*zoomFactor, y - yy*zoomFactor), selectedTiles[xx, yy % selectedTiles.GetLength(1)]);
				//                                        }
				//                                    }
				//                                }
				//                                else {
				//                                    if (toolSelected == TileTool.Box)
				//                                        AddTile(new Vector2(x, y), selectedTiles[(x - (int)lastPosScene.x) % selectedTiles.GetLength(0), ((int)mousePos.y - y) % selectedTiles.GetLength(1)]);
				//                                    else if (toolSelected == TileTool.FloodFill)
				//                                        FloodFill(new Vector2(x, y));
				//                                }
				//
				//                                break;
				//                                case TileTool.Erase:
				//                                RemoveTile(new Vector2(x, y));
				//                                break;
				//                                case TileTool.Rotate90:
				//                                RotateTile(new Vector2(x, y));
				//                                break;
				//                                case TileTool.Flip:
				//                                FlipTile(new Vector2(x, y));
				//                                break;
				//                            }
				//                        }
				//                    }



				layerTileInfo.UpdateVisualMesh( true );

				//                }
			}

			current.Use();
			Repaint();
		}
	}

	public void OnGUI() {
		GUILayout.BeginHorizontal();
		DrawTileSheet();
		DrawSideControls();
		GUILayout.EndHorizontal();
	}

	Texture2D lastBlankImage;
	Texture2D BlankImage ( Color col ) {
		Texture2D result = lastBlankImage;
		if( result == null || result.GetPixel(0,0) != col ) {
			result = new Texture2D( 2, 2 );
			Color[] cols = new Color[4];
			for( int i = 0; i < 4; i++ )
				cols[i] = col;
			result.SetPixels( cols );
			result.Apply();
			lastBlankImage = result;
		}
		return result;
	}

	void DrawTileSheet() {
		GUILayout.BeginHorizontal();
		zoomScale = GUILayout.VerticalSlider(zoomScale, 5f, 0.5f, GUILayout.MaxHeight(200));

		fullSheetScroll = GUILayout.BeginScrollView(fullSheetScroll);

		if (mat != null && mat.mainTexture != null) {
			GUILayout.Label(
				string.Empty, GUILayout.Width(mat.mainTexture.width * zoomScale), GUILayout.Height(mat.mainTexture.height * zoomScale));

			Rect posRect = new Rect(0, 0, mat.mainTexture.width * zoomScale, mat.mainTexture.height * zoomScale);
			GUI.DrawTexture( posRect, BlankImage( backgroundColor ), ScaleMode.StretchToFill );

			GUI.DrawTexture(
				posRect,
				mat.mainTexture,
				ScaleMode.StretchToFill,
				true,
				1);
			DrawGrid();
			HandleSelection();

		}
		backgroundColor = EditorGUILayout.ColorField( "Background Color", backgroundColor );
		GUILayout.EndScrollView();
		GUILayout.EndHorizontal();
	}

	void DrawSelectedTiles() {
		if (mat == null || mat.mainTexture == null)
			return;

		GUILayout.Label(
			string.Empty, GUILayout.Height(200));

		if (autoTileSelected == -1) {
			float displaySize = 150 / Mathf.Max(selectedTiles.GetLength(0), selectedTiles.GetLength(1));

			int elementY = layerToEdit == null ? 180 : 250;
			for (int x = 0; x < selectedTiles.GetLength(0); x++) {
				for (int y = 0; y < selectedTiles.GetLength(1); y++) {
					if( selectedTiles[x,y] == Tile.empty )
						continue;
					Rect texCoords = new Rect( (offsetX + (float)selectedTiles[x, y].xIndex * (tileSize + spacing)) / (float)mat.mainTexture.width,
						1 - (((float)selectedTiles[x, y].yIndex + 1) * (tileSize + spacing) - spacing + offsetY) / mat.mainTexture.height,
						(tileSize + spacing) / (float)mat.mainTexture.width,
						(tileSize + spacing) / (float)mat.mainTexture.height);
					Vector2 pivot = new Vector2(50 + (displaySize / 2) + (x * displaySize), 100 + (displaySize / 2) + (y * displaySize));
					if (selectedTiles[x, y].flip) {
						texCoords.x += texCoords.width;
						texCoords.width *= -1;
					}
					EditorGUIUtility.RotateAroundPivot(selectedTiles[x, y].rotation * 90, pivot);
					GUI.DrawTextureWithTexCoords(
						new Rect(50 + x * displaySize, elementY + y * displaySize, displaySize, displaySize),
						mat.mainTexture,
						texCoords
					);
					GUI.matrix = Matrix4x4.identity;
				}
			}
		}
		else
			GUI.Label(new Rect(50, 250, 150, 50), "Auto Tile Selected");
	}

	void SaveCollisionData() {
		if (mat == null || mat.mainTexture == null)
			return;
		string path = EditorUtility.SaveFilePanel("Save collision data", "Assets/2DTilezoneFiles/CollisionData", mat.mainTexture.name + "_collision_data", "cd");
		if (path != string.Empty) {
			XmlSerializer xmls = new XmlSerializer(typeof(CollisionData));
			CollisionData collisionData = new CollisionData();
			collisionData.collisions = layerTileInfo.collisions;

			StringWriter sw = new StringWriter();
			xmls.Serialize(sw, (object)collisionData);
			string xml = sw.ToString();
			File.WriteAllText(path, xml);
		}
	}

	void LoadCollisionData() {
		if (mat == null || mat.mainTexture == null)
			return;

		string path = EditorUtility.OpenFilePanel("Load collision data", "Assets/2DTilezoneFiles/CollisionData", "cd");

		if (path != string.Empty) {
			XmlSerializer xmls = new XmlSerializer(typeof(CollisionData));
			CollisionData collisionData = xmls.Deserialize(new StringReader(File.ReadAllText(path))) as CollisionData;

			if (collisionData.collisions.Length == layerTileInfo.collisions.Length) {
				layerTileInfo.collisions = collisionData.collisions;
			}
			else
				EditorUtility.DisplayDialog("Incorect Texture Size",
					"Can not load as the collision data you attempted to load is from a texture of a different size.",
					"OK");
		}
	}

	void BakeAutoTile( int index ) {
		if( index >= layerTileInfo.numberOfAutotiles )
			return;
		if( EditorUtility.DisplayDialog( "Bake Autotile", "This will remove all " + layerTileInfo.autoTileNames[index] + " Autotiles from this layer and replace them with the same tile without being an Autotile.", "OK", "Cancel" ) ) {
			Undo.RecordObject( layerTileInfo, "Bake Autotile" );
			layerTileInfo.BakeAutoTile(index);
		}
	}

	void SaveAutoTile(int index) {
		if (mat == null || mat.mainTexture == null)
			return;

		string path = EditorUtility.SaveFilePanel("Save auto tile data", "Assets/2DTilezoneFiles/AutoTileFiles", mat.mainTexture.name + "_" + layerTileInfo.autoTileNames[index], "atd");
		if (path != string.Empty) {
			XmlSerializer xmls = new XmlSerializer(typeof(AutoTile));
			AutoTile autoTile = new AutoTile();
			for (int i = 48 * index; i < 48 * index + 48; i++) {
				autoTile.autoTileData.Add(layerTileInfo.autoTileData[i]);
			}
			autoTile.autoTileName = layerTileInfo.autoTileNames[index];
			autoTile.textureWidth = mat.mainTexture.width;
			autoTile.textureHeight = mat.mainTexture.height;

			StringWriter sw = new StringWriter();
			xmls.Serialize(sw, (object)autoTile);
			string xml = sw.ToString();
			File.WriteAllText(path, xml);
		}

	}

	void LoadAutoTile(int index) {
		if (mat == null || mat.mainTexture == null)
			return;
		string path = EditorUtility.OpenFilePanel("Load auto tile data", "Assets/2DTilezoneFiles/AutoTileFiles", "atd");
		if (path != string.Empty) {
			XmlSerializer xmls = new XmlSerializer(typeof(AutoTile));
			AutoTile autoTile = xmls.Deserialize(new StringReader(File.ReadAllText(path))) as AutoTile;

			if (mat.mainTexture.width == autoTile.textureWidth && mat.mainTexture.height == autoTile.textureHeight) {
				for (int i = 0; i < 48; i++) {
					layerTileInfo.autoTileData[i + 48 * index] = autoTile.autoTileData[i];
				}
				layerTileInfo.autoTileNames[index] = autoTile.autoTileName;
			}
			else
				EditorUtility.DisplayDialog("Incorect Texture Size",
					"Can not load as the auto tile you attempted to load is from a texture of a different size.",
					"OK");
		}


	}

	TilezoneAutotileCopyData autotileCopyBuffer;
	void CopyAutoTileData () {
		autotileCopyBuffer = new TilezoneAutotileCopyData( layerTileInfo );
	}
	void PasteAutoTileData () {
		if( autotileCopyBuffer == null )
			return;
		Undo.RecordObject( layerTileInfo, "Paste AutoTile Data" );
		layerTileInfo.numberOfAutotiles = autotileCopyBuffer.numberOfAutotiles;
		layerTileInfo.autoTileNames = new List<string>( autotileCopyBuffer.autoTileNames );
		layerTileInfo.autoTileEdgeMode = new List<TileInfo.AutoTileEdgeMode>( autotileCopyBuffer.autoTileEdgeMode );
		layerTileInfo.autoTileLinkMask = new List<int>( autotileCopyBuffer.autoTileLinkkMask );
		layerTileInfo.autoTileType = new List<TileInfo.AutoTileType>( autotileCopyBuffer.autoTileType );
		layerTileInfo.autoTileData = new List<Tile>( autotileCopyBuffer.autoTileData );
		layerTileInfo.showAutoTile = new List<bool>();
		for (int i = 0; i < autotileCopyBuffer.numberOfAutotiles; i++) {
			layerTileInfo.showAutoTile.Add( false );
		}
	}

	void DrawAutoTiles() {
		if (layerTileInfo == null)
			return;

		if (mat == null)
			return;

		if (mat.mainTexture == null)
			return;
		GUILayout.BeginHorizontal();
//		layerTileInfo.numberOfAutotiles = EditorGUILayout.IntField("Number of Auto Tiles:", layerTileInfo.numberOfAutotiles);
		if (GUILayout.Button( "Add AutoTile" )) {
			layerTileInfo.numberOfAutotiles++;
			//add empty tile data if it doesnt exsist
			while (layerTileInfo.autoTileData.Count < 48 * layerTileInfo.numberOfAutotiles) {
				layerTileInfo.autoTileData.Add(Tile.empty);
			}
			while (layerTileInfo.autoTileNames.Count < layerTileInfo.numberOfAutotiles) {
				layerTileInfo.autoTileNames.Add("Auto Tile " + (layerTileInfo.autoTileNames.Count + 1).ToString());
				layerTileInfo.autoTileLinkMask.Add(1 << layerTileInfo.autoTileLinkMask.Count);
				layerTileInfo.showAutoTile.Add(true);
				layerTileInfo.autoTileEdgeMode.Add(TileInfo.AutoTileEdgeMode.None);
			}
		}
		if( GUILayout.Button( "Copy" ) ) { CopyAutoTileData(); }
		if( autotileCopyBuffer != null && GUILayout.Button( "Paste" ) ) { PasteAutoTileData(); }
		GUILayout.EndHorizontal();
		//kept seperate for version compatability
		while (layerTileInfo.autoTileType.Count < layerTileInfo.numberOfAutotiles)
			layerTileInfo.autoTileType.Add(TileInfo.AutoTileType.Normal);
		while (layerTileInfo.autoTileNames.Count > layerTileInfo.numberOfAutotiles) {
			layerTileInfo.autoTileNames.RemoveAt( layerTileInfo.autoTileNames.Count - 1 );
			layerTileInfo.autoTileLinkMask.RemoveAt( layerTileInfo.autoTileLinkMask.Count - 1 );
			layerTileInfo.showAutoTile.RemoveAt( layerTileInfo.showAutoTile.Count - 1 );
			layerTileInfo.autoTileEdgeMode.RemoveAt( layerTileInfo.autoTileEdgeMode .Count - 1 );
		}


		for (int i = 0; i < layerTileInfo.numberOfAutotiles; i++) {
			bool isSelected = (autoTileSelected == i);
			GUILayout.BeginHorizontal ();
			layerTileInfo.showAutoTile[i] = EditorGUILayout.Foldout(layerTileInfo.showAutoTile[i], layerTileInfo.autoTileNames[i]);
			if( !layerTileInfo.showAutoTile[i] ) {
				isSelected = EditorGUILayout.Toggle( isSelected, GUILayout.Width( 32 ) );
				if (isSelected)
					autoTileSelected = i;

				if( GUILayout.Button( "Delete", GUILayout.Width( 48 ) ) ) {
					layerTileInfo.autoTileNames.RemoveAt( i );
					layerTileInfo.autoTileLinkMask.RemoveAt( i );
					layerTileInfo.showAutoTile.RemoveAt( i );
					layerTileInfo.autoTileEdgeMode.RemoveAt( i );
					layerTileInfo.autoTileType.RemoveAt( i );
					layerTileInfo.autoTileData.RemoveRange( i*48, 48 );
					layerTileInfo.numberOfAutotiles--;
					i--;
				}
			}
			GUILayout.EndHorizontal();
			if (layerTileInfo.showAutoTile[i]) {
				while (autoTileY.Count <= i)
					autoTileY.Add(0);
				autoTileY[i] = (int)EditorGUILayout.BeginVertical().y;
				layerTileInfo.autoTileNames[i] = EditorGUILayout.TextField(layerTileInfo.autoTileNames[i]);
				GUILayout.Label(string.Empty, GUILayout.Height(250));
				SetCursor(new Rect(50, autoTileY[i] + 30, 150, 200));
				layerTileInfo.autoTileLinkMask[i] = EditorGUILayout.MaskField("Link With", layerTileInfo.autoTileLinkMask[i], layerTileInfo.autoTileNames.ToArray());
				layerTileInfo.autoTileEdgeMode[i] = (TileInfo.AutoTileEdgeMode)EditorGUILayout.EnumPopup("Map edge link mode", layerTileInfo.autoTileEdgeMode[i]);
				layerTileInfo.autoTileType[i] = (TileInfo.AutoTileType)EditorGUILayout.EnumPopup("Auto Tile Type", layerTileInfo.autoTileType[i]);
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Save")) { SaveAutoTile(i); }
				if (GUILayout.Button("Load")) {
					LoadAutoTile(i);
					Focus();
				}
				if( GUILayout.Button( "Bake to scene" ) ) { BakeAutoTile(i); }
				EditorGUILayout.EndHorizontal();
				GUI.DrawTexture(new Rect(50, autoTileY[i] + 30, 150, 200), autoTileGuide, ScaleMode.StretchToFill);

//				bool isSelected = (autoTileSelected == i);
				isSelected = EditorGUI.Toggle(new Rect(20, autoTileY[i] + 100, 20, 20), isSelected);
				if (isSelected)
					autoTileSelected = i;

				if (!GUI.RepeatButton(new Rect(0, autoTileY[i] + 130, 40, 20), "Hide")) {

					for (int a = 0; a < 48; a++) {
						if (layerTileInfo.autoTileData.Count > a + i * 48 && layerTileInfo.autoTileData[a + i * 48] != Tile.empty) {
							Rect texCoords = new Rect((offsetX + (float)layerTileInfo.autoTileData[a + i * 48].xIndex * (tileSize + spacing)) / (float)mat.mainTexture.width,
								1 - (((float)layerTileInfo.autoTileData[a + i * 48].yIndex + 1) * (tileSize + spacing) - spacing + offsetY) / mat.mainTexture.height,
								tileSize / (float)mat.mainTexture.width,
								tileSize / (float)mat.mainTexture.height);
							//flip tile
							if (layerTileInfo.autoTileData[a + i * 48].flip) {
								texCoords.x += texCoords.width;
								texCoords.width *= -1;
							}

							Vector2 pivot = new Vector2(50 + (a % 6) * 25, autoTileY[i] + 30 + (a / 6) * 25);
							pivot.x += 12.5f;
							pivot.y += 12.5f;

							EditorGUIUtility.RotateAroundPivot(layerTileInfo.autoTileData[a + i * 48].rotation * 90, pivot);
							GUI.DrawTextureWithTexCoords(
								new Rect(50 + (a % 6) * 25, autoTileY[i] + 30 + (a / 6) * 25, 25, 25),
								mat.mainTexture,
								texCoords
							);
							GUI.matrix = Matrix4x4.identity;
						}
					}
				}
				EditorGUILayout.EndVertical();
			}
		}
		EditorGUILayout.Space();
	}

	string[] sortingLayerNames {
		get {
			Type ieut = typeof( UnityEditorInternal.InternalEditorUtility );
			PropertyInfo properties = ieut.GetProperty( "sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic );
			return (string[])properties.GetValue( null, new object[0] );
		}
	}

	int[] sortingLayerIDs {
		get {
			Type ieut = typeof( UnityEditorInternal.InternalEditorUtility );
			PropertyInfo properties = ieut.GetProperty( "sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic );
			return (int[])properties.GetValue( null, new object[0] );
		}
	}

	void DrawSideControls() {
		GUILayout.BeginVertical(GUILayout.Width(250));
		//toolSelected = (TileTool)GUILayout.SelectionGrid( (int)toolSelected,  Enum.GetNames( typeof(TileTool) ), 3, EditorStyles.radioButton );
		EditorGUI.BeginChangeCheck();
		toolSelected = (TileTool)GUILayout.Toolbar((int)toolSelected, toolIcons, new GUILayoutOption[0]);
		if (EditorGUI.EndChangeCheck()) {
			SetWireframeHidden( toolSelected != TileTool.None );
			Repaint();
			SceneView.lastActiveSceneView.Repaint();
		}
		string s = " Hold the shift key to select tiles in the Scene View.";
		switch (toolSelected) {
		case TileTool.None:
			EditorGUILayout.HelpBox("No tool is selected. To start editing select a tool.", MessageType.Info);
			break;
		case TileTool.Box:
			EditorGUILayout.HelpBox("Box Tool. Click and drag to draw a box." + s, MessageType.Info);
			break;
		case TileTool.Erase:
			EditorGUILayout.HelpBox("Erase Tool. Click and drag to erase.", MessageType.Info);
			break;
		case TileTool.Collisions:
			EditorGUILayout.HelpBox("Collisions. Edit the collision data in the window to the left.", MessageType.Info);
			break;
		case TileTool.Rotate90:
			EditorGUILayout.HelpBox("Rotate Tool. Click on a tile to rotate it.", MessageType.Info);
			break;
		case TileTool.Flip:
			EditorGUILayout.HelpBox("Flip Tool. Click on a tile to flip it", MessageType.Info);
			break;
		case TileTool.FloodFill:
			EditorGUILayout.HelpBox("Fill Tool. Click on a tile to fill in an area. Note deosn't work with Scriptable Tile Layer." + s, MessageType.Info);
			break;
		case TileTool.Draw:
			EditorGUILayout.HelpBox("Paint Brush Tool. Click on a tile to paint a tile" + s, MessageType.Info);
			break;
		}

		//EditorGUILayout.LabelField( "Layer to edit." );
		EditorGUI.BeginChangeCheck();
		if( layerToEdit != null && layerTileInfo == null )
			layerTileInfo = layerToEdit.GetComponent<TileInfo>();
		layerTileInfo = (TileInfo)EditorGUILayout.ObjectField("Layer to edit.", layerTileInfo, typeof(TileInfo), true);
		if (EditorGUI.EndChangeCheck()) {
			if (layerTileInfo != null)
				layerToEdit = layerTileInfo.gameObject;
			else
				layerToEdit = null;
			//GameObject layerToEdit = GameObject.Find( layerName );
			if (layerToEdit != null && layerToEdit.GetComponent<TileInfo>() != null) {
				TileInfo tile = layerToEdit.GetComponent<TileInfo>();
				tileSize = tile.tileSize;
				offsetX = tile.offsetX;
				offsetY = tile.offsetY;
				spacing = tile.spacing;
				mat = tile.meshRenderer.sharedMaterial;
				layerTileInfo = layerToEdit.GetComponent<TileInfo>();
				InitLayerToEdit();
				Repaint();
				SceneView.lastActiveSceneView.Repaint();
			}
		}
		if (mat != null && mat.mainTexture != null) {
			if (GUILayout.Button("Create New Layer")) {
					posToCreateMesh = (Vector2)SceneView.lastActiveSceneView.camera.transform.position;
					CreateTileLayer.Init();
			}
		}
		else {
			EditorGUILayout.HelpBox( "You must at least have a material set before you can create a new layer", MessageType.Info );
		}

		if (toolSelected == TileTool.Collisions) {
			EditorGUILayout.Space();
			sideControlsScroll = GUILayout.BeginScrollView(sideControlsScroll);
			if( layerTileInfo != null ) {
				if (GUILayout.Button("Update Colliders")) {
					bool u = layerTileInfo.update2DColliders;
					layerTileInfo.update2DColliders = true;
					layerTileInfo.mapHasChanged = true;
					layerTileInfo.UpdateVisualMesh(true);
					layerTileInfo.update2DColliders = u;
				}
				EditorGUI.BeginChangeCheck();
				layerTileInfo.update2DColliders = EditorGUILayout.Toggle("Update 2D colliders", layerTileInfo.update2DColliders);
				if( EditorGUI.EndChangeCheck() ) {
					layerTileInfo.mapHasChanged = true;
					layerTileInfo.UpdateVisualMesh(true);
				}
			}

			if (GUILayout.Button("Save Collision Data")) {
				if (layerTileInfo != null)
					SaveCollisionData();
			}

			if (GUILayout.Button("Load Collision Data")) {
				if (layerTileInfo != null)
					LoadCollisionData();
			}
			if( layerTileInfo != null ) {
				EditorGUILayout.HelpBox("The below feature requires the textures read write flag to be enabled in the texture import settings.", MessageType.Info);
				layerTileInfo.pixelColliders = EditorGUILayout.Toggle( "Slopes from image", layerTileInfo.pixelColliders );
			}
			GUILayout.EndScrollView();
		}
		else {
			sideControlsScroll = GUILayout.BeginScrollView(sideControlsScroll);
			tileSize = EditorGUILayout.IntField("Tile Size", tileSize);
			tileSize = Mathf.Max(4, tileSize);
			spacing = EditorGUILayout.IntField("Spacing", spacing);
			spacing = Mathf.Max(0, spacing);
			offsetX = EditorGUILayout.IntField("X Offset", offsetX);
			offsetX = Mathf.Clamp(offsetX, 0, tileSize);
			offsetY = EditorGUILayout.IntField("Y Offset", offsetY);
			offsetY = Mathf.Clamp(offsetY, 0, tileSize);
			if( layerTileInfo != null ) {
				EditorGUI.BeginChangeCheck();
				layerTileInfo.zoomFactor = EditorGUILayout.FloatField("Zoom", layerTileInfo.zoomFactor);
				layerTileInfo.zoomFactor = Mathf.Max(layerTileInfo.zoomFactor, 0.1f);
				if( EditorGUI.EndChangeCheck() )
					layerToEdit.transform.localScale = new Vector3( layerTileInfo.zoomFactor, layerTileInfo.zoomFactor, 1 );
			}
			mat = (Material)EditorGUILayout.ObjectField("Material", mat, typeof(Material), true);
			EditorGUILayout.Space();

			if( meshRenderer != null && layerToEdit != null ) {
				meshRenderer.Update();
				EditorGUI.BeginChangeCheck();
				meshRendererSelectedLayer = EditorGUILayout.Popup( "Sorting Layer", meshRendererSelectedLayer, sortingLayerNames );
				EditorGUILayout.PropertyField( meshRendererSortingOrder, new GUIContent("Order in Layer") );
				if( EditorGUI.EndChangeCheck() ) {
					meshRendererSortingLayer.intValue = sortingLayerIDs[meshRendererSelectedLayer];
				}
				meshRenderer.ApplyModifiedProperties();


				//				EditorGUI.BeginChangeCheck();
				//				meshRendererSelectedLayer = EditorGUILayout.Popup( "Sorting Layer", meshRendererSelectedLayer, sortingLayerNames );
				//				if( EditorGUI.EndChangeCheck() ) {
				//					meshRenderer.sortingLayerID = sortingLayerIDs[meshRendererSelectedLayer];
				//				}
				//				meshRenderer.sortingOrder = EditorGUILayout.IntField( "Order in Layer", meshRenderer.sortingOrder );
			}

			EditorGUILayout.Space();

//			if (GUILayout.Button("Update Colliders")) {
//				if (layerTileInfo != null)
//					layerTileInfo.UpdateColliders();
//			}

			if( layerTileInfo != null ) {
				if (GUILayout.Button("Update Colliders")) {
					bool u = layerTileInfo.update2DColliders;
					layerTileInfo.update2DColliders = true;
					layerTileInfo.mapHasChanged = true;
					layerTileInfo.UpdateVisualMesh(true);
					layerTileInfo.update2DColliders = u;
				}
				EditorGUI.BeginChangeCheck();
				layerTileInfo.update2DColliders = EditorGUILayout.Toggle("Update 2D colliders", layerTileInfo.update2DColliders);
				if( EditorGUI.EndChangeCheck() ) {
					layerTileInfo.mapHasChanged = true;
					layerTileInfo.UpdateVisualMesh(true);
				}
			}

			if( GUILayout.Button( "Edit Key Shortcuts" ) ) {
				EditTilezoneKeyShortcuts.Init();
			}

			//            GUILayout.Label("Zoom");
			//            zoomScale = GUILayout.VerticalSlider(zoomScale, 5f, 0.5f, GUILayout.MaxHeight(200));

			DrawSelectedTiles();

			if (layerTileInfo != null) {
				DrawAutoTiles();

				HandleAutoTileSelection();

				EditorGUILayout.Space();


				EditorGUILayout.HelpBox("The below features require the textures read write flag to be enabled in the texture import settings.", MessageType.Info);
				if (GUILayout.Button("Export to PNG")) {
					ExportToPng();
				}
				EditorGUI.BeginChangeCheck();
				layerTileInfo.update3DWalls = EditorGUILayout.Toggle("Update 3D walls", layerTileInfo.update3DWalls);
				if( EditorGUI.EndChangeCheck() ) {
					layerTileInfo.mapHasChanged = true;
					layerTileInfo.UpdateVisualMesh(true);
				}
				layerTileInfo.depthOf3DWalls = EditorGUILayout.FloatField( "3D wall depth", layerTileInfo.depthOf3DWalls );
				layerTileInfo.pixelColliders = EditorGUILayout.Toggle( "Slopes from image", layerTileInfo.pixelColliders );
			}

			GUILayout.EndScrollView();
		}
		GUILayout.EndVertical();
	}

	void HandleAutoTileSelection() {
		Event current = Event.current;
		if (current.isMouse && current.button == 0 && current.type == EventType.MouseUp) {
			if (layerToEdit == null)
				return;

			Undo.RecordObject(layerTileInfo, "Change AutoTile");

			mousePos = current.mousePosition;
			int autoTileClicked = -1;
			for (int i = 0; i < layerTileInfo.numberOfAutotiles; i++) {
				if (!layerTileInfo.showAutoTile[i])
					continue;

				Vector2 tileClicked = current.mousePosition;
				tileClicked.x -= 50;
				tileClicked.y -= autoTileY[i] + 30;
				tileClicked /= 25;
				tileClicked.x = Mathf.Floor(tileClicked.x);
				tileClicked.y = Mathf.Floor(tileClicked.y);
				if (tileClicked.x >= 0 && tileClicked.x < 6 && tileClicked.y >= 0 && tileClicked.y < 8) {
					autoTileClicked = (int)tileClicked.x + 6 * (int)tileClicked.y;
					autoTileClicked += 48 * i;

					switch (toolSelected) {
					case TileTool.Flip:
						//flip
						if (layerTileInfo.autoTileData[autoTileClicked] != Tile.empty)
							layerTileInfo.autoTileData[autoTileClicked].flip = !layerTileInfo.autoTileData[autoTileClicked].flip;
						break;
					case TileTool.Rotate90:
						//rotate
						if (layerTileInfo.autoTileData[autoTileClicked] != Tile.empty) {
							layerTileInfo.autoTileData[autoTileClicked].rotation--;
							if (layerTileInfo.autoTileData[autoTileClicked].rotation < 0)
								layerTileInfo.autoTileData[autoTileClicked].rotation += 4;
						}
						break;
					case TileTool.Erase:
						layerTileInfo.autoTileData[autoTileClicked] = Tile.empty;
						break;
					default:
						for (int x = 0; x < selectedTiles.GetLength(0); x++) {
							for (int y = 0; y < selectedTiles.GetLength(1); y++) {

								if (((autoTileClicked - 48 * i) % 6) + x >= 6)
									break;
								if (((autoTileClicked - 48 * i) / 6) + y >= 8)
									break;

								int t = (((autoTileClicked - 48 * i) / 6) + y) * 6 + (((autoTileClicked - 48 * i) % 6) + x);
								t += i * 48;
								if (layerTileInfo.autoTileData[t] == selectedTiles[x, y] && selectedTiles.Length == 1)
									layerTileInfo.autoTileData[t] = Tile.empty;
								else
									layerTileInfo.autoTileData[t] = new Tile(selectedTiles[x, y]);
							}
						}

						break;
					}
				}
			}
			Repaint();
		}
	}

	void HandleSelection() {
		Event current = Event.current;
		if( !EditorGUIUtility.editingTextField )
			CheckForKeyboardShortcuts( current );

		if(current.type == EventType.ScrollWheel && current.control && current.mousePosition.x < position.width - 280 + fullSheetScroll.x) {
			zoomScale = Mathf.Clamp( zoomScale - current.delta.y * 0.05f, 0.5f, 5f );
			current.Use();
			Repaint();
		}
		if (current.isMouse) {

			mousePos = current.mousePosition;

			mousePos.x = Mathf.Clamp( Mathf.Floor((mousePos.x - offsetX*zoomScale) / ((tileSize + spacing) * zoomScale)), 0, sheetWidth-1 );
			mousePos.y = Mathf.Clamp( Mathf.Floor((mousePos.y - offsetY*zoomScale) / ((tileSize + spacing) * zoomScale)), 0, sheetHeight-1 );

			if(current.type == EventType.MouseDrag && current.button == 2) {
				fullSheetScroll -= current.delta;
				Repaint();
			}


			if (current.type == EventType.MouseDown && current.button == 0) {
				lastPosWindow = mousePos;
				EditorGUIUtility.editingTextField = false;
			}
			if (current.type == EventType.MouseUp && current.button == 0) {
				mouseDownScene = false;
				Vector2 bottomLeft = lastPosWindow;
				bottomLeft.y = mousePos.y;
				Vector2 topRight = mousePos;
				topRight.y = lastPosWindow.y;
				if (mousePos.x < lastPosWindow.x) {
					bottomLeft.x = mousePos.x;
					topRight.x = lastPosWindow.x;
				}
				if (mousePos.y < lastPosWindow.y) {
					bottomLeft.y = lastPosWindow.y;
					topRight.y = mousePos.y;
				}


				if (toolSelected != TileTool.Collisions) {
					selectedTiles = new Tile[((int)topRight.x - (int)bottomLeft.x) + 1, ((int)bottomLeft.y - (int)topRight.y) + 1];
					autoTileSelected = -1;
				}
				for (int x = 0; x <= topRight.x - bottomLeft.x; x++) {
					for (int y = 0; y <= bottomLeft.y - topRight.y; y++) {

						if (bottomLeft.x + x >= sheetWidth
							|| topRight.y + y >= sheetHeight)
							continue;

						if (toolSelected == TileTool.Collisions) {
							if (layerToEdit != null && layerToEdit.GetComponent<TileInfo>() != null) {
								Undo.RecordObject(layerTileInfo, "Change collision data");
								if (layerToEdit.GetComponent<TileInfo>().collisions.Length == (sheetWidth) * (sheetHeight)) {
									layerToEdit.GetComponent<TileInfo>().collisions[((int)topRight.y + y) * (sheetWidth) + (int)bottomLeft.x + x]++;
									if ((int)layerToEdit.GetComponent<TileInfo>().collisions[((int)topRight.y + y) * (sheetWidth) + (int)bottomLeft.x + x] > 4)
										layerToEdit.GetComponent<TileInfo>().collisions[((int)topRight.y + y) * (sheetWidth) + (int)bottomLeft.x + x] = TileInfo.CollisionType.None;
								}
								else {
									layerToEdit.GetComponent<TileInfo>().collisions = new TileInfo.CollisionType[(sheetWidth) * (sheetHeight)];
									for (int i = 0; i < (sheetWidth) * (sheetHeight); i++) {
										layerToEdit.GetComponent<TileInfo>().collisions[i] = TileInfo.CollisionType.Full;
									}
								}
							}
						}
						else {
							selectedTiles[x, y] = new Tile((int)bottomLeft.x + x, (int)topRight.y + y);
						}
					}
				}
				this.Repaint();
			}


		}

		if (toolSelected == TileTool.Collisions) {
			if (layerToEdit != null && layerToEdit.GetComponent<TileInfo>() != null
				&& layerToEdit.GetComponent<TileInfo>().collisions.Length == (sheetWidth) * (sheetHeight)) {
				for (int x = 0; x < sheetWidth; x++) {
					for (int y = 0; y < sheetHeight; y++) {
						if (layerToEdit.GetComponent<TileInfo>().collisions[y * (sheetWidth) + x] == TileInfo.CollisionType.Full)
							EditorGUI.DrawRect(new Rect((offsetX + x * (tileSize + spacing)) * zoomScale, (offsetY + y * (tileSize + spacing)) * zoomScale, tileSize * zoomScale, tileSize * zoomScale), new Color(0, 0, 1, 0.3f));
						if (layerToEdit.GetComponent<TileInfo>().collisions[y * (sheetWidth) + x] == TileInfo.CollisionType.SlopeUpRight)
							DrawTriangle(new Rect((offsetX + x * (tileSize + spacing)) * zoomScale, (offsetY + y * (tileSize + spacing)) * zoomScale, tileSize * zoomScale, tileSize * zoomScale), new Color(0, 0, 1, 0.3f), true);
						if (layerToEdit.GetComponent<TileInfo>().collisions[y * (sheetWidth) + x] == TileInfo.CollisionType.SlopeUpLeft)
							DrawTriangle(new Rect((offsetX + x * (tileSize + spacing)) * zoomScale, (offsetY + y * (tileSize + spacing)) * zoomScale, tileSize * zoomScale, tileSize * zoomScale), new Color(0, 0, 1, 0.3f), false);
						if (layerToEdit.GetComponent<TileInfo>().collisions[y * (sheetWidth) + x] == TileInfo.CollisionType.Platform)
							EditorGUI.DrawRect(new Rect((offsetX + x * (tileSize + spacing)) * zoomScale, (offsetY + y * (tileSize + spacing)) * zoomScale, tileSize * zoomScale, (tileSize * zoomScale) / 2), new Color(0, 0, 1, 0.3f));
					}
				}
			}
		}
		else if (selectedTiles.Length > 0 && selectedTiles[0, 0] != null) {
			if (autoTileSelected == -1) {
				for (int x = 0; x < selectedTiles.GetLength(0); x++) {
					for (int y = 0; y < selectedTiles.GetLength(1); y++) {
						if (selectedTiles[x, y] != Tile.empty && selectedTiles[x, y] != null)
							EditorGUI.DrawRect(new Rect(offsetX * zoomScale + selectedTiles[x, y].xIndex * (tileSize + spacing) * zoomScale, offsetY * zoomScale + selectedTiles[x, y].yIndex * (tileSize + spacing) * zoomScale, tileSize * zoomScale, tileSize * zoomScale), new Color(1, 0, 0, 0.3f));
					}
				}
			}
		}
		else {
			selectedTiles = new Tile[1, 1] { { new Tile(0, 0) } };
		}

	}

	void ExportToPng() {
		if (layerTileInfo == null) {
			EditorUtility.DisplayDialog("Export To Sprite", "You have no layer selected.", "OK");
			return;
		}
		tileSize = layerTileInfo.tileSize;
		spacing = layerTileInfo.spacing;
		offsetX = layerTileInfo.offsetX;
		offsetY = layerTileInfo.offsetY;

		Texture2D mapSprite = new Texture2D((tileSize + spacing) * layerTileInfo.mapWidth, (tileSize + spacing) * layerTileInfo.mapHeight);
		Color[] allPixels = mapSprite.GetPixels();
		for (int i = 0; i < allPixels.Length; i++) {
			allPixels[i] = Color.clear;
		}
		mapSprite.SetPixels(allPixels);
		Texture2D tileTex = (Texture2D)layerTileInfo.meshRenderer.sharedMaterial.mainTexture;
		for (int x = 0; x < layerTileInfo.mapWidth; x++) {
			for (int y = 0; y < layerTileInfo.mapHeight; y++) {

				int i = y * layerTileInfo.mapWidth + x;
				if (layerTileInfo.tiles[i] == Tile.empty)
					continue;
				Color[] thisTile = tileTex.GetPixels(offsetX + layerTileInfo.tiles[i].xIndex * (tileSize + spacing),
					tileTex.height - (((int)layerTileInfo.tiles[i].yIndex + 1) * (tileSize + spacing)) + spacing - offsetY,
					tileSize, tileSize);
				Color[] modTile = new Color[thisTile.Length];

				//flip and rotate the color array
				if (layerTileInfo.tiles[i].flip) {
					for (int xx = 0; xx < tileSize; xx++) {
						for (int yy = 0; yy < tileSize; yy++) {
							modTile[yy * tileSize + xx] = thisTile[yy * tileSize + (tileSize - 1 - xx)];
						}
					}
					modTile.CopyTo(thisTile, 0);
				}

				if (layerTileInfo.tiles[i].rotation == 1 || layerTileInfo.tiles[i].rotation == 3) {
					for (int xx = 0; xx < tileSize; xx++) {
						for (int yy = 0; yy < tileSize; yy++) {
							if (layerTileInfo.tiles[i].rotation == 3)
								modTile[yy * tileSize + xx] = thisTile[(tileSize - 1 - xx) * tileSize + yy];
							if (layerTileInfo.tiles[i].rotation == 1)
								modTile[yy * tileSize + xx] = thisTile[xx * tileSize + (tileSize - 1 - yy)];
						}
					}
					thisTile = modTile;
				}

				if (layerTileInfo.tiles[i].rotation == 2) {
					for (int ii = 0; ii < thisTile.Length; ii++) {
						modTile[ii] = thisTile[thisTile.Length - 1 - ii];
					}
					thisTile = modTile;
				}

				mapSprite.SetPixels(x * tileSize, y * tileSize, tileSize, tileSize, thisTile);
			}
		}

		mapSprite.Apply();

		string path = EditorUtility.SaveFilePanelInProject("Save as PNG",
			layerTileInfo.name + ".png",
			"png",
			"Please enter a file name to save the map to");

		if (path != string.Empty) {
			byte[] pngData = mapSprite.EncodeToPNG();
			if (pngData != null) {

				File.WriteAllBytes(path, pngData);

				AssetDatabase.Refresh();
				TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(path);
				textureImporter.spritePixelsPerUnit = tileSize;
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			}
		}
	}
}

public class EditTilezoneKeyShortcuts : EditorWindow {
	public static void Init() {
		EditTilezoneKeyShortcuts window = (EditTilezoneKeyShortcuts)EditorWindow.GetWindow(typeof(EditTilezoneKeyShortcuts));
		window.ShowPopup();
	}

	public void OnGUI () {
		TilesetEditor.noneShortcut = (KeyCode)EditorGUILayout.EnumPopup( "No Tool:", TilesetEditor.noneShortcut );
		TilesetEditor.drawShortcut = (KeyCode)EditorGUILayout.EnumPopup( "Paint Brush Tool:", TilesetEditor.drawShortcut );
		TilesetEditor.eraseShortcut = (KeyCode)EditorGUILayout.EnumPopup( "Eraser Tool:", TilesetEditor.eraseShortcut );
		TilesetEditor.boxShortcut = (KeyCode)EditorGUILayout.EnumPopup( "Box Tool:", TilesetEditor.boxShortcut );
		TilesetEditor.rotateShortcut = (KeyCode)EditorGUILayout.EnumPopup( "Rotate Tool:", TilesetEditor.rotateShortcut );
		TilesetEditor.flipShortcut = (KeyCode)EditorGUILayout.EnumPopup( "Flip Tool:", TilesetEditor.flipShortcut );
		TilesetEditor.fillShortcut = (KeyCode)EditorGUILayout.EnumPopup( "Fill Tool:", TilesetEditor.fillShortcut );
		TilesetEditor.collisionsShortcut = (KeyCode)EditorGUILayout.EnumPopup( "Collisions:", TilesetEditor.collisionsShortcut );
	}
}

public class CreateTileLayer : EditorWindow {
	int mapWidth;
	int mapHeight;
	string goName;
	Vector2 scrollPos = Vector2.zero;

	public static void Init() {
		CreateTileLayer window = (CreateTileLayer)EditorWindow.GetWindow(typeof(CreateTileLayer));
		window.ShowPopup();

		window.mapWidth = 16;
		window.mapHeight = 16;
		window.goName = "Layer" + (GameObject.FindObjectsOfType<TileInfo>().Length + 1);
	}
	public void OnGUI() {
		scrollPos = GUILayout.BeginScrollView( scrollPos );

		EditorGUILayout.HelpBox( "Make sure the tile size and spacing is set up before you create a new layer.", MessageType.Info );
		mapWidth = EditorGUILayout.IntField("Map Width", mapWidth);
		mapHeight = EditorGUILayout.IntField("Map Height", mapHeight);

		mapWidth = Mathf.Max(mapWidth, 1);
		mapWidth = Mathf.Min(mapWidth, 64);

		mapHeight = Mathf.Max(mapHeight, 1);
		mapHeight = Mathf.Min(mapHeight, 64);

		goName = EditorGUILayout.TextField("Layer name: ", goName);

		float labelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 256;
		GUILayout.Space( 16 );
		EditorGUI.BeginChangeCheck();



		if (GUILayout.Button("Create Layer")) {
			Rect mapRect = new Rect();
			mapRect.x = TilesetEditor.posToCreateMesh.x;
			mapRect.y = TilesetEditor.posToCreateMesh.y;
			mapRect.width = mapWidth;
			mapRect.height = mapHeight;
			TilesetEditor.CreateMesh(mapRect, goName);
			TilesetEditor.autoTileSelected = -1;
			this.Close();
		}
		EditorGUIUtility.labelWidth = labelWidth;
		GUILayout.EndScrollView();
	}
}


[CustomPropertyDrawer(typeof(Tile))]
public class TileDrawer : PropertyDrawer {
	
	TileInfo tileLayer;
	int tileSize;
	int spacing;
	int offsetX;
	int offsetY;
	Texture tileTex;
	
	void DrawTile( SerializedProperty property, Rect position ) {
		if( tileTex == null ||
		   offsetX + ( property.FindPropertyRelative( "xIndex" ).intValue + 1 ) * ( tileSize + spacing ) >= tileTex.width ||
		   (property.FindPropertyRelative( "yIndex" ).intValue + 1)  * (tileSize + spacing) - spacing + offsetY >= tileTex.height )
			return;
		if( property.FindPropertyRelative( "xIndex" ).intValue == -1 )
			GUI.DrawTexture( position, EditorGUIUtility.whiteTexture );
		else {
			//			int tileSize = tileLayer.tileSize;
			//			int spacing = tileLayer.spacing;
			//			Texture tileTex = tileLayer.GetComponent<MeshRenderer>().sharedMaterial.mainTexture;
			Rect texCoords = new Rect( ( offsetX + (float)property.FindPropertyRelative( "xIndex" ).intValue * (tileSize + spacing) ) / (float)tileTex.width,
			                          1 - (((float)property.FindPropertyRelative( "yIndex" ).intValue + 1)  * (tileSize + spacing) - spacing + offsetY) / tileTex.height,
			                          (tileSize + spacing) / (float)tileTex.width,
			                          (tileSize + spacing) / (float)tileTex.height );
			Vector2 pivot = new Vector2( position.x + position.width / 2, position.y + position.height / 2 );
			if( property.FindPropertyRelative( "flip" ).boolValue ) {
				texCoords.x += texCoords.width;
				texCoords.width *= -1;
			}
			EditorGUIUtility.RotateAroundPivot(property.FindPropertyRelative( "rotation" ).intValue * 90 , pivot );
			GUI.DrawTextureWithTexCoords( position, tileTex, texCoords );
			GUI.matrix = Matrix4x4.identity;
		}
	}
	
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		//		if( (property.serializedObject.targetObject as MonoBehaviour) == null )
		//			return;
		if( tileLayer == null ) {
			if( property.serializedObject.targetObject as MonoBehaviour != null )
				tileLayer = (property.serializedObject.targetObject as MonoBehaviour).GetComponent<TileInfo>();
			if( tileLayer == null ) {
				tileSize = TilesetEditor.TileSize;
				spacing = TilesetEditor.Spacing;
				offsetX = TilesetEditor.OffsetX;
				offsetY = TilesetEditor.OffsetY;
				tileTex = TilesetEditor.Mat.mainTexture;
				tileLayer = TilesetEditor.LayerTileInfo;
			}
			else {
				tileSize = tileLayer.tileSize;
				spacing = tileLayer.spacing;
				offsetX = tileLayer.offsetX;
				offsetY = tileLayer.offsetY;
				tileTex = tileLayer.GetComponent<MeshRenderer>().sharedMaterial.mainTexture;
			}
		}
		if( tileTex == null )
			return;
		//		RepaintAllInspectors();
		position.height = 18;
		position.x += 12;
		EditorGUI.BeginProperty( position, label, property );
		property.isExpanded = EditorGUI.Foldout( position, property.isExpanded, label );
		if( property.isExpanded ) {
			
			Rect tileRect = new Rect( position.x + 4, position.y + 16, 64, 64 );
			if( Event.current.type == EventType.repaint )
				DrawTile( property, tileRect );
			if( GUI.Button( tileRect, GUIContent.none, GUIStyle.none ) ) {
				Tile tileToCopy = ( TilesetEditor.autoTileSelected == -1 || tileLayer == null ) ? TilesetEditor.selectedTiles[0,0] : tileLayer.autoTileData[TilesetEditor.autoTileSelected * 48 + 21];
				property.FindPropertyRelative( "xIndex" ).intValue = tileToCopy.xIndex;
				property.FindPropertyRelative( "yIndex" ).intValue = tileToCopy.yIndex;
				property.FindPropertyRelative( "flip" ).boolValue = tileToCopy.flip;
				property.FindPropertyRelative( "rotation" ).intValue = tileToCopy.rotation;
				property.FindPropertyRelative( "autoTileIndex" ).intValue = TilesetEditor.autoTileSelected;
			}
			tileRect.x += 72;
			tileRect.width = 128;
			if( GUI.Button( tileRect, "Remove" ) ) {
				property.FindPropertyRelative( "xIndex" ).intValue = -1;
				property.FindPropertyRelative( "yIndex" ).intValue = -1;
				property.FindPropertyRelative( "flip" ).boolValue = false;
				property.FindPropertyRelative( "rotation" ).intValue = -1;
				property.FindPropertyRelative( "autoTileIndex" ).intValue = -1;
			}
		}
		EditorGUI.EndProperty();
		
	}
	
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		if( !property.isExpanded )
			return 18;
		else return 82;
	}
	

}
[CustomEditor(typeof(MonoBehaviour),true)]
public class DummyMonoBehaviour : Editor {}
[CustomEditor(typeof(ScriptableObject),true)]
public class DummyScriptableObject : Editor {}

[CustomPropertyDrawer(typeof(SortingLayerIndexAttribute))]
public class SortingLayerIndexDrawer : PropertyDrawer {
	
	string[] sortingLayerNames {
		get {
			Type ieut = typeof( UnityEditorInternal.InternalEditorUtility );
			PropertyInfo properties = ieut.GetProperty( "sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic );
			return (string[])properties.GetValue( null, new object[0] );
		}
	}
	
	int[] sortingLayerIDs {
		get {
			Type ieut = typeof( UnityEditorInternal.InternalEditorUtility );
			PropertyInfo properties = ieut.GetProperty( "sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic );
			return (int[])properties.GetValue( null, new object[0] );
		}
	}
	
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		if( property.propertyType == SerializedPropertyType.Integer ) {
			string[] names = sortingLayerNames;
			int[] uniqueIDs = sortingLayerIDs;
			property.intValue = EditorGUI.IntPopup( position, label.text, property.intValue, names, uniqueIDs );
		}
	}
}

public class TilezoneAutotileCopyData {
	public int numberOfAutotiles;
	public string[] autoTileNames;
	public int[] autoTileLinkkMask;
	public Tile[] autoTileData;
	public TileInfo.AutoTileEdgeMode[] autoTileEdgeMode;
	public TileInfo.AutoTileType[] autoTileType;

	public TilezoneAutotileCopyData ( TileInfo tileInfo ) {
		this.numberOfAutotiles = tileInfo.numberOfAutotiles;
		this.autoTileNames = new string[tileInfo.numberOfAutotiles];
		this.autoTileLinkkMask = new int[tileInfo.numberOfAutotiles];
		this.autoTileData = new Tile[tileInfo.numberOfAutotiles*48];
		this.autoTileEdgeMode = new TileInfo.AutoTileEdgeMode[tileInfo.numberOfAutotiles];
		this.autoTileType = new TileInfo.AutoTileType[tileInfo.numberOfAutotiles];

		for( int i = 0; i < tileInfo.numberOfAutotiles; i++ ) {
			this.autoTileNames[i] = tileInfo.autoTileNames[i];
			this.autoTileLinkkMask[i] = tileInfo.autoTileLinkMask[i];
			this.autoTileEdgeMode[i] = tileInfo.autoTileEdgeMode[i];
			this.autoTileType[i] = tileInfo.autoTileType[i];
			for( int ii = 0; ii < 48; ii++ ) {
				this.autoTileData[i*48+ii] = new Tile( tileInfo.autoTileData[i*48+ii] );
			}
		}
	}
}
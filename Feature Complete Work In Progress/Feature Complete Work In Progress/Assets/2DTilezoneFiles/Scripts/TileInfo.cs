using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[AddComponentMenu("2D/Tile Info")]
public class TileInfo : MonoBehaviour {

	public enum CollisionType {
		None, Full, SlopeUpRight, SlopeUpLeft, Platform
	};

	public enum AutoTileEdgeMode {
		None, LinkToEdge, Wrap
	};

	public enum AutoTileType {
		Normal, IsometricWall
	};

	public class TilePolygonPoints {
		public bool isKnown;
		public Vector2[] points;
	}
	[HideInInspector]
	[System.NonSerialized]
	public TilePolygonPoints[,] knownTilePoints;

	[HideInInspector]
	/// <summary>
	/// List of the tiles in a 1D array in the format [ local y * mapWidth + local x ].
	/// To get the index of a tile from a world position use WorldPointToMapIndex
	/// </summary>
	public Tile[] tiles;
	[HideInInspector]
	public int mapWidth;
	[HideInInspector]
	public int mapHeight;
	//	[HideInInspector]
	//	public int _numberOfTiles;
	//
	//	public int numberOfTiles {
	//		get {
	//			return _numberOfTiles;
	//		}
	//	}

	//[HideInInspector]
	//public List<Vector2> tileIndex = new List<Vector2>();
//	[HideInInspector]
	public int tileSize;
//	[HideInInspector]
	public int spacing;
//	[HideInInspector]
	public int offsetX;
//	[HideInInspector]
	public int offsetY;
	[HideInInspector]
	public float zoomFactor = 1;
	[HideInInspector]
	public CollisionType[] collisions;
	//[HideInInspector]
	//public Vector3 positionAtLastEdit;
	[HideInInspector]
	public int numberOfAutotiles;
	[HideInInspector]
	public List<string> autoTileNames = new List<string>();
	[HideInInspector]
	public List<int> autoTileLinkMask = new List<int>();
	[HideInInspector]
	public List<Tile> autoTileData = new List<Tile>();
	[HideInInspector]
	public List<bool> showAutoTile = new List<bool>();
	[HideInInspector]
	public List<AutoTileEdgeMode> autoTileEdgeMode = new List<AutoTileEdgeMode>();
	[HideInInspector]
	public List<AutoTileType>autoTileType = new List<AutoTileType>();
	[HideInInspector] public bool pixelColliders;

	public TileInfo upLayer;
	public TileInfo downLayer;
	public TileInfo leftLayer;
	public TileInfo rightLayer;
	[Space(10)]
	public TileInfo upRightLayer;
	public TileInfo upLeftLayer;
	public TileInfo downRightLayer;
	public TileInfo downLeftLayer;

	//settings
	[HideInInspector]
	public bool update3DWalls = false;
	[HideInInspector]
	public float depthOf3DWalls = 10;
	[HideInInspector]
	public bool update2DColliders = false;

	[HideInInspector]
	public bool mapHasChanged = false;

	Vector2 _nonTransparentUV = new Vector2( -1, -1 );

	public Vector2 nonTransparentUV {
		get {

			if( _nonTransparentUV == new Vector2( -1, -1 ) ) {
				if( meshRenderer == null || meshRenderer.sharedMaterial == null || meshRenderer.sharedMaterial.mainTexture == null )
					return Vector2.zero;

				Texture2D tex = (Texture2D)meshRenderer.sharedMaterial.mainTexture;
				for( int x = 0; x < tex.width; x++ ) {
					for( int y = 0; y < tex.height; y++ ) {
						if( tex.GetPixel( x, y ).a == 1 ) {
							_nonTransparentUV = new Vector2( (float)x / (float)tex.width, (float)y / (float)tex.height );
							return _nonTransparentUV;
						}
					}
				}
			}
			return _nonTransparentUV;
		}
	}

	PolygonCollider2D _mainCollider;

	public PolygonCollider2D mainCollider {
		get {
			if( _mainCollider == null ) {
				PolygonCollider2D[] allColliders = GetComponents<PolygonCollider2D>();
				foreach( PolygonCollider2D col in allColliders ) {
					if( !col.usedByEffector )
						_mainCollider = col;
				}
				if( _mainCollider == null )
					_mainCollider = gameObject.AddComponent<PolygonCollider2D>();
			}
			return _mainCollider;
		}
	}

	PolygonCollider2D _platformCollider;

	public PolygonCollider2D platformCollider {
		get {
			if( _platformCollider == null ) {
				PolygonCollider2D[] allColliders = GetComponents<PolygonCollider2D>();
				foreach( PolygonCollider2D col in allColliders ) {
					if( col.usedByEffector )
						_platformCollider = col;
				}
				if( _platformCollider == null ) {
					_platformCollider = gameObject.AddComponent<PolygonCollider2D>();
					_platformCollider.usedByEffector = true;
					if( GetComponent<PlatformEffector2D>() == null )
						gameObject.AddComponent<PlatformEffector2D>();
				}
			}
			return _platformCollider;
		}
	}

	MeshRenderer _meshRenderer;

	public MeshRenderer meshRenderer {
		get {
			if( _meshRenderer == null )
				_meshRenderer = GetComponent<MeshRenderer>();
			return _meshRenderer;
		}
	}

	public static TileInfo[] _allMaps;
	public static TileInfo[] allMaps {
		get {
			if( _allMaps == null )
				_allMaps = FindObjectsOfType<TileInfo>();
			return _allMaps;
		}
	}

	public static TileInfo GetMapAtWorldPos ( Vector2 worldPos ) {
		foreach( TileInfo map in allMaps ) {
			if( map.WorldPointToMapIndex( worldPos ) != -1 )
				return map;
		}
		return null;
	}

	public static TileInfo[] GetMapsAtWorldPos ( Vector2 worldPos ) {
		List<TileInfo> result = new List<TileInfo>();
		foreach( TileInfo map in allMaps ) {
			if( map.WorldPointToMapIndex( worldPos ) != -1 )
				result.Add( map );
		}
		return result.ToArray();
	}

	public static bool CollisionAtWorldPos ( Vector2 worldPos ) {
		TileInfo[] maps = GetMapsAtWorldPos( worldPos );
		foreach( TileInfo map in maps ) {
			if( map.tiles[map.WorldPointToMapIndex(worldPos)].GetCollisionType( map ) != CollisionType.None )
				return true;
		}
		return false;
	}

	public static CollisionType CollisionTypeAtWorldPos ( Vector2 worldPos ) {
		TileInfo[] maps = GetMapsAtWorldPos( worldPos );
		CollisionType result = CollisionType.None;
		foreach( TileInfo map in maps ) {
			CollisionType tempResult = map.tiles[map.WorldPointToMapIndex(worldPos)].GetCollisionType( map );
			if( tempResult != CollisionType.None && result != CollisionType.Full )
				result = tempResult;
		}
		return result;
	}

	void AddWall ( Vector2 p1, Vector2 p2, List<Vector3> vertices, List<Vector2> uv, List<int> triangles ) {
		vertices.Add( new Vector3( p1.x, p1.y, depthOf3DWalls ) );
		vertices.Add( new Vector3( p1.x, p1.y, 0 ) );
		vertices.Add( new Vector3( p2.x, p2.y, 0 ) );
		vertices.Add( new Vector3( p2.x, p2.y, depthOf3DWalls ) );

		uv.Add( nonTransparentUV );
		uv.Add( nonTransparentUV );
		uv.Add( nonTransparentUV );
		uv.Add( nonTransparentUV );

		triangles.Add( vertices.Count - 4 );
		triangles.Add( vertices.Count - 3 );
		triangles.Add( vertices.Count - 1 );
		triangles.Add( vertices.Count - 1 );
		triangles.Add( vertices.Count - 3 );
		triangles.Add( vertices.Count - 2 );
	}

	void OnEnable () {
		if( GetComponent<MeshFilter>().sharedMesh == null ) {
			mapHasChanged = true;
			UpdateVisualMesh( true );
		}
	}

	public struct Collider2DEdgeConnection {
		public int index;
		public bool toP1;
		public Collider2DEdgeConnection ( int index, bool toP1 ) {
			this.index = index;
			this.toP1 = toP1;
		}
	}

	public struct Collider2DEdge {
		public Vector2 p1;
		public Vector2 p2;
		public List<Collider2DEdgeConnection> p1Connections;
		public List<Collider2DEdgeConnection> p2Connections;
		public bool processed;
		public Collider2DEdge ( Vector2 p1, Vector2 p2 ) {
			this.p1 = p1;
			this.p2 = p2;
			this.p1Connections = new List<Collider2DEdgeConnection>();
			this.p2Connections = new List<Collider2DEdgeConnection>();
			this.processed = false;
		}
	}
	bool Approximately( Vector2 v1, Vector2 v2 ) {
		float tolerance = 0.05f * zoomFactor;
		return ( v1.x > v2.x - tolerance && v1.x < v2.x + tolerance && v1.y > v2.y - tolerance && v1.y < v2.y + tolerance );
	}

	void AddEdge( List<Collider2DEdge> edges, Vector2 p1, Vector2 p2 ) {
		int index = edges.Count;
		Collider2DEdge thisEdge = new Collider2DEdge( p1, p2 );
		for (int i = 0; i < index; i++) {
			if( Approximately( edges[i].p1, p1 ) ) {
				edges[i].p1Connections.Add( new Collider2DEdgeConnection( index, true ) );
				thisEdge.p1Connections.Add( new Collider2DEdgeConnection( i, true ) );
			}
			else if( Approximately( edges[i].p1, p2 ) ) {
				edges[i].p1Connections.Add( new Collider2DEdgeConnection( index, false ) );
				thisEdge.p2Connections.Add( new Collider2DEdgeConnection( i, true ) );
			}
			else if( Approximately( edges[i].p2, p1 ) ) {
				edges[i].p2Connections.Add( new Collider2DEdgeConnection( index, true ) );
				thisEdge.p1Connections.Add( new Collider2DEdgeConnection( i, false ) );
			}
			else if( Approximately( edges[i].p2, p2 ) ) {
				edges[i].p2Connections.Add( new Collider2DEdgeConnection( index, false ) );
				thisEdge.p2Connections.Add( new Collider2DEdgeConnection( i, false ) );
			}
		}
		edges.Add( thisEdge );
	}

	void RemoveEdgeConnections( List<Collider2DEdge> edges, int index ) {
//		if( isP2End ) {
			for( int i = 0; i < edges[index].p2Connections.Count; i++ ) {
				int j = edges[index].p2Connections[i].index;
				if( edges[index].p2Connections[i].toP1 ) {
					for( int k = 0; k < edges[j].p1Connections.Count; k++ ) {
						if( edges[j].p1Connections[k].index == index ) {
							edges[j].p1Connections.RemoveAt( k );
							break;
						}
					}
				}
				else {
					for( int k = 0; k < edges[j].p2Connections.Count; k++ ) {
						if( edges[j].p2Connections[k].index == index ) {
							edges[j].p2Connections.RemoveAt( k );
							break;
						}
					}
				}
			}
//		}
//		else {
			for( int i = 0; i < edges[index].p1Connections.Count; i++ ) {
				int j = edges[index].p1Connections[i].index;
				if( edges[index].p1Connections[i].toP1 ) {
					for( int k = 0; k < edges[j].p1Connections.Count; k++ ) {
						if( edges[j].p1Connections[k].index == index ) {
							edges[j].p1Connections.RemoveAt( k );
							break;
						}
					}
				}
				else {
					for( int k = 0; k < edges[j].p2Connections.Count; k++ ) {
						if( edges[j].p2Connections[k].index == index ) {
							edges[j].p2Connections.RemoveAt( k );
							break;
						}
					}
				}
			}
//		}
	}

	void PolygoniseEdges ( List<Vector2[]> polygons, List<Collider2DEdge> edges ) {
		if( edges.Count == 0 )
			return;
		List<Vector2> currentPolygon = new List<Vector2>();
		currentPolygon.Add( edges[0].p1 );
		bool isP2End = true;
		List<int> edgeIndices = new List<int>( edges.Count );
		for( int i = 0; i < edges.Count; i++ ) {
			edgeIndices.Add( i );
		}
		int index = 0;
		while( edgeIndices.Count > 0 ) {
			edgeIndices.Remove( index );

			if( ( isP2End && edges[index].p2Connections.Count == 0 ) || ( !isP2End && edges[index].p1Connections.Count == 0 ) ) {
				// no loop found polygon is complete
				RemoveEdgeConnections( edges, index );
				polygons.Add( currentPolygon.ToArray() );
				currentPolygon.Clear();
				if( edgeIndices.Count == 0 )
					break;
				index = edgeIndices[0];
				currentPolygon.Add( edges[index].p1 );
				isP2End = true;
				continue;
			}

			Vector3 thisPoint = isP2End ? edges[index].p2 : edges[index].p1;
			int newIndex = isP2End ? edges[index].p2Connections[0].index : edges[index].p1Connections[0].index;
			bool newIsP2End = isP2End ? edges[index].p2Connections[0].toP1 : edges[index].p1Connections[0].toP1;

			//check if loop is complete
//			if( Approximately( thisPoint, currentPolygon[0] ) ) {
//				result.Add( currentPolygon.ToArray() );
//				currentPolygon.Clear();
//				if( edgeIndices.Count == 0 )
//					break;
//				index = edgeIndices[0];
//				currentPolygon.Add( edges[index].p1 );
//				isP2End = true;
//				continue;
//			}

			//check if the point lays on the same line as the previous point and the next point
			if( isP2End && newIsP2End ) {
				if( ( !Mathf.Approximately( edges[index].p1.x, thisPoint.x ) || !Mathf.Approximately( thisPoint.x, edges[newIndex].p2.x ) ) &&
					( !Mathf.Approximately( edges[index].p1.y, thisPoint.y ) || !Mathf.Approximately( thisPoint.y, edges[newIndex].p2.y ) ) )
					currentPolygon.Add( thisPoint );
			}
			if( isP2End && !newIsP2End ) {
				if( ( !Mathf.Approximately( edges[index].p1.x, thisPoint.x ) || !Mathf.Approximately( thisPoint.x, edges[newIndex].p1.x ) ) &&
					( !Mathf.Approximately( edges[index].p1.y, thisPoint.y ) || !Mathf.Approximately( thisPoint.y, edges[newIndex].p1.y ) ) )
					currentPolygon.Add( thisPoint );
			}
			if( !isP2End && newIsP2End ) {
				if( ( !Mathf.Approximately( edges[index].p2.x, thisPoint.x ) || !Mathf.Approximately( thisPoint.x, edges[newIndex].p2.x ) ) &&
					( !Mathf.Approximately( edges[index].p2.y, thisPoint.y ) || !Mathf.Approximately( thisPoint.y, edges[newIndex].p2.y ) ) )
					currentPolygon.Add( thisPoint );
			}
			if( !isP2End && !newIsP2End ) {
				if( ( !Mathf.Approximately( edges[index].p2.x, thisPoint.x ) || !Mathf.Approximately( thisPoint.x, edges[newIndex].p1.x ) ) &&
					( !Mathf.Approximately( edges[index].p2.y, thisPoint.y ) || !Mathf.Approximately( thisPoint.y, edges[newIndex].p1.y ) ) )
					currentPolygon.Add( thisPoint );
			}
			RemoveEdgeConnections( edges, index );

			index = newIndex;
			isP2End = newIsP2End;
		}

		return;
	}

	/// <summary>
	/// Updates the visual mesh.
	/// </summary>
	public void UpdateVisualMesh ( bool fromEditor ) {
		if( !mapHasChanged )
			return;
		mapHasChanged = false;
		List<Collider2DEdge> edges = new List<Collider2DEdge>();
		List<Vector2[]> polygons = new List<Vector2[]>();

		Mesh m = new Mesh();
		//		Vector3[] vertices = new Vector3[numberOfTiles * 4];
		//		Vector2[] uv = new Vector2[numberOfTiles * 4];
		//		int[] triangles = new int[numberOfTiles * 6];

		List<Vector3> vertices = new List<Vector3>();
		//vertices.AddRange( new Vector3[numberOfTiles * 4] );
		List<Vector2> uv = new List<Vector2>();
		//uv.AddRange( new Vector2[numberOfTiles * 4] );
		List<int> triangles = new List<int>();
		//triangles.AddRange( new int[numberOfTiles * 6] );

		//int i = 0;

		if( pixelColliders && ( knownTilePoints == null || knownTilePoints.Length == 0 ) ) {
			knownTilePoints = new TilePolygonPoints[width,height];
		}
		for( int x = 0; x < mapWidth; x++ ) {
			for( int y = 0; y < mapHeight; y++ ) {
				UpdateAutoTile( x, y );
			}
		}
		for( int x = 0; x < mapWidth; x++ ) {
			for( int y = 0; y < mapHeight; y++ ) {

//				UpdateAutoTile( x, y );

				if( tiles[ y * mapWidth + x ] == Tile.empty )
					continue;

				int i = vertices.Count / 4;

				vertices.AddRange( new Vector3[4] );
				uv.AddRange( new Vector2[4] );
				triangles.AddRange( new int[6] );



				vertices[i*4 + tiles[y * mapWidth + x].rotation ] = new Vector3( x, y );
				vertices[i*4 + (tiles[y * mapWidth + x].rotation + 1) % 4] = new Vector3( x + 1, y );
				vertices[i*4 + (tiles[y * mapWidth + x].rotation + 2) % 4] = new Vector3( x + 1, y + 1 );
				vertices[i*4 + (tiles[y * mapWidth + x].rotation + 3) % 4] = new Vector3( x, y + 1 );

				triangles[i*6] = i*4;
				triangles[i*6 + 1] = i*4 + 2;
				triangles[i*6 + 2] = i*4 + 1;

				triangles[i*6 + 3] = i*4;
				triangles[i*6 + 4] = i*4 + 3;
				triangles[i*6 + 5] = i*4 + 2;

				Material mat = meshRenderer.sharedMaterial;
				//Vector2 uvBottomLeft = new Vector2( (float)(tiles[ y * mapWidth + x ].xIndex * (tileSize + spacing)) / mat.mainTexture.width, 1f - (float)((tiles[ y * mapWidth + x ].yIndex + 1)  * (float)(tileSize + spacing) - spacing) / (float)mat.mainTexture.height );
				Vector2 uvBottomLeft = tiles[ y * mapWidth + x ].GetBottomLeftUV( this );
				Vector2 uvRight = new Vector2( (float)tileSize / mat.mainTexture.width, 0 );
				Vector2 uvTop = new Vector2( 0, (float)tileSize / mat.mainTexture.height );

				if( tiles[y * mapWidth + x].flip ) {
					uv[i*4] = uvBottomLeft + uvRight;
					uv[i*4 + 1] = uvBottomLeft;
					uv[i*4 + 2] = uvBottomLeft + uvTop;
					uv[i*4 + 3] = uvBottomLeft + uvRight + uvTop;
				}
				else {
					uv[i*4] = uvBottomLeft;
					uv[i*4 + 1] = uvBottomLeft + uvRight;
					uv[i*4 + 2] = uvBottomLeft + uvRight + uvTop;
					uv[i*4 + 3] = uvBottomLeft + uvTop;
				}

				//i++;

				if( update3DWalls || update2DColliders ) {

					Vector2 ti = (Vector2)tiles[y * mapWidth + x];
					//int width = (gameObject.GetComponent<Renderer>().sharedMaterial.mainTexture.width / (tileSize + spacing));


//					bool up = LocalPointToMapIndex(new Vector2( x, y + 1 )) != -1 && tiles[(y+1) * mapWidth + x] != Tile.empty && collisions[tiles[(y+1) * mapWidth + x].yIndex * width + tiles[(y+1) * mapWidth + x].xIndex] == CollisionType.Full;
//					bool down = LocalPointToMapIndex(new Vector2( x, y - 1 )) != -1 && tiles[(y-1) * mapWidth + x] != Tile.empty && collisions[tiles[(y-1) * mapWidth + x].yIndex * width + tiles[(y-1) * mapWidth + x].xIndex] == CollisionType.Full;
//					bool left = LocalPointToMapIndex(new Vector2( x - 1, y )) != -1 && tiles[y * mapWidth + (x-1)] != Tile.empty && collisions[tiles[y * mapWidth + (x-1)].yIndex * width + tiles[y * mapWidth + (x-1)].xIndex] == CollisionType.Full;
//					bool right = LocalPointToMapIndex(new Vector2( x + 1, y )) != -1 && tiles[y * mapWidth + (x+1)] != Tile.empty && collisions[tiles[y * mapWidth + (x+1)].yIndex * width + tiles[y * mapWidth + (x+1)].xIndex] == CollisionType.Full;

					int index = LocalPointToMapIndex(new Vector2( x, y + 1 ));
					bool up = index >= 0 && tiles[index].IsCollisionDown( this );
					index = LocalPointToMapIndex(new Vector2( x, y - 1 ));
					bool down = index >= 0 && tiles[index].IsCollisionUp( this );
					index = LocalPointToMapIndex(new Vector2( x - 1, y ));
					bool left = index >= 0 && tiles[index].IsCollisionRight( this );
					index = LocalPointToMapIndex(new Vector2( x + 1, y ));
					bool right = index >= 0 && tiles[index].IsCollisionLeft( this );

					if( update2DColliders ) {
						switch( collisions[ (int)ti.y * width + (int)ti.x ] ) {
						case CollisionType.Full:
							if( !up )
								AddEdge( edges, vertices[i*4 + (tiles[y * mapWidth + x].rotation + 2) % 4], vertices[i*4 + (tiles[y * mapWidth + x].rotation + 3) % 4] );

							if( !down )
								AddEdge( edges, vertices[i*4 + tiles[y * mapWidth + x].rotation], vertices[i*4 + (tiles[y * mapWidth + x].rotation + 1) % 4] );

							if( !left )
								AddEdge( edges, vertices[i*4 + (tiles[y * mapWidth + x].rotation + 3) % 4], vertices[i*4 + tiles[y * mapWidth + x].rotation] );

							if( !right )
								AddEdge( edges, vertices[i*4 + (tiles[y * mapWidth + x].rotation + 1) % 4], vertices[i*4 + (tiles[y * mapWidth + x].rotation + 2) % 4] );
							break;
						}
					}

//					if( y == mapHeight - 1 && upLayer != null ) {
//						Vector2 worldPos = transform.position + new Vector3( x, y+1 );
//						index = upLayer.WorldPointToMapIndex(worldPos);
//						up = index != -1 && upLayer.tiles[index] != Tile.empty && upLayer.collisions[upLayer.tiles[index].yIndex * upLayer.width + upLayer.tiles[index].xIndex] == CollisionType.Full;
//					}
//					if( y == 0 && downLayer != null ) {
//						Vector2 worldPos = transform.position + new Vector3( x, y-1 );
//						index = downLayer.WorldPointToMapIndex(worldPos);
//						down = index != -1 && downLayer.tiles[index] != Tile.empty && downLayer.collisions[downLayer.tiles[index].yIndex * downLayer.width + downLayer.tiles[index].xIndex] == CollisionType.Full;
//					}
//					if( x == 0 && leftLayer != null ) {
//						Vector2 worldPos = transform.position + new Vector3( x-1, y );
//						index = leftLayer.WorldPointToMapIndex(worldPos);
//						left = index != -1 && leftLayer.tiles[index] != Tile.empty && leftLayer.collisions[leftLayer.tiles[index].yIndex * leftLayer.width + leftLayer.tiles[index].xIndex] == CollisionType.Full;
//					}
//					if( x == mapWidth - 1 && rightLayer != null ) {
//						Vector2 worldPos = transform.position + new Vector3( x+1, y );
//						index = rightLayer.WorldPointToMapIndex(worldPos);
//						right = index != -1 && rightLayer.tiles[index] != Tile.empty && rightLayer.collisions[rightLayer.tiles[index].yIndex * rightLayer.width + rightLayer.tiles[index].xIndex] == CollisionType.Full;
//					}


					if( pixelColliders ) {
//						if( pixelColliders ) {
						switch( collisions[ (int)ti.y * width + (int)ti.x ] ) {
						case CollisionType.Full:
							if( !up )
								AddWall( vertices[i*4 + (tiles[y * mapWidth + x].rotation + 2) % 4], vertices[i*4 + (tiles[y * mapWidth + x].rotation + 3) % 4], vertices, uv, triangles );

							if( !down )
								AddWall( vertices[i*4 + tiles[y * mapWidth + x].rotation], vertices[i*4 + (tiles[y * mapWidth + x].rotation + 1) % 4], vertices, uv, triangles );

							if( !left )
								AddWall( vertices[i*4 + (tiles[y * mapWidth + x].rotation + 3) % 4], vertices[i*4 + tiles[y * mapWidth + x].rotation], vertices, uv, triangles );

							if( !right )
								AddWall( vertices[i*4 + (tiles[y * mapWidth + x].rotation + 1) % 4], vertices[i*4 + (tiles[y * mapWidth + x].rotation + 2) % 4], vertices, uv, triangles );
							break;

						case CollisionType.SlopeUpRight:
						case CollisionType.SlopeUpLeft:
						case CollisionType.Platform:

							Vector2[] points;
							Tile thisTile = tiles[y * mapWidth + x];
							if( knownTilePoints[ thisTile.xIndex, thisTile.yIndex ] == null )
								knownTilePoints[ thisTile.xIndex, thisTile.yIndex ] = new TilePolygonPoints();

							if( knownTilePoints[ thisTile.xIndex, thisTile.yIndex ].isKnown ) {
								points = (Vector2[])knownTilePoints[ thisTile.xIndex, thisTile.yIndex ].points.Clone();
							}
							else {
								points = AutoColliderGenerator.GetTilePoints( this, thisTile );
								knownTilePoints[ thisTile.xIndex, thisTile.yIndex ].points = (Vector2[])points.Clone();
								knownTilePoints[ thisTile.xIndex, thisTile.yIndex ].isKnown = true;
							}

							for( int n = 0; n < points.Length; n++ ) {
								if( thisTile.flip ) {
									points[n].x = 1 - points[n].x;
								}
								Vector2 tempPoint = points[n];
								switch( thisTile.rotation ) {
								case 1:
									points[n].x = tempPoint.y;
									points[n].y = 1-tempPoint.x;
									break;
								case 2:
									points[n].x = 1-tempPoint.x;
									points[n].y = 1-tempPoint.y;
									break;
								case 3:
									points[n].x = 1-tempPoint.y;
									points[n].y = tempPoint.x;
									break;
								}

								points[n] += new Vector2( x, y );
							}

							if( update3DWalls ) {
								for( int n = 0; n < points.Length; n++ ) {

									if( up && points[n].y - y == 1 && points[(n+1)%points.Length].y - y == 1 )
										continue;
									if( down && points[n].y - y == 0 && points[(n+1)%points.Length].y - y == 0 )
										continue;
									if( left && points[n].x - x == 0 && points[(n+1)%points.Length].x - x == 0 )
										continue;
									if( right && points[n].x - x == 1 && points[(n+1)%points.Length].x - x == 1 )
										continue;

									if( thisTile.flip )
										AddWall( points[n], points[(n+1)%points.Length], vertices, uv, triangles );
									else
										AddWall( points[(n+1)%points.Length], points[n], vertices, uv, triangles );
								}
							}

							if( update2DColliders && points.Length > 2 ) {
								polygons.Add( points );
							}

							break;
						}
					}
					else {
//							Vector2[] points;
						Vector2[] pointsB;

						bool flip = tiles[ y * mapWidth + x ].flip;
						int rotation = tiles[y * mapWidth + x].rotation;

						switch( collisions[ (int)ti.y * width + (int)ti.x ] ) {
						case CollisionType.Full:
						case CollisionType.Platform:
							if( update3DWalls ) {
								if( !up )
									AddWall( vertices[i*4 + (rotation + 2) % 4], vertices[i*4 + (rotation + 3) % 4], vertices, uv, triangles );

								if( !down )
									AddWall( vertices[i*4 + rotation], vertices[i*4 + (rotation + 1) % 4], vertices, uv, triangles );

								if( !left )
									AddWall( vertices[i*4 + (rotation + 3) % 4], vertices[i*4 + rotation], vertices, uv, triangles );

								if( !right )
									AddWall( vertices[i*4 + (rotation + 1) % 4], vertices[i*4 + (rotation + 2) % 4], vertices, uv, triangles );
							}

							break;

						case CollisionType.SlopeUpRight:

//								points = new Vector2[2];
							pointsB = new Vector2[4];
							pointsB[0] = new Vector2( x, y );
							pointsB[1] = new Vector2( x+1, y );
							pointsB[2] = new Vector2( x+1, y+1 );
							pointsB[3] = new Vector2( x, y+1 );
							if( flip ) {
//									points[0] = pointsB[(1 + 4 - tiles[y * mapWidth + x].rotation) % 4];
//									points[1] = pointsB[(3 + 4 - tiles[y * mapWidth + x].rotation) % 4];
								if( update3DWalls )
									AddWall( pointsB[(1 + 4 - rotation) % 4], pointsB[(3 + 4 - rotation) % 4], vertices, uv, triangles );
								if( update2DColliders )
									AddEdge( edges, pointsB[(1 + 4 - rotation) % 4], pointsB[(3 + 4 - rotation) % 4] );
								if( !up && ( rotation == 2 || rotation == 1 ) ) {
									if( update3DWalls )
										AddWall( pointsB[2], pointsB[3], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[2], pointsB[3] );
								}
								if( !down && ( rotation == 0 || rotation == 3 ) ) {
									if( update3DWalls )
										AddWall( pointsB[0], pointsB[1], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[0], pointsB[1] );
								}
								if( !left && ( rotation == 1 || rotation == 0 ) ) {
									if( update3DWalls )
										AddWall( pointsB[3], pointsB[0], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[3], pointsB[0] );
								}
								if( !right && ( rotation == 3 || rotation == 2 ) ) {
									if( update3DWalls )
										AddWall( pointsB[1], pointsB[2], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[1], pointsB[2] );
								}
									
							}
							else
							{
//									points[0] = pointsB[(0 + 4 - tiles[y * mapWidth + x].rotation) % 4];
//									points[1] = pointsB[(2 + 4 - tiles[y * mapWidth + x].rotation) % 4];
								if( update3DWalls )
									AddWall( pointsB[(2 + 4 - rotation) % 4], pointsB[(0 + 4 - rotation) % 4], vertices, uv, triangles );
								if( update2DColliders )
									AddEdge( edges, pointsB[(2 + 4 - rotation) % 4], pointsB[(0 + 4 - rotation) % 4] );
								if( !up && ( rotation == 2 || rotation == 3 ) ) {
									if( update3DWalls )
										AddWall( pointsB[2], pointsB[3], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[2], pointsB[3] );
								}
								if( !down && ( rotation == 1 || rotation == 0 ) ) {
									if( update3DWalls )
										AddWall( pointsB[0], pointsB[1], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[0], pointsB[1] );
								}
								if( !left && ( rotation == 2 || rotation == 1 ) ) {
									if( update3DWalls )
										AddWall( pointsB[3], pointsB[0], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[3], pointsB[0] );
								}
								if( !right && ( rotation == 0 || rotation == 3 ) ) {
									if( update3DWalls )
										AddWall( pointsB[1], pointsB[2], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[1], pointsB[2] );
								}
							}

//								if( flip ) {
//									AddWall( points[0], points[1], vertices, uv, triangles );
//								}
//								else
//									AddWall( points[1], points[0], vertices, uv, triangles );

							break;

						case CollisionType.SlopeUpLeft:

//								points = new Vector2[2];
							pointsB = new Vector2[4];
							pointsB[0] = new Vector2( x, y );
							pointsB[1] = new Vector2( x+1, y );
							pointsB[2] = new Vector2( x+1, y+1 );
							pointsB[3] = new Vector2( x, y+1 );
							if( flip ) {
//									points[0] = pointsB[(0 + 4 - tiles[y * mapWidth + x].rotation) % 4];
//									points[1] = pointsB[(2 + 4 - tiles[y * mapWidth + x].rotation) % 4];
								if( update3DWalls )
									AddWall( pointsB[(2 + 4 - rotation) % 4], pointsB[(0 + 4 - rotation) % 4], vertices, uv, triangles );
								if( update2DColliders )
									AddEdge( edges, pointsB[(2 + 4 - rotation) % 4], pointsB[(0 + 4 - rotation) % 4] );
								if( !up && ( rotation == 3 || rotation == 2 ) ) {
									if( update3DWalls )
										AddWall( pointsB[2], pointsB[3], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[2], pointsB[3] );
								}
								if( !down && ( rotation == 1 || rotation == 0 ) ) {
									if( update3DWalls )
										AddWall( pointsB[0], pointsB[1], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[0], pointsB[1] );
								}
								if( !left && ( rotation == 2 || rotation == 1 ) ) {
									if( update3DWalls )
										AddWall( pointsB[3], pointsB[0], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[3], pointsB[0] );
								}
								if( !right && ( rotation == 0 || rotation == 3 ) ) {
									if( update3DWalls )
										AddWall( pointsB[1], pointsB[2], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[1], pointsB[2] );
								}
							}
							else {
//									points[0] = pointsB[(1 + 4 - tiles[y * mapWidth + x].rotation) % 4];
//									points[1] = pointsB[(3 + 4 - tiles[y * mapWidth + x].rotation) % 4];
								if( update3DWalls )
									AddWall( pointsB[(1 + 4 - rotation) % 4], pointsB[(3 + 4 - rotation) % 4], vertices, uv, triangles );
								if( update2DColliders )
									AddEdge( edges, pointsB[(1 + 4 - rotation) % 4], pointsB[(3 + 4 - rotation) % 4] );
								if( !up && ( rotation == 2 || rotation == 1 ) ) {
									if( update3DWalls )
										AddWall( pointsB[2], pointsB[3], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[2], pointsB[3] );
								}
								if( !down && ( rotation == 0 || rotation == 3 ) ) {
									if( update3DWalls )
										AddWall( pointsB[0], pointsB[1], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[0], pointsB[1] );
								}
								if( !left && ( rotation == 1 || rotation == 0 ) ) {
									if( update3DWalls )
										AddWall( pointsB[3], pointsB[0], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[3], pointsB[0] );
								}
								if( !right && ( rotation == 3 || rotation == 2 ) ) {
									if( update3DWalls )
										AddWall( pointsB[1], pointsB[2], vertices, uv, triangles );
									if( update2DColliders )
										AddEdge( edges, pointsB[1], pointsB[2] );
								}
							}

//								if( flip )
//									AddWall( points[1], points[0], vertices, uv, triangles );
//								else
//									AddWall( points[0], points[1], vertices, uv, triangles );

							break;
						}
					}


				}
			}
		}

		m.vertices = vertices.ToArray();
		m.triangles = triangles.ToArray();
		m.uv = uv.ToArray();
		m.RecalculateNormals();

		GetComponent<MeshFilter>().sharedMesh = m;
		MeshCollider mc = GetComponent<MeshCollider>();
		if( mc != null ) {
			mc.sharedMesh = m;
		}

		if( update2DColliders ) {
			PolygoniseEdges( polygons, edges );
			mainCollider.pathCount = polygons.Count;
			for( int i = 0; i < polygons.Count; i++ ) {
				mainCollider.SetPath( i, polygons[i] );
			}
		}
	}

	bool AutoTileIsLinked( int otherTile, int thisTile ) {
		if( otherTile == -1 || thisTile == -1 )
			return false;
//		return (autoTileLinkMask[thisTile] & (int)Mathf.Pow( 2, otherTile )) == (int)Mathf.Pow( 2, otherTile );
		return (autoTileLinkMask[thisTile] & (1<<otherTile)) == (1<<otherTile);
	}

	void UpdateAutoTile ( int x, int y ) {

		if( LocalPointToMapIndex( new Vector2( x, y ) ) == -1 )
			return;
		if( tiles[ LocalPointToMapIndex( new Vector2( x, y ) ) ].autoTileIndex == -1 )
			return;
		if( tiles[ LocalPointToMapIndex( new Vector2( x, y ) ) ].autoTileIndex >= numberOfAutotiles )
			return;

		bool up, down, left, right;
		bool upRight, upLeft, downRight, downLeft;

		int autoTileIndex = tiles[ LocalPointToMapIndex( new Vector2( x, y ) ) ].autoTileIndex;

		Tile selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 21 ] );

		if( autoTileType[autoTileIndex] == AutoTileType.Normal ) {

			switch( autoTileEdgeMode[autoTileIndex] ) {

			case AutoTileEdgeMode.None:
			default:
				up = LocalPointToMapIndex( new Vector2( x, y + 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				down = LocalPointToMapIndex( new Vector2( x, y - 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				left = LocalPointToMapIndex( new Vector2( x - 1, y ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y ) ) ].autoTileIndex, autoTileIndex );
				right = LocalPointToMapIndex( new Vector2( x + 1, y ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y ) ) ].autoTileIndex, autoTileIndex );

				upRight = LocalPointToMapIndex( new Vector2( x + 1, y + 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				upLeft = LocalPointToMapIndex( new Vector2( x - 1, y +1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				downRight = LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				downLeft = LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				break;

			case AutoTileEdgeMode.LinkToEdge:
				up = LocalPointToMapIndex( new Vector2( x, y + 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				down = LocalPointToMapIndex( new Vector2( x, y - 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				left = LocalPointToMapIndex( new Vector2( x - 1, y ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y ) ) ].autoTileIndex, autoTileIndex );
				right = LocalPointToMapIndex( new Vector2( x + 1, y ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y ) ) ].autoTileIndex, autoTileIndex );

				upRight = LocalPointToMapIndex( new Vector2( x + 1, y + 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				upLeft = LocalPointToMapIndex( new Vector2( x - 1, y +1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				downRight = LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				downLeft = LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				break;

			case AutoTileEdgeMode.Wrap:
				up = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, Mod(y + 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				down = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, Mod(y - 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				left = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x - 1, mapWidth), y ) ) ].autoTileIndex, autoTileIndex );
				right = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x + 1, mapWidth), y ) ) ].autoTileIndex, autoTileIndex );

				upRight = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x + 1, mapWidth), Mod(y + 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				upLeft = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x - 1, mapWidth), Mod(y + 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				downRight = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x + 1, mapWidth), Mod(y - 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				downLeft = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x - 1, mapWidth), Mod(y - 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				break;
			}



//			if( x == 0 && leftLayer != null ) {
//				left = leftLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x-1, y ) ) != -1 &&
//					AutoTileIsLinked( leftLayer.tiles[ leftLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x-1, y ) ) ].autoTileIndex, autoTileIndex );
//				if( y != 0 )
//					downLeft = leftLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x-1, y-1 ) ) != -1 &&
//						AutoTileIsLinked( leftLayer.tiles[ leftLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x-1, y-1 ) ) ].autoTileIndex, autoTileIndex );
//				if( y != mapHeight-1 )
//					upLeft = leftLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x-1, y+1 ) ) != -1 &&
//						AutoTileIsLinked( leftLayer.tiles[ leftLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x-1, y+1 ) ) ].autoTileIndex, autoTileIndex );
//			}
//			if( x == mapWidth - 1 && rightLayer != null ) {
//				right = rightLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x+1, y ) ) != -1 &&
//					AutoTileIsLinked( rightLayer.tiles[ rightLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x+1, y ) ) ].autoTileIndex, autoTileIndex );
//				if( y != 0 )
//					downRight = rightLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x+1, y-1 ) ) != -1 &&
//						AutoTileIsLinked( rightLayer.tiles[ rightLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x+1, y-1 ) ) ].autoTileIndex, autoTileIndex );
//				if( y != mapHeight-1 )
//					upRight = rightLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x+1, y+1 ) ) != -1 &&
//						AutoTileIsLinked( rightLayer.tiles[ rightLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x+1, y+1 ) ) ].autoTileIndex, autoTileIndex );
//			}
//			if( y == 0 && downLayer != null ) {
//				down = downLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x, y-1 ) ) != -1 &&
//					AutoTileIsLinked( downLayer.tiles[ downLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x, y-1 ) ) ].autoTileIndex, autoTileIndex );
//				if( x != 0 )
//					downLeft = downLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x-1, y-1 ) ) != -1 &&
//						AutoTileIsLinked( downLayer.tiles[ downLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x-1, y-1 ) ) ].autoTileIndex, autoTileIndex );
//				if( x != mapWidth-1 )
//					downRight = downLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x+1, y-1 ) ) != -1 &&
//						AutoTileIsLinked( downLayer.tiles[ downLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x+1, y-1 ) ) ].autoTileIndex, autoTileIndex );
//			}
//			if( y == mapHeight - 1 && upLayer != null ) {
//				up = upLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x, y+1 ) ) != -1 &&
//					AutoTileIsLinked( upLayer.tiles[ upLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x, y+1 ) ) ].autoTileIndex, autoTileIndex );
//				if( x != 0 )
//					upLeft = upLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x-1, y+1 ) ) != -1 &&
//						AutoTileIsLinked( upLayer.tiles[ upLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x-1, y+1 ) ) ].autoTileIndex, autoTileIndex );
//				if( x != mapWidth-1 )
//					upRight = upLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x+1, y+1 ) ) != -1 &&
//						AutoTileIsLinked( upLayer.tiles[ upLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x+1, y+1 ) ) ].autoTileIndex, autoTileIndex );
//			}
//
//			if( x == 0 && y == 0 && downLeftLayer != null )
//				downLeft = downLeftLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x-1, y-1 ) ) != -1 &&
//					AutoTileIsLinked( downLeftLayer.tiles[ downLeftLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x-1, y-1 ) ) ].autoTileIndex, autoTileIndex );
//			if( x == 0 && y == mapHeight - 1 && upLeftLayer != null )
//				upLeft = upLeftLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x-1, y+1 ) ) != -1 &&
//					AutoTileIsLinked( upLeftLayer.tiles[ upLeftLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x-1, y+1 ) ) ].autoTileIndex, autoTileIndex );
//			if( x == mapWidth - 1 && y == 0 && downRightLayer != null )
//				downRight = downRightLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x+1, y-1 ) ) != -1 &&
//					AutoTileIsLinked( downRightLayer.tiles[ downRightLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x+1, y-1 ) ) ].autoTileIndex, autoTileIndex );
//			if( x == mapWidth - 1 && y == mapHeight - 1 && upRightLayer != null )
//				upRight = upRightLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x+1, y+1 ) ) != -1 &&
//					AutoTileIsLinked( upRightLayer.tiles[ upRightLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( x+1, y+1 ) ) ].autoTileIndex, autoTileIndex );

			//new code
			if( autoTileEdgeMode[autoTileIndex] == AutoTileEdgeMode.None ) {
				if( x == 0 ) {
					left = AutoTileIsLinked( GetWorldAutoTileIndex( x-1, y ), autoTileIndex );
					downLeft = AutoTileIsLinked( GetWorldAutoTileIndex( x-1, y-1 ), autoTileIndex );
					upLeft = AutoTileIsLinked( GetWorldAutoTileIndex( x-1, y+1 ), autoTileIndex );
				}
				if( x == mapWidth - 1 ) {
					right = AutoTileIsLinked( GetWorldAutoTileIndex( x+1, y ), autoTileIndex );
					downRight = AutoTileIsLinked( GetWorldAutoTileIndex( x+1, y-1 ), autoTileIndex );
					upRight = AutoTileIsLinked( GetWorldAutoTileIndex( x+1, y+1 ), autoTileIndex );
				}
				if( y == 0 ) {
					down = AutoTileIsLinked( GetWorldAutoTileIndex( x, y-1 ), autoTileIndex );
					if( x != 0 )
						downLeft = AutoTileIsLinked( GetWorldAutoTileIndex( x-1, y-1 ), autoTileIndex );
					if( x != mapWidth-1 )
						downRight = AutoTileIsLinked( GetWorldAutoTileIndex( x+1, y-1 ), autoTileIndex );
				}
				if( y == mapHeight - 1 ) {
					up = AutoTileIsLinked( GetWorldAutoTileIndex( x, y+1 ), autoTileIndex );
					if( x != 0 )
						upLeft = AutoTileIsLinked( GetWorldAutoTileIndex( x-1, y+1 ), autoTileIndex );
					if( x != mapWidth-1 )
						upRight = AutoTileIsLinked( GetWorldAutoTileIndex( x+1, y+1 ), autoTileIndex );
				}
			}


			if( !up && !left && down && right && downRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 0 ] );

			if( !up && left && down && right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 1 ] );

			if( !up && left && down && !right && downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 2 ] );

			if( !up && !left && down && !right  )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 3 ] );

			if( !up && !left && down && right && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 4 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 0 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 4 ] );
			}

			if( !up && left && down && !right && !downLeft ) {
				if( autoTileData[ autoTileIndex * 48 + 5 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 2 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 5 ] );
			}

			if( up && !left && down && right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 6 ] );

			if( up && left && down && right && upRight && upLeft && downRight && downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );

			if( up && left && down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 8 ] );

			if( up && !left && down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 9 ] );

			if( up && !left && !down && right && !upRight ) {
				if( autoTileData[ autoTileIndex * 48 + 10 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 12 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 10 ] );
			}

			if( up && left && !down && !right && !upLeft ) {
				if( autoTileData[ autoTileIndex * 48 + 11 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 14 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 11 ] );
			}

			if( up && !left && !down && right && upRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 12 ] );

			if( up && left && !down && right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 13 ] );

			if( up && left && !down && !right && upLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 14 ] );

			if( up && !left && !down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 15 ] );

			if( up && left && down && right && !downRight && upLeft && upRight && downLeft ) {
				if( autoTileData[ autoTileIndex * 48 + 16 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 16 ] );
			}

			if( up && left && down && right && !downLeft && upLeft && upRight && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 17 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 17 ] );
			}

			if( !up && !left && !down && right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 18 ] );

			if( !up && left && !down && right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 19 ] );

			if( !up && left && !down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 20 ] );

			if( !up && !left && !down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 21 ] );

			if( up && left && down && right && ! upRight && upLeft && downLeft && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 22 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 22 ] );
			}

			if( up && left && down && right && !upLeft && upRight && downLeft && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 23 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 23 ] );
			}

			if( up && !left && down && right && !upRight && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 24 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 6 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 24 ] );
			}

			if( !up && left && down && right && !downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 25 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 1 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 25 ] );
			}

			if( up && left && down && right && upLeft && !upRight && downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 26 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 26 ] );
			}

			if( up && left && down && right && upLeft && upRight && !downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 27 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 27 ] );
			}

			if( up && left && down && right && !upLeft && !upRight && downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 28 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 28 ] );
			}

			if( up && left && down && right && upLeft && !upRight && !downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 29 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 29 ] );
			}

			if( up && left && !down && right && !upLeft && !upRight ) {
				if( autoTileData[ autoTileIndex * 48 + 30 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 13 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 30 ] );
			}

			if( up && left && down && !right && !upLeft && !downLeft ) {
				if( autoTileData[ autoTileIndex * 48 + 31 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 8 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 31 ] );
			}

			if( up && left && down && right && !upLeft && !upRight && downLeft && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 32 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 32 ] );
			}

			if( up && left && down && right && !upLeft && upRight && !downLeft && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 33 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 33 ] );
			}

			if( up && left && down && right && !upLeft && !upRight && !downLeft && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 34 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 34 ] );
			}
			if( up && left && down && right && !upLeft && upRight && !downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 35 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 35 ] );
			}

			if( up && left && down && right && !upLeft && !upRight && !downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 40 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 40 ] );
			}

			if( up && left && down && right && upLeft && !upRight && !downLeft && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 41 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 41 ] );
			}

			if( up && left && down && right && !upLeft && upRight && downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 46 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 46 ] );
			}

			if( up && !left && down && right && upRight && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 36 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 6 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 36 ] );
			}

			if( !up && left && down && right && !downLeft && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 37 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 1 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 37 ] );
			}

			if( !up && left && down && right && downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 38 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 1 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 38 ] );
			}

			if( up && left && down && !right && upLeft && !downLeft ) {
				if( autoTileData[ autoTileIndex * 48 + 39 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 8 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 39 ] );
			}

			if( up && left && !down && right && upLeft && !upRight ) {
				if( autoTileData[ autoTileIndex * 48 + 42 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 13 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 42 ] );
			}

			if( up && left && down && !right && !upLeft && downLeft ) {
				if( autoTileData[ autoTileIndex * 48 + 43 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 8 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 43 ] );
			}

			if( up && !left && down && right && !upRight && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 44 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 6 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 44 ] );
			}

			if( up && left && !down && right && !upLeft && upRight ) {
				if( autoTileData[ autoTileIndex * 48 + 45 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 13 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 45 ] );
			}
		}


		if( autoTileType[autoTileIndex] == AutoTileType.IsometricWall ) {

			switch( autoTileEdgeMode[autoTileIndex] ) {

			case AutoTileEdgeMode.None:
			default:
				up = LocalPointToMapIndex( new Vector2( x, y + 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				down = LocalPointToMapIndex( new Vector2( x, y - 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				left = LocalPointToMapIndex( new Vector2( x - 1, y ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y ) ) ].autoTileIndex, autoTileIndex );
				right = LocalPointToMapIndex( new Vector2( x + 1, y ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y ) ) ].autoTileIndex, autoTileIndex );

				upLeft = LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				upRight = LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				break;

			case AutoTileEdgeMode.LinkToEdge:
				up = LocalPointToMapIndex( new Vector2( x, y + 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				down = LocalPointToMapIndex( new Vector2( x, y - 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				left = LocalPointToMapIndex( new Vector2( x - 1, y ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y ) ) ].autoTileIndex, autoTileIndex );
				right = LocalPointToMapIndex( new Vector2( x + 1, y ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y ) ) ].autoTileIndex, autoTileIndex );

				upLeft = LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				upRight = LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				break;

			case AutoTileEdgeMode.Wrap:
				up = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, Mod(y + 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				down = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, Mod(y - 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				left = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x - 1, mapWidth), y ) ) ].autoTileIndex, autoTileIndex );
				right = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x + 1, mapWidth), y ) ) ].autoTileIndex, autoTileIndex );

				upLeft = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x - 1, mapWidth), Mod(y - 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				upRight = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x + 1, mapWidth), Mod(y - 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				break;
			}

			downRight = LocalPointToMapIndex( new Vector2( x, y - 1 ) ) != -1
				&& down && (tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 8 ] ||
					tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 14 ] || 
					tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 9 ] || 
					tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 15 ] );

			downLeft = LocalPointToMapIndex( new Vector2( x, y - 1 ) ) != -1
				&& down && (tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 6 ] ||
					tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 12 ] ||
					tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 9 ] || 
					tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 15 ] );

			//			upRight = LocalPointToMapIndex( new Vector2( x, y + 1 ) ) != -1
			//				&& up && (tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 8 ] ||
			//				            tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 2 ] || 
			//				            tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 9 ] || 
			//				            tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 3 ] );
			//			
			//			upLeft = LocalPointToMapIndex( new Vector2( x, y + 1 ) ) != -1
			//				&& up && (tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 6 ] ||
			//				            tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 0 ] ||
			//				            tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 9 ] || 
			//				            tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 3 ] );

			if( !up && !left && down && right && !downRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 0 ] );

			if( !up && !left && down && right && downRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 3 ] );

			if( !up && left && down && !right && !downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 2 ] );

			if( !up && left && down && !right && downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 3 ] );

			if( !up && !left && down && !right  )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 3 ] );

			if( up && !left && down && right && !downRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 6 ] );

			if( up && !left && down && right && downRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 9 ] );

			if( up && left && down && !right && !downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 8 ] );

			if( up && left && down && !right && downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 9 ] );

			if( up && !left && down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 9 ] );

			if( up && !left && !down && right && !upRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 12 ] );

			if( up && !left && !down && right && upRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 15 ] );

			if( up && left && !down && right && !upRight && !upLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 13 ] );

			if( up && left && !down && right && upRight && !upLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 14 ] );

			if( up && left && !down && right && !upRight && upLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 12 ] );

			if( up && left && !down && right && upRight && upLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 15 ] );

			if( up && left && !down && !right && !upLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 14 ] );

			if( up && left && !down && !right && upLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 15 ] );

			if( up && !left && !down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 15 ] );

			if( !up && !left && !down && right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 18 ] );

			if( !up && left && !down && right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 19 ] );

			if( !up && left && !down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 20 ] );

			if( !up && !left && !down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 21 ] );

			if( !up && left && down && right && !downRight && !downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 1 ] );

			if( !up && left && down && right && downRight && !downLeft)
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 2 ] );

			if( !up && left && down && right && !downRight && downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 0 ] );

			if( !up && left && down && right && downRight && downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 3 ] );

			if( up && left && down && right && !downRight && !downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );

			if( up && left && down && right && downRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 8 ] );

			if( up && left && down && right && downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 6 ] );
			//			if( !up && left && down && right ) {
			//				if( LocalPointToMapIndex( new Vector2( x, y - 1 ) ) != -1 ) {
			//					if( tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 8 ] || 
			//					   tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 14 ] ) {
			//						selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 2 ] );
			//						Debug.Log( "made side1 wall" );
			//					}
			//					else if( tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 6 ] || 
			//					        tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 12 ] ) {
			//						selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 0 ] );
			//						Debug.Log( "made side2 wall" );
			//					}
			//					else {
			//						selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 1 ] );
			//						Debug.Log( "made middle wall" );
			//					}
			//				}
			//				else {
			//					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 1 ] );
			//					Debug.Log( "made middle wall at bottom" );
			//				}
			//			}
			//			
			//			if( up && left && down && right ) {
			//				if( LocalPointToMapIndex( new Vector2( x, y - 1 ) ) != -1 ) {
			//					if( tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 8 ] || 
			//					   tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 14 ] ) {
			//						selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 8 ] );
			//						Debug.Log( "made side1 wall" );
			//					}
			//					else if( tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 6 ] || 
			//					        tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 12 ] ) {
			//						selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 6 ] );
			//						Debug.Log( "made side2 wall" );
			//					}
			//					else {
			//						selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
			//						Debug.Log( "made middle wall" );
			//					}
			//				}
			//				else {
			//					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
			//					Debug.Log( "made middle wall at bottom" );
			//				}
			//			}
		}


		if( selectedTile == Tile.empty )
			selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 21 ] );

		//		if( selectedTile == Tile.empty )
		//			_numberOfTiles--;
		else
			selectedTile.autoTileIndex = autoTileIndex;
		tiles[ LocalPointToMapIndex( new Vector2( x, y ) ) ] = new Tile( selectedTile );
	}

	public static int Mod( int value, int length ) {
		if( length == 1 )
			return 0;
		while( value < 0 )
			value += length;
		return value % length;
	}

	public void FloodFill ( Vector2 worldPos, Tile[,] selectedTiles, Vector2 originalPos, Tile originalTile ) {
		if( WorldPointToMapIndex(originalPos) == -1 )
			return;
		if( WorldPointToMapIndex( worldPos ) == -1 )
			return;
		Tile replacementTile = selectedTiles[Mod((int)(worldPos.x - originalPos.x), selectedTiles.GetLength(0)), Mod((int)(originalPos.y - worldPos.y), selectedTiles.GetLength(1))];

		if( originalTile == replacementTile )
			return;

		if( tiles[WorldPointToMapIndex(worldPos)] != originalTile ) {
			//if( targetTile.autoTileIndex == -1 || targetTile.autoTileIndex != tiles[WorldPointToMapIndex(worldPos)].autoTileIndex )
			return;
		}

		AddTile( worldPos, replacementTile );
		FloodFill( new Vector2( worldPos.x + zoomFactor, worldPos.y ), selectedTiles, originalPos, originalTile );
		FloodFill( new Vector2( worldPos.x - zoomFactor, worldPos.y ), selectedTiles, originalPos, originalTile );
		FloodFill( new Vector2( worldPos.x, worldPos.y + zoomFactor ), selectedTiles, originalPos, originalTile );
		FloodFill( new Vector2( worldPos.x, worldPos.y - zoomFactor ), selectedTiles, originalPos, originalTile );
	}

	public void FloodFill ( Vector2 worldPos, int autoTileIndex, Vector2 originalPos, Tile originalTile ) {
		if( WorldPointToMapIndex(originalPos) == -1 )
			return;
		if( WorldPointToMapIndex( worldPos ) == -1 )
			return;
		Tile replacementTile = autoTileData[ autoTileIndex * 48 + 0 ];

		if( originalTile == replacementTile )
			return;

		if( tiles[WorldPointToMapIndex(worldPos)] != originalTile ) {
			//if( targetTile.autoTileIndex == -1 || targetTile.autoTileIndex != tiles[WorldPointToMapIndex(worldPos)].autoTileIndex )
			return;
		}

		AddTile( worldPos, new Tile( (Vector2)replacementTile, replacementTile.rotation, autoTileIndex ) );
		FloodFill( new Vector2( worldPos.x + zoomFactor, worldPos.y ), autoTileIndex, originalPos, originalTile );
		FloodFill( new Vector2( worldPos.x - zoomFactor, worldPos.y ), autoTileIndex, originalPos, originalTile );
		FloodFill( new Vector2( worldPos.x, worldPos.y + zoomFactor ), autoTileIndex, originalPos, originalTile );
		FloodFill( new Vector2( worldPos.x, worldPos.y - zoomFactor ), autoTileIndex, originalPos, originalTile );
	}

	/// <summary>
	/// Adds a tile with default flip and rotation. UpdateVisualMesh() must be called after all changes have been made to see the result.
	/// </summary>
	/// <param name="worldPos">World position.</param>
	/// <param name="selectedTile">Selected tile.</param>
	public void AddTile( Vector2 worldPos, Tile selectedTile ) {
		if( selectedTile == new Vector2( 0.5f, 0.5f ) )
			return;
		if( WorldPointToMapIndex( worldPos ) < 0 )
			return;

		if( tiles[WorldPointToMapIndex(worldPos)] != Tile.empty )
			RemoveTile( worldPos );

		tiles[WorldPointToMapIndex(worldPos)] = new Tile( selectedTile );
		//		_numberOfTiles++;
		mapHasChanged = true;
	}

	/// <summary>
	/// Adds an auto tile. UpdateVisualMesh() must be called after all changes have been made to see the result.
	/// </summary>
	/// <param name="worldPos">World position.</param>
	/// <param name="autoTileIndex">Auto tile index.</param>
	public void AddTile( Vector2 worldPos, int autoTileIndex ) {
		if( autoTileData.Count == 0 )
			return;
		if( WorldPointToMapIndex( worldPos ) < 0 )
			return;

		//		if( autoTileData[ autoTileIndex * 48 + 21 ] == Tile.empty )
		//			return;

		if( tiles[WorldPointToMapIndex(worldPos)] != Tile.empty )
			RemoveTile( worldPos );


		if( autoTileData[ autoTileIndex * 48 + 21 ] != Tile.empty ) {
			tiles[WorldPointToMapIndex(worldPos)] = new Tile( autoTileData[ autoTileIndex * 48 + 21 ].xIndex, autoTileData[ autoTileIndex * 48 + 21 ].yIndex, 0, autoTileIndex );
			//			_numberOfTiles++;
		}
		mapHasChanged = true;
	}


	/// <summary>
	/// Resizes the bounds to fit world position.
	/// </summary>
	/// <param name="worldPos">World position.</param>
	public void ResizeBoundsToFitWorldPos ( Vector2 worldPos ) {
		const int maxSize = 64;
		if( WorldPointToMapIndex( worldPos ) != -1 )
			return;
		worldPos.x = Mathf.Floor( worldPos.x );
		worldPos.y = Mathf.Floor( worldPos.y );

		//resize to the right
//		if( worldPos.x + 1 > ( transform.position.x + mapWidth ) * zoomFactor ) {
		if( ( worldPos.x - transform.position.x ) / zoomFactor > mapWidth ) {
			if( (int)((worldPos.x - transform.position.x)/zoomFactor + 1) > maxSize )
				worldPos.x = transform.position.x + maxSize - 1;
			Tile[] newMap = new Tile[(int)((worldPos.x - transform.position.x)/zoomFactor + 1) * mapHeight];
			for( int x = 0; x < (int)((worldPos.x - transform.position.x)/zoomFactor + 1); x++ ) {
				for( int y = 0; y < mapHeight; y++ ) {
					if( x >= mapWidth ) {
						newMap[y*(int)((worldPos.x - transform.position.x)/zoomFactor + 1)+x] = Tile.empty;
						continue;
					}
					newMap[y*(int)((worldPos.x - transform.position.x)/zoomFactor + 1)+x] = tiles[y*mapWidth+x];
				}
			}
			mapWidth = (int)((worldPos.x - transform.position.x)/zoomFactor + 1);
			tiles = newMap;
		}

		//resize to the left
		if( worldPos.x  < transform.position.x ) {
			if( ((int)((transform.position.x - worldPos.x)/zoomFactor) + mapWidth) > maxSize )
				worldPos.x = transform.position.x - (maxSize-mapWidth);
			Tile[] newMap = new Tile[((int)((transform.position.x - worldPos.x)/zoomFactor) + mapWidth) * mapHeight];
			for( int x = 0; x < ((int)((transform.position.x - worldPos.x)/zoomFactor) + mapWidth); x++ ) {
				for( int y = 0; y < mapHeight; y++ ) {
					if( x < (int)(transform.position.x - worldPos.x) ) {
						newMap[y*((int)((transform.position.x - worldPos.x)/zoomFactor) + mapWidth)+x] = Tile.empty;
						continue;
					}
					newMap[y*((int)((transform.position.x - worldPos.x)/zoomFactor) + mapWidth)+x] = tiles[y*mapWidth+(x-(int)((transform.position.x - worldPos.x)/zoomFactor))];
				}
			}
			mapWidth = ((int)((transform.position.x - worldPos.x)/zoomFactor) + mapWidth);
			tiles = newMap;
			Vector3 newPos = transform.position;
			newPos.x -= (transform.position.x - worldPos.x);
			transform.position = newPos;
			mapHasChanged = true;
			UpdateVisualMesh( true );
		}

		//resize to the up
//		if( worldPos.y + 1 > ( transform.position.y + mapHeight ) * zoomFactor ) {
		if( ( worldPos.y - transform.position.y ) / zoomFactor > mapHeight ) {
			if( (int)((worldPos.y - transform.position.y)/zoomFactor + 1) > maxSize )
				worldPos.y = transform.position.y + maxSize - 1;
			Tile[] newMap = new Tile[(int)((worldPos.y - transform.position.y)/zoomFactor + 1) * mapWidth];
			for( int x = 0; x < mapWidth; x++ ) {
				for( int y = 0; y < (int)((worldPos.y - transform.position.y)/zoomFactor + 1); y++ ) {
					if( y >= mapHeight ) {
						newMap[y*mapWidth+x] = Tile.empty;
						continue;
					}
					newMap[y*mapWidth+x] = tiles[y*mapWidth+x];
				}
			}
			mapHeight = (int)((worldPos.y - transform.position.y)/zoomFactor + 1);
			tiles = newMap;
		}

		//resize to the down
		if( worldPos.y  < transform.position.y ) {
			if( ((int)((transform.position.y - worldPos.y)/zoomFactor) + mapHeight) > maxSize )
				worldPos.y = transform.position.y - (maxSize-mapHeight);
			Tile[] newMap = new Tile[((int)((transform.position.y - worldPos.y)/zoomFactor) + mapHeight) * mapWidth];
			for( int x = 0; x < mapWidth; x++ ) {
				for( int y = 0; y < ((int)((transform.position.y - worldPos.y)/zoomFactor) + mapHeight); y++ ) {
					if( y < (int)((transform.position.y - worldPos.y)/zoomFactor) ) {
						newMap[y*mapWidth+x] = Tile.empty;
						continue;
					}
					newMap[y*mapWidth+x] = tiles[(y-(int)((transform.position.y - worldPos.y)/zoomFactor))*mapWidth+x];
				}
			}
			mapHeight = ((int)((transform.position.y - worldPos.y)/zoomFactor) + mapHeight);
			tiles = newMap;
			Vector3 newPos = transform.position;
			newPos.y -= (transform.position.y - worldPos.y);
			transform.position = newPos;
			mapHasChanged = true;
			UpdateVisualMesh( true );
		}
	}

	public int width {
		get {
			return Mathf.RoundToInt((float)meshRenderer.sharedMaterial.mainTexture.width / (tileSize + spacing));
		}
	}

	public int height {
		get {
			return Mathf.RoundToInt((float)meshRenderer.sharedMaterial.mainTexture.height / (tileSize + spacing));
		}
	}


	/// <summary>
	/// Updates the colliders.
	/// </summary>
	public void UpdateCollidersOld () {
		//		PolygonCollider2D mainCollider;
		//		if( GetComponent<PolygonCollider2D>() == null )
		//			mainCollider = gameObject.AddComponent<PolygonCollider2D>();
		//		else
		//			mainCollider = gameObject.GetComponent<PolygonCollider2D>();
		mainCollider.pathCount = 0;
		if( _platformCollider != null )
			platformCollider.pathCount = 0;
		int i = 0;
		int p = 0;
		for( int x = 0; x < mapWidth; x++ ) {
			for( int y = 0; y < mapHeight; y++ ) {
				if( tiles[y * mapWidth + x] == Tile.empty )
					continue;

				Tile ti = tiles[y * mapWidth + x];
				//int width = (gameObject.GetComponent<Renderer>().sharedMaterial.mainTexture.width / (tileSize + spacing));
				Vector2[] points;
				Vector2[] pointsB;

				bool up = LocalPointToMapIndex(new Vector2( x, y + 1 )) != -1 && tiles[(y+1) * mapWidth + x] != Tile.empty && collisions[tiles[(y+1) * mapWidth + x].yIndex * width + tiles[(y+1) * mapWidth + x].xIndex] == CollisionType.Full &&
					LocalPointToMapIndex(new Vector2( x-1, y + 1 )) != -1 && tiles[(y+1) * mapWidth + (x-1)] != Tile.empty && collisions[tiles[(y+1) * mapWidth + (x-1)].yIndex * width + tiles[(y+1) * mapWidth + (x-1)].xIndex] == CollisionType.Full &&
						LocalPointToMapIndex(new Vector2( x+1, y + 1 )) != -1 && tiles[(y+1) * mapWidth + (x+1)] != Tile.empty && collisions[tiles[(y+1) * mapWidth + (x+1)].yIndex * width + tiles[(y+1) * mapWidth + (x+1)].xIndex] == CollisionType.Full
						;
				bool down = LocalPointToMapIndex(new Vector2( x, y - 1 )) != -1 && tiles[(y-1) * mapWidth + x] != Tile.empty && collisions[tiles[(y-1) * mapWidth + x].yIndex * width + tiles[(y-1) * mapWidth + x].xIndex] == CollisionType.Full &&
					LocalPointToMapIndex(new Vector2( x-1, y - 1 )) != -1 && tiles[(y-1) * mapWidth + (x-1)] != Tile.empty && collisions[tiles[(y-1) * mapWidth + (x-1)].yIndex * width + tiles[(y-1) * mapWidth + (x-1)].xIndex] == CollisionType.Full &&
						LocalPointToMapIndex(new Vector2( x+1, y - 1 )) != -1 && tiles[(y-1) * mapWidth + (x+1)] != Tile.empty && collisions[tiles[(y-1) * mapWidth + (x+1)].yIndex * width + tiles[(y-1) * mapWidth + (x+1)].xIndex] == CollisionType.Full;

				bool left = LocalPointToMapIndex(new Vector2( x - 1, y )) != -1 && tiles[y * mapWidth + (x-1)] != Tile.empty && collisions[tiles[y * mapWidth + (x-1)].yIndex * width + tiles[y * mapWidth + (x-1)].xIndex] == CollisionType.Full;
				bool right = LocalPointToMapIndex(new Vector2( x + 1, y )) != -1 && tiles[y * mapWidth + (x+1)] != Tile.empty && collisions[tiles[y * mapWidth + (x+1)].yIndex * width + tiles[y * mapWidth + (x+1)].xIndex] == CollisionType.Full;

				if( y == mapHeight - 1 && upLayer != null ) {
					Vector2 worldPos = transform.position + new Vector3( x, y+1 ) * zoomFactor + new Vector3( 0.5f, 0.5f ) * zoomFactor;
					int index = upLayer.WorldPointToMapIndex(worldPos);
					up = index != -1 && upLayer.tiles[index] != Tile.empty && upLayer.collisions[upLayer.tiles[index].yIndex * upLayer.width + upLayer.tiles[index].xIndex] == CollisionType.Full;
					worldPos = transform.position + new Vector3( x-1, y+1 ) * zoomFactor + new Vector3( 0.5f, 0.5f ) * zoomFactor;
					index = upLayer.WorldPointToMapIndex(worldPos);
					up = up && index != -1 && upLayer.tiles[index] != Tile.empty && upLayer.collisions[upLayer.tiles[index].yIndex * upLayer.width + upLayer.tiles[index].xIndex] == CollisionType.Full;
					worldPos = transform.position + new Vector3( x+1, y+1 ) * zoomFactor + new Vector3( 0.5f, 0.5f ) * zoomFactor;
					index = upLayer.WorldPointToMapIndex(worldPos);
					up = up && index != -1 && upLayer.tiles[index] != Tile.empty && upLayer.collisions[upLayer.tiles[index].yIndex * upLayer.width + upLayer.tiles[index].xIndex] == CollisionType.Full;
				}
				if( y == 0 && downLayer != null ) {
					Vector2 worldPos = transform.position + new Vector3( x, y-1 ) * zoomFactor + new Vector3( 0.5f, 0.5f ) * zoomFactor;
					int index = downLayer.WorldPointToMapIndex(worldPos);
					down = index != -1 && downLayer.tiles[index] != Tile.empty && downLayer.collisions[downLayer.tiles[index].yIndex * downLayer.width + downLayer.tiles[index].xIndex] == CollisionType.Full;
					worldPos = transform.position + new Vector3( x-1, y-1 ) * zoomFactor + new Vector3( 0.5f, 0.5f ) * zoomFactor;
					index = downLayer.WorldPointToMapIndex(worldPos);
					down = down && index != -1 && downLayer.tiles[index] != Tile.empty && downLayer.collisions[downLayer.tiles[index].yIndex * downLayer.width + downLayer.tiles[index].xIndex] == CollisionType.Full;
					worldPos = transform.position + new Vector3( x+1, y-1 ) * zoomFactor + new Vector3( 0.5f, 0.5f ) * zoomFactor;
					index = downLayer.WorldPointToMapIndex(worldPos);
					down = down && index != -1 && downLayer.tiles[index] != Tile.empty && downLayer.collisions[downLayer.tiles[index].yIndex * downLayer.width + downLayer.tiles[index].xIndex] == CollisionType.Full;
				}
				if( x == 0 && leftLayer != null ) {
					Vector2 worldPos = transform.position + new Vector3( x-1, y ) * zoomFactor + new Vector3( 0.5f, 0.5f ) * zoomFactor;
					int index = leftLayer.WorldPointToMapIndex(worldPos);
					left = index != -1 && leftLayer.tiles[index] != Tile.empty && leftLayer.collisions[leftLayer.tiles[index].yIndex * leftLayer.width + leftLayer.tiles[index].xIndex] == CollisionType.Full;
				}
				if( x == mapWidth - 1 && rightLayer != null ) {
					Vector2 worldPos = transform.position + new Vector3( x+1, y ) * zoomFactor + new Vector3( 0.5f, 0.5f ) * zoomFactor;
					int index = rightLayer.WorldPointToMapIndex(worldPos);
					right = index != -1 && rightLayer.tiles[index] != Tile.empty && rightLayer.collisions[rightLayer.tiles[index].yIndex * rightLayer.width + rightLayer.tiles[index].xIndex] == CollisionType.Full;
				}
				switch( collisions[ ti.yIndex * width + ti.xIndex ] ) {
				case CollisionType.Full:

					if( up && down && left && right )
						break;
					points = new Vector2[4];
					points[0] = new Vector2( x, y );
					points[1] = new Vector2( x+1, y );
					points[2] = new Vector2( x+1, y+1 );
					points[3] = new Vector2( x, y+1 );
					mainCollider.pathCount = i + 1;
					mainCollider.SetPath( i, points );
					i++;

					break;

				case CollisionType.SlopeUpRight:

					points = new Vector2[3];
					pointsB = new Vector2[4];
					pointsB[0] = new Vector2( x, y );
					pointsB[1] = new Vector2( x+1, y );
					pointsB[2] = new Vector2( x+1, y+1 );
					pointsB[3] = new Vector2( x, y+1 );
					if( tiles[y * mapWidth + x].flip ) {
						points[0] = pointsB[(0 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[1] = pointsB[(1 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[2] = pointsB[(3 + 4 - tiles[y * mapWidth + x].rotation) % 4];
					}
					else
					{
						points[0] = pointsB[(0 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[1] = pointsB[(1 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[2] = pointsB[(2 + 4 - tiles[y * mapWidth + x].rotation) % 4];
					}
					mainCollider.pathCount = i + 1;
					mainCollider.SetPath( i, points );
					i++;

					break;

				case CollisionType.SlopeUpLeft:

					points = new Vector2[3];
					pointsB = new Vector2[4];
					pointsB[0] = new Vector2( x, y );
					pointsB[1] = new Vector2( x+1, y );
					pointsB[2] = new Vector2( x+1, y+1 );
					pointsB[3] = new Vector2( x, y+1 );
					if( tiles[y * mapWidth + x].flip ) {
						points[0] = pointsB[(0 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[1] = pointsB[(1 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[2] = pointsB[(2 + 4 - tiles[y * mapWidth + x].rotation) % 4];
					}
					else
					{
						points[0] = pointsB[(0 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[1] = pointsB[(1 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[2] = pointsB[(3 + 4 - tiles[y * mapWidth + x].rotation) % 4];
					}
					mainCollider.pathCount = i + 1;
					mainCollider.SetPath( i, points );
					i++;

					break;

					//case for platforms
				case CollisionType.Platform:

					if( up && down && left && right )
						break;
					points = new Vector2[4];
					points[0] = new Vector2( x, y+0.5f );
					points[1] = new Vector2( x+1, y+0.5f );
					points[2] = new Vector2( x+1, y+1 );
					points[3] = new Vector2( x, y+1 );
					platformCollider.pathCount = p + 1;
					platformCollider.SetPath( p, points );
					p++;

					break;
				}
			}
			//			if( Application.isEditor )
			//				UnityEditor.EditorUtility.DisplayProgressBar( "Updating 2D Collisions", "Updating the PolygonCollider2D points...", (float)x / mapHeight );
		}
		//		if( Application.isEditor )
		//			UnityEditor.EditorUtility.ClearProgressBar();
	}


	public void UpdateColliders () {
		if( pixelColliders )
			AutoColliderGenerator.UpdateColliders( this );
		else UpdateCollidersOld();
	}


	/// <summary>
	/// Returns the tile index of the tile at the given world coordinates. Returns -1 if the position is out of the map bounds
	/// </summary>
	/// <returns>The point to map index.</returns>
	/// <param name="worldPos">World position.</param>
	public int WorldPointToMapIndex ( Vector2 worldPos ) {
		return LocalPointToMapIndex( ( worldPos - (Vector2)transform.position ) / zoomFactor );
	}

	/// <summary>
	/// Returns the tile index of the tile at the given world coordinates. Returns -1 if the position is out of the map bounds
	/// </summary>
	/// <returns>The point to map index.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public int WorldPointToMapIndex ( float x, float y ) {
		return WorldPointToMapIndex( new Vector2( x, y ) );
	}

	/// <summary>
	/// Returns the tile index of the tile at the given local coordinates. Returns -1 if the position is out of the map bounds
	/// </summary>
	/// <returns>The point to map index.</returns>
	/// <param name="localPos">Local position.</param>
	public int LocalPointToMapIndex ( Vector2 localPos ) {
		int xIndex = Mathf.FloorToInt( localPos.x );
		int yIndex = Mathf.FloorToInt( localPos.y );
		if( xIndex < 0 || xIndex >= mapWidth || yIndex < 0 || yIndex >= mapHeight )
			return -1;
		return yIndex * mapWidth + xIndex;
	}

	/// <summary>
	/// Removes the tile. UpdateVisualMesh() must be called after all changes have been made to see the result.
	/// </summary>
	/// <param name="worldPos">World position.</param>
	public void RemoveTile ( Vector2 worldPos ) {
		//GameObject layerToEdit = GameObject.Find( layerName );
		//System.Collections.Generic.List<Vector2> tileList = layerToEdit.GetComponent<TileInfo>().tiles;
		if( WorldPointToMapIndex( worldPos ) < 0 )
			return;

		if( tiles[WorldPointToMapIndex( worldPos )] == Tile.empty )
			return;

		tiles[WorldPointToMapIndex( worldPos )] = Tile.empty;
		//		_numberOfTiles--;
		mapHasChanged = true;
	}

	/// <summary>
	/// Rotates the tile. UpdateVisualMesh() must be called after all changes have been made to see the result.
	/// </summary>
	/// <param name="worldPos">World position.</param>
	public void RotateTile ( Vector2 worldPos ) {
		if( WorldPointToMapIndex( worldPos ) < 0 )
			return;

		if( tiles[WorldPointToMapIndex( worldPos )] == Tile.empty )
			return;

		tiles[WorldPointToMapIndex( worldPos )].rotation--;
		if( tiles[WorldPointToMapIndex( worldPos )].rotation < 0 )
			tiles[WorldPointToMapIndex( worldPos )].rotation += 4;
		mapHasChanged = true;
	}

	/// <summary>
	/// Flips the tile. UpdateVisualMesh() must be called after all changes have been made to see the result.
	/// </summary>
	/// <param name="worldPos">World position.</param>
	public void FlipTile ( Vector2 worldPos ) {
		if( WorldPointToMapIndex( worldPos ) < 0 )
			return;

		if( tiles[WorldPointToMapIndex( worldPos )] == Tile.empty )
			return;

		tiles[WorldPointToMapIndex( worldPos )].flip = !tiles[WorldPointToMapIndex( worldPos )].flip;
		mapHasChanged = true;
	}
		
	public int GetWorldAutoTileIndex ( int localX, int localY ) {
		if( localX < 0 ) {
			if( localY < 0 ) {
				int index = downLeftLayer == null ? -1 : downLeftLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( localX, localY ) * zoomFactor );
				return index == -1 ? -1 : downLeftLayer.tiles[ index ].autoTileIndex;
			}
			else if( localY >= mapHeight ) {
				int index = upLeftLayer == null ? -1 : upLeftLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( localX, localY ) * zoomFactor );
				return index == -1 ? -1 : upLeftLayer.tiles[ index ].autoTileIndex;
			}
			else {
				int index = leftLayer == null ? -1 : leftLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( localX, localY ) * zoomFactor );
				return index == -1 ? -1 : leftLayer.tiles[ index ].autoTileIndex;
			}
		}
		else if( localX >= mapWidth ) {
			if( localY < 0 ) {
				int index = downRightLayer == null ? -1 : downRightLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( localX, localY ) * zoomFactor );
				return index == -1 ? -1 : downRightLayer.tiles[ index ].autoTileIndex;
			}
			else if( localY >= mapHeight ) {
				int index = upRightLayer == null ? -1 : upRightLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( localX, localY ) * zoomFactor );
				return index == -1 ? -1 : upRightLayer.tiles[ index ].autoTileIndex;
			}
			else {
				int index = rightLayer == null ? -1 : rightLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( localX, localY ) * zoomFactor );
				return index == -1 ? -1 : rightLayer.tiles[ index ].autoTileIndex;
			}
		}
		else if( localY < 0 ) {
			int index = downLayer == null ? -1 : downLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( localX, localY ) * zoomFactor );
			return index == -1 ? -1 : downLayer.tiles[ index ].autoTileIndex;
		}
		else if( localY >= mapHeight ) {
			int index = upLayer == null ? -1 : upLayer.WorldPointToMapIndex( (Vector2)transform.position + new Vector2( localX, localY ) * zoomFactor );
			return index == -1 ? -1 : upLayer.tiles[ index ].autoTileIndex;
		}
		return -1;
	}

	public override int GetHashCode () {
		return ( numberOfAutotiles & 0xff )
			+ ( ( width & 0xfff ) << 8 )
			+ ( ( height & 0xfff ) << 20 );
	}

	public void BakeAutoTile( int index ) {
		if( index >= numberOfAutotiles || index < 0 )
			return;
		for( int i = 0; i < tiles.Length; i++ ) {
			if( tiles[i].autoTileIndex == index )
				tiles[i].autoTileIndex = -1;
		}
	}
}

[XmlRoot]
public class AutoTile {
	[XmlElement]
	public string autoTileName;
	[XmlElement]
	public List<Tile> autoTileData = new List<Tile>();
	[XmlElement]
	public int textureWidth;
	[XmlElement]
	public int textureHeight;
}

[XmlRoot]
public class CollisionData {
	[XmlElement]
	public TileInfo.CollisionType[] collisions;
}

/// <summary>
/// Tile class. 
/// </summary>
[System.Serializable]
public class Tile {

	public static Tile empty {
		get {
			return new Tile( -1, -1, -1 );

		}
	}

	public Tile () {
		this.xIndex = -1;
		this.yIndex = -1;
		this.rotation = -1;
		this.autoTileIndex = -1;
		this.flip = false;
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="Tile"/> class.
	/// </summary>
	/// <param name="xIndex">X The x position of the tile image on the sprite sheet.</param>
	/// <param name="yIndex">Y The y position of the tile image on the sprite sheet.</param>
	/// <param name="meshIndex">Mesh The index of the mesh for referencing what vertices and triacgles belong to this tile..</param>
	public Tile ( int xIndex, int yIndex, int rotation = 0, int autoTileIndex = -1, bool flip = false ) {
		this.xIndex = xIndex;
		this.yIndex = yIndex;
		this.rotation = rotation;
		this.flip = flip;
		this.autoTileIndex = autoTileIndex;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Tile"/> class.
	/// </summary>
	/// <param name="index">The position of the tile image on the sprite sheet starting at the top left most tile being x = 0 y = 0, and the next tile to the right being x = 1 y = 0..</param>
	/// <param name="meshIndex">Mesh index.</param>
	public Tile ( Vector2 index, int rotation = 0, int autoTileIndex = -1, bool flip = false ) {
		this.xIndex = (int)index.x;
		this.yIndex = (int)index.y;
		this.rotation = rotation;
		this.flip = flip;
		this.autoTileIndex = autoTileIndex;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Tile"/> class.
	/// </summary>
	/// <param name="original">Original tile to clone.</param>
	public Tile ( Tile original ) {
		this.xIndex = original.xIndex;
		this.yIndex = original.yIndex;
		this.rotation = original.rotation;
		this.flip = original.flip;
		this.autoTileIndex = original.autoTileIndex;
	}

	/// <summary>
	/// The x position of the tile image on the sprite sheet
	/// starting with the left most tile being x = 0.
	/// </summary>
	public int xIndex;

	/// <summary>
	/// The y position of the tile image on the sprite sheet
	/// starting with the top most tile being y = 0.
	/// </summary>
	public int yIndex;

	/// <summary>
	/// Has the tile been filpped horizontally?
	/// </summary>
	public bool flip;

	/// <summary>
	/// The rotation of the tile. 0 = no rotation, 1 = 90 degree rotation ect
	/// </summary>
	public int rotation;

	/// <summary>
	/// The index of the auto tile, or -1 if this tile does not belong to an auto tile
	/// </summary>
	public int autoTileIndex;

	/// <summary>
	/// Gets the UV of the bottom left of the tile without taking in to account the rotation and flip
	/// </summary>
	/// <returns>The U.</returns>
	/// <param name="tileInfo">Tile info.</param>
	public Vector2 GetBottomLeftUV ( TileInfo tileInfo ) {
		Material mat = tileInfo.meshRenderer.sharedMaterial;
		//Vector2 uvBottomLeft = new Vector2( (float)(tiles[ y * mapWidth + x ].xIndex * (tileSize + spacing)) / mat.mainTexture.width, 1f - (float)((tiles[ y * mapWidth + x ].yIndex + 1)  * (float)(tileSize + spacing) - spacing) / (float)mat.mainTexture.height );
		Vector2 result = new Vector2( (float)(tileInfo.offsetX + xIndex * ( tileInfo.tileSize + tileInfo.spacing )) / (float)mat.mainTexture.width,
			1f - (float)((yIndex + 1)  * (float)(tileInfo.tileSize + tileInfo.spacing) - tileInfo.spacing + tileInfo.offsetY) / (float)mat.mainTexture.height );

		return result;
	}

	/// <summary>
	/// Gets the type of the collision according to the tileInfo.
	/// </summary>
	/// <returns>The collision type.</returns>
	/// <param name="tileInfo">Tile info.</param>
	public TileInfo.CollisionType GetCollisionType( TileInfo tileInfo ) {
		if( xIndex < 0 )
			return TileInfo.CollisionType.None;
		return tileInfo.collisions[yIndex * tileInfo.width + xIndex];
	}

	public bool IsCollisionDown( TileInfo tileInfo ) {
		if( xIndex < 0 )
			return false;
		TileInfo.CollisionType t = tileInfo.collisions[yIndex * tileInfo.width + xIndex];
		if( t == TileInfo.CollisionType.Full )
			return true;
		if( tileInfo.pixelColliders )
			return false;
		if( t == TileInfo.CollisionType.Platform )
			return true;

		if( ( ( t == TileInfo.CollisionType.SlopeUpLeft ) != flip ) && ( rotation == 0 || rotation == 3 ) )
			return true;
		if( ( ( t == TileInfo.CollisionType.SlopeUpRight ) != flip ) && ( rotation == 0 || rotation == 1 ) )
			return true;
		return false;
	}

	public bool IsCollisionUp( TileInfo tileInfo ) {
		if( xIndex < 0 )
			return false;
		TileInfo.CollisionType t = tileInfo.collisions[yIndex * tileInfo.width + xIndex];
		if( t == TileInfo.CollisionType.Full )
			return true;
		if( tileInfo.pixelColliders )
			return false;
		if( t == TileInfo.CollisionType.Platform )
			return true;

		if( ( ( t == TileInfo.CollisionType.SlopeUpLeft ) != flip ) && ( rotation == 2 || rotation == 1 ) )
			return true;
		if( ( ( t == TileInfo.CollisionType.SlopeUpRight ) != flip ) && ( rotation == 2 || rotation == 3 ) )
			return true;
		return false;
	}

	public bool IsCollisionLeft( TileInfo tileInfo ) {
		if( xIndex < 0 )
			return false;
		TileInfo.CollisionType t = tileInfo.collisions[yIndex * tileInfo.width + xIndex];
		if( t == TileInfo.CollisionType.Full )
			return true;
		if( tileInfo.pixelColliders )
			return false;
		if( t == TileInfo.CollisionType.Platform )
			return true;

		if( ( ( t == TileInfo.CollisionType.SlopeUpLeft ) != flip ) && ( rotation == 0 || rotation == 1 ) )
			return true;
		if( ( ( t == TileInfo.CollisionType.SlopeUpRight ) != flip ) && ( rotation == 2 || rotation == 1 ) )
			return true;
		return false;
	}

	public bool IsCollisionRight( TileInfo tileInfo ) {
		if( xIndex < 0 )
			return false;
		TileInfo.CollisionType t = tileInfo.collisions[yIndex * tileInfo.width + xIndex];
		if( t == TileInfo.CollisionType.Full )
			return true;
		if( tileInfo.pixelColliders )
			return false;
		if( t == TileInfo.CollisionType.Platform )
			return true;

		if( ( ( t == TileInfo.CollisionType.SlopeUpLeft ) != flip ) && ( rotation == 3 || rotation == 2 ) )
			return true;
		if( ( ( t == TileInfo.CollisionType.SlopeUpRight ) != flip ) && ( rotation == 0 || rotation == 3 ) )
			return true;
		return false;
	}

	public static explicit operator Vector2(Tile t) {
		return new Vector2( t.xIndex, t.yIndex );
	}

	public override bool Equals(System.Object obj) {
		// If parameter is null return false.
		if (obj == null) {
			return false;
		}

		Tile t = obj as Tile;

		// If parameter cannot be cast to Tile return false.
		if ((System.Object)t == null) {
			return false;
		}

		// Return true if the fields match:
		return ((xIndex == t.xIndex) && (yIndex == t.yIndex) && (rotation == t.rotation) && (flip == t.flip)) || (autoTileIndex == t.autoTileIndex && autoTileIndex > -1);
	}

	public bool Equals(Tile t) {
		// If parameter is null return false:
		if ((object)t == null) {
			return false;
		}

		// Return true if the fields match:
		return ((xIndex == t.xIndex) && (yIndex == t.yIndex) && (rotation == t.rotation) && (flip == t.flip)) || (autoTileIndex == t.autoTileIndex && autoTileIndex > -1);
	}

	public bool Equals( Vector2 v ) {
		return( xIndex == v.x ) && ( yIndex == v.y );
	}

	public override int GetHashCode() {
		if( autoTileIndex >= 0 )
			return autoTileIndex;
		return Serialize();
	}

	public static bool operator ==(Tile lhs, Tile rhs) {
		return Equals(lhs, rhs);
	}

	public static bool operator ==(Vector2 lhs, Tile rhs) {
		return Equals(lhs, rhs);
	}
	public static bool operator ==(Tile lhs, Vector2 rhs) {
		return Equals(lhs, rhs);
	}
	public static bool operator !=(Vector2 lhs, Tile rhs) {
		return !Equals(lhs, rhs);
	}
	public static bool operator !=(Tile lhs, Vector2 rhs) {
		return !Equals(lhs, rhs);
	}

	public static bool operator !=(Tile lhs, Tile rhs) {
		return !Equals(lhs, rhs);
	}

	public int Serialize () {
		
		if( xIndex == -1 )
			return 0;
		int result = flip ? 3 : 1;
		result |= ( rotation << 2 );
		result |= ( xIndex << 4 );
		result |= ( yIndex << 14 );
		if( autoTileIndex < 0 )
			result |= 0xff << 24;
		else
			result |= ( autoTileIndex << 24 );
		return result;
	}

	public static Tile DeSerialize ( int tileData ) {
		if( tileData == 0 )
			return Tile.empty;
		Tile result = new Tile();
		result.flip = ( ( tileData & 2 ) > 0 );
		result.rotation = ( ( tileData >> 2 ) & 3 );
		result.xIndex = ( ( tileData >> 4 ) & 0x3ff );
		result.yIndex = ( ( tileData >> 14 ) & 0x3ff );
		result.autoTileIndex = ( ( tileData >> 24 ) & 0xff );
		if( result.autoTileIndex == 0xff )
			result.autoTileIndex = -1;
		return result;
	}
}

public class SortingLayerIndexAttribute : PropertyAttribute {
	
}
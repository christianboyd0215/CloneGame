using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class AutoColliderGenerator {

	public enum EdgeDirection {
		Up, Down, Left, Right
	}

	public static void UpdateColliders ( TileInfo map ) {

		map.mainCollider.pathCount = 0;

		int pathIndex = 0;
		int platformPathIndex = 0;

		if( map.knownTilePoints == null )
			map.knownTilePoints = new TileInfo.TilePolygonPoints[map.width,map.height];

		for( int x = 0; x < map.mapWidth; x++ ) {
			for( int y = 0; y < map.mapHeight; y++ ) {
				

				Tile thisTile = map.tiles[y * map.mapWidth + x];
				if( thisTile == Tile.empty || map.collisions[ thisTile.yIndex * map.width + thisTile.xIndex ] == TileInfo.CollisionType.None )
					continue;
				if( map.knownTilePoints[thisTile.xIndex, thisTile.yIndex] == null )
					map.knownTilePoints[thisTile.xIndex, thisTile.yIndex] = new TileInfo.TilePolygonPoints();

				/*
				bool up = map.LocalPointToMapIndex(new Vector2( x, y + 1 )) != -1 && map.tiles[(y+1) * map.mapWidth + x] != Tile.empty && map.collisions[map.tiles[(y+1) * map.mapWidth + x].yIndex * map.width + map.tiles[(y+1) * map.mapWidth + x].xIndex] == TileInfo.CollisionType.Full;
				bool down = map.LocalPointToMapIndex(new Vector2( x, y - 1 )) != -1 && map.tiles[(y-1) * map.mapWidth + x] != Tile.empty && map.collisions[map.tiles[(y-1) * map.mapWidth + x].yIndex * map.width + map.tiles[(y-1) * map.mapWidth + x].xIndex] == TileInfo.CollisionType.Full;
				bool left = map.LocalPointToMapIndex(new Vector2( x - 1, y )) != -1 && map.tiles[y * map.mapWidth + (x-1)] != Tile.empty && map.collisions[map.tiles[y * map.mapWidth + (x-1)].yIndex * map.width + map.tiles[y * map.mapWidth + (x-1)].xIndex] == TileInfo.CollisionType.Full;
				bool right = map.LocalPointToMapIndex(new Vector2( x + 1, y )) != -1 && map.tiles[y * map.mapWidth + (x+1)] != Tile.empty && map.collisions[map.tiles[y * map.mapWidth + (x+1)].yIndex * map.width + map.tiles[y * map.mapWidth + (x+1)].xIndex] == TileInfo.CollisionType.Full;
				
				if( y == map.mapHeight - 1 && map.upLayer != null ) {
					Vector2 worldPos = map.transform.position + new Vector3( x, y+1 );
					int index = map.upLayer.WorldPointToMapIndex(worldPos);
					up = index != -1 && map.upLayer.tiles[index] != Tile.empty && map.upLayer.collisions[map.upLayer.tiles[index].yIndex * map.upLayer.width + map.upLayer.tiles[index].xIndex] == TileInfo.CollisionType.Full;
				}
				if( y == 0 && map.downLayer != null ) {
					Vector2 worldPos = map.transform.position + new Vector3( x, y-1 );
					int index = map.downLayer.WorldPointToMapIndex(worldPos);
					down = index != -1 && map.downLayer.tiles[index] != Tile.empty && map.downLayer.collisions[map.downLayer.tiles[index].yIndex * map.downLayer.width + map.downLayer.tiles[index].xIndex] == TileInfo.CollisionType.Full;
				}
				if( x == 0 && map.leftLayer != null ) {
					Vector2 worldPos = map.transform.position + new Vector3( x-1, y );
					int index = map.leftLayer.WorldPointToMapIndex(worldPos);
					left = index != -1 && map.leftLayer.tiles[index] != Tile.empty && map.leftLayer.collisions[map.leftLayer.tiles[index].yIndex * map.leftLayer.width + map.leftLayer.tiles[index].xIndex] == TileInfo.CollisionType.Full;
				}
				if( x == map.mapWidth - 1 && map.rightLayer != null ) {
					Vector2 worldPos = map.transform.position + new Vector3( x+1, y );
					int index = map.rightLayer.WorldPointToMapIndex(worldPos);
					right = index != -1 && map.rightLayer.tiles[index] != Tile.empty && map.rightLayer.collisions[map.rightLayer.tiles[index].yIndex * map.rightLayer.width + map.rightLayer.tiles[index].xIndex] == TileInfo.CollisionType.Full;
				}
				*/
				bool up = map.LocalPointToMapIndex(new Vector2( x, y + 1 )) != -1 && map.tiles[(y+1) * map.mapWidth + x] != Tile.empty && map.collisions[map.tiles[(y+1) * map.mapWidth + x].yIndex * map.width + map.tiles[(y+1) * map.mapWidth + x].xIndex] == TileInfo.CollisionType.Full &&
					map.LocalPointToMapIndex(new Vector2( x-1, y + 1 )) != -1 && map.tiles[(y+1) * map.mapWidth + (x-1)] != Tile.empty && map.collisions[map.tiles[(y+1) * map.mapWidth + (x-1)].yIndex * map.width + map.tiles[(y+1) * map.mapWidth + (x-1)].xIndex] == TileInfo.CollisionType.Full &&
						map.LocalPointToMapIndex(new Vector2( x+1, y + 1 )) != -1 && map.tiles[(y+1) * map.mapWidth + (x+1)] != Tile.empty && map.collisions[map.tiles[(y+1) * map.mapWidth + (x+1)].yIndex * map.width + map.tiles[(y+1) * map.mapWidth + (x+1)].xIndex] == TileInfo.CollisionType.Full
						;
				bool down = map.LocalPointToMapIndex(new Vector2( x, y - 1 )) != -1 && map.tiles[(y-1) * map.mapWidth + x] != Tile.empty && map.collisions[map.tiles[(y-1) * map.mapWidth + x].yIndex * map.width + map.tiles[(y-1) * map.mapWidth + x].xIndex] == TileInfo.CollisionType.Full &&
					map.LocalPointToMapIndex(new Vector2( x-1, y - 1 )) != -1 && map.tiles[(y-1) * map.mapWidth + (x-1)] != Tile.empty && map.collisions[map.tiles[(y-1) * map.mapWidth + (x-1)].yIndex * map.width + map.tiles[(y-1) * map.mapWidth + (x-1)].xIndex] == TileInfo.CollisionType.Full &&
						map.LocalPointToMapIndex(new Vector2( x+1, y - 1 )) != -1 && map.tiles[(y-1) * map.mapWidth + (x+1)] != Tile.empty && map.collisions[map.tiles[(y-1) * map.mapWidth + (x+1)].yIndex * map.width + map.tiles[(y-1) * map.mapWidth + (x+1)].xIndex] == TileInfo.CollisionType.Full;
				
				bool left = map.LocalPointToMapIndex(new Vector2( x - 1, y )) != -1 && map.tiles[y * map.mapWidth + (x-1)] != Tile.empty && map.collisions[map.tiles[y * map.mapWidth + (x-1)].yIndex * map.width + map.tiles[y * map.mapWidth + (x-1)].xIndex] == TileInfo.CollisionType.Full;
				bool right = map.LocalPointToMapIndex(new Vector2( x + 1, y )) != -1 && map.tiles[y * map.mapWidth + (x+1)] != Tile.empty && map.collisions[map.tiles[y * map.mapWidth + (x+1)].yIndex * map.width + map.tiles[y * map.mapWidth + (x+1)].xIndex] == TileInfo.CollisionType.Full;
				
				if( y == map.mapHeight - 1 && map.upLayer != null ) {
					Vector2 worldPos = map.transform.position + new Vector3( x, y+1 ) * map.zoomFactor + new Vector3( 0.5f, 0.5f ) * map.zoomFactor;
					int index = map.upLayer.WorldPointToMapIndex(worldPos);
					up = index != -1 && map.upLayer.tiles[index] != Tile.empty && map.upLayer.collisions[map.upLayer.tiles[index].yIndex * map.upLayer.width + map.upLayer.tiles[index].xIndex] == TileInfo.CollisionType.Full;
					worldPos = map.transform.position + new Vector3( x-1, y+1 ) * map.zoomFactor + new Vector3( 0.5f, 0.5f ) * map.zoomFactor;
					index = map.upLayer.WorldPointToMapIndex(worldPos);
					up = up && index != -1 && map.upLayer.tiles[index] != Tile.empty && map.upLayer.collisions[map.upLayer.tiles[index].yIndex * map.upLayer.width + map.upLayer.tiles[index].xIndex] == TileInfo.CollisionType.Full;
					worldPos = map.transform.position + new Vector3( x+1, y+1 ) * map.zoomFactor + new Vector3( 0.5f, 0.5f ) * map.zoomFactor;
					index = map.upLayer.WorldPointToMapIndex(worldPos);
					up = up && index != -1 && map.upLayer.tiles[index] != Tile.empty && map.upLayer.collisions[map.upLayer.tiles[index].yIndex * map.upLayer.width + map.upLayer.tiles[index].xIndex] == TileInfo.CollisionType.Full;
				}
				if( y == 0 && map.downLayer != null ) {
					Vector2 worldPos = map.transform.position + new Vector3( x, y-1 ) * map.zoomFactor + new Vector3( 0.5f, 0.5f ) * map.zoomFactor;
					int index = map.downLayer.WorldPointToMapIndex(worldPos);
					down = index != -1 && map.downLayer.tiles[index] != Tile.empty && map.downLayer.collisions[map.downLayer.tiles[index].yIndex * map.downLayer.width + map.downLayer.tiles[index].xIndex] == TileInfo.CollisionType.Full;
					worldPos = map.transform.position + new Vector3( x-1, y-1 ) * map.zoomFactor + new Vector3( 0.5f, 0.5f ) * map.zoomFactor;
					index = map.downLayer.WorldPointToMapIndex(worldPos);
					down = down && index != -1 && map.downLayer.tiles[index] != Tile.empty && map.downLayer.collisions[map.downLayer.tiles[index].yIndex * map.downLayer.width + map.downLayer.tiles[index].xIndex] == TileInfo.CollisionType.Full;
					worldPos = map.transform.position + new Vector3( x+1, y-1 ) * map.zoomFactor + new Vector3( 0.5f, 0.5f ) * map.zoomFactor;
					index = map.downLayer.WorldPointToMapIndex(worldPos);
					down = down && index != -1 && map.downLayer.tiles[index] != Tile.empty && map.downLayer.collisions[map.downLayer.tiles[index].yIndex * map.downLayer.width + map.downLayer.tiles[index].xIndex] == TileInfo.CollisionType.Full;
				}
				if( x == 0 && map.leftLayer != null ) {
					Vector2 worldPos = map.transform.position + new Vector3( x-1, y ) * map.zoomFactor + new Vector3( 0.5f, 0.5f ) * map.zoomFactor;
					int index = map.leftLayer.WorldPointToMapIndex(worldPos);
					left = index != -1 && map.leftLayer.tiles[index] != Tile.empty && map.leftLayer.collisions[map.leftLayer.tiles[index].yIndex * map.leftLayer.width + map.leftLayer.tiles[index].xIndex] == TileInfo.CollisionType.Full;
				}
				if( x == map.mapWidth - 1 && map.rightLayer != null ) {
					Vector2 worldPos = map.transform.position + new Vector3( x+1, y ) * map.zoomFactor + new Vector3( 0.5f, 0.5f ) * map.zoomFactor;
					int index = map.rightLayer.WorldPointToMapIndex(worldPos);
					right = index != -1 && map.rightLayer.tiles[index] != Tile.empty && map.rightLayer.collisions[map.rightLayer.tiles[index].yIndex * map.rightLayer.width + map.rightLayer.tiles[index].xIndex] == TileInfo.CollisionType.Full;
				}

				if( up && down && left && right )
					continue;

				Vector2[] points;
				if( map.collisions[ thisTile.yIndex * map.width + thisTile.xIndex ] != TileInfo.CollisionType.Full ) {
					if( map.knownTilePoints[ thisTile.xIndex, thisTile.yIndex ].isKnown ) {
						points = (Vector2[])map.knownTilePoints[ thisTile.xIndex, thisTile.yIndex ].points.Clone();
					}
					else {
						points = GetTilePoints( map, thisTile );
						map.knownTilePoints[ thisTile.xIndex, thisTile.yIndex ].points = (Vector2[])points.Clone();
						map.knownTilePoints[ thisTile.xIndex, thisTile.yIndex ].isKnown = true;
					}

					for( int i = 0; i < points.Length; i++ ) {
						if( thisTile.flip ) {
							points[i].x = 1 - points[i].x;
						}
						Vector2 tempPoint = points[i];
						switch( thisTile.rotation ) {
						case 1:
							points[i].x = tempPoint.y;
							points[i].y = 1-tempPoint.x;
							break;
						case 2:
							points[i].x = 1-tempPoint.x;
							points[i].y = 1-tempPoint.y;
							break;
						case 3:
							points[i].x = 1-tempPoint.y;
							points[i].y = tempPoint.x;
							break;
						}

						points[i] += new Vector2( x, y );
					}
				}
				else {
					points = new Vector2[4] {
						new Vector2( x, y ),
						new Vector2( x, y+1 ),
						new Vector2( x+1, y+1 ),
						new Vector2( x+1, y )
					};
				}

				if(  map.collisions[ thisTile.yIndex * map.width + thisTile.xIndex ] == TileInfo.CollisionType.Platform ) {
					map.platformCollider.pathCount = platformPathIndex + 1;
					map.platformCollider.SetPath( platformPathIndex, points );
					platformPathIndex++;
				}
				else {
					map.mainCollider.pathCount = pathIndex + 1;
					map.mainCollider.SetPath( pathIndex, points );
					pathIndex++;
				}
			}
		}
	}

	public static Vector2[] GetTilePoints ( TileInfo map, Tile thisTile ) {
		Texture2D tex = (Texture2D)map.GetComponent<Renderer>().sharedMaterial.mainTexture;
		Color[] pixels = tex.GetPixels( map.offsetX + thisTile.xIndex * ( map.tileSize + map.spacing ), tex.height + map.spacing - ( ( thisTile.yIndex + 1 ) * ( map.tileSize + map.spacing ) ) - map.offsetY, map.tileSize, map.tileSize );

		List<Vector2> result = new List<Vector2>();

		int leftEdge = 0;
		int rightEdge = map.tileSize - 1;
		int bottomEdge = 0;
		int topEdge = map.tileSize - 1;

		if( pixels[bottomEdge*map.tileSize+leftEdge].a > 0.5f ) //result.Add( new Vector2( (float)leftEdge/(float)(map.tileSize), (float)bottomEdge/(float)(map.tileSize) ) );
			AddToList( result, leftEdge, bottomEdge, map.tileSize );
		else {
			while( leftEdge < rightEdge ) {
				if( CheckEdgeAndAdd( result, EdgeDirection.Up, leftEdge, bottomEdge, map.tileSize, pixels ) )
					break;
				else
					leftEdge++;
			}
		}
		if( pixels[topEdge*map.tileSize+leftEdge].a > 0.5f )// AddToList( result, new Vector2( (float)leftEdge/(float)(map.tileSize), (float)(topEdge+1)/(float)(map.tileSize) ) );
			AddToList( result, leftEdge, topEdge, map.tileSize );
		else {
			CheckEdgeAndAdd( result, EdgeDirection.Down, leftEdge, topEdge, map.tileSize, pixels );
			while( topEdge > bottomEdge ) {
				if( CheckEdgeAndAdd( result, EdgeDirection.Right, leftEdge, topEdge, map.tileSize, pixels ) )
					break;
				else
					topEdge--;
			}
		}
		if( pixels[topEdge*map.tileSize+rightEdge].a > 0.5f ) //AddToList( result, new Vector2( (float)(rightEdge+1)/(float)(map.tileSize), (float)(topEdge+1)/(float)(map.tileSize) ) );
			AddToList( result, rightEdge, topEdge, map.tileSize );
		else {
			CheckEdgeAndAdd( result, EdgeDirection.Left, rightEdge, topEdge, map.tileSize, pixels );
			while( rightEdge > leftEdge ) {
				if( CheckEdgeAndAdd( result, EdgeDirection.Down, rightEdge, topEdge, map.tileSize, pixels ) )
					break;
				else
					rightEdge--;
			}
		}
		if( pixels[bottomEdge*map.tileSize+rightEdge].a > 0.5f ) //AddToList( result, new Vector2( (float)(rightEdge+1)/(float)(map.tileSize), (float)bottomEdge/(float)(map.tileSize) ) );
			AddToList( result, rightEdge, bottomEdge, map.tileSize );
		else {
			CheckEdgeAndAdd( result, EdgeDirection.Up, rightEdge, bottomEdge, map.tileSize, pixels );
			while( bottomEdge < topEdge ) {
				if( CheckEdgeAndAdd( result, EdgeDirection.Left, rightEdge, bottomEdge, map.tileSize, pixels ) )
					break;
				else
					bottomEdge++;
			}
		}
		if( pixels[bottomEdge*map.tileSize+leftEdge].a <= 0.5f ) CheckEdgeAndAdd( result, EdgeDirection.Right, leftEdge, bottomEdge, map.tileSize, pixels );

		return result.ToArray();
	}

//	static void AddToList( List<Vector2> list, Vector2 item ) {
//		if( list.Contains( item ) )
//			return;
//			list.Add( item );
//	}

	static bool Approximately( Vector2 v1, Vector2 v2, float tolerance ) {
		return ( v1.x > v2.x - tolerance && v1.x < v2.x + tolerance && v1.y > v2.y - tolerance && v1.y < v2.y + tolerance );
	}

	static void AddToList( List<Vector2> list, int x, int y, float size ) {
		Vector2 item = new Vector2( (float)(((x+1)/2)*2) / size, (float)(((y+1)/2)*2) / size );
		if( list.Exists( li => Approximately( li, item, 0.05f ) ) )
			return;
		list.Add( item );
	}

	static bool CheckEdgeAndAdd( List<Vector2> result, EdgeDirection direction, int x, int y, int tileSize, Color[] pixels ) {
		while( x >= 0 && y >= 0 && x < tileSize && y < tileSize ) {
			if( pixels[y * tileSize + x].a > 0.5f ) {

//				int xMod = Snap( x, tileSize );
//				int yMod = Snap( y, tileSize );

//				Vector2 thisPoint = new Vector2( (float)(xMod) / (float)(tileSize), (float)(yMod) / (float)(tileSize) );
				AddToList( result, Snap( x, tileSize ), Snap( y, tileSize ), tileSize );
				return true;
			}
			switch( direction ) {
			case EdgeDirection.Down:
				y--;
				break;
			case EdgeDirection.Up:
			default:
				y++;
				break;
			case EdgeDirection.Left:
				x--;
				break;
			case EdgeDirection.Right:
				x++;
				break;
			}

		}
		return false;
	}

	static int Snap( int n, int size ) {
		if( n < 2 )
			return 0;
		if( n >= size-2 )
			return size;
		return n;
	}
}

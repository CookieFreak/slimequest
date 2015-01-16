﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;


#if UNITY_EDITOR
	using UnityEditor;
#endif

namespace CreativeSpore.RpgMapEditor
{
	public class AutoTileMapGui : MonoBehaviour 
	{
		private AutoTileMap m_autoTileMap;
		private Camera2DController m_camera2D;
		private FollowObjectBehaviour m_camera2DFollowBehaviour;

		private int m_selectedTileIdx = 0;

		private Rect m_rEditorRect;
		private Rect m_rTilesetRect;
		private Rect m_rMinimapRect;
		private Rect m_rMapViewRect;

		private bool m_isCtrlKeyHold = false;

		private const float k_timeBeforeKeyRepeat = 1f;
		private const float k_timeBetweenKeyRepeat = 0.01f;
		private float m_keyPressTimer = 0f;
		private bool m_showCollisions = false;
		private bool m_showMinimap = false;
		private bool m_isInitialized = false;

		private GameObject m_spriteCollLayer;

		private Texture2D[] m_thumbnailTextures; // thumbnail texture for each tileset group

		//+++ used for rectangle selection in tileset
		private int m_tilesetSelStart = -1;
		private int m_tilesetSelEnd = -1;
		//---

		enum eEditorWindow
		{
			NONE,
			TOOLS,
			MAPVIEW,
			MINIMAP
		}
		private eEditorWindow m_focusWindow;

		// Use this for initialization
		public void Init() 
		{
			m_autoTileMap = GetComponent<AutoTileMap>();

			if( m_autoTileMap != null && m_autoTileMap.IsInitialized )
			{
				m_isInitialized = true;
				if( m_autoTileMap.ViewCamera == null )
				{
					Debug.LogWarning( "AutoTileMap has no ViewCamera set. Camera.main will be set as ViewCamera" );
					m_autoTileMap.ViewCamera = Camera.main;
				}
				m_camera2D = m_autoTileMap.ViewCamera.GetComponent<Camera2DController>();

				if( m_camera2D == null )
				{
					m_camera2D = m_autoTileMap.ViewCamera.gameObject.AddComponent<Camera2DController>();
				}
				
				m_camera2DFollowBehaviour = m_camera2D.transform.GetComponent<FollowObjectBehaviour>();	

				// Generate thumbnail textures
				m_thumbnailTextures = new Texture2D[ 5 ];
				m_thumbnailTextures[0] = UtilsAutoTileMap.GenerateTilesetTexture( m_autoTileMap.Tileset.TilesetsAtlasTexture, AutoTileMap.eTilesetGroupType.GROUND );
				m_thumbnailTextures[1] = UtilsAutoTileMap.GenerateTilesetTexture( m_autoTileMap.Tileset.TilesetsAtlasTexture, AutoTileMap.eTilesetGroupType.OBJECTS_B );
				m_thumbnailTextures[2] = UtilsAutoTileMap.GenerateTilesetTexture( m_autoTileMap.Tileset.TilesetsAtlasTexture, AutoTileMap.eTilesetGroupType.OBJECTS_C );
				m_thumbnailTextures[3] = UtilsAutoTileMap.GenerateTilesetTexture( m_autoTileMap.Tileset.TilesetsAtlasTexture, AutoTileMap.eTilesetGroupType.OBJECTS_D );
				m_thumbnailTextures[4] = UtilsAutoTileMap.GenerateTilesetTexture( m_autoTileMap.Tileset.TilesetsAtlasTexture, AutoTileMap.eTilesetGroupType.OBJECTS_E );

				#region Collision Layer
				m_spriteCollLayer = new GameObject();
				m_spriteCollLayer.name = "CollisionLayer";
				m_spriteCollLayer.transform.parent = transform;
				SpriteRenderer sprRender = m_spriteCollLayer.AddComponent<SpriteRenderer>();
				sprRender.sortingOrder = 50; //TODO: +50 temporal? see for a const number later
				_GenerateCollisionTexture();
				#endregion

			}
		}


		private int m_prevMouseTileX = -1;
		private int m_prevMouseTileY = -1;
		private int m_startDragTileX = -1;
		private int m_startDragTileY = -1;
		private int m_dragTileX = -1;
		private int m_dragTileY = -1;
		private bool m_drawSelectionRect;
		private Vector3 m_mousePrevPos;
		private Vector2 m_prevScreenSize;
		void Update () 
		{

			if( !m_isInitialized )
			{
				Init();
				return;
			}

			#region Draw Collisions
			// Generate texture again in case window has been resized
			Vector2 screenSize = new Vector2(Screen.width, Screen.height);
			if( m_prevScreenSize != screenSize )
			{
				_GenerateCollisionTexture();
			}
			m_prevScreenSize = screenSize;

			m_spriteCollLayer.SetActive( m_showCollisions );
			if( m_showCollisions && (int)(Time.timeSinceLevelLoad*4)%2 == 0 )
			{
				SpriteRenderer sprRender = m_spriteCollLayer.GetComponent<SpriteRenderer>();
				Vector3 vPos = m_camera2D.transform.position;
				vPos.x -= (vPos.x % (AutoTileset.TileWorldWidth/4));
				vPos.y -= (vPos.y % (AutoTileset.TileWorldHeight/4));
				vPos.z += 1f;

				// Collision texture position snap to a quarter of tile part
				sprRender.transform.position = vPos;

				// Collision texture pixel scaled to a quarter of tile part
				sprRender.transform.localScale = new Vector3( (AutoTileset.TilePartWidth/2), (AutoTileset.TilePartHeight/2), 1f );

				vPos = m_camera2D.GetComponent<Camera>().WorldToScreenPoint( sprRender.transform.position ); // vPos = center of collision texture in screen coords
				Vector3 vTopLeftOff = new Vector3( sprRender.sprite.texture.width*(AutoTileset.TilePartWidth/2)/2, -sprRender.sprite.texture.height*(AutoTileset.TilePartHeight/2)/2 ) * m_camera2D.Zoom;
				vPos -= vTopLeftOff;
				vPos = m_camera2D.GetComponent<Camera>().ScreenToWorldPoint( vPos ); // vPos is now the top left corner of the collison texture in world coordinates

				Color32[] colors = sprRender.sprite.texture.GetPixels32();
				float factorX = AutoTileset.TileWorldWidth/4; //smallest collision part has a size of a quarter of tile part
				float factorY = AutoTileset.TileWorldHeight/4;
				for( int y = 0; y < sprRender.sprite.texture.height; ++y )
				{
					for( int x = 0; x < sprRender.sprite.texture.width; ++x )
					{
						Vector3 vCheckPos = vPos;
						vCheckPos.x += (x+0.5f)*factorX;
						vCheckPos.y -= (y+0.5f)*factorY;
						AutoTileMap.eTileCollisionType collType = m_autoTileMap.GetAutotileCollisionAtPosition( vCheckPos );
						//Color32 color = (x+y)%2 == 0? new Color32(0, 0, 64, 128) : new Color32(64, 0, 0, 128) ;
						Color32 color = new Color32(0, 0, 0, 0);
						colors[ (sprRender.sprite.texture.height-1-y) * sprRender.sprite.texture.width + x ] = (collType != AutoTileMap.eTileCollisionType.NONE)? new Color32(255, 0, 0, 128) : color;
					}
				}
				sprRender.sprite.texture.SetPixels32( colors );
				sprRender.sprite.texture.Apply();
			}
			#endregion

			#region Undo / Redo
			if( m_isCtrlKeyHold )
			{
				if( Input.GetKeyDown(KeyCode.Z ) )
				{
					m_autoTileMap.BrushGizmo.UndoAction();
				}
				else if( Input.GetKeyDown(KeyCode.Y ) )
				{
					m_autoTileMap.BrushGizmo.RedoAction();
				}

				//+++ Key Repetition Implementation
				if( Input.GetKey(KeyCode.Z ) )
				{
					m_keyPressTimer += Time.deltaTime;
					if( m_keyPressTimer >= k_timeBeforeKeyRepeat )
					{
						m_keyPressTimer -= k_timeBetweenKeyRepeat;
						m_autoTileMap.BrushGizmo.UndoAction();
					}
				}
				else if( Input.GetKey(KeyCode.Y ) )
				{
					m_keyPressTimer += Time.deltaTime;
					if( m_keyPressTimer >= k_timeBeforeKeyRepeat )
					{
						m_keyPressTimer -= k_timeBetweenKeyRepeat;
						m_autoTileMap.BrushGizmo.RedoAction();
					}
				}
				else
				{
					m_keyPressTimer = 0f;
				}
				//---
			}
			#endregion

			if( Input.GetKeyDown(KeyCode.M) ) m_showMinimap = !m_showMinimap;
			if( Input.GetKeyDown(KeyCode.C) ) m_showCollisions = !m_showCollisions;

			bool isMouseLeft = Input.GetMouseButton(0);
			bool isMouseRight = Input.GetMouseButton(1);
			bool isMouseMiddle = Input.GetMouseButton(2);
			bool isMouseLeftDown = Input.GetMouseButtonDown(0);
			bool isMouseRightDown = Input.GetMouseButtonDown(1);
			
			m_drawSelectionRect = false;

			Vector3 vGuiMouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			Vector3 vGuiMouseDelta = vGuiMouse - m_mousePrevPos;
			m_mousePrevPos = vGuiMouse;

			//+++ Set window with focus
			if( !isMouseLeft )
			{
				if( m_rEditorRect.Contains( vGuiMouse ) )
				{
					m_focusWindow = eEditorWindow.TOOLS;
				}
				else if( m_rMinimapRect.Contains( vGuiMouse ) && m_showMinimap )
				{
					m_focusWindow = eEditorWindow.MINIMAP;
				}
				// Added an extra padding to avoid drawing tiles when resizing window
				else if( new Rect(m_rEditorRect.x + m_rEditorRect.width + 10f, 10f, Screen.width-20f-(m_rEditorRect.x + m_rEditorRect.width), Screen.height-20f).Contains( vGuiMouse ) )
				{
					m_focusWindow = eEditorWindow.MAPVIEW;
				}
				else
				{
					m_focusWindow = eEditorWindow.NONE;
				}
			}
			//---

			// drag and move over the map
			if( isMouseMiddle )
			{
				if( m_camera2DFollowBehaviour )
				{
					m_camera2DFollowBehaviour.Target = null;
				}
				Vector3 vTemp = vGuiMouseDelta; vTemp.y = -vTemp.y;
				m_camera2D.transform.position -= (vTemp/100)/m_camera2D.Zoom;
			}

			//
			// Inputs inside Editor Rect
			//
			if( m_rEditorRect.Contains( vGuiMouse ) )
			{
				if( m_rTilesetRect.Contains( vGuiMouse ) )
				{
					vGuiMouse += new Vector3(m_scrollPos.x, m_scrollPos.y);
					Vector3 vOff = new Vector2(vGuiMouse.x, vGuiMouse.y) - m_rTilesetRect.position;
					int tileX = (int)(vOff.x / AutoTileset.TileWidth);
					int tileY = (int)(vOff.y / AutoTileset.TileHeight);
					int autotileIdx = tileY * AutoTileset.AutoTilesPerRow + tileX + (m_TilesetGroupIdx * 256);

					if( isMouseLeftDown || isMouseRightDown && m_isCtrlKeyHold )
					{
						if( m_isCtrlKeyHold )
						{
							// cycle pressed tile collision type
							int collType = (int)m_autoTileMap.Tileset.AutotileCollType[ autotileIdx ];
							collType += isMouseLeftDown? 1 : (int)AutoTileMap.eTileCollisionType._SIZE - 1;
							collType%=(int)AutoTileMap.eTileCollisionType._SIZE;
							m_autoTileMap.Tileset.AutotileCollType[ autotileIdx ] = (AutoTileMap.eTileCollisionType)(collType);
						}
						else
						{
							// select pressed tile
							m_selectedTileIdx = autotileIdx;

							// Remove Brush
							m_autoTileMap.BrushGizmo.Clear();
							m_tilesetSelStart = m_tilesetSelEnd = -1;
						}
					}
					else if( isMouseRightDown )
					{
						m_tilesetSelStart = m_tilesetSelEnd = autotileIdx;
					}
					else if( isMouseRight )
					{
						m_tilesetSelEnd = autotileIdx;
					}
					else if( m_tilesetSelStart >= 0 && m_tilesetSelEnd >= 0 )
					{
						m_autoTileMap.BrushGizmo.RefreshBrushGizmoFromTileset( m_tilesetSelStart, m_tilesetSelEnd );
						m_tilesetSelStart = m_tilesetSelEnd = -1;
					}
				}
			}
			//
			// Inputs inside Minimap Rect
			//
			else if( m_showMinimap && m_rMinimapRect.Contains( vGuiMouse ) && m_focusWindow == eEditorWindow.MINIMAP )
			{
				if( isMouseLeft )
				{
					Vector3 vPos = vGuiMouse - new Vector3( m_rMinimapRect.position.x, m_rMinimapRect.position.y);
					vPos.y = -vPos.y;
					vPos.x *= AutoTileset.TileWidth / AutoTileset.PixelToUnits;
					vPos.y *= AutoTileset.TileHeight / AutoTileset.PixelToUnits;
					vPos.z = m_camera2D.transform.position.z;
					m_camera2D.transform.position = vPos;
					if( m_camera2DFollowBehaviour )
					{
						m_camera2DFollowBehaviour.Target = null;
					}
				}
			}
			//
			// Insputs inside map view
			//
			else if( m_focusWindow == eEditorWindow.MAPVIEW )
			{
				Vector3 vWorldMousePos = m_autoTileMap.ViewCamera.ScreenToWorldPoint( new Vector3(Input.mousePosition.x, Input.mousePosition.y) );
				m_autoTileMap.BrushGizmo.UpdateBrushGizmo( vWorldMousePos );

				if( isMouseRight || isMouseLeft )
				{
					m_drawSelectionRect = isMouseRight;

					//+++ Move camera automatically when near bounds
					if( isMouseLeft )
					{
						float fAutoDragDistX = m_rMapViewRect.width/15;
						float fAutoDragDistY = m_rMapViewRect.height/15;
						float fHDist = m_rMapViewRect.center.x - vGuiMouse.x;
						float fVDist = m_rMapViewRect.center.y - vGuiMouse.y;
						float fHSpeed = Mathf.Lerp(0f, -Mathf.Sign(fHDist), Mathf.Abs(fHDist) < (m_rMapViewRect.width/2 - fAutoDragDistX)? 0 : 1f - (m_rMapViewRect.width/2 - Mathf.Abs(fHDist)) / fAutoDragDistX );
						float fVSpeed = Mathf.Lerp(0f, Mathf.Sign(fVDist), Mathf.Abs(fVDist) < (m_rMapViewRect.height/2 - fAutoDragDistY)? 0 : 1f - (m_rMapViewRect.height/2 - Mathf.Abs(fVDist)) / fAutoDragDistY );
						if( fVSpeed != 0f || fHSpeed != 0f )
						{
							if( m_camera2DFollowBehaviour )
							{
								m_camera2DFollowBehaviour.Target = null;
							}
							m_camera2D.transform.position += (new Vector3(fHSpeed, fVSpeed, 0f)/30)/m_camera2D.Zoom;
						}
					}
					//---

					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					Plane hPlane = new Plane(Vector3.forward, Vector3.zero);
					float distance = 0; 
					if (hPlane.Raycast(ray, out distance))
					{
						// get the hit point:
						Vector3 vPos = ray.GetPoint(distance);
						int tile_x = (int)(vPos.x / AutoTileset.TileWorldWidth);
						int tile_y = (int)(-vPos.y / AutoTileset.TileWorldHeight);
					
						// for optimization, is true when mouse is over a diffent tile during the first update
						bool isMouseTileChanged = (tile_x != m_prevMouseTileX) || (tile_y != m_prevMouseTileY);

						//if ( m_autoTileMap.IsValidAutoTilePos(tile_x, tile_y)) // commented to allow drawing outside map, useful when brush has a lot of copied tiles
						{
							int gndTileType = m_autoTileMap.GetAutoTile( tile_x, tile_y, (int)AutoTileMap.eTileLayer.GROUND ).Type;
							int gndOverlayTileType = m_autoTileMap.GetAutoTile( tile_x, tile_y, (int)AutoTileMap.eTileLayer.GROUND_OVERLAY ).Type;

							// mouse right for tile selection
							if( isMouseRightDown || isMouseRight && isMouseTileChanged )
							{
								if( isMouseRightDown )
								{
									m_startDragTileX = tile_x;
									m_startDragTileY = tile_y;

									// copy tile
									if( m_isCtrlKeyHold )
									{
										m_selectedTileIdx = -2; //NOTE: -2 means, ignore this tile when painting
									}
									else
									{
										m_selectedTileIdx = gndTileType >= 0? gndTileType : gndOverlayTileType;
									}
								}
								m_dragTileX = tile_x;
								m_dragTileY = tile_y;

								// Remove Brush
								m_autoTileMap.BrushGizmo.Clear();
								m_tilesetSelStart = m_tilesetSelEnd = -1;
							}
							// isMouseLeft
							else if( isMouseLeftDown || isMouseTileChanged ) // avoid Push the same action twice during mouse drag
							{
								AutoTileBrush.TileAction action = new AutoTileBrush.TileAction();
								if( m_autoTileMap.BrushGizmo.BrushAction != null )
								{
									//+++ case of multiple tiles painting
									action.CopyRelative( m_autoTileMap, m_autoTileMap.BrushGizmo.BrushAction, tile_x, tile_y );
									if(m_isCtrlKeyHold)
									{
										// ground tiles become ground overlay, ground overlay are removed, overlay tiles remains
										action.BecomeOverlay();
									}
								}
								else 
								{
									//+++ case of single tile painting

									if( m_isCtrlKeyHold || m_autoTileMap.IsAutoTileHasAlpha( m_selectedTileIdx ) )
									{
										// Put tiles with alpha in the overlay layer
										action.Push( m_autoTileMap, tile_x, tile_y, m_selectedTileIdx, (int)AutoTileMap.eTileLayer.GROUND_OVERLAY );
									}
									else
									{
										action.Push( m_autoTileMap, tile_x, tile_y, m_selectedTileIdx, (int)AutoTileMap.eTileLayer.GROUND );
									}
								}

								m_autoTileMap.BrushGizmo.PerformAction( action );
							}
						}

						m_prevMouseTileX = tile_x;
						m_prevMouseTileY = tile_y;
					}
				}
				else
				{
					// Copy selected tiles
					if( m_dragTileX != -1 && m_dragTileY != -1 )
					{
						m_autoTileMap.BrushGizmo.BrushAction = new AutoTileBrush.TileAction();
						int startTileX = Mathf.Min( m_startDragTileX, m_dragTileX );
						int startTileY = Mathf.Min( m_startDragTileY, m_dragTileY );
						int endTileX = Mathf.Max( m_startDragTileX, m_dragTileX );
						int endTileY = Mathf.Max( m_startDragTileY, m_dragTileY );

						for( int tile_x = startTileX; tile_x <= endTileX; ++tile_x  )
						{
							for( int tile_y = startTileY; tile_y <= endTileY; ++tile_y  )
							{
								int gndTileType = m_autoTileMap.GetAutoTile( tile_x, tile_y, (int)AutoTileMap.eTileLayer.GROUND ).Type;
								int gndOverlayTileType = m_autoTileMap.GetAutoTile( tile_x, tile_y, (int)AutoTileMap.eTileLayer.GROUND_OVERLAY ).Type;
								int overlayTileType = m_autoTileMap.GetAutoTile( tile_x, tile_y, (int)AutoTileMap.eTileLayer.OVERLAY ).Type;

								// Tile position is relative to last released position ( m_dragTile )
								if( m_isCtrlKeyHold )
								{
									// Copy overlay only
									if( gndOverlayTileType >= 0 ) // this allow paste overlay tiles without removing ground or ground overlay
										m_autoTileMap.BrushGizmo.BrushAction.Push( m_autoTileMap, tile_x - m_dragTileX, tile_y - m_dragTileY, gndOverlayTileType, (int)AutoTileMap.eTileLayer.GROUND_OVERLAY);
									m_autoTileMap.BrushGizmo.BrushAction.Push( m_autoTileMap, tile_x - m_dragTileX, tile_y - m_dragTileY, overlayTileType, (int)AutoTileMap.eTileLayer.OVERLAY);
								}
								else
								{
									m_autoTileMap.BrushGizmo.BrushAction.Push( m_autoTileMap, tile_x - m_dragTileX, tile_y - m_dragTileY, gndTileType, (int)AutoTileMap.eTileLayer.GROUND);
									m_autoTileMap.BrushGizmo.BrushAction.Push( m_autoTileMap, tile_x - m_dragTileX, tile_y - m_dragTileY, gndOverlayTileType, (int)AutoTileMap.eTileLayer.GROUND_OVERLAY);
									m_autoTileMap.BrushGizmo.BrushAction.Push( m_autoTileMap, tile_x - m_dragTileX, tile_y - m_dragTileY, overlayTileType, (int)AutoTileMap.eTileLayer.OVERLAY);
								}
							}
						}

						m_autoTileMap.BrushGizmo.RefreshBrushGizmo( startTileX, startTileY, endTileX, endTileY, m_dragTileX, m_dragTileY, m_isCtrlKeyHold );

						m_dragTileX = m_dragTileY = -1;
					}

					if (Input.GetAxis("Mouse ScrollWheel") < 0) // back
					{
						if( m_camera2D.Zoom > 1f )
							m_camera2D.Zoom = Mathf.Max(m_camera2D.Zoom-1, 1);
						else
							m_camera2D.Zoom = Mathf.Max(m_camera2D.Zoom/2f, 0.25f);
					}
					else if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
					{
						if( m_camera2D.Zoom >= 1f )
							m_camera2D.Zoom = Mathf.Min(m_camera2D.Zoom+1, 10);
						else
							m_camera2D.Zoom*=2f;
					}
				}
			}
		}

		void _GenerateCollisionTexture()
		{
			SpriteRenderer sprRender = m_spriteCollLayer.GetComponent<SpriteRenderer>();
			Texture2D texture = new Texture2D( Screen.width/(AutoTileset.TilePartWidth/2) + 50 , Screen.height/(AutoTileset.TilePartHeight/2) + 50);
			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Clamp;
            sprRender.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), AutoTileset.PixelToUnits);
		}

		string[] m_tileGroupNames = new string[] {"A", "B", "C", "D", "E"};
		private int m_TilesetGroupIdx = 0;

		private Vector2 m_scrollPos = Vector2.zero;

		void OnGUI()
		{

			if( !m_isInitialized )
			{
				return;
			}

	#if UNITY_EDITOR
			m_isCtrlKeyHold = Event.current.shift;
	#else
			m_isCtrlKeyHold = Event.current.control || Event.current.shift;
	#endif
			Rect vRectTemp;

			float fPad = 4f;
			float fTilesetOffset = 64f;
			float fScrollBarWidth = 16f;
			float fTileGroupGrifHeight = 30f;

			int tilesWidth = AutoTileset.TileWidth*AutoTileset.AutoTilesPerRow;
			int tilesHeight = AutoTileset.TileHeight*(256 / AutoTileset.AutoTilesPerRow);

			m_rMinimapRect = new Rect( 
			                          Screen.width - m_autoTileMap.MinimapTexture.width, 
			                          Screen.height - m_autoTileMap.MinimapTexture.height,
			                          m_autoTileMap.MinimapTexture.width, 
			                          m_autoTileMap.MinimapTexture.height);
			m_rEditorRect = new Rect(0f, 0f, tilesWidth+2*fPad + fScrollBarWidth, Screen.height);
			m_rMapViewRect = new Rect( m_rEditorRect.x + m_rEditorRect.width, 0f, Screen.width - m_rEditorRect.width, Screen.height);
			m_rTilesetRect = new Rect( fPad, fTilesetOffset + fPad, tilesWidth + fScrollBarWidth, Screen.height);
			m_rTilesetRect.height -= (m_rTilesetRect.y + fPad + fTileGroupGrifHeight);

			vRectTemp = new Rect(m_rTilesetRect.x, Screen.height-fTileGroupGrifHeight, tilesWidth, fTileGroupGrifHeight);
			m_TilesetGroupIdx = GUI.SelectionGrid(vRectTemp, m_TilesetGroupIdx, m_tileGroupNames, m_tileGroupNames.Length);

			GUI.Box( m_rEditorRect, "" );

			if( GUI.Button( new Rect(0, 0, 130, 32), m_showCollisions? "Hide Collisions (C)" : "Show Collisions (C)") )
			{
				m_showCollisions = !m_showCollisions;
			}

			if( GUI.Button( new Rect(0, 32, 130, 32), m_showMinimap? "Hide Minimap (M)" : "Show Minimap (M)") )
			{
				m_showMinimap = !m_showMinimap;
			}

			if( GUI.Button( new Rect(130, 0, 120, 32), "Save") )
			{
				m_autoTileMap.ShowSaveDialog();
			}
			if( GUI.Button( new Rect(130, 32, 120, 32), "Load") )
			{
				m_autoTileMap.ShowLoadDialog();
			}

			Rect viewRect = new Rect(0, 0, m_rTilesetRect.width-fScrollBarWidth, tilesHeight);
			m_scrollPos = GUI.BeginScrollView(m_rTilesetRect, m_scrollPos, viewRect);
			//+++ Draw Tiles Thumbnails
			{
				float fTileRowNb = 32;
				vRectTemp = new Rect( 0f, 0f, AutoTileset.TileWidth * AutoTileset.AutoTilesPerRow, AutoTileset.TileHeight * fTileRowNb );
				vRectTemp.position += m_rEditorRect.position;
				int thumbIdx = 0;
				GUI.DrawTexture( vRectTemp, m_thumbnailTextures[m_TilesetGroupIdx] );
				for( int y = 0; thumbIdx < 256; ++y ) //256 number of tileset for each tileset group
				{
					for( int x = 0; x < AutoTileset.AutoTilesPerRow; ++x, ++thumbIdx )
					{
						Rect rDst = new Rect( x*AutoTileset.TileWidth, y*AutoTileset.TileHeight, AutoTileset.TileWidth, AutoTileset.TileHeight );
						rDst.position += vRectTemp.position;
						//if( MyAutoTileMap.IsAutoTileHasAlpha( x, y ) ) GUI.Box( rDst, "A" ); //for debug
						if( m_isCtrlKeyHold )
						{
							string sCollision = "";
							switch( m_autoTileMap.Tileset.AutotileCollType[m_TilesetGroupIdx*256 + thumbIdx] )
							{
							//NOTE: if you don't see the special characters properly, be sure this file is saved in UTF-8
							case AutoTileMap.eTileCollisionType.BLOCK: sCollision = "■"; break;
							case AutoTileMap.eTileCollisionType.FENCE: sCollision = "#"; break;
							case AutoTileMap.eTileCollisionType.WALL: sCollision = "□"; break;
							case AutoTileMap.eTileCollisionType.OVERLAY: sCollision = "★"; break;
							}
							if( sCollision.Length > 0 )
							{
								GUI.color = new Color(1f, 1f, 1f, 1f);
								GUIStyle style = new GUIStyle();
								style.fontSize = 30;
								style.fontStyle = FontStyle.Bold;
								style.alignment = TextAnchor.MiddleCenter;
								style.normal.textColor = Color.white;
								GUI.Box( rDst, sCollision, style );
								GUI.color = Color.white;
							}
						}
					}
				}
				Rect rSelected = new Rect( 0, 0, AutoTileset.TileWidth , AutoTileset.TileHeight );

				int tileWithSelectMark = m_selectedTileIdx;
				tileWithSelectMark -= (m_TilesetGroupIdx * 256);
				rSelected.position = vRectTemp.position + new Vector2( (tileWithSelectMark % AutoTileset.AutoTilesPerRow)*AutoTileset.TileWidth, (tileWithSelectMark / AutoTileset.AutoTilesPerRow)*AutoTileset.TileHeight );

				UtilsGuiDrawing.DrawRectWithOutline( rSelected, new Color( 0f, 0f, 0f, 0.1f ), new Color( 1f, 1f, 1f, 1f ) );
			}
			//----
			GUI.EndScrollView();

			if( m_showMinimap )
			{
				//NOTE: the texture is drawn blurred in web player unless default quality is set to Fast in project settings
				// see here for solution http://forum.unity3d.com/threads/webplayer-gui-issue.100256/#post-868451
				GUI.DrawTexture( m_rMinimapRect, m_autoTileMap.MinimapTexture );
				UtilsGuiDrawing.DrawRectWithOutline( m_rMinimapRect, new Color(0, 0, 0, 0), Color.black );

				// Draw camera region on minimap
				Vector3 vCameraPos = m_autoTileMap.ViewCamera.ScreenPointToRay(new Vector3(0, Screen.height-1)).origin;
				int camTileX = (int)(vCameraPos.x / AutoTileset.TileWorldWidth);
				int camTileY = (int)(-vCameraPos.y / AutoTileset.TileWorldHeight);
				Rect rMinimapCam = new Rect( camTileX, camTileY, Screen.width/(m_camera2D.Zoom*AutoTileset.TileWidth), Screen.height/(m_camera2D.Zoom*AutoTileset.TileHeight) );
				rMinimapCam.position += m_rMinimapRect.position;
				UtilsGuiDrawing.DrawRectWithOutline( rMinimapCam, new Color(0, 0, 0, 0), Color.white );

				// Draw player on minimap
				if( m_camera2DFollowBehaviour != null && m_camera2DFollowBehaviour.Target != null )
				{
					int plyTileX = -1 + (int)(m_camera2DFollowBehaviour.Target.transform.position.x / AutoTileset.TileWorldWidth);
					int plyTileY = -1 + (int)(-m_camera2DFollowBehaviour.Target.transform.position.y / AutoTileset.TileWorldHeight);
					Rect rPlayer = new Rect(plyTileX, plyTileY, 3, 3);
					rPlayer.position += m_rMinimapRect.position;
					UtilsGuiDrawing.DrawRectWithOutline( rPlayer, Color.yellow, Color.blue );
				}
			}

			#region Draw Selection Rect
			// Map Version
			if( m_drawSelectionRect )
			{
				Rect selRect = new Rect( );
				selRect.width = (Mathf.Abs( m_dragTileX - m_startDragTileX ) + 1) * AutoTileset.TileWidth * m_camera2D.Zoom;
				selRect.height = (Mathf.Abs( m_dragTileY - m_startDragTileY ) + 1) * AutoTileset.TileHeight * m_camera2D.Zoom;
				float worldX = Mathf.Min( m_startDragTileX, m_dragTileX ) * AutoTileset.TileWorldWidth;
				float worldY = -Mathf.Min( m_startDragTileY, m_dragTileY ) * AutoTileset.TileWorldHeight;
				Vector3 vScreen = m_camera2D.GetComponent<Camera>().WorldToScreenPoint( new Vector3( worldX, worldY) + m_autoTileMap.transform.position);
				selRect.position = new Vector2( vScreen.x, vScreen.y );
				selRect.y = Screen.height - selRect.y;
				UtilsGuiDrawing.DrawRectWithOutline( selRect, new Color(0f, 1f, 0f, 0.2f), new Color(0f, 1f, 0f, 1f));
			}
			// Tileset Version
			if( m_tilesetSelStart >= 0 && m_tilesetSelEnd >= 0 )
			{
				int tilesetIdxStart = m_tilesetSelStart - (m_TilesetGroupIdx * 256); // make it relative to selected tileset
				int tilesetIdxEnd = m_tilesetSelEnd - (m_TilesetGroupIdx * 256); // make it relative to selected tileset
				Rect selRect = new Rect( );
				int TileStartX = tilesetIdxStart % AutoTileset.AutoTilesPerRow;
				int TileStartY = tilesetIdxStart / AutoTileset.AutoTilesPerRow;
				int TileEndX = tilesetIdxEnd % AutoTileset.AutoTilesPerRow;
				int TileEndY = tilesetIdxEnd / AutoTileset.AutoTilesPerRow;
				selRect.width = (Mathf.Abs( TileEndX - TileStartX ) + 1) * AutoTileset.TileWidth;
				selRect.height = (Mathf.Abs( TileEndY - TileStartY ) + 1) * AutoTileset.TileHeight;
				float scrX = Mathf.Min( TileStartX, TileEndX ) * AutoTileset.TileWidth;
				float scrY = Mathf.Min( TileStartY, TileEndY ) * AutoTileset.TileHeight;
				selRect.position = new Vector2( scrX, scrY - m_scrollPos.y );
				selRect.position += m_rTilesetRect.position;
				//selRect.y = Screen.height - selRect.y;
				UtilsGuiDrawing.DrawRectWithOutline( selRect, new Color(0f, 1f, 0f, 0.2f), new Color(0f, 1f, 0f, 1f));
			}
			#endregion
		}
	}
}
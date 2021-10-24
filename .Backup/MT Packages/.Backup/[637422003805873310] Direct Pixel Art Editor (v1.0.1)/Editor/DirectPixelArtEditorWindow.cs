using MT.Packages.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MT.Packages.DPAE
{
	public class DirectPixelArtEditorWindow : EditorWindow {
		[MenuItem("Tools/" + Utility.ASSET_NAME + "/Direct Pixel Art Editor")]
		static void Init() {
			var window = GetWindow<DirectPixelArtEditorWindow>("Direct Pixel Art Editor", typeof(SceneView));
			window.brushToolWindow = new BrushToolWindow(window);
			window.createNewTextureToolWindow = new CreateNewTextureToolWindow(window);
			window.framesToolWindow = new FramesToolWindow(window);
			window.generalToolWindow = new GeneralToolWindow(window);
			window.historyToolWindow = new HistoryToolWindow(window);
			window.resizeTextureToolWindow = new ResizeTextureToolWindow(window);
			window.settingsToolWindow = new SettingsToolWindow(window);
			window.Setup();
			foreach (var toolWindow in window.toolWindows) {
				Vector2Int position = toolWindow.GetDefaultPosition();
				toolWindow.window.x = position.x;
				toolWindow.window.y = position.y;
			}
		}

		public List<ToolWindow> toolWindows;

		public BrushToolWindow brushToolWindow;
		public CreateNewTextureToolWindow createNewTextureToolWindow;
		public FramesToolWindow framesToolWindow;
		public GeneralToolWindow generalToolWindow;
		public HistoryToolWindow historyToolWindow;
		public ResizeTextureToolWindow resizeTextureToolWindow;
		public SettingsToolWindow settingsToolWindow;
		
		public Texture2D sourceTexture;
		public Texture2D workingTexture;
		public int zoomLevel = 1;
		public Vector2 panOffset;

		public Texture2D lastSourceTexture;
		public bool reloadSourceTexture;

		bool _resetSourceTexture;
		bool _panning;
		Vector2 _panOffsetBefore;
		bool _drawing;
		Vector2Int _lastDrawnPixel;

		void Setup() {
			toolWindows = new List<ToolWindow> {
				brushToolWindow,
				createNewTextureToolWindow,
				framesToolWindow,
				generalToolWindow,
				historyToolWindow,
				resizeTextureToolWindow,
				settingsToolWindow
			};
		}

		void Update() {
			if (toolWindows == null) {
				Setup();
			}
			Repaint();
		}

		void OnGUI() {
			if (toolWindows == null) {
				return;
			}
			var e = Event.current;
			GUILayout.BeginArea(new Rect(0, 0, position.width, position.height));
			if (sourceTexture) {
				var rect = new Rect(position.width / 2 - 200, position.height / 2 - 20, 400, 30);
				var errorStyle = new GUIStyle(GUI.skin.label) {
					alignment = TextAnchor.MiddleCenter
				};
				if (!sourceTexture.isReadable) {
					DestroyWorkingTexture();
					_resetSourceTexture = true;
					EditorGUI.LabelField(rect, "Error: 'Read/Write' is not enabled", errorStyle);
				} else {
					TextureImporter textureAsset = (TextureImporter) AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sourceTexture));
					if (textureAsset && textureAsset.textureCompression != TextureImporterCompression.Uncompressed) {
						DestroyWorkingTexture();
						_resetSourceTexture = true;
						EditorGUI.LabelField(rect, "Error: 'Texture Compression' is enabled", errorStyle);
					} else {
						if (_resetSourceTexture) {
							_resetSourceTexture = false;
							sourceTexture = null;
						}
						if (workingTexture) {
							OnGUI_WorkingTexture(e);
						}
					}
				}
			}
			GUILayout.EndArea();

			OnGUI_PreWindows();

			BeginWindows();
			OnGUI_Windows();
			EndWindows();

			OnGUI_PostWindows();
		}

		void OnGUI_WorkingTexture(Event e) {

			Vector3 mouse = new Vector3(e.mousePosition.x, e.mousePosition.y, e.delta.y);
			bool mouseInsideToolWindow = false;
			foreach (var toolWindow in toolWindows) {
				if (toolWindow.window.Contains(e.mousePosition)) {
					mouseInsideToolWindow = true;
					break;
				}
			}

			Vector2 workspaceCenter = new Vector2(position.width / 2, position.height / 2);
			Vector2 size = new Vector2(
				workingTexture.width * zoomLevel,
				workingTexture.height * zoomLevel
			);

			if (!_panning && e.isScrollWheel) {
				int zoomFactor = Mathf.CeilToInt(zoomLevel / 4f);
				Vector2 sizeAfter = new Vector2(
					workingTexture.width * (zoomLevel + zoomFactor),
					workingTexture.height * (zoomLevel + zoomFactor)
				);
				Vector2 additionalPanOffset = new Vector2(
					(sizeAfter.x - size.x) / 2 * ((mouse.x - workspaceCenter.x - panOffset.x) / (size.x / 2)),
					(sizeAfter.y - size.y) / 2 * ((mouse.y - workspaceCenter.y - panOffset.y) / (size.y / 2))
				);
				if (mouse.z < 0) {
					panOffset.x -= additionalPanOffset.x;
					panOffset.y -= additionalPanOffset.y;
					zoomLevel += zoomFactor;
				} else if (mouse.z > 0 && zoomLevel > 1) {
					panOffset.x += additionalPanOffset.x;
					panOffset.y += additionalPanOffset.y;
					zoomLevel -= zoomFactor;
				}
				if (zoomLevel < 1) {
					zoomLevel = 1;
				}
				size = new Vector2(
					workingTexture.width * zoomLevel,
					workingTexture.height * zoomLevel
				);
			}

			float x = (position.width - size.x) / 2 + panOffset.x;
			float y = (position.height - size.y) / 2 + panOffset.y;

			Rect rect = new Rect(x, y, size.x, size.y);
			EditorGUI.DrawRect(rect, settingsToolWindow.transparentBackgroundColor);
			var indexCoords = framesToolWindow.GetIndexCoords();
			float fw = 1f / framesToolWindow.grid.x;
			float fh = 1f / framesToolWindow.grid.y;
			float fx = fw * indexCoords.x;
			float fy = 1f - fh * (indexCoords.y + 1);
			GUI.DrawTextureWithTexCoords(rect, workingTexture, new Rect(fx, fy, fw, fh));
			
			Vector2Int pixel = new Vector2Int(
				Mathf.FloorToInt((mouse.x - x) / zoomLevel / framesToolWindow.grid.x),
				Mathf.FloorToInt((mouse.y - y) / zoomLevel / framesToolWindow.grid.y)
			);
			pixel.x += workingTexture.width / framesToolWindow.grid.x * indexCoords.x;
			pixel.y += workingTexture.height / framesToolWindow.grid.y * indexCoords.y;

			if (!mouseInsideToolWindow) {
				if (e.type == EventType.MouseDown) {
					if (e.button == 0) {
						if (brushToolWindow.type == 2) {
							Fill(pixel.x, pixel.y, brushToolWindow.color);
							ApplyWorkingTextureChanges();
							historyToolWindow.WriteHistory();
						} else {
							_drawing = true;
							_lastDrawnPixel = new Vector2Int(-1, -1);
						}
					}
					if (e.button == 1) {
						Color? pixelColor = GetPixelColor(pixel.x, pixel.y);
						if (pixelColor.HasValue) {
							brushToolWindow.color = pixelColor.Value;
						}
					}
					if (e.button == 2) {
						_panning = true;
						_panOffsetBefore = e.mousePosition - panOffset;
					}
				}
			}

			if (e.type == EventType.MouseUp) {
				if (e.button == 0 && _drawing) {
					_drawing = false;
					historyToolWindow.WriteHistory();
				}
				if (e.button == 2) {
					_panning = false;
				}
			}

			if (e.type == EventType.KeyDown) {
				if (brushToolWindow.type == 0 && e.control) {
					brushToolWindow.type = 1;
				}
			} else if (e.type == EventType.KeyUp) {
				if (brushToolWindow.type == 1 && !e.control) {
					brushToolWindow.type = 0;
				}
			}

			if (_drawing) {
				for (int i = 0; i < brushToolWindow.size; i++) {
					for (int j = 0; j < brushToolWindow.size; j++) {
						if (brushToolWindow.type == 1) {
							PaintPixel(pixel.x + i, pixel.y + j, Color.clear, false);
						} else if (brushToolWindow.type == 0) {
							PaintPixel(pixel.x + i, pixel.y + j, brushToolWindow.color, true);
						}
					}
				}
				ApplyWorkingTextureChanges();
			}

			if (_panning) {
				panOffset = e.mousePosition - _panOffsetBefore;
			}

			if (!mouseInsideToolWindow && zoomLevel >= 4) {
				var s = 1; // zoomLevel / 8;
				var zoomSize = zoomLevel * brushToolWindow.size * framesToolWindow.grid.x;
				x = x + (pixel.x - workingTexture.width / framesToolWindow.grid.x * indexCoords.x) * zoomLevel * framesToolWindow.grid.x;
				y = y + (pixel.y - workingTexture.height / framesToolWindow.grid.y * indexCoords.y) * zoomLevel * framesToolWindow.grid.y;
				Color brushColor = brushToolWindow.color;
				Color cursorColor = new Color(brushColor.r, brushColor.g, brushColor.b, Mathf.Max(brushColor.a, 0.5f));
				EditorGUI.DrawRect(new Rect(x, y, zoomSize, s), cursorColor);
				EditorGUI.DrawRect(new Rect(x, y + zoomSize - s, zoomSize, s), cursorColor);
				EditorGUI.DrawRect(new Rect(x, y + s, s, zoomSize - s * 2), cursorColor);
				EditorGUI.DrawRect(new Rect(x + zoomSize - s, y + s, s, zoomSize - s * 2), cursorColor);
			}
		}

		void OnGUI_PreWindows() {
			lastSourceTexture = sourceTexture;
			if (historyToolWindow.entries.Count <= 1 && Selection.activeObject is Texture2D activeTexture) {
				sourceTexture = activeTexture;
			}
		}

		void OnGUI_Windows() {
			foreach (var toolWindow in toolWindows) {
				toolWindow.OnGUI();
			}
		}

		void OnGUI_PostWindows() {
			if (sourceTexture && sourceTexture.isReadable && (!workingTexture || sourceTexture != lastSourceTexture || reloadSourceTexture)) {
				if (workingTexture) {
					DestroyImmediate(workingTexture);
				}
				workingTexture = sourceTexture.Clone();
				resizeTextureToolWindow.size = new Vector2Int(workingTexture.width, workingTexture.height);
				historyToolWindow.ResetHistory();
				if (!reloadSourceTexture) {
					ResetView();
				}
			} else if ((!sourceTexture || !sourceTexture.isReadable) && workingTexture) {
				DestroyWorkingTexture();
			}
		}

		public void ResetView() {
			panOffset = new Vector2(0, 0);
			zoomLevel = (int) (position.height / (workingTexture.height + 2));
		}

		public Color? GetPixelColor(int x, int y) {
			if (x >= 0 && x < workingTexture.width) {
				if (y >= 0 && y < workingTexture.height) {
					return workingTexture.GetPixel(x, workingTexture.height - y - 1);
				}
			}
			return null;
		}

		public void SetPixelColor(int x, int y, Color color) {
			if (x >= 0 && x < workingTexture.width) {
				if (y >= 0 && y < workingTexture.height) {
					workingTexture.SetPixel(x, workingTexture.height - y - 1, color);
				}
			}
		}

		void Fill(int x, int y, Color color) {
			if (!GetPixelColor(x, y).HasValue) {
				return;
			}
			Color[] textureColors = workingTexture.GetPixels();
			int w = workingTexture.width;
			int h = workingTexture.height;
			Color originColor = textureColors[w * (h - y - 1) + x];

			void Fill(int _x, int _y, Color _color, int _w, int _h, Color _originColor, ref Color[] _textureColors, int _limit) {
				if (_limit > 1000) {
					return;
				}
				_limit++;
				if (_color == _originColor) {
					return;
				}
				if (_x < 0 || _y < 0 || _x >= _w || _y >= _h) {
					return;
				}
				int index = _w * (_h - _y - 1) + _x;
				Color pixelColor = _textureColors[index];
				if (pixelColor != _originColor) {
					return;
				}
				_textureColors[index] = _color;
				Fill(_x - 1, _y, _color, _w, _h, _originColor, ref _textureColors, _limit);
				Fill(_x + 1, _y, _color, _w, _h, _originColor, ref _textureColors, _limit);
				Fill(_x, _y - 1, _color, _w, _h, _originColor, ref _textureColors, _limit);
				Fill(_x, _y + 1, _color, _w, _h, _originColor, ref _textureColors, _limit);
			}

			Fill(x, y, color, w, h, originColor, ref textureColors, 0);

			workingTexture.SetPixels(textureColors);
		}

		void PaintPixel(int x, int y, Color color, bool blendAlpha) {
			Color? pixelColor = GetPixelColor(x, y);
			if (!pixelColor.HasValue) {
				return;
			}
			Vector2Int pixel = new Vector2Int(x, y);
			if (_lastDrawnPixel != pixel) {
				_lastDrawnPixel = pixel;
				if (blendAlpha) {
					SetPixelColor(x, y, GetAlphaBlendedColor(pixelColor.Value, brushToolWindow.color));
				} else {
					SetPixelColor(x, y, Color.clear);
				}
			}
		}

		public void ApplyWorkingTextureChanges() {
			workingTexture.Apply();
		}

		void DestroyWorkingTexture() {
			foreach (var entry in historyToolWindow.entries) {
				DestroyImmediate(entry);
			}
			historyToolWindow.entries.Clear();
			historyToolWindow.index = 0;
			if (workingTexture) {
				DestroyImmediate(workingTexture);
				workingTexture = null;
			}
		}

		public static Texture2D CreateTexture(int width, int height, Texture2D sourceTexture, TextAnchor anchor) {
			Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false) {
				alphaIsTransparency = true,
				filterMode = FilterMode.Point
			};
			Color[] pixels = texture.GetPixels();
			for (int i = 0; i < pixels.Length; i++) {
				pixels[i] = Color.clear;
			}
			texture.SetPixels(pixels);
			if (sourceTexture) {
				for (int i = 0; i < sourceTexture.width && i < width; i++) {
					for (int j = 0; j < sourceTexture.height && j < height; j++) {
						switch (anchor) {
							case TextAnchor.LowerLeft:
								texture.SetPixel(i, j, sourceTexture.GetPixel(i, j));
								break;
						}

					}
				}
			}
			texture.Apply();
			return texture;
		}

		static Color GetAlphaBlendedColor(Color srcColor, Color dstColor) {
			return new Color(
				Mathf.Lerp(srcColor.r, dstColor.r, dstColor.a),
				Mathf.Lerp(srcColor.g, dstColor.g, dstColor.a),
				Mathf.Lerp(srcColor.b, dstColor.b, dstColor.a),
				srcColor.a + (1 - srcColor.a) * dstColor.a
			);
		}
	}
}
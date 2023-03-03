using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GDExtension;

using NAudio.CoreAudioApi;

using RhubarbVR.Bindings.Input;
using RhubarbVR.Bindings.TextureBindings;

using RhuEngine;
using RhuEngine.Components;
using RhuEngine.Input;
using RhuEngine.Linker;
using RhuEngine.WorldObjects.ECS;

using SixLabors.ImageSharp.Processing;

using Viewport = RhuEngine.Components.Viewport;

namespace RhubarbVR.Bindings.ComponentLinking
{
	public sealed class ViewportLink : WorldNodeLinked<Viewport, SubViewport>
	{
		public override string ObjectName => "Viewport";

		public class InputAction
		{
			public RNumerics.Vector2f Pos;
			public RNumerics.Vector2f LastPos;

			public RNumerics.Vector2f Tilt;
			public float PressForce;

			public bool IsClickedPrime;
			public bool IsClickedPrimeLastFrame;
			public double IsClickedPrimeTimeStateChange;


			public bool IsClickedSecod;
			public bool IsClickedSecodLastFrame;
			public double IsClickedSecodTimeStateChange;

			public bool IsClickedTur;
			public bool IsClickedTurLastFrame;
			public double IsClickedTurTimeStateChange;

		}

		public Dictionary<int, InputAction> InputActions = new();

		public const float TIME_FOR_DOUBLE_CLICK = 0.25f;

		public override void Render() {
			if (LinkedComp.Engine.KeyboardInteraction is Component element && element.Entity.Viewport == LinkedComp) {
				foreach (var item in KeyboardSystem._keys) {
					var time = LinkedComp.InputManager.KeyboardSystem.GetStateChangeTime(item);
					if (time is 0 or > 0.5f) {
						var keyInput = new InputEventKey {
							Pressed = LinkedComp.InputManager.KeyboardSystem.IsKeyDown(item),
							Echo = !LinkedComp.InputManager.KeyboardSystem.IsKeyJustDown(item),
							Keycode = GodotKeyboard.ToGodotKey(item),
							AltPressed = LinkedComp.InputManager.KeyboardSystem.IsKeyDown(RhuEngine.Linker.Key.Alt),
							CtrlPressed = LinkedComp.InputManager.KeyboardSystem.IsKeyDown(RhuEngine.Linker.Key.Ctrl),
							ShiftPressed = LinkedComp.InputManager.KeyboardSystem.IsKeyDown(RhuEngine.Linker.Key.Shift),

						};
						node.PushInput(keyInput, true);
					}
				}
				foreach (var item in LinkedComp.InputManager.KeyboardSystem.TypeDelta.EnumerateRunes()) {
					var keyInput = new InputEventKey {
						Pressed = true,
						AltPressed = LinkedComp.InputManager.KeyboardSystem.IsKeyDown(RhuEngine.Linker.Key.Alt),
						CtrlPressed = LinkedComp.InputManager.KeyboardSystem.IsKeyDown(RhuEngine.Linker.Key.Ctrl),
						ShiftPressed = LinkedComp.InputManager.KeyboardSystem.IsKeyDown(RhuEngine.Linker.Key.Shift),
						Unicode = item.Value
					};
					node.PushInput(keyInput, true);
				}
			}
			foreach (var item in InputActions) {
				var value = item.Value;
				var id = item.Key;


				var buttonMask = (MouseButtonMask)0;
				if (value.IsClickedPrime) {
					buttonMask |= MouseButtonMask.Left;
				}
				if (value.IsClickedSecod) {
					buttonMask |= MouseButtonMask.Right;
				}
				if (value.IsClickedTur) {
					buttonMask |= MouseButtonMask.Middle;
				}
				var inputMove = new InputEventMouseMotion {
					Position = new Vector2(value.Pos.x, value.Pos.y),
					Device = id,
					Pressure = value.PressForce,
					ButtonMask = buttonMask,
					Velocity = new Vector2(value.Pos.x, value.Pos.y) - new Vector2(value.LastPos.x, value.LastPos.y),
					Tilt = new Vector2(value.Tilt.x, value.Tilt.y),
				};
				node.PushInput(inputMove, true);



				if (value.IsClickedPrime & value.IsClickedPrimeLastFrame) {
					var dragEvent = new InputEventScreenDrag {
						Device = id,
						Position = new Vector2(value.Pos.x, value.Pos.y),
						Relative = new Vector2(value.Pos.x, value.Pos.y) - new Vector2(value.LastPos.x, value.LastPos.y),
					};
					node.PushInput(dragEvent, true);
				}


				value.LastPos = value.Pos;

				var scroll = LinkedComp.Engine.inputManager.MouseSystem.ScrollDelta;

				if (scroll.y > 0) {
					var scrollweel = new InputEventMouseButton {
						Device = id,
						ButtonIndex = MouseButton.WheelUp,
						Pressed = true,
						Position = new Vector2(value.Pos.x, value.Pos.y),
					};
					node.PushInput(scrollweel, true);
				}
				if (scroll.y < 0) {
					var scrollweel = new InputEventMouseButton {
						Device = id,
						ButtonIndex = MouseButton.WheelDown,
						Pressed = true,
						Position = new Vector2(value.Pos.x, value.Pos.y),
					};
					node.PushInput(scrollweel, true);
				}
				if (scroll.x > 0) {
					var scrollweel = new InputEventMouseButton {
						Device = id,
						ButtonIndex = MouseButton.WheelRight,
						Pressed = true,
						Position = new Vector2(value.Pos.x, value.Pos.y),
					};
					node.PushInput(scrollweel, true);
				}
				if (scroll.x < 0) {
					var scrollweel = new InputEventMouseButton {
						Device = id,
						ButtonIndex = MouseButton.WheelLeft,
						Pressed = true,
						Position = new Vector2(value.Pos.x, value.Pos.y),
					};
					node.PushInput(scrollweel, true);
				}


				if (value.IsClickedPrime != value.IsClickedPrimeLastFrame) {
					var primeinputButton = new InputEventMouseButton {
						Device = id,
						ButtonIndex = MouseButton.Left,
						ButtonMask = MouseButtonMask.Left,
						Pressed = value.IsClickedPrime,
						DoubleClick = value.IsClickedPrimeTimeStateChange <= TIME_FOR_DOUBLE_CLICK,
						Position = new Vector2(value.Pos.x, value.Pos.y),
					};
					node.PushInput(primeinputButton, true);
					value.IsClickedPrimeTimeStateChange = 0;
				}
				else {
					value.IsClickedPrimeTimeStateChange += RTime.Elapsed;
				}

				if (value.IsClickedSecod != value.IsClickedSecodLastFrame) {
					var secinputButton = new InputEventMouseButton {
						Device = id,
						ButtonIndex = MouseButton.Right,
						ButtonMask = MouseButtonMask.Right,
						Pressed = value.IsClickedSecod,
						DoubleClick = value.IsClickedSecodTimeStateChange <= TIME_FOR_DOUBLE_CLICK,
						Position = new Vector2(value.Pos.x, value.Pos.y),
					};
					node.PushInput(secinputButton, true);
					value.IsClickedSecodTimeStateChange = 0;
				}
				else {
					value.IsClickedSecodTimeStateChange += RTime.Elapsed;
				}


				if (value.IsClickedTur != value.IsClickedTurLastFrame) {
					var turinputButton = new InputEventMouseButton {
						Device = id,
						ButtonIndex = MouseButton.Middle,
						ButtonMask = MouseButtonMask.Middle,
						Pressed = value.IsClickedTur,
						DoubleClick = value.IsClickedTurTimeStateChange <= TIME_FOR_DOUBLE_CLICK,
						Position = new Vector2(value.Pos.x, value.Pos.y),
					};
					node.PushInput(turinputButton, true);
					value.IsClickedTurTimeStateChange = 0;
				}
				else {
					value.IsClickedTurTimeStateChange += RTime.Elapsed;
				}

				value.IsClickedPrimeLastFrame = value.IsClickedPrime;
				value.IsClickedSecodLastFrame = value.IsClickedSecod;
				value.IsClickedTurLastFrame = value.IsClickedTur;

			}
			LinkedComp.RCursorShape = (RCursorShape)GDExtension.Input.GetCurrentCursorShape();
		}

		public override void StartContinueInit() {
			LinkedComp.Size.Changed += Size_Changed;
			LinkedComp.Size2DOverride.Changed += Size2DOverride_Changed;
			LinkedComp.Size2DOverrideStretch.Changed += Size2DOverrideStretch_Changed;
			LinkedComp.UseTAA.Changed += UseTAA_Changed;
			LinkedComp.UseDebanding.Changed += UseDebanding_Changed;
			LinkedComp.Disable3D.Changed += Disable3D_Changed;
			LinkedComp.OwnWorld3D.Changed += OwnWorld3D_Changed;
			LinkedComp.TransparentBG.Changed += TransparentBG_Changed;
			LinkedComp.Snap2DTransformsToPixels.Changed += Snap2DTransformsToPixels_Changed;
			LinkedComp.Snap2DVerticesToPixels.Changed += Snap2DVerticesToPixels_Changed;
			LinkedComp.Msaa2D.Changed += Msaa2D_Changed;
			LinkedComp.Msaa3D.Changed += Msaa3D_Changed;
			LinkedComp.ScreenSpaceAA.Changed += ScreenSpaceAA_Changed;
			LinkedComp.ClearMode.Changed += ClearMode_Changed;
			LinkedComp.UpdateMode.Changed += UpdateMode_Changed;
			LinkedComp.UseOcclusionCulling.Changed += UseOcclusionCulling_Changed;
			LinkedComp.DebugDraw.Changed += DebugDraw_Changed;
			LinkedComp.Scaling3DMode.Changed += Scaling3DMode_Changed;
			LinkedComp.Scaling3DScale.Changed += Scaling3DScale_Changed;
			LinkedComp.TextureMipmapBias.Changed += TextureMipmapBias_Changed;
			LinkedComp.FSRSharpness.Changed += FSRSharpness_Changed;
			LinkedComp.CanvasDefaultTextureFilter.Changed += CanvasDefaultTextureFilter_Changed;
			LinkedComp.CanvasDefaultTextureRepate.Changed += CanvasDefaultTextureRepate_Changed;
			LinkedComp.SnapUIToPixels.Changed += SnapUIToPixels_Changed;
			LinkedComp.SDFOversize.Changed += SDFOversize_Changed;
			LinkedComp.SDFScale.Changed += SDFScale_Changed;
			LinkedComp.PositionalShadowAtlas.Changed += PositionalShadowAtlas_Changed;
			LinkedComp.PositionalShadow16Bit.Changed += PositionalShadow16Bit_Changed;
			LinkedComp.QuadZero.Changed += QuadZero_Changed;
			LinkedComp.QuadOne.Changed += QuadOne_Changed;
			LinkedComp.QuadTwo.Changed += QuadTwo_Changed;
			LinkedComp.QuadThree.Changed += QuadThree_Changed;
			LinkedComp.GUIDisableInput.Changed += GUIDisableInput_Changed;
			Children_OnReorderList();
			GUIDisableInput_Changed(null);
			Size_Changed(null);
			Size2DOverride_Changed(null);
			Size2DOverrideStretch_Changed(null);
			UseTAA_Changed(null);
			UseDebanding_Changed(null);
			Disable3D_Changed(null);
			OwnWorld3D_Changed(null);
			TransparentBG_Changed(null);
			Snap2DTransformsToPixels_Changed(null);
			Snap2DVerticesToPixels_Changed(null);
			Msaa2D_Changed(null);
			Msaa3D_Changed(null);
			ScreenSpaceAA_Changed(null);
			ClearMode_Changed(null);
			UpdateMode_Changed(null);
			UseOcclusionCulling_Changed(null);
			DebugDraw_Changed(null);
			Scaling3DMode_Changed(null);
			Scaling3DScale_Changed(null);
			TextureMipmapBias_Changed(null);
			FSRSharpness_Changed(null);
			CanvasDefaultTextureFilter_Changed(null);
			CanvasDefaultTextureRepate_Changed(null);
			SnapUIToPixels_Changed(null);
			SDFOversize_Changed(null);
			SDFScale_Changed(null);
			PositionalShadowAtlas_Changed(null);
			PositionalShadow16Bit_Changed(null);
			QuadZero_Changed(null);
			QuadOne_Changed(null);
			QuadTwo_Changed(null);
			QuadThree_Changed(null);
			LinkedComp.Load(new RTexture2D(new GodotTexture2D(node.GetTexture())));
			LinkedComp.SendInputEvent = UpdateInput;
			LinkedComp.ClearBackGroundCalled = ClearCalled;
			LinkedComp.RenderFrameCalled = RenderFrameCalled;
			LinkedComp.Entity.children.OnReorderList += Children_OnReorderList;
			node.GuiEmbedSubwindows = true;
		}

		private void GUIDisableInput_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.GuiDisableInput = LinkedComp.GUIDisableInput.Value);
		}

		private void UpdateInput(RNumerics.Vector2f pos, RNumerics.Vector2f Tilt, float PressForce, Handed side, int current, bool isLazer, bool IsClickedPrime, bool IsClickedSecod, bool IsClickedTur) {
			if (_isInputUpdate) {
				node.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
			}
			var deviceid = (current << 8) | 255 | ((int)side << 16);
			if (!InputActions.TryGetValue(deviceid, out var inputAction)) {
				inputAction = new InputAction();
				InputActions.Add(deviceid, inputAction);
			}
			inputAction.Pos = new RNumerics.Vector2f(pos.x * node.Size.x, pos.y * node.Size.y);
			inputAction.PressForce = PressForce;
			inputAction.Tilt = Tilt;
			inputAction.IsClickedPrime = IsClickedPrime;
			inputAction.IsClickedSecod = IsClickedSecod;
			inputAction.IsClickedTur = IsClickedTur;
		}

		private void Children_OnReorderList() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				for (var i = 0; i < LinkedComp.Entity.children.Count; i++) {
					var item = LinkedComp.Entity.children[i];
					if (item?.CanvasItem?.WorldLink is ICanvasItemNodeLinked canvasItem) {
						if (canvasItem?.CanvasItem?.GetParent() == node) {
							node.MoveChild(canvasItem.CanvasItem, -1);
						}
						else {
							canvasItem.CanvasItem?.GetParent()?.RemoveChild(canvasItem.CanvasItem);
							node.AddChild(canvasItem.CanvasItem);
							canvasItem.CanvasItem.Owner = node;
							node.MoveChild(canvasItem.CanvasItem, -1);
						}
					}
				}
			});
		}

		private void RenderFrameCalled() {
			switch (node.RenderTargetUpdateMode) {
				case SubViewport.UpdateMode.Disabled:
					node.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
					break;
				case SubViewport.UpdateMode.Once:
					node.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
					break;
				default:
					break;
			}
		}

		private void ClearCalled() {
			RenderThread.ExecuteOnEndOfFrame(() => {
				switch (node.RenderTargetClearMode) {
					case SubViewport.ClearMode.Never:
						node.RenderTargetClearMode = SubViewport.ClearMode.Once;
						break;
					case SubViewport.ClearMode.Once:
						node.RenderTargetClearMode = SubViewport.ClearMode.Once;
						break;
					default:
						break;
				}
			});
		}

		private void QuadThree_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.PositionalShadowAtlasQuad3 = LinkedComp.QuadThree.Value switch {
				RShadowSelect.Shadow_1 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._1,
				RShadowSelect.Shadows_4 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._4,
				RShadowSelect.Shadows_16 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._16,
				RShadowSelect.Shadows_64 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._64,
				RShadowSelect.Shadows_256 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._256,
				RShadowSelect.Shadows_1024 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._1024,
				_ => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv.Disabled,
			});;
		}

		private void QuadTwo_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.PositionalShadowAtlasQuad2 = LinkedComp.QuadTwo.Value switch {
				RShadowSelect.Shadow_1 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._1,
				RShadowSelect.Shadows_4 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._4,
				RShadowSelect.Shadows_16 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._16,
				RShadowSelect.Shadows_64 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._64,
				RShadowSelect.Shadows_256 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._256,
				RShadowSelect.Shadows_1024 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._1024,
				_ => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv.Disabled,
			});
		}

		private void QuadOne_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.PositionalShadowAtlasQuad1 = LinkedComp.QuadOne.Value switch {
				RShadowSelect.Shadow_1 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._1,
				RShadowSelect.Shadows_4 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._4,
				RShadowSelect.Shadows_16 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._16,
				RShadowSelect.Shadows_64 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._64,
				RShadowSelect.Shadows_256 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._256,
				RShadowSelect.Shadows_1024 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._1024,
				_ => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv.Disabled,
			});
		}

		private void QuadZero_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.PositionalShadowAtlasQuad0 = LinkedComp.QuadZero.Value switch {
				RShadowSelect.Shadow_1 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._1,
				RShadowSelect.Shadows_4 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._4,
				RShadowSelect.Shadows_16 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._16,
				RShadowSelect.Shadows_64 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._64,
				RShadowSelect.Shadows_256 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._256,
				RShadowSelect.Shadows_1024 => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv._1024,
				_ => GDExtension.Viewport.PositionalShadowAtlasQuadrantSubdiv.Disabled,
			});
		}

		private void PositionalShadow16Bit_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.PositionalShadowAtlas16Bits = LinkedComp.PositionalShadow16Bit.Value);
		}

		private void PositionalShadowAtlas_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.PositionalShadowAtlasSize = LinkedComp.PositionalShadowAtlas.Value);
		}

		private void SDFScale_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.SdfScale = LinkedComp.SDFScale.Value switch { RSDFSize._100 => GDExtension.Viewport.SDFScale._100Percent, RSDFSize._50 => GDExtension.Viewport.SDFScale._50Percent, _ => GDExtension.Viewport.SDFScale._25Percent, });
		}

		private void SDFOversize_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.SdfOversize = LinkedComp.SDFOversize.Value switch { RSDFOversize._120 => GDExtension.Viewport.SDFOversize._120Percent, RSDFOversize._150 => GDExtension.Viewport.SDFOversize._150Percent, RSDFOversize._200 => GDExtension.Viewport.SDFOversize._200Percent, _ => GDExtension.Viewport.SDFOversize._100Percent, });
		}

		private void SnapUIToPixels_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.GuiSnapControlsToPixels = LinkedComp.SnapUIToPixels.Value);
		}

		private void CanvasDefaultTextureRepate_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.CanvasItemDefaultTextureRepeat = LinkedComp.CanvasDefaultTextureRepate.Value switch { RTextureRepeat.Enabled => GDExtension.Viewport.DefaultCanvasItemTextureRepeat.Enabled, RTextureRepeat.Mirror => GDExtension.Viewport.DefaultCanvasItemTextureRepeat.Mirror, _ => GDExtension.Viewport.DefaultCanvasItemTextureRepeat.Disabled, });
		}

		private void CanvasDefaultTextureFilter_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.CanvasItemDefaultTextureFilter = LinkedComp.CanvasDefaultTextureFilter.Value switch { RTextureFilter.Linear => GDExtension.Viewport.DefaultCanvasItemTextureFilter.Linear, RTextureFilter.LinearMipmap => GDExtension.Viewport.DefaultCanvasItemTextureFilter.LinearWithMipmaps, RTextureFilter.NearestMipmap => GDExtension.Viewport.DefaultCanvasItemTextureFilter.NearestWithMipmaps, _ => GDExtension.Viewport.DefaultCanvasItemTextureFilter.Nearest, });
		}

		private void FSRSharpness_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.FsrSharpness = LinkedComp.FSRSharpness.Value);
		}

		private void TextureMipmapBias_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TextureMipmapBias = LinkedComp.TextureMipmapBias.Value);
		}

		private void Scaling3DScale_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Scaling3DScale = LinkedComp.Scaling3DScale.Value);
		}

		private void Scaling3DMode_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Scaling3DModeValue = LinkedComp.Scaling3DMode.Value switch { RScaling3D.FSR => GDExtension.Viewport.Scaling3DMode.Fsr, _ => GDExtension.Viewport.Scaling3DMode.Bilinear, });
		}

		private void DebugDraw_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.DebugDrawValue = LinkedComp.DebugDraw.Value switch { RDebugDraw.Unshaded => GDExtension.Viewport.DebugDraw.Unshaded, RDebugDraw.Lighting => GDExtension.Viewport.DebugDraw.Lighting, RDebugDraw.Overdraw => GDExtension.Viewport.DebugDraw.Overdraw, RDebugDraw.Wireframe => GDExtension.Viewport.DebugDraw.Wireframe, RDebugDraw.NormalBuffer => GDExtension.Viewport.DebugDraw.NormalBuffer, RDebugDraw.VoxelGiAlbedo => GDExtension.Viewport.DebugDraw.VoxelGiAlbedo, RDebugDraw.VoxelGiLighting => GDExtension.Viewport.DebugDraw.VoxelGiLighting, RDebugDraw.VoxelGiEmission => GDExtension.Viewport.DebugDraw.VoxelGiEmission, RDebugDraw.ShadowAtlas => GDExtension.Viewport.DebugDraw.ShadowAtlas, RDebugDraw.DirectionalShadowAtlas => GDExtension.Viewport.DebugDraw.DirectionalShadowAtlas, RDebugDraw.SceneLuminance => GDExtension.Viewport.DebugDraw.SceneLuminance, RDebugDraw.Ssao => GDExtension.Viewport.DebugDraw.Ssao, RDebugDraw.Ssil => GDExtension.Viewport.DebugDraw.Ssil, RDebugDraw.PssmSplits => GDExtension.Viewport.DebugDraw.PssmSplits, RDebugDraw.DecalAtlas => GDExtension.Viewport.DebugDraw.DecalAtlas, RDebugDraw.Sdfgi => GDExtension.Viewport.DebugDraw.Sdfgi, RDebugDraw.SdfgiProbes => GDExtension.Viewport.DebugDraw.SdfgiProbes, RDebugDraw.GiBuffer => GDExtension.Viewport.DebugDraw.GiBuffer, RDebugDraw.DisableLod => GDExtension.Viewport.DebugDraw.DisableLod, RDebugDraw.ClusterOmniLights => GDExtension.Viewport.DebugDraw.ClusterOmniLights, RDebugDraw.ClusterSpotLights => GDExtension.Viewport.DebugDraw.ClusterSpotLights, RDebugDraw.ClusterDecals => GDExtension.Viewport.DebugDraw.ClusterDecals, RDebugDraw.ClusterReflectionProbes => GDExtension.Viewport.DebugDraw.ClusterReflectionProbes, RDebugDraw.Occluders => GDExtension.Viewport.DebugDraw.Occluders, RDebugDraw.MotionVectors => GDExtension.Viewport.DebugDraw.MotionVectors, _ => GDExtension.Viewport.DebugDraw.Disabled, });
		}

		private void UseOcclusionCulling_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.UseOcclusionCulling = LinkedComp.UseOcclusionCulling.Value);
		}

		private bool _isInputUpdate = false;

		private void UpdateMode_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => {
				switch (LinkedComp.UpdateMode.Value) {
					case RUpdateMode.InputUpdate:
						_isInputUpdate = true;
						node.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
						break;
					case RUpdateMode.Always:
						node.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
						break;
					default:
						node.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
						break;
				}
			});
		}

		private void ClearMode_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.RenderTargetClearMode = LinkedComp.ClearMode.Value switch { RClearMode.Always => SubViewport.ClearMode.Always, _ => SubViewport.ClearMode.Never, });
		}

		private void ScreenSpaceAA_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.ScreenSpaceAa = LinkedComp.ScreenSpaceAA.Value switch { RScreenSpaceAA.Fxaa => GDExtension.Viewport.ScreenSpaceAA.Fxaa, _ => GDExtension.Viewport.ScreenSpaceAA.Disabled, });
		}

		private void Msaa3D_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Msaa3D = LinkedComp.Msaa3D.Value switch { RMsaa.TwoX => GDExtension.Viewport.MSAA._2x, RMsaa.FourX => GDExtension.Viewport.MSAA._4x, RMsaa.EightX => GDExtension.Viewport.MSAA._8x, _ => GDExtension.Viewport.MSAA.Disabled, });
		}

		private void Msaa2D_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Msaa2D = LinkedComp.Msaa2D.Value switch { RMsaa.TwoX => GDExtension.Viewport.MSAA._2x, RMsaa.FourX => GDExtension.Viewport.MSAA._4x, RMsaa.EightX => GDExtension.Viewport.MSAA._8x, _ => GDExtension.Viewport.MSAA.Disabled, });
		}

		private void Snap2DVerticesToPixels_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Snap2DVerticesToPixel = LinkedComp.Snap2DVerticesToPixels.Value);
		}

		private void Snap2DTransformsToPixels_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Snap2DTransformsToPixel = LinkedComp.Snap2DTransformsToPixels.Value);
		}

		private void TransparentBG_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.TransparentBg = LinkedComp.TransparentBG.Value);
		}

		private void OwnWorld3D_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.OwnWorld3D = LinkedComp.OwnWorld3D.Value);
		}

		private void Disable3D_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Disable3D = LinkedComp.Disable3D.Value);
		}

		private void UseDebanding_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.UseDebanding = LinkedComp.UseDebanding.Value);
		}

		private void UseTAA_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.UseTaa = LinkedComp.UseTAA.Value);
		}

		private void Size2DOverrideStretch_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Size2DOverrideStretch = LinkedComp.Size2DOverrideStretch.Value);
		}

		private void Size2DOverride_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Size2DOverride = new Vector2i(LinkedComp.Size2DOverride.Value.x, LinkedComp.Size2DOverride.Value.y));
		}

		private void Size_Changed(RhuEngine.WorldObjects.IChangeable obj) {
			RenderThread.ExecuteOnEndOfFrame(() => node.Size = new Vector2i(System.Math.Max(2, LinkedComp.Size.Value.x), System.Math.Max(2, LinkedComp.Size.Value.y)));
		}

		public override void Started() {

		}

		public override void Stopped() {

		}
	}
}

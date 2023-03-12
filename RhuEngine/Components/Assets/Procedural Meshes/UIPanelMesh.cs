using System;
using System.Collections.Generic;
using System.Text;

using RNumerics;
using RhuEngine.WorldObjects.ECS;
using RhuEngine.WorldObjects;

namespace RhuEngine.Components
{
	[Category(new string[] { "Assets/Procedural Meshes" })]
	public sealed partial class UIPanelMesh : ProceduralMesh
	{
		[Default(1f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> Width;

		[Default(1f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> Height;

		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector3f> PosOffset;

		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<bool> Center;

		[Default(0.01f)]
		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<float> Border;


		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2f> UVMax;

		[OnChanged(nameof(LoadMesh))]
		public readonly Sync<Vector2f> UVMin;

		protected override void OnAttach() {
			base.OnAttach();
			UVMax.Value = Vector2f.One;
		}

		public override void ComputeMesh() {
			if (!Engine.EngineLink.CanRender) {
				return;
			}

			var topRight = new Vector3f(Width / 2, Height / 2) + PosOffset.Value;
			var topLeft = new Vector3f(-Width / 2, Height / 2) + PosOffset.Value;
			var bottomRight = new Vector3f(Width / 2, -Height / 2) + PosOffset.Value;
			var bottomLeft = new Vector3f(-Width / 2, -Height / 2) + PosOffset.Value;

			if (!Center) {
				topRight += new Vector3f(Width / 2, Height / 2);
				topLeft += new Vector3f(Width / 2, Height / 2);
				bottomRight += new Vector3f(Width / 2, Height / 2);
				bottomLeft += new Vector3f(Width / 2, Height / 2);
			}

			var mainPanel = new ComplexMesh();
			mainPanel.Vertices.Add(topRight);
			mainPanel.Vertices.Add(topLeft);
			mainPanel.Vertices.Add(bottomRight);
			mainPanel.Vertices.Add(bottomLeft);
			mainPanel.TexCoords = new List<Vector3f>[1] { new List<Vector3f> {
				new Vector3f(UVMin.Value.x,UVMin.Value.y),
				new Vector3f(UVMax.Value.x,UVMin.Value.y),
				new Vector3f(UVMin.Value.x,UVMax.Value.y),
				new Vector3f(UVMax.Value.x,UVMax.Value.y),
			} };
			mainPanel.PrimitiveType = RPrimitiveType.Triangle;
			mainPanel.Faces = new List<RFace> { 
				new RFace(1, 0, 2), 
				new RFace(1, 2, 3),
			};

			var backGroundMesh = new ComplexMesh {
				PrimitiveType = RPrimitiveType.Triangle
			};

			backGroundMesh.Vertices.Add(topRight);// 0
			backGroundMesh.Vertices.Add(topLeft);// 1
			backGroundMesh.Vertices.Add(bottomRight);// 2
			backGroundMesh.Vertices.Add(bottomLeft);// 3

			var topRightOffset = topRight + new Vector3f(Border.Value / 2, Border.Value / 2, -Border.Value / 2);
			var topLeftOffset = topLeft + new Vector3f(-Border.Value / 2, Border.Value / 2, -Border.Value / 2);
			var bottomRightOffset = bottomRight + new Vector3f(Border.Value / 2, -Border.Value / 2, -Border.Value / 2);
			var bottomLeftOffset = bottomLeft + new Vector3f(-Border.Value / 2, -Border.Value / 2, -Border.Value / 2);

			backGroundMesh.Vertices.Add(topRightOffset);// 4
			backGroundMesh.Vertices.Add(topLeftOffset);// 5
			backGroundMesh.Vertices.Add(bottomRightOffset);// 6
			backGroundMesh.Vertices.Add(bottomLeftOffset);// 7

			var topRightOffsetOffset = topRightOffset + new Vector3f(Border.Value / 2, Border.Value / 2, Border.Value / 2);
			var topLeftOffsetOffset = topLeftOffset + new Vector3f(-Border.Value / 2, Border.Value / 2, Border.Value / 2);
			var bottomRightOffsetOffset = bottomRightOffset + new Vector3f(Border.Value / 2, -Border.Value / 2, Border.Value / 2);
			var bottomLeftOffsetOffset = bottomLeftOffset + new Vector3f(-Border.Value / 2, -Border.Value / 2, Border.Value / 2);

			backGroundMesh.Vertices.Add(topRightOffsetOffset);// 8
			backGroundMesh.Vertices.Add(topLeftOffsetOffset);// 9
			backGroundMesh.Vertices.Add(bottomRightOffsetOffset);// 10
			backGroundMesh.Vertices.Add(bottomLeftOffsetOffset);// 11

			var topRightBack = topRight + new Vector3f(Border.Value / 2, Border.Value / 2, Border.Value / 2);
			var topLeftBack = topLeft + new Vector3f(-Border.Value / 2, Border.Value / 2, Border.Value / 2);
			var bottomRightBack = bottomRight + new Vector3f(Border.Value / 2, -Border.Value / 2, Border.Value / 2);
			var bottomLeftBack = bottomLeft + new Vector3f(-Border.Value / 2, -Border.Value / 2, Border.Value / 2);

			backGroundMesh.Vertices.Add(topRightBack);// 12
			backGroundMesh.Vertices.Add(topLeftBack);// 13
			backGroundMesh.Vertices.Add(bottomRightBack);// 14
			backGroundMesh.Vertices.Add(bottomLeftBack);// 15

			backGroundMesh.Faces = new List<RFace> {
				//TOP
				new RFace(5, 4, 0),
				new RFace(1, 5, 0),

				new RFace(4, 5, 8),
				new RFace(5, 9, 8),

				new RFace(8, 9, 12),
				new RFace(9, 13, 12),

				//Left
				new RFace(3, 5, 1),
				new RFace(7, 5, 3),

				new RFace(5, 7, 9),
				new RFace(11, 9, 7),

				new RFace(9, 11, 13),
				new RFace(15, 13, 11),

				//Bottom
				new RFace(3, 2, 6),
				new RFace(7, 3, 6),

				new RFace(7, 6, 10),
				new RFace(11, 7, 10),

				new RFace(11, 10, 14),
				new RFace(15, 11, 14),

				//Right
                new RFace(2, 0, 6),
				new RFace(6, 0, 4),

				new RFace(6, 4, 8),
				new RFace(6, 8, 10),

				new RFace(10, 8, 12),
				new RFace(10, 12, 14),

				//BackGround
				new RFace(12, 13, 14),
				new RFace(13, 15, 14),
			};

			mainPanel.AddSubMesh(backGroundMesh);
			GenMesh(mainPanel);
		}
	}
}

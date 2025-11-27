#region Copyright & License Information
/*
 * Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Mods.Common.Graphics
{
	public class RangeCircleRenderable : IRenderable, IFinalizedRenderable
	{
		const int RangeCircleSegments = 32;
		static readonly Int32Matrix4x4[] RangeCircleStartRotations = Exts.MakeArray(RangeCircleSegments, i => WRot.FromFacing(8 * i).AsMatrix());
		static readonly Int32Matrix4x4[] RangeCircleEndRotations = Exts.MakeArray(RangeCircleSegments, i => WRot.FromFacing(8 * i + 6).AsMatrix());

		readonly WDist radius;
		readonly Color color;
		readonly Color contrastColor;

		public RangeCircleRenderable(WPos centerPosition, WDist radius, int zOffset, Color color, Color contrastColor)
		{
			Pos = centerPosition;
			ZOffset = zOffset;
			this.radius = radius;
			this.color = color;
			this.contrastColor = contrastColor;
		}

		public WPos Pos { get; }
		public int ZOffset { get; }
		public bool IsDecoration => true;

		public IRenderable WithZOffset(int newOffset) { return new RangeCircleRenderable(Pos, radius, newOffset, color, contrastColor); }
		public IRenderable OffsetBy(in WVec vec) { return new RangeCircleRenderable(Pos + vec, radius, ZOffset, color, contrastColor); }
		public IRenderable AsDecoration() { return this; }

		public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }
		public void Render(WorldRenderer wr)
		{
			DrawRangeCircle(wr, Pos, radius, 1, color, 3, contrastColor);
		}

		public static void DrawRangeCircle(WorldRenderer wr, WPos centerPosition, WDist radius,
			float width, Color color, float contrastWidth, Color contrastColor)
		{
			var wcr = Game.Renderer.WorldRgbaColorRenderer;
			var offset = new WVec(radius.Length, 0, 0);
			for (var i = 0; i < RangeCircleSegments; i++)
			{
				var a = wr.Screen3DPosition(centerPosition + offset.Rotate(ref RangeCircleStartRotations[i]));
				var b = wr.Screen3DPosition(centerPosition + offset.Rotate(ref RangeCircleEndRotations[i]));

				if (contrastWidth > 0)
					wcr.DrawLine(a, b, contrastWidth / wr.Viewport.Zoom, contrastColor);

				if (width > 0)
					wcr.DrawLine(a, b, width / wr.Viewport.Zoom, color);
			}
		}

		public void RenderDebugGeometry(WorldRenderer wr) { }
		public Rectangle ScreenBounds(WorldRenderer wr) { return Rectangle.Empty; }
	}
}

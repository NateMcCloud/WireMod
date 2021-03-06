﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using WireMod.Devices;

namespace WireMod.UI
{
	public class ElectronicsVisionUI : UIState
	{
		public static bool Visible { get; set; }

		public const int Thickness = 4;

		public const float DeviceVisibility = 1f;
		public const float WireVisibility = 0.5f;

		public static Rectangle WireRect = new Rectangle(0, 0, 1, Thickness);


		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			DrawDevices(spriteBatch);
			DrawWires(spriteBatch);
		}

		private static void DrawDevices(SpriteBatch spriteBatch)
		{
			var screenRect = Helpers.GetScreenRect();

			foreach (var device in WireMod.Devices)
			{
				if (!device.LocationWorldRect.Intersects(screenRect)) continue;
				
				var pos = (device.LocationWorld - Main.screenPosition) - Helpers.Drift;

				var texture = WireMod.Instance.GetTexture($"Images/{device.GetType().Name}");
				spriteBatch.Draw(texture, pos, device.GetSourceRect(), Color.White * DeviceVisibility, 0f, new Vector2(0, 0), /*1 + ((Main.UIScale - 1) / 2)*/ 1f, SpriteEffects.None, 0f);
				device.Draw(spriteBatch);
			}
		}

		private static void DrawWires(SpriteBatch spriteBatch)
		{
			var screenRect = Helpers.GetScreenRect();
			var modPlayer = Main.LocalPlayer.GetModPlayer<WireModPlayer>();

			foreach (var pin in WireMod.Pins)
			{
				if (pin.Type != "Out") continue;
				if (!pin.IsConnected()) continue;
				
				foreach (var p in ((PinOut)pin).ConnectedPins)
				{
					if (!screenRect.Contains(pin.Location.ToWorldCoordinates().ToPoint()) && !screenRect.Contains(p.Location.ToWorldCoordinates().ToPoint())) continue;

					var pinWire = pin.Wires.FirstOrDefault(w => w.StartPin == p || w.EndPin == p);
					if (pinWire == null) continue;

					var vis = WireVisibility;
					if (HoverDebuggerUI.Visible)
					{
						var hoverDevice = ((HoverDebuggerUI)WireMod.Instance.DebuggerHoverUserInterface.CurrentState).Device;
						if (pin.Device == hoverDevice || p.Device == hoverDevice) vis = 1f;
					}
					
					DrawWire(spriteBatch, pinWire, vis);
				}
			}

			// Draw trailing wire to mouse
			if (modPlayer.ConnectingPin == null) return;

			var placingPoints = modPlayer.PlacingWire.GetPoints();

			if (placingPoints.Count > 1)
			{
				for (var i = 0; i < placingPoints.Count - 1; i++)
				{
					DrawLine(
						spriteBatch,
						placingPoints[i].ToWorldCoordinates() - Main.screenPosition - Helpers.Drift,
						placingPoints[i + 1].ToWorldCoordinates() - Main.screenPosition - Helpers.Drift,
						GetWireColor(modPlayer.ConnectingPin)
					);
				}
			}

			DrawLine(
				spriteBatch,
				placingPoints.Last().ToWorldCoordinates() - Main.screenPosition - Helpers.Drift,
				Main.MouseScreen,
				GetWireColor(modPlayer.ConnectingPin)
			);
		}

		private static void DrawWire(SpriteBatch spriteBatch, Wire wire, float visibility = WireVisibility)
		{
			var points = wire.GetPoints();

			for (var i = 0; i < points.Count - 1; i++)
			{
				var start = points[i].ToWorldCoordinates() - Main.screenPosition - Helpers.Drift;
				var end = points[i + 1].ToWorldCoordinates() - Main.screenPosition - Helpers.Drift;

				if (start.X < end.X)
				{
					start.X -= (Thickness);
					start.Y -= (Thickness / 2);
					end.Y -= (Thickness / 2);
				}
				else if (start.X > end.X)
				{
					start.Y += (Thickness / 2);
					end.X -= (Thickness);
					end.Y += (Thickness / 2);
				}

				if (start.Y < end.Y)
				{
					start.Y -= (Thickness / 2);
					end.Y += (Thickness / 2);
				}
				else if (start.Y > end.Y)
				{
					start.X -= (Thickness);
					start.Y += (Thickness / 2);
					end.X -= (Thickness);
					end.Y -= (Thickness / 2);
				}

				start.X += (Thickness / 2);
				end.X += (Thickness / 2);

				DrawLine(spriteBatch, start, end, GetWireColor(wire.StartPin.Type == "Out" ? wire.StartPin : wire.EndPin) * visibility);
			}
		}

		private static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color)
		{
			var edge = end - start;

			WireRect.Width = (int)edge.Length();
			WireRect.X = (int)start.X;
			WireRect.Y = (int)start.Y;

			spriteBatch.Draw(Main.magicPixel, WireRect, null, color, (float)Math.Atan2(edge.Y, edge.X), Vector2.Zero, SpriteEffects.None, 1f);
		}

		private static Color GetWireColor(Pin pin)
		{
			switch (pin.DataType)
			{
				case "bool":
					if (pin.GetValue() == "0") return Color.Red;
					if (pin.GetValue() == "1") return Color.Green;
					break;
				case "int":
					return Color.Yellow;
				case "string":
					return Color.Blue;
			}

			return Color.White;
		}
	}
}

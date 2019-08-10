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

		public static float DeviceVisibility { get; set; } = 1f;
		public static float WireVisibility { get; set; } = 0.75f;

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			DrawDevices(spriteBatch);
			DrawWires(spriteBatch);
		}

		private static void DrawDevices(SpriteBatch spriteBatch)
		{
			var pixels = 16/* * Main.UIScale*/;

			var screenRect = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);

			foreach (var device in WireMod.Devices)
			{
				if (device.LocationRect == default(Rectangle)) continue;

				var deviceWorldRect = new Rectangle((int)(device.LocationRect.X * pixels), (int)(device.LocationRect.Y * pixels), (int)(device.Width * pixels), (int)(device.Height * pixels));
				if (!deviceWorldRect.Intersects(screenRect)) continue;

				var deviceScreenRect = new Rectangle(deviceWorldRect.X - screenRect.X, deviceWorldRect.Y - screenRect.Y, deviceWorldRect.Width, deviceWorldRect.Height);

				var texture = WireMod.Instance.GetTexture($"Images/{device.GetType().Name}");
				spriteBatch.Draw(texture, new Vector2(deviceScreenRect.X, deviceScreenRect.Y), device.GetSourceRect(), Color.White * DeviceVisibility, 0f, new Vector2(0, 0), Main.UIScale, SpriteEffects.None, 0f);
				device.Draw(spriteBatch);
			}
		}

		private static void DrawWires(SpriteBatch spriteBatch)
		{
			var screenRect = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);

			foreach (var pin in WireMod.Pins)
			{
				if (pin.Type != "Out") continue;
				if (!pin.IsConnected()) continue;

				var pinRect = new Rectangle((int)pin.Location.ToWorldCoordinates(0, 0).X, (int)pin.Location.ToWorldCoordinates(0, 0).Y, 16, 16);
				if (!screenRect.Intersects(pinRect)) continue;

				foreach (var p in ((PinOut)pin).ConnectedPins)
				{
					var pinWire = pin.Wires.FirstOrDefault(w => w.StartPin == p || w.EndPin == p);

					if (pinWire == null) continue;

					var points = pinWire.GetPoints(pin);

					for (var i = 0; i < points.Count - 1; i++)
					{
						DrawLine(
							spriteBatch,
							points[i].ToWorldCoordinates() - screenRect.Location.ToVector2(),
							points[i + 1].ToWorldCoordinates() - screenRect.Location.ToVector2(),
							GetWireColor(pin)
						);
					}

					DrawWireDot(spriteBatch, pinWire.StartPin.Location.ToWorldCoordinates() - screenRect.Location.ToVector2());
					DrawWireDot(spriteBatch, pinWire.EndPin.Location.ToWorldCoordinates() - screenRect.Location.ToVector2());
				}
			}

			// Draw trailing wire to mouse
			var modPlayer = Main.LocalPlayer.GetModPlayer<WireModPlayer>();
			if (modPlayer.ConnectingPin == null) return;

			var placingPoints = modPlayer.PlacingWire.GetPoints();

			if (placingPoints.Count > 1)
			{
				for (var i = 0; i < placingPoints.Count - 1; i++)
				{
					DrawLine(
						spriteBatch,
						placingPoints[i].ToWorldCoordinates() - screenRect.Location.ToVector2(),
						placingPoints[i + 1].ToWorldCoordinates() - screenRect.Location.ToVector2(),
						GetWireColor(modPlayer.ConnectingPin)
					);
				}
			}

			DrawLine(
				spriteBatch,
				placingPoints.Last().ToWorldCoordinates() - screenRect.Location.ToVector2(),
				Main.MouseScreen,
				GetWireColor(modPlayer.ConnectingPin)
			);

			DrawWireDot(spriteBatch, modPlayer.ConnectingPin.Location.ToWorldCoordinates() - screenRect.Location.ToVector2());
		}

		private static void DrawWireDot(SpriteBatch spriteBatch, Vector2 position, int size = 4)
		{
			spriteBatch.Draw(Helpers.CreateRectangle(size, size), position - new Vector2(size / 2, size / 2), Color.White * 0.5f * WireVisibility);
			//spriteBatch.Draw(Helpers.CreateCircle(size), position - new Vector2(size / 2, size / 2), Color.White * 0.5f * WireVisibility);
		}

		private static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness = 4)
		{

			if (start.X < end.X)
			{
				start.X -= (thickness);
				start.Y -= (thickness / 2);
				end.Y -= (thickness / 2);
			}
			else if (start.X > end.X)
			{
				start.Y += (thickness / 2);
				end.X -= (thickness);
				end.Y += (thickness / 2);
			}

			if (start.Y < end.Y)
			{
				start.Y -= (thickness / 2);
				end.Y += (thickness / 2);
			}
			else if (start.Y > end.Y)
			{
				start.X -= (thickness);
				start.Y += (thickness / 2);
				end.X -= (thickness);
				end.Y -= (thickness / 2);
			}

			var edge = end - start;
			var angle = (float)Math.Atan2(edge.Y, edge.X);

			var line = new Rectangle((int)start.X + (thickness / 2), (int)start.Y, (int)edge.Length(), thickness);
			spriteBatch.Draw(Main.magicPixel, line, null, color * WireVisibility, angle, new Vector2(0, 0), SpriteEffects.None, 1f);
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

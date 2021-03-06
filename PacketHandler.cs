﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WireMod.Devices;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace WireMod
{
	internal class DevicePacketHandler
	{
		public const byte Place = 1;
		public const byte Remove = 2;
		public const byte Connect = 3;
		public const byte Disconnect = 4;
		public const byte ChangeSetting = 5;
		public const byte TripWire = 6;
		public const byte Request = 7;


		protected ModPacket GetPacket(byte packetType, int from)
		{
			var packet = WireMod.Instance.GetPacket();
			packet.Write(packetType);
			return packet;
		}

		public void HandlePacket(BinaryReader reader, int from)
		{
			switch (reader.ReadByte())
			{
				case Place:
					ReceivePlace(reader, from);
					break;
				case Remove:
					ReceiveRemove(reader, from);
					break;
				case Connect:
					ReceiveConnect(reader, from);
					break;
				case Disconnect:
					ReceiveDisconnect(reader, from);
					break;
				case ChangeSetting:
					ReceiveChangeSetting(reader, from);
					break;
				case TripWire:
					ReceiveTripWire(reader, from);
					break;
				case Request:
					ReceiveRequest(reader, from);
					break;
			}
		}

		#region Place
		public void SendPlace(int to, int from, string name, Dictionary<string, string> settings, int x, int y)
		{
			var packet = GetPacket(Place, from);

			packet.Write(name);
			packet.Write(x);
			packet.Write(y);
			packet.Write(settings.Count);

			foreach (var setting in settings)
			{
				packet.Write(setting.Key);
				packet.Write(setting.Value);
			}

			packet.Send(to, from);
		}

		public void ReceivePlace(BinaryReader reader, int from)
		{
			var name = reader.ReadString();
			var x = reader.ReadInt32();
			var y = reader.ReadInt32();

			if (WireMod.Debug) WireMod.Instance.Logger.Info($"{from} Received Place: name {name}, x {x}, y {y}");

			var device = (Device)Activator.CreateInstance(Type.GetType("WireMod.Devices." + name) ?? throw new InvalidOperationException("Device not found!"));

			if (!WireMod.CanPlace(device, x, y))
			{
				if (WireMod.Debug) WireMod.Instance.Logger.Error($"{from} Place: Cannot place device at: x {x}, y {y}");
			}

			var settingCount = reader.ReadInt32();
			var settings = new Dictionary<string, string>();
			for (var i = 0; i < settingCount; i++)
			{
				var key = reader.ReadString();
				var value = reader.ReadString();

				settings.Add(key, value);
			}

			if (Main.netMode == NetmodeID.Server)
			{
				SendPlace(-1, from, name, settings, x, y);
			}

			device.SetLocation(x - device.Origin.X, y - device.Origin.Y);
			device.Settings = settings;
			WireMod.PlaceDevice(device, x, y);
		}
		#endregion

		#region Request
		public void SendRequest(int to, int from)
		{
			var packet = GetPacket(Request, from);

			packet.Write(from);

			packet.Send(to, from);
		}

		public void ReceiveRequest(BinaryReader reader, int from)
		{
			from = reader.ReadInt32();

			if (WireMod.Debug) WireMod.Instance.Logger.Info($"{from} Received Request from {Main.player[from].name}");

			if (Main.netMode != NetmodeID.Server) return;

			foreach (var device in WireMod.Devices)
			{
				this.SendPlace(from, 256, device.GetType().Name, device.Settings, device.LocationRect.X + device.Origin.X, device.LocationRect.Y + device.Origin.Y);
			}

			var wires = WireMod.Pins.Where(p => p.Type == "In" && p.IsConnected()).Select(p => new
			{
				src = p.Location,
				dest = ((PinIn)p).ConnectedPin.Location,
				wire = p.GetWire(((PinIn)p).ConnectedPin)
			}).ToList();

			foreach (var conn in wires)
			{
				this.SendConnect(from, 256, conn.src.X, conn.src.Y, conn.dest.X, conn.dest.Y, conn.wire);
			}
		}
		#endregion

		#region Remove
		public void SendRemove(int to, int from, int x, int y)
		{
			var packet = GetPacket(Remove, from);
			packet.Write(x);
			packet.Write(y);
			packet.Send(to, from);
		}

		public void ReceiveRemove(BinaryReader reader, int from)
		{
			var x = reader.ReadInt32();
			var y = reader.ReadInt32();

			if (WireMod.Debug) WireMod.Instance.Logger.Info($"{from} Received Remove: x {x}, y {y}");

			if (!WireMod.Devices.Any(d => d.LocationRect.Intersects(new Rectangle(x, y, 1, 1))))
			{
				if (WireMod.Debug) WireMod.Instance.Logger.Error($"{from} Remove: No device found at: x {x}, y {y}");
			}

			if (Main.netMode == NetmodeID.Server)
			{
				SendRemove(-1, from, x, y);
			}

			WireMod.RemoveDevice(x, y);
		}
		#endregion

		#region Connect
		public void SendConnect(int to, int from, int srcX, int srcY, int destX, int destY, Wire wire)
		{
			var packet = GetPacket(Connect, from);

			packet.Write(srcX);
			packet.Write(srcY);
			packet.Write(destX);
			packet.Write(destY);

			packet.Write(wire.Points.Count);
			foreach (var point in wire.Points)
			{
				packet.Write((int)point.X);
				packet.Write((int)point.Y);
			}

			packet.Send(to, from);
		}

		public void ReceiveConnect(BinaryReader reader, int from)
		{
			var srcX = reader.ReadInt32();
			var srcY = reader.ReadInt32();
			var destX = reader.ReadInt32();
			var destY = reader.ReadInt32();

			var pointsCount = reader.ReadInt32();
			var points = new List<Point16>();
			if (pointsCount > 0)
			{
				for (var i = 0; i < pointsCount; i++)
				{
					var x = reader.ReadInt32();
					var y = reader.ReadInt32();

					points.Add(new Point16(x, y));
				}
			}

			if (WireMod.Debug) WireMod.Instance.Logger.Info($"{from} Received Connect: srcX {srcX}, srcY {srcY}, destX {destX}, destY {destY}");

			var src = WireMod.GetDevicePin(srcX, srcY);
			if (src == null)
			{
				if (WireMod.Debug) WireMod.Instance.Logger.Error($"{from} Connect: No pin found at: x {srcX}, y {srcY}");
				return;
			}

			var dest = WireMod.GetDevicePin(destX, destY);
			if (dest == null)
			{
				if (WireMod.Debug) WireMod.Instance.Logger.Error($"{from} Connect: No pin found at: x {srcX}, y {srcY}");
				return;
			}
			
			var wire = new Wire(src, dest, points);

			if (Main.netMode == NetmodeID.Server)
			{
				this.SendConnect(-1, from, srcX, srcY, destX, destY, wire);
			}

			src.Connect(dest, wire);
			dest.Connect(src, wire);
		}
		#endregion

		#region Disconnect
		public void SendDisconnect(int to, int from, int x, int y)
		{
			var packet = GetPacket(Disconnect, from);

			packet.Write(x);
			packet.Write(y);

			packet.Send(to, from);
		}

		public void ReceiveDisconnect(BinaryReader reader, int from)
		{
			var x = reader.ReadInt32();
			var y = reader.ReadInt32();

			if (WireMod.Debug) WireMod.Instance.Logger.Info($"{from} Received Disconnect: x {x}, y {y}");

			var src = WireMod.GetDevicePin(x, y);
			if (src == null)
			{
				if (WireMod.Debug) WireMod.Instance.Logger.Error($"{from} Disconnect: No pin found at: x {x}, y {y}");
				return;
			}

			if (Main.netMode == NetmodeID.Server)
			{
				this.SendDisconnect(-1, from, x, y);
			}

			src.Disconnect();
		}
		#endregion

		#region Change Setting
		public void SendChangeSetting(int to, int from, int x, int y, string key, string value)
		{
			var packet = GetPacket(ChangeSetting, from);
			packet.Write(x);
			packet.Write(y);
			packet.Write(key);
			packet.Write(value);
			packet.Send(to, from);
		}

		public void ReceiveChangeSetting(BinaryReader reader, int from)
		{
			var x = reader.ReadInt32();
			var y = reader.ReadInt32();
			var key = reader.ReadString();
			var value = reader.ReadString();

			if (WireMod.Debug) WireMod.Instance.Logger.Info($"{from} Received ChangeValue: x {x}, y {y}, value {value}");

			if (Main.netMode == NetmodeID.Server)
			{
				this.SendChangeSetting(-1, from, x, y, key, value);
			}

			var device = WireMod.GetDevice(x, y);
			if (device == null)
			{
				if (WireMod.Debug) WireMod.Instance.Logger.Error($"{from} ChangeValue: No device found at: x {x}, y {y}");
				return;
			}

			device.Settings[key] = value;
		}
		#endregion

		#region TripWire
		public void SendTripWire(int to, int from, int x, int y)
		{
			var packet = GetPacket(TripWire, from);

			packet.Write(x);
			packet.Write(y);

			packet.Send(to, from);
		}

		public void ReceiveTripWire(BinaryReader reader, int from)
		{
			// TODO: Find out what happens if player 0 leaves
			if (from != 0) return;

			var x = reader.ReadInt32();
			var y = reader.ReadInt32();

			if (WireMod.Debug) WireMod.Instance.Logger.Info($"{from} Received TripWire: x {x}, y {y}");

			Wiring.TripWire(x, y, 1, 1);
		}
		#endregion
	}
}
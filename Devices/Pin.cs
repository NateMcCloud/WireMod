﻿using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace WireMod.Devices
{
    public abstract class Pin
    {
        public Device Device { get; set; }
        public int Num { get; set; }
        public string Type { get; set; }
        public string DataType { get; set; } = "bool";

        public Point16 Location { get; set; }

        public List<Wire> Wires { get; set; } = new List<Wire>();

        private string _name = "";
        public string Name
        {
            get => !string.IsNullOrEmpty(this._name) ? this._name : $"Pin{this.Type}{this.Num}";
            set => this._name = value;
        }

        public abstract bool IsConnected(Pin pin = null);

        public abstract void Connect(Pin pin, Wire wire);
        public abstract void Disconnect(Pin pin = null);

        public abstract string GetValue();
        public abstract List<string> GetDebug();

        public Wire GetWire(Pin connectedPin) => this.Wires.FirstOrDefault(w => w.StartPin == this && w.EndPin == connectedPin || w.StartPin == connectedPin && w.EndPin == this);
    }

    internal class PinIn : Pin
    {
        public Pin ConnectedPin { get; set; }
        private string _value => this.ConnectedPin?.GetValue();
        
        public PinIn(Device device)
        {
            this.Device = device;
            this.Type = "In";
        }

        public override bool IsConnected(Pin pin = null) => this.ConnectedPin != null;

        public override void Connect(Pin pin, Wire wire)
        {
            if (this.IsConnected()) this.ConnectedPin.Disconnect(this);
            this.ConnectedPin = pin;
            this.Wires.Add(wire);
        }

        public override void Disconnect(Pin pin = null)
        {
            if (!this.IsConnected()) return;

            this.Wires.RemoveAll(w => w.StartPin == this && w.EndPin == this.ConnectedPin);
            this.Wires.RemoveAll(w => w.StartPin == this.ConnectedPin && w.EndPin == this);

            this.ConnectedPin.Disconnect(this);
            this.ConnectedPin = null;
        }

        public override string GetValue() => this._value;

        public override List<string> GetDebug()
        {
            return this.IsConnected()
                ? new List<string> { $"{this.Name} [{(this.DataType == "bool" ? (this.GetValue() == "1" ? "True" : (this.GetValue() == "0" ? "False" : "Disconnected")) : this.GetValue())}] => {this.ConnectedPin.Device.Name} {this.ConnectedPin.Name}" }
                : new List<string> { $"{this.Name} (Disconnected)" };
        }
    }

    internal class PinOut : Pin
    {
        public List<Pin> ConnectedPins { get; set; } = new List<Pin>();
        private string _value => (this.Device is IOutput ? ((IOutput)this.Device).Output(this) : "");

        public PinOut(Device device)
        {
            this.Device = device;
            this.Type = "Out";
        }

        public override bool IsConnected(Pin pin = null)
        {
            if (pin == null)
            {
                return this.ConnectedPins.Count > 0;
            }

            return this.ConnectedPins.Contains(pin);
        }

        public override void Connect(Pin pin, Wire wire)
        {
            this.ConnectedPins.Add(pin);
            this.Wires.Add(wire);
        }

        public override void Disconnect(Pin pin = null)
        {
            if (!this.IsConnected()) return;

            if (pin != null)
            {
                if (this.ConnectedPins.Contains(pin)) this.ConnectedPins.Remove(pin);
                return;
            }

            var pins = this.ConnectedPins.Where(p => p.IsConnected()).ToList();
            foreach (var p in pins) p.Disconnect();
      
            this.ConnectedPins.ForEach(p =>
            {
                this.Wires.RemoveAll(w => w.StartPin == this && w.EndPin == p);
                this.Wires.RemoveAll(w => w.StartPin == p && w.EndPin == this);
            });
        }

        public override string GetValue() => this._value;

        public override List<string> GetDebug()
        {
            var lines = new List<string>();

            if (this.IsConnected())
            {
                foreach (var pin in this.ConnectedPins)
                {
                    lines.Add($"{this.Name} [{(this.DataType == "bool" ? (this.GetValue() == "1" ? "True" : (this.GetValue() == "0" ? "False" : "Disconnected")) : this.GetValue())}] => {pin.Device.Name} {pin.Name}");
                }
            }
            else
            {
                lines.Add($"{this.Name} (Disconnected) [{(this.DataType == "bool" ? (this.GetValue() == "1" ? "True" : (this.GetValue() == "0" ? "False" : "Disconnected")) : this.GetValue())}]");
            }
            
            return lines;
        }
    }
}

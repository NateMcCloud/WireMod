﻿using System.Collections.Generic;
using Terraria.DataStructures;

namespace WireMod.Devices
{
    internal class LessThan : Device, IOutput
    {
        public LessThan()
        {
            this.Name = "Less Than";
            this.Width = 3;
            this.Height = 2;
            this.Origin = new Point16(1, 0);

            this.PinLayout = new List<PinDesign>
            {
                new PinDesign("In", 0, new Point16(0, 0), "int"),
                new PinDesign("In", 1, new Point16(2, 0), "int"),
                new PinDesign("Out", 0, new Point16(1, 1), "bool"),
            };
        }

        public string Output(Pin pin = null) => this.GetOutput().ToString();

        private int GetOutput()
        {
            if (!this.GetPinIn(0).IsConnected() || !this.GetPinIn(1).IsConnected()) return -1;
            if (!int.TryParse(this.GetPinIn(0).GetValue(), out var in0)) return -1;
            if (!int.TryParse(this.GetPinIn(1).GetValue(), out var in1)) return -1;
            return in0 < in1 ? 1 : 0;
        }
    }
}

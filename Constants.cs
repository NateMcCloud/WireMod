﻿using System.Collections.Generic;

namespace WireMod
{
    public static class Constants
    {
        public static readonly List<string> ToolCategories = new List<string>
        {
            "Tools",
            "Logic Gates",
            "Maths",
            "Comparison",
            "Basic Inputs",
            //hlw Addition
            "Variables",
            //end Addition
            "Sensor Inputs",
            "Text",
            "Outputs",
            "Experimental",
        };

        public static readonly List<List<string>> Tools = new List<List<string>>
        {
            new List<string> // Tools
            {
                "None",
                "Wiring",
                "Delete",
                "Debug",
            },
            new List<string> // Logic Gates
            {
                "AndGate",
                "OrGate",
                "NotGate",
                "If",
            },
            new List<string> // Maths
            {
                "Add",
                "Subtract",
                "Multiply",
                "Divide",
                "Modulo",
            },
            new List<string> // Comparison
            {
                "Equals",
                "LessThan",
                "GreaterThan",
            },
            new List<string> // Basic Inputs
            {
                "BooleanConstant",
                "IntegerConstant",
                "StringConstant",
                "TeamColorConstant",
                "PointConstant",
                "RandomInt",
                "AreaInput",
            },
            new List<string>// Variables (hlw Addition)
            {
                "IntegerVariable"
            },
            new List<string> // Sensor Inputs
            {
                "TimeSensor",
                "PlayerDistanceSensor",
                "NPCDistanceSensor",
                "PlayerCounter",
                "NPCCounter",
                "TileCounter",
            },
            new List<string> // Text
            {
                "Concat",
                "TextTileController",
            },
            new List<string> // Outputs
            {
                "OutputLamp",
                //"OutputSign",
                "FlipFlop",
                "Trigger",
            },
            new List<string> // Experimental
            {
                "Repulsor",
                "Spawner",
                "Buffer",
            },
        };

        public static readonly Dictionary<string, string> ToolNames = new Dictionary<string, string>
        {
            { "None", "None" },
            { "Wiring", "Wiring" },
            { "Delete", "Remove Device" },
            { "Debug", "Debug Device" },
            { "AndGate", "AND Logic Gate" },
            { "OrGate", "OR Logic Gate" },
            { "NotGate", "NOT Logic Gate" },
            { "If", "Conditional Logic Gate" },
            { "Add", "Add Operation" },
            { "Subtract", "Subtract Operation" },
            { "Multiply", "Multiply Operation" },
            { "Divide", "Divide Operation" },
            { "Modulo", "Modulo Operation" },
            { "Equals", "Equals (=) Comparer" },
            { "LessThan", "Less Than (<) Comparer" },
            { "GreaterThan", "Greater Than (>) Comparer" },
            { "BooleanConstant", "Constant Boolean Value" },
            { "IntegerConstant", "Constant Number Value" },
            { "StringConstant", "Constant String Value" },
            { "TeamColorConstant", "Constant Team Color Value" },
            { "PointConstant", "Constant Point Value" },
            { "RandomInt", "Random Integer Value" },
            { "AreaInput", "Area Value" },
            { "Concat", "Concatenate Strings" },
            { "TextTileController", "Text Tile Controller" },
            { "FlipFlop", "Flip/Flop" },
            { "OutputLamp", "Output Lamp" },
            //{ "OutputSign", "Output Sign" },
            { "Trigger", "Trigger Output" },
            { "PlayerDistanceSensor", "Nearest Player Distance Sensor" },
            { "NPCDistanceSensor", "Nearest NPC Distance Sensor" },
            { "TimeSensor", "Time Sensor - basically a glorified clock" },
            { "PlayerCounter", "Count nearby Players" },
            { "NPCCounter", "Count nearby NPCs" },
            { "TileCounter", "Count nearby tiles" },
            { "Repulsor", "Repulsor (USE WITH CAUTION)" },
            { "Spawner", "NPC Spawner (USE WITH CAUTION)" },
            { "Buffer", "Buffer (USE WITH CAUTION)" },
            {"IntegerVariable", "Store number value (hlw addition)" },
        };
    }
}
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // =======================================================================================
        //                                 --- CONFIGURATION ---
        // ======================================================================================= 

        // Name of the group with the printer's pistons.
        string pistonGroupName = "3D Pistons";

        // Name of the group with the printer's welders.
        string welderGroupName = "3D Welders";

        // Speed at which the pistons should retract during printing. Should always be negative. Remember to leave the 'f' at the end of the number!
        float speedPrinting = -0.0075f;

        // Speed at which the pistons should retract when it's not printing. Should always be negative.
        float speedRetract = -1;

        // Speed at which the pistons should extend to prepare for printing. Should always be positive.
        float speedExtend = 1;

        // Warning lights.
        // Example: string[] warningLightGroups = { "3D Warning Lights Top Row", "3D Warning Lights Spinning"};
        bool useWarningLights = false;
        string[] warningLightGroups = { };

        // =======================================================================================
        //                             --- END OF CONFIGURATION ---
        // ======================================================================================= 

        // List Declaration
        List<IMyTerminalBlock> pistonList = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> welderList = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> warningLightList = new List<IMyTerminalBlock>();

        public Program()
        {
            // Makes the script run every 10 in-game ticks
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            ParseGroups();
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
            // Should probably do something with this...
        }

        public void ParseGroups()
        {
            string ERROR_TEXT = "";

            // Pistons
            if (GridTerminalSystem.GetBlockGroupWithName(pistonGroupName) != null)
            {
                GridTerminalSystem.GetBlockGroupWithName(pistonGroupName).GetBlocksOfType<IMyPistonBase>(pistonList, FilterThis);
                if (pistonList.Count == 0)
                {
                    ERROR_TEXT += "Group \"" + pistonGroupName + "\" has no piston blocks!\n";
                }
            }
            else
            {
                ERROR_TEXT += "Group \"" + pistonGroupName + "\" not found!\n";
            }

            // Welders
            if (GridTerminalSystem.GetBlockGroupWithName(welderGroupName) != null)
            {
                GridTerminalSystem.GetBlockGroupWithName(welderGroupName).GetBlocksOfType<IMyShipWelder>(welderList);
                if (welderList.Count == 0)
                {
                    ERROR_TEXT += "Group \"" + welderGroupName + "\" has no welder blocks!\n";
                }
            }
            else
            {
                ERROR_TEXT += "Group \"" + welderGroupName + "\" not found!\n";
            }

            // Shows the errors
            if (ERROR_TEXT != "")
            {
                Echo("Script errors found:\n" + ERROR_TEXT + "Please check the ownership settings of your blocks!");
                return;
            }
            else
            {
                Echo("");
            }

            Echo("Script was compiled at " + DateTime.Now.ToString("HH:mm:ss"));
            //Echo(pistonList.Count.ToString() + " pistons found");
            //foreach (var block in pistonList)
            //{
            //    Echo($" - {block.CustomName}");
            //}

            // Welder Group
            //IMyBlockGroup welderGroup = GridTerminalSystem.GetBlockGroupWithName(welderGroupName);
            //if (welderGroup == null)
            //{
            //    Echo("No welder group found!");
            //    return;
            //}
            //List<IMyTerminalBlock> welderList = new List<IMyTerminalBlock>();
            //welderGroup.GetBlocks(welderList);

            //foreach (var block in welderList)
            //{
            //    Echo($" - {block.CustomName}");
            //}


            // Warning Lights Group
        }

        bool FilterThis(IMyTerminalBlock block)
        {
            return block.CubeGrid == Me.CubeGrid;
        }

        public void Main(string argument, UpdateType updateSource)
        {

            if (argument.ToLower() == "welders")
            {
                WelderController(1);
            }
            if (argument.ToLower() == "weldersoff")
            {
                WelderController(0);
            }

            if (argument.ToLower() == "extend")
            {
                PistonController(speedExtend);
            }
            if (argument.ToLower() == "retract")
            {
                PistonController(speedRetract);
            }

            else if ((updateSource & (UpdateType.Update10)) == 0 && argument == "")
            {
                Echo("Fill in an argument you numpty.");
            }
        }

        // Function to turn the welders on or off

        public void WelderController(int state)
        {
            Echo(welderGroupName);
            if (state == 1)
            {
                Echo("Welders On");
                for (int i = 0; i < welderList.Count; i++)
                {
                    welderList[i].ApplyAction("OnOff_On");
                }
            }
            else
            {
                Echo("Welders Off");
                for (int i = 0; i < welderList.Count; i++)
                {
                    welderList[i].ApplyAction("OnOff_Off");
                }
            }
        }


        // Function to set the piston's velocity
        public void PistonController(float velocity)
        {
            Echo(pistonGroupName);
            Echo("Piston speed is now " + velocity);
            for (int i = 0; i < pistonList.Count; i++)
            {
                pistonList[i].SetValue("Velocity", (float)velocity);
            }
        }
    }
}
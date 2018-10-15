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

        // Speed at which the pistons should retract during printing. Should always be negative.
        double speedPrinting = -0.0075;

        // Speed at which the pistons should retract when it's not printing. Should always be negative.
        double speedRetract = -1;

        // Speed at which the pistons should extend to prepare for printing. Should always be positive.
        double speedExtend = 1;

        // Warning lights.
        // Example: string[] warningLightGroups = { "3D Warning Lights Top Row", "3D Warning Lights Spinning"};
        bool useWarningLights = false;
        string[] warningLightGroups = { };

        // =======================================================================================
        //                             --- END OF CONFIGURATION ---
        // ======================================================================================= 


        string command;

        public Program()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set RuntimeInfo.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.

            // Makes the script run every 10 in-game ticks
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            // Creates lists for the block groups
            List<IMyTerminalBlock> welderList = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> pistonList = new List<IMyTerminalBlock>();
            IMyBlockGroup welderGroup = GridTerminalSystem.GetBlockGroupWithName(welderGroupName);
            IMyBlockGroup pistonGroup = GridTerminalSystem.GetBlockGroupWithName(pistonGroupName);
            welderGroup.GetBlocks(welderList);
            pistonGroup.GetBlocks(pistonList);

        }


        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {



            if (welderGroup == null)
            {
                Echo("No welder group found!");
                return;
            }

            if (pistonGroup == null)
            {
                Echo("No piston group found!");
                return;
            }


            if (argument == "welders")
            {
                Welders(1);
            }
            if (argument == "weldersOff")
            {
                Welders(0);
            }
            if (argument == "extend")
            {
                Pistons(speedExtend);
            }
            if (argument == "retract")
            {
                Pistons(speedRetract);
            }

            //else if(UpdateType.Terminal != 0)
            //{
            //    Echo("Fill in an argument you numpty.");
            //}
        }

        // Function to turn the welders on or off
        public void Welders(int state)
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
            else if (state == 0)
            {
                Echo("Welders Off");
                for (int i = 0; i < welderList.Count; i++)
                {
                    welderList[i].ApplyAction("OnOff_Off");
                }
            }
        }

        // Function to set the piston's velocity
        public void Pistons(double velocity)
        {
            Echo(pistonGroupName);
            Echo("Piston speed is now " + velocity);
            for (int i = 0; i < pistonList.Count; i++)
            {
                pistonList[i].SetValue("Velocity", (double)velocity);
            }
        }

    }
}
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
            if(argument == "welders")
            {
                Echo(welderGroupName);
            }
            else
            {
                Echo("Fill in an argument you numpty.");
            }

            
        }
    }
}
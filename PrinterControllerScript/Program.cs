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

        // Speed at which the pistons should retract during printing. 
        //Should /always/ be negative. 
        //Please remember to leave the 'f' at the end of the number!
        float speedPrinting = -0.0075f;

        // Speed at which the pistons should retract when it's not printing. 
        //Should /always/ be negative.
        float speedRetract = -1;

        // Speed at which the pistons should extend to prepare for printing. 
        //Should /always/ be positive.
        float speedExtend = 1;

        // Warning lights.
        // Example: string[] warningLightGroups = { "3D Warning Lights (Top Row)", "3D Warning Lights Spinning"};
        bool useWarningLights = true;
        string[] warningLightGroups = { "3D Warning Lights (Top Row)", "3D Warning Lights Spinning" };

        // =======================================================================================
        //                             --- END OF CONFIGURATION ---
        // ======================================================================================= 

        // Declaration of non-user editable variables
        List<IMyTerminalBlock> pistonList = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> welderList = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> warningLightList = new List<IMyTerminalBlock>();
        string ERROR_TEXT = "";
        string helpOwnership = "\nPlease check the ownership settings of your blocks!\n";
        bool isValid = true;

        public Program()
        {
            // Makes the script run every 10 in-game ticks
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            VariableSanityCheck();
            ParseGroups();
            ErrorHandler(1);
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

        public void VariableSanityCheck()
        {
            if (speedPrinting >= -0.000001)
            {
                ERROR_TEXT += "Printing speed should be less than 0.";
            }

            if (speedRetract >= -0.000001)
            {
                ERROR_TEXT += "Piston retraction speed should be less than 0.";
            }

            if (speedExtend <= 0.000001)
            {
                ERROR_TEXT += "Piston extension speed should be more than 0.";
            }
        }

        public void ParseGroups()
        {
            // Checks and sets up Pistons group.
            if (GridTerminalSystem.GetBlockGroupWithName(pistonGroupName) != null)
            {
                GridTerminalSystem.GetBlockGroupWithName(pistonGroupName).GetBlocksOfType<IMyPistonBase>(pistonList);
                if (pistonList.Count == 0)
                {
                    ERROR_TEXT += "Group \"" + pistonGroupName + "\" has no piston blocks!\n";
                    isValid = false;
                }
            }
            else
            {
                ERROR_TEXT += "Group \"" + pistonGroupName + "\" not found!\n";
                isValid = false;
            }

            // Checks and sets up Welders group.
            if (GridTerminalSystem.GetBlockGroupWithName(welderGroupName) != null)
            {
                GridTerminalSystem.GetBlockGroupWithName(welderGroupName).GetBlocksOfType<IMyShipWelder>(welderList);
                if (welderList.Count == 0)
                {
                    ERROR_TEXT += "Group \"" + welderGroupName + "\" has no welder blocks!\n";
                    isValid = false;
                }
            }
            else
            {
                ERROR_TEXT += "Group \"" + welderGroupName + "\" not found!\n";
                isValid = false;
            }

            // TODO: Warning Lights Group
            // Warning Lights
            if (useWarningLights)
            {
                foreach (var word in warningLightGroups)
                {
                    if (GridTerminalSystem.GetBlockGroupWithName(word) != null)
                    {
                        // TODO: Make it append the new items to the list instead of making a new list for the second, third, etc. group, overwriting the old.
                        //GridTerminalSystem.GetBlockGroupWithName(word).GetBlocks(warningLightList);

                        List<IMyTerminalBlock> warningLightStorage = new List<IMyTerminalBlock>();
                        GridTerminalSystem.GetBlockGroupWithName(word).GetBlocks(warningLightStorage);
                        //warningLightList<IMyTerminalBlock>.Add(warningLightStorage);
                        foreach (var block in warningLightStorage)
                        {
                            warningLightList.Add(block);
                        }
                        if (warningLightList.Count == 0)
                        {
                            ERROR_TEXT += "Group \"" + word + "\" has no blocks!\n";
                            isValid = false;
                        }
                    }
                    else
                    {
                        ERROR_TEXT += "Group \"" + word + "\" not found!\n";
                        isValid = false;
                    }
                }
                //if (GridTerminalSystem.GetBlockGroupWithName(warningLightGroups))
                //{

                //}
            }

            // Amends a useful block ownership tip to the error message if it concerns a group issue.
            if (ERROR_TEXT.Contains("Group"))
            {
                ERROR_TEXT += helpOwnership;
            }
        }

        // Main function, processes all incoming run requests and arguments
        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & (UpdateType.Update10)) == 0 && isValid == true)
            {
                switch (argument.ToLower())
                {
                    case "welders":
                        WelderController(1);
                        break;
                    case "weldersoff":
                        WelderController(0);
                        break;
                    case "extend":
                        PistonController(speedExtend);
                        break;
                    case "retract":
                        PistonController(speedRetract);
                        break;
                    case "print":
                        PistonController(speedPrinting);
                        WelderController(1);
                        break;
                    case "lights":
                        WarningLightController(1);
                        break;
                    case "lightsoff":
                        WarningLightController(0);
                        break;
                    case "reset":
                        PistonController(speedRetract);
                        WelderController(0);
                        break;
                    case "":
                        ERROR_TEXT += "Please fill in an argument\n";
                        ErrorHandler(1);
                        ErrorHandler(0);
                        break;
                    default:
                        ERROR_TEXT += "\"" + argument.ToLower() + "\" is not a valid argument.\n";
                        ErrorHandler(1);
                        ErrorHandler(0);
                        break;
                }
            }
        }

        // Function to turn the welders on or off.
        public void WelderController(int state)
        {
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

        // Function to set the piston's velocity.
        public void PistonController(float velocity)
        {
            Echo("Piston speed is now " + velocity);
            for (int i = 0; i < pistonList.Count; i++)
            {
                pistonList[i].SetValue("Velocity", (float)velocity);
            }
        }

        // Function to turn the warning lights on or off.
        public void WarningLightController(int state)
        {
            foreach (var block in warningLightList)
            {
                Echo($" - {block.CustomName}");
            }
            if (state == 1)
            {
                Echo("Warning Lights On");
                for (int i = 0; i < warningLightList.Count; i++)
                {
                    warningLightList[i].ApplyAction("OnOff_On");
                }
            }
            else
            {
                Echo("Warning Lights Off");
                for (int i = 0; i < warningLightList.Count; i++)
                {
                    warningLightList[i].ApplyAction("OnOff_Off");
                }
            }
        }

        // Function to display errors to the user using Echo().
        public void ErrorHandler(int state)
        {
            // Shows the errors
            if (state == 1 && ERROR_TEXT != "")
            {
                Echo("!!!Script errors found!!!\n\n" + ERROR_TEXT);
                return;
            }
            else
            {
                Echo(null);
                ERROR_TEXT = "";
            }
        }
    }
}
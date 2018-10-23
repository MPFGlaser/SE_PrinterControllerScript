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
        string pistonGroupName = "Name of the piston group goes here";

        // Name of the group with the printer's welders.
        string welderGroupName = "Name of the welder group goes here";

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
        // Example: string[] warningLightGroups = { "Printer Warning Lights", "Printer Warning Lights 2"};
        bool useWarningLights = false;
        string[] warningLightGroups = { };

        // === ADVANCED USERS ONLY ===
        // Only touch this if you know what you're doing.
        // setting this to true will disable the checking of user-defined piston speeds. THIS CAN SEVERELY BREAK THINGS.
        bool disableSanitycheck = false;

        // =======================================================================================
        //                             --- END OF CONFIGURATION ---
        // ======================================================================================= 






        //Touchy touchy means breaky breaky.





        // Declaration of non-user editable variables
        List<IMyTerminalBlock> pistonList = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> welderList = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> warningLightList = new List<IMyTerminalBlock>();
        string ERROR_TEXT = "";
        string helpOwnership = "\nPlease check the ownership settings of your blocks!\n";
        int pistonExtendedCount;
        bool isValid = true;
        bool usePistonCheck;
        bool currentlyPrinting;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            if (!disableSanitycheck)
            {
                VariableSanityCheck();
            }
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

            // Warning Lights
            if (useWarningLights)
            {
                foreach (var word in warningLightGroups)
                {
                    if (GridTerminalSystem.GetBlockGroupWithName(word) != null)
                    {
                        List<IMyTerminalBlock> warningLightStorage = new List<IMyTerminalBlock>();
                        GridTerminalSystem.GetBlockGroupWithName(word).GetBlocks(warningLightStorage);
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
            if ((updateSource & (UpdateType.Update100)) == 0 && isValid == true)
            {
                switch (argument.ToLower())
                {
                    case "print":
                        usePistonCheck = true;
                        break;
                    case "reset":
                        Reset();
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

            // Printing sequence logic
            if (usePistonCheck && pistonExtendedCount != pistonList.Count && !currentlyPrinting)
            {
                PistonExtensionChecker();
                PistonController(speedExtend);
            }

            if (usePistonCheck && pistonExtendedCount == pistonList.Count && !currentlyPrinting)
            {
                WelderController(1);
                PistonController(speedPrinting);
                usePistonCheck = false;
                currentlyPrinting = true;
                pistonExtendedCount = 0;
            }

            if (currentlyPrinting && usePistonCheck)
            {
                Reset();
            }

            // Makes the lights do blinky blinky when sh!t gets real.
            if ((updateSource & (UpdateType.Update100)) != 0)
            {
                WarningLightController();
            }
        }

        public void Reset()
        {
            PistonController(speedRetract);
            WelderController(0);
            usePistonCheck = false;
            currentlyPrinting = false;
        }

        // Function to turn the welders on or off.
        public void WelderController(int state)
        {
            if (state == 1)
            {
                for (int i = 0; i < welderList.Count; i++)
                {
                    welderList[i].ApplyAction("OnOff_On");
                }
            }
            else
            {
                for (int i = 0; i < welderList.Count; i++)
                {
                    welderList[i].ApplyAction("OnOff_Off");
                }
            }
        }

        // Function to set the piston's velocity.
        public void PistonController(float velocity)
        {
            for (int i = 0; i < pistonList.Count; i++)
            {
                pistonList[i].SetValue("Velocity", (float)velocity);
            }
        }

        // Function to turn the warning lights on or off depending on piston postion and welder status.
        public void WarningLightController()
        {
            // Variable initialisation for counting system
            bool weldersOff;
            int inactiveWelderCount = 0;
            bool pistonsHome;
            int pistonHomeCount = 0;

            // Counting of inactive welders
            for (int i = 0; i < welderList.Count; i++)
            {
                if (!((IMyShipWelder)welderList[i]).Enabled)
                {
                    inactiveWelderCount++;
                }
            }

            // Counting of pistons that are "home" (near 0.0m extension)
            for (int i = 0; i < pistonList.Count; i++)
            {
                if (((IMyPistonBase)pistonList[i]).CurrentPosition < 0.1)
                {
                    pistonHomeCount++;
                }
            }

            // If the amount of inactive (off) welders is equal to the total amount of welders, all welders are off.
            if (inactiveWelderCount == welderList.Count)
            {
                weldersOff = true;
            }
            else
            {
                weldersOff = false;
            }

            // If the amount of pistons in the "home" position is equal to the amount of pistons, all pistons are home.
            if (pistonHomeCount == pistonList.Count)
            {
                pistonsHome = true;
            }
            else
            {
                pistonsHome = false;
            }

            // if the welders aren't off (thus, on) OR if the pistons aren't home (extended beyond 0.1m), turn on all lights in the warning light groups declared in the config.
            if (!weldersOff || !pistonsHome)
            {
                for (int i = 0; i < warningLightList.Count; i++)
                {
                    warningLightList[i].ApplyAction("OnOff_On");
                }
            }
            // When both all pistons are home and all welders are off, turn off all warning lights.
            else
            {
                for (int i = 0; i < warningLightList.Count; i++)
                {
                    warningLightList[i].ApplyAction("OnOff_Off");
                }
            }
        }

        public void PistonExtensionChecker()
        {
            pistonExtendedCount = 0;
            for (int i = 0; i < pistonList.Count; i++)
            {
                if (((IMyPistonBase)pistonList[i]).CurrentPosition == ((IMyPistonBase)pistonList[i]).MaxLimit)
                {
                    pistonExtendedCount++;
                }
            }
        }

        // Function to display errors to the user using Echo().
        public void ErrorHandler(int state)
        {
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
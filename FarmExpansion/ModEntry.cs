using StardewModdingAPI;
using StardewModdingAPI.Events;
using FarmExpansion.Framework;

namespace FarmExpansion
{

    public class ModEntry : Mod
    {
        private FEFramework framework;

        public override void Entry(IModHelper helper)
        {
            framework = new FEFramework(helper, Monitor);
            //ControlEvents.KeyPressed += framework.ControlEvents_KeyPress;
            ControlEvents.ControllerButtonPressed += framework.ControlEvents_ControllerButtonPressed;
            ControlEvents.MouseChanged += framework.ControlEvents_MouseChanged;
            //LocationEvents.CurrentLocationChanged += framework.LocationEvents_CurrentLocationChanged;
            MenuEvents.MenuChanged += framework.MenuEvents_MenuChanged;
            MenuEvents.MenuClosed += framework.MenuEvents_MenuClosed;
            SaveEvents.AfterLoad += framework.SaveEvents_AfterLoad;
            SaveEvents.BeforeSave += framework.SaveEvents_BeforeSave;
            SaveEvents.AfterSave += framework.SaveEvents_AfterSave;
            SaveEvents.AfterReturnToTitle += framework.SaveEvents_AfterReturnToTitle;
            TimeEvents.AfterDayStarted += framework.TimeEvents_AfterDayStarted;
        }

    }
}
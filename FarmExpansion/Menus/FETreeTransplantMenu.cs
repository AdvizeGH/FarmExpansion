using FarmExpansion.Framework;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using xTile.Dimensions;
using Microsoft.Xna.Framework.Graphics;

namespace FarmExpansion.Menus
{
    internal class FETreeTransplantMenu : IClickableMenu
    {
        FEFramework framework;
        private IClickableMenu TreeTransplantMenu;
        private ClickableTextureComponent swapFarmButton;


        /* Texture position and texture used for icon are placeholders currently*/
        public FETreeTransplantMenu(FEFramework framework, IClickableMenu menu)
        {
            this.framework = framework;
            this.TreeTransplantMenu = menu;

            swapFarmButton = new ClickableTextureComponent(
                new Rectangle(
                    xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - (Game1.tileSize * 4) - (Game1.tileSize / 4),
                    yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + Game1.tileSize,
                    Game1.tileSize,
                    Game1.tileSize),
                Game1.mouseCursors,
                new Rectangle(0, 0, 64, 64),
                1.0f);

            //resetBounds();
            swapFarmButton.bounds.X = (Game1.viewport.Width - Game1.tileSize * 2) - (int)(Game1.tileSize / 0.75) - (int)(Game1.tileSize / 0.75);
            swapFarmButton.bounds.Y = (Game1.viewport.Height - Game1.tileSize * 2) - (int)(Game1.tileSize / 0.75) - (int)(Game1.tileSize / 0.75);
        }

        //private void resetBounds()
        //{
        //    swapFarmButton = new ClickableTextureComponent(
        //        new Rectangle(
        //            xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - (Game1.tileSize * 4) - (Game1.tileSize / 4),
        //            yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + Game1.tileSize,
        //            Game1.tileSize,
        //            Game1.tileSize),
        //        Game1.mouseCursors,
        //        new Rectangle(0, 0, 64, 64),
        //        1.0f);
        //}

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (Game1.globalFade)
                return;
            if (swapFarmButton.containsPoint(x, y))
            {
                handleSwapFarmAction();
                return;
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void performHoverAction(int x, int y)
        {
            swapFarmButton.tryHover(x, y);
        }

        public override void draw(SpriteBatch b)
        {
            swapFarmButton.draw(b);
        }

        private void handleSwapFarmAction()
        {
            if (TreeTransplantMenu.readyToClose())
                Game1.globalFadeToBlack(new Game1.afterFadeFunction(swapFarm));
            else
                framework.helper.Reflection.GetPrivateMethod(TreeTransplantMenu, "handleCancelAction").Invoke();
            //bool selectedTree = framework.helper.Reflection.GetPrivateField<object>(TreeTransplantMenu, "selectedTree") != null;
            //if (selectedTree)
            //{
            //    framework.helper.Reflection.GetPrivateField<object>(TreeTransplantMenu, "selectedTree").SetValue(null);
            //    Game1.playSound("shwip");
            //}
        }

        private void swapFarm()
        {
            // clean up before leaving the area
            Game1.currentLocation.cleanupBeforePlayerExit();
            // move to the opposite farm
            Game1.currentLocation = Game1.currentLocation.GetType().FullName.Contains("Expansion") ? Game1.getFarm() : Game1.getLocationFromName("FarmExpansion");
            // reset the location for our entry
            Game1.currentLocation.resetForPlayerEntry();
            // freeze the viewport
            //Game1.viewportFreeze = true;
            // set the new viewport
            Game1.viewport.Location = new Location(49 * Game1.tileSize, 5 * Game1.tileSize);
            // pan the screen
            Game1.panScreen(0, 0);
            // fade the screen in with no callback
            Game1.globalFadeToClear();
        }
    }
}

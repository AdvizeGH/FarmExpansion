using FarmExpansion.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FarmExpansion.Framework
{
    public class FEFramework
    {
        internal IModHelper helper;
        internal IMonitor monitor;

        private FarmExpansion farmExpansion;
        private Map map;
        private XmlSerializer locationSerializer = new XmlSerializer(typeof(FarmExpansion));
        private NPC robin;
        internal FEConfig config;

        internal bool IsTreeTransplantLoaded;
        private IClickableMenu menuOverride;
        private bool overridableMenuActive;

        public FEFramework(IModHelper helper, IMonitor monitor)
        {
            this.helper = helper;
            this.monitor = monitor;
            config = helper.ReadConfig<FEConfig>();
        }

        /*internal void ControlEvents_KeyPress(object sender, EventArgsKeyPressed e)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
                return;
            if (e.KeyPressed.ToString() == "V")
            {
                if (Game1.currentLocation.Name != "FarmExpansion")
                {
                    Game1.warpFarmer("FarmExpansion", 46, 4, false);
                }
                else
                {
                    Game1.warpFarmer("FarmHouse", 5, 8, false);
                }
            }
            if (e.KeyPressed.ToString().Equals("K"))
            {
                Game1.activeClickableMenu = new FECarpenterMenu(this);
            }
            if (e.KeyPressed.ToString().Equals("N"))
            {
                Game1.activeClickableMenu = new FEPurchaseAnimalsMenu(this);
            }
            if (e.KeyPressed.ToString().Equals("G"))
            {

            }
            if (e.KeyPressed.ToString().Equals("O"))
            {

            }
        }*/

        [Obsolete("Obsolete as of 3.1")]
        internal void ControlEvents_ControllerButtonPressed(object sender, EventArgsControllerButtonPressed e)
        {
            if (!Context.IsWorldReady) return;
            if (e.ButtonPressed == Buttons.A)
            {
                CheckForAction();
            }
        }

        [Obsolete("Obsolete as of 3.1")]
        internal void ControlEvents_MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            if (!overridableMenuActive)
                return;
            if (e.NewState.LeftButton == ButtonState.Pressed && e.PriorState.LeftButton != ButtonState.Pressed)
            {
                menuOverride.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY(), false);
            }
            /*if (!Context.IsWorldReady) return;
            if (e.NewState.RightButton == ButtonState.Pressed && e.PriorState.RightButton != ButtonState.Pressed)
            {
                CheckForAction();
            }*/
            /*if (e.NewState.LeftButton == ButtonState.Pressed && e.PriorState.LeftButton != ButtonState.Pressed)
            {
                this.monitor.Log($"Current terrain features in area {farmExpansion.terrainFeatures.Count}");
            }*/
        }

        internal void GraphicsEvents_OnPostRenderGuiEvent(object sender, EventArgs e)
        {
            menuOverride.draw(Game1.spriteBatch);
        }

        /*internal void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            if (e.NewLocation?.Name == "FarmExpansion")
            {
                farmExpansion.terrainFeatures.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(TerrainFeaturesChanged);
                return;
            }
            if (e.PriorLocation?.Name == "FarmExpansion")
            {
                farmExpansion.terrainFeatures.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(TerrainFeaturesChanged);
                return;
            }
        }*/

        internal void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            // Add farm expansion point to world map
            if (e.NewMenu is GameMenu)
            {
                MapPage mp = null;

                foreach (IClickableMenu page in this.helper.Reflection.GetPrivateValue<List<IClickableMenu>>(Game1.activeClickableMenu, "pages"))
                {
                    if (!(page is MapPage))
                        continue;
                    mp = page as MapPage;
                    break;
                }
                if (mp == null)
                    return;

                int mapX = this.helper.Reflection.GetPrivateValue<int>(mp, "mapX");
                int mapY = this.helper.Reflection.GetPrivateValue<int>(mp, "mapY");
                Rectangle locationOnMap = new Rectangle(mapX + 156, mapY + 272, 100, 80);

                mp.points.Add(new ClickableComponent(locationOnMap, "Farm Expansion"));

                foreach (ClickableComponent cc in mp.points)
                {
                    if (cc.myID != 1030)
                        continue;

                    cc.bounds.Width -= 64;
                    break;
                }

                if (Game1.currentLocation == farmExpansion)
                {
                    this.helper.Reflection.GetPrivateField<Vector2>(mp, "playerMapPosition").SetValue(new Vector2(mapX + 50 * Game1.pixelZoom, mapY + 75 * Game1.pixelZoom));
                    this.helper.Reflection.GetPrivateField<string>(mp, "playerLocationName").SetValue("Farm Expansion");
                }
                return;
            }
            // Intercept carpenter menu
            if (e.NewMenu is CarpenterMenu)
            {
                if (!this.helper.Reflection.GetPrivateValue<bool>(e.NewMenu, "magicalConstruction"))
                    Game1.activeClickableMenu = new FECarpenterMenu(this);
                return;
            }
            // Intercept purchase animals menu
            if (e.NewMenu is PurchaseAnimalsMenu)
            {
                Game1.activeClickableMenu = new FEPurchaseAnimalsMenu(this);
                return;
            }
            // Fixes infinite loop when animals hatch on farm expansion
            if (e.NewMenu is NamingMenu)
            {
                foreach (Building building in farmExpansion.buildings)
                    if (building.indoors != null && building.indoors == Game1.currentLocation)
                        Game1.getFarm().buildings.AddRange(farmExpansion.buildings);
                return;
            }
            if (IsTreeTransplantLoaded)
                if (e.NewMenu.GetType().FullName.Equals("TreeTransplant.TreeTransplantMenu"))
                {
                    menuOverride = new FETreeTransplantMenu(this, e.NewMenu);
                    overridableMenuActive = true;
                    GraphicsEvents.OnPostRenderGuiEvent += this.GraphicsEvents_OnPostRenderGuiEvent;
                    ControlEvents.MouseChanged += this.ControlEvents_MouseChanged;
                }
                    
            
        }

        internal void MenuEvents_MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            if (e.PriorMenu is NamingMenu)
                foreach (Building building in farmExpansion.buildings)
                    Game1.getFarm().buildings.Remove(building);
            if (IsTreeTransplantLoaded)
                if (e.PriorMenu.GetType().FullName.Equals("TreeTransplant.TreeTransplantMenu"))
                {
                    ControlEvents.MouseChanged -= this.ControlEvents_MouseChanged;
                    GraphicsEvents.OnPostRenderGuiEvent -= this.GraphicsEvents_OnPostRenderGuiEvent;
                    overridableMenuActive = false;
                    menuOverride = null;
                }
        }

        internal void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            try
            {
                map = helper.Content.Load<Map>("FarmExpansion.xnb", ContentSource.ModFolder);
                map.LoadTileSheets(Game1.mapDisplayDevice);
            }
            catch (Exception ex)
            {
                //ControlEvents.KeyPressed -= this.ControlEvents_KeyPress;
                //ControlEvents.ControllerButtonPressed -= this.ControlEvents_ControllerButtonPressed;
                //ControlEvents.MouseChanged -= this.ControlEvents_MouseChanged;
                //LocationEvents.CurrentLocationChanged -= this.LocationEvents_CurrentLocationChanged;
                MenuEvents.MenuChanged -= this.MenuEvents_MenuChanged;
                MenuEvents.MenuClosed -= this.MenuEvents_MenuClosed;
                SaveEvents.AfterLoad -= this.SaveEvents_AfterLoad;
                SaveEvents.BeforeSave -= this.SaveEvents_BeforeSave;
                SaveEvents.AfterSave -= this.SaveEvents_AfterSave;
                SaveEvents.AfterReturnToTitle -= this.SaveEvents_AfterReturnToTitle;
                TimeEvents.AfterDayStarted -= this.TimeEvents_AfterDayStarted;

                monitor.Log(ex.Message, LogLevel.Error);
                monitor.Log($"Unable to load map file 'FarmExpansion.xnb', unloading mod. Please try re-installing the mod.", LogLevel.Alert);
                return;
            }

            if (!File.Exists(Path.Combine(helper.DirectoryPath, "pslocationdata", $"{Constants.SaveFolderName}.xml")))
            {
                farmExpansion = new FarmExpansion(map, "FarmExpansion", this)
                {
                    isFarm = true,
                    isOutdoors = true
                };
            }
            else
            {
                Load();
            }

            for (int i = 0; i < farmExpansion.Map.TileSheets.Count; i++)
            {
                if (!farmExpansion.Map.TileSheets[i].ImageSource.Contains("path") && !farmExpansion.Map.TileSheets[i].ImageSource.Contains("object"))
                {
                    farmExpansion.Map.TileSheets[i].ImageSource = "Maps\\" + Game1.currentSeason + "_" + farmExpansion.Map.TileSheets[i].ImageSource.Split(new char[]
                    {
                                    '_'
                    })[1];
                    farmExpansion.Map.DisposeTileSheets(Game1.mapDisplayDevice);
                    farmExpansion.Map.LoadTileSheets(Game1.mapDisplayDevice);
                }
            }

            Game1.locations.Add(farmExpansion);
            PatchMap(Game1.getFarm());
            RepairBuildingWarps();
        }

        internal void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            Save();
            Game1.locations.Remove(farmExpansion);
        }

        internal void SaveEvents_AfterSave(object sender, EventArgs e)
        {
            Game1.locations.Add(farmExpansion);
        }

        internal void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            farmExpansion = null;
            map = null;
        }

        internal void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            if (Game1.isRaining)
                foreach (KeyValuePair<Vector2, TerrainFeature> pair in farmExpansion.terrainFeatures)
                    if (pair.Value != null && pair.Value is HoeDirt)
                        ((HoeDirt)pair.Value).state = 1;

            foreach (Building current in farmExpansion.buildings)
                if (current.indoors != null)
                    for (int k = current.indoors.objects.Count - 1; k >= 0; k--)
                        if (current.indoors.objects[current.indoors.objects.Keys.ElementAt(k)].minutesElapsed(3000 - Game1.timeOfDay, current.indoors))
                            current.indoors.objects.Remove(current.indoors.objects.Keys.ElementAt(k));

            if (Game1.player.currentUpgrade != null)
                if (farmExpansion.objects.ContainsKey(new Vector2(Game1.player.currentUpgrade.positionOfCarpenter.X / Game1.tileSize, Game1.player.currentUpgrade.positionOfCarpenter.Y / Game1.tileSize)))
                    farmExpansion.objects.Remove(new Vector2(Game1.player.currentUpgrade.positionOfCarpenter.X / Game1.tileSize, Game1.player.currentUpgrade.positionOfCarpenter.Y / Game1.tileSize));

            RepairBuildingWarps();

            if (farmExpansion.isThereABuildingUnderConstruction() && !Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
            {
                bool flag2 = false;
                foreach (GameLocation location in Game1.locations)
                {
                    if (flag2)
                        break;

                    foreach (NPC npc in location.characters)
                    {
                        if (!npc.name.Equals("Robin"))
                            continue;

                        robin = npc;
                        npc.ignoreMultiplayerUpdates = true;
                        npc.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                        {
                            new FarmerSprite.AnimationFrame(24, 75),
                            new FarmerSprite.AnimationFrame(25, 75),
                            new FarmerSprite.AnimationFrame(26, 300, false, false, new AnimatedSprite.endOfAnimationBehavior(robinHammerSound), false),
                            new FarmerSprite.AnimationFrame(27, 1000, false, false, new AnimatedSprite.endOfAnimationBehavior(robinVariablePause), false)
                        });
                        npc.ignoreScheduleToday = true;
                        Building buildingUnderConstruction = farmExpansion.getBuildingUnderConstruction();
                        if (buildingUnderConstruction.daysUntilUpgrade > 0)
                        {
                            if (!buildingUnderConstruction.indoors.characters.Contains(npc))
                                buildingUnderConstruction.indoors.addCharacter(npc);

                            if (npc.currentLocation != null)
                                npc.currentLocation.characters.Remove(npc);

                            npc.currentLocation = buildingUnderConstruction.indoors;
                            npc.setTilePosition(1, 5);
                        }
                        else
                        {
                            Game1.warpCharacter(npc, "FarmExpansion", new Vector2(buildingUnderConstruction.tileX + buildingUnderConstruction.tilesWide / 2, buildingUnderConstruction.tileY + buildingUnderConstruction.tilesHigh / 2), false, false);
                            npc.position.X = npc.position.X + Game1.tileSize / 4;
                            npc.position.Y = npc.position.Y - Game1.tileSize / 2;
                        }
                        npc.CurrentDialogue.Clear();
                        npc.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3926", new object[0]), npc));
                        flag2 = true;
                        break;
                    }
                }
            }
            else
            {
                farmExpansion.removeCarpenter();
            }
        }

        /*private void TerrainFeaturesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            monitor.Log($"New terrain features in area {farmExpansion.terrainFeatures.Count}");
            monitor.Log($"terrainFeatures changed, {e.OldItems?.Count} items removed, {e.NewItems?.Count} added");
        }*/

        [Obsolete("Obsolete as of 3.1")]
        private void CheckForAction()
        {
            if (Game1.player.UsingTool || Game1.pickingTool || Game1.menuUp || (Game1.eventUp && !Game1.currentLocation.currentEvent.playerControlSequence) || Game1.nameSelectUp || Game1.numberOfSelectedItems != -1 || Game1.fadeToBlack || Game1.activeClickableMenu != null)
                return;

            // get the activated tile
            Vector2 grabTile = new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize;
            if (!Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, 1, Game1.player))
                grabTile = Game1.player.GetGrabTile();

            // check tile action
            xTile.Tiles.Tile tile = Game1.currentLocation.map.GetLayer("Buildings").PickTile(new Location((int)grabTile.X * Game1.tileSize, (int)grabTile.Y * Game1.tileSize), Game1.viewport.Size);
            xTile.ObjectModel.PropertyValue propertyValue = null;
            tile?.Properties.TryGetValue("Action", out propertyValue);
            if (propertyValue != null)
            {
                /*if (propertyValue == "TestProperty")
                {
                    Game1.drawObjectDialogue("Cottage interior not yet implemented.");
                }*/
                if (propertyValue == "FECarpenter")
                {
                    if (Game1.player.getTileY() > grabTile.Y)
                    {
                        carpenter(new Location((int)grabTile.X, (int)grabTile.Y));
                    }
                }
                if (propertyValue == "FEAnimalShop")
                {
                    if (Game1.player.getTileY() > grabTile.Y)
                    {
                        animalShop(new Location((int)grabTile.X, (int)grabTile.Y));
                    }
                }
            }
        }

        private void Save()
        {
            string path = Path.Combine(helper.DirectoryPath, "pslocationdata", $"{Constants.SaveFolderName}.xml");

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var writer = XmlWriter.Create(path))
            {
                locationSerializer.Serialize(writer, farmExpansion);
            }
            //monitor.Log($"Object serialized to {path}");
        }

        private void Load()
        {
            farmExpansion = new FarmExpansion(map, "FarmExpansion", this)
            {
                isFarm = true,
                isOutdoors = true
            };

            string path = Path.Combine(helper.DirectoryPath, "pslocationdata", $"{Constants.SaveFolderName}.xml");

            FarmExpansion loaded;
            using (var reader = XmlReader.Create(path))
            {
                loaded = (FarmExpansion)locationSerializer.Deserialize(reader);
            }
            //monitor.Log($"Object deserialized from {path}");

            farmExpansion.animals = loaded.animals;
            farmExpansion.buildings = loaded.buildings;
            farmExpansion.characters = loaded.characters;
            farmExpansion.terrainFeatures = loaded.terrainFeatures;
            farmExpansion.largeTerrainFeatures = loaded.largeTerrainFeatures;
            farmExpansion.resourceClumps = loaded.resourceClumps;
            farmExpansion.objects = loaded.objects;
            farmExpansion.numberOfSpawnedObjectsOnMap = loaded.numberOfSpawnedObjectsOnMap;
            farmExpansion.piecesOfHay = loaded.piecesOfHay;
            //farmExpansion.hasSeenGrandpaNote = loaded.hasSeenGrandpaNote;
            //farmExpansion.grandpaScore = loaded.grandpaScore;

            foreach (KeyValuePair<long, FarmAnimal> animal in farmExpansion.animals)
                animal.Value.reload();

            foreach (Building building in farmExpansion.buildings)
            {
                building.load();
                if (building.indoors != null && building.indoors is AnimalHouse)
                {
                    foreach (KeyValuePair<long, FarmAnimal> animalsInBuilding in ((AnimalHouse)building.indoors).animals)
                    {
                        FarmAnimal animal = animalsInBuilding.Value;

                        foreach (Building current in farmExpansion.buildings)
                        {
                            if (current.tileX == (int)animal.homeLocation.X && current.tileY == (int)animal.homeLocation.Y)
                            {
                                animal.home = current;
                                break;
                            }
                        }
                    }
                }
            }
            for (int i = farmExpansion.characters.Count - 1; i >= 0; i--)
            {
                if (!farmExpansion.characters[i].DefaultPosition.Equals(Vector2.Zero))
                    farmExpansion.characters[i].position = farmExpansion.characters[i].DefaultPosition;

                farmExpansion.characters[i].currentLocation = farmExpansion;

                if (i < farmExpansion.characters.Count)
                    farmExpansion.characters[i].reloadSprite();
            }

            foreach (KeyValuePair<Vector2, TerrainFeature> terrainFeature in farmExpansion.terrainFeatures)
                terrainFeature.Value.loadSprite();

            foreach (KeyValuePair<Vector2, Object> current in farmExpansion.objects)
            {
                current.Value.initializeLightSource(current.Key);
                current.Value.reloadSprite();
            }
            foreach (Building building in farmExpansion.buildings)
            {
                Vector2 tile = new Vector2((float)building.tileX, (float)building.tileY);

                if (building.indoors is Shed)
                {
                    (building.indoors as Shed).furniture = (loaded.getBuildingAt(tile).indoors as Shed).furniture;
                    (building.indoors as Shed).wallPaper = (loaded.getBuildingAt(tile).indoors as Shed).wallPaper;
                    (building.indoors as Shed).floor = (loaded.getBuildingAt(tile).indoors as Shed).floor;
                }
            }
        }

        private void RepairBuildingWarps()
        {
            foreach (Building building in farmExpansion.buildings)
            {
                if (building.indoors != null)
                {
                    List<Warp> warps = new List<Warp>();
                    foreach (Warp warp in building.indoors.warps)
                    {
                        warps.Add(new Warp(warp.X, warp.Y, "FarmExpansion", building.humanDoor.X + building.tileX, building.humanDoor.Y + building.tileY + 1, false));
                    }
                    building.indoors.warps.Clear();
                    building.indoors.warps.AddRange(warps);
                }
            }
        }

        private void PatchMap(GameLocation gl)
        {
            string t = "untitled tile sheet";
            int warpYLocA, warpYLocB, warpYLocC;
            List<Tile> tiles = new List<Tile>();

            switch (Game1.whichFarm)
            {
                case 1: // Fishing Farm
                    tiles.Add(new Tile(TileLayer.Back, 0, 55, 251, t)); tiles.Add(new Tile(TileLayer.Back, 1, 55, 251, t)); tiles.Add(new Tile(TileLayer.Back, 0, 56, 326, t));
                    tiles.Add(new Tile(TileLayer.Back, 1, 56, 326, t)); tiles.Add(new Tile(TileLayer.Back, 0, 57, 351, t)); tiles.Add(new Tile(TileLayer.Back, 1, 57, 351, t));

                    tiles.Add(new Tile(TileLayer.Buildings, 0, 55, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 55, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 55, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 56, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 56, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 56, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 57, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 57, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 57, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 58, 175, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 58, 175, t));

                    tiles.Add(new Tile(TileLayer.Front, 0, 54, -1, t)); tiles.Add(new Tile(TileLayer.Front, 1, 54, -1, t)); tiles.Add(new Tile(TileLayer.Front, 2, 54, -1, t));
                    tiles.Add(new Tile(TileLayer.Front, 0, 55, -1, t)); tiles.Add(new Tile(TileLayer.Front, 1, 55, -1, t)); tiles.Add(new Tile(TileLayer.Front, 0, 57, 413, t));
                    tiles.Add(new Tile(TileLayer.Front, 1, 57, 414, t)); tiles.Add(new Tile(TileLayer.Front, 2, 57, 438, t)); tiles.Add(new Tile(TileLayer.Front, 0, 58, 175, t));
                    tiles.Add(new Tile(TileLayer.Front, 1, 58, 175, t));

                    warpYLocA = 55; warpYLocB = 56; warpYLocC = 57;
                    break;
                case 2: // Foraging Farm
                    tiles.Add(new Tile(TileLayer.Back, 2, 28, 375, t)); tiles.Add(new Tile(TileLayer.Back, 3, 28, 376, t)); tiles.Add(new Tile(TileLayer.Back, 4, 28, 376, t));
                    tiles.Add(new Tile(TileLayer.Back, 5, 28, 377, t)); tiles.Add(new Tile(TileLayer.Back, 2, 29, 175, t)); tiles.Add(new Tile(TileLayer.Back, 3, 29, 175, t));
                    tiles.Add(new Tile(TileLayer.Back, 4, 29, 175, t)); tiles.Add(new Tile(TileLayer.Back, 5, 29, 175, t)); tiles.Add(new Tile(TileLayer.Back, 2, 30, 175, t));
                    tiles.Add(new Tile(TileLayer.Back, 3, 30, 175, t)); tiles.Add(new Tile(TileLayer.Back, 4, 30, 175, t)); tiles.Add(new Tile(TileLayer.Back, 5, 30, 175, t));

                    tiles.Add(new Tile(TileLayer.Buildings, 0, 28, 967, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 28, 968, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 28, 967, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 3, 28, 968, t)); tiles.Add(new Tile(TileLayer.Buildings, 0, 29, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 29, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 2, 29, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 3, 29, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 0, 30, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 1, 30, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 30, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 3, 30, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 4, 30, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 0, 31, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 31, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 2, 31, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 3, 31, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 4, 31, -1, t));

                    tiles.Add(new Tile(TileLayer.Front, 0, 31, 1042, t)); tiles.Add(new Tile(TileLayer.Front, 1, 31, 1043, t)); tiles.Add(new Tile(TileLayer.Front, 2, 31, 1042, t));
                    tiles.Add(new Tile(TileLayer.Front, 3, 31, 1042, t)); tiles.Add(new Tile(TileLayer.Front, 4, 31, 1043, t)); tiles.Add(new Tile(TileLayer.Front, 5, 31, 1017, t));

                    tiles.Add(new Tile(TileLayer.AlwaysFront, 0, 26, 940, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 0, 27, 941, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 1, 27, 942, t));
                    tiles.Add(new Tile(TileLayer.AlwaysFront, 0, 28, 967, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 1, 28, 968, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 2, 28, 967, t));
                    tiles.Add(new Tile(TileLayer.AlwaysFront, 3, 28, 968, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 4, 28, 992, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 0, 29, -1, t));
                    tiles.Add(new Tile(TileLayer.AlwaysFront, 1, 29, -1, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 2, 29, -1, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 3, 29, -1, t));
                    tiles.Add(new Tile(TileLayer.AlwaysFront, 4, 29, -1, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 5, 29, -1, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 0, 30, -1, t));
                    tiles.Add(new Tile(TileLayer.AlwaysFront, 1, 30, -1, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 2, 30, -1, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 3, 30, -1, t));
                    tiles.Add(new Tile(TileLayer.AlwaysFront, 4, 30, -1, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 5, 30, -1, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 0, 31, -1, t));
                    tiles.Add(new Tile(TileLayer.AlwaysFront, 1, 31, -1, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 2, 31, -1, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 3, 31, -1, t));
                    tiles.Add(new Tile(TileLayer.AlwaysFront, 4, 31, -1, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 5, 31, -1, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 0, 32, 1068, t));
                    tiles.Add(new Tile(TileLayer.AlwaysFront, 1, 32, 1067, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 2, 32, 1068, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 3, 32, 1067, t));
                    tiles.Add(new Tile(TileLayer.AlwaysFront, 0, 33, 1070, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 1, 33, 1070, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 2, 33, 1065, t));
                    tiles.Add(new Tile(TileLayer.AlwaysFront, 3, 33, 1065, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 0, 34, 971, t)); tiles.Add(new Tile(TileLayer.AlwaysFront, 1, 34, 996, t));

                    warpYLocA = 29; warpYLocB = 30; warpYLocC = 31;
                    break;
                case 3: // Mining Farm
                    tiles.Add(new Tile(TileLayer.Back, 0, 50, 537, t)); tiles.Add(new Tile(TileLayer.Back, 1, 50, 537, t)); tiles.Add(new Tile(TileLayer.Back, 0, 51, 562, t));
                    tiles.Add(new Tile(TileLayer.Back, 1, 51, 562, t)); tiles.Add(new Tile(TileLayer.Back, 2, 51, 562, t)); tiles.Add(new Tile(TileLayer.Back, 0, 52, 587, t));
                    tiles.Add(new Tile(TileLayer.Back, 1, 52, 587, t)); tiles.Add(new Tile(TileLayer.Back, 0, 53, 587, t)); tiles.Add(new Tile(TileLayer.Back, 1, 53, 587, t));
                    tiles.Add(new Tile(TileLayer.Back, 0, 55, 326, t));

                    tiles.Add(new Tile(TileLayer.Buildings, 0, 47, 467, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 47, 468, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 47, 467, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 48, 493, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 48, 492, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 48, 493, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 49, 518, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 49, 517, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 49, 518, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 50, 543, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 50, 542, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 50, 543, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 51, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 51, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 51, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 52, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 52, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 52, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 53, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 53, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 53, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 54, 175, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 54, 175, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 55, 352, t));

                    tiles.Add(new Tile(TileLayer.Front, 0, 48, -1, t)); tiles.Add(new Tile(TileLayer.Front, 1, 48, -1, t)); tiles.Add(new Tile(TileLayer.Front, 0, 49, -1, t));
                    tiles.Add(new Tile(TileLayer.Front, 1, 49, -1, t)); tiles.Add(new Tile(TileLayer.Front, 2, 49, -1, t)); tiles.Add(new Tile(TileLayer.Front, 0, 50, -1, t));
                    tiles.Add(new Tile(TileLayer.Front, 1, 50, -1, t)); tiles.Add(new Tile(TileLayer.Front, 2, 50, -1, t)); tiles.Add(new Tile(TileLayer.Front, 0, 51, -1, t));
                    tiles.Add(new Tile(TileLayer.Front, 1, 51, -1, t)); tiles.Add(new Tile(TileLayer.Front, 2, 51, -1, t)); tiles.Add(new Tile(TileLayer.Front, 0, 52, -1, t));
                    tiles.Add(new Tile(TileLayer.Front, 0, 53, 414, t)); tiles.Add(new Tile(TileLayer.Front, 1, 53, 413, t)); tiles.Add(new Tile(TileLayer.Front, 2, 53, 438, t));
                    tiles.Add(new Tile(TileLayer.Front, 0, 54, 175, t)); tiles.Add(new Tile(TileLayer.Front, 1, 54, 175, t)); tiles.Add(new Tile(TileLayer.Front, 2, 54, 419, t));

                    warpYLocA = 51; warpYLocB = 52; warpYLocC = 53;
                    break;
                case 4: // Combat Farm
                    tiles.Add(new Tile(TileLayer.Back, 2, 33, 346, t)); tiles.Add(new Tile(TileLayer.Back, 2, 34, 346, t)); tiles.Add(new Tile(TileLayer.Back, 2, 35, 346, t));
                    tiles.Add(new Tile(TileLayer.Back, 0, 38, 537, t)); tiles.Add(new Tile(TileLayer.Back, 1, 38, 537, t)); tiles.Add(new Tile(TileLayer.Back, 2, 38, 618, t));
                    tiles.Add(new Tile(TileLayer.Back, 0, 39, 587, t)); tiles.Add(new Tile(TileLayer.Back, 1, 39, 587, t)); tiles.Add(new Tile(TileLayer.Back, 0, 41, 587, t));
                    tiles.Add(new Tile(TileLayer.Back, 1, 41, 587, t)); tiles.Add(new Tile(TileLayer.Back, 2, 41, 587, t));

                    tiles.Add(new Tile(TileLayer.Buildings, 1, 33, 377, t)); tiles.Add(new Tile(TileLayer.Buildings, 0, 34, 175, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 34, 175, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 2, 34, 444, t)); tiles.Add(new Tile(TileLayer.Buildings, 0, 35, 467, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 35, 468, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 2, 35, 469, t)); tiles.Add(new Tile(TileLayer.Buildings, 0, 36, 492, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 36, 493, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 2, 36, 371, t)); tiles.Add(new Tile(TileLayer.Buildings, 0, 37, 517, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 37, 518, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 2, 37, 519, t)); tiles.Add(new Tile(TileLayer.Buildings, 0, 38, 542, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 38, 543, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 2, 38, 544, t)); tiles.Add(new Tile(TileLayer.Buildings, 0, 39, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 39, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 2, 39, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 0, 40, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 40, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 2, 40, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 0, 41, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 41, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 2, 41, -1, t));

                    tiles.Add(new Tile(TileLayer.Front, 0, 35, -1, t)); tiles.Add(new Tile(TileLayer.Front, 2, 36, 494, t));

                    warpYLocA = 39; warpYLocB = 40; warpYLocC = 41;
                    break;
                default: // Default Farm
                    tiles.Add(new Tile(TileLayer.Back, 0, 38, 175, t)); tiles.Add(new Tile(TileLayer.Back, 1, 38, 175, t)); tiles.Add(new Tile(TileLayer.Back, 0, 43, 537, t));
                    tiles.Add(new Tile(TileLayer.Back, 1, 43, 537, t)); tiles.Add(new Tile(TileLayer.Back, 2, 43, 586, t)); tiles.Add(new Tile(TileLayer.Back, 0, 44, 566, t));
                    tiles.Add(new Tile(TileLayer.Back, 1, 44, 537, t)); tiles.Add(new Tile(TileLayer.Back, 2, 44, 618, t)); tiles.Add(new Tile(TileLayer.Back, 0, 45, 587, t));
                    tiles.Add(new Tile(TileLayer.Back, 1, 45, 473, t)); tiles.Add(new Tile(TileLayer.Back, 0, 46, 587, t)); tiles.Add(new Tile(TileLayer.Back, 1, 46, 587, t));
                    tiles.Add(new Tile(TileLayer.Back, 0, 48, 175, t)); tiles.Add(new Tile(TileLayer.Back, 1, 48, 175, t));

                    tiles.Add(new Tile(TileLayer.Buildings, 0, 39, 175, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 39, 175, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 39, 444, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 40, 446, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 40, 468, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 40, 469, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 41, 492, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 41, 493, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 41, 494, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 42, 517, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 42, 518, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 42, 519, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 43, 542, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 43, 543, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 43, 544, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 44, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 44, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 44, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 45, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 45, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 45, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 46, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 46, -1, t)); tiles.Add(new Tile(TileLayer.Buildings, 2, 46, -1, t));
                    tiles.Add(new Tile(TileLayer.Buildings, 0, 47, 175, t)); tiles.Add(new Tile(TileLayer.Buildings, 1, 47, 175, t)); tiles.Add(new Tile(TileLayer.Buildings, 0, 48, -1, t));

                    tiles.Add(new Tile(TileLayer.Front, 0, 36, -1, t)); tiles.Add(new Tile(TileLayer.Front, 1, 36, -1, t)); tiles.Add(new Tile(TileLayer.Front, 0, 37, -1, t));
                    tiles.Add(new Tile(TileLayer.Front, 1, 37, -1, t)); tiles.Add(new Tile(TileLayer.Front, 0, 38, -1, t)); tiles.Add(new Tile(TileLayer.Front, 1, 38, -1, t));
                    tiles.Add(new Tile(TileLayer.Front, 0, 39, -1, t)); tiles.Add(new Tile(TileLayer.Front, 1, 39, -1, t)); tiles.Add(new Tile(TileLayer.Front, 0, 40, -1, t));
                    tiles.Add(new Tile(TileLayer.Front, 0, 41, -1, t)); tiles.Add(new Tile(TileLayer.Front, 0, 46, 414, t)); tiles.Add(new Tile(TileLayer.Front, 1, 46, 413, t));
                    tiles.Add(new Tile(TileLayer.Front, 2, 46, 438, t)); tiles.Add(new Tile(TileLayer.Front, 0, 47, 175, t)); tiles.Add(new Tile(TileLayer.Front, 1, 47, 175, t));
                    tiles.Add(new Tile(TileLayer.Front, 2, 47, 394, t));

                    warpYLocA = 44; warpYLocB = 45; warpYLocC = 46;
                    break;
            }

            foreach (Tile tile in tiles)
            {
                Layer layer = gl.map.GetLayer(tile.LayerName);
                TileSheet tilesheet = gl.map.GetTileSheet(tile.Tilesheet);

                if (tile.TileID < 0)
                {
                    gl.removeTile(tile.X, tile.Y, tile.LayerName);
                    continue;
                }

                if (layer.Tiles[tile.X, tile.Y] == null || layer.Tiles[tile.X, tile.Y].TileSheet.Id != tile.Tilesheet)
                    layer.Tiles[tile.X, tile.Y] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tile.TileID);
                else
                    gl.setMapTileIndex(tile.X, tile.Y, tile.TileID, layer.Id);
            }

            Game1.getFarm().warps.Add(new Warp(-1, warpYLocA, "FarmExpansion", 46, 4, false));
            Game1.getFarm().warps.Add(new Warp(-1, warpYLocB, "FarmExpansion", 46, 5, false));
            Game1.getFarm().warps.Add(new Warp(-1, warpYLocC, "FarmExpansion", 46, 6, false));

            farmExpansion.warps.Add(new Warp(48, 4, "Farm", 0, warpYLocA, false));
            farmExpansion.warps.Add(new Warp(48, 5, "Farm", 0, warpYLocB, false));
            farmExpansion.warps.Add(new Warp(48, 6, "Farm", 0, warpYLocC, false));

            //Game1.getLocationFromName("ScienceHouse").setTileProperty(8, 19, "Buildings", "Action", "FECarpenter");
            //Game1.getLocationFromName("AnimalShop").setTileProperty(12, 15, "Buildings", "Action", "FEAnimalShop");
        }

        [Obsolete("Obsolete as of 3.1")]
        private void carpenter(Location tileLocation)
        {
            if (Game1.player.currentUpgrade == null)
            {
                foreach (NPC current in Game1.currentLocation.characters)
                {
                    if (current.name.Equals("Robin"))
                    {
                        if (Vector2.Distance(current.getTileLocation(), new Vector2(tileLocation.X, tileLocation.Y)) > 3f)
                            return;

                        current.faceDirection(2);
                        if (Game1.player.daysUntilHouseUpgrade < 0 && !Game1.getFarm().isThereABuildingUnderConstruction() && !farmExpansion.isThereABuildingUnderConstruction())
                        {
                            Response[] answerChoices;
                            if (Game1.player.houseUpgradeLevel < 3)
                            {
                                answerChoices = new Response[]
                                {
                                    new Response("Shop", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop", new object[0])),
                                    new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_UpgradeHouse", new object[0])),
                                    new Response("Construct", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Construct", new object[0])),
                                    new Response("Leave", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave", new object[0]))
                                };
                            }
                            else
                            {
                                answerChoices = new Response[]
                                {
                                    new Response("Shop", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop", new object[0])),
                                    new Response("Construct", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Construct", new object[0])),
                                    new Response("Leave", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave", new object[0]))
                                };
                            }
                            Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu"), answerChoices, carpenter2);
                            return;
                        }
                        Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock(), 0, "Robin");
                        return;
                    }
                }
                if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Tue"))
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_RobinAbsent", new object[0]).Replace('\n', '^'));
            }
        }

        [Obsolete("Obsolete as of 3.1")]
        private void animalShop(Location tileLocation)
        {
            foreach (NPC current in Game1.currentLocation.characters)
            {
                if (current.name.Equals("Marnie"))
                {
                    if (!current.getTileLocation().Equals(new Vector2((float)tileLocation.X, (float)(tileLocation.Y - 1))))
                        return;

                    current.faceDirection(2);
                    Response[] answerChoices = new Response[]
                    {
                        new Response("Supplies", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Supplies", new object[0])),
                        new Response("Purchase", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Animals", new object[0])),
                        new Response("Leave", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Leave", new object[0])),
                    };
                    Game1.currentLocation.createQuestionDialogue("", answerChoices, animalShop2);
                    return;
                }
            }
            if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Tue"))
                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Absent", new object[0]).Replace('\n', '^'));
        }

        [Obsolete("Obsolete as of 3.1")]
        private void carpenter2(StardewValley.Farmer who, string whichAnswer)
        {
            switch (whichAnswer)
            {
                case "Shop":
                    Game1.player.forceCanMove();
                    Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock(), 0, "Robin");
                    return;
                case "Upgrade":
                    switch (Game1.player.houseUpgradeLevel)
                    {
                        case 0:
                            Game1.currentLocation.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse1", new object[0])), Game1.currentLocation.createYesNoResponses(), "upgrade");
                            return;
                        case 1:
                            Game1.currentLocation.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse2", new object[0])), Game1.currentLocation.createYesNoResponses(), "upgrade");
                            return;
                        case 2:
                            Game1.currentLocation.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:ScienceHouse_Carpenter_UpgradeHouse3", new object[0])), Game1.currentLocation.createYesNoResponses(), "upgrade");
                            return;
                        default:
                            return;
                    }
                case "Construct":
                    Game1.activeClickableMenu = new FECarpenterMenu(this);
                    return;
                default:
                    return;
            }
        }

        [Obsolete("Obsolete as of 3.1")]
        private void animalShop2(StardewValley.Farmer who, string whichAnswer)
        {
            switch (whichAnswer)
            {
                case "Supplies":
                    Game1.activeClickableMenu = new ShopMenu(Utility.getAnimalShopStock(), 0, "Marnie");
                    return;
                case "Purchase":
                    Game1.player.forceCanMove();
                    Game1.activeClickableMenu = new FEPurchaseAnimalsMenu(this);
                    return;
                default:
                    return;
            }
        }

        private void robinHammerSound(StardewValley.Farmer who)
        {
            if (Game1.currentLocation.Equals(robin.currentLocation) && Utility.isOnScreen(robin.position, Game1.tileSize * 4))
            {
                Game1.playSound((Game1.random.NextDouble() < 0.1) ? "clank" : "axchop");
                helper.Reflection.GetPrivateField<int>(robin, "shakeTimer").SetValue(250);
            }
        }

        private void robinVariablePause(StardewValley.Farmer who)
        {
            if (Game1.random.NextDouble() < 0.4)
            {
                robin.sprite.currentAnimation[robin.sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(27, 300, false, false, new AnimatedSprite.endOfAnimationBehavior(robinVariablePause), false);
                return;
            }
            if (Game1.random.NextDouble() < 0.25)
            {
                robin.sprite.currentAnimation[robin.sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(23, Game1.random.Next(500, 4000), false, false, new AnimatedSprite.endOfAnimationBehavior(robinVariablePause), false);
                return;
            }
            robin.sprite.currentAnimation[robin.sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(27, Game1.random.Next(1000, 4000), false, false, new AnimatedSprite.endOfAnimationBehavior(robinVariablePause), false);
        }

        internal Farm swapFarm(Farm currentFarm)
        {
            return expansionSelected(currentFarm.Name) ? Game1.getFarm() : farmExpansion;
        }

        internal bool expansionSelected(string currentFarmName)
        {
            return currentFarmName.Equals("FarmExpansion");
        }
    }
}
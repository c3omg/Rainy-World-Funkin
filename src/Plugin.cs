using BepInEx;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RWF
{
    [BepInPlugin(MOD_ID, "Rainy World Funkin'", "0.2.1")]
    class Plugin : BaseUnityPlugin
    {
        public static ProcessManager.ProcessID FunkinMenu => new ProcessManager.ProcessID("FunkinMenu", register: true);
        public static ProcessManager.ProcessID FunkinRestart => new ProcessManager.ProcessID("FunkinRestart", register: true);

        public static SoundID[] missnote_sounds;

        public static SoundID fnfDeath = new SoundID("FNFDeath", true);
        public static SoundID fnfRestart = new SoundID("FNFRestart", true);
        //FNFRestart 

        public static ProcessManager.ProcessID FunkinFreeplayMenu => new ProcessManager.ProcessID("FunkinFreeplayMenu", register: true);
        private const string MOD_ID = "silky.rwf";

        public static Dictionary<string, string> Songs = new Dictionary<string, string>();
        internal static float camGameScale = 1f;
        internal static float camHUDScale = 1f;

        internal static string SelectedSong = "blammed";

        // Add hooks

        private static void LoadFunkin()
        {
            Debug.Log("Attempting to load Funkin files");

            string[] files = AssetManager.ListDirectory("funkin", includeAll: true);

            if (files != null)
            {
                Debug.Log("Funkin file exists, loading the shit in it");

                Debug.Log("Checking for Charts");

                string path1 = "funkin/charts";

                //" + Path.DirectorySeparatorChar.ToString() + "
                string[] charts = AssetManager.ListDirectory(path1, includeAll: true);

                if (charts != null)
                {

                    string soog = "";

                    foreach (var chartName in charts.Where(x => x.EndsWith(".json")))
                    {

                        string path = chartName;

                        path = path.Replace('/', Path.DirectorySeparatorChar);

                        string chart = Path.GetFileNameWithoutExtension(path);

                        if (chart == null)
                        {
                            Debug.Log("Chart was either empty, or doesnt exist");
                            continue;
                        }
                        if (Songs.ContainsKey(chart)) continue;

                        Debug.Log("Chart " + chart);

                        Songs.Add(chart, path);

                        if (soog == "")
                            soog = chart;
                        else
                            soog = soog + ", " + chart;

                    }

                    Debug.Log("added " + Songs.Count + " Songs");
                    Debug.Log("Songs: " + soog);

                }
                else
                {
                    Debug.Log("Charts folder was empty");
                }

            }
            else
            {
                Debug.Log("Funkin file does not exist, uh oh");
            }
        }

        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
            On.Menu.MainMenu.ctor += MainMenu_ctor;
            IL.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;

            RWF.FunkinMenu.OnBeatHit += FunkinMenu_OnBeatHit;
        }

        private void FunkinMenu_OnBeatHit(FunkinMenu self, int curBeat)
        {
            
            if (self.SONG.Name.ToLower() == "disk")
            {
                if (curBeat == 32)
                {
                    self.camBounceSpeed = 1;
                    self.camStrengh = 1.5f;
                }
            }
            else if (self.SONG.Name.ToLower() == "fazfuck news")
            {
                if (curBeat == 80)
                {
                    self.camBounceSpeed = 1;
                }
            }

        }

        private static void ProcessManager_PostSwitchMainProcess(ILContext il)
        {
            var cursor = new ILCursor(il);


            try
            {
                if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdarg(0),
                                                         i => i.MatchLdfld<ProcessManager>("oldProcess"),
                                                         i => i.MatchLdarg(0),
                                                         i => i.MatchLdfld<ProcessManager>("currentMainLoop"),
                                                         i => i.MatchCallOrCallvirt<MainLoopProcess>("CommunicateWithUpcomingProcess")))
                {
                    throw new Exception("Failed to match IL for ProcessManager_PostSwitchMainProcess!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception when matching IL for ProcessManager_PostSwitchMainProcess!");
                Debug.LogException(ex);
                Debug.LogError(il);
                throw;
            }

            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate((ProcessManager self, ProcessManager.ProcessID ID) =>
            {
                if (ID == Plugin.FunkinFreeplayMenu)
                {
                    self.currentMainLoop = new FunkinFreeplayMenu(self);
                }
                else if (ID == Plugin.FunkinMenu)
                {
                    self.currentMainLoop = new FunkinMenu(self);
                }
                else if (ID == Plugin.FunkinRestart)
                {
                    self.currentMainLoop = new FunkinMenu(self);
                }
            });

        }

        private void MainMenu_ctor(On.Menu.MainMenu.orig_ctor orig, Menu.MainMenu self, ProcessManager manager, bool showRegionSpecificBkg)
        {
            orig(self, manager, showRegionSpecificBkg);

            float buttonWidth = Menu.MainMenu.GetButtonWidth(self.CurrLang);
            Vector2 pos = new Vector2(683f - buttonWidth / 2f, 0f);
            Vector2 size = new Vector2(buttonWidth, 30f);

            void ClickedReturnToIntro()
            {
                if (self.manager.musicPlayer != null)
                {
                    self.manager.musicPlayer.FadeOutAllSongs(5f);
                }
                self.manager.RequestMainProcessSwitch(Plugin.FunkinFreeplayMenu);
                self.PlaySound(SoundID.MENU_Switch_Page_Out);
            }

            var bruh = new Menu.SimpleButton(self, self.pages[0], self.Translate("PLAY FNF"), "FUNKIN", pos, size * new Vector2(1f, 1));
            self.AddMainMenuButton(bruh, new Action(ClickedReturnToIntro), 4);

        }

        private void LoadFunkinAtlasIamge(string path)
        {
            Futile.atlasManager.LoadAtlas("funkin/images/" + path);
        }

        private bool init = false;

        private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);

            if (init) return;

            init = true;

            LoadFunkin();

            string[] array = new string[]
            {
                "flx_bar",
                "notes/FNF_Note",
                "notes/FNF_Note_Splash",
                "stages/outskirts",
                "characters/hunter",
            };

            foreach (string item in array)
            {
                LoadFunkinAtlasIamge(item);
            }


        }

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {

            missnote_sounds = new SoundID[]
            {
                new SoundID("Missnote1", true),
                new SoundID("Missnote2", true),
                new SoundID("Missnote3", true),
            };

            fnfRestart = new SoundID("FNFRestart", true);

            MachineConnector.SetRegisteredOI(MOD_ID, new RWF_Options());

        }
    }

    internal class RWF_Options : OptionInterface
    {

        public RWF_Options() : base()
        {
            GhostTapping = this.config.Bind("fnfghosttapping", true);

            key_note_left = this.config.Bind("fnfkeynoteleft", KeyCode.A);
            key_note_down = this.config.Bind("fnfkeynotedown", KeyCode.S);
            key_note_up = this.config.Bind("fnfkeynoteup", KeyCode.W);
            key_note_right = this.config.Bind("fnfkeynoteright", KeyCode.D);

            key_note_left_alt = this.config.Bind("fnfkeynoteleft_alt", KeyCode.LeftArrow);
            key_note_down_alt = this.config.Bind("fnfkeynotedown_alt", KeyCode.DownArrow);
            key_note_up_alt = this.config.Bind("fnfkeynoteup_alt", KeyCode.UpArrow);
            key_note_right_alt = this.config.Bind("fnfkeynoteright_alt", KeyCode.RightArrow);

        }

        public override void Initialize()
        {
            this.Tabs = new OpTab[]
            {
                new OpTab(this, "Input"),
                new OpTab(this, "Gameplay"),
            };

            OpCheckBox OpGhostTap = new OpCheckBox(GhostTapping, new Vector2(45f, 530f));

            this.Tabs[1].AddItems(new UIelement[]
            {
                OpGhostTap,
                new OpLabel(45f, 500f, OptionInterface.Translate("Ghost Tap"), false)
                {
                    bumpBehav = OpGhostTap.bumpBehav,
                    description = OptionInterface.Translate("Toggle if only modded intro rolls show up.")
                },
            });

            OpKeyBinder OpLeft = new OpKeyBinder(key_note_left, new Vector2(45f, 530f), new Vector2(125, 25));
            OpKeyBinder OpDown = new OpKeyBinder(key_note_down, new Vector2(45f + (125f), 530f), new Vector2(125, 25));
            OpKeyBinder OpUp = new OpKeyBinder(key_note_up, new Vector2(45f + (125f * 2), 530f), new Vector2(125, 25));
            OpKeyBinder OpRight = new OpKeyBinder(key_note_right, new Vector2(45f + (125f * 3), 530f), new Vector2(125, 25));

            this.Tabs[0].AddItems(new UIelement[]
            {
                OpLeft,
                OpDown,
                OpUp,
                OpRight,
                new OpLabel(45f, 500f, OptionInterface.Translate("Left Key"), false)
                {
                    bumpBehav = OpLeft.bumpBehav,
                },
                new OpLabel(45f + (125f * 1), 500f, OptionInterface.Translate("Down Key"), false)
                {
                    bumpBehav = OpDown.bumpBehav,
                },
                new OpLabel(45f + (125f * 2), 500f, OptionInterface.Translate("Up Key"), false)
                {
                    bumpBehav = OpUp.bumpBehav,
                },
                new OpLabel(45f + (125f * 3), 500f, OptionInterface.Translate("Right Key"), false)
                {
                    bumpBehav = OpRight.bumpBehav,
                },
            });

        }

        public static Configurable<bool> GhostTapping;
        public static Configurable<KeyCode> key_note_left;
        public static Configurable<KeyCode> key_note_down;
        public static Configurable<KeyCode> key_note_up;
        public static Configurable<KeyCode> key_note_right;

        public static Configurable<KeyCode> key_note_left_alt;
        public static Configurable<KeyCode> key_note_down_alt;
        public static Configurable<KeyCode> key_note_up_alt;
        public static Configurable<KeyCode> key_note_right_alt;

    }

}
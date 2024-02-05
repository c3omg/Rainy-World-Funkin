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
    [BepInPlugin(MOD_ID, "Rainy World Funkin': c3 Edition", "0.4.13")]
    public class Plugin : BaseUnityPlugin
    {
        public static ProcessManager.ProcessID FunkinMenu => new ProcessManager.ProcessID("FunkinMenu", register: true);
        public static ProcessManager.ProcessID FunkinRestart => new ProcessManager.ProcessID("FunkinRestart", register: true);
        public static ProcessManager.ProcessID FunkinCharacterEditor => new ProcessManager.ProcessID("FunkinCharacterEditor", register: true);

        public static SoundID[] introSounds = new SoundID[]
        {
            new SoundID("FNFIntro1", true),
            new SoundID("FNFIntro2", true),
            new SoundID("FNFIntro3", true),
            new SoundID("FNFIntroGo", true),
        };

        //yo mama so fat, she was thought to be a fucking bump in the road when she died

        public static SoundID fnfDeath = new SoundID("FNFDeath", true);
        public static SoundID fnfRestart = new SoundID("FNFRestart", true);
        //FNFRestart 

        public static ProcessManager.ProcessID FunkinFreeplayMenu => new ProcessManager.ProcessID("FunkinFreeplayMenu", register: true);
        private const string MOD_ID = "c3.rwf";

        public static Dictionary<string, string> Songs = new Dictionary<string, string>();
        public static float camGameScale = 1f;
        public static float camHUDScale = 1f;

        public static string SelectedSong = "blammed";

        private RWF_Options options;

        // Add hooks

        public Plugin()
        {
            options = new RWF_Options();
        }

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
            RWF.FunkinMenu.OnUpdate += FunkinMenu_OnUpdate;
            RWF.FunkinMenu.OnCreate += FunkinMenu_OnCreate;
            RWF.FunkinMenu.OnEnemyHit += this.FunkinMenu_OnEnemyHit;
        }

        private void FunkinMenu_OnCreate(FunkinMenu self)
        {
            if (self.SONG.Name.ToLower() == "ice cube")
            {
                self.stage.camSpeed = 0.005f;
            }
        }

        private void FunkinMenu_OnEnemyHit(FunkinMenu self, Swagshit.Note daNote)
        {
            bool flag = self.SONG.Name == "Ballistic (HQ)";
            if (flag)
            {
                self.Add_Camera_Zoom(0.015f, 0.03f);
            }
        }

        private void FunkinMenu_OnUpdate(FunkinMenu self)
        {
            if (self.SONG.Name.ToLower() == "disk")
            {

                if (Conductor.curBeat >= 32)
                {
                    var bruh = 6f;
                    var shit = 2.5f;

                    if (Conductor.curBeat % 2 == 0)
                        self.pages[0].Container.RotateAroundPointAbsolute(self.manager.rainWorld.screenSize / 2, Mathf.Lerp(-bruh, -shit, FlxEase.backOut(self.curDecBeat % 1)));
                    else
                        self.pages[0].Container.RotateAroundPointAbsolute(self.manager.rainWorld.screenSize / 2, Mathf.Lerp(bruh, shit, FlxEase.backOut(self.curDecBeat % 1)));
                }

            }

            if (self.SONG.Name.ToLower() == "ice cube" && Conductor.curBeat >= 1)
            {

                var up = 3.75f;
                var down = 3.25f;
                var fuckme = 25f;
                var fuckyou = 20f;

                if (Conductor.curBeat % 2 == 0)
                {
                    self.boyfriend.sprite.scaleX = Mathf.Lerp(up, self.boyfriend.size, FlxEase.circOut(self.curDecBeat % 1));
                    self.boyfriend.sprite.scaleY = Mathf.Lerp(down, self.boyfriend.size, FlxEase.circOut(self.curDecBeat % 1));
                    self.dad.sprite.scaleX = Mathf.Lerp(down, self.dad.size, FlxEase.circOut(self.curDecBeat % 1));
                    self.dad.sprite.scaleY = Mathf.Lerp(up, self.dad.size, FlxEase.circOut(self.curDecBeat % 1));

                    self.boyfriend.spriteOffset.y = Mathf.Lerp(-fuckyou, 0, FlxEase.circOut(self.curDecBeat % 1));
                    self.dad.spriteOffset.y = Mathf.Lerp(fuckme, 0, FlxEase.circOut(self.curDecBeat % 1));
                }
                else
                {
                    self.boyfriend.sprite.scaleX = Mathf.Lerp(down, self.boyfriend.size, FlxEase.circOut(self.curDecBeat % 1));
                    self.boyfriend.sprite.scaleY = Mathf.Lerp(up, self.boyfriend.size, FlxEase.circOut(self.curDecBeat % 1));
                    self.dad.sprite.scaleX = Mathf.Lerp(up, self.dad.size, FlxEase.circOut(self.curDecBeat % 1));
                    self.dad.sprite.scaleY = Mathf.Lerp(down, self.dad.size, FlxEase.circOut(self.curDecBeat % 1));

                    self.dad.spriteOffset.y = Mathf.Lerp(-fuckme, 0, FlxEase.circOut(self.curDecBeat % 1));
                    self.boyfriend.spriteOffset.y = Mathf.Lerp(fuckyou, 0, FlxEase.circOut(self.curDecBeat % 1));
                }
                    

            }

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
            else if (self.SONG.Name.ToLower() == "Ballistic (HQ)")
            {
                if (curBeat == 80)
                {
                    if (self.boyfriend.curAnim == "idle")
                    {
                        self.boyfriend.PlayAnimation("idle", true);
                    }
                    if (self.dad.curAnim == "idle")
                    {
                        self.dad.PlayAnimation("idle", true);
                    }
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
                else if (ID == Plugin.FunkinCharacterEditor)
                {
                    self.currentMainLoop = new FunkinCharacterEditor(self);
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
                //"stages/outskirts", 
            };

            foreach (string item in array)
            {
                LoadFunkinAtlasIamge(item);
            }


        }

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            MachineConnector.SetRegisteredOI(MOD_ID, options);

            introSounds = new SoundID[]
            {
                new SoundID("FNFIntro1", true),
                new SoundID("FNFIntro2", true),
                new SoundID("FNFIntro3", true),
                new SoundID("FNFIntroGo", true),
            };

            fnfRestart = new SoundID("FNFRestart", true);


        }
    }

    public class RWF_Options : OptionInterface
    {
        public RWF_Options() : base()
        {
            GhostTapping = this.config.Bind("fnfghosttapping", true);

            botplay = this.config.Bind("fnfbotplay", false);

            downscroll = this.config.Bind("fnfdownscroll", false);

            HoldNoteThickness = this.config.Bind("fnfnotethickness", 1f);

            kN_Left = this.config.Bind<KeyCode>("fnfkeynoteleft", KeyCode.A);
            kN_Down = this.config.Bind<KeyCode>("fnfkeynotedown", KeyCode.S);
            kN_Up = this.config.Bind<KeyCode>("fnfkeynoteup", KeyCode.W);
            kN_Right = this.config.Bind<KeyCode>("fnfkeynoteright", KeyCode.D);

            kN_LeftAlt = this.config.Bind<KeyCode>("fnfkeynoteleft_alt", KeyCode.LeftArrow);
            kN_DownAlt = this.config.Bind<KeyCode>("fnfkeynotedown_alt", KeyCode.DownArrow);
            kN_UpAlt = this.config.Bind<KeyCode>("fnfkeynoteup_alt", KeyCode.UpArrow);
            kN_RightAlt = this.config.Bind<KeyCode>("fnfkeynoteright_alt", KeyCode.RightArrow);
        }

        public override void Initialize()
        {
            //OpTab opInput = new OpTab(this, "Input");
            OpTab opGameplay = new OpTab(this, "Gameplay");

            OpCheckBox OpGhostTap = new OpCheckBox(GhostTapping, new Vector2(45f, 530f));
            OpCheckBox OpBotplay = new OpCheckBox(botplay, new Vector2(45f, 500f));
            OpCheckBox OpDownscroll = new OpCheckBox(downscroll, new Vector2(45f, 470f));
            OpFloatSlider OpThickness = new OpFloatSlider(HoldNoteThickness, new Vector2(45f, 420f), 260, 2, false);

            this.Tabs = new OpTab[]
            {
                //opInput,
                opGameplay
            };

            //this.inputElement = new UIelement[] // rain world has a seizure every fuckin time i try to add a KeyBinder so i guess thats out for a while
            //{
            //    new OpKeyBinder(kN_Left, new Vector2(45f, 530f), new Vector2(125, 25)),
            //    new OpLabel(45f, 500f, OptionInterface.Translate("Left Key"), false){ alignment = FLabelAlignment.Center },

            //    new OpKeyBinder(kN_Down, new Vector2(175f, 530f), new Vector2(125, 25)),
            //    new OpLabel(175f, 500f, OptionInterface.Translate("Down Key"), false){ alignment = FLabelAlignment.Center },

            //    new OpKeyBinder(kN_Up, new Vector2(305f, 530f), new Vector2(125, 25)),
            //    new OpLabel(305f, 500f, OptionInterface.Translate("Up Key"), false){ alignment = FLabelAlignment.Center },

            //    new OpKeyBinder(kN_Right, new Vector2(435f, 530f), new Vector2(125, 25)),
            //    new OpLabel(435f, 500f, OptionInterface.Translate("Right Key"), false){ alignment = FLabelAlignment.Center },
            //};
            //opInput.AddItems(inputElement);

            this.gameplayElement = new UIelement[]
            {
                OpGhostTap,
                new OpLabel(75f, 530f, OptionInterface.Translate("Ghost Tap"), false){ bumpBehav = OpGhostTap.bumpBehav, },

                OpBotplay,
                new OpLabel(75f, 500f, OptionInterface.Translate("Botplay"), false){ bumpBehav = OpBotplay.bumpBehav, },

                OpDownscroll,
                new OpLabel(75f, 470f, OptionInterface.Translate("Downscroll"), false) { bumpBehav = OpDownscroll.bumpBehav, },

                OpThickness,
                new OpLabel(45f, 445f, OptionInterface.Translate("Hold Note Thickness"), false){ bumpBehav = OpThickness.bumpBehav, },

            };
            opGameplay.AddItems(gameplayElement);

        }

        public static Configurable<bool> GhostTapping;

        public static Configurable<bool> botplay;

        public static Configurable<bool> downscroll;

        public static Configurable<float> HoldNoteThickness;

        public static Configurable<KeyCode>[] key_note;
        public static Configurable<KeyCode>[] key_note_alt;

        // kill me fr
        public static Configurable<KeyCode> kN_Left;
        public static Configurable<KeyCode> kN_LeftAlt;
        public static Configurable<KeyCode> kN_Down;
        public static Configurable<KeyCode> kN_DownAlt;
        public static Configurable<KeyCode> kN_Up;
        public static Configurable<KeyCode> kN_UpAlt;
        public static Configurable<KeyCode> kN_Right;
        public static Configurable<KeyCode> kN_RightAlt;

        //private UIelement[] inputElement;
        private UIelement[] gameplayElement;
    }

}
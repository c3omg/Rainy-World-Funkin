using Menu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static MonoMod.InlineRT.MonoModRule;

namespace RWF.Swagshit
{

    public class CharacterJSON
    {

        public List<Animation> animations = new List<Animation>();
        public float scale = 2.5f;
        public string icon = "tut_slugger";
        public bool isFacingRight = false;
        public string character_image = "characters/tut_scug";
        public string char_name;
        public float[] GlobalPosition = new float[] { 0, 0 };
        public float singDuration = 4f;
        public float[] CameraOffset = new float[] { 0, 0 };

        public CharacterJSON()
        {
        }

        public class Animation
        {
            public string anim;
            public string elementName = "Futile_White";
            public bool loops = false;
            public int framerate = 40;
        }

    }

    public class Character : FunkinSprite
    {
        public Dictionary<string, string> animations = new Dictionary<string, string> { };
        private Dictionary<string, int> FRAME_MAXS = new Dictionary<string, int> { };
        private Dictionary<string, int> ANIM_FRATE = new Dictionary<string, int> { };
        private Dictionary<string, bool> ANIM_LOOPABLES = new Dictionary<string, bool> { };

        public Dictionary<string, string> dirAnimations = new Dictionary<string, string> { };

        public string curAnim = "";
        public string iconName = "";
        public int frame = 0;
        public int framerate = 40;
        private int frameCounter = 0;
        public CharacterJSON json;
        private int failedattempts = 0;
        public Vector2 CameraOffset = Vector2.zero;
        public bool finished = true;
        public float holdtimer = 0f;
        public float singDuration = 4f;

        public string Name = "bruhmoment";

        public bool stunned = false;

        public bool isPlayer = false;

        public bool flipped = false;
        public float size = 1.0f;

        public UnityEngine.Vector2 offsetPosition = Vector2.zero;

        /// <summary>
        /// Attempts to Auto the FRAMES_MAXS part of your animations
        /// </summary>
        /// <param name="animName"></param>
        /// <returns></returns>
        public static int AttemptToGuessMaxNumberOfFrames(string animName = "idle") // it works well enough
        {
            if (Futile.atlasManager.DoesContainElementWithName(animName + "_0"))
            {

                int limit = 0;

                for (int i = 0; i < 9999; i++)
                {
                    if (!Futile.atlasManager.DoesContainElementWithName(animName + "_" + i)) break;
                    limit++;
                }

                return limit;

            }

            return 0;
        }

        public void LoadCharacterDataFromRawData(string rawData)
        {
            try
            {
                CharacterJSON Data = JsonConvert.DeserializeObject<CharacterJSON>(rawData);
                this.json = Data;
                string imagePath = AssetManager.ResolveFilePath("funkin/images/" + Data.character_image);
                bool flag = !Futile.atlasManager.DoesContainAtlas(Path.GetFileNameWithoutExtension(imagePath));
                if (flag)
                {
                    Futile.atlasManager.LoadAtlas("funkin/images/" + Data.character_image);
                }
                foreach (CharacterJSON.Animation v in Data.animations)
                {
                    this.AddAnimation(v.anim, v.elementName, Character.AttemptToGuessMaxNumberOfFrames(v.elementName), v.framerate, v.loops);
                }
                this.sprite.scale = Data.scale;
                this.size = Data.scale;
                this.flipped = Data.isFacingRight;
                this.singDuration = Data.singDuration;
                this.iconName = Data.icon;
                this.CameraOffset = new Vector2(Data.CameraOffset[0], Data.CameraOffset[1]);
                this.offsetPosition = new Vector2(Data.GlobalPosition[0], Data.GlobalPosition[1]);
                bool flag2 = Data.char_name != null;
                if (flag2)
                {
                    this.Name = Data.char_name;
                }
                bool flag3 = this.flipped;
                if (flag3)
                {
                    this.sprite.scaleX *= -1f;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(string.Format("RWF MODDING: HAD A FAIL WHILE LOADING RAPPER || Exception logged : {0}", ex));
                bool flag4 = this.failedattempts < 6;
                if (flag4)
                {
                    this.LoadCharacterDataFromRawData(File.ReadAllText(AssetManager.ResolveFilePath("funkin/characters/tut_slugger.json")));
                    this.failedattempts++;
                }
            }
        }

        public Character(Menu.Menu menu, Menu.MenuObject owner) : base(menu, owner) // why would you ever want to create a character manually????
        {
        }


        /// <summary>
        /// Creates a Character via a Character JSON file
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="owner"></param>
        /// <param name="charData"></param>
        public Character(Menu.Menu menu, Menu.MenuObject owner, string charData) : base(menu, owner)
        {
            this.LoadCharacterDataFromRawData(charData);
        }

        public void AddAnimation(string name, string elementName, int frames = 1, int fps = 40, bool loops = false)
        {
            if (!animations.ContainsKey(name))
            {
                this.animations.Add(name, elementName);
                this.FRAME_MAXS.Add(name, frames);
                this.ANIM_FRATE.Add(name, fps);
                ANIM_LOOPABLES.Add(name, loops);
            }
        }

        public void PlayAnimation(string name, bool forced = false)
        {
            
            if (this.flipped)
            {
                if (name.StartsWith("left")) name = name.Replace("left", "right");
                else if (name.StartsWith("right")) name = name.Replace("right", "left");
            }

            if (!animations.ContainsKey(name)) return;
            if (!Futile.atlasManager.DoesContainElementWithName(animations[name] + "_0")) return;
            if (!forced && curAnim == name && !finished) return;

            this.frameCounter = 0;
            finished = false;
            this.frame = 0;
            this.framerate = ANIM_FRATE[name];
            this.curAnim = name;
        }

        public override void Update()
        {
            base.Update();

            if (Time.deltaTime > 1 / menu.framesPerSecond)
                holdtimer += Time.deltaTime;
            else
                holdtimer += 1 / menu.framesPerSecond;

            if (animations.ContainsKey(curAnim))
            {
                this.sprite.element = Futile.atlasManager.GetElementWithName(animations[curAnim] + "_" + frame);

                if (frameCounter == (60 / this.framerate))
                {

                    if (!ANIM_LOOPABLES[curAnim])
                    {
                        if (this.frame != FRAME_MAXS[curAnim] - 1)
                        {
                            this.frame++;
                            finished = false;
                        }
                    }
                    else
                    {

                        if (this.frame == FRAME_MAXS[curAnim] - 1 && ANIM_LOOPABLES[curAnim])
                            this.frame = 0;
                        else
                            this.frame++;
                    }

                    this.frameCounter = 0;
                }

                if (ANIM_LOOPABLES[curAnim])
                    frameCounter++;
                else if (this.frame != FRAME_MAXS[curAnim] - 1)
                    frameCounter++;
                else
                    finished = true;
            }

        }

    }

    public class HealthIcon : FunkinSprite
    {

        private static Vector2 wantedsize = new(1.25f, 1.25f);
        public Vector2 Size = wantedsize;
        public bool flipped = false;

        public HealthIcon(Menu.Menu menu, MenuObject owner, string elementName) : base(menu, owner)
        {

            if (!File.Exists(AssetManager.ResolveFilePath("funkin/images/icons/icon-" + elementName + ".png")))
            {
                elementName = "tut_slugger";
            }

            if (!Futile.atlasManager.DoesContainElementWithName("funkin/images/icons/icon-" + elementName))
            {
                Futile.atlasManager.LoadImage("funkin/images/icons/icon-" + elementName);
            }

            sprite.element = Futile.atlasManager.GetElementWithName("funkin/images/icons/icon-" + elementName);

            sprite.SetAnchor(0.7f, 0.5f);

            scrollFactor = Vector2.zero;
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            sprite.scaleX = flipped ? -Size.x : Size.x;
            sprite.scaleY = Size.y;
        }

    }

    public class FLX_BAR : FunkinSprite
    {

        public static UnityEngine.Vector2 wantedsize = new(15, 3.5f);
        public UnityEngine.Vector2 Size = wantedsize;
        public bool flipped = false;

        public FLX_BAR(Menu.Menu menu, MenuObject owner) : base(menu, owner)
        {

            sprite.element = Futile.atlasManager.GetElementWithName("FLXGBAR");

            scrollFactor = Vector2.zero;

            this.IsPartOfHUD = true;

        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            this.Size = UnityEngine.Vector2.Lerp(this.Size, wantedsize, 0.3f);

            sprite.scaleX = flipped ? -Size.x : Size.x;
            sprite.scaleY = Size.y;
        }
    }

}

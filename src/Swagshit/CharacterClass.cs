using Menu;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RWF.Swagshit
{

    public class CharacterJSON
    {

        public List<string[]> animations = new List<string[]> { };
        public float scale = 2.5f;
        public string icon = "tut_slugger";
        public bool isFacingRight = false;
        public string character_image = "characters/tut_scug";
        public string char_name;
        public float[] GlobalPosition = new float[] { 0, 0 };
        public float[] CameraOffset = new float[] { 0, 0 };

        public CharacterJSON()
        {
        }

    }

    public class Character : Menu.MenuObject
    {
        private Dictionary<string, string> animations = new Dictionary<string, string> { };
        private Dictionary<string, int> FRAME_MAXS = new Dictionary<string, int> { };
        private Dictionary<string, int> ANIM_FRATE = new Dictionary<string, int> { };
        private Dictionary<string, bool> ANIM_LOOPABLES = new Dictionary<string, bool> { };

        public Dictionary<string, string> dirAnimations = new Dictionary<string, string> { };

        public string curAnim = "";
        public string iconName = "";
        public int frame = 0;
        public int framerate = 40;
        private int frameCounter = 0;
        public Vector2 pos = Vector2.zero;
        public Vector2 CameraOffset = Vector2.zero;
        public bool finished = true;

        public string Name = "bruhmoment";

        public bool stunned = false;

        public bool isPlayer = false;

        public bool flipped = false;
        public float size = 1.0f;
        public Vector2 spriteOffset = Vector2.zero;

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

            var Data = JsonConvert.DeserializeObject<CharacterJSON>(rawData);

            var imagePath = AssetManager.ResolveFilePath("funkin/images/" + Data.character_image);

            if (!Futile.atlasManager.DoesContainAtlas(Path.GetFileNameWithoutExtension(imagePath)))
            { // i have a bad feeling of this, something tells me this isnt good, but oh well, allows for custom atlas without a dll file
                Futile.atlasManager.LoadAtlas("funkin/images/" + Data.character_image);
            }

            foreach (string[] v in Data.animations)
            {

                this.AddAnimation(v[0], v[1], AttemptToGuessMaxNumberOfFrames(v[1]), int.Parse(v[2]), bool.Parse(v[3]));

            }

            this.sprite.scale = Data.scale;
            this.size = Data.scale;
            this.flipped = Data.isFacingRight;
            this.iconName = Data.icon;
            this.CameraOffset = new(Data.CameraOffset[0], Data.CameraOffset[1]);
            this.offsetPosition = new(Data.GlobalPosition[0], Data.GlobalPosition[1]);

            if (Data.char_name != null)
                this.Name = Data.char_name;

            if (this.flipped)
                this.sprite.scaleX *= -1f;

        }

        public Character(Menu.Menu menu, Menu.MenuObject owner) : base(menu, owner) // why would you ever want to create a character manually????
        {
            this.sprite = new("Futile_White");

            this.Container.AddChild(sprite);
        }


        /// <summary>
        /// Creates a Character via a Character JSON file
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="owner"></param>
        /// <param name="charData"></param>
        public Character(Menu.Menu menu, Menu.MenuObject owner, string charData) : base(menu, owner)
        {
            this.sprite = new("Futile_White");

            this.Container.AddChild(sprite);

            this.LoadCharacterDataFromRawData(charData);
        }

        public void Destroy()
        {
            if (this.sprite != null)
                this.Container.RemoveChild(this.sprite);

            if (this.owner != null)
                (this.page as Page).subObjects.Remove(this);
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
                if (name == "left") name = "right";
                else if (name == "right") name = "left";
                else if (name == "left-miss") name = "right-miss";
                else if (name == "right-miss") name = "left-miss";
            }

            if (!animations.ContainsKey(name)) return;
            if (!Futile.atlasManager.DoesContainElementWithName(animations[name] + "_0")) return;
            if (!forced && curAnim == name &&!finished) return;

            this.frameCounter = 0;
            finished = false;
            this.frame = 0;
            this.framerate = ANIM_FRATE[name];
            this.curAnim = name;
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            this.sprite.SetPosition((this.menu as FunkinMenu).GetPositionBasedOffCamScale(this.pos + this.spriteOffset, false, isPlayer ? (this.menu as FunkinMenu).stage.bfscroll : (this.menu as FunkinMenu).stage.dadscroll));

            if (animations.ContainsKey(curAnim))
            {
                this.sprite.element = Futile.atlasManager.GetElementWithName(animations[curAnim] + "_" + frame);

                if (frameCounter == (40 / this.framerate))
                {

                    if (!ANIM_LOOPABLES[curAnim])
                        if (this.frame != FRAME_MAXS[curAnim] - 1)
                        {
                            this.frame++;
                            finished = false;
                        }
                        else if (this.frame == FRAME_MAXS[curAnim] - 1)
                            finished = true;
                        else
                            this.frame++;

                    this.frameCounter = 0;
                }

                if (this.frame == FRAME_MAXS[curAnim] && ANIM_LOOPABLES[curAnim])
                    this.frame = 0;

                if (ANIM_LOOPABLES[curAnim])
                    frameCounter++;
                else
                {
                    if (this.frame != FRAME_MAXS[curAnim])
                        frameCounter++;
                }
            }

        }

        public FSprite sprite;

    }

    public class HealthIcon : MenuObject
    {

        public UnityEngine.Vector2 pos;
        private static UnityEngine.Vector2 wantedsize = new(1.25f, 1.25f);
        public UnityEngine.Vector2 Size = wantedsize;
        public bool flipped = false;

        public HealthIcon(Menu.Menu menu, MenuObject owner, string elementName) : base(menu, owner)
        {
            if (!Futile.atlasManager.DoesContainElementWithName("funkin/images/icons/icon-" + elementName))
            {

                if (!File.Exists(AssetManager.ResolveFilePath("funkin/images/icons/icon-" + elementName + ".png")))
                {
                    elementName = "tut_slugger";

                    if (!Futile.atlasManager.DoesContainElementWithName("funkin/images/icons/icon-" + elementName))
                        Futile.atlasManager.LoadImage("funkin/images/icons/icon-" + elementName);

                }
                else
                {
                    Futile.atlasManager.LoadImage("funkin/images/icons/icon-" + elementName);
                }

            }

            sprite = new FSprite("funkin/images/icons/icon-" + elementName);

            sprite.SetAnchor(0.7f, 0.5f);

            this.Container.AddChild(sprite);
        }

        public void Destroy()
        {
            if (this.sprite != null)
                this.Container.RemoveChild(this.sprite);

            if (this.owner != null)
                (this.page as Page).subObjects.Remove(this);
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            this.Size = UnityEngine.Vector2.Lerp(this.Size, wantedsize, 0.3f);

            sprite.SetPosition((this.menu as FunkinMenu).GetPositionBasedOffCamScale(pos, true));
            sprite.scaleX = flipped ? -Size.x : Size.x * Plugin.camHUDScale;
            sprite.scaleY = Size.y * Plugin.camHUDScale;
        }

        public FSprite sprite;

    }

    public class FLX_BAR : MenuObject
    {

        public UnityEngine.Vector2 pos;
        public static UnityEngine.Vector2 wantedsize = new(7, 3.5f);
        public UnityEngine.Vector2 Size = wantedsize;
        public bool flipped = false;

        public FLX_BAR(Menu.Menu menu, MenuObject owner) : base(menu, owner)
        {
            sprite = new FSprite("FLXGBAR");

            this.Container.AddChild(sprite);
        }

        public void Destroy()
        {
            if (this.sprite != null)
                this.Container.RemoveChild(this.sprite);

            if (this.owner != null)
                (this.page as Page).subObjects.Remove(this);
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            this.Size = UnityEngine.Vector2.Lerp(this.Size, wantedsize, 0.3f);

            sprite.SetPosition((this.menu as FunkinMenu).GetPositionBasedOffCamScale(pos, true));
            sprite.scaleX = flipped ? -Size.x : Size.x * Plugin.camHUDScale;
            sprite.scaleY = Size.y * Plugin.camHUDScale;
        }

        public FSprite sprite;

    }

}

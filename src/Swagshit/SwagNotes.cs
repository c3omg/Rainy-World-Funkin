using Menu;
using UnityEngine;

namespace RWF.Swagshit
{

    public class NoteJSON
    {

        public float health_gain = 1;
        public float health_loss = 1;
        public string note_atlas = "notes/FNF_Note";
        public string note_spriteName = "StrumNote_0";
        public int[] overrideColor;
        public bool ignoreNote = false;
        public bool CPUignoreNote = false;
        public bool causesHitMiss = false;
        public bool no_animation = false;

        public NoteJSON() { }

    }

    public class NoteSplash : MenuObject // this doesnt fucking work
    {
        public UnityEngine.Vector2 pos = UnityEngine.Vector2.zero;
        public float sprSize = 2.5f;
        private bool altmode = false;

        public NoteSplash(Menu.Menu menu, MenuObject menuObject, Color color, UnityEngine.Vector2 pos = default) : base(menu, menuObject)
        {

            if (UnityEngine.Random.value < .5f)
                altmode = true;

            sprite = new(altmode ? "note_splash_alt_0" : "note_splash_0", true);

            sprite.scale = sprSize;
            //sprite.SetAnchor(0.5f, 0.5f);
            sprite.color = color;

            this.Container.AddChild(sprite);
            this.pos = pos;
        }

        public void Destroy()
        {
            if (this.sprite != null)
                this.Container.RemoveChild(this.sprite);

            if (this.owner != null)
                (this.page as Page).subObjects.Remove(this);
        }

        public override void GrafUpdate(float timestacker)
        {
            base.GrafUpdate(timestacker);
            sprite.SetPosition((this.menu as FunkinMenu).GetPositionBasedOffCamScale(this.pos, true));
            sprite.scale = sprSize * Plugin.camHUDScale;

            this.sprite.element = Futile.atlasManager.GetElementWithName(altmode ? "note_splash_alt_" + frame : "note_splash_" + frame);

            if (frameCounter == (40 / 14))
            {

                this.frame++;

                this.frameCounter = 0;
            }

            if (this.frame == framecap)
                this.Destroy();
            else
                frameCounter++;

        }

        private int frame = 0;
        private int framecap = 0;
        private int frameCounter = 0;

        public FSprite sprite;
    }

    public class Note : MenuObject
    {

        public UnityEngine.Vector2 pos = UnityEngine.Vector2.zero;
        private Color color;

        public float length = 1f;

        public Color[] NoteColours = new Color[]
        {
            Color.red,
            Color.cyan,
            Color.green,
            new Color(1, 0, 0.45f)
        };

        public float strumTime = 0;
        public bool mustPress = false;
        public int noteData = 0;
        public bool canBeHit = false;
        public bool tooLate = false;
        public bool wasGoodHit = false;
        public bool ignoreNote = false; // im going have to turn this into 2 vars, perchance
        public bool CPUignoreNote = false; // this is just bloats the vars, too bad!
        public bool causesHitMiss = false;
        public bool hitByOpponent = false;
        public bool noteWasHit = false;
        public Note prevNote;
        public Note nextNote;
        public bool IsSusNote = false;

        public bool no_animation = false;

        private bool lastsus = false;

        public float healthGain = 1f;
        public float healthLoss = 1f;

        public float earlyHitMult = 0.5f;
        public float lateHitMult = 1;
        public bool lowPriority = false;

        public float MusicCurrentTime
        {
            get
            {

                return Conductor.songPosition;
            }
        }

        public Note(Menu.Menu menu, MenuObject menuObject, float strumTime = 0, int leData = 0, bool isPlayer = false, UnityEngine.Vector2 pos = default, bool SUSSY = false, bool lastSUSSY = false) : base(menu, menuObject)
        {

            leData %= 4;

            this.strumTime = strumTime;
            this.noteData = leData;
            this.mustPress = isPlayer;
            this.IsSusNote = SUSSY;
            this.lastsus = lastSUSSY;

            this.color = NoteColours[leData];

            if (SUSSY)
            {
                if (lastSUSSY)
                    sprite = new("HoldEnd", true);
                else
                    sprite = new("HoldNote", true);
                sprite.SetAnchor(0.5f, 0f);
            }
            else
            {
                sprite = new("StrumNote_0", true);

                sprite.SetAnchor(0.5f, 0f);

                switch (leData)
                {
                    case 0:
                        sprite.rotation = -90;
                        sprite.SetAnchor(0f, 0.5f);
                        break;
                    case 1:
                        sprite.rotation = 180;
                        sprite.SetAnchor(0.5f, 1f);
                        break;
                    case 3:
                        sprite.rotation = 90;
                        sprite.SetAnchor(1f, 0.5f);
                        break;
                }

            }

            if (SUSSY)
            {
                sprite.scale = 2.5f;
                sprite.scaleX = 2f;
            }
            else
                sprite.scale = 2.5f;

            sprite.color = this.color;

            this.Container.AddChild(sprite);
            this.pos = pos;
        }

        public void Destroy()
        {

            this.Container.RemoveChild(this.sprite);
            this.menu.pages[1].subObjects.Remove(this);

        }

        public override void Update()
        {
            base.Update();

            if (mustPress)
            {
                if (strumTime > MusicCurrentTime - (350 * lateHitMult)
                    && strumTime < MusicCurrentTime + (350 * earlyHitMult))
                    canBeHit = true;
                else
                    canBeHit = false;
            }
            else
            {
                canBeHit = false;

            }

            /*
             
            if (mustPress)
		    {
			    // ok river
			    if (strumTime > Conductor.songPosition - (Conductor.safeZoneOffset * lateHitMult)
				    && strumTime < Conductor.songPosition + (Conductor.safeZoneOffset * earlyHitMult))
				    canBeHit = true;
			    else
				    canBeHit = false;

			    if (strumTime < Conductor.songPosition - Conductor.safeZoneOffset && !wasGoodHit)
				    tooLate = true;
		    }
		    else
		    {
			    canBeHit = false;

			    if (strumTime < Conductor.songPosition + (Conductor.safeZoneOffset * earlyHitMult))
			    {
				    if((isSustainNote && prevNote.wasGoodHit) || strumTime <= Conductor.songPosition)
					    wasGoodHit = true;
			    }
		    }

            */

        }

        public override void GrafUpdate(float timestacker)
        {
            base.GrafUpdate(timestacker);

            sprite.SetPosition((this.menu as FunkinMenu).GetPositionBasedOffCamScale(this.pos, true));
            sprite.scale = 2.5f * Plugin.camHUDScale;
            
            if (IsSusNote)
            {
                sprite.scaleX = 2.5f * RWF_Options.HoldNoteThickness.Value;
                sprite.scaleY = length * Plugin.camHUDScale;
            }


        }

        private int[] animation_frames;
        private int[] animation_framecap;

        public FSprite sprite;

    }

    public class StrumNote : MenuObject
    {

        public UnityEngine.Vector2 pos = UnityEngine.Vector2.zero;
        private Color color;
        public float resetAnim = 0;
        private int noteData = 0;
        public bool downScroll = false;//plan on doing scroll directions soon -bb
        public bool sustainReduce = true;
        public float sprSize = 2.5f;
        private bool player;

        public StrumNote(Menu.Menu menu, MenuObject menuObject, int leData = 0, bool isPlayer = false, UnityEngine.Vector2 pos = default) : base(menu, menuObject)
        {

            leData %= 4;

            this.noteData = leData;
            this.player = isPlayer;

            sprite = new("StrumNote_0", true);

            switch (leData)
            {
                case 0:
                    sprite.rotation = -90;
                    break;
                case 1:
                    sprite.rotation = 180;
                    break;
                case 3:
                    sprite.rotation = 90;
                    break;
            }

            sprite.scale = sprSize;
            //sprite.SetAnchor(0.5f, 0.5f);
            sprite.color = new Color(.75f, .75f, .75f);

            this.Container.AddChild(sprite);
            this.pos = pos;
        }

        public void Destroy()
        {

            this.Container.RemoveChild(this.sprite);
            this.menu.pages[1].subObjects.Remove(this);

        }

        public override void GrafUpdate(float timestacker)
        {
            base.GrafUpdate(timestacker);
            sprite.SetPosition((this.menu as FunkinMenu).GetPositionBasedOffCamScale(this.pos, true));
            sprite.scale = sprSize * Plugin.camHUDScale;
        }

        private int[] animation_frames;
        private int[] animation_framecap;

        public FSprite sprite;

    }

}

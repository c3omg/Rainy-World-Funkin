using Menu;
using RWF.FNFJSON;
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
            this.pos = this.lastpos = pos;
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
            sprite.SetPosition((this.menu as FunkinMenu).GetPositionBasedOffCamScale(Vector2.Lerp(lastpos, pos, timestacker), true));
            sprite.scale = sprSize * Plugin.camHUDScale;

            this.sprite.element = Futile.atlasManager.GetElementWithName(altmode ? "note_splash_alt_" + frame : "note_splash_" + frame);

        }

        public override void Update()
        {
            base.Update();

            if (frameCounter == 3)
            {

                this.frame++;

                this.frameCounter = 0;
            }

            if (this.frame == framecap)
                this.Destroy();
            else
                frameCounter++;

            lastpos = pos;
        }

        private int frame = 0;
        private int framecap = 5;
        public Vector2 lastpos = Vector2.zero;
        private int frameCounter = 0;

        public FSprite sprite;
    }

    public class Note : MenuObject
    {

        public UnityEngine.Vector2 pos = UnityEngine.Vector2.zero;
        public Vector2 lastpos = Vector2.zero;
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

        public string animation_suffix = "";

        public string noteType = "";

        public bool no_animation = false;
        public bool noMissAnimation = false;

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

            if (prevNote == null)
                prevNote = this;

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
                sprite.SetAnchor(0.5f, 0.2f);
            }
            else
            {
                sprite = new("Note", true);

                sprite.SetAnchor(0.5f, 0.5f);

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
            this.pos = this.lastpos = pos;
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
                if (strumTime > Conductor.songPosition - (350 * lateHitMult)
                    && strumTime < Conductor.songPosition + (350 * earlyHitMult))
                    canBeHit = true;
                else
                    canBeHit = false;

                if (strumTime < Conductor.songPosition - 350 && !wasGoodHit)
                    tooLate = true;
            }
            else
            {
                canBeHit = false;

                if (strumTime < Conductor.songPosition + (350 * earlyHitMult))
                {
                    if ((IsSusNote && prevNote.wasGoodHit) || strumTime <= Conductor.songPosition)
                        wasGoodHit = true;
                }

            }

            lastpos = pos;

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
            this.sprite.SetPosition((this.menu as FunkinMenu).GetPositionBasedOffCamScale(Vector2.Lerp(lastpos, pos, timestacker), true, default(Vector2)));
            this.sprite.scale = 2.5f * Plugin.camHUDScale;
            bool isSusNote = this.IsSusNote;
            if (isSusNote)
            {
                this.sprite.scaleX = 2.5f * RWF_Options.HoldNoteThickness.Value;
            }

            if (IsSusNote)
            {
                if (mustPress)
                    clipToStrumNote(FunkinMenu.instance.playerStrums[noteData]);
                else
                    clipToStrumNote(FunkinMenu.instance.opponentStrums[noteData]);
            }
        }

        /*
         public function clipToStrumNote(myStrum:StrumNote)
	{
		var center:Float = myStrum.y + offsetY + Note.swagWidth / 2;
		if(isSustainNote && (mustPress || !ignoreNote) &&
			(!mustPress || (wasGoodHit || (prevNote.wasGoodHit && !canBeHit))))
		{
			var swagRect:FlxRect = clipRect;
			if(swagRect == null) swagRect = new FlxRect(0, 0, frameWidth, frameHeight);

			if (myStrum.downScroll)
			{
				if(y - offset.y * scale.y + height >= center)
				{
					swagRect.width = frameWidth;
					swagRect.height = (center - y) / scale.y;
					swagRect.y = frameHeight - swagRect.height;
				}
			}
			else if (y + offset.y * scale.y <= center)
			{
				swagRect.y = (center - y) / scale.y;
				swagRect.width = width / scale.x;
				swagRect.height = (height / scale.y) - swagRect.y;
			}
			clipRect = swagRect;
		}
	}
         */

        public void clipToStrumNote(StrumNote myStrum)
        {
            float center = myStrum.pos.y;
            if (IsSusNote && (mustPress || !ignoreNote) &&
                (!mustPress || (wasGoodHit || (prevNote.wasGoodHit && !canBeHit))))
            {

                var bruhMoment = center - pos.y;

                if (Mathf.Abs(bruhMoment) <= sprite.element.sourceSize.y * length && bruhMoment > 0)
                    sprite.height = Mathf.Abs(bruhMoment);
                else if (bruhMoment <= 0)
                    sprite.scaleY = 0;
                else
                    sprite.scaleY = length;

            }
            else if (IsSusNote)
            {
                sprite.scaleY = length;
            }
        }

        public bool gfNote = false;
        private int[] animation_frames;
        private int[] animation_framecap;
        public FSprite sprite;

    }

    public class StrumNote : MenuObject
    {

        public UnityEngine.Vector2 pos = UnityEngine.Vector2.zero;
        public Vector2 lastpos = Vector2.zero;
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

            sprite.anchorX = sprite.anchorY = 0.5f;

            sprite.scale = sprSize;
            //sprite.SetAnchor(0.5f, 0.5f);
            sprite.color = new Color(.75f, .75f, .75f);

            this.Container.AddChild(sprite);
            this.pos = this.lastpos = pos;
        }

        public override void Update()
        {
            base.Update();

            lastpos = pos;
        }

        public void Destroy()
        {

            this.Container.RemoveChild(this.sprite);
            this.menu.pages[1].subObjects.Remove(this);

        }

        public override void GrafUpdate(float timestacker)
        {
            base.GrafUpdate(timestacker);
            sprite.SetPosition((this.menu as FunkinMenu).GetPositionBasedOffCamScale(Vector2.Lerp(lastpos, pos, timestacker), true));
            //this.sprite.SetPosition(Vector2.Lerp(this.sprite.GetPosition(), (this.menu as FunkinMenu).GetPositionBasedOffCamScale(this.pos, true, default(Vector2)), timestacker));
            sprite.scale = sprSize;
        }

        private int[] animation_frames;
        private int[] animation_framecap;

        public FSprite sprite;

    }

    public class Rating : MenuObject
    {
        // Token: 0x06000080 RID: 128 RVA: 0x00008DD8 File Offset: 0x00006FD8
        public Rating(Menu.Menu menu, MenuObject menuObject, string rating, Vector2 pos = default(Vector2)) : base(menu, menuObject)
        {
            Futile.atlasManager.LoadImage("funkin/images/ratings/" + rating);
            this.sprite = new FSprite("funkin/images/ratings/" + rating, true)
            {
                scale = 1.5f
            };
            this.pos = this.lastpos = pos;
            this.Container.AddChild(this.sprite);
        }

        // Token: 0x06000081 RID: 129 RVA: 0x00008E6C File Offset: 0x0000706C
        public void Destroy()
        {
            bool flag = this.sprite != null;
            if (flag)
            {
                this.Container.RemoveChild(this.sprite);
            }
            bool flag2 = this.owner != null;
            if (flag2)
            {
                base.page.subObjects.Remove(this);
            }
        }

        // Token: 0x06000082 RID: 130 RVA: 0x00008EB8 File Offset: 0x000070B8
        public override void Update()
        {
            this.pos += this.vel;
            this.vel += this.acc;
            this.lifecounter++;
            bool flag = this.sprite.alpha <= 0f && this.lifecounter >= 200;
            if (flag)
            {
                this.Destroy();
            }
            else
            {
                bool flag2 = this.lifecounter > 15;
                if (flag2)
                {
                    this.sprite.alpha -= 0.025f;
                }
                base.Update();
            }

            lastpos = pos;
        }

        // Token: 0x06000083 RID: 131 RVA: 0x00008F62 File Offset: 0x00007162
        public override void GrafUpdate(float timestacker)
        {
            base.GrafUpdate(timestacker);
            this.sprite.SetPosition(Vector2.Lerp(lastpos, pos, timestacker));
        }

        // Token: 0x040000A2 RID: 162
        public Vector2 pos = Vector2.zero;
        public Vector2 lastpos = Vector2.zero;

        // Token: 0x040000A3 RID: 163
        public Vector2 vel = Vector2.zero;

        // Token: 0x040000A4 RID: 164
        public Vector2 acc = Vector2.zero;

        // Token: 0x040000A5 RID: 165
        public int lifecounter = 0;

        // Token: 0x040000A6 RID: 166
        public FSprite sprite;
    }

}

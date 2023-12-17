using Menu;
using Newtonsoft.Json;
using RWF.FNFJSON;
using RWF.Swagshit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using UnityEngine;
using static MonoMod.InlineRT.MonoModRule;
using static RWF.Conductor;


namespace RWF
{
    public class FunkinMenu : Menu.Menu
    {

        //EVENTS

        public static event FunkinMenu.hook_beatHit OnBeatHit;
        public static event FunkinMenu.hook_update OnUpdate;
        public static event FunkinMenu.hook_updatePost OnUpdatePost;
        public static event FunkinMenu.hook_ctor OnCreate;
        public static event FunkinMenu.hooke_playerhit OnPlayerHit;
        public static event FunkinMenu.hooke_enemyhit OnEnemyHit;
        public static event FunkinMenu.hook_missnote OnMiss;
        public static event FunkinMenu.hook_notecreated OnNoteCreated;
        public static event FunkinMenu.hook_stephit OnStepHit;
        public static event FunkinMenu.hook_updatecameratarget UpdateCameraTarget;
        public static event FunkinMenu.hook_songstart OnSongStart;

        public delegate void hook_beatHit(RWF.FunkinMenu self, int curBeat);
        public delegate void hook_update(RWF.FunkinMenu self);
        public delegate void hook_updatePost(RWF.FunkinMenu self);
        public delegate void hook_ctor(RWF.FunkinMenu self);
        public delegate void hooke_playerhit(RWF.FunkinMenu self, Swagshit.Note daNote);
        public delegate void hooke_enemyhit(RWF.FunkinMenu self, Swagshit.Note daNote);
        public delegate void hook_missnote(RWF.FunkinMenu self, Swagshit.Note daNote);
        public delegate void hook_notecreated(FunkinMenu self, RWF.Swagshit.Note daNote);
        public delegate void hook_stephit(FunkinMenu self, int curStep);
        public delegate void hook_updatecameratarget(RWF.FunkinMenu self);
        public delegate void hook_songstart(RWF.FunkinMenu self);

        // Varibles

        private Color[] BasicNoteColours = new Color[]
        {
            Color.red,
            Color.cyan,
            Color.green,
            new(1, 0, 0.45f)
        };

        public float camGameWantedZoom = 0.9f;

        public int camBounceSpeed = 4;
        public float camStrengh = 1f;

        public float health = 1f;
        public Song SONG = null;
        public bool startedCountdown = false;
        public float cameraHUDScale = 0f;
        public float cameraGameScale = 0f;
        public float spawnTime = 2000;
        public bool skipCountdown = false;
        public bool useDefaultExitFunction = true;

        public bool alreadygoingtoadifferentsecene = false;

        public List<StrumNote> strumLineNotes;
        public List<StrumNote> opponentStrums;
        public List<StrumNote> playerStrums;

        public Character boyfriend;
        public Character dad;

        public Dictionary<string, Character> currentRappers = new Dictionary<string, Character>();

        public Stage stage;

        public string DeathMusicName = "FNF_GAMEOVER_MUSIC";

        public HealthIcon hpIconP1;
        public HealthIcon hpIconP2;

        private Dictionary<KeyCode, bool> keysPressed;
        private Dictionary<KeyCode, int> keyData;

        public List<Swagshit.Note> notes = new List<Swagshit.Note> { };
        public List<Swagshit.Note> unspawnNotes = new List<Swagshit.Note> { };

        public int combo = 0;
        public int score = 0;
        public int misses = 0;
        public float totalNotesHit = 0;
        public int totalPlayed = 0;

        public float decBeat = 0;

        public FLX_BAR bar;

        public UnityEngine.Vector2 cameraPosiion = new(0, 0);
        public UnityEngine.Vector2 cameraTarget = new(0, 0);

        private int lastBeat = 0;
        private int lastStep = 0;

        public float CurrentTime
        {
            get
            {
                return Conductor.songPosition;
            }
        }

        private bool startingSong = true;

        public MenuLabel scoretText;

        public static FunkinMenu instance = null;

        public bool gotblueballed = false;

        public List<Conductor.ratingshit> RatingData = new List<Conductor.ratingshit>
            {
                new Conductor.ratingshit("perfect", 1f),
                new Conductor.ratingshit("sick", 45f),
                new Conductor.ratingshit("good", 90f, 0.67f),
                new Conductor.ratingshit("bad", 150f, 0.34f),
                new Conductor.ratingshit("shit", 230f, 0f)
            };

        // Functions

        private RWF.Swagshit.Note CreateUsableNote(RWF.FNFJSON.Note daNote)
        {
            Swagshit.Note oldNote;

            RWF.Swagshit.Note dunceNote = new RWF.Swagshit.Note(this, this.pages[1], (float)daNote.StrumTime, (int)daNote.NoteData, daNote.GottaHit, default(Vector2), daNote.isSustainNote, daNote.lastSustainNote);
            if (unspawnNotes.Count > 0)
                oldNote = unspawnNotes[unspawnNotes.Count - 1];
            else
                oldNote = dunceNote;
            dunceNote.noteType = daNote.PsychNoteType;
            bool flag = daNote.PsychNoteType != "";
            if (flag)
            {
                NoteJSON noteType = File.Exists(AssetManager.ResolveFilePath("funkin/custom_note/" + daNote.PsychNoteType.ToString() + ".json")) ? JsonConvert.DeserializeObject<NoteJSON>(File.ReadAllText(AssetManager.ResolveFilePath("funkin/custom_note/" + daNote.PsychNoteType.ToString() + ".json"))) : null;
                bool flag2 = noteType != null;
                if (flag2)
                {
                    bool flag3 = noteType.overrideColor != null;
                    if (flag3)
                    {
                        dunceNote.NoteColours[0] = new Color((float)(noteType.overrideColor[0] / 255), (float)(noteType.overrideColor[1] / 255), (float)(noteType.overrideColor[2] / 255));
                        dunceNote.NoteColours[1] = new Color((float)(noteType.overrideColor[0] / 255), (float)(noteType.overrideColor[1] / 255), (float)(noteType.overrideColor[2] / 255));
                        dunceNote.NoteColours[2] = new Color((float)(noteType.overrideColor[0] / 255), (float)(noteType.overrideColor[1] / 255), (float)(noteType.overrideColor[2] / 255));
                        dunceNote.NoteColours[3] = new Color((float)(noteType.overrideColor[0] / 255), (float)(noteType.overrideColor[1] / 255), (float)(noteType.overrideColor[2] / 255));
                    }
                    dunceNote.healthGain = noteType.health_gain;
                    dunceNote.healthLoss = noteType.health_loss;
                    dunceNote.ignoreNote = noteType.ignoreNote;
                    dunceNote.CPUignoreNote = noteType.CPUignoreNote;
                    dunceNote.causesHitMiss = noteType.causesHitMiss;
                    dunceNote.no_animation = noteType.no_animation;
                    string imagePath = AssetManager.ResolveFilePath("funkin/images/" + noteType.note_atlas);
                    bool flag4 = !Futile.atlasManager.DoesContainAtlas(Path.GetFileNameWithoutExtension(imagePath));
                    if (flag4)
                    {
                        Futile.atlasManager.LoadAtlas("funkin/images/" + noteType.note_atlas);
                    }
                    dunceNote.sprite.element = Futile.atlasManager.GetElementWithName(noteType.note_spriteName);
                    bool flag5 = noteType.overrideColor != null;
                    if (flag5)
                    {
                        dunceNote.sprite.color = new Color((float)(noteType.overrideColor[0] / 255), (float)(noteType.overrideColor[1] / 255), (float)(noteType.overrideColor[2] / 255));
                    }
                }

                switch (daNote.PsychNoteType)
                {
                    case "Alt Animation":
                        dunceNote.animation_suffix = "-alt";
                        break;
                }

            }
            bool isSustainNote = daNote.isSustainNote;
            if (isSustainNote)
            {
                dunceNote.length *= Conductor.step_crochet / 100f * 1.4f;
                dunceNote.length *= this.SONG.speed;
            }
            dunceNote.gfNote = daNote.gfNote;
            dunceNote.pos = new Vector2(-5000f, 5000f);
            this.pages[1].subObjects.Add(dunceNote);
            dunceNote.prevNote = oldNote;
            return dunceNote;
        }

        private void CreateNotes()
        {

            foreach (Section section in this.SONG.Sections)
            {
                bool flag3 = section == null;
                if (!flag3)
                {
                    bool isgfnote = section.gfSection;
                    foreach (RWF.FNFJSON.Note note in section.ConvertSectionToNotes())
                    {
                        bool flag4 = note == null;
                        if (!flag4)
                        {
                            note.gfNote = isgfnote;
                            bool flag5 = note.SustainLength > 0f;
                            if (flag5)
                            {
                                List<RWF.FNFJSON.Note> susnotes = new List<RWF.FNFJSON.Note>();
                                int floorSus = (int)Mathf.Floor(note.SustainLength / Conductor.step_crochet);
                                bool flag6 = floorSus > 0;
                                if (flag6)
                                {
                                    for (int k = 1; k < floorSus + 1; k++)
                                    {
                                        RWF.FNFJSON.Note Sus_note = new RWF.FNFJSON.Note(note)
                                        {
                                            PsychNoteType = note.PsychNoteType,
                                            gfNote = isgfnote
                                        };
                                        bool flag7 = k == floorSus;
                                        if (flag7)
                                        {
                                            Sus_note.lastSustainNote = true;
                                        }
                                        Sus_note.StrumTime += (int)Conductor.step_crochet * k;
                                        this.unspawnNotes.Add(this.CreateUsableNote(Sus_note));
                                        susnotes.Add(Sus_note);
                                    }
                                }
                            }
                            note.SustainLength = 0f;
                            this.unspawnNotes.Add(this.CreateUsableNote(note));
                        }
                    }
                }
            }



            this.unspawnNotes = (from x in this.unspawnNotes
                                 orderby x.strumTime
                                 select x).ToList<RWF.Swagshit.Note>();
        }

        public FunkinMenu(ProcessManager manager) : base(manager, Plugin.FunkinMenu) // this whole thing is fucking bloated, but it works alright
        {

            this.framesPerSecond = 60;

            instance = this;

            curBeat = 0;
            curStep = 0;
            decBeat = 0;

            Conductor.songPosition = -5000 / Conductor.songPosition;

            this.pages.Add(new Page(this, null, "game", 0));
            this.pages.Add(new Page(this, null, "hud", 1));
            this.pages.Add(new Page(this, null, "other", 2));
            this.pages[0].Container = new();
            this.pages[1].Container = new();
            this.pages[2].Container = new();

            Futile.stage.AddChild(this.pages[0].Container);
            Futile.stage.AddChild(this.pages[1].Container);
            Futile.stage.AddChild(this.pages[2].Container);

            if (!Plugin.Songs.ContainsKey(Plugin.SelectedSong))
            {
                Debug.LogWarning("Song does not exist, either that or i couldnt find the chart, but if thats the case, how did i even log that??");
                this.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
                return;
            }

            this.SONG = Song.LoadFromFile(Plugin.Songs[Plugin.SelectedSong]);

            if (SONG == null)
            {
                Debug.LogWarning("Song failed to load");
                this.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
                return;
            }

            bpm = this.SONG.bpm;

            crochet = (60f / bpm) * 1000;

            strumLineNotes = new List<StrumNote>();
            opponentStrums = new List<StrumNote>();
            playerStrums = new List<StrumNote>();

            keysPressed = new Dictionary<KeyCode, bool>()
            {
                [RWF_Options.key_note[0].Value] = false,
                [RWF_Options.key_note[1].Value] = false,
                [RWF_Options.key_note[2].Value] = false,
                [RWF_Options.key_note[3].Value] = false,
                [KeyCode.Escape] = false,
                [KeyCode.Return] = false,
                [KeyCode.R] = false,
            };

            keyData = new Dictionary<KeyCode, int>()
            {
                [RWF_Options.key_note[0].Value] = 0,
                [RWF_Options.key_note[1].Value] = 1,
                [RWF_Options.key_note[2].Value] = 2,
                [RWF_Options.key_note[3].Value] = 3,
            };

            var stageCheck = File.Exists(AssetManager.ResolveFilePath("funkin/stages/" + SONG.StageName.ToString().ToLower() + ".json")) ? AssetManager.ResolveFilePath("funkin/stages/" + SONG.StageName.ToString().ToLower() + ".json") : null;

            if (stageCheck == null) stageCheck = AssetManager.ResolveFilePath("funkin/stages/outskirts.json");

            stage = new(this, this.pages[0], File.ReadAllText(stageCheck));

            this.camGameWantedZoom = stage.camZoom;

            this.pages[0].subObjects.Add(stage);

            for (int i = 0; i < 4; i++)
            {
                StrumNote strum = new(this, this.pages[1], i, true, new(777 + (135 * i), 655));

                this.pages[1].subObjects.Add(strum);

                playerStrums.Add(strum);
                strumLineNotes.Add(strum);

            }

            for (int i = 0; i < 4; i++)
            {
                StrumNote strum = new(this, this.pages[1], i, true, new(111 + (135 * i), 655));

                this.pages[1].subObjects.Add(strum);

                opponentStrums.Add(strum);
                strumLineNotes.Add(strum);

            }

            CreateNotes();

            var charDataP1 = AssetManager.ResolveFilePath("funkin/characters/" + SONG.Player1Char.ToString() + ".json");
            var charDataP2 = AssetManager.ResolveFilePath("funkin/characters/" + SONG.Player2Char.ToString() + ".json");

            if (!File.Exists(charDataP1)) charDataP1 = AssetManager.ResolveFilePath("funkin/characters/tut_slugger.json");
            if (!File.Exists(charDataP2)) charDataP2 = AssetManager.ResolveFilePath("funkin/characters/tut_slugger.json");

            bar = new FLX_BAR(this, this.pages[1]);

            boyfriend = new Character(this, this.pages[0], File.ReadAllText(charDataP1));

            this.pages[0].subObjects.Add(boyfriend);

            boyfriend.PlayAnimation("idle");

            boyfriend.isPlayer = true;

            dad = new Character(this, this.pages[0], File.ReadAllText(charDataP2));

            dad.flipped = !dad.flipped;
            dad.sprite.scaleX *= -1;

            this.pages[0].subObjects.Add(dad);

            dad.PlayAnimation("idle");

            boyfriend.pos = new(stage.bf_pos.x + boyfriend.offsetPosition.x, stage.bf_pos.y + boyfriend.offsetPosition.y);
            dad.pos = new(stage.dad_pos.x + dad.offsetPosition.x, stage.dad_pos.y + dad.offsetPosition.y);

            hpIconP2 = new(this, this.pages[1], boyfriend.iconName);
            hpIconP1 = new(this, this.pages[1], dad.iconName);

            hpIconP1.pos = new(650, 125);
            hpIconP2.pos = new(850, 125);

            hpIconP2.flipped = true;

            bar.pos = UnityEngine.Vector2.Lerp(new UnityEngine.Vector2(500, 100), new UnityEngine.Vector2(850, 115), 0.5f);

            this.pages[1].subObjects.Add(bar);
            this.pages[1].subObjects.Add(hpIconP1);
            this.pages[1].subObjects.Add(hpIconP2);

            scoretText = new(this, this.pages[1], score + " : Score", new(this.manager.rainWorld.screenSize.x / 2, 45), new(150, 50), false, null);

            this.pages[1].subObjects.Add(scoretText);

            UpdateScoreText();

            scoretText.label.color = stage.textColorOverride;

            Conductor.songPosition = -Conductor.crochet * 5;

            if (FunkinMenu.OnCreate != null) FunkinMenu.OnCreate(this);

            currentRappers.Add("dad", dad);
            currentRappers.Add("bf", boyfriend);

            startCountdown();

        }

        private void startCountdown()
        {
            startedCountdown = true;
            if (skipCountdown)
                Conductor.songPosition = 0;
            else
                Conductor.songPosition = -Conductor.crochet * 5;
        }

        public void goodNoteHit(Swagshit.Note daNote)
        {
            if (daNote.wasGoodHit) return;

            daNote.wasGoodHit = true;

            if (OnPlayerHit != null) OnPlayerHit(this, daNote);
                        
            health += 0.023f * daNote.healthGain;

            combo++;
            score += 350;
            totalPlayed++;

            UpdateScoreText();

            if (!daNote.IsSusNote)
            {
                notes.Remove(daNote);
                daNote.Destroy();
            }
        }

        public void noteMiss(Swagshit.Note daNote)
        {

            if (OnMiss != null) OnMiss(this, daNote);

            int random = UnityEngine.Random.Range(1, 3);

            this.PlaySound(Plugin.missnote_sounds[random - 1], 0, 0.1f, 1f);

            health -= 0.092f * daNote.healthLoss;

            combo = 0;
            score -= 150;
            misses++;
            totalPlayed++;

            UpdateScoreText();

            if (!daNote.noMissAnimation)
            {
                switch (daNote.noteData)
                {
                    case 0:
                        boyfriend.PlayAnimation("left-miss", true);
                        break;
                    case 1:
                        boyfriend.PlayAnimation("down-miss", true);
                        break;
                    case 2:
                        boyfriend.PlayAnimation("up-miss", true);
                        break;
                    case 3:
                        boyfriend.PlayAnimation("right-miss", true);
                        break;
                }
            }

            boyfriend.holdtimer = 0;

            notes.Remove(daNote);
            daNote.Destroy();
        }

        public void noteMiss(int noteData)
        {
            if (OnMiss != null) OnMiss(this, null);

            int random = UnityEngine.Random.Range(1, 3);

            this.PlaySound(Plugin.missnote_sounds[random - 1], 0, 0.1f, 1f);

            health -= 0.092f;

            combo = 0;
            score -= 150;
            misses++;

            UpdateScoreText();

            noteData %= 4;

            switch (noteData)
            {
                case 0:
                    boyfriend.PlayAnimation("left-miss", true);
                    break;
                case 1:
                    boyfriend.PlayAnimation("down-miss", true);
                    break;
                case 2:
                    boyfriend.PlayAnimation("up-miss", true);
                    break;
                case 3:
                    boyfriend.PlayAnimation("right-miss", true);
                    break;
            }

            boyfriend.holdtimer = 0;

        }

        public void beatHit(int curBeat)
        {
            if (OnBeatHit != null)
            {
                FunkinMenu.OnBeatHit(this, curBeat);
            }

            switch (curBeat)
            {
                case (-4):
                    this.PlaySound(Plugin.introSounds[2], 0, 0.2f, 1);
                    break;
                case (-3):
                    this.PlaySound(Plugin.introSounds[1], 0, 0.2f, 1);
                    break;
                case (-2):
                    this.PlaySound(Plugin.introSounds[0], 0, 0.2f, 1);
                    break;
                case (-1):
                    this.PlaySound(Plugin.introSounds[3], 0, 0.2f, 1);
                    break;
            }

            if (curBeat % stage.bop_speed == 0)
            {

                foreach (string characterName in currentRappers.Keys.ToList())
                {

                    Character character = currentRappers[characterName];

                    if (character.curAnim == "idle")
                    {
                        character.PlayAnimation("idle", false);
                    }

                }

            }

            if (curBeat % camBounceSpeed == 0)
            {
                //Plugin.camGameScale += 0.015f * camStrengh;
                //Plugin.camHUDScale += 0.03f * camStrengh;

                Add_Camera_Zoom(0.015f * camStrengh, 0.03f * camStrengh);
            }
        }

        public void Add_Camera_Zoom(float gameZoom = 0.015f, float hudZoom = 0.03f)
        {
            this.pages[0].Container.ScaleAroundPointAbsolute(this.manager.rainWorld.screenSize / 2, this.pages[0].Container.scaleX + gameZoom, this.pages[0].Container.scaleY + gameZoom);
            this.pages[1].Container.ScaleAroundPointAbsolute(this.manager.rainWorld.screenSize / 2, this.pages[1].Container.scaleX + hudZoom, this.pages[1].Container.scaleY + hudZoom);
        }

        public void UpdateScoreText()
        {
            int sicks = RatingData[0].hits + RatingData[1].hits;
            int goods = RatingData[2].hits;
            int bads = RatingData[3].hits;
            int dogshits = RatingData[4].hits;

            float accurate = 0;
            if (totalPlayed != 0)
            {
                accurate = Mathf.Min(1, Mathf.Max(0, totalNotesHit / (float)totalPlayed));
            }

            string Rating = "Clear";

            if (misses == 0)
            {
                if (bads > 0 || dogshits > 0) Rating = "FC";
                else if (goods > 0) Rating = "GFC";
                else if (sicks > 0) Rating = "SFC";
            }
            else if (misses < 10)
                Rating = "SDCB";
            else if (misses > 50)
                Rating = "Bro how are you still alive";
            else if (misses > 50)
                Rating = "Jesus christ..";

            if (totalPlayed == 0)
                Rating = "???";

            scoretText.text = "Misses : " + misses + " // Combo : " + combo + " // Score : " + score + " // Rating : " + Rating + " (" + (accurate * 100f) + "%)";
        }

        public void AttemptToPressNote(int Data)
        {

            // heavily based on my own code LOL if it aint broke dont fix it
            List<Swagshit.Note> pressNotes = new List<Swagshit.Note> { };
            //var notesDatas:Array<Int> = [];
            bool notesStopped = false;
            bool pressedNote = false;
            string animation_suffix = "";
            bool playAnim = true;
            Color noteColor = BasicNoteColours[Data];

            List<Swagshit.Note> pressableNotes = new List<Swagshit.Note> { };

            foreach (Swagshit.Note note in notes)
            {

                if (note.noteData == Data && note.canBeHit && !note.IsSusNote)
                    pressableNotes.Add(note);

            }

            if (pressableNotes.Count > 0)
            {

                foreach (Swagshit.Note epicNote in pressableNotes.OrderBy(x => x.strumTime).ToList())
                {

                    foreach (Swagshit.Note doubleNote in pressNotes.OrderBy(x => x.strumTime).ToList())
                    {
                        if (Mathf.Abs(doubleNote.strumTime - epicNote.strumTime) < 1)
                        {
                            notes.Remove(doubleNote);
                            doubleNote.Destroy();
                        }
                        else
                            notesStopped = true;
                    }

                    // eee jack detection before was not super good
                    if (!notesStopped)
                    {

                        if (epicNote.causesHitMiss)
                            noteMiss(epicNote);
                        else
                        {
                            if (!epicNote.IsSusNote)
                            {
                                Conductor.ratingshit rating = Conductor.judgeNote(epicNote, RatingData, Mathf.Abs(epicNote.strumTime - Conductor.songPosition));

                                totalNotesHit += rating.ratingMod;

                                if (rating.name == "sick" | rating.name == "perfect")
                                {
                                    NoteSplash splash = new NoteSplash(this, this.pages[1], noteColor, this.playerStrums[Data].pos);
                                    this.pages[1].subObjects.Add(splash);
                                }
                                Rating rate = new Rating(this, this.pages[1], rating.name, this.manager.rainWorld.screenSize / 2f)
                                {
                                    vel = new Vector2(UnityEngine.Random.Range(-1f, 1f), 4.5f),
                                    acc = new Vector2(0f, -0.15f)
                                };
                                rate.pos.x += 425f;
                                rate.pos.y -= 300f;
                                rate.lastpos = rate.pos;
                                this.pages[1].subObjects.Add(rate);
                            }


                            animation_suffix = epicNote.animation_suffix.ToString();
                            goodNoteHit(epicNote);
                        }

                        noteColor = epicNote.NoteColours[epicNote.noteData];
                        pressedNote = true;
                        playAnim = !epicNote.no_animation;
                        pressNotes.Add(epicNote);
                    }

                }

            }

            if (pressedNote)
            {
                playerStrums[Data].sprite.color = Color.Lerp(noteColor, Color.white, 0.65f);
                playerStrums[Data].sprSize = 2.65f;
                
                if (playAnim)
                {

                    if (SONG.Sections[curSection] != null)
                    {
                        if (SONG.Sections[curSection].altAnim && !SONG.Sections[curSection].gfSection)
                        {
                            animation_suffix = "-alt";
                        }
                    }

                    switch (Data)
                    {
                        case 0:
                            boyfriend.PlayAnimation("left" + animation_suffix, true);
                            break;
                        case 1:
                            boyfriend.PlayAnimation("down" + animation_suffix, true);
                            break;
                        case 2:
                            boyfriend.PlayAnimation("up" + animation_suffix, true);
                            break;
                        case 3:
                            boyfriend.PlayAnimation("right" + animation_suffix, true);
                            break;
                    }

                    boyfriend.holdtimer = 0;
                }

            }
            else
            {
                playerStrums[Data].sprite.color = Color.Lerp(noteColor, Color.black, 0.65f);
                playerStrums[Data].sprSize = 2.15f;

                if (!RWF_Options.GhostTapping.Value)
                    noteMiss(Data);

            }


        }

        public void KillPlayer()
        {

            gotblueballed = true;

            dad.Destroy();

            foreach (StrumNote strumNote in strumLineNotes)
            {
                strumNote.Destroy();
            }

            dad.Destroy();
            stage.Destroy();

            foreach (Swagshit.Note note in notes)
            {
                note.Destroy();
            }

            unspawnNotes.Clear();

            if (this.manager.musicPlayer.song != null)
            {
                this.manager.musicPlayer.song.subTracks[0].source.Stop();
                this.manager.musicPlayer.song = null;
            }

            boyfriend.PlayAnimation("Death", true);

            hpIconP1.Destroy();
            hpIconP2.Destroy();
            bar.Destroy();

            this.PlaySound(Plugin.fnfDeath, 0, 0.3f, 1f);



        }

        //Hold Notes
        public void checkKeys()
        {

            List<bool> holdArray = new()
            {
                Input.GetKey(RWF_Options.key_note[0].Value),
                Input.GetKey(RWF_Options.key_note[1].Value),
                Input.GetKey(RWF_Options.key_note[2].Value),
                Input.GetKey(RWF_Options.key_note[3].Value),
            };


            if (notes.Count > 0)
            {
                foreach (Swagshit.Note daNote in notes.ToList())
                {
                    if (daNote.IsSusNote && holdArray[daNote.noteData] && daNote.canBeHit
                        && daNote.mustPress && !daNote.tooLate && !daNote.wasGoodHit)
                    {
                        playerStrums[daNote.noteData].sprite.color = Color.Lerp(daNote.sprite.color, Color.white, 0.65f);
                        playerStrums[daNote.noteData].sprSize = 2.65f;

                        if (!daNote.no_animation)
                        {

                            var animation_suffix = "";

                            if (SONG.Sections[curSection] != null)
                            {
                                if (SONG.Sections[curSection].altAnim && !SONG.Sections[curSection].gfSection)
                                {
                                    animation_suffix = "-alt";
                                }
                            }

                            switch (daNote.noteData)
                            {
                                case 0:
                                    boyfriend.PlayAnimation("left" + animation_suffix, true);
                                    break;
                                case 1:
                                    boyfriend.PlayAnimation("down" + animation_suffix, true);
                                    break;
                                case 2:
                                    boyfriend.PlayAnimation("up" + animation_suffix, true);
                                    break;
                                case 3:
                                    boyfriend.PlayAnimation("right" + animation_suffix, true);
                                    break;
                            }

                            boyfriend.holdtimer = 0;
                        }

                        goodNoteHit(daNote);

                    }
                }
            }

            /*
             * if (note.strumTime <= Conductor.songPosition && note.mustPress && note.IsSusNote && !note.CPUignoreNote)
                    {                        

                        if (note.IsSusNote) note.clipToStrumNote(playerStrums[note.noteData]);

                        foreach (KeyCode kvp in keysPressed.Keys.ToList<KeyCode>())
                        {
                            if (Input.GetKey(kvp) && note.noteData == keyData[kvp])
                            {
                                playerStrums[note.noteData].sprite.color = Color.Lerp(note.sprite.color, Color.white, 0.65f);
                                playerStrums[note.noteData].sprSize = 2.65f;

                                if (!note.no_animation)
                                {

                                    var animation_suffix = "";

                                    if (SONG.Sections[curSection] != null)
                                    {
                                        if (SONG.Sections[curSection].altAnim && !SONG.Sections[curSection].gfSection)
                                        {
                                            animation_suffix = "-alt";
                                        }
                                    }

                                    switch (note.noteData)
                                    {
                                        case 0:
                                            boyfriend.PlayAnimation("left" + animation_suffix, true);
                                            break;
                                        case 1:
                                            boyfriend.PlayAnimation("down" + animation_suffix, true);
                                            break;
                                        case 2:
                                            boyfriend.PlayAnimation("up" + animation_suffix, true);
                                            break;
                                        case 3:
                                            boyfriend.PlayAnimation("right" + animation_suffix, true);
                                            break;
                                    }

                                    boyfriend.holdtimer = 0;
                                }

                                totalNotesHit++;

                                goodNoteHit(note);
                                continue;
                            }
                        }

                    }*/
        }

        public UnityEngine.Vector2 GetPositionBasedOffCamScale(UnityEngine.Vector2 vector2, bool UsesHUDScaling = false, UnityEngine.Vector2 scrollFactor = default) // i wasnt tired when writing this, i just dont know how to fucking code alrighted
        {

            float scale = Plugin.camGameScale;

            if (UsesHUDScaling)
                scale = Plugin.camHUDScale;

            UnityEngine.Vector2 vector = this.manager.rainWorld.screenSize;

            vector /= 2;

            UnityEngine.Vector2 trueVecoter = vector2;

            if (!UsesHUDScaling)
            {
                trueVecoter.x -= (cameraPosiion.x - vector.x) * scrollFactor.x;
                trueVecoter.y -= (cameraPosiion.y - vector.y) * scrollFactor.y;
            }

            return (trueVecoter + ((trueVecoter - (vector)) * (scale - 1)));
        }

        private void startSong()
        {
            startingSong = false;

            if (FunkinMenu.OnSongStart != null) FunkinMenu.OnSongStart(this);

            var song = new Music.Song(this.manager.musicPlayer, "FNF - " + SONG.Name, Music.MusicPlayer.MusicContext.Menu);

            Conductor.CurrentSong = song;

            if (this.manager.musicPlayer.song == null)
            {
                this.manager.musicPlayer.song = song;
                this.manager.musicPlayer.song.playWhenReady = true;
            }

        }

        public override void Update() // this might be the reason for the lag, but im scared it'll bite back if i even think about making it work better
        {
            base.Update();

            if (OnUpdate != null)
            {
                FunkinMenu.OnUpdate(this);
            }


            Plugin.camGameScale = Mathf.Lerp(Plugin.camGameScale, 1, 0.1f);
            Plugin.camHUDScale = Mathf.Lerp(Plugin.camHUDScale, 1, 0.1f);

            this.pages[0].Container.ScaleAroundPointAbsolute(this.manager.rainWorld.screenSize / 2, Mathf.Lerp(this.pages[0].Container.scaleX, camGameWantedZoom, 0.04f), Mathf.Lerp(this.pages[0].Container.scaleY, camGameWantedZoom, 0.04f));
            this.pages[1].Container.ScaleAroundPointAbsolute(this.manager.rainWorld.screenSize / 2, Mathf.Lerp(this.pages[1].Container.scaleX, 1f, 0.04f), Mathf.Lerp(this.pages[1].Container.scaleY, 1f, 0.04f));

            if (!gotblueballed)
            {
                if (startedCountdown && this.manager.musicPlayer.song == null)
                {
                    Conductor.songPosition += (1f/60f) * 1000;                
                }
                else if (this.manager.musicPlayer.song != null)
                {
                    Conductor.songPosition = this.manager.musicPlayer.song.subTracks[0].source.time * 1000;
                }

                if (startingSong)
                {
                    if (startedCountdown && Conductor.songPosition >= 0)
                        startSong();
                    else if (!startedCountdown)
                        Conductor.songPosition = -Conductor.crochet * 12;
                }

                decBeat = (CurrentTime / 1000f) / (60f / bpm);
                curBeat = (int)Mathf.Floor((CurrentTime / 1000f) / (60f / bpm));
                curStep = (int)Mathf.Floor(((CurrentTime / 1000f) / (60f / bpm)) * 4f);

                var boyfriendlass = boyfriend.flipped ? boyfriend.pos + new UnityEngine.Vector2(-boyfriend.CameraOffset.x, boyfriend.CameraOffset.y) : boyfriend.pos + new UnityEngine.Vector2(boyfriend.CameraOffset.x, boyfriend.CameraOffset.y);
                var dadlass = dad.flipped ? dad.pos + new UnityEngine.Vector2(-dad.CameraOffset.x, dad.CameraOffset.y) : dad.pos + new UnityEngine.Vector2(dad.CameraOffset.x, dad.CameraOffset.y);

                if (curBeat >= 0 && SONG.Sections.Count > curBeat / 4)
                {
                    if (SONG.Sections[curBeat / 4].mustHitSection)
                        cameraTarget = boyfriendlass;
                    else
                        cameraTarget = dadlass;
                }
                else cameraTarget = dadlass;

                if (UpdateCameraTarget != null)
                {
                    FunkinMenu.UpdateCameraTarget(this);
                }

                cameraPosiion = UnityEngine.Vector2.Lerp(cameraPosiion, cameraTarget, stage.camSpeed);
                
                hpIconP1.Size = new Vector2(Mathf.Lerp(1.3f, 1.0f, FlxEase.quartOut(decBeat % 1)), Mathf.Lerp(1.3f, 1.0f, FlxEase.quartOut(decBeat % 1)));
                hpIconP2.Size = new Vector2(Mathf.Lerp(1.3f, 1.0f, FlxEase.quartOut(decBeat % 1)), Mathf.Lerp(1.3f, 1.0f, FlxEase.quartOut(decBeat % 1)));

                if (health > 2) health = 2;
                else if (health <= 0)
                {
                    KillPlayer();
                    return;
                }

                scoretText.pos = scoretText.lastPos = GetPositionBasedOffCamScale(new(this.manager.rainWorld.screenSize.x / 2, 45), true);
                scoretText.label.SetPosition(scoretText.pos);
                scoretText.label.scale = 1.5f * Plugin.camHUDScale;

                var hpIconOffsets = 16;
                var hpIconPosiiton = UnityEngine.Vector2.Lerp(new(bar.pos.x + (bar.sprite.width / 2), 125), new(bar.pos.x - (bar.sprite.width / 2), 125), health / 2);

                hpIconP1.pos = hpIconPosiiton - new UnityEngine.Vector2(hpIconOffsets, 0);
                hpIconP2.pos = hpIconPosiiton + new UnityEngine.Vector2(hpIconOffsets, 0);

                if (curBeat != lastBeat)
                {
                    //Debug.Log("Beat Hit: " + curBeat);
                    //Plugin.camGameScale += 0.3f;
                    lastBeat = curBeat;
                    beatHit(curBeat);
                }

                if (curStep != lastStep)
                {
                    //Debug.Log("Beat Hit: " + curBeat);
                    //Plugin.camGameScale += 0.3f;
                    lastStep = curStep;
                    stepHit(curStep);
                }

                if (unspawnNotes.Count > 0 && unspawnNotes[0] != null)
                {
                    float time = this.spawnTime;
                    bool flag13 = this.SONG.speed < 1f;
                    if (flag13)
                    {
                        time /= this.SONG.speed;
                    }
                    while (this.unspawnNotes.Count > 0 && this.unspawnNotes[0].strumTime - Conductor.songPosition < time)
                    {
                        RWF.Swagshit.Note dunceNote = this.unspawnNotes[0];
                        this.notes.Add(dunceNote);
                        if (OnNoteCreated != null)
                        {
                            OnNoteCreated(this, dunceNote);
                        }
                        this.unspawnNotes.Remove(this.unspawnNotes[0]);
                    }
                }
                else // kinda a hacky way of ending the song, i dont want the song to end right after starting since there might be a frame where the song is still null, i also dont know if update is called still, or if it only gets called after creation
                {
                    if (this.manager.musicPlayer.song == null && useDefaultExitFunction)
                    {
                        this.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
                        this.framesPerSecond = 40;
                    }
                }

                checkKeys();

                foreach (Swagshit.Note note in notes.ToList())
                {

                    if (Conductor.songPosition > spawnTime + note.strumTime)
                    {
                        notes.Remove(note);
                        note.Destroy();
                        continue;
                    }



                    if (note.strumTime - CurrentTime <= -0 && !note.mustPress && !note.CPUignoreNote)
                    {
                        if (OnEnemyHit != null) OnEnemyHit(this, note);

                        if (!note.no_animation)
                        {

                            string suffix = note.animation_suffix;

                            if (SONG.Sections[curSection].altAnim)
                                suffix = "-alt";

                            switch (note.noteData)
                            {
                                case 0:
                                    dad.PlayAnimation("left" + suffix, true);
                                    break;
                                case 1:
                                    dad.PlayAnimation("down" + suffix, true);
                                    break;
                                case 2:
                                    dad.PlayAnimation("up" + suffix, true);
                                    break;
                                case 3:
                                    dad.PlayAnimation("right" + suffix, true);
                                    break;
                            }

                            dad.holdtimer = 0;
                        }

                        notes.Remove(note);
                        note.Destroy();
                        continue;
                    }

                    if (Conductor.songPosition - note.strumTime > Mathf.Max(Conductor.step_crochet, 350 / SONG.speed))
                    {
                        if (note.mustPress && !note.ignoreNote && (note.tooLate || !note.wasGoodHit))
                            noteMiss(note);

                        note.inactive = true;
                        note.sprite.isVisible = false;

                        notes.Remove(note);
                        note.Destroy();

                        continue;
                    }

                    float setYpos = playerStrums[note.noteData].pos.y;

                    if (!note.mustPress)
                    {
                        setYpos = opponentStrums[note.noteData].pos.y;
                    }

                    UnityEngine.Vector2 vector2 = new();

                    if (note.mustPress)
                        vector2.x = playerStrums[note.noteData % 4].pos.x;
                    else
                        vector2.x = opponentStrums[note.noteData % 4].pos.x;

                    vector2.y = (setYpos + 0.45f * (CurrentTime - note.strumTime) * SONG.speed * 1f);

                    note.pos = vector2;



                }

                foreach (string characterName in currentRappers.Keys.ToList())
                {

                    Character character = currentRappers[characterName];

                    if (character.isPlayer)
                    {
                        if (character.curAnim != "idle" && (character.holdtimer > Conductor.step_crochet * (0.0011) * character.singDuration) && !keysPressed.ContainsValue(true))
                        {
                            character.PlayAnimation("idle");
                        }
                    }
                    else
                    {
                        if (character.curAnim != "idle" && (character.holdtimer > Conductor.step_crochet * (0.0011) * character.singDuration))
                        {
                            character.PlayAnimation("idle");
                        }
                    }

                }

            }
            else
            {
                var boyfriendlass = boyfriend.flipped ? boyfriend.pos + new UnityEngine.Vector2(-boyfriend.CameraOffset.x, boyfriend.CameraOffset.y) : boyfriend.pos + new UnityEngine.Vector2(boyfriend.CameraOffset.x, boyfriend.CameraOffset.y);
                cameraPosiion = UnityEngine.Vector2.Lerp(cameraPosiion, boyfriendlass, 0.1f);

                scoretText.text = "";

                if (boyfriend.finished && this.manager.musicPlayer.song == null && !alreadygoingtoadifferentsecene)
                {
                    var song = new Music.Song(this.manager.musicPlayer, DeathMusicName, Music.MusicPlayer.MusicContext.Menu);

                    this.manager.musicPlayer.song = song;
                    this.manager.musicPlayer.song.playWhenReady = true;
                }

            }

            foreach (KeyCode kvp in keysPressed.Keys.ToList<KeyCode>())
            {

                if (Input.GetKey(kvp))
                {

                    if (!keysPressed[kvp])
                    {
                        if (!gotblueballed && keyData.ContainsKey(kvp))
                        {

                            AttemptToPressNote(keyData[kvp]);

                        }
                        else if (!gotblueballed && kvp == KeyCode.R)
                        {
                            health = -999;
                        }
                        else if (gotblueballed && !alreadygoingtoadifferentsecene)
                        {
                            //this.PlaySound(Plugin.fnfDeath, 0, 0.3f, 1f);
                            if (kvp == KeyCode.Escape)
                            {
                                
                                if (this.manager?.musicPlayer?.song != null)
                                {
                                    this.manager.musicPlayer.FadeOutAllSongs(40f);
                                }

                                this.manager.RequestMainProcessSwitch(Plugin.FunkinFreeplayMenu);
                                this.framesPerSecond = 40;
                                alreadygoingtoadifferentsecene = true;
                            }
                            else if (kvp == KeyCode.Return)
                            {
                                alreadygoingtoadifferentsecene = true;
                                
                                if (this.manager?.musicPlayer?.song != null)
                                {
                                    this.manager.musicPlayer.song.subTracks[0].source.Stop();
                                    this.manager.musicPlayer.song = null;
                                }

                                this.PlaySound(Plugin.fnfRestart, 0, 0.2f, 1f);
                                this.manager.RequestMainProcessSwitch(Plugin.FunkinRestart, 4f);
                            }

                        }
                    }


                    keysPressed[kvp] = true;
                }
                else
                {

                    if (keysPressed[kvp] && keyData.ContainsKey(kvp))
                    {
                        playerStrums[keyData[kvp]].sprite.color = new Color(.75f, .75f, .75f);
                        playerStrums[keyData[kvp]].sprSize = 2.5f;
                    }

                    keysPressed[kvp] = false;
                }

            }

            if (OnUpdatePost != null)
            {
                FunkinMenu.OnUpdatePost(this);
            }

        }

        private void stepHit(int curStep)
        {
            if (OnStepHit != null) OnStepHit(this, curStep);
        }
    }

    internal class FunkinFreeplayMenu : Menu.Menu // its fucking funny how the freeplay menu is so much smaller than the actual game
    {

        private MenuLabel menuLabel;
        private bool eneterigdebug = false;

        public FunkinFreeplayMenu(ProcessManager manager) : base(manager, Plugin.FunkinFreeplayMenu)
        {

            this.pages.Add(new Page(this, null, "songList", 0));

            foreach (string kvp in Plugin.Songs.Keys)
            {
                CreateSongButton(kvp);
            }

            menuLabel = new(this, this.pages[0], "Currently Selected Song: " + Plugin.SelectedSong, new(1200, 250), new(125, 35), false);

            this.pages[0].subObjects.Add(menuLabel);
            this.pages[0].subObjects.Add(new SimpleButton(this, this.pages[0], this.Translate("Play"), "PLAYFNFSONG", new(1200, 150), new(55, 35)));

            this.pages[0].subObjects.Add(new SimpleButton(this, this.pages[0], this.Translate("Return to Menu"), "EXITTOMENU", new(50, 50), new(125, 35)));

        }
        public override void Update()
        {
            if (!this.eneterigdebug && Input.GetKey(KeyCode.C))
            {
                this.eneterigdebug = true;
                this.manager.RequestMainProcessSwitch(Plugin.FunkinCharacterEditor);
            }
            base.Update();
        }

        public override void Singal(MenuObject sender, string message)
        {

            if (message == "PLAYFNFSONG")
            {
                this.PlaySound(SoundID.MENU_Player_Join_Game, 0, 1, 3);
                this.manager.RequestMainProcessSwitch(Plugin.FunkinMenu);
            }
            else if (message == "EXITTOMENU")
            {
                this.PlaySound(SoundID.MENU_Player_Unjoin_Game);
                this.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
            }
            else
            {
                foreach (SimpleButton button in buttons)
                {
                    if (songButtons.ContainsKey(button) && message == button.signalText)
                    {
                        this.PlaySound(SoundID.MENU_Next_Slugcat, 0, 1, 1);
                        Plugin.SelectedSong = songButtons[button];

                        menuLabel.text = "Currently Selected Song: " + Plugin.SelectedSong;

                        break;
                    }
                }
            }

        }

        public void CreateSongButton(string Song = "Unnamed Song")
        {

            float maxCapOfButtons = 10;
            float amountOfButtons = songButtons.Count % maxCapOfButtons;

            Debug.Log("RWF DEBUG: songButtons : " + songButtons.Count);

            SimpleButton button = new(this, this.pages[0], Song, Song.ToUpper(), new(75 + (125f * (int)Mathf.Floor(songButtons.Count / maxCapOfButtons)), 730 - (47.5f * amountOfButtons)), new(100, 35));

            this.pages[0].subObjects.Add(button);

            songButtons.Add(button, Song);

            buttons.Add(button);

        }

        private List<SimpleButton> buttons = new List<SimpleButton> { };
        private Dictionary<SimpleButton, string> songButtons = new Dictionary<SimpleButton, string> { };
        private Dictionary<SimpleButton, Action> buttonFuncs = new Dictionary<SimpleButton, Action> { };

    }

    public class FunkinCharacterEditor : Menu.Menu
    {
        // Token: 0x0600004D RID: 77 RVA: 0x00005C74 File Offset: 0x00003E74
        public FunkinCharacterEditor(ProcessManager manager) : base(manager, Plugin.FunkinCharacterEditor)
        {
            this.framesPerSecond = 60;
            this.pages.Add(new Page(this, null, "songList", 0));
            FSprite bgsprite = new FSprite("Futile_White", true)
            {
                scale = 300f,
                color = new Color(0.35f, 0.35f, 0.35f)
            };
            bgsprite.SetPosition(this.manager.rainWorld.screenSize / 2f);
            this.container.AddChild(bgsprite);
            this.pages[0].subObjects.Add(new SimpleButton(this, this.pages[0], base.Translate("Return to Menu"), "EXITTOMENU", new Vector2(50f, 50f), new Vector2(125f, 35f)));
            this.pages[0].subObjects.Add(new SimpleButton(this, this.pages[0], base.Translate("Save Character JSON"), "SAVECHAR", new Vector2(50f, 110f), new Vector2(125f, 35f)));
            this.pages[0].subObjects.Add(new SimpleButton(this, this.pages[0], base.Translate("Reload Character"), "RELOADCHAR", new Vector2(1100f, 700f), new Vector2(125f, 35f)));
            this.pages[0].subObjects.Add(new SimpleButton(this, this.pages[0], base.Translate("Prev Anim"), "LANIM", new Vector2(1110f, 600f), new Vector2(62f, 35f)));
            this.pages[0].subObjects.Add(new SimpleButton(this, this.pages[0], base.Translate("Next Anim"), "RANIM", new Vector2(1172f, 600f), new Vector2(62f, 35f)));
            this.charlistbutton = new SimpleButton(this, this.pages[0], base.Translate("tut_slugger"), "OPENCHARLIST", new Vector2(1225f, 700f), new Vector2(125f, 35f));
            this.pages[0].subObjects.Add(this.charlistbutton);
            int index = 0;
            foreach (string str in AssetManager.ListDirectory("funkin/characters", false, true))
            {
                index++;
                SimpleButton button = new SimpleButton(this, this.pages[0], base.Translate(Path.GetFileNameWithoutExtension(str)), "SELECTNEWCHAR", new Vector2(-999f, (float)(700 - 50 * index)), new Vector2(125f, 35f));
                this.pages[0].subObjects.Add(button);
                this.list.Add(button);
            }
            this.character = new Character(this, this.pages[0], File.ReadAllText(AssetManager.ResolveFilePath(this.charPath + ".json")))
            {
                pos = this.manager.rainWorld.screenSize / 2f
            };
            this.animName = new MenuLabel(this, this.pages[0], "Current Animation: idle", new Vector2(950f, 700f), new Vector2(150f, 35f), false, null);
            this.pages[0].subObjects.Add(this.character);
            this.pages[0].subObjects.Add(this.animName);
            this.character.PlayAnimation(this.character.animations.Keys.ToList<string>()[this.animIndex], true);
        }

        // Token: 0x0600004E RID: 78 RVA: 0x000060DC File Offset: 0x000042DC
        public override void Singal(MenuObject sender, string message)
        {
            bool flag = message == "EXITTOMENU";
            if (flag)
            {
                base.PlaySound(SoundID.MENU_Player_Unjoin_Game);
                this.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
            }
            else
            {
                bool flag2 = message == "OPENCHARLIST";
                if (flag2)
                {
                    this.charlistbutton.toggled = !this.charlistbutton.toggled;
                    bool toggled = this.charlistbutton.toggled;
                    if (toggled)
                    {
                        foreach (SimpleButton button in this.list)
                        {
                            button.pos.x = this.charlistbutton.pos.x;
                        }
                    }
                    else
                    {
                        foreach (SimpleButton button2 in this.list)
                        {
                            button2.pos.x = -9999f;
                        }
                    }
                }
                else
                {
                    bool flag3 = message == "SELECTNEWCHAR";
                    if (flag3)
                    {
                        this.charlistbutton.toggled = false;
                        this.charlistbutton.menuLabel.text = (sender as SimpleButton).menuLabel.text;
                        this.charPath = "funkin/characters/" + this.charlistbutton.menuLabel.text;
                        foreach (SimpleButton button3 in this.list)
                        {
                            button3.pos.x = -9999f;
                        }
                        Character character = this.character;
                        if (character != null)
                        {
                            character.Destroy();
                        }
                        this.character = new Character(this, this.pages[0], File.ReadAllText(AssetManager.ResolveFilePath(this.charPath + ".json")))
                        {
                            pos = this.manager.rainWorld.screenSize / 2f
                        };
                        this.pages[0].subObjects.Add(this.character);
                        this.animIndex = 0;
                        this.character.PlayAnimation(this.character.animations.Keys.ToList<string>()[this.animIndex], true);
                        this.animName.text = "Current Anim: " + this.character.animations.Keys.ToList<string>()[this.animIndex];
                    }
                    else
                    {
                        bool flag4 = message == "RELOADCHAR";
                        if (flag4)
                        {
                            base.PlaySound(SoundID.MENU_Player_Join_Game);
                            Character character2 = this.character;
                            if (character2 != null)
                            {
                                character2.Destroy();
                            }
                            this.character = new Character(this, this.pages[0], File.ReadAllText(AssetManager.ResolveFilePath(this.charPath + ".json")))
                            {
                                pos = this.manager.rainWorld.screenSize / 2f
                            };
                            this.pages[0].subObjects.Add(this.character);
                            this.animIndex = 0;
                            this.character.PlayAnimation(this.character.animations.Keys.ToList<string>()[this.animIndex], true);
                            this.animName.text = "Current Anim: " + this.character.animations.Keys.ToList<string>()[this.animIndex];
                        }
                        else
                        {
                            bool flag5 = message == "SAVECHAR";
                            if (flag5)
                            {
                                string json = JsonConvert.SerializeObject(this.character.json);
                                File.WriteAllText(AssetManager.ResolveFilePath(this.charPath + ".json"), json);
                            }
                            else
                            {
                                bool flag6 = message == "LANIM";
                                if (flag6)
                                {
                                    base.PlaySound(SoundID.MENU_Continue_Game);
                                    this.animIndex--;
                                    bool flag7 = this.animIndex < 0;
                                    if (flag7)
                                    {
                                        this.animIndex = this.character.animations.Keys.Count - 1;
                                    }
                                    this.character.PlayAnimation(this.character.animations.Keys.ToList<string>()[this.animIndex], true);
                                    this.animName.text = "Current Anim: " + this.character.animations.Keys.ToList<string>()[this.animIndex];
                                }
                                else
                                {
                                    bool flag8 = message == "RANIM";
                                    if (flag8)
                                    {
                                        base.PlaySound(SoundID.MENU_Continue_Game);
                                        this.animIndex++;
                                        this.animIndex %= this.character.animations.Keys.Count;
                                        this.character.PlayAnimation(this.character.animations.Keys.ToList<string>()[this.animIndex], true);
                                        this.animName.text = "Current Anim: " + this.character.animations.Keys.ToList<string>()[this.animIndex];
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Token: 0x0400003D RID: 61
        private Character character;

        // Token: 0x0400003E RID: 62
        private MenuLabel animName;

        // Token: 0x0400003F RID: 63
        private string charPath = "funkin/characters/tut_slugger";

        // Token: 0x04000040 RID: 64
        private int animIndex = 0;

        // Token: 0x04000041 RID: 65
        private List<SimpleButton> list = new List<SimpleButton>();

        // Token: 0x04000042 RID: 66
        private SimpleButton charlistbutton;
    }

}

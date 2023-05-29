using Menu;
using Newtonsoft.Json;
using RWF.FNFJSON;
using RWF.Swagshit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RWF
{
    public class FunkinMenu : Menu.Menu
    {

        //EVENTS

        public static event FunkinMenu.hook_beatHit OnBeatHit;
        public static event FunkinMenu.hook_update OnUpdate;
        public static event FunkinMenu.hook_updatePost OnUpdatePost;
        public static event FunkinMenu.hook_ctor OnCreate;

        public delegate void hook_beatHit(RWF.FunkinMenu self, int curBeat);
        public delegate void hook_update(RWF.FunkinMenu self);
        public delegate void hook_updatePost(RWF.FunkinMenu self);
        public delegate void hook_ctor(RWF.FunkinMenu self);

        // Varibles

        private Color[] BasicNoteColours = new Color[]
        {
            Color.red,
            Color.cyan,
            Color.green,
            new Color(1, 0, 0.45f)
        };

        public float camGameWantedZoom = 0.9f;

        public int camBounceSpeed = 4;
        public float camStrengh = 1f;

        public float health = 1f;
        public Song SONG = null;
        public float cameraHUDScale = 0f;
        public float cameraGameScale = 0f;
        public float spawnTime = 2000;

        public bool alreadygoingtoadifferentsecene = false;

        public List<StrumNote> strumLineNotes;
        public List<StrumNote> opponentStrums;
        public List<StrumNote> playerStrums;

        public Character boyfriend;
        public Character dad;
        public Stage stage;

        public HealthIcon hpIconP1;
        public HealthIcon hpIconP2;

        private Dictionary<KeyCode, bool> keysPressed;
        private Dictionary<KeyCode, int> keyData;

        public List<Swagshit.Note> notes = new List<Swagshit.Note> { };
        public List<FNFJSON.Note> unspawnNotes = new List<FNFJSON.Note> { };

        public float BPM = 100.0f;
        public int currentBeat = 0;
        public int currentStep = 0;

        public int combo = 0;
        public int score = 0;

        public int sicks = 0;
        public int goods = 0;
        public int bads = 0;
        public int dogshits = 0;
        public int misses = 0;

        public FLX_BAR bar;

        public UnityEngine.Vector2 cameraPosiion = new(0, 0);

        private int lastBeat = 0;

        public float crochet = 0;
        public float step_crochet
        {
            get
            {
                return crochet / 4;
            }
        }

        public float CurrentTime
        {
            get
            {

                if (this.manager.musicPlayer != null && this.manager.musicPlayer.song != null)
                {
                    if (this.manager.musicPlayer.song.subTracks[0].source != null)
                    {
                        return this.manager.musicPlayer.song.subTracks[0].source.time * 1000;
                    }
                }

                return 0f;
            }
        }

        public MenuLabel scoretText;

        public bool gotblueballed = false;

        // Functions

        public FunkinMenu(ProcessManager manager) : base(manager, Plugin.FunkinMenu) // this whole thing is fucking bloated, but it works alright
        {

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

            BPM = this.SONG.bpm;

            crochet = (60f / BPM) * 1000;

            strumLineNotes = new List<StrumNote>();
            opponentStrums = new List<StrumNote>();
            playerStrums = new List<StrumNote>();

            keysPressed = new Dictionary<KeyCode, bool>()
            {
                [RWF_Options.key_note_left.Value] = false,
                [RWF_Options.key_note_down.Value] = false,
                [RWF_Options.key_note_up.Value] = false,
                [RWF_Options.key_note_right.Value] = false,
                [KeyCode.Escape] = false,
                [KeyCode.Return] = false,
            };

            keyData = new Dictionary<KeyCode, int>()
            {
                [RWF_Options.key_note_left.Value] = 0,
                [RWF_Options.key_note_down.Value] = 1,
                [RWF_Options.key_note_up.Value] = 2,
                [RWF_Options.key_note_right.Value] = 3,
            };

            this.pages.Add(new Page(this, null, "game", 0));
            this.pages.Add(new Page(this, null, "hud", 1));

            var stageCheck = File.Exists(AssetManager.ResolveFilePath("funkin/stages/" + SONG.StageName.ToString().ToLower() + ".json")) ? AssetManager.ResolveFilePath("funkin/stages/" + SONG.StageName.ToString().ToLower() + ".json") : null;

            if (stageCheck == null) stageCheck = AssetManager.ResolveFilePath("funkin/stages/outskirts.json");

            stage = new(this, this.pages[0], File.ReadAllText(stageCheck));

            this.camGameWantedZoom = stage.camZoom;

            this.pages[0].subObjects.Add(stage);

            foreach (Section section in SONG.Sections)
            {

                if (section == null) continue;

                foreach (FNFJSON.Note note in section.ConvertSectionToNotes())
                {

                    if (note == null) continue;

                    if (note.SustainLength > 0)
                    {
                        List<FNFJSON.Note> susnotes = new List<FNFJSON.Note> { };

                        var floorSus = (int)Mathf.Floor(note.SustainLength / step_crochet);

                        if (floorSus > 0)
                        {
                            for (int i = 0; i < floorSus + 1; i++)
                            {

                                FNFJSON.Note Sus_note = new(note);

                                if (i == floorSus)
                                {
                                    Sus_note.lastSustainNote = true;
                                }

                                Sus_note.StrumTime += (int)step_crochet * i;

                                unspawnNotes.Add(Sus_note);
                                susnotes.Add(Sus_note);

                            }
                        }

                    }

                    note.SustainLength = 0;

                    unspawnNotes.Add(note);

                }

            }

            //unspawnNotes = unspawnNotes.OrderByDescending(x => x.StrumTime).ToList();

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

            scoretText.label.color = stage.textColorOverride;

            var song = new Music.Song(this.manager.musicPlayer, "FNF - " + SONG.Name, Music.MusicPlayer.MusicContext.Menu);

            if (this.manager.musicPlayer.song == null)
            {
                this.manager.musicPlayer.song = song;
                this.manager.musicPlayer.song.playWhenReady = true;
            }

            if (FunkinMenu.OnCreate != null)
            {
                FunkinMenu.OnCreate(this);
            }

        }

        public void goodNoteHit(Swagshit.Note daNote)
        {
            health += 0.023f * daNote.healthGain;

            combo++;
            score += 350;

            UpdateScoreText();

            notes.Remove(daNote);
            daNote.Destroy();
        }

        public void noteMiss(Swagshit.Note daNote)
        {
            int random = UnityEngine.Random.Range(1, 3);

            this.PlaySound(Plugin.missnote_sounds[random - 1], 0, 0.1f, 1f);

            health -= 0.092f * daNote.healthLoss;

            combo = 0;
            score -= 150;
            misses++;

            UpdateScoreText();

            notes.Remove(daNote);
            daNote.Destroy();
        }

        public void noteMiss(int noteData)
        {
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
        }

        public void beatHit(int curBeat)
        {
            if (OnBeatHit != null)
            {
                FunkinMenu.OnBeatHit(this, curBeat);
            }

            if (curBeat % stage.bop_speed == 0)
            {
                if (boyfriend.finished | boyfriend.curAnim == "idle")
                {
                    boyfriend.PlayAnimation("idle");
                }
                if (dad.finished | dad.curAnim == "idle")
                {
                    dad.PlayAnimation("idle");
                }
            }

            hpIconP1.Size *= 1.3f;
            hpIconP2.Size *= 1.3f;

            if (curBeat % camBounceSpeed == 0)
            {
                Plugin.camGameScale += 0.015f * camStrengh;
                Plugin.camHUDScale += 0.03f * camStrengh;
            }
        }

        public void UpdateScoreText()
        {
            scoretText.text = "Misses : " + misses + " // Combo : " + combo + " // Score : " + score;
        }

        public void AttemptToPressNote(int Data)
        {

            // heavily based on my own code LOL if it aint broke dont fix it
            List<Swagshit.Note> pressNotes = new List<Swagshit.Note> { };
            //var notesDatas:Array<Int> = [];
            bool notesStopped = false;
            bool pressedNote = false;
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
                        //goodNoteHit(epicNote);

                        if (epicNote.causesHitMiss)
                            noteMiss(epicNote);
                        else
                            goodNoteHit(epicNote);

                        noteColor = epicNote.NoteColours[epicNote.noteData];
                        pressedNote = true;
                        pressNotes.Add(epicNote);
                    }

                }

            }

            if (pressedNote)
            {
                playerStrums[Data].sprite.color = Color.Lerp(noteColor, Color.white, 0.65f);
                playerStrums[Data].sprSize = 2.65f;

                NoteSplash splash = new(this, pages[1], noteColor, playerStrums[Data].pos);

                this.pages[1].subObjects.Add(splash);

                switch (Data)
                {
                    case 0:
                        boyfriend.PlayAnimation("left", true);
                        break;
                    case 1:
                        boyfriend.PlayAnimation("down", true);
                        break;
                    case 2:
                        boyfriend.PlayAnimation("up", true);
                        break;
                    case 3:
                        boyfriend.PlayAnimation("right", true);
                        break;
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

            this.manager.musicPlayer.song.subTracks[0].source.Stop();
            this.manager.musicPlayer.song = null;

            boyfriend.PlayAnimation("Death", true);

            hpIconP1.Destroy();
            hpIconP2.Destroy();
            bar.Destroy();

            this.PlaySound(Plugin.fnfDeath, 0, 0.3f, 1f);



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

        public override void Update() // this might be the reason for the lag, but im scared it'll bite back if i even think about making it work better
        {
            base.Update();

            if (OnUpdate != null)
            {
                FunkinMenu.OnUpdate(this);
            }

            Plugin.camGameScale = Mathf.Lerp(Plugin.camGameScale, camGameWantedZoom, 0.1f);
            Plugin.camHUDScale = Mathf.Lerp(Plugin.camHUDScale, 1f, 0.1f);

            
            if (!gotblueballed)
            {
                currentBeat = (int)Mathf.Floor((CurrentTime / 1000f) / (60f / BPM));
                currentStep = (int)Mathf.Floor((CurrentTime / 1000f) / (60f / BPM)) * 4;

                var boyfriendlass = boyfriend.flipped ? boyfriend.pos + new UnityEngine.Vector2(-boyfriend.CameraOffset.x, boyfriend.CameraOffset.y) : boyfriend.pos + new UnityEngine.Vector2(boyfriend.CameraOffset.x, boyfriend.CameraOffset.y);
                var dadlass = dad.flipped ? dad.pos + new UnityEngine.Vector2(-dad.CameraOffset.x, dad.CameraOffset.y) : dad.pos + new UnityEngine.Vector2(dad.CameraOffset.x, dad.CameraOffset.y);

                if (SONG.Sections.Count > currentBeat / 4)
                {
                    if (SONG.Sections[currentBeat / 4].mustHitSection)
                        cameraPosiion = UnityEngine.Vector2.Lerp(cameraPosiion, boyfriendlass, stage.camSpeed);
                    else
                        cameraPosiion = UnityEngine.Vector2.Lerp(cameraPosiion, dadlass, stage.camSpeed);
                }

                if (health > 2) health = 2;
                else if (health <= 0)
                {
                    KillPlayer();
                    return;
                }

                scoretText.pos = scoretText.lastPos = GetPositionBasedOffCamScale(new(this.manager.rainWorld.screenSize.x / 2, 45), true);
                scoretText.label.SetPosition(scoretText.pos);
                scoretText.label.scale = 1.5f * Plugin.camHUDScale;

                hpIconP1.pos = UnityEngine.Vector2.Lerp(new(850, 125), new(500, 125), health / 2) - new UnityEngine.Vector2(15, 0);
                hpIconP2.pos = UnityEngine.Vector2.Lerp(new(850, 125), new(500, 125), health / 2) + new UnityEngine.Vector2(15, 0);

                if (currentBeat != lastBeat)
                {
                    //Debug.Log("Beat Hit: " + currentBeat);
                    //Plugin.camGameScale += 0.3f;
                    lastBeat = currentBeat;
                    beatHit(currentBeat);
                }

                if (unspawnNotes.Count > 0 && unspawnNotes[0] != null)
                {
                    float time = spawnTime;
                    if (SONG.speed < 1f) time /= SONG.speed;

                    while (unspawnNotes.Count > 0 && unspawnNotes[0].StrumTime - CurrentTime < time)
                    {

                        FNFJSON.Note daNote = unspawnNotes[0];

                        Swagshit.Note dunceNote = new Swagshit.Note(this, null, unspawnNotes[0].StrumTime, ((int)unspawnNotes[0].NoteData), unspawnNotes[0].GottaHit, default, unspawnNotes[0].isSustainNote, unspawnNotes[0].lastSustainNote);

                        if (daNote.PsychNoteType != "")
                        {
                            var noteType = File.Exists(AssetManager.ResolveFilePath("funkin/custom_note/" + daNote.PsychNoteType.ToString() + ".json")) ? JsonConvert.DeserializeObject<NoteJSON>(File.ReadAllText(AssetManager.ResolveFilePath("funkin/custom_note/" + daNote.PsychNoteType.ToString() + ".json"))) : null;

                            if (noteType != null)
                            {

                                if (noteType.overrideColor != null)
                                {
                                    dunceNote.NoteColours[0] = new Color(noteType.overrideColor[0] / 255, noteType.overrideColor[1] / 255, noteType.overrideColor[2] / 255);
                                    dunceNote.NoteColours[1] = new Color(noteType.overrideColor[0] / 255, noteType.overrideColor[1] / 255, noteType.overrideColor[2] / 255);
                                    dunceNote.NoteColours[2] = new Color(noteType.overrideColor[0] / 255, noteType.overrideColor[1] / 255, noteType.overrideColor[2] / 255);
                                    dunceNote.NoteColours[3] = new Color(noteType.overrideColor[0] / 255, noteType.overrideColor[1] / 255, noteType.overrideColor[2] / 255);
                                }

                                dunceNote.healthGain = noteType.health_gain;
                                dunceNote.healthLoss = noteType.health_loss;
                                dunceNote.ignoreNote = noteType.ignoreNote;
                                dunceNote.CPUignoreNote = noteType.CPUignoreNote;
                                dunceNote.causesHitMiss = noteType.causesHitMiss;

                                var imagePath = AssetManager.ResolveFilePath("funkin/images/" + noteType.note_atlas);

                                if (!Futile.atlasManager.DoesContainAtlas(Path.GetFileNameWithoutExtension(imagePath)))
                                {
                                    Futile.atlasManager.LoadAtlas("funkin/images/" + noteType.note_atlas);
                                }

                                dunceNote.sprite.element = Futile.atlasManager.GetElementWithName(noteType.note_spriteName);

                                if (noteType.overrideColor != null)
                                    dunceNote.sprite.color = new Color(noteType.overrideColor[0] / 255, noteType.overrideColor[1] / 255, noteType.overrideColor[2] / 255);

                            }
                        }

                        notes.Add(dunceNote);
                        notes = notes.OrderByDescending(x => x.strumTime).ToList();

                        this.pages[1].subObjects.Add(dunceNote);

                        var index = unspawnNotes[0];
                        unspawnNotes.Remove(index);
                    }
                }
                else // kinda a hacky way of ending the song, i dont want the song to end right after starting since there might be a frame where the song is still null, i also dont know if update is called still, or if it only gets called after creation
                {
                    if (this.manager.musicPlayer.song == null)
                    {
                        this.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
                    }
                }

                foreach (Swagshit.Note note in notes.OrderBy(x => x.strumTime).ToList())
                {

                    if (note.strumTime - CurrentTime > CurrentTime + spawnTime)
                    {
                        notes.Remove(note);
                        note.Destroy();
                        continue;
                    }



                    if (note.strumTime - CurrentTime <= -0 && !note.mustPress && !note.CPUignoreNote)
                    {

                        switch (note.noteData)
                        {
                            case 0:
                                dad.PlayAnimation("left", true);
                                break;
                            case 1:
                                dad.PlayAnimation("down", true);
                                break;
                            case 2:
                                dad.PlayAnimation("up", true);
                                break;
                            case 3:
                                dad.PlayAnimation("right", true);
                                break;
                        }

                        notes.Remove(note);
                        note.Destroy();
                        continue;
                    }

                    if (note.strumTime - CurrentTime <= -0 && note.mustPress && note.IsSusNote && !note.CPUignoreNote)
                    {

                        foreach (KeyCode kvp in keysPressed.Keys.ToList<KeyCode>())
                        {
                            if (Input.GetKey(kvp) && note.noteData == keyData[kvp])
                            {
                                playerStrums[note.noteData].sprite.color = Color.Lerp(note.sprite.color, Color.white, 0.65f);
                                playerStrums[note.noteData].sprSize = 2.65f;

                                switch (note.noteData)
                                {
                                    case 0:
                                        boyfriend.PlayAnimation("left", true);
                                        break;
                                    case 1:
                                        boyfriend.PlayAnimation("down", true);
                                        break;
                                    case 2:
                                        boyfriend.PlayAnimation("up", true);
                                        break;
                                    case 3:
                                        boyfriend.PlayAnimation("right", true);
                                        break;
                                }

                                goodNoteHit(note);
                                continue;
                            }
                        }

                    }

                    if (350 + note.strumTime < CurrentTime && note.mustPress && !note.ignoreNote)
                    {

                        noteMiss(note);
                        continue;

                    }

                    float setYpos = 555;

                    UnityEngine.Vector2 vector2 = new();

                    if (note.mustPress)
                        vector2.x = playerStrums[note.noteData % 4].pos.x;
                    else
                        vector2.x = opponentStrums[note.noteData % 4].pos.x;

                    vector2.y = (setYpos + 0.45f * (CurrentTime - note.strumTime) * SONG.speed * 1f);

                    note.pos = vector2;



                }
            }
            else
            {
                var boyfriendlass = boyfriend.flipped ? boyfriend.pos + new UnityEngine.Vector2(-boyfriend.CameraOffset.x, boyfriend.CameraOffset.y) : boyfriend.pos + new UnityEngine.Vector2(boyfriend.CameraOffset.x, boyfriend.CameraOffset.y);
                cameraPosiion = UnityEngine.Vector2.Lerp(cameraPosiion, boyfriendlass, 0.1f);

                scoretText.text = "";

                if (boyfriend.finished && this.manager.musicPlayer.song == null && !alreadygoingtoadifferentsecene)
                {
                    var song = new Music.Song(this.manager.musicPlayer, "FNF - Game Over", Music.MusicPlayer.MusicContext.Menu);

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
                        else if (gotblueballed)
                        {
                            //this.PlaySound(Plugin.fnfDeath, 0, 0.3f, 1f);
                            if (kvp == KeyCode.Escape)
                            {
                                
                                if (this.manager?.musicPlayer?.song != null)
                                {
                                    this.manager.musicPlayer.FadeOutAllSongs(40f);
                                }

                                this.manager.RequestMainProcessSwitch(Plugin.FunkinFreeplayMenu);
                                alreadygoingtoadifferentsecene = true;
                            }
                            else if (kvp == KeyCode.Return && !alreadygoingtoadifferentsecene)
                            {
                                alreadygoingtoadifferentsecene = true;
                                
                                if (this.manager?.musicPlayer?.song != null)
                                {
                                    this.manager.musicPlayer.song.subTracks[0].source.Stop();
                                    this.manager.musicPlayer.song = null;
                                }

                                this.PlaySound(Plugin.fnfRestart);
                                this.manager.RequestMainProcessSwitch(Plugin.FunkinRestart);
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

    }

    internal class FunkinFreeplayMenu : Menu.Menu // its fucking funny how the freeplay menu is so much smaller than the actual game
    {

        private MenuLabel menuLabel;

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

        public override void Singal(MenuObject sender, string message)
        {

            if (message == "PLAYFNFSONG")
            {
                this.PlaySound(SoundID.MENU_Player_Join_Game);
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
                        this.PlaySound(SoundID.MENU_Next_Slugcat);
                        Plugin.SelectedSong = songButtons[button];

                        menuLabel.text = "Currently Selected Song: " + Plugin.SelectedSong;

                        break;
                    }
                }
            }

        }

        public void CreateSongButton(string Song = "Unnamed Song")
        {

            int amountOfButtons = buttons.Count % 9;

            SimpleButton button = new(this, this.pages[0], Song, Song.ToUpper(), new(75 + (47.5f * (int)Mathf.Floor(amountOfButtons / 12)), 730 - (47.5f * amountOfButtons)), new(100, 35));

            this.pages[0].subObjects.Add(button);

            songButtons.Add(button, Song);

            buttons.Add(button);

        }

        private List<SimpleButton> buttons = new List<SimpleButton> { };
        private Dictionary<SimpleButton, string> songButtons = new Dictionary<SimpleButton, string> { };
        private Dictionary<SimpleButton, Action> buttonFuncs = new Dictionary<SimpleButton, Action> { };

    }

}

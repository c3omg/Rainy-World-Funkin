using Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RWF
{
    public class BPMChangeEvent
    {
        public int stepTime;
        public float songTime;
        public float bpm;
        public float stepCrochet = 0;
    }

    public static class Conductor
    {

        public static Song CurrentSong;
        public static float bpm = 100.0f;
        public static int curBeat;
        public static int curStep;
        public static float crochet = 0;
        public static float songPosition = 0;
        public static List<BPMChangeEvent> bpmChangeMap = new();


        public class ratingshit
        {
            public ratingshit(string name, float window = 0f, float ratingMod = 1f)
            {
                this.hitWindow = window;
                this.name = name;
                this.ratingMod = ratingMod;
            }

            public float hitWindow = 0f;
            public float ratingMod = 1f;
            public int hits = 0;
            public string name = "";
        }

        public static float step_crochet
        {
            get
            {
                return crochet / 4;
            }
        }

        public static int curSection
        {
            get
            {
                return curBeat / 4;
            }
        }

        public static Conductor.ratingshit judgeNote(Swagshit.Note note, List<ratingshit> data, float diff = 0f)
        {
            
            for (int i = 0; i < data.Count; i++)
            {
                bool flag = diff <= data[i].hitWindow;
                if (flag)
                {
                    data[i].hits++;
                    return data[i];
                }
            }
            return data[data.Count - 1];
        }

        public static float getCrotchetAtTime(float time)
        {
            var lastChange = getBPMFromSeconds(time);
            return lastChange.stepCrochet * 4;
        }

        public static BPMChangeEvent getBPMFromSeconds(float time)
        {
            BPMChangeEvent lastChange = new()
            {
                stepTime = 0,
                songTime = 0,
                bpm = bpm,
                stepCrochet = step_crochet,
            };

            for (int i = 0; i < Conductor.bpmChangeMap.Count; i++)
            {
                if (time >= Conductor.bpmChangeMap[i].songTime)
                    lastChange = Conductor.bpmChangeMap[i];
            }
            return lastChange;
        }

        public static BPMChangeEvent getBPMFromStep(float step)
        {
            BPMChangeEvent lastChange = new()
            {
                stepTime = 0,
                songTime = 0,
                bpm = bpm,
                stepCrochet = step_crochet,
            };

            for (int i = 0; i < Conductor.bpmChangeMap.Count; i++)
            {
                if (Conductor.bpmChangeMap[i].stepTime <= step)
                    lastChange = Conductor.bpmChangeMap[i];
            }

            return lastChange;
        }

        public static float beatToSeconds(float beat)
        {
            var step = beat * 4;
            var lastChange = getBPMFromStep(step);
            return lastChange.songTime + ((step - lastChange.stepTime) / (lastChange.bpm / 60) / 4) * 1000; // TODO: make less shit and take BPM into account PROPERLY
        }

        public static float getStep(float time)
        {
            var lastChange = getBPMFromSeconds(time);
            return lastChange.stepTime + (time - lastChange.songTime) / lastChange.stepCrochet;
        }

        public static float getStepRounded(float time)
        {
            var lastChange = getBPMFromSeconds(time);
            return lastChange.stepTime + Mathf.FloorToInt(time - lastChange.songTime) / lastChange.stepCrochet;
        }

        public static float getBeat(float time)
        {
            return getStep(time) / 4;
        }

        public static int getBeatRounded(float time)
        {
            return Mathf.FloorToInt(getStepRounded(time) / 4);
        }

        public static void mapBPMChanges(FNFJSON.Song song)
        {
            bpmChangeMap = new();

            float curBPM = song.bpm;
            int totalSteps = 0;
            float totalPos = 0;
            for (int i = 0; i < song.Sections.Count; i++)
            {
                if (song.Sections[i].changeBPM && song.Sections[i].bpm != curBPM)
                {
                    curBPM = song.Sections[i].bpm;
                    BPMChangeEvent lastChange = new()
                    {
                        stepTime = totalSteps,
                        songTime = totalPos,
                        bpm = curBPM,
                        stepCrochet = calculateCrochet(curBPM) / 4,
                    };
                    bpmChangeMap.Add(lastChange);
                }

                int deltaSteps = Mathf.RoundToInt(getSectionBeats(song, i) * 4);
                totalSteps += deltaSteps;
                totalPos += ((60 / curBPM) * 1000 / 4) * deltaSteps;
            }
            Debug.Log("new BPM map BUDDY " + bpmChangeMap);
        }

        public static float getSectionBeats(FNFJSON.Song song, int section)
        {
            float? val = null;
            if (song.Sections[section] != null) val = song.Sections[section].sectionBeats;
            return (float)(val != null ? val : 4);
        }

        public static float calculateCrochet(float bpm)
        {
            return (60 / bpm) * 1000;
        }

        public static float set_bpm(float newBPM)
        {
            bpm = newBPM;
            crochet = calculateCrochet(bpm);

            return bpm = newBPM;
        }

    }

}

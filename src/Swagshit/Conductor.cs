using Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RWF
{
    public static class Conductor
    {

        public static Song CurrentSong;
        public static float bpm = 100.0f;
        public static int curBeat;
        public static int curStep;
        public static float crochet = 0;
        public static float songPosition = 0;

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

    }
}

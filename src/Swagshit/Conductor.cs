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

    }
}

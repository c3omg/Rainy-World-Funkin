using System;
using UnityEngine;

namespace RWF.Swagshit
{
    internal class Utilities
    {
        public static string formatTime(float secs, bool ms) // Format Time just like Flixel
        {
            string timeString = ((int)(secs / 60) + ":").ToString();
            int timeStringHelper = (int)(secs % 60);
            if (timeStringHelper < 10) timeString += "0";
            timeString += timeStringHelper;

            if (ms == true)
            {
                timeString += ".";
                timeStringHelper = ((int)(secs - (int)(secs)) * 100);
                if (timeStringHelper < 10) timeString += "0";
                timeString += timeStringHelper;
            }
            return timeString;
        }

        public static float boundTo(float val, float min, float max) { return Mathf.Max(min, Mathf.Min(max, val)); } // probably doesnt work but meh i Do Not Care
    }
}

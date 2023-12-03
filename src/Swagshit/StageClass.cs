﻿using Menu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RWF.Swagshit
{

    public class StageJSON
    {

        public List<StagePieces> pieces = new List<StagePieces>();
        public float[] bf_scroll = new float[] { 1, 1 };
        public float[] dad_scroll = new float[] { 1, 1 };
        public float[] bf_pos = new float[] { 1100, 175 };
        public float[] dad_pos = new float[] { 330, 175 };
        public float zoom = 1f;
        public int bop_speed = 2;
        public float[] textColorOverride = new float[] {255, 255, 255};
        public float camSpeed = 0.05f;

        public StageJSON() { }

        public class StagePieces
        {
            // Token: 0x0400010E RID: 270
            public string spr_name;

            // Token: 0x0400010F RID: 271
            public Vector2 pos = Vector2.zero;

            // Token: 0x04000110 RID: 272
            public Vector2 scale = Vector2.zero;

            // Token: 0x04000111 RID: 273
            public Vector2 scroll = Vector2.zero;
        }
    }

    public class Stage : Menu.MenuObject
    {

        public UnityEngine.Vector2 bfscroll = new(1, 1);
        public UnityEngine.Vector2 dadscroll = new(1, 1);
        public UnityEngine.Vector2 bf_pos = new(1, 1);
        public UnityEngine.Vector2 dad_pos = new(1, 1);
        public Color textColorOverride = Color.white;
        private int failedattempts = 0;
        public float camSpeed = 0.1f;
        public int bop_speed = 2;

        public Stage(Menu.Menu menu, Menu.MenuObject owner) : base(menu, owner)
        {

        }

        public Stage(Menu.Menu menu, Menu.MenuObject owner, string rawData) : base(menu, owner)
        {
            this.LoadFromJSON(rawData);
        }

        public void LoadFromJSON(string rawData)
        {
            bool flag = this.sprites != null && this.sprites.Count > 0;
            if (flag)
            {
                foreach (FSprite fp in this.sprites)
                {
                    this.Container.RemoveChild(fp);
                }
            }
            this.sprites.Clear();
            this.spriteScrollFactor.Clear();
            this.spritePostions.Clear();
            try
            {
                StageJSON Data = JsonConvert.DeserializeObject<StageJSON>(rawData);
                foreach (StageJSON.StagePieces v in Data.pieces)
                {
                    Vector2 vector = v.pos;
                    FSprite sprite = new FSprite(v.spr_name, true);
                    sprite.SetAnchor(0f, 0f);
                    sprite.SetPosition(vector);
                    sprite.scaleX = v.scale.x;
                    sprite.scaleY = v.scale.y;
                    this.sprites.Add(sprite);
                    this.spritePostions.Add(sprite, vector);
                    this.spriteScrollFactor.Add(sprite, v.scroll);
                    this.spriteSize.Add(sprite, v.scale);
                    this.Container.AddChild(sprite);
                }
                this.camZoom = Data.zoom;
                this.bfscroll = new Vector2(Data.bf_scroll[0], Data.bf_scroll[1]);
                this.dadscroll = new Vector2(Data.dad_scroll[0], Data.dad_scroll[1]);
                this.textColorOverride = new Color(Data.textColorOverride[0] / 255f, Data.textColorOverride[1] / 255f, Data.textColorOverride[2] / 255f);
                this.bf_pos = new Vector2(Data.bf_pos[0], Data.bf_pos[1]);
                this.dad_pos = new Vector2(Data.dad_pos[0], Data.dad_pos[1]);
                this.bop_speed = Data.bop_speed;
                this.camSpeed = Data.camSpeed;
            }
            catch (Exception ex)
            {
                Debug.Log(string.Format("RWF MODDING: HAD A FAIL WHILE LOADING STAGE || Exception logged : {0}", ex));
                bool flag2 = this.failedattempts < 6;
                if (flag2)
                {
                    this.LoadFromJSON(File.ReadAllText(AssetManager.ResolveFilePath("funkin/stages/outskirts.json")));
                    this.failedattempts++;
                }
            }
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            foreach (FSprite fp in sprites)
            {
                fp.SetPosition((this.menu as FunkinMenu).GetPositionBasedOffCamScale(this.spritePostions[fp], false, this.spriteScrollFactor[fp]));
                fp.scaleX = spriteSize[fp].x * Plugin.camGameScale;
                fp.scaleY = spriteSize[fp].y * Plugin.camGameScale;
            }

        }

        public void Destroy()
        {
            if (this.sprites != null)
            {
                foreach (FSprite fp in sprites)
                {
                    this.Container.RemoveChild(fp);
                }
            }

            if (this.owner != null)
                this.page?.subObjects.Remove(this);
        }

        public List<FSprite> sprites = new List<FSprite>() { };
        public Dictionary<FSprite, UnityEngine.Vector2> spritePostions = new Dictionary<FSprite, UnityEngine.Vector2>() { };
        public Dictionary<FSprite, UnityEngine.Vector2> spriteScrollFactor = new Dictionary<FSprite, UnityEngine.Vector2>() { };
        public Dictionary<FSprite, UnityEngine.Vector2> spriteSize = new Dictionary<FSprite, UnityEngine.Vector2>() { };
        public float camZoom = 0.9f;

    }
}
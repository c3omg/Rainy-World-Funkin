using Menu;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace RWF.Swagshit
{

    public class StageJSON
    {

        public List<string[]> pieces = new List<string[]> { }; // spr_name, x, y, scaleX, scaleY, scrollX, scrollY
        public float[] bf_scroll = new float[] { 1, 1 };
        public float[] dad_scroll = new float[] { 1, 1 };
        public float[] bf_pos = new float[] { 1100, 175 };
        public float[] dad_pos = new float[] { 330, 175 };
        public float zoom = 1f;
        public int bop_speed = 2;
        public float[] textColorOverride = new float[] {255, 255, 255};
        public float camSpeed = 0.1f;

        public StageJSON() { }
    }

    public class Stage : Menu.MenuObject
    {

        public UnityEngine.Vector2 bfscroll = new(1, 1);
        public UnityEngine.Vector2 dadscroll = new(1, 1);
        public UnityEngine.Vector2 bf_pos = new(1, 1);
        public UnityEngine.Vector2 dad_pos = new(1, 1);
        public Color textColorOverride = Color.white;
        public float camSpeed = 0.1f;
        public int bop_speed = 2;

        public Stage(Menu.Menu menu, Menu.MenuObject owner) : base(menu, owner)
        {

        }

        public Stage(Menu.Menu menu, Menu.MenuObject owner, string rawData) : base(menu, owner)
        {

            var Data = JsonConvert.DeserializeObject<StageJSON>(rawData);

            foreach (string[] v in Data.pieces)
            {

                UnityEngine.Vector2 vector = new(float.Parse(v[1]), float.Parse(v[2]));

                FSprite sprite = new FSprite(v[0]);
                sprite.SetAnchor(0, 0);
                sprite.SetPosition(vector);

                sprites.Add(sprite);
                spritePostions.Add(sprite, vector);
                spriteScrollFactor.Add(sprite, new(float.Parse(v[5]), float.Parse(v[6])));
                spriteSize.Add(sprite, new(float.Parse(v[3]), float.Parse(v[4])));

                this.Container.AddChild(sprite);

            }

            this.camZoom = Data.zoom;
            this.bfscroll = new(Data.bf_scroll[0], Data.bf_scroll[1]);
            this.dadscroll = new(Data.dad_scroll[0], Data.dad_scroll[1]);
            this.textColorOverride = new Color(Data.textColorOverride[0] /255, Data.textColorOverride[1] /255, Data.textColorOverride[2] /255);
            this.bf_pos = new(Data.bf_pos[0], Data.bf_pos[1]);
            this.dad_pos = new(Data.dad_pos[0], Data.dad_pos[1]);
            this.bop_speed = Data.bop_speed;
            this.camSpeed = Data.camSpeed;

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

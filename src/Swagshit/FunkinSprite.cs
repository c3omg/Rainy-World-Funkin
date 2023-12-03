using Menu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEngine;

namespace RWF.Swagshit
{
    public class FunkinSprite : Menu.MenuObject
    {

        public FSprite sprite;
        public UnityEngine.Vector2 pos;
        public Vector2 scrollFactor = new Vector2(1, 1);

        public FunkinSprite(Menu.Menu menu, Menu.MenuObject owner) : base(menu, owner) // why didnt i fucking add this from the start
        {
            this.sprite = new("Futile_White");

            this.Container.AddChild(sprite);

            scrollFactor = new Vector2(0.9f, 0.9f);
        }

        public void Destroy()
        {
            if (this.sprite != null)
                this.Container.RemoveChild(this.sprite);

            if (this.owner != null)
                (this.page as Page).subObjects.Remove(this);
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            sprite.SetPosition((this.menu as FunkinMenu).GetPositionBasedOffCamScale(pos, true, scrollFactor));
        }

    }
}

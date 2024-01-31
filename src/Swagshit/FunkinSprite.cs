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
        public Vector2 pos;
        public Vector2 lastpos = Vector2.zero;
        public Vector2 scrollFactor = Vector2.one;
        public Vector2 spriteOffset = Vector2.zero;
        public bool IsPartOfHUD = false;

        public FunkinMenu funkinMenu
        {
            get
            {
                return this.menu as FunkinMenu;
            }
        }

        public bool isInFunkinSecene
        {
            get
            {
                return this.menu is FunkinMenu;
            }
        }

        public FunkinSprite(Menu.Menu menu, Menu.MenuObject owner, bool IsPartOfHUD = false) : base(menu, owner) // why didnt i fucking add this from the start
        {
            this.sprite = new("Futile_White");

            this.Container.AddChild(sprite);

            this.IsPartOfHUD = IsPartOfHUD;
        }

        public void Destroy()
        {
            if (this.sprite != null)
                this.Container.RemoveChild(this.sprite);

            if (this.owner != null)
                (this.page as Page).subObjects.Remove(this);
        }

        public override void Update()
        {
            base.Update();

            lastpos = pos;
        }

        private Vector2 GetPositionBasedOffCamScale(float timeStacker) // i wasnt tired when writing this, i just dont know how to fucking code alrighted
        {
            var scale = 1;
            Vector2 trueVecoter = Vector2.Lerp(lastpos, pos, timeStacker) + spriteOffset;

            if (!isInFunkinSecene) return trueVecoter;

            Vector2 vector = this.menu.manager.rainWorld.screenSize;
            vector /= 2;

            Vector2 cameraPosiion = funkinMenu.cameraPosiion;

            trueVecoter.x -= (cameraPosiion.x - vector.x) * scrollFactor.x;
            trueVecoter.y -= (cameraPosiion.y - vector.y) * scrollFactor.y;

            return (trueVecoter + ((trueVecoter - vector) * (scale - 1)));
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            sprite.SetPosition(GetPositionBasedOffCamScale(timeStacker));
        }

    }
}

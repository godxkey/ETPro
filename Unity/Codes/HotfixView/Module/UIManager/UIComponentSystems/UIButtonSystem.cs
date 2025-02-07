﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ET
{
    [UISystem]
    [FriendClass(typeof (UIButton))]
    public class UIButtonOnCreateSystem: OnCreateSystem<UIButton>
    {
        public override void OnCreate(UIButton self)
        {
            self.grayState = false;
        }
    }

    [UISystem]
    [FriendClass(typeof (UIButton))]
    public class UIButtonOnDestroySystem: OnDestroySystem<UIButton>
    {
        public override void OnDestroy(UIButton self)
        {
            if (self.onClick != null)
                self.button.onClick.RemoveListener(self.onClick);
            if (!string.IsNullOrEmpty(self.spritePath))
                ImageLoaderComponent.Instance?.ReleaseImage(self.spritePath);
            self.onClick = null;
        }
    }

    [FriendClass(typeof (UIButton))]
    public static class UIButtonSystem
    {
        static void ActivatingComponent(this UIButton self)
        {
            if (self.button == null)
            {
                self.button = self.GetGameObject().GetComponent<Button>();
                if (self.button == null)
                {
                    Log.Error($"添加UI侧组件UIButton时，物体{self.GetGameObject().name}上没有找到Button组件");
                }
            }
        }

        static void ActivatingImageComponent(this UIButton self)
        {
            if (self.image == null)
            {
                self.image = self.GetGameObject().GetComponent<Image>();
                if (self.image == null)
                {
                    Log.Error($"添加UI侧组件UIButton时，物体{self.GetGameObject().name}上没有找到Image组件");
                }
            }
        }

        /// <summary>
        /// 虚拟点击
        /// </summary>
        /// <param name="self"></param>
        public static void Click(this UIButton self)
        {
            self.onClick?.Invoke();
        }

        public static void SetOnClick(this UIButton self, Action callback)
        {
            self.ActivatingComponent();
            self.RemoveOnClick();
            self.onClick = () =>
            {
                //SoundComponent.Instance.PlaySound("Audio/Common/Click.mp3");
                callback?.Invoke();
            };
            self.button.onClick.AddListener(self.onClick);
        }

        public static void RemoveOnClick(this UIButton self)
        {
            if (self.onClick != null)
                self.button.onClick.RemoveListener(self.onClick);
            self.onClick = null;
        }

        public static void SetEnabled(this UIButton self, bool flag)
        {
            self.ActivatingComponent();
            self.button.enabled = flag;
        }

        public static void SetInteractable(this UIButton self, bool flag)
        {
            self.ActivatingComponent();
            self.button.interactable = flag;
        }

        /// <summary>
        /// 设置按钮变灰
        /// </summary>
        /// <param name="isGray">是否变灰</param>
        /// <param name="includeText">是否包含字体, 不填的话默认为true</param>
        /// <param name="affectInteractable">是否影响交互, 不填的话默认为true</param>
        public static async ETTask SetBtnGray(this UIButton self, bool isGray, bool includeText = true, bool affectInteractable = true)
        {
            if (self.grayState == isGray) return;
            self.ActivatingImageComponent();
            self.grayState = isGray;
            var mat = await MaterialComponent.Instance.LoadMaterialAsync("UI/UICommon/Materials/uigray.mat");
            if (affectInteractable)
            {
                self.image.raycastTarget = !isGray;
            }

            self.SetBtnGray(mat, isGray, includeText);
        }

        public static void SetBtnGray(this UIButton self, Material grayMaterial, bool isGray, bool includeText)
        {
            self.ActivatingImageComponent();
            GameObject go = self.GetGameObject();
            if (go == null)
            {
                return;
            }

            Material mt = null;
            if (isGray)
            {
                mt = grayMaterial;
            }

            var coms = go.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < coms.Length; i++)
            {
                coms[i].material = mt;
            }

            if (includeText)
            {
                var textComs = go.GetComponentsInChildren<Text>();
                for (int i = 0; i < textComs.Length; i++)
                {
                    var uITextColorCtrl = TextColorCtrl.Get(textComs[i].gameObject);
                    if (isGray)
                    {
                        uITextColorCtrl.SetTextColor(new Color(89 / 255f, 93 / 255f, 93 / 255f));
                    }
                    else
                    {
                        uITextColorCtrl.ClearTextColor();
                    }
                }
            }
        }

        public static async ETTask SetSpritePath(this UIButton self, string sprite_path)
        {
            if (string.IsNullOrEmpty(sprite_path)) return;
            if (sprite_path == self.spritePath) return;
            self.ActivatingImageComponent();
            var base_sprite_path = self.spritePath;
            self.spritePath = sprite_path;
            var sprite = await ImageLoaderComponent.Instance.LoadImageAsync(sprite_path);
            if (sprite == null)
            {
                ImageLoaderComponent.Instance.ReleaseImage(sprite_path);
                return;
            }

            if (!string.IsNullOrEmpty(base_sprite_path))
                ImageLoaderComponent.Instance.ReleaseImage(base_sprite_path);

            self.image.sprite = sprite;
        }

        public static string GetSpritePath(this UIButton self)
        {
            return self.spritePath;
        }

        public static void SetImageColor(this UIButton self, Color color)
        {
            self.ActivatingImageComponent();
            self.image.color = color;
        }
    }
}
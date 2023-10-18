using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using System.Collections;

namespace Burger
{
    public class Milkshake : PlayerItem
    {
        public float Duration = 3f;

        public float DamageMultiplier = 2f;

        public Color flatColorOverride = new Color(0.5f, 0f, 0f, 0.75f);

        public GameObject OverheadVFX;

        private bool m_isRaged;

        private float m_elapsed;

        private GameObject instanceVFX;

        private PlayerController m_player;

        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "Milkshake";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "Burger/Resources/milkshake";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Milkshake>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Shaken Not Stirred";
            string longDesc = "All of the milkshake's goodness are stuck inside!\n\n" +
                "This angers you immensely. Why can't the milkshake just be ready to drink right when you pay for it!";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bur");

            //Adds the actual passive effect to the item
            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, 1, StatModifier.ModifyMethod.ADDITIVE);
            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Coolness, 1);

            //Set the cooldown
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.PerRoom, 2f);
            item.consumable = false;


            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.C;
        }
        public override void Pickup(PlayerController player)
        {
            if (!m_pickedUp)
            {
                base.Pickup(player);
                m_player = player;
            }
        }
        private IEnumerator HandleRage()
        {
            m_isRaged = true;
            instanceVFX = null;
            if ((bool)OverheadVFX)
            {
                instanceVFX = m_player.PlayEffectOnActor(OverheadVFX, new Vector3(0f, 1.375f, 0f), attached: true, alreadyMiddleCenter: true);
            }
            StatModifier damageStat = new StatModifier
            {
                amount = DamageMultiplier,
                modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE,
                statToBoost = PlayerStats.StatType.Damage
            };
            m_player.ownerlessStatModifiers.Add(damageStat);
            m_player.stats.RecalculateStats(m_player);
            if (m_player.CurrentGun != null)
            {
                m_player.CurrentGun.ForceImmediateReload();
            }
            m_elapsed = 0f;
            float particleCounter = 0f;
            while (m_elapsed < Duration)
            {
                m_elapsed += BraveTime.DeltaTime;
                m_player.baseFlatColorOverride = flatColorOverride.WithAlpha(Mathf.Lerp(flatColorOverride.a, 0f, Mathf.Clamp01(m_elapsed - (Duration - 1f))));
                if ((bool)instanceVFX && m_elapsed > 1f)
                {
                    instanceVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out");
                    instanceVFX = null;
                }
                if (GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW && (bool)m_player && m_player.IsVisible && !m_player.IsFalling)
                {
                    particleCounter += BraveTime.DeltaTime * 40f;
                    if (particleCounter > 1f)
                    {
                        int num = Mathf.FloorToInt(particleCounter);
                        particleCounter %= 1f;
                        GlobalSparksDoer.DoRandomParticleBurst(num, m_player.sprite.WorldBottomLeft.ToVector3ZisY(), m_player.sprite.WorldTopRight.ToVector3ZisY(), Vector3.up, 90f, 0.5f, null, null, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
                    }
                }
                yield return null;
            }
            if ((bool)instanceVFX)
            {
                instanceVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out");
            }
            m_player.ownerlessStatModifiers.Remove(damageStat);
            m_player.stats.RecalculateStats(m_player);
            m_isRaged = false;
        }

        public override void DoEffect(PlayerController obj)
        {
            if (m_isRaged)
            {
                if ((bool)OverheadVFX && !instanceVFX)
                {
                    instanceVFX = m_player.PlayEffectOnActor(OverheadVFX, new Vector3(0f, 1.375f, 0f), attached: true, alreadyMiddleCenter: true);
                }
                m_elapsed = 0f;
            }
            else
            {
                obj.StartCoroutine(HandleRage());
            }
            Module.Log($"Attempted to rage");
        }


    }
}

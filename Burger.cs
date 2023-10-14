using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;

namespace Burger
{
    public class Burger : PassiveItem
    {
        int shieldOnPickup = 1;
        int shieldOnFloorCleared = 1;
        int shieldOnFloorClearedTotal = 0;
        //Call this method from the Start() method of your ETGModule extension
        public static void Register()
        {
            //The name of the item
            string itemName = "B.U.R.G.E.R.";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "Burger/Resources/burger";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Burger>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Long Acronyms Taste Better";
            string longDesc = "The B.U.R.G.E.R. is also known as fast food.\n\n" +
                "This beautifully hand-crafted B.U.R.G.E.R. stands for Barrier Utilizing Reinforcement Generating Energy Reserves and gives the gungeonier shield every floor. Thank goodness for unsustainable food production practiced by companies that strive only to make an extra dime!";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "bur");

            //Adds the actual passive effect to the item
            //ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Health, 1, StatModifier.ModifyMethod.ADDITIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Coolness, 1);

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.B;


            Module.Log($"B.U.R.G.E.R. mod successfully started");

        }


        //Keep track of how many times the player has entered a new chamber or picked up the item
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.healthHaver.Armor += shieldOnPickup;
            Module.Log($"Player picked up B.U.R.G.E.R. successfully");
            player.OnNewFloorLoaded += NewFloorLoaded;

        }

        public override void DisableEffect(PlayerController player)
        {
            Module.Log($"Player dropped or got rid of B.U.R.G.E.R. successfully");
        }
        
        //Keep track of how many times the player has dropped the item
        public override DebrisObject Drop(PlayerController player)
        {
            player.OnNewFloorLoaded -= this.NewFloorLoaded;
            player.healthHaver.Armor -= shieldOnPickup;
            player.healthHaver.Armor -= shieldOnFloorClearedTotal;
            return base.Drop(player);
        }

        bool half_toggle = false;
        public void NewFloorLoaded(PlayerController player)
        {
            if (half_toggle)
            {
                player.healthHaver.Armor += shieldOnFloorCleared;
                half_toggle = false;
                shieldOnFloorClearedTotal++;
            }
            else
            {
                half_toggle = true;
            }
          
        }

        public static void SynergyList()
        {
            //Set the required items for the synergy
            List<string> mandatoryConsoleIDs = new List<string>
            {
                "bur:b.u.r.g.e.r.",
                "bur:friguon",
            };
            List<string> optionalConsoleIDs = new List<string>
            {
                "",
            };
            CustomSynergies.Add("Combo Meal", mandatoryConsoleIDs, optionalConsoleIDs, true);


        }

        public void GotSynergy(PlayerController player)
        {

            if (player.PlayerHasActiveSynergy("Combo Meal"))
            {
                player.healthHaver.maximumHealth += 1;

                Module.Log($"Combo Meal synergy successfully applied");
            }
        }

    }
}

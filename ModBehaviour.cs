using System;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace NeedForCrown
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private static string MOD_NAME = "NeedForCrown";

        private static int CROWN_ID = 1254, KEY_X = 827, KEY_O = 828;

        private static string LEVEL_FARM = "Level_Farm_01";

        private static string SOUND_HIGH = "event:/UI/game_start";

        void OnEnable()
        {
            Log("OnEnable");
            LevelManager.OnLevelInitialized += LevelManager_OnLevelInitialized;
        }

        void OnDisable()
        {
            Log("OnDisable");
            LevelManager.OnLevelInitialized -= LevelManager_OnLevelInitialized;
        }

        private void LevelManager_OnLevelInitialized()
        {
            string scene_name = SceneManager.GetActiveScene().name;
            Log($"OnLevelInitialized: {scene_name}");

            if (scene_name != LEVEL_FARM)
                return;

            var inventory = LevelManager.Instance?.PetProxy?.Inventory;
            if (inventory == null)
                return;

            bool key_x = false, key_o = false;
            foreach (var item in inventory)
            {
                if (item.TypeID == KEY_X)
                    key_x = true;
                else if (item.TypeID == KEY_O)
                    key_o = true;
                if (item.Slots != null)
                    foreach (var slot in item.Slots)
                    {
                        if (slot.Content == null)
                            continue;
                        if (slot.Content.TypeID == KEY_X)
                            key_x = true;
                        else if (slot.Content.TypeID == KEY_O)
                            key_o = true;
                    }
            }
            Log($"KeyX: {key_x}, KeyO: {key_o}");
            if (!(key_x && key_o))
            {
                Log("狗子身上需要有神秘钥匙X+O");
                return;
            }

            bool crown = false;
            List<(string, int)> items = new List<(string, int)>();
            foreach(var item in UnityEngine.Object.FindObjectsByType<ItemStatsSystem.Item>(FindObjectsSortMode.None))
            {
                int type_id = item.TypeID;
                // 6张实验室门禁卡 / 神秘钥匙XO / 皇冠
                if (!(type_id == 801 || type_id == 802 || type_id == 803 || type_id == 804 || type_id == 886 || type_id == 887 || type_id == 827 || type_id == 828 || type_id == CROWN_ID))
                    continue;
                if (item.FromInfoKey == "Ground")
                {
                    items.Add((item.DisplayName, type_id));
                    Log($"{item.DisplayName} ({type_id})");
                    if (type_id == CROWN_ID)
                        crown = true;
                }
            }

            if (items.Count == 0)
                return;

            if (!crown)
            {
                string items_str = "";
                foreach (var (name, id) in items)
                    items_str += $" {name}";
                CharacterMainControl.Main.PopText($"地上有: {items_str}");
            }
            else
            {
                CharacterMainControl.Main.PopText($"OHHH! 皇冠!!!");
                EventInstance eventInstance = RuntimeManager.CreateInstance(SOUND_HIGH);
                eventInstance.setVolume(1F);
                eventInstance.start();
                eventInstance.release();
            }
        }

        void Log(string msg)
        {
            Debug.Log($"> {MOD_NAME}: {msg}");
        }
    }
}

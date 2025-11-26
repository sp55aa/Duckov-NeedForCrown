using System;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace NeedForCrown
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private static string MOD_NAME = "NeedForCrown";

        private static int CROWN_ID = 1254, KEY_X = 827, KEY_O = 828, CARD_PURPLE = 887;

        private static string LEVEL_FARM = "Level_Farm_01";

        private static string SOUND_HIGH = "event:/UI/game_start";
        private static string SOUND_MEDIUM = "event:/UI/sceneloader_click";

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

            if (scene_name != LEVEL_FARM) {
                return;
            }

            var (key_x, key_o) = FindKeyXO(LevelManager.Instance?.PetProxy?.Inventory, false, false);
            (key_x, key_o) = FindKeyXO(LevelManager.Instance?.MainCharacter?.CharacterItem?.Inventory, key_x, key_o);
            Log($"KeyX: {key_x}, KeyO: {key_o}");
            if (!(key_x && key_o)) {
                Log("需要带上神秘钥匙X和神秘钥匙O");
                return;
            }

            HashSet<int> self_items = new HashSet<int>();
            GetHighValueInstIDs(LevelManager.Instance?.PetProxy?.Inventory, self_items);
            GetHighValueInstIDs(LevelManager.Instance?.MainCharacter?.CharacterItem?.Inventory, self_items);

            bool crown = false;
            List<(string, int)> items = new List<(string, int)>();
            foreach(var item in UnityEngine.Object.FindObjectsByType<ItemStatsSystem.Item>(FindObjectsSortMode.None)) {
                int type_id = item.TypeID;
                // 6张实验室门禁卡 / 神秘钥匙XO / 皇冠
                if (!HighValueItem(type_id))
                    continue;
                if (self_items.Contains(item.GetInstanceID())) {
                    Log($"--忽略自带物品: {item.DisplayName} ({type_id}) {item.GetInstanceID()} {item.FromInfoKey}");
                    continue;
                }
                if (item.FromInfoKey != "Ground") {
                    Log($"--忽略不是地上的物品: {item.DisplayName} ({type_id}) {item.GetInstanceID()} {item.FromInfoKey}");
                    continue;
                }
                items.Add((item.DisplayName, type_id));
                Log($"{item.DisplayName} ({type_id}) {item.GetInstanceID()} {item.FromInfoKey}");
                if (type_id == CROWN_ID)
                    crown = true;
            }

            if (items.Count == 0) {
                CharacterMainControl.Main.PopText("皇冠呢？");
                return;
            }

            string sound = SOUND_HIGH, msg = "OHHH! 皇冠!!!";
            float vol = 4f;
            if (!crown)
            {
                sound = SOUND_MEDIUM;
                msg = "地上有:";
                foreach (var (name, id) in items) {
                    msg += $" {name}";
                    if (id == CARD_PURPLE)
                        msg += "(可能在紫卡小屋?)";
                }
            }
            CharacterMainControl.Main.PopText(msg);
            EventInstance eventInstance = RuntimeManager.CreateInstance(sound);
            eventInstance.setVolume(vol);
            eventInstance.start();
            eventInstance.release();
        }

        (bool, bool) FindKeyXO(Inventory items, bool key_x, bool key_o)
        {
            if (items == null)
                return (key_x, key_o);
            foreach (var item in items) {
                if (key_x && key_o)
                    break;
                if (item.TypeID == KEY_X)
                    key_x = true;
                else if (item.TypeID == KEY_O)
                    key_o = true;
                if (item.Slots != null)
                    foreach (var slot in item.Slots) {
                        if (slot.Content == null)
                            continue;
                        if (slot.Content.TypeID == KEY_X)
                            key_x = true;
                        else if (slot.Content.TypeID == KEY_O)
                            key_o = true;
                    }
            }
            return (key_x, key_o);
        }

        void GetHighValueInstIDs(Inventory items, HashSet<int> ids)
        {
            if (items == null)
                return;
            foreach (var item in items) {
                if (HighValueItem(item.TypeID)) {
                    ids.Add(item.GetInstanceID());
                    Log($"+{item.DisplayName}({item.TypeID}) {item.GetInstanceID()}");
                }
                if (item.Slots != null)
                    foreach (var slot in item.Slots)
                        if (slot.Content != null && HighValueItem(slot.Content.TypeID)) {
                            ids.Add(slot.Content.GetInstanceID());
                            Log($"+{slot.Content.DisplayName}({slot.Content.TypeID}) {slot.Content.GetInstanceID()}");
                        }
            }
        }

        bool HighValueItem(int type_id)
        {
            return type_id == 801 || type_id == 802 || type_id == 803 || type_id == 804 || type_id == 886 || type_id == 887 || type_id == 827 || type_id == 828 || type_id == CROWN_ID;
        }

        void Log(string msg)
        {
            Debug.Log($"> {MOD_NAME}: {msg}");
        }
    }
}

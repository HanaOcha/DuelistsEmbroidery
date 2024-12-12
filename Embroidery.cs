using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;

using UnityEngine;
using TMPro;

using HarmonyLib;
using MelonLoader;

[assembly: MelonInfo(typeof(Embroidery_DOEMod.Embroidery), "Embroidery", "1.0.0", "HanaOcha")]
[assembly: MelonGame()]

namespace Embroidery_DOEMod
{
    [HarmonyPatch(typeof(AssetLibrary), "LoadMods")]
    internal class Embroidery : MelonMod
    {
        [HarmonyPostfix]
        internal static void Load(AssetLibrary __instance)
        {
            __instance.StartCoroutine(EmbroideryData.GetFlavorTexts());
        }

        [HarmonyPatch(typeof(TipCard), nameof(TipCard.Set))]
        [HarmonyPostfix]
        internal static void SetDeck(TipCard __instance, ItemData itemData)
        {
            Transform embroidery = __instance.cardInner.transform.Find("Embroidery");
            if (embroidery == null)
            {
                TextMeshProUGUI setup = new GameObject().AddComponent<TextMeshProUGUI>();
                embroidery = setup.transform;
                embroidery.SetParent(__instance.cardInner.transform, false);
                embroidery.name = "Embroidery";

                setup.alignment = TextAlignmentOptions.Left;
                setup.verticalAlignment = VerticalAlignmentOptions.Bottom;
                setup.fontStyle = FontStyles.Italic;
                setup.enableWordWrapping = true;
                setup.color = Color.white * 0.7f;
                setup.fontSize = __instance.cardInner.description.fontSize - 0.6f;
                setup.rectTransform.sizeDelta = new Vector2(40f, setup.rectTransform.sizeDelta.y);

                embroidery.localPosition += new Vector3(-7, -16.5f, 0);
            }

            bool frameData = false;
            if (CharacterSelectController.I.deckbuilderController.showFrameData)
            {
                frameData = true;
            }

            TextMeshProUGUI flavor = embroidery.GetComponent<TextMeshProUGUI>();
            flavor.text = string.Empty;
            if (!frameData && EmbroideryData.flavorTexts.ContainsKey(itemData.id))
            {
                flavor.text = EmbroideryData.flavorTexts[itemData.id];
            }
        }
    }
    internal class EmbroideryData
    {
        public static readonly string link = "https://raw.githubusercontent.com/HanaOcha/DuelistsEmbroidery/main/";
        public static readonly string filePath = "Flavors.txt";
        internal static Dictionary<string, string> flavorTexts = new Dictionary<string, string>();
        internal static IEnumerator GetFlavorTexts()
        {
            flavorTexts.Clear();

            Debug.Log("EMBROIDERY MOD;");
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link + filePath);
                request.UserAgent = "Duelists Embroidery Request";
                Debug.Log("Requesting \"" + filePath + "\" from \"" + link + "\"");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                Debug.Log("Reading " + filePath);

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.StartsWith("//"))
                    {
                        string[] data = line.Split(";");
                        if (data.Length >= 2 && AssetLibrary.spells.ContainsKey(data[0]))
                        {
                            flavorTexts.Add(data[0], data[1]);
                        }
                    }
                }
                Debug.Log("Added flavor text for " + flavorTexts.Count + " spells");
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }

            yield break;
        }
    }
}

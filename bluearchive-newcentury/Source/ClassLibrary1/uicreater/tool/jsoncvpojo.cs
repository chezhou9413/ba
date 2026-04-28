using BANWlLib.mainUI.pojo;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Verse;

namespace newpro
{
    public class jsoncvpojo
    {
        public static UIhead LoadUIheadFromJson(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {            
                return null;
            }

            try
            {
                string jsonText = File.ReadAllText(jsonPath);
                UIhead obj = JsonConvert.DeserializeObject<UIhead>(jsonText);
                return obj;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public static UIbody LoadUIbodyFromJson(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                return null;
            }

            try
            {
                string jsonText = File.ReadAllText(jsonPath);
                UIbody obj = JsonConvert.DeserializeObject<UIbody>(jsonText);
                return obj;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public static List<poitlist> LoadPoitlistFromJson(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                return null;
            }

            try
            {
                string jsonText = File.ReadAllText(jsonPath);
                List<poitlist> obj = JsonConvert.DeserializeObject<List<poitlist>>(jsonText);
                return obj;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public static List<shot> LoadShotFromJson(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                return null;
            }

            try
            {
                string jsonText = File.ReadAllText(jsonPath);
                List<shot> obj = JsonConvert.DeserializeObject<List<shot>>(jsonText);
                return obj;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using SpawnConfig.ExtendedClasses;

public class JsonManager {

    public static List<ExtendedGroupCounts> GetEGCListFromJSON(string path){
        List<ExtendedGroupCounts> temp = [];
        if(File.Exists(path)){
            string readFile = File.ReadAllText(path);
            if(readFile != null && readFile != ""){
                temp = JsonConvert.DeserializeObject<List<ExtendedGroupCounts>>(readFile);
            }
        }
        return temp;
    }

    public static List<ExtendedEnemySetup> GetEESListFromJSON(string path){
        List<ExtendedEnemySetup> temp = [];
        if(File.Exists(path)){
            string readFile = File.ReadAllText(path);
            if(readFile != null && readFile != ""){
                temp = JsonConvert.DeserializeObject<List<ExtendedEnemySetup>>(readFile);
            }
        }
        return temp;
    }

    public static string GroupCountsToJSON(List<ExtendedGroupCounts> gcList) {

        StringBuilder json = new();
        StringWriter sw = new(json);
        using (JsonWriter writer = new JsonTextWriter(sw))
        {
            writer.Formatting = Formatting.Indented;
            writer.WriteStartArray();

            foreach(ExtendedGroupCounts gc in gcList){
                writer.Formatting = Formatting.Indented;
                writer.WriteStartObject();
                writer.WritePropertyName("level");
                writer.WriteValue(gc.level);
                writer.WritePropertyName("possibleGroupCounts");
                writer.WriteStartArray();
                writer.WriteStartArray();
                writer.Formatting = Formatting.None;
                writer.WriteValue(gc.possibleGroupCounts[0][0]);
                writer.WriteValue(gc.possibleGroupCounts[0][1]);
                writer.WriteValue(gc.possibleGroupCounts[0][2]);
                writer.WriteEndArray();
                writer.Formatting = Formatting.Indented;
                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        return json.ToString();

    }

}
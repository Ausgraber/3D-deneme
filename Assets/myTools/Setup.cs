using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

using static UnityEditor.AssetDatabase;


public class Setup
{
    [MenuItem("Tools/Setup/Creat Defaults Folders")]
    public static void CreateDefaultFolder(){
        string[] foldersName={"Animation","Art","Materials","Prefabs","ScriptableObjects","Scripts","Settings"};
        Folders.CreateDefault(root:"_Project" , foldersName);
        Refresh();
    }
    
    static class Folders{
        public static void CreateDefault(string root, params string[] folders){

            var fullpath = Path.Combine(Application.dataPath,root);
            foreach(var folder in folders){
                var path = Path.Combine(fullpath,folder);
                if(!Directory.Exists(path)){
                    Directory.CreateDirectory(path);
                }
            }
        }
    }
}

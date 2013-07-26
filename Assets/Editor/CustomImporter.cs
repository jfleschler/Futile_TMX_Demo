using UnityEngine;
using UnityEditor;
using System.Collections;


class MyAllPostprocessor : AssetPostprocessor {
	
	static void OnPostprocessAllAssets (
		string[] importedAssets,
		string[] deletedAssets,
		string[] movedAssets,
		string[] movedFromAssetPaths) {
		
		foreach (string str in importedAssets) {
			//Debug.Log("Reimported Asset: " + str);
			
			if (str.Contains(".tmx")) {
				string newName = str.Substring(0,str.Length-4) + ".txt";
				
				bool r = AssetDatabase.DeleteAsset(newName);
				if (r) Debug.Log("Previous file found and Deleted");
				
				string result = AssetDatabase.MoveAsset(str, newName);
				if(result != "") Debug.Log(result);
				
				AssetDatabase.ImportAsset(newName);
			}
		}
		
		foreach (string str in deletedAssets)
			Debug.Log("Deleted Asset: " + str);
		
		for (int i=0;i<movedAssets.Length;i++)
			Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
	}
}
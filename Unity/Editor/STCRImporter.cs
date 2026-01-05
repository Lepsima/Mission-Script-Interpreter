using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "stcr")]
public class STCRImporter : ScriptedImporter {
	public override void OnImportAsset(AssetImportContext ctx) {
		string text = System.IO.File.ReadAllText(ctx.assetPath);
		TextAsset asset = new(text);
		ctx.AddObjectToAsset("main", asset);
		ctx.SetMainObject(asset);
	}
}
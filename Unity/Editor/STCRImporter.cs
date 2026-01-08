using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "stcr")]
public class STCRImporter : ScriptedImporter {
	public override void OnImportAsset(AssetImportContext ctx) {
		// ReSharper disable once RedundantNameQualifier
		string text = System.IO.File.ReadAllText(AssetImportContext.assetPath);
		TextAsset asset = new(text);
		ctx.AddObjectToAsset("main", asset);
		ctx.SetMainObject(asset);
	}
}
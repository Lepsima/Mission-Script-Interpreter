using UnityEditor.AssetImporters;
using UnityEngine;
using System.IO;

[ScriptedImporter(1, "stcr")]
public class STCRImporter : ScriptedImporter {
	public override void OnImportAsset(AssetImportContext ctx) {
		// ReSharper disable once RedundantNameQualifier
		string text = File.ReadAllText(ctx.assetPath);
		TextAsset asset = new(text);
		ctx.AddObjectToAsset("main", asset);
		ctx.SetMainObject(asset);
	}
}
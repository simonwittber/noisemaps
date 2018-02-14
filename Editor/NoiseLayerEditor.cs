using UnityEditor;
using UnityEngine;

namespace DifferentMethods.NoiseMaps
{
    [CustomEditor(typeof(NoiseLayer))]
    public class NoiseLayerEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            var nl = target as NoiseLayer;
            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                base.OnInspectorGUI();
                if (cc.changed)
                {
                    nl.Refresh();
                }
            }
            if (GUILayout.Button("Bake2D"))
                Bake2D(nl);
        }

        void Bake2D(NoiseLayer nl)
        {
            var t = new Texture2D(nl.size, nl.size, TextureFormat.ARGB32, mipmap: false);
            var p = t.GetPixels();
            nl.GenerateTexture(p, nl.size, nl.size);
            t.SetPixels(p);
            t.Apply();
            var bytes = t.EncodeToPNG();
            var path = AssetDatabase.GenerateUniqueAssetPath(AssetDatabase.GetAssetPath(nl) + ".png");
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
        }

        public override bool HasPreviewGUI() => true;

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            var nl = target as NoiseLayer;
            if (nl.Texture != null)
                GUI.DrawTexture(r, nl.Texture);
        }
    }
}
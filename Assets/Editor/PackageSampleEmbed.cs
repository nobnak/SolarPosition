using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using System.IO;

[InitializeOnLoad]
public class PackageSampleEmbed {
    
    static PackageSampleEmbed() {
        // スクリプトのコンパイルが完了したときに呼ばれる
        CompilationPipeline.compilationStarted += OnCompilationStarted;
    }
    
    private const string SAMPLES_SOURCE_PATH = "Assets/Samples";
    private const string SAMPLES_DEST_PATH = "Packages/jp.nobnak.solar/Samples~";

    static void OnCompilationStarted(object context) {
         Debug.Log("OnPreprocessBuild: サンプルファイルをパッケージにコピー中...");
        CopySamplesToPackage();
    }
    
    static void CopySamplesToPackage() {
        if (!Directory.Exists(SAMPLES_SOURCE_PATH)) {
            Debug.LogWarning($"サンプルフォルダが見つかりません: {SAMPLES_SOURCE_PATH}");
            return;
        }
        
        // コピー先ディレクトリを作成
        if (Directory.Exists(SAMPLES_DEST_PATH)) {
            Directory.Delete(SAMPLES_DEST_PATH, true);
        }
        Directory.CreateDirectory(SAMPLES_DEST_PATH);
        
        // ディレクトリを再帰的にコピー
        CopyDirectory(SAMPLES_SOURCE_PATH, SAMPLES_DEST_PATH);
        
        Debug.Log($"サンプルファイルのコピーが完了しました: {SAMPLES_SOURCE_PATH} -> {SAMPLES_DEST_PATH}");
    }
    
    static void CopyDirectory(string sourcePath, string destPath) {
        Directory.CreateDirectory(destPath); 
        
        // ファイルをコピー
        foreach (string file in Directory.GetFiles(sourcePath)) {
            // .meta ファイルはスキップ
            if (file.EndsWith(".meta")) continue;
            
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destPath, fileName);
            File.Copy(file, destFile, true);
        }
        
        // サブディレクトリを再帰的にコピー
        foreach (string directory in Directory.GetDirectories(sourcePath)) {
            string dirName = Path.GetFileName(directory);
            string destDir = Path.Combine(destPath, dirName);
            CopyDirectory(directory, destDir);
        }
    }
}
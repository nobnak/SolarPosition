using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.Callbacks;
using System.IO;

[InitializeOnLoad]
public class PackageSampleEmbed {
    
    private static bool isCopying = false;
    
    static PackageSampleEmbed() {
        // スクリプトのコンパイルが開始されたときに呼ばれる
        CompilationPipeline.compilationStarted += OnCompilationStarted;
        
        // スクリプトのコンパイルが完了したときに呼ばれる
        CompilationPipeline.compilationFinished += OnCompilationFinished;
        
        // エディタの初期化時に呼ばれる
        EditorApplication.delayCall += OnEditorInitialized;
        
        // プロジェクトの再読み込み時に呼ばれる
        EditorApplication.projectChanged += OnProjectChanged;
    }
    
    private const string SAMPLES_SOURCE_PATH = "Assets/Samples";
    private const string SAMPLES_DEST_PATH = "Packages/jp.nobnak.solar/Samples~";

    static void OnCompilationStarted(object context) {
        CopySamplesToPackage();
    }
    
    static void OnCompilationFinished(object context) {
        CopySamplesToPackage();
    }
    
    static void OnEditorInitialized() {
        CopySamplesToPackage();
    }
    
    static void OnProjectChanged() {
        CopySamplesToPackage();
    }
    
    static void CopySamplesToPackage() {
        // 重複実行を防ぐ
        if (isCopying) {
            return;
        }
        
        if (!Directory.Exists(SAMPLES_SOURCE_PATH)) {
            Debug.LogWarning($"サンプルフォルダが見つかりません: {SAMPLES_SOURCE_PATH}");
            return;
        }
        
        // 実行中フラグを設定
        isCopying = true;
        
        try {
            // コピー先ディレクトリを作成
            if (Directory.Exists(SAMPLES_DEST_PATH)) {
                Directory.Delete(SAMPLES_DEST_PATH, true);
            }
            Directory.CreateDirectory(SAMPLES_DEST_PATH);
            
            // ディレクトリを再帰的にコピー
            CopyDirectory(SAMPLES_SOURCE_PATH, SAMPLES_DEST_PATH);
            
            // アセットデータベースを更新
            AssetDatabase.Refresh();
            
        } catch (System.Exception e) {
            Debug.LogError($"PackageSampleEmbed: サンプルファイルのコピー中にエラーが発生しました: {e.Message}");
        } finally {
            // 成功・失敗に関わらず、フラグをリセット
            isCopying = false;
        }
    }
    
    static void CopyDirectory(string sourcePath, string destPath) {
        Directory.CreateDirectory(destPath); 
        
        // ファイルをコピー（.metaファイルも含む）
        foreach (string file in Directory.GetFiles(sourcePath)) {
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
    
    /// <summary>
    /// 手動でサンプルファイルをコピーするメニューアイテム
    /// </summary>
    [MenuItem("Tools/Package Sample Embed/Force Copy Samples")]
    static void ForceCopySamples() {
        CopySamplesToPackage();
    }
}
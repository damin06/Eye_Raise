using Codice.CM.WorkspaceServer.Tree;
using System;
using System.Collections.Generic;
using UnityEditor;

public class BuildScript 
{
    private const string BUILD_ROOT = "builds/";
    private const string BUILD_SERVER_PATH = BUILD_ROOT + "Server/";
    private const string BUILD_CLIENT_PATH = BUILD_ROOT + "Client";

    [ExtensionOfNativeClass]
    [MenuItem("Builder/DedicatedServer")]
    public static void BuildDedicatedServer()
    {
        BuildPlayerOptions buildOption = new BuildPlayerOptions();
        buildOption.locationPathName = BUILD_SERVER_PATH + "server.exe";        
        buildOption.scenes = GetBuildSceneList();
        buildOption.target = BuildTarget.StandaloneWindows64;
        buildOption.subtarget = (int) StandaloneBuildSubtarget.Server;
        //빌드하자마자 실행되는 옵션. 만약 원하지 않으면 주석처리.
        buildOption.options = BuildOptions.AutoRunPlayer;
        BuildPipeline.BuildPlayer(buildOption);
    }


    [MenuItem("Builder/Client")]
    public static void BuildClient()
    {
        BuildClientNumber(1);
    }

    [MenuItem("Builder/ServerAndClient")]
    public static void BuildServerAndClient()
    {
        BuildDedicatedServer();
        BuildClientNumber(1);
    }

    [MenuItem("Builder/ServerAnd2Client")]
    public static void BuildServerAnd2Client()
    {
        BuildDedicatedServer();
        BuildClientNumber(1);
        BuildClientNumber(2);
    }

    private static void BuildClientNumber(int number)
    {
        BuildPlayerOptions buildOption = new BuildPlayerOptions();
        buildOption.locationPathName = $"{BUILD_CLIENT_PATH}{number}/client{number}.exe";
        buildOption.scenes = GetBuildSceneList();
        buildOption.target = BuildTarget.StandaloneWindows64;
        //빌드하자마자 실행되는 옵션. 만약 원하지 않으면 주석처리.
        buildOption.options = BuildOptions.AutoRunPlayer;

        BuildPipeline.BuildPlayer(buildOption);
    }


    private static string[] GetBuildSceneList()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        List<string> listScenePath = new List<string>();
        for (int i = 0; i < scenes.Length; ++i)
        {
            if (scenes[i].enabled)
            {
                listScenePath.Add(scenes[i].path);
            }
        }

        return listScenePath.ToArray();
    }
}

internal class ExtensionOfNativeClassAttribute : Attribute
{
}
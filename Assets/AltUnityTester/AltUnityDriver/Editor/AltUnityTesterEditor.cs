﻿
using System.Linq;

public class AltUnityTesterEditor : UnityEditor.EditorWindow
{
   

    private UnityEngine.UI.Button _android;
    UnityEngine.Object _obj;

    public static bool needsRepaiting = false;

    public static EditorConfiguration EditorConfiguration;
    public static AltUnityTesterEditor _window;

    public static NUnit.Framework.Internal.TestSuite _testSuite;

    // public TestRunDelegate CallRunDelegateCommandline = new TestRunDelegate();

    private static UnityEngine.Texture2D passIcon;
    private static UnityEngine.Texture2D failIcon;
    public static int selectedTest = -1;
    private static UnityEngine.Color defaultColor;
    private static UnityEngine.Color greenColor = new UnityEngine.Color(0.0f, 0.5f, 0.2f, 1f);
    private static UnityEngine.Color redColor = new UnityEngine.Color(0.7f, 0.15f, 0.15f, 1f);
    private static UnityEngine.Color selectedTestColor = new UnityEngine.Color(1f, 1f, 1f, 1f);



    UnityEngine.Vector2 _scrollPosition;
    private UnityEngine.Vector2 _scrollPositonTestResult;


    private bool _foldOutScenes = true;
    private bool _foldOutBuildSettings = true;
    private bool _foldOutIosSettings = true;
    private bool _foldOutAltUnityServerSettings = true;

    //TestResult after running a test
    public static bool isTestRunResultAvailable = false;
    public static int reportTestPassed;
    public static int reportTestFailed;
    public static double timeTestRan;

    public static System.Collections.Generic.List<MyDevices> devices=new System.Collections.Generic.List<MyDevices>();
    public static System.Collections.Generic.Dictionary<string, int> iosForwards =new System.Collections.Generic.Dictionary<string, int>();

    // Add menu item named "My Window" to the Window menu
    [UnityEditor.MenuItem("Window/AltUnityTester")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        _window = (AltUnityTesterEditor)GetWindow(typeof(AltUnityTesterEditor));
        _window.Show();

    }


    private void OnFocus()
    {
    

        if (EditorConfiguration == null)
        {
            InitEditorConfiguration();
        }

        if (failIcon == null)
        {
            var findIcon = UnityEditor.AssetDatabase.FindAssets("16px-indicator-fail");
            failIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(findIcon[0]));
        }
        if (passIcon == null)
        {
            var findIcon = UnityEditor.AssetDatabase.FindAssets("16px-indicator-pass");
            passIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(findIcon[0]));

        }


        GetListOfSceneFromEditor();
        AltUnityTestRunner.SetUpListTest();


    }

    private void GetListOfSceneFromEditor()
    {
        System.Collections.Generic.List<MyScenes> newSceneses =new System.Collections.Generic.List<MyScenes>();
        foreach (var scene in UnityEditor.EditorBuildSettings.scenes)
        {
            newSceneses.Add(new MyScenes(scene.enabled,scene.path,0));
        }

        EditorConfiguration.Scenes = newSceneses;
    }


    public static void InitEditorConfiguration()
    {
        if (UnityEditor.AssetDatabase.FindAssets("AltUnityTesterEditorSettings").Length == 0)
        {
            var altUnityEditorFolderPath=UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("AltUnityTesterEditor")[0]);
            altUnityEditorFolderPath=altUnityEditorFolderPath.Substring(0,altUnityEditorFolderPath.Length-24);
            UnityEngine.Debug.Log(altUnityEditorFolderPath);
            EditorConfiguration = UnityEngine.ScriptableObject.CreateInstance<EditorConfiguration>();
            UnityEditor.AssetDatabase.CreateAsset(EditorConfiguration, altUnityEditorFolderPath+"/AltUnityTesterEditorSettings.asset");
            UnityEditor.AssetDatabase.SaveAssets();

        }
        else
        {
            EditorConfiguration = UnityEditor.AssetDatabase.LoadAssetAtPath<EditorConfiguration>(
                UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("AltUnityTesterEditorSettings")[0]));
        }
        UnityEditor.EditorUtility.SetDirty(EditorConfiguration);

    }


    void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {

        if (needsRepaiting)
        {
            needsRepaiting = false;
            Repaint();
        }

        if (isTestRunResultAvailable)
        {
            isTestRunResultAvailable = !UnityEditor.EditorUtility.DisplayDialog("Test Report",
                  " Total tests:" + (reportTestFailed + reportTestPassed) + System.Environment.NewLine + " Tests passed:" +
                  reportTestPassed + System.Environment.NewLine + " Tests failed:" + reportTestFailed + System.Environment.NewLine +
                  " Duration:" + timeTestRan + " seconds", "Ok");
            reportTestFailed = 0;
            reportTestPassed = 0;
            timeTestRan = 0;
        }
        if (UnityEngine.Application.isPlaying && !EditorConfiguration.ranInEditor)
        {
            EditorConfiguration.ranInEditor = true;
        }

        if (!UnityEngine.Application.isPlaying && EditorConfiguration.ranInEditor)
        {
            AfterExitPlayMode();

        }

        DrawGUI();

    }

    private void DrawGUI()
    {
        var screenWidth = UnityEditor.EditorGUIUtility.currentViewWidth;
        //----------------------Left Panel------------
        UnityEditor.EditorGUILayout.BeginHorizontal();
        var leftSide = (screenWidth / 3) * 2;
        _scrollPosition = UnityEditor.EditorGUILayout.BeginScrollView(_scrollPosition, false, false, UnityEngine.GUILayout.MinWidth(leftSide));

        DisplayTestGui(EditorConfiguration.MyTests);

        UnityEditor.EditorGUILayout.Separator();

        DisplayBuildSettings();

        UnityEditor.EditorGUILayout.Separator();

        DisplayAltUnityServerSettings();

        UnityEditor.EditorGUILayout.Separator();

        DisplayPortForwarding();


        UnityEditor.EditorGUILayout.EndScrollView();

        //-------------------Right Panel--------------
        var rightSide = (screenWidth / 3);
        UnityEditor.EditorGUILayout.BeginVertical();

        UnityEditor.EditorGUILayout.LabelField("Platform", UnityEditor.EditorStyles.boldLabel);

        UnityEditor.EditorGUILayout.BeginHorizontal();
        EditorConfiguration.platform = (Platform)UnityEngine.GUILayout.SelectionGrid((int)EditorConfiguration.platform, System.Enum.GetNames(typeof(Platform)), System.Enum.GetNames(typeof(Platform)).Length, UnityEditor.EditorStyles.radioButton);

        UnityEditor.EditorGUILayout.EndHorizontal();
        UnityEditor.EditorGUILayout.Separator();
        UnityEditor.EditorGUILayout.Separator();
        UnityEditor.EditorGUILayout.Separator();

        UnityEditor.EditorGUILayout.LabelField("Tests", UnityEditor.EditorStyles.boldLabel);

        if (UnityEngine.GUILayout.Button("Run All Tests"))
        {
            if (EditorConfiguration.platform == Platform.Editor)
            {
                System.Threading.Thread testThread = new System.Threading.Thread(() => AltUnityTestRunner.RunTests(AltUnityTestRunner.TestRunMode.RunAllTest));
                testThread.Start();
            }
            else
            {

                AltUnityTestRunner.RunTests(AltUnityTestRunner.TestRunMode.RunAllTest);
            }
        }
        if (UnityEngine.GUILayout.Button("Run Selected Tests"))
        {
            if (EditorConfiguration.platform == Platform.Editor)
            {
                System.Threading.Thread testThread = new System.Threading.Thread(() => AltUnityTestRunner.RunTests(AltUnityTestRunner.TestRunMode.RunSelectedTest));
                testThread.Start();
            }
            else
            {

                AltUnityTestRunner.RunTests(AltUnityTestRunner.TestRunMode.RunSelectedTest);
            }
        }
        if (UnityEngine.GUILayout.Button("Run Failed Tests"))
        {
            if (EditorConfiguration.platform == Platform.Editor)
            {
                System.Threading.Thread testThread = new System.Threading.Thread(() => AltUnityTestRunner.RunTests(AltUnityTestRunner.TestRunMode.RunFailedTest));
                testThread.Start();
            }
            else
            {

                AltUnityTestRunner.RunTests(AltUnityTestRunner.TestRunMode.RunFailedTest);
            }
        }

        UnityEditor.EditorGUILayout.Separator();
        UnityEditor.EditorGUILayout.Separator();
        UnityEditor.EditorGUILayout.Separator();
        if (AltUnityBuilder.built)
        {
            var found = false;

            UnityEngine.SceneManagement.Scene scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(AltUnityBuilder.GetFirstSceneWhichWillBeBuilt());
            if (scene.path.Equals(AltUnityBuilder.GetFirstSceneWhichWillBeBuilt()))
            {
                if (scene.GetRootGameObjects()
                    .Any(gameObject => gameObject.name.Equals("AltUnityRunnerPrefab")))
                {
                    UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
                    var altunityRunner = scene.GetRootGameObjects()
                        .First(a => a.name.Equals("AltUnityRunnerPrefab"));
                    DestroyAltUnityRunner(altunityRunner);
                    found = true;
                }

                if (found == false)
                    AltUnityBuilder.built = false;
            }

        }

        UnityEditor.EditorGUILayout.LabelField("Build", UnityEditor.EditorStyles.boldLabel);
        if (EditorConfiguration.platform != Platform.Editor)
        {
            if (UnityEngine.GUILayout.Button("Build Only"))
            {
                if (EditorConfiguration.platform == Platform.Android)
                {
                    AltUnityBuilder.BuildAndroidFromUI(autoRun: false);
                }
#if UNITY_EDITOR_OSX
                else if (EditorConfiguration.platform == Platform.iOS) {
                    AltUnityBuilder.BuildiOSFromUI(autoRun: false);
                }
#endif
                else
                {
                    RunInEditor();
                }
            }
        }
        else
        {
            UnityEditor.EditorGUI.BeginDisabledGroup(true);
            UnityEngine.GUILayout.Button("Build Only");
            UnityEditor.EditorGUI.EndDisabledGroup();
        }

        UnityEditor.EditorGUILayout.Separator();
        UnityEditor.EditorGUILayout.Separator();
        UnityEditor.EditorGUILayout.Separator();

        UnityEditor.EditorGUILayout.LabelField("Run", UnityEditor.EditorStyles.boldLabel);
        if (UnityEngine.GUILayout.Button("Play in Editor"))
        {
            EditorConfiguration.platform = Platform.Editor;
            RunInEditor();
        }

        if (EditorConfiguration.platform != Platform.Editor)
        {
            if (UnityEngine.GUILayout.Button("Build & Run on Device"))
            {
                if (EditorConfiguration.platform == Platform.Android)
                {
                    AltUnityBuilder.BuildAndroidFromUI(autoRun: true);
                }
#if UNITY_EDITOR_OSX
                else if (EditorConfiguration.platform == Platform.iOS) {
                    AltUnityBuilder.BuildiOSFromUI(autoRun: true);
                }
#endif
            }
        }
        else
        {
            UnityEditor.EditorGUI.BeginDisabledGroup(true);
            UnityEngine.GUILayout.Button("Build & Run on Device");
            UnityEditor.EditorGUI.EndDisabledGroup();
        }

        UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.ExpandHeight(true));
        //Status test

        _scrollPositonTestResult = UnityEditor.EditorGUILayout.BeginScrollView(_scrollPositonTestResult, UnityEngine.GUI.skin.textArea);
        if (selectedTest != -1)
        {
            UnityEditor.EditorGUILayout.LabelField("Test Result for:  " + EditorConfiguration.MyTests[selectedTest].TestName, UnityEditor.EditorStyles.boldLabel);
            UnityEditor.EditorGUILayout.Separator();
            UnityEditor.EditorGUILayout.LabelField("Message:");
            if (EditorConfiguration.MyTests[selectedTest].TestResultMessage == null)
                UnityEngine.GUILayout.TextArea("No informartion about this test available.\nPlease rerun the test.",
                    UnityEngine.GUILayout.MaxHeight(75));
            else
            {
                string text = EditorConfiguration.MyTests[selectedTest].TestResultMessage;
                int lineContor = 1;
                int textLength = (int)rightSide / 7;
                if (text.Length > textLength)
                {
                    var splited = text.Split(' ');
                    text = "";
                    foreach (var word in splited)
                    {
                        text = text + " " + word;
                        if (text.Length > textLength * lineContor)
                        {
                            lineContor++;
                            text = text + "\n";
                        }
                    }
                }

                UnityEditor.EditorGUILayout.TextArea(text);
            }
        }
        else
        {
            UnityEditor.EditorGUILayout.LabelField("No test selected");
        }
        UnityEditor.EditorGUILayout.EndScrollView();
        UnityEditor.EditorGUILayout.EndVertical();
        UnityEditor.EditorGUILayout.EndHorizontal();
    }

    private void DisplayPortForwarding()
    {
        _foldOutScenes = UnityEditor.EditorGUILayout.Foldout(_foldOutScenes, "PortForwading");
        UnityEditor.EditorGUILayout.BeginHorizontal();
        UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
        UnityEditor.EditorGUILayout.BeginVertical();
        if (_foldOutScenes)
        {
            UnityEngine.GUILayout.BeginVertical(UnityEngine.GUI.skin.textField);
            UnityEngine.GUILayout.BeginHorizontal();
            UnityEngine.GUILayout.Label("DeviceId", UnityEditor.EditorStyles.boldLabel, UnityEngine.GUILayout.MinWidth(50));
            UnityEngine.GUILayout.Label("Local Port", UnityEditor.EditorStyles.boldLabel, UnityEngine.GUILayout.MinWidth(50), UnityEngine.GUILayout.MaxWidth(100));
            UnityEngine.GUILayout.Label("Remote Port", UnityEditor.EditorStyles.boldLabel, UnityEngine.GUILayout.MinWidth(50), UnityEngine.GUILayout.MaxWidth(100));
            if (UnityEngine.GUILayout.Button("Refresh", UnityEngine.GUILayout.MinWidth(50), UnityEngine.GUILayout.MaxWidth(100)))
            {
                RefreshDeviceList();
            }
            UnityEngine.GUILayout.EndHorizontal();

            if (devices.Count != 0)
            {
                foreach (var device in devices)
                {
                    if (device.Active)
                    {
                        var styleActive = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.textField);
                        styleActive.normal.background = MakeTexture(20, 20, greenColor);
                        
                        UnityEngine.GUILayout.BeginHorizontal(styleActive);
                        UnityEngine.GUILayout.Label(device.DeviceId, UnityEngine.GUILayout.MinWidth(50));
                        UnityEngine.GUILayout.Label(device.LocalPort.ToString(), UnityEngine.GUILayout.MinWidth(50), UnityEngine.GUILayout.MaxWidth(100));
                        UnityEngine.GUILayout.Label(device.RemotePort.ToString(), UnityEngine.GUILayout.MinWidth(50), UnityEngine.GUILayout.MaxWidth(100));
                        if(UnityEngine.GUILayout.Button("Stop", UnityEngine.GUILayout.MinWidth(50), UnityEngine.GUILayout.MaxWidth(100)))
                        {
                            if (device.Platform == Platform.Android)
                            {
                                AltUnityPortHandler.RemoveForwardAndroid(device.LocalPort,device.DeviceId);
                            }
#if UNITY_EDITOR_OSX
                            else
                            {
                                int id;
                                if(iosForwards.TryGetValue(device.DeviceId,out id)){
                                    AltUnityPortHandler.KillIProxy(id);
                                    iosForwards.Remove(device.DeviceId);
                                }

                            }
#endif

                            device.Active = false;
                            RefreshDeviceList();

                        }
                    }
                    else
                    {
                        var style = UnityEngine.GUI.skin.textField;
                        UnityEngine.GUILayout.BeginHorizontal(style);
                        UnityEngine.GUILayout.Label(device.DeviceId, UnityEngine.GUILayout.MinWidth(50));
                        device.LocalPort=UnityEditor.EditorGUILayout.IntField(device.LocalPort, UnityEngine.GUILayout.MinWidth(50), UnityEngine.GUILayout.MaxWidth(100));
                        device.RemotePort=UnityEditor.EditorGUILayout.IntField(device.RemotePort, UnityEngine.GUILayout.MinWidth(50), UnityEngine.GUILayout.MaxWidth(100));
                        if(UnityEngine.GUILayout.Button("Start", UnityEngine.GUILayout.MinWidth(50), UnityEngine.GUILayout.MaxWidth(100)))
                        {
                            if (device.Platform == Platform.Android)
                            {
                                var response=AltUnityPortHandler.ForwardAndroid(device.DeviceId,device.LocalPort,device.RemotePort);
                                if(!response.Equals("Ok")){
                                    UnityEngine.Debug.LogError(response);
                                }
                            }
#if UNITY_EDITOR_OSX
                            else
                            {
                                var response=AltUnityPortHandler.ForwardIos(device.DeviceId,device.LocalPort,device.RemotePort);
                                if(response.StartsWith("Ok")){
                                    var processID=int.Parse(response.Split(' ')[1]);
                                    iosForwards.Add(device.DeviceId,processID);
                                    device.Active=true;
                                }else{
                                    UnityEngine.Debug.LogError(response);
                                }
                                
                            }

#endif
                            RefreshDeviceList();
                        }

                    }                  

                    UnityEngine.GUILayout.EndHorizontal();

                }

                
            }
            else
            {
                UnityEditor.EditorGUILayout.TextArea("No devices connected. Click \"refresh\" button to search for devices");
            }
            UnityEngine.GUILayout.EndVertical();
        }

        UnityEditor.EditorGUILayout.EndVertical();
        UnityEditor.EditorGUILayout.EndHorizontal();
    }

    private void RefreshDeviceList()
    {
        System.Collections.Generic.List<MyDevices> adbDevices = AltUnityPortHandler.GetDevicesAndroid();
        System.Collections.Generic.List<MyDevices> androidForwardedDevices = AltUnityPortHandler.GetForwardedDevicesAndroid();
        foreach(var adbDevice in adbDevices)
        {
            var deviceForwarded = androidForwardedDevices.FirstOrDefault(device => device.DeviceId.Equals(adbDevice.DeviceId));
            if (deviceForwarded != null)
            {
                adbDevice.LocalPort = deviceForwarded.LocalPort;
                adbDevice.RemotePort = deviceForwarded.RemotePort;
                adbDevice.Active = deviceForwarded.Active;
            }
        }
        foreach(var device in devices)
        {
            var existingDevice = adbDevices.FirstOrDefault(d => d.DeviceId.Equals(device.DeviceId));
            if (existingDevice != null && device.Active==false && existingDevice.Active==false)
            {
                existingDevice.LocalPort = device.LocalPort;
                existingDevice.RemotePort = device.RemotePort;
            }
        }
 #if UNITY_EDITOR_OSX
        System.Collections.Generic.List<MyDevices> iOSDEvices=AltUnityPortHandler.GetConnectediOSDevices();
        foreach(var iOSDEvice in iOSDEvices){
            var iOSForwardedDevice=devices.FirstOrDefault(a=>a.DeviceId.Equals(iOSDEvice.DeviceId));
            if(iOSForwardedDevice!=null){
                iOSDEvice.LocalPort=iOSForwardedDevice.LocalPort;
                iOSDEvice.RemotePort=iOSForwardedDevice.RemotePort;
                iOSDEvice.Active=iOSForwardedDevice.Active;
            }
        }
#endif
        

        devices = adbDevices;
#if UNITY_EDITOR_OSX
        devices.AddRange(iOSDEvices);
#endif
    }

    private void DisplayAltUnityServerSettings()
    {
        _foldOutAltUnityServerSettings = UnityEditor.EditorGUILayout.Foldout(_foldOutAltUnityServerSettings, "AltUnityServer Settings");
        if (_foldOutAltUnityServerSettings)
        {
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
            EditorConfiguration.requestSeparator = UnityEditor.EditorGUILayout.TextField("Request separator", EditorConfiguration.requestSeparator);
            UnityEditor.EditorGUILayout.EndHorizontal();

            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
            EditorConfiguration.requestEnding = UnityEditor.EditorGUILayout.TextField("Request ending", EditorConfiguration.requestEnding);
            UnityEditor.EditorGUILayout.EndHorizontal();


            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
            EditorConfiguration.serverPort = UnityEditor.EditorGUILayout.IntField("Server port", EditorConfiguration.serverPort);
            UnityEditor.EditorGUILayout.EndHorizontal();
        }
    }

    private void AfterExitPlayMode() {
        RemoveAltUnityRunnerPrefab();
        AltUnityBuilder.RemoveAltUnityTesterFromScriptingDefineSymbols(UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup);
        EditorConfiguration.ranInEditor = false;
    }

    private static void RemoveAltUnityRunnerPrefab() {
        var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        var altUnityRunners = activeScene.GetRootGameObjects()
            .Where(gameObject => gameObject.name.Equals("AltUnityRunnerPrefab"));
        foreach (var altUnityRunner in altUnityRunners) {
            DestroyImmediate(altUnityRunner);

        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
    }


    private void RunInEditor()
    {
        AltUnityBuilder.InsertAltUnityInTheFirstScene();
        AltUnityBuilder.AddAltUnityTesterInScritpingDefineSymbolsGroup(UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup);

        UnityEditor.EditorApplication.isPlaying = true;

    }

    private void DisplayBuildSettings()
    {
        _foldOutBuildSettings = UnityEditor.EditorGUILayout.Foldout(_foldOutBuildSettings, "Build Settings");
        if (_foldOutBuildSettings)
        {
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
            EditorConfiguration.OutputPathName = UnityEditor.EditorGUILayout.TextField("Output path", EditorConfiguration.OutputPathName);
            UnityEditor.EditorGUILayout.EndHorizontal();
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
            var androidBundleIdentifier = UnityEditor.EditorGUILayout.TextField("Android Bundle Identifier",
                UnityEditor.PlayerSettings.GetApplicationIdentifier(UnityEditor.BuildTargetGroup.Android));
            if (androidBundleIdentifier != UnityEditor.PlayerSettings.GetApplicationIdentifier(UnityEditor.BuildTargetGroup.Android))
            {
                UnityEditor.PlayerSettings.SetApplicationIdentifier(UnityEditor.BuildTargetGroup.Android, androidBundleIdentifier);
            }
            UnityEditor.EditorGUILayout.EndHorizontal();
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
            var iOSBundleIdentifier = UnityEditor.EditorGUILayout.TextField("iOS Bundle Identifier",
                UnityEditor.PlayerSettings.GetApplicationIdentifier(UnityEditor.BuildTargetGroup.iOS));
            if (iOSBundleIdentifier != UnityEditor.PlayerSettings.GetApplicationIdentifier(UnityEditor.BuildTargetGroup.iOS))
            {
                UnityEditor.PlayerSettings.SetApplicationIdentifier(UnityEditor.BuildTargetGroup.iOS, iOSBundleIdentifier);
            }
            //            BundleIdentifier= UnityEditor.EditorGUILayout.TextField("Android Bundle Identifier", BundleIdentifier);
            UnityEditor.EditorGUILayout.EndHorizontal();
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
            UnityEditor.PlayerSettings.companyName = UnityEditor.EditorGUILayout.TextField("Company Name", UnityEditor.PlayerSettings.companyName);
            UnityEditor.EditorGUILayout.EndHorizontal();
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
            UnityEditor.PlayerSettings.productName = UnityEditor.EditorGUILayout.TextField("Product Name", UnityEditor.PlayerSettings.productName);
            UnityEditor.EditorGUILayout.EndHorizontal();
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
            UnityEditor.EditorGUILayout.LabelField("Append \"Test\" to product name for AltUnityTester builds");
           
            EditorConfiguration.appendToName =
                UnityEditor.EditorGUILayout.Toggle(EditorConfiguration.appendToName, UnityEngine.GUILayout.MaxWidth(30));
            UnityEditor.EditorGUILayout.EndHorizontal();
            
#if UNITY_EDITOR_OSX
            _foldOutIosSettings = UnityEditor.EditorGUILayout.Foldout(_foldOutIosSettings, "IOS Settings");
            if (_foldOutIosSettings)
            {
                UnityEditor.EditorGUILayout.BeginHorizontal();
                UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
                UnityEditor.PlayerSettings.iOS.appleDeveloperTeamID = UnityEditor.EditorGUILayout.TextField("Signing Team Id: ", UnityEditor.PlayerSettings.iOS.appleDeveloperTeamID);
                UnityEditor.EditorGUILayout.EndHorizontal();
                UnityEditor.EditorGUILayout.BeginHorizontal();
                UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
                UnityEditor.PlayerSettings.iOS.appleEnableAutomaticSigning = UnityEditor.EditorGUILayout.Toggle("Automatically Sign: ", UnityEditor.PlayerSettings.iOS.appleEnableAutomaticSigning );
                UnityEditor.EditorGUILayout.EndHorizontal();

                UnityEditor.EditorGUILayout.BeginHorizontal();
                UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
                EditorConfiguration.AdbPath = UnityEditor.EditorGUILayout.TextField("Adb Path: ", EditorConfiguration.AdbPath);
                UnityEditor.EditorGUILayout.EndHorizontal();
                UnityEditor.EditorGUILayout.BeginHorizontal();
                UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
                EditorConfiguration.IProxyPath = UnityEditor.EditorGUILayout.TextField("Iproxy Path: ", EditorConfiguration.IProxyPath);
                UnityEditor.EditorGUILayout.EndHorizontal();
                UnityEditor.EditorGUILayout.BeginHorizontal();
                UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
                EditorConfiguration.XcrunPath = UnityEditor.EditorGUILayout.TextField("Xcrun Path: ", EditorConfiguration.XcrunPath);
                UnityEditor.EditorGUILayout.EndHorizontal();
            }
#endif


            DisplayScenes();
        }
    }

    private void DisplayScenes()
    {
        _foldOutScenes = UnityEditor.EditorGUILayout.Foldout(_foldOutScenes, "SceneManager");
        UnityEditor.EditorGUILayout.BeginHorizontal();
        UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
        UnityEditor.EditorGUILayout.BeginVertical();
        if (_foldOutScenes)
        {
            if (EditorConfiguration.Scenes.Count != 0)
            {
                UnityEditor.EditorGUILayout.BeginHorizontal();
                UnityEditor.EditorGUILayout.LabelField("Display scene full path: ");
                EditorConfiguration.scenePathDisplayed=UnityEditor.EditorGUILayout.Toggle(EditorConfiguration.scenePathDisplayed);
                UnityEditor.EditorGUILayout.EndHorizontal();
                UnityEngine.GUILayout.BeginVertical(UnityEngine.GUI.skin.textField);
                MyScenes sceneToBeRemoved = null;
                int counter = 0;
                foreach (var scene in EditorConfiguration.Scenes)
                {
                    UnityEngine.GUILayout.BeginHorizontal(UnityEngine.GUI.skin.textArea);
                   
                    var valToggle = UnityEditor.EditorGUILayout.Toggle(scene.ToBeBuilt, UnityEngine.GUILayout.MaxWidth(10));
                    if (valToggle != scene.ToBeBuilt)
                    {
                        scene.ToBeBuilt = valToggle;
                        UnityEditor.EditorBuildSettings.scenes = PathFromTheSceneInCurrentList();
                    }
                    var sceneName = scene.Path;
                    if (!EditorConfiguration.scenePathDisplayed)
                    {
                        var splitedPath = sceneName.Split('/');
                        sceneName = splitedPath[splitedPath.Length - 1];
                    }
                    UnityEditor.EditorGUILayout.LabelField(sceneName);
                    string value;
                    if (scene.ToBeBuilt)
                    {
                        scene.BuildScene = counter;
                        counter++;
                        value = scene.BuildScene.ToString();
                    }
                    else
                    {
                        value = "";
                    }

                    UnityEditor.EditorGUILayout.LabelField(value, UnityEngine.GUILayout.MaxWidth(30));


                    if (EditorConfiguration.Scenes.IndexOf(scene) != 0 && EditorConfiguration.Scenes.Count > 1)
                    {

                        if (UnityEngine.GUILayout.Button("^", UnityEngine.GUILayout.MaxWidth(30)))
                        {
                            SceneMove(scene, true);
                            UnityEditor.EditorBuildSettings.scenes = PathFromTheSceneInCurrentList();
                        }
                    }

                    if (EditorConfiguration.Scenes.IndexOf(scene) != EditorConfiguration.Scenes.Count - 1 && EditorConfiguration.Scenes.Count > 1)
                        if (UnityEngine.GUILayout.Button("v", UnityEngine.GUILayout.MaxWidth(30)))
                        {
                            SceneMove(scene, false);
                            UnityEditor.EditorBuildSettings.scenes = PathFromTheSceneInCurrentList();
                        }


                    if (UnityEngine.GUILayout.Button("X", UnityEngine.GUILayout.MaxWidth(30)))
                    {
                        sceneToBeRemoved = scene;
                    }

                    UnityEngine.GUILayout.EndHorizontal();

                }


                if (sceneToBeRemoved != null)
                {
                    RemoveScene(sceneToBeRemoved);
                }

                UnityEngine.GUILayout.EndVertical();
            }

            UnityEngine.GUILayout.BeginVertical();
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField("Add scene: ", UnityEngine.GUILayout.MaxWidth(80));
            _obj = UnityEditor.EditorGUILayout.ObjectField(_obj, typeof(UnityEditor.SceneAsset), true);

            if (_obj != null)
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(_obj);
                if (EditorConfiguration.Scenes.All(n => n.Path != path))
                {
                    EditorConfiguration.Scenes.Add(new MyScenes(false, path, 0));
                    _obj = new UnityEngine.Object();
                }

                _obj = null;
            }
            UnityEditor.EditorGUILayout.EndHorizontal();
            UnityEditor.EditorGUILayout.BeginHorizontal();
            if (UnityEngine.GUILayout.Button("Add all scenes", UnityEditor.EditorStyles.miniButtonLeft))
            {
                AddAllScenes();
            }

            if (UnityEngine.GUILayout.Button("Select all scenes", UnityEditor.EditorStyles.miniButtonMid))
            {
                SelectAllScenes();
            }
            if (UnityEngine.GUILayout.Button("Deselect all scenes", UnityEditor.EditorStyles.miniButtonMid))
            {
                DeselectAllScenes();
            }
            if (UnityEngine.GUILayout.Button("Remove not selected scenes", UnityEditor.EditorStyles.miniButtonMid))
            {
                RemoveNotSelectedScenes();
            }
            if (UnityEngine.GUILayout.Button("Remove all scenes", UnityEditor.EditorStyles.miniButtonRight))
            {
                EditorConfiguration.Scenes = new System.Collections.Generic.List<MyScenes>();
                UnityEditor.EditorBuildSettings.scenes = PathFromTheSceneInCurrentList();
            }
            UnityEditor.EditorGUILayout.EndHorizontal();
            UnityEditor.EditorGUILayout.EndVertical();
        }

        UnityEditor.EditorGUILayout.EndVertical();
        UnityEditor.EditorGUILayout.EndHorizontal();

    }

    private void RemoveNotSelectedScenes()
    {
        System.Collections.Generic.List<MyScenes> copyMySceneses = new System.Collections.Generic.List<MyScenes>();
        foreach (var scene in EditorConfiguration.Scenes)
        {
            if (scene.ToBeBuilt)
            {
                copyMySceneses.Add(scene);
            }
        }

        EditorConfiguration.Scenes = copyMySceneses;
        UnityEditor.EditorBuildSettings.scenes = PathFromTheSceneInCurrentList();
    }

    private void DeselectAllScenes()
    {
        foreach (var scene in EditorConfiguration.Scenes)
        {
            scene.ToBeBuilt = false;
        }
        UnityEditor.EditorBuildSettings.scenes = PathFromTheSceneInCurrentList();

    }

    public static void SelectAllScenes()
    {
        foreach (var scene in EditorConfiguration.Scenes)
        {
            scene.ToBeBuilt = true;
        }
        UnityEditor.EditorBuildSettings.scenes = PathFromTheSceneInCurrentList();


    }


    private void DisplayTestGui(System.Collections.Generic.List<MyTest> tests)
    {
        UnityEditor.EditorGUILayout.LabelField("Test", UnityEditor.EditorStyles.boldLabel);
        UnityEditor.EditorGUILayout.BeginHorizontal();
        UnityEditor.EditorGUILayout.LabelField("Display test class name: ");
        EditorConfiguration.testPathDisplayed = UnityEditor.EditorGUILayout.Toggle(EditorConfiguration.testPathDisplayed);
        UnityEditor.EditorGUILayout.EndHorizontal();
        UnityEditor.EditorGUILayout.BeginVertical(UnityEngine.GUI.skin.textArea);

        int foldOutCounter = 0;

        foreach (var test in tests)
        {
            if (foldOutCounter > 0)
            {
                foldOutCounter--;
                continue;
            }

            if (tests.IndexOf(test) == selectedTest)
            {
                UnityEngine.GUIStyle gsAlterQuest = new UnityEngine.GUIStyle();
                gsAlterQuest.normal.background = MakeTexture(20,20, selectedTestColor);
                UnityEditor.EditorGUILayout.BeginHorizontal(gsAlterQuest);

            }
            else
            {
                UnityEditor.EditorGUILayout.BeginHorizontal();
            }

            if (test.Type == typeof(NUnit.Framework.Internal.TestFixture))
            {
                UnityEditor.EditorGUILayout.LabelField("    ", UnityEngine.GUILayout.Width(30));
            }
            else if (test.Type == typeof(NUnit.Framework.Internal.TestMethod))
            {
                UnityEditor.EditorGUILayout.LabelField("    ", UnityEngine.GUILayout.Width(60));
            }

            var valueChanged = UnityEditor.EditorGUILayout.Toggle(test.Selected, UnityEngine.GUILayout.Width(10));
            if (valueChanged != test.Selected)
            {
                test.Selected = valueChanged;
                ChangeSelectionChildsAndParent(test);
            }

            var testName = test.TestName;
            if (!EditorConfiguration.testPathDisplayed)
            {
                if (test.ParentName == "")
                {
                    var splitedPath = testName.Split('/');
                    testName = splitedPath[splitedPath.Length - 1];
                }
                else
                {
                    var splitedPath = testName.Split('.');
                    testName = splitedPath[splitedPath.Length - 1];
                }
                
                
            }
            if (test.Status == 0)
            {
                var style = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.label) { alignment = UnityEngine.TextAnchor.MiddleLeft };
                UnityEditor.EditorGUILayout.LabelField(testName, style);
            }
            else
            {
                UnityEngine.Color color = redColor;
                UnityEngine.Texture2D icon = failIcon;
                if (test.Status == 1)
                {
                    color = greenColor;
                    icon = passIcon;
                }
                UnityEngine.GUILayout.Label(icon, UnityEngine.GUILayout.Width(30));
                UnityEngine.GUIStyle guiStyle = new UnityEngine.GUIStyle { normal = {textColor = color}};

                UnityEditor.EditorGUILayout.LabelField(testName, guiStyle,UnityEngine.GUILayout.ExpandWidth(true));
            }

            if (test.Type != typeof(NUnit.Framework.Internal.TestMethod))
            {
                test.FoldOut = UnityEditor.EditorGUILayout.Foldout(test.FoldOut,"");
                if (!test.FoldOut)
                {
                    if (test.Type == typeof(NUnit.Framework.Internal.TestAssembly))
                    {
                        foldOutCounter = tests.Count - 1;
                    }
                    else
                    {
                        foldOutCounter = test.TestCaseCount;
                    }
                }
            }

            if (!test.IsSuite) { 
                if (UnityEngine.GUILayout.Button("Info", UnityEngine.GUILayout.Width(50)))
                {
                    selectedTest = tests.IndexOf(test);
                }
            }
            else
            {
                UnityEngine.GUILayout.Label("", new UnityEngine.GUIStyle { stretchWidth = true });
            }
            UnityEditor.EditorGUILayout.EndHorizontal();

        }
        UnityEditor.EditorGUILayout.EndVertical();
    }
    
    private void ChangeSelectionChildsAndParent(MyTest test)
    {
        if (test.Selected)
        {
            if (test.Type == typeof(NUnit.Framework.Internal.TestAssembly))
            {
                foreach (var test2 in EditorConfiguration.MyTests)
                {
                    test2.Selected = true;
                }
            }
            else
            {
                if (test.IsSuite)
                {
                    var index = EditorConfiguration.MyTests.IndexOf(test);
                    for (int i = index + 1; i <= index + test.TestCaseCount; i++)
                    {
                        EditorConfiguration.MyTests[i].Selected = true;
                    }
                }
            }
        }
        else
        {
            if (test.Type == typeof(NUnit.Framework.Internal.TestAssembly))
            {
                foreach (var test2 in EditorConfiguration.MyTests)
                {
                    test2.Selected = false;
                }
            }
            else
            {
                var dummy = test;
                if (test.Type == typeof(NUnit.Framework.Internal.TestFixture))
                {
                    var index = EditorConfiguration.MyTests.IndexOf(test);
                    for (int i = index + 1; i <= index + test.TestCaseCount; i++)
                    {
                        EditorConfiguration.MyTests[i].Selected = false;
                    }
                }
                while (dummy.ParentName != null)
                {
                    dummy = EditorConfiguration.MyTests.FirstOrDefault(a => a.TestName.Equals(dummy.ParentName));
                    if (dummy != null)
                        dummy.Selected = false;
                    else
                        return;
                }
            }
        }

    }

    private static void SceneMove(MyScenes scene, bool up)
    {
        int index = EditorConfiguration.Scenes.IndexOf(scene);
        if (up)
        {
            Swap(index, index - 1);
        }
        else
        {
            Swap(index, index + 1);
        }
    }


    public static void Swap(int index1, int index2)
    {
        MyScenes backUp = EditorConfiguration.Scenes[index1];
        EditorConfiguration.Scenes[index1] = EditorConfiguration.Scenes[index2];
        EditorConfiguration.Scenes[index2] = backUp;
    }


    public static void AddAllScenes()
    {
        var scenesToBeAddedGuid = UnityEditor.AssetDatabase.FindAssets("t:SceneAsset");
        EditorConfiguration.Scenes = new System.Collections.Generic.List<MyScenes>();
        foreach (var sceneGuid in scenesToBeAddedGuid)
        {
            var scenePath = UnityEditor.AssetDatabase.GUIDToAssetPath(sceneGuid);
            EditorConfiguration.Scenes.Add(new MyScenes(false, scenePath, 0));

        }

        UnityEditor.EditorBuildSettings.scenes = PathFromTheSceneInCurrentList();

    }

    private static UnityEditor.EditorBuildSettingsScene[] PathFromTheSceneInCurrentList()
    {
        System.Collections.Generic.List<UnityEditor.EditorBuildSettingsScene> listofPath = new System.Collections.Generic.List<UnityEditor.EditorBuildSettingsScene>();
        foreach (var scene in EditorConfiguration.Scenes)
        {
            listofPath.Add(new UnityEditor.EditorBuildSettingsScene(scene.Path, scene.ToBeBuilt));
        }

        return listofPath.ToArray();
    }

    private void RemoveScene(MyScenes scene)
    {

        EditorConfiguration.Scenes.Remove(scene);
        UnityEditor.EditorBuildSettings.scenes = PathFromTheSceneInCurrentList();

    }

    private UnityEngine.Texture2D MakeTexture(int width, int height, UnityEngine.Color col)
    {
        UnityEngine.Color[] pix = new UnityEngine.Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        UnityEngine.Texture2D result = new UnityEngine.Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }



    [UnityEditor.MenuItem("Assets/Create/AltUnityTest", false, 80)]
    public static void CreateAltUnityTest()
    {

        var templatePath = UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("DefaultTestExample")[0]);

        string folderPath = GetPathForSelectedItem();
        System.Reflection.MethodInfo method = typeof(UnityEditor.ProjectWindowUtil).GetMethod("CreateScriptAsset", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        if (method == null)
            return;
        string newFilePath = System.IO.Path.Combine(folderPath, "NewAltUnityTest.cs");
        method.Invoke((object)null, new object[2]
        {
            (object) templatePath,
            (object) newFilePath
        });

    }

    [UnityEditor.MenuItem("Assets/Create/AltUnityTest", true, 80)]
    public static bool CanCreateAltUnityTest()
    {
        return (GetPathForSelectedItem() + "/").Contains("/Editor/");
    }

    [UnityEditor.MenuItem("Window/CreateAltUnityTesterPackage")]
    public static void CreateAltUnityTesterPackage() {
        UnityEngine.Debug.Log("AltUnityTester - Unity Package creation started...");
        string packageName="AltUnityTester.unitypackage";
        string assetPathNames = "Assets/AltUnityTester";
        UnityEditor.AssetDatabase.ExportPackage(assetPathNames, packageName, UnityEditor.ExportPackageOptions.Recurse);
        UnityEngine.Debug.Log("AltUnityTester - Unity Package done.");
    }

    private static string GetPathForSelectedItem()
    {
        string path = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject);
        if (System.IO.Path.GetExtension(path) != "") //checks if current item is a folder or a file 
        {
            path = path.Replace(System.IO.Path.GetFileName(UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject)), "");
        }
        return path;
    }

    private static void DestroyAltUnityRunner(UnityEngine.Object altUnityRunner)
    {

        DestroyImmediate(altUnityRunner);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(AltUnityBuilder.PreviousScenePath);
    }

}

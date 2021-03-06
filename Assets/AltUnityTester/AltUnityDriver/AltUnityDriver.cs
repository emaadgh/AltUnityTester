﻿
public enum PLayerPrefKeyType { Int = 1, String, Float }

public class AltUnityDriver
{
    public System.Net.Sockets.TcpClient Socket;
    private static string tcp_ip = "127.0.0.1";
    private static int tcp_port = 13000;
    private static int BUFFER_SIZE = 1024;
    public static string requestSeparatorString;
    public static string requestEndingString;
    public AltUnityDriver(string tcp_ip = "127.0.0.1", int tcp_port = 13000, string requestSeparator = ";", string requestEnding = "&")
    {

        Socket = new System.Net.Sockets.TcpClient();
        Socket.Connect(tcp_ip, tcp_port);
        AltUnityObject.altUnityDriver = this;
        requestSeparatorString = requestSeparator;
        requestEndingString = requestEnding;

    }

    public void Stop()
    {
        Socket.Client.Send(toBytes(CreateCommand("closeConnection")));
        System.Threading.Thread.Sleep(1000);
        Socket.Close();




    }
    public string CreateCommand(params string[] arguments)
    {
        string command = "";
        foreach(var argument in arguments)
        {
            command += argument + requestSeparatorString;
        }
        command += requestEndingString;
        return command;
    }
    public string Recvall()
    {

        string data = "";
        string previousPart = "";
        while (true)
        {
            var bytesReceived = new byte[BUFFER_SIZE];
            Socket.Client.Receive(bytesReceived);
            string part = fromBytes(bytesReceived);
            string partToSeeAltEnd = previousPart + part;
            data += part;
            if (partToSeeAltEnd.Contains("::altend"))
                break;
            previousPart = part;
        }

        try
        {
            string[] start = new string[] { "altstart::" };
            string[] end = new string[] { "::altend" };
            data = data.Split(start, System.StringSplitOptions.None)[1].Split(end, System.StringSplitOptions.None)[0];
        }
        catch (System.Exception)
        {
            UnityEngine.Debug.Log("Data received from socket doesn't have correct start and end control strings");
        }

        return data;
    }

    private byte[] toBytes(string text)
    {
        return System.Text.Encoding.ASCII.GetBytes(text);
    }

    private string fromBytes(byte[] text)
    {
        return System.Text.Encoding.ASCII.GetString(text);
    }

    public void LoadScene(string scene)
    {
        Socket.Client.Send(toBytes(CreateCommand("loadScene",scene)));
        var data = Recvall();
        if (data.Equals("Ok"))
            return;
        HandleErrors(data);
    }

    public void SetTimeScale(float timeScale)
    {
        Socket.Client.Send(toBytes(CreateCommand("setTimeScale", Newtonsoft.Json.JsonConvert.SerializeObject(timeScale))));
        var data = Recvall();
        if (data.Equals("Ok"))
            return;
        HandleErrors(data);
    }

    public float GetTimeScale()
    {
        Socket.Client.Send(toBytes(CreateCommand("getTimeScale")));
        var data = Recvall();
        if (!data.Contains("error"))
            return Newtonsoft.Json.JsonConvert.DeserializeObject<float>(data);
        HandleErrors(data);
        return -1f;
    }

    public string CallStaticMethods(string typeName, string methodName,
        string parameters, string typeOfParameters = "", string assemblyName = "")
    {
        string actionInfo =
            Newtonsoft.Json.JsonConvert.SerializeObject(new AltUnityObjectAction(typeName, methodName, parameters, typeOfParameters, assemblyName));
        Socket.Client.Send(toBytes(CreateCommand("callComponentMethodForObject","",actionInfo)));
        var data = Recvall();
        if (!data.Contains("error:")) return data;
        HandleErrors(data);
        return null;
    }
    public void DeletePlayerPref()
    {
        Socket.Client.Send(toBytes(CreateCommand("deletePlayerPref")));
        var data = Recvall();
        if (data.Equals("Ok"))
            return;
        HandleErrors(data);

    }
    public void DeleteKeyPlayerPref(string keyName)
    {
        Socket.Client.Send(toBytes(CreateCommand("deleteKeyPlayerPref" , keyName )));
        var data = Recvall();
        if (data.Equals("Ok"))
            return;
        HandleErrors(data);

    }
    public void SetKeyPlayerPref(string keyName, int valueName)
    {
        Socket.Client.Send(toBytes(CreateCommand("setKeyPlayerPref", keyName , valueName.ToString() , PLayerPrefKeyType.Int.ToString() )));
        var data = Recvall();
        if (data.Equals("Ok"))
            return;

        HandleErrors(data);


    }
    public void SetKeyPlayerPref(string keyName, float valueName)
    {
        Socket.Client.Send(toBytes(CreateCommand("setKeyPlayerPref", keyName , valueName.ToString(),PLayerPrefKeyType.Float.ToString())));
        var data = Recvall();
        if (data.Equals("Ok"))
            return;
        HandleErrors(data);

    }
    public void SetKeyPlayerPref(string keyName, string valueName)
    {
        Socket.Client.Send(toBytes(CreateCommand("setKeyPlayerPref", keyName , valueName.ToString(), PLayerPrefKeyType.String.ToString())));
        var data = Recvall();
        if (data.Equals("Ok"))
            return;
        HandleErrors(data);

    }
    public int GetIntKeyPlayerPref(string keyname)
    {
        Socket.Client.Send(toBytes(CreateCommand("getKeyPlayerPref", keyname, PLayerPrefKeyType.Int.ToString())));
        var data = Recvall();
        if (!data.Contains("error:")) return System.Int32.Parse(data);
        HandleErrors(data);
        return 0;

    }
    public float GetFloatKeyPlayerPref(string keyname)
    {
        Socket.Client.Send(toBytes(CreateCommand("getKeyPlayerPref" , keyname , PLayerPrefKeyType.Float.ToString())));
        var data = Recvall();
        if (!data.Contains("error:")) return System.Single.Parse(data);
        HandleErrors(data);
        return 0;

    }
    public string GetStringKeyPlayerPref(string keyname)
    {
        Socket.Client.Send(toBytes(CreateCommand("getKeyPlayerPref" ,keyname , PLayerPrefKeyType.String.ToString())));
        var data = Recvall();
        if (!data.Contains("error:")) return data;
        HandleErrors(data);
        return null;

    }

    public string GetCurrentScene()
    {

        Socket.Client.Send(toBytes(CreateCommand("getCurrentScene")));
        string data = Recvall();
        if (!data.Contains("error:")) return Newtonsoft.Json.JsonConvert.DeserializeObject<AltUnityObject>(data).name;
        HandleErrors(data);
        return null;
    }


    public void Swipe(UnityEngine.Vector2 start, UnityEngine.Vector2 end, float duration)
    {
        string vectorStartJson = Newtonsoft.Json.JsonConvert.SerializeObject(start, Newtonsoft.Json.Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings
        {
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
        });
        string vectorEndJson = Newtonsoft.Json.JsonConvert.SerializeObject(end, Newtonsoft.Json.Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings
        {
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
        });
        Socket.Client.Send(toBytes(CreateCommand("movingTouch" ,vectorStartJson,vectorEndJson,duration.ToString())));
        var data = Recvall();
        if (data.Equals("Ok"))
            return;
        HandleErrors(data);
    }

    public void SwipeAndWait(UnityEngine.Vector2 start, UnityEngine.Vector2 end, float duration)
    {
        Swipe(start, end, duration);
        System.Threading.Thread.Sleep((int)duration * 1000);
        string data;
        do
        {
            Socket.Client.Send(toBytes(CreateCommand("swipeFinished")));
            data = Recvall();
        } while (data == "No");
        if (data.Equals("Yes"))
            return;
        HandleErrors(data);
    }
    public void HoldButton(UnityEngine.Vector2 position, float duration)
    {
        Swipe(position, position, duration);
    }

    public void HoldButtonAndWait(UnityEngine.Vector2 position, float duration)
    {
        SwipeAndWait(position, position, duration);
    }
    public AltUnityObject TapScreen(float x, float y)
    {
        Socket.Client.Send(toBytes(CreateCommand("tapScreen", x.ToString(), y.ToString() )));
        string data = Recvall();
        if (!data.Contains("error:")) return Newtonsoft.Json.JsonConvert.DeserializeObject<AltUnityObject>(data);
        if (data.Contains("error:notFound")) return null;
        HandleErrors(data);
        return null;
    }
    

    public void Tilt(UnityEngine.Vector3 acceleration)
    {
        string accelerationString = Newtonsoft.Json.JsonConvert.SerializeObject(acceleration, Newtonsoft.Json.Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings
        {
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
        });
        Socket.Client.Send(toBytes(CreateCommand("tilt",accelerationString)));
        string data = Recvall();
        if (data.Equals("OK")) return;
        HandleErrors(data);


    }

    public AltUnityObject FindElementWhereNameContains(string name, string cameraName = "", bool enabled = true)
    {
        Socket.Client.Send(toBytes(CreateCommand("findObjectWhereNameContains",name,cameraName,enabled.ToString() )));
        string data = Recvall();
        if (!data.Contains("error:"))
        {
            AltUnityObject altElement = Newtonsoft.Json.JsonConvert.DeserializeObject<AltUnityObject>(data);
            if (altElement.name.Contains(name))
            {
                return altElement;
            }
        }
        HandleErrors(data);
        return null;

    }

    public System.Collections.Generic.List<AltUnityObject> GetAllElements(string cameraName = "", bool enabled = true)
    {
        Socket.Client.Send(toBytes(CreateCommand("findAllObjects",cameraName,enabled.ToString())));
        string data = Recvall();
        if (!data.Contains("error:")) return Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<AltUnityObject>>(data);
        HandleErrors(data);
        return null;

    }

    public AltUnityObject FindElement(string name, string cameraName = "", bool enabled = true)
    {
        Socket.Client.Send(toBytes(CreateCommand("findObjectByName",name,cameraName,enabled.ToString())));
        string data = Recvall();
        if (!data.Contains("error:"))
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AltUnityObject>(data);

        }
        HandleErrors(data);
        return null;
    }

    public System.Collections.Generic.List<AltUnityObject> FindElements(string name, string cameraName = "", bool enabled = true)
    {
        Socket.Client.Send(toBytes(CreateCommand("findObjectsByName", name, cameraName, enabled.ToString())));
        string data = Recvall();
        if (!data.Contains("error:")) return Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<AltUnityObject>>(data);
        HandleErrors(data);
        return null;
    }

    public System.Collections.Generic.List<AltUnityObject> FindElementsWhereNameContains(string name, string cameraName = "", bool enabled = true)
    {
        Socket.Client.Send(toBytes(CreateCommand("findObjectsWhereNameContains",name ,cameraName ,enabled.ToString() )));
        string data = Recvall();
        if (!data.Contains("error:")) return Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<AltUnityObject>>(data);
        HandleErrors(data);
        return null;
    }



    public string WaitForCurrentSceneToBe(string sceneName, double timeout = 10, double interval = 1)
    {
        double time = 0;
        string currentScene = "";
        while (time < timeout)
        {
            currentScene = GetCurrentScene();
            if (!currentScene.Equals(sceneName))
            {
                UnityEngine.Debug.Log("Waiting for scene to be " + sceneName + "...");
                System.Threading.Thread.Sleep(System.Convert.ToInt32(interval * 1000));
                time += interval;
            }
            else
            {
                break;
            }
        }

        if (sceneName.Equals(currentScene))
            return currentScene;
        throw new Assets.AltUnityTester.AltUnityDriver.WaitTimeOutException("Scene " + sceneName + " not loaded after " + timeout + " seconds");

    }

    public AltUnityObject WaitForElementWhereNameContains(string name, string cameraName = "", double timeout = 20, double interval = 0.5)
    {
        double time = 0;
        AltUnityObject altElement = null;
        while (time < timeout)
        {
            try
            {
                altElement = FindElementWhereNameContains(name, cameraName);
                break;
            }
            catch (System.Exception)
            {
                UnityEngine.Debug.Log("Waiting for element where name contains " + name + "....");
                System.Threading.Thread.Sleep(System.Convert.ToInt32(interval * 1000));
                time += interval;
            }
        }
        if (altElement != null)
            return altElement;
        throw new Assets.AltUnityTester.AltUnityDriver.WaitTimeOutException("Element " + name + " still not found after " + timeout + " seconds");

    }



    public void WaitForElementToNotBePresent(string name, string cameraName = "", double timeout = 20, double interval = 0.5)
    {
        double time = 0;
        bool found = false; 
        AltUnityObject altElement = null;
        while (time <= timeout)
        {
            found = false;
            try
            {
                altElement = FindElement(name, cameraName);
                found = true;
                System.Threading.Thread.Sleep(System.Convert.ToInt32(interval * 1000));
                time += interval;
                UnityEngine.Debug.Log("Waiting for element " + name + " to not be present");
            }
            catch (System.Exception)
            {
                break;
            }

        }

        if (found)
            throw new Assets.AltUnityTester.AltUnityDriver.WaitTimeOutException("Element " + name + " still found after " + timeout + " seconds");
    }



    public AltUnityObject WaitForElement(string name, string cameraName = "", double timeout = 20, double interval = 0.5)
    {
        double time = 0;
        AltUnityObject altElement = null;
        while (time < timeout)
        {
            try
            {
                altElement = FindElement(name, cameraName);
                break;
            }
            catch (System.Exception)
            {
                System.Threading.Thread.Sleep(System.Convert.ToInt32(interval * 1000));
                time += interval;
                UnityEngine.Debug.Log("Waiting for element " + name + "...");
            }

        }

        if (altElement != null)
        {
            return altElement;
        }
        throw new Assets.AltUnityTester.AltUnityDriver.WaitTimeOutException("Element " + name + " not loaded after " + timeout + " seconds");
    }


    public AltUnityObject WaitForElementWithText(string name, string text, string cameraName = "", double timeout = 20, double interval = 0.5)
    {
        double time = 0;
        AltUnityObject altElement = null;
        while (time < timeout)
        {
            try
            {
                altElement = FindElement(name, cameraName);
                if (altElement.GetText().Equals(text))
                    break;
                throw new System.Exception("Not the wanted text");
            }
            catch (System.Exception)
            {
                System.Threading.Thread.Sleep(System.Convert.ToInt32(interval * 1000));
                time += interval;
                UnityEngine.Debug.Log("Waiting for element " + name + " to have text " + text);
            }
        }
        if (altElement != null && altElement.GetText().Equals(text))
        {
            return altElement;
        }
        throw new Assets.AltUnityTester.AltUnityDriver.WaitTimeOutException("Element with text: " + text + " not loaded after " + timeout + " seconds");
    }

    public AltUnityObject FindElementByComponent(string componentName, string assemblyName = "", string cameraName = "", bool enabled = true)
    {
        Socket.Client.Send(toBytes(CreateCommand("findObjectByComponent",assemblyName,componentName,cameraName,enabled.ToString() )));
        string data = Recvall();
        if (!data.Contains("error:"))
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AltUnityObject>(data);
        }
        HandleErrors(data);
        return null;
    }
  
    public System.Collections.Generic.List<AltUnityObject> FindElementsByComponent(string componentName, string assemblyName = "", string cameraName = "", bool enabled = true)
    {
        Socket.Client.Send(toBytes(CreateCommand("findObjectsByComponent", assemblyName, componentName, cameraName, enabled.ToString())));
        string data = Recvall();
        if (!data.Contains("error:")) return Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<AltUnityObject>>(data);
        HandleErrors(data);
        return null;
    }

    public System.Collections.Generic.List<string> GetAllScenes()
    {
        Socket.Client.Send(toBytes(CreateCommand("getAllScenes")));
        string data = Recvall();
        if (!data.Contains("error:")) return Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<string>>(data);
        HandleErrors(data);
        return null;
    }
    
    public System.Collections.Generic.List<string> GetAllCameras()
    {
        Socket.Client.Send(toBytes(CreateCommand("getAllCameras")));
        string data = Recvall();
        if (!data.Contains("error:")) return Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<string>>(data);
        HandleErrors(data);
        return null;
    }

    public TextureInformation ReceiveImage() {

        var data = Recvall();
     
        string[] screenshotInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(data);

        // Some workaround this: https://stackoverflow.com/questions/710853/base64-string-throwing-invalid-character-error
        var screenshotParts = screenshotInfo[4].Split('\0');
        screenshotInfo[4] = "";
        for (int i = 0; i < screenshotParts.Length; i++) {
            screenshotInfo[4] += screenshotParts[i];
        }

        var scaleDifference = screenshotInfo[0];    

        var length = screenshotInfo[1];
        var LongLength = Newtonsoft.Json.JsonConvert.DeserializeObject<long>(length);
        var textureFormatString = screenshotInfo[2];
        var textureFormat = (UnityEngine.TextureFormat)System.Enum.Parse(typeof(UnityEngine.TextureFormat), textureFormatString);
        var textSizeString = screenshotInfo[3];
        var textSizeVector3 = Newtonsoft.Json.JsonConvert.DeserializeObject<UnityEngine.Vector3>(textSizeString);

        System.Byte[] imageCompressed = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Byte[]>(screenshotInfo[4], new Newtonsoft.Json.JsonSerializerSettings
        {
            StringEscapeHandling = Newtonsoft.Json.StringEscapeHandling.EscapeNonAscii
        });

        System.Byte[] imageDecompressed=DeCompressScreenshot(imageCompressed);
        return new TextureInformation(imageDecompressed, Newtonsoft.Json.JsonConvert.DeserializeObject<UnityEngine.Vector2>(scaleDifference), textSizeVector3, textureFormat);
    }

    public TextureInformation GetScreenshot(UnityEngine.Vector2 size=default(UnityEngine.Vector2))
    {
        var sizeSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(size);
        Socket.Client.Send(toBytes(CreateCommand("getScreenshot",sizeSerialized)));
        return ReceiveImage();
    }
    public TextureInformation GetScreenshot(int id, UnityEngine.Color color,float width,UnityEngine.Vector2 size = default(UnityEngine.Vector2))
    {
        var sizeSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(size);
        var colorAndWidth = color.r + "!!" + color.g + "!!" + color.b + "!!" + color.a + "!-!" + width;
        Socket.Client.Send(toBytes(CreateCommand("hightlightObjectScreenshot",id.ToString(),colorAndWidth,sizeSerialized)));
        return ReceiveImage();
    }
    public TextureInformation GetScreenshot(UnityEngine.Vector2 coordinates, UnityEngine.Color color, float width, UnityEngine.Vector2 size = default(UnityEngine.Vector2))
    {
        var coordinatesSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(coordinates);
        var sizeSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(size);
        var colorAndWidth = color.r+"!!" + color.g + "!!" + color.b + "!!" + color.a + "!-!" + width;
        Socket.Client.Send(toBytes(CreateCommand("hightlightObjectFromCoordinatesScreenshot",coordinatesSerialized, colorAndWidth, sizeSerialized )));
        return ReceiveImage();
        
    }

    public static void HandleErrors(string data)
    {

        var typeOfException = data.Split(';')[0];
        switch (typeOfException)
        {
            case "error:notFound":
                throw new Assets.AltUnityTester.AltUnityDriver.NotFoundException(data);
            case "error:propertyNotFound":
                throw new Assets.AltUnityTester.AltUnityDriver.PropertyNotFoundException(data);
            case "error:methodNotFound":
                throw new Assets.AltUnityTester.AltUnityDriver.MethodNotFoundException(data);
            case "error:componentNotFound":
                throw new Assets.AltUnityTester.AltUnityDriver.ComponentNotFoundException(data);
            case "error:couldNotPerformOperation":
                throw new Assets.AltUnityTester.AltUnityDriver.CouldNotPerformOperationException(data);
            case "error:couldNotParseJsonString":
                throw new Assets.AltUnityTester.AltUnityDriver.CouldNotParseJsonStringException(data);
            case "error:incorrectNumberOfParameters":
                throw new Assets.AltUnityTester.AltUnityDriver.IncorrectNumberOfParametersException(data);
            case "error:failedToParseMethodArguments":
                throw new Assets.AltUnityTester.AltUnityDriver.FailedToParseArgumentsException(data);
            case "error:objectNotFound":
                throw new Assets.AltUnityTester.AltUnityDriver.ObjectWasNotFoundException(data);
            case "error:propertyCannotBeSet":
                throw new Assets.AltUnityTester.AltUnityDriver.PropertyNotFoundException(data);
            case "error:nullRefferenceException":
                throw new Assets.AltUnityTester.AltUnityDriver.NullRefferenceException(data);
            case "error:unknownError":
                throw new Assets.AltUnityTester.AltUnityDriver.UnknownErrorException(data);
            case "error:formatException":
                throw new Assets.AltUnityTester.AltUnityDriver.FormatException(data);
        }


    }

     public static byte[] DeCompressScreenshot(byte[] screenshotCompressed)
    {

        using (var memoryStreamInput = new System.IO.MemoryStream(screenshotCompressed))
            using (var memoryStreamOutput = new System.IO.MemoryStream())
            {
                using (var gs = new Unity.IO.Compression.GZipStream(memoryStreamInput, Unity.IO.Compression.CompressionMode.Decompress))
                {
                    CopyTo(gs, memoryStreamOutput);
                }

                return memoryStreamOutput.ToArray();
            }
    }
    public static T[] SubArray<T>( T[] data, int index, long length)
    {
        T[] result = new T[length];
        System.Array.Copy(data, index, result, 0, length);
        return result;
    }
    public static void CopyTo(System.IO.Stream src, System.IO.Stream dest)
    {
        byte[] bytes = new byte[4096];

        int cnt;

        while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
        {
            dest.Write(bytes, 0, cnt);
        }
    }

    public struct TextureInformation
    {
        public byte[] imageData;
        public UnityEngine.Vector2 scaleDifference;
        public UnityEngine.Vector3 textureSize;
        public UnityEngine.TextureFormat textureFormat;

        public TextureInformation(byte[] imageData, UnityEngine.Vector2 scaleDifference, UnityEngine.Vector3 textureSize, UnityEngine.TextureFormat textureFormat)
        {
            this.imageData = imageData;
            this.scaleDifference = scaleDifference;
            this.textureSize = textureSize;
            this.textureFormat = textureFormat;
        }
    }

    
}



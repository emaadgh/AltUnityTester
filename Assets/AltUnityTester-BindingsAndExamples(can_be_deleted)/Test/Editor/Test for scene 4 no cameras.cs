﻿using NUnit.Framework;

public class Testforscene4nocameras
{
    public AltUnityDriver AltUnityDriver;
    //Before any test it connects with the socket
    [OneTimeSetUp]
    public void SetUp()
    {
        AltUnityDriver =new AltUnityDriver();
        AltUnityDriver.LoadScene("Scene 4 No Cameras");
    }

    //At the end of the test closes the connection with the socket
    [OneTimeTearDown]
    public void TearDown()
    {
        AltUnityDriver.Stop();
    }

    [Test]
    public void TestFindElementInASceneWithNoCameras() 
    {
        Assert.AreEqual(0, AltUnityDriver.GetAllCameras().Count);
        var altObject = AltUnityDriver.FindElement("Plane");
        Assert.AreEqual(0, altObject.worldX,"WorldX was: "+ altObject.worldX+" when it should have been 0");
        Assert.AreEqual(0, altObject.worldY, "WorldY was: " + altObject.worldY + " when it should have been 0");
        Assert.AreEqual(0, altObject.worldZ, "WorldZ was: " + altObject.worldZ + " when it should have been 0");
        Assert.AreEqual(-1, altObject.x);
        Assert.AreEqual(-1, altObject.y);
        Assert.AreEqual(-1, altObject.z);
        Assert.AreEqual(-1, altObject.idCamera);
    }


}
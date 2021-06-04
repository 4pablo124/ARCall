using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FirebaseTestSuite
{
    [Test]
    public void FirebaseCompruebaYArreglaDependencias(){
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            Assert.True(dependencyStatus == Firebase.DependencyStatus.Available);
        });
    }


}

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FirebaseTestSuite
{
    [UnityTest]
    public IEnumerator FirebaseCompruebaYArreglaDependencias(){
        var task = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(()=>task.IsCompleted);

        var dependencyStatus = task.Result;
        Assert.True(dependencyStatus == Firebase.DependencyStatus.Available);

    }

    [Test]
    public void FirebaseDatabaseObtieneInstancia(){
        var databaseInstance = FirebaseDatabase.DefaultInstance;
        Assert.IsNotNull(databaseInstance);
    }

    [UnityTest]
    public IEnumerator FirebaseDatabaseLeeDato(){
        var task = FirebaseDatabase.DefaultInstance.GetReference("Tests").Child("Read").GetValueAsync();
        yield return new WaitUntil(()=>task.IsCompleted);
        Assert.AreEqual("Value", task.Result.GetValue(true));

    }

    [UnityTest]
    public IEnumerator FirebaseDatabaseBorraDato(){
        var task = FirebaseDatabase.DefaultInstance.GetReference("Tests").Child("Remove").RemoveValueAsync();
        yield return new WaitUntil(()=>task.IsCompleted);
        
        var task2 = FirebaseDatabase.DefaultInstance.GetReference("Tests").Child("Remove").GetValueAsync();
        yield return new WaitUntil(()=>task2.IsCompleted);
        
        Assert.False(task2.Result.Exists);

        var task3 = FirebaseDatabase.DefaultInstance.GetReference("Tests").Child("Remove").SetValueAsync("Value");
        yield return new WaitUntil(()=>task3.IsCompleted);

    }

    [UnityTest]
    public IEnumerator FirebaseDatabaseEscribeDato(){
        var task = FirebaseDatabase.DefaultInstance.GetReference("Tests").Child("Write").SetValueAsync("Value");
        yield return new WaitUntil(()=>task.IsCompleted);
        
        var task2 = FirebaseDatabase.DefaultInstance.GetReference("Tests").Child("Write").GetValueAsync();
        yield return new WaitUntil(()=>task2.IsCompleted);
        Assert.AreEqual("Value", task2.Result.GetValue(true));

        var task3 = FirebaseDatabase.DefaultInstance.GetReference("Tests").Child("Write").RemoveValueAsync();
        yield return new WaitUntil(()=>task3.IsCompleted);

    }

}

using System.Threading.Tasks;
using Firebase;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestDependenciesSetUp
{
    [AsyncOneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        Debug.Log("Setting up Dependencies");
        await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
        DatabaseManager.Database = Firebase.Database.FirebaseDatabase.DefaultInstance;
        UserManager.LogIn(Firebase.Auth.FirebaseAuth.GetAuth(FirebaseApp.DefaultInstance));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        UserManager.LogOut();
    }

}

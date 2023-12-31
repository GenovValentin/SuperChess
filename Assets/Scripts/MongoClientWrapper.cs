using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using UnityEngine;

public class MongoClientWrapper : MonoBehaviour
{
    private const string MONGO_URI = "mongodb://localhost:27018";

    private const string DATABASE_NAME = "SuperChess";

    private MongoClient client;

    private IMongoDatabase db;

    private IMongoCollection<UserModel> userCollection = null;

    static MongoClientWrapper instance = null;

    public static MongoClientWrapper GetInstance()
    {
        if (instance == null)
        {
            GameObject mongoClientWrapperObject =
                new GameObject("MongoClientWrapper");

            instance =
                mongoClientWrapperObject.AddComponent<MongoClientWrapper>();
        }
        return instance;
    }

    MongoClientWrapper()
    {
        client = new MongoClient(MONGO_URI);
        db = client.GetDatabase(DATABASE_NAME);
        userCollection = db.GetCollection<UserModel>("Players");
    }

    public void CreateUser(string username, string password)
    {
        UserModel user = new UserModel();
        user.Name = username;
        user.ShaPassword = password;
        userCollection.InsertOne (user);
    }

    public bool IsUsernameTaken(string username)
    {
        UserModel modelUser =
            userCollection
                .Find(user => user.Name.Equals(username))
                .SingleOrDefault();
        if (modelUser == null)
        {
            return false;
        }
        return true;
    }

    public bool DoesUserExist(string username, string password)
    {
        UserModel modelUser =
            userCollection
                .Find(user =>
                    user.Name.Equals(username) &&
                    user.ShaPassword.Equals(password))
                .SingleOrDefault();

        if (modelUser == null)
        {
            return false;
        }
        return true;
    }
}

// Unity-mongo-csharp-driver-dlls:
// using MongoDB.Bson;
// using MongoDB.Bson.Serialization.Attributes;
// using MongoDB.Driver;
// private const string MONGO_URI = "mongodb://username:password@127.0.0.1:27017";
// private const string DATABASE_NAME = "testDatabase";
// private MongoClient client;
// private IMongoDatabase db;
// client = new MongoClient(MONGO_URI);
// db = client.GetDatabase(DATABASE_NAME);

// Reference Collection:
// private readonly IMongoCollection<UserModel> userCollection = db.GetCollection<UserModel>("collectionName");

// UserModel Sample
// public class UserModel {
//         public ObjectId _id { set; get; }

//         public int ActiveConnection { set; get; }
//         public string Username { private set; get; }
//         public string Email { private set; get; }
//         public string ShaPassword { private set; get; }

//         //Possible Methods ...
// }

// Get All Models from Collection:
// var modelUser =
// userCollection.Find(user => true).SingleOrDefault();

// Get a Single Model from Collection (or null if no match was found):
// UserModel modelUser = userCollection.Find(user => user._id.Equals(id)).SingleOrDefault();

// Insert One Document to Collection (Post):
// userCollection.InsertOne(newModelUser);

// Replace One Document in Collection (Update):
// userCollection.FindOneAndReplace(user => user._id == newModelUser._id, newModelUser);

// Update Many Documents in Collection:
// // on all users that have a ActiveConnection different from 0, set ActiveConnection to 0
// userCollection.UpdateMany(user => user.ActiveConnection != 0, Builders<Model_User>.Update.Set(user => user.ActiveConnection, 0));

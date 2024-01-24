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

    public void CreateUser(string username, string password, float volume)
    {
        UserModel user = new UserModel();
        user.name = username;
        user.shaPassword = password;
        user.settings = new Settings();
        user.settings.volume = volume;
        userCollection.InsertOne (user);
    }

    public bool IsUsernameTaken(string username)
    {
        UserModel user =
            userCollection
                .Find(user => user.name.Equals(username))
                .SingleOrDefault();

        return user != null;
    }

    public bool DoesUserExist(string username, string password)
    {
        UserModel user =
            userCollection
                .Find(user =>
                    user.name.Equals(username) &&
                    user.shaPassword.Equals(password))
                .SingleOrDefault();

        return user != null;
    }

    public void SetUserVolume(string username, float volume)
    {
        var filter = Builders<UserModel>.Filter.Eq(u => u.name, username);
        var update =
            Builders<UserModel>.Update.Set(u => u.settings.volume, volume);

        userCollection.UpdateOne (filter, update);
    }

    public Settings GetUserSettings(string username)
    {
        UserModel user =
            userCollection
                .Find(user => user.name.Equals(username))
                .SingleOrDefault();

        return user.settings;
    }

    public int GetUserRating(string username)
    {
        UserModel user =
            userCollection
                .Find(user => user.name.Equals(username))
                .SingleOrDefault();
        return user.rating;
    }

    public void SetUserRating(string username, int rating)
    {
        var filter = Builders<UserModel>.Filter.Eq(u => u.name, username);
        var update = Builders<UserModel>.Update.Set(u => u.rating, rating);

        userCollection.UpdateOne (filter, update);
    }

    public void ChangeUsername(string username, string newUsername)
    {
        var filter = Builders<UserModel>.Filter.Eq(u => u.name, username);
        var update = Builders<UserModel>.Update.Set(u => u.name, newUsername);

        userCollection.UpdateOne (filter, update);
    }

    public void DeleteUser(string username)
    {
        var filter = Builders<UserModel>.Filter.Eq(u => u.name, username);
        userCollection.DeleteOne (filter);
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
// var user =
// userCollection.Find(user => true).SingleOrDefault();

// Get a Single Model from Collection (or null if no match was found):
// UserModel user = userCollection.Find(user => user._id.Equals(id)).SingleOrDefault();

// Insert One Document to Collection (Post):
// userCollection.InsertOne(newModelUser);

// Replace One Document in Collection (Update):
// userCollection.FindOneAndReplace(user => user._id == newModelUser._id, newModelUser);

// Update Many Documents in Collection:
// // on all users that have a ActiveConnection different from 0, set ActiveConnection to 0
// userCollection.UpdateMany(user => user.ActiveConnection != 0, Builders<UserModel>.Update.Set(user => user.ActiveConnection, 0));

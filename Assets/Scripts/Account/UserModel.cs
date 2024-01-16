using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using UnityEngine;

public class UserModel
{
    public ObjectId _id { set; get; }

    public int activeConnection { set; get; }

    public string name;

    public string shaPassword;

    public Settings settings;
}

public class Settings
{
    public float volume;
}

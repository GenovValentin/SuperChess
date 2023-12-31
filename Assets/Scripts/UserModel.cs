using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using UnityEngine;

public class UserModel
{
    public ObjectId _id { set; get; }

    public int ActiveConnection { set; get; }

    public string Name;

    public string ShaPassword;
}

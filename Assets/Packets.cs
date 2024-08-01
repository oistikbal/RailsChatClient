using System;
using UnityEngine;

[Serializable]
public abstract class BasePackage
{
    public string Type { get; set; }
}

[Serializable]
public class ConfirmSubcriptionPackage: BasePackage
{
    public string message;
}
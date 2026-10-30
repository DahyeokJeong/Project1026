using System;

[Serializable]
[ExcelSheet("Augment")]
public class AugmentData
{
    public string Id;
    public string ConfigKey;
    public string AttackConfigKey;
    public int Value;
}

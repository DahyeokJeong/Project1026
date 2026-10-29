using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class AugmentLoader : IDataLoader
{
    public List<AugmentData> Augments = new();

    public IList GetDataList()
    {
        return Augments;
    }
}

using System;
using System.Collections.Generic;

public class AugmentSelector
{
    public static bool TrySelect(IEnumerable<AugmentData> source, Func<string, bool> canApply, int count, out List<AugmentData> choices)
    {
        choices = new List<AugmentData>();
        if (source == null || canApply == null || count <= 0)
            return false;

        List<AugmentData> candidates = new();
        HashSet<string> ids = new();

        foreach(AugmentData data in source)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.Id))
                continue;
            if(!canApply(data.Id) || !ids.Add(data.Id))
                continue;

            candidates.Add(data);
        }

        if(candidates.Count < count)
            return false;

        for(int i = 0; i < count; ++i)
        {
            int randomIndex = UnityEngine.Random.Range(i, candidates.Count);

            AugmentData selected = candidates[randomIndex];
            candidates[randomIndex] = candidates[i];
            candidates[i] = selected;

            choices.Add(selected);
        }

        return true;
    }
}

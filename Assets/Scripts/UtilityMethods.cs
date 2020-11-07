using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class UtilityMethods
{
    public static float GetTotalHeightOfChildren(RectTransform rectTransform)
    {
        var totalHeight = 0f;

        foreach (RectTransform childRectT in rectTransform)
        {
            totalHeight += childRectT.sizeDelta.y;
        }

        return totalHeight;
    }

    public static bool EssentialUoFilesExist(List<string> files)
    {
        int mul = 0, idx = 0, enu = 0, def = 0, txt = 0;
        files.ForEach(x =>
        {
            var fileNameLowerCase = x.ToLowerInvariant();
            if (fileNameLowerCase.Contains(".mul"))
                ++mul;
            else if (fileNameLowerCase.Contains(".idx"))
                ++idx;
            else if (fileNameLowerCase.Contains(".enu"))
                ++enu;
            else if (fileNameLowerCase.Contains(".def"))
                ++def;
            else if (fileNameLowerCase.Contains(".txt"))
                ++txt;
        });
        return mul > 40 && idx > 5 && enu > 10 && def > 14 && txt > 14;
    }
}
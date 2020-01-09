using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PanelTypes
{
    RaceOver,
    RaceOver2,
    RaceOver3,
    MainHud
}

public class InGameHudManager : HUDManager<PanelTypes>
{
    private void Start()
    {
    }

    public void ShowRaceOverScreen()
    {
        GameObject go = null;
        ShowPanel(PanelTypes.RaceOver, new object[] { 3, go });
    }
    public void HideRaceOverScreen()
    {
        HideTopPanel();
    }
}

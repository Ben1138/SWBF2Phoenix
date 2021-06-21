using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhxHUD : PhxMenuInterface
{
    PhxGameMatch Match => PhxGameRuntime.GetMatch();

    [Header("References")]
    public PhxUIMap Map;
    public Text ReinforcementTeam1;
    public Text ReinforcementTeam2;


    public override void Clear()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(Map != null);
        Debug.Assert(ReinforcementTeam1 != null);
        Debug.Assert(ReinforcementTeam2 != null);

        ReinforcementTeam1.color = Match.GetTeamColor(1);
        ReinforcementTeam2.color = Match.GetTeamColor(2);
    }

    // Update is called once per frame
    void Update()
    {
        ReinforcementTeam1.text = Match.GetReinforcementCount(1).ToString();
        ReinforcementTeam2.text = Match.GetReinforcementCount(2).ToString();

        if (Match.Player.Pawn != null)
        {
            Vector3 playerPos = Match.Player.Pawn.GetInstance().transform.position;
            Map.MapOffset.x = -playerPos.x;
            Map.MapOffset.y = -playerPos.z;
        }
    }
}

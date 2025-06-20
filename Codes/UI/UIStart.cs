using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public partial class UIStart : _UIBase
{
    public override void OnStart()
    {
        base.OnStart();

        btnStart.onClick.AddListener(onclick_btnStart);

    }
    public override void OnShow()
    {
        base.OnShow();

    }
    private async void onclick_btnStart()
    {
        var scene = await Game.world.AddChild<BattleScene>("Scene/Land");
        await UIHelper.Create<UIBattle>(scene);
        await UIHelper.Create<UICosts>(scene);
        await UIHelper.Create<UIBlood>(scene);
        
        scene.LoadBattle();

        Entity.FastCall(Events.OnBattleReady);
        CloseMe();
    }

}

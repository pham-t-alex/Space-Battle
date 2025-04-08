using UnityEngine;

public class MoneyGenT3R : MoneyGenerator
{
    [SerializeField] private float incomeMultiplierChange;

    public override void OnBuild()
    {
        base.OnBuild();
        MoneyController.Instance.ChangeIncomeMultiplier(player.PlayerNum, incomeMultiplierChange);
    }

    public override void OnSell()
    {
        base.OnSell();
        MoneyController.Instance.ChangeIncomeMultiplier(player.PlayerNum, -incomeMultiplierChange);
    }

    public override void OnUpgrade()
    {
        base.OnUpgrade();
        MoneyController.Instance.ChangeIncomeMultiplier(player.PlayerNum, -incomeMultiplierChange);
    }
}

using UnityEngine;

public class MoneyGenT4R : MoneyGenT3R
{
    [SerializeField] private float incomeRateMultiplierChange;

    public override void OnBuild()
    {
        base.OnBuild();
        MoneyController.Instance.ChangeIncomeRateMultiplier(player.PlayerNum, incomeRateMultiplierChange);
    }

    public override void OnSell()
    {
        base.OnSell();
        MoneyController.Instance.ChangeIncomeRateMultiplier(player.PlayerNum, -incomeRateMultiplierChange);
    }

    public override void OnUpgrade()
    {
        base.OnUpgrade();
        MoneyController.Instance.ChangeIncomeRateMultiplier(player.PlayerNum, -incomeRateMultiplierChange);
    }
}

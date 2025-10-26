public class PartridgeMovement : tMovement
{
    void Update()
    {
        if (!CheckCanMove()) { return; }
        PerformStandartMovement();
    }
}

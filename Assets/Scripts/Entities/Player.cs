namespace PEC1.Entities
{
    public class Player
    {
        public readonly int PlayerNumber;

        public readonly Tank Tank;

        public Player(int playerNumber, Tank tank)
        {
            PlayerNumber = playerNumber;
            Tank = tank;
        }
    }
}

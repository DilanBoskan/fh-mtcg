namespace Application.Game;
public class RandomNumberGenerator : IRandomNumberGenerator {
    public int Next(int max) => _random.Next(max);

    private Random _random = new Random();
}

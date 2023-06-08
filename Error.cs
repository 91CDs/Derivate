namespace nineT1CD;

public static partial class Derivate
{
    public static void Error(string message, int position)
    {
        Console.WriteLine($"Error: {message} in position {position}. \n");
        Environment.Exit(0);
    }
}
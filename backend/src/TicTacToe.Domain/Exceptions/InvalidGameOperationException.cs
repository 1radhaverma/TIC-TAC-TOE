namespace TicTacToe.Domain.Exceptions;


public class InvalidGameOperationException : Exception
{
    public InvalidGameOperationException(string message) : base(message)
    {
    }
}

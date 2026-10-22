namespace Core.Domain;

public sealed record StockMovement(
    string ProductId, 
    int Delta, 
    string OperationType);
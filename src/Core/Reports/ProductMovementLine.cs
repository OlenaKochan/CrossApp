namespace Core.Reports;
// додаткове 1 : DTO для звичайного Join
public sealed record ProductMovementLine(
    string Sku, 
    string Name, 
    int Delta, 
    string OperationType);
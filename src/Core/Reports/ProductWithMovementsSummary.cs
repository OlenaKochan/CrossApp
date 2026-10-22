namespace Core.Reports;
// додаткове 1: DTO для GroupJoin
public sealed record ProductWithMovementsSummary(
    string Sku, 
    string Name, 
    int MoveCount, 
    int TotalDelta);
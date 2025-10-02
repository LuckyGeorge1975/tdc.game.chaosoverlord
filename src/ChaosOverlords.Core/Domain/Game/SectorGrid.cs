using System;

namespace ChaosOverlords.Core.Domain.Game;

/// <summary>
/// Utility helpers for parsing map sector identifiers and evaluating adjacency.
/// </summary>
public static class SectorGrid
{
    public static bool AreAdjacent(string? sourceSectorId, string? targetSectorId)
    {
        if (string.IsNullOrWhiteSpace(sourceSectorId) || string.IsNullOrWhiteSpace(targetSectorId))
        {
            return false;
        }

        if (!TryParse(sourceSectorId, out var sourceColumn, out var sourceRow) ||
            !TryParse(targetSectorId, out var targetColumn, out var targetRow))
        {
            return false;
        }

        var columnDelta = Math.Abs(sourceColumn - targetColumn);
        var rowDelta = Math.Abs(sourceRow - targetRow);
        // Adjacent if target is one step away in any direction (orthogonal or diagonal),
        // but not the same cell.
        return (columnDelta | rowDelta) != 0 && Math.Max(columnDelta, rowDelta) == 1;
    }

    public static bool TryParse(string sectorId, out int column, out int row)
    {
        column = 0;
        row = 0;

        if (string.IsNullOrWhiteSpace(sectorId))
        {
            return false;
        }

        sectorId = sectorId.Trim();
        if (sectorId.Length < 2)
        {
            return false;
        }

        var columnChar = char.ToUpperInvariant(sectorId[0]);
        if (columnChar < 'A' || columnChar > 'Z')
        {
            return false;
        }

        if (!int.TryParse(sectorId[1..], out var parsedRow))
        {
            return false;
        }

        column = columnChar - 'A';
        row = parsedRow - 1;
        return row >= 0;
    }
}

using Microsoft.AspNetCore.Components;
using PKHeX.Core;
using PKMDS_Type_Chart.Services;

namespace PKMDS_Type_Chart.Pages;

public partial class Home
{
    private string _selectedGameId = "SV";
    private MoveType? _attackType;
    private MoveType? _defendType1;
    private MoveType? _defendType2;

    private TypeEra CurrentEra => TypeChartService.GetEra(_selectedGameId);
    private IReadOnlyList<MoveType> ValidTypes => TypeChartService.GetValidTypes(CurrentEra);

    private void OnGameChanged(ChangeEventArgs e)
    {
        _selectedGameId = e.Value?.ToString() ?? "SV";
        var valid = ValidTypes;
        if (_attackType.HasValue && !valid.Contains(_attackType.Value))
        {
            _attackType = null;
        }

        if (_defendType1.HasValue && !valid.Contains(_defendType1.Value))
        {
            _defendType1 = null;
        }

        if (_defendType2.HasValue && !valid.Contains(_defendType2.Value))
        {
            _defendType2 = null;
        }
    }

    private void SelectAttackType(MoveType type)
        => _attackType = _attackType == type
            ? null
            : type;

    private void SelectDefendType1(MoveType type)
        => _defendType1 = _defendType1 == type
            ? null
            : type;

    private void SelectDefendType2(MoveType type)
        => _defendType2 = _defendType2 == type
            ? null
            : type;

    private static (string Multiplier, string Label, string Description, string CssClass) GetResultInfo(float eff) => eff switch
    {
        0f => ("×0", "No Effect", "have no effect on", "tc-result-immune"),
        0.25f => ("×¼", "Not Very Effective", "are not very effective against", "tc-result-nve"),
        0.5f => ("×½", "Not Very Effective", "are not very effective against", "tc-result-nve"),
        2f => ("×2", "Super Effective!", "are super effective against", "tc-result-se"),
        4f => ("×4", "Super Effective!!", "are extremely effective against", "tc-result-4se"),
        _ => ("×1", "Normal", "deal normal damage to", "tc-result-normal"),
    };

    private static string GetDescription(MoveType attacker, string defenderLabel, (string Multiplier, string Label, string Description, string CssClass) info)
        => $"{attacker}-type moves {info.Description} {defenderLabel}-type Pokémon.";
}

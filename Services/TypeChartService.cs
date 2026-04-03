using PKHeX.Core;

namespace PKMDS_Type_Chart.Services;

public enum TypeEra
{
    Gen1,
    Gen2to5,
    Gen6Plus
}

public record GameEntry(string Id, string DisplayName, TypeEra Era);

public record GameGroup(string Name, IReadOnlyList<GameEntry> Games);

public static class TypeChartService
{
    // Type index constants matching MoveType enum values
    private const int N = 0, Fi = 1, Fl = 2, P = 3, G = 4, R = 5;
    private const int B = 6, Gh = 7, St = 8, Fr = 9, W = 10, Gr = 11;
    private const int E = 12, Ps = 13, I = 14, Dr = 15, Dk = 16, Fa = 17;

    private static readonly float[,] _gen6PlusChart = BuildGen6PlusChart();
    private static readonly float[,] _gen2to5Chart = BuildGen2to5Chart();
    private static readonly float[,] _gen1Chart = BuildGen1Chart();

    private static readonly IReadOnlyList<MoveType> Gen1Types =
    [
        MoveType.Normal, MoveType.Fighting, MoveType.Flying, MoveType.Poison,
        MoveType.Ground, MoveType.Rock, MoveType.Bug, MoveType.Ghost,
        MoveType.Fire, MoveType.Water, MoveType.Grass, MoveType.Electric,
        MoveType.Psychic, MoveType.Ice, MoveType.Dragon
    ];

    private static readonly IReadOnlyList<MoveType> Gen2to5Types =
    [
        MoveType.Normal, MoveType.Fighting, MoveType.Flying, MoveType.Poison,
        MoveType.Ground, MoveType.Rock, MoveType.Bug, MoveType.Ghost,
        MoveType.Steel, MoveType.Fire, MoveType.Water, MoveType.Grass,
        MoveType.Electric, MoveType.Psychic, MoveType.Ice, MoveType.Dragon,
        MoveType.Dark
    ];

    private static readonly IReadOnlyList<MoveType> AllTypes =
    [
        MoveType.Normal, MoveType.Fighting, MoveType.Flying, MoveType.Poison,
        MoveType.Ground, MoveType.Rock, MoveType.Bug, MoveType.Ghost,
        MoveType.Steel, MoveType.Fire, MoveType.Water, MoveType.Grass,
        MoveType.Electric, MoveType.Psychic, MoveType.Ice, MoveType.Dragon,
        MoveType.Dark, MoveType.Fairy
    ];

    public static readonly IReadOnlyDictionary<MoveType, string> TypeColors = new Dictionary<MoveType, string>
    {
        [MoveType.Normal] = "#A8A878",
        [MoveType.Fighting] = "#C03028",
        [MoveType.Flying] = "#A890F0",
        [MoveType.Poison] = "#A040A0",
        [MoveType.Ground] = "#E0C068",
        [MoveType.Rock] = "#B8A038",
        [MoveType.Bug] = "#A8B820",
        [MoveType.Ghost] = "#705898",
        [MoveType.Steel] = "#B8B8D0",
        [MoveType.Fire] = "#F08030",
        [MoveType.Water] = "#6890F0",
        [MoveType.Grass] = "#78C850",
        [MoveType.Electric] = "#F8D030",
        [MoveType.Psychic] = "#F85888",
        [MoveType.Ice] = "#98D8D8",
        [MoveType.Dragon] = "#7038F8",
        [MoveType.Dark] = "#705848",
        [MoveType.Fairy] = "#EE99AC"
    };

    public static readonly IReadOnlyList<GameGroup> GameGroups =
    [
        new("Generation I", [
            new("RBY", "Red / Blue / Yellow", TypeEra.Gen1),
            new("Stadium", "Stadium", TypeEra.Gen1)
        ]),
        new("Generation II", [
            new("GSC", "Gold / Silver / Crystal", TypeEra.Gen2to5),
            new("Stadium2", "Stadium 2", TypeEra.Gen2to5)
        ]),
        new("Generation III", [
            new("RSE", "Ruby / Sapphire / Emerald", TypeEra.Gen2to5),
            new("FRLG", "FireRed / LeafGreen", TypeEra.Gen2to5),
            new("Colo", "Colosseum", TypeEra.Gen2to5),
            new("XD", "XD: Gale of Darkness", TypeEra.Gen2to5)
        ]),
        new("Generation IV", [
            new("DPPt", "Diamond / Pearl / Platinum", TypeEra.Gen2to5),
            new("HGSS", "HeartGold / SoulSilver", TypeEra.Gen2to5)
        ]),
        new("Generation V", [
            new("BW", "Black / White", TypeEra.Gen2to5),
            new("BW2", "Black 2 / White 2", TypeEra.Gen2to5)
        ]),
        new("Generation VI", [
            new("XY", "X / Y", TypeEra.Gen6Plus),
            new("ORAS", "Omega Ruby / Alpha Sapphire", TypeEra.Gen6Plus)
        ]),
        new("Generation VII", [
            new("SM", "Sun / Moon", TypeEra.Gen6Plus),
            new("USUM", "Ultra Sun / Ultra Moon", TypeEra.Gen6Plus),
            new("LGPE", "Let's Go Pikachu / Eevee", TypeEra.Gen6Plus)
        ]),
        new("Generation VIII", [
            new("SwSh", "Sword / Shield", TypeEra.Gen6Plus),
            new("BDSP", "Brilliant Diamond / Shining Pearl", TypeEra.Gen6Plus),
            new("PLA", "Legends: Arceus", TypeEra.Gen6Plus)
        ]),
        new("Generation IX", [
            new("SV", "Scarlet / Violet", TypeEra.Gen6Plus),
            new("ZA", "Legends: Z-A", TypeEra.Gen6Plus)
        ]),
        new("Other", [
            new("Champions", "Champions", TypeEra.Gen6Plus)
        ])
    ];

    public static TypeEra GetEra(string gameId)
    {
        foreach (var group in GameGroups)
        {
            foreach (var game in group.Games)
            {
                if (game.Id == gameId)
                {
                    return game.Era;
                }
            }
        }

        return TypeEra.Gen6Plus;
    }

    public static IReadOnlyList<MoveType> GetValidTypes(TypeEra era) => era switch
    {
        TypeEra.Gen1 => Gen1Types,
        TypeEra.Gen2to5 => Gen2to5Types,
        _ => AllTypes
    };

    /// <summary>
    /// Returns the damage multiplier for an attacking type against a defending Pokémon with one or two types.
    /// </summary>
    public static float GetEffectiveness(MoveType attacker, MoveType type1, MoveType? type2, TypeEra era)
    {
        var chart = era switch
        {
            TypeEra.Gen1 => _gen1Chart,
            TypeEra.Gen2to5 => _gen2to5Chart,
            _ => _gen6PlusChart
        };

        var a = (int)attacker;
        var eff = chart[a, (int)type1];
        if (type2.HasValue)
        {
            eff *= chart[a, (int)type2.Value];
        }

        return eff;
    }

    private static float[,] BuildGen6PlusChart()
    {
        var c = new float[18, 18];
        for (var i = 0; i < 18; i++)
        {
            for (var j = 0; j < 18; j++)
            {
                c[i, j] = 1f;
            }
        }

        // Normal defending
        c[Fi, N] = 2f;
        c[Gh, N] = 0f;

        // Fighting defending
        c[Fl, Fi] = 2f;
        c[Ps, Fi] = 2f;
        c[Fa, Fi] = 2f;
        c[R, Fi] = .5f;
        c[B, Fi] = .5f;
        c[Dk, Fi] = .5f;

        // Flying defending
        c[E, Fl] = 2f;
        c[I, Fl] = 2f;
        c[R, Fl] = 2f;
        c[Fi, Fl] = .5f;
        c[B, Fl] = .5f;
        c[Gr, Fl] = .5f;
        c[G, Fl] = 0f;

        // Poison defending
        c[G, P] = 2f;
        c[Ps, P] = 2f;
        c[Fi, P] = .5f;
        c[P, P] = .5f;
        c[B, P] = .5f;
        c[Gr, P] = .5f;
        c[Fa, P] = .5f;

        // Ground defending
        c[Gr, G] = 2f;
        c[I, G] = 2f;
        c[W, G] = 2f;
        c[P, G] = .5f;
        c[R, G] = .5f;
        c[E, G] = 0f;

        // Rock defending
        c[Fi, R] = 2f;
        c[Gr, R] = 2f;
        c[G, R] = 2f;
        c[St, R] = 2f;
        c[W, R] = 2f;
        c[Fr, R] = .5f;
        c[Fl, R] = .5f;
        c[N, R] = .5f;
        c[P, R] = .5f;

        // Bug defending
        c[Fr, B] = 2f;
        c[Fl, B] = 2f;
        c[R, B] = 2f;
        c[Fi, B] = .5f;
        c[Gr, B] = .5f;
        c[G, B] = .5f;

        // Ghost defending
        c[Dk, Gh] = 2f;
        c[Gh, Gh] = 2f;
        c[B, Gh] = .5f;
        c[P, Gh] = .5f;
        c[N, Gh] = 0f;
        c[Fi, Gh] = 0f;

        // Steel defending (Gen 6+: does NOT resist Ghost or Dark)
        c[Fi, St] = 2f;
        c[Fr, St] = 2f;
        c[G, St] = 2f;
        c[B, St] = .5f;
        c[Dr, St] = .5f;
        c[Fa, St] = .5f;
        c[Fl, St] = .5f;
        c[Gr, St] = .5f;
        c[I, St] = .5f;
        c[N, St] = .5f;
        c[Ps, St] = .5f;
        c[R, St] = .5f;
        c[St, St] = .5f;
        c[P, St] = 0f;

        // Fire defending
        c[G, Fr] = 2f;
        c[R, Fr] = 2f;
        c[W, Fr] = 2f;
        c[B, Fr] = .5f;
        c[Fa, Fr] = .5f;
        c[Fr, Fr] = .5f;
        c[Gr, Fr] = .5f;
        c[I, Fr] = .5f;
        c[St, Fr] = .5f;

        // Water defending
        c[E, W] = 2f;
        c[Gr, W] = 2f;
        c[Fr, W] = .5f;
        c[I, W] = .5f;
        c[St, W] = .5f;
        c[W, W] = .5f;

        // Grass defending
        c[B, Gr] = 2f;
        c[Fr, Gr] = 2f;
        c[Fl, Gr] = 2f;
        c[I, Gr] = 2f;
        c[P, Gr] = 2f;
        c[E, Gr] = .5f;
        c[Gr, Gr] = .5f;
        c[G, Gr] = .5f;
        c[W, Gr] = .5f;

        // Electric defending
        c[G, E] = 2f;
        c[E, E] = .5f;
        c[Fl, E] = .5f;
        c[St, E] = .5f;

        // Psychic defending
        c[B, Ps] = 2f;
        c[Dk, Ps] = 2f;
        c[Gh, Ps] = 2f;
        c[Fi, Ps] = .5f;
        c[Ps, Ps] = .5f;

        // Ice defending
        c[Fi, I] = 2f;
        c[Fr, I] = 2f;
        c[R, I] = 2f;
        c[St, I] = 2f;
        c[I, I] = .5f;

        // Dragon defending
        c[Dr, Dr] = 2f;
        c[Fa, Dr] = 2f;
        c[I, Dr] = 2f;
        c[E, Dr] = .5f;
        c[Fr, Dr] = .5f;
        c[Gr, Dr] = .5f;
        c[W, Dr] = .5f;

        // Dark defending
        c[B, Dk] = 2f;
        c[Fa, Dk] = 2f;
        c[Fi, Dk] = 2f;
        c[Dk, Dk] = .5f;
        c[Gh, Dk] = .5f;
        c[Ps, Dk] = 0f;

        // Fairy defending
        c[P, Fa] = 2f;
        c[St, Fa] = 2f;
        c[B, Fa] = .5f;
        c[Dk, Fa] = .5f;
        c[Fi, Fa] = .5f;
        c[Dr, Fa] = 0f;

        return c;
    }

    private static float[,] BuildGen2to5Chart()
    {
        // Gen 2–5: Steel gained resistances to Ghost and Dark that it lost in Gen 6
        var c = BuildGen6PlusChart();
        c[Gh, St] = .5f;
        c[Dk, St] = .5f;
        return c;
    }

    private static float[,] BuildGen1Chart()
    {
        // Gen 1 differences from Gen 2+:
        //   • Poison is super effective vs. Bug (2×)
        //   • Bug is super effective vs. Poison (2×)
        //   • Ghost has no effect on Psychic (game bug — intended 2×)
        //   • Ice is neutral vs. Fire (Fire did not resist Ice)
        var c = BuildGen2to5Chart();
        c[P, B] = 2f; // Poison → Bug: 2×
        c[B, P] = 2f; // Bug → Poison: 2×
        c[Gh, Ps] = 0f; // Ghost → Psychic: 0× (immune)
        c[I, Fr] = 1f; // Ice → Fire: 1× (neutral)
        return c;
    }
}

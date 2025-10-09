using System;

namespace Lost.PortForwarding;

/// <summary>
/// Mapping lifetime
/// </summary>
public readonly struct MappingLifetime
{
	internal int Seconds { get; }
	/// <summary>
	/// Creates a mapping lifetime with a specified duration.
	/// </summary>
	/// <param name="lifetime">Duration of the mapping</param>
	/// <exception cref="ArgumentOutOfRangeException">The value of <paramref name="lifetime"/> is out of range.</exception>
	public MappingLifetime(TimeSpan lifetime)
	{
		Seconds = checked((int)lifetime.TotalSeconds);
		if (Seconds <= 0)
			throw new ArgumentOutOfRangeException(nameof(lifetime), "Lifetime must be at least one second (rounded down to whole seconds).");
		if (Seconds == int.MaxValue)
			throw new ArgumentOutOfRangeException(nameof(lifetime), $"Use {nameof(MappingLifetime)}.{nameof(Session)}");
	}

	internal MappingLifetime(int lifetime)
	{
		Seconds = lifetime;
	}

	/// <summary>
	/// Creates a mapping lifetime with a specified duration.
	/// </summary>
	public static implicit operator MappingLifetime(TimeSpan lifetime) => new(lifetime);

	/// <summary>
	/// Keeps mapping alive until program exits (behavior undefined if program crashes).
	/// </summary>
	public static MappingLifetime Session => new(int.MaxValue);
	/// <summary>
	/// Mapping lifetime is permanent. It will stay open until explicitly deleted.
	/// </summary>
	public static MappingLifetime Permanent => new(0);

	/// <inheritdoc/>
	public override readonly string ToString() => Type switch
	{
		MappingLifetimeType.Permanent => "Permanent",
		MappingLifetimeType.Session => "Session",
		_ => TimeSpan.FromSeconds(Seconds).ToString("g"),
	};

	internal readonly MappingLifetimeType Type => Seconds switch
	{
		0 => MappingLifetimeType.Permanent,
		int.MaxValue => MappingLifetimeType.Session,
		_ => MappingLifetimeType.Manual,
	};
}

namespace CommonLibraryTests;

public class TestConfig
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public NestedConfig Nested { get; set; } = new NestedConfig();
}

public class NestedConfig
{
    public bool IsEnabled { get; set; }
    public string Description { get; set; } = string.Empty;
}

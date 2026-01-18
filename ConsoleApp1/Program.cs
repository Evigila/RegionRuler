namespace ConsoleApp1;

internal class Program
{
    #region Main

    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }

    #endregion Main

    #region MyRegion

    // The region name "MyRegion" will be classified as "Warning" because it does not follow RegionRule rules.
    // "MyRegion"块名字将被分类为“警告”，因为它不符合RegionRule规则。
    public static void Greet()
    {
        Console.WriteLine("Hello!");
    }

    #endregion MyRegion

    #region Helpers

    // The region name "Helpers" will be available because it is included in the allowed_regions list.
    // To add custom allowed regions, modify the .editorconfig file.
    // "Helpers"块名字将被允许，因为它包含在allowed_regions列表中。
    // 要添加自定义允许的区域，请修改.editorconfig文件。
    public static int Add(int a, int b)
    {
        return a + b;
    }

    #endregion Helpers
}
namespace AppwriteMigrator.Utils;
public static class ListUtils
{
    public static bool AreStringListsDifferent(List<string> list1, List<string> list2)
    {
        var diff1 = list1.Except(list2).ToList();

        var diff2 = list2.Except(list1).ToList();

        return diff1.Count != 0 || diff2.Count != 0;
    }
}
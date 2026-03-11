public static class NameGenerator
{
    private static string[] adjectives =
    {
        "용감한",
        "신비로운",
        "배고픈",
        "빛나는",
        "강력한",
        "졸린",
        "수상한",
    };
    private static string[] nouns = { "타워", "수호자", "전사", "궁수", "마법사", "기사" };

    public static string Generate()
    {
        string adj = adjectives[UnityEngine.Random.Range(0, adjectives.Length)];
        string noun = nouns[UnityEngine.Random.Range(0, nouns.Length)];


        return $"{adj} {noun}";
    }
}
//Program.cs

Console.WriteLine("Crawler");

for (int i = 0; i < 10; i++)
{ 
    Crawler crawler = new Crawler("https://en.wikipedia.org/wiki/Butter",
        "https://en.wikipedia.org/wiki/Special:Random", 3);
    crawler.Run();
    Console.WriteLine("XD");
}

using System.Collections.Concurrent;
using System.Net;
using System.Xml.Linq;
using System.IO;

public class Crawler
{
    private string FirstAddress { set; get; }
    private string SearchedAddress { set; get; }
    private int MaxDepth { set; get; }
    private static bool _foundAddress;
    private static ConcurrentQueue<LinkInfo> _concurrentQueue;

    public Crawler(string searchedAddress, string firstAddress, int maxDepth)
    {
        SearchedAddress = searchedAddress;
        FirstAddress = firstAddress;
        MaxDepth = maxDepth;
        _foundAddress = false;
        _concurrentQueue = new ConcurrentQueue<LinkInfo>();
    }

    public void Run()
    {
        if (FirstAddress.Equals(SearchedAddress))
        {
            WriteResults(new LinkInfo(FirstAddress, 0));
            return;
        }

        _concurrentQueue.Enqueue(new LinkInfo(FirstAddress, 0));

        Task[] tasks = new Task[15];

        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = new Task(() => SearchTroughAddresses());
            tasks[i].Start();
        }

        Task.WaitAll(tasks);

    }

    public void SearchTroughAddresses()
    {
        while (_foundAddress == false)
        {
            WebClient web = new();

            LinkInfo currentInfo;
            if (_concurrentQueue.TryDequeue(out currentInfo))
            {
                if (currentInfo.depth >= MaxDepth)
                {
                    Console.WriteLine("Couldn't find the link");
                    _foundAddress = true;
                    break;
                }

                try
                {
                    Stream stream = web.OpenRead(currentInfo.adress);

                    if (stream == null)
                    {
                        continue;
                    }

                    using (StreamReader reader = new(stream))
                    {
                        String articleBody = reader.ReadToEnd();

                        var listOfAddresses = XElement.Parse(articleBody).Descendants("a")
                            .Select(tag => tag?.Attribute("href")?.Value)
                            .Where(link => link != null).Where(link => link?.IndexOf("/wiki/") == 0);
                        var listOfWikipediaAddresses =
                            listOfAddresses.Select(link => "https://en.wikipedia.org" + link).ToList();

                        foreach (var wikipediaAddress in listOfWikipediaAddresses)
                        {
                            LinkInfo newLinkInfo = new LinkInfo(wikipediaAddress, currentInfo.depth + 1);
                            if (newLinkInfo.adress.Equals(SearchedAddress))
                            {
                                WriteResults(currentInfo);
                                _foundAddress = true;
                                break;
                            }

                            _concurrentQueue.Enqueue(newLinkInfo);
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
            else
            {
                Thread.Sleep(2);
            }
        }
    }

    public void WriteResults(LinkInfo foundLink)
    {
        if (_foundAddress) return;
        Console.WriteLine("Found " + foundLink.adress + " at depth: " + foundLink.depth);
    }
}
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebScraperPrimary.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using CsvHelper;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebScraperPrimary.DAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


string primaryurlappend = "/user/ur54236545/ratings?ref_=nv_usr_rt_4";
string baseurl = "https://www.imdb.com";
ScrapingDataWrite(primaryurlappend, baseurl);


static async Task<string> CallUrl(string fullUrl)
{
    HttpClient client = new HttpClient();
    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
    client.DefaultRequestHeaders.Accept.Clear();
    var response = client.GetStringAsync(fullUrl);
    return await response;
}

void ScrapingDataWrite(string primaryurlappend, string baseurl)
{
    for (int i = 0; i < 13; i++)
    {
        string pageurl = baseurl + primaryurlappend;
        var response = CallUrl(pageurl).Result;

        var moviedictionary = ParseHtml(response);
        WriteToCsv(moviedictionary);

        HtmlDocument htmlDoc = new();
        htmlDoc.LoadHtml(response);
        primaryurlappend = htmlDoc.DocumentNode.SelectSingleNode(".//a[@class='flat-button lister-page-next next-page']")
            .GetAttributeValue("href", string.Empty);
    }
}

Dictionary<string, List<string>> ParseHtml(string html)
{
    HtmlDocument htmlDoc = new HtmlDocument();
    htmlDoc.LoadHtml(html);
    var programmerLinks = htmlDoc.DocumentNode.Descendants("div")
            .Where(node => node.GetAttributeValue("class", "").Contains("lister-item-content")).ToList();

    Dictionary<string, List<string>> parentdictionary = new Dictionary<string, List<string>>();

    List<string> titles = new List<string>();
    List<string> ratings = new List<string>();
    List<string> userratings = new List<string>();

    foreach (var link in programmerLinks)
    {
        var titlenode = link.SelectSingleNode(".//h3");
        string titlename = titlenode.SelectSingleNode(".//a").InnerText;
        titlename = titlename.Replace(',', ';');

        string rating = link.SelectNodes(".//span[@class='ipl-rating-star__rating']")[0].InnerText;
        string userrating = link.SelectNodes(".//span[@class='ipl-rating-star__rating']")[1].InnerText;

        titles.Add(titlename);
        ratings.Add(rating);
        userratings.Add(userrating);
    }

    parentdictionary.Add("titlelist", titles);
    parentdictionary.Add("ratinglist", ratings);
    parentdictionary.Add("userratinglist", userratings);

    return parentdictionary;
}

void WriteToCsv(Dictionary<string, List<string>> links)
{
    var csv = new StringBuilder();
    List<string> DataFeederSize = links["titlelist"];

    MovieContext _context = new MovieContext();
    _context.Database.EnsureCreated();

    for (int i = 0; i < DataFeederSize.Count; i++)
    {
        var newLine = string.Format("{0},{1},{2}", links["titlelist"][i], links["ratinglist"][i], links["userratinglist"][i]);
        csv.AppendLine(newLine);
        var moviemaster = new MovieMaster { MovieName = links["titlelist"][i], Userrating = links["ratinglist"][i], Rating = links["userratinglist"][i] };
        _context.MovieMaster.Add(moviemaster);
    }
    _context.SaveChanges();
    System.IO.File.AppendAllText("links.csv", csv.ToString());
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

//builder.Services.AddDbContext<WebScraperPrimaryContext>(options =>
//  options.UseNpgsql(builder.Configuration.GetConnectionString("WebScraperPrimaryContext")));


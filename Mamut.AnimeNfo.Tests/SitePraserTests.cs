﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mamut.AnimeNfo.Contract;
using NUnit.Framework;
using FluentAssertions;

namespace Mamut.AnimeNfo.Tests
{
    [TestFixture]
    public class SitePraserTests
    {
        private static readonly Regex _animeDetailsTableRegex = new Regex(@"<table[^>]*class=""anime_info""[^>]*>.+?</table>", RegexOptions.Singleline );
        private static readonly string _animeDetailsPage = new StreamReader(Path.Combine("Files", "AnimeDetailsPage.html")).ReadToEnd();
        private static readonly string _studioValue = @"<a href=""animestudio,524,nggzzl,seven.html"">
		Seven</a><br />";

        private static readonly Dictionary<string, string> _expectedMatchedGroups = new Dictionary<string, string>
            {
                {"Title", "Ai Mai Mi"},
                {"Japanese Title", "あいまいみー"},
                {"Official Site", "http://www.takeshobo.co.jp/sp/tv_aimaimi/"},
                {"Category", "TV"},
                {"Total Episodes", "-"},
                {"Genres", "-"},
                {"Year Published", "2013"},
                {"Release Date", "2013-01-03 &sim;"},
                {"Broadcaster", "-"},
                {"Studio", _studioValue},
                {"US Distribution", ""},
                {"User Rating", "N/A"},
                {"Updated", "Tue, 25 Dec 2012 16:51:07 -0500"}
            };

        [Test]
        public void ShouldMatchAnimeTable()
        {
            var match = _animeDetailsTableRegex.Match(_animeDetailsPage);

            match.Success.Should().BeTrue();
            match.Value.Should().Contain(@"<img class=""float"" src=""image/anime_5246.jpg"" alt=""Ai Mai Mi"" />");
            match.Value.Should().Contain("Title");
            match.Value.Should().Contain("Japanese Title");
            match.Value.Should().Contain("Official Site");
            match.Value.Should().Contain("Category");
            match.Value.Should().Contain("Total Episodes");
            match.Value.Should().Contain("Genres");
            match.Value.Should().Contain("Year Published");
            match.Value.Should().Contain("Release Date");
            match.Value.Should().Contain("Broadcaster");
            match.Value.Should().Contain("Studio");
            match.Value.Should().Contain("US Distribution");
            match.Value.Should().Contain("User Rating");
            match.Value.Should().Contain("Updated"); 
        }

        [Test]
        public void ShouldMatchImageSource()
        {
            var animeDetailsTable = _animeDetailsTableRegex.Match(_animeDetailsPage).Value;
            var imageRegex = new Regex("src=\"(?<link>.*?)\"");
            imageRegex.Match(animeDetailsTable).Groups["link"].Value.Should().Be("image/anime_5246.jpg");
        }

        [Test]
        public void ShouldMatchAnimeTextualData()
        {
            var animeDetailsTable = _animeDetailsTableRegex.Match(_animeDetailsPage).Value;
            var textualDataRegex = new Regex("<td.*?<b>(?<key>.*?)</b>.*?<td.*?>((<a.*?>(?<value>.*?)</a>)|(?<value>.*?))</td>", RegexOptions.Singleline);
            var matchCollection = textualDataRegex.Matches(animeDetailsTable);

            _expectedMatchedGroups.Count.Should().Be(matchCollection.Count);
            _expectedMatchedGroups.Keys.All(k =>
                {
                    var single = matchCollection.Cast<Match>().Single(m => m.Groups["key"].Value == k);
                    return _expectedMatchedGroups[k] == single.Groups["value"].Value;
                })
                .Should()
                .BeTrue();

        }

        [Test]
        public void SiteParserShouldProcessAnime()
        {
            Anime anime = Services.SitePraser.animeFromPage(_animeDetailsPage);

            anime.Title.Should().Be(_expectedMatchedGroups["Title"]);
            anime.JapaneseTitle.Should().Be(_expectedMatchedGroups["Japanese Title"]);
            anime.OfficialSite.Should().Be(_expectedMatchedGroups["Official Site"]);
            anime.Category.Should().Be(_expectedMatchedGroups["Category"]);
            anime.TotalEpisodes.Should().Be(_expectedMatchedGroups["Total Episodes"]);
            anime.Genres.Should().Be(_expectedMatchedGroups["Genres"]);
            anime.YearPublished.Should().Be(_expectedMatchedGroups["Year Published"]);
            anime.ReleaseDate.Should().Be(_expectedMatchedGroups["Release Date"]);
            anime.Broadcaster.Should().Be(_expectedMatchedGroups["Broadcaster"]);
            anime.Studio.Should().Be(_expectedMatchedGroups["Studio"]);
            anime.USDistribution.Should().Be(_expectedMatchedGroups["US Distribution"]);
            anime.UserRating.Should().Be(_expectedMatchedGroups["User Rating"]);
            anime.Updated.Should().Be(_expectedMatchedGroups["Updated"]);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        string dataDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Load league setup and teams
        var leagueSetup = CsvLoader.Load<LeagueSetup>(Path.Combine(dataDirectory, "setup.csv")).FirstOrDefault();
        if (leagueSetup != null){

            Console.WriteLine($"League Name: {leagueSetup.LeagueName}");
            Console.WriteLine($"Champions League Positions: {leagueSetup.ChampionsLeaguePositions}");
            Console.WriteLine($"Europa League Positions: {leagueSetup.EuropaLeaguePositions}");
            Console.WriteLine($"Conference League Positions: {leagueSetup.ConferenceLeaguePositions}");
            Console.WriteLine($"Promotion Position: {leagueSetup.PromotionPositions}");
            Console.WriteLine($"Relegation Position: {leagueSetup.RelegationPositions}");
        }
        else{
            Console.WriteLine("The collection is empty or the data is not formatted correctly.");
        }
        var teams = CsvLoader.Load<Team>(Path.Combine(dataDirectory, "teams.csv"));

        // Initialize standings
        PrintStandings(teams, leagueSetup);

        // Process each round
        int roundNumber = 1;
        while (File.Exists(Path.Combine(dataDirectory, $"round-{roundNumber}.csv"))){
            var roundResults = CsvLoader.Load<MatchResult>(Path.Combine(dataDirectory, $"round-{roundNumber}.csv"));
            ProcessRound(teams, roundResults);
            PrintStandings(teams, leagueSetup);
            roundNumber++;
        }
    }

    private static void ProcessRound(List<Team> teams, List<MatchResult> roundResults)
    {
        foreach (var result in roundResults)
        {
            var homeTeam = teams.First(t => t.Abbreviation == result.HomeTeam);
            var awayTeam = teams.First(t => t.Abbreviation == result.AwayTeam);

            homeTeam.GamesPlayed++;
            awayTeam.GamesPlayed++;

            homeTeam.GoalsFor += result.HomeGoals;
            homeTeam.GoalsAgainst += result.AwayGoals;

            awayTeam.GoalsFor += result.AwayGoals;
            awayTeam.GoalsAgainst += result.HomeGoals;

            if (result.HomeGoals > result.AwayGoals)
            {
                homeTeam.Wins++;
                awayTeam.Losses++;

                homeTeam.Streak = UpdateStreak(homeTeam.Streak, 'W');
                awayTeam.Streak = UpdateStreak(awayTeam.Streak, 'L');
            }
            else if (result.HomeGoals < result.AwayGoals)
            {
                homeTeam.Losses++;
                awayTeam.Wins++;

                homeTeam.Streak = UpdateStreak(homeTeam.Streak, 'L');
                awayTeam.Streak = UpdateStreak(awayTeam.Streak, 'W');
            }
            else
            {
                homeTeam.Draws++;
                awayTeam.Draws++;

                homeTeam.Streak = UpdateStreak(homeTeam.Streak, 'D');
                awayTeam.Streak = UpdateStreak(awayTeam.Streak, 'D');
            }
        }
    }

    private static string UpdateStreak(string currentStreak, char result)
    {
        currentStreak = currentStreak.Insert(0, result.ToString());
        if (currentStreak.Length > 5)
            currentStreak = currentStreak.Substring(0, 5);

        return currentStreak;
    }

    private static void PrintStandings(List<Team> teams, LeagueSetup leagueSetup)
    {
        Console.WriteLine($"Current Standings for {leagueSetup.LeagueName}:");

        var sortedTeams = teams.OrderByDescending(t => t.Points)
            .ThenByDescending(t => t.GoalDifference)
            .ThenByDescending(t => t.GoalsFor)
            .ThenBy(t => t.GoalsAgainst)
            .ThenBy(t => t.FullName)
            .ToList();

        int currentPosition = 1;
        int previousPosition = 0;
        int samePositionCount = 1;

        for (int i = 0; i < sortedTeams.Count; i++)
        {
            var team = sortedTeams[i];
            if (i > 0 && CompareTeams(sortedTeams[i - 1], team) == 0)
            {
                currentPosition = previousPosition;
                samePositionCount++;
            }
            else
            {
                currentPosition = i + 1;
                samePositionCount = 1;
            }

            string positionString = samePositionCount == 1 ? currentPosition.ToString() : "-";
            Console.WriteLine($"{positionString}\t{team.SpecialRanking}\t{team.FullName}\t{team.GamesPlayed}\t{team.Wins}\t{team.Draws}\t{team.Losses}\t{team.GoalsFor}\t{team.GoalsAgainst}\t{team.GoalDifference}\t{team.Points}\t{team.Streak}");
            previousPosition = currentPosition;
        }

        Console.WriteLine();
    }

    private static int CompareTeams(Team a, Team b)
    {
        if (a.Points != b.Points) return a.Points.CompareTo(b.Points);
        if (a.GoalDifference != b.GoalDifference) return a.GoalDifference.CompareTo(b.GoalDifference);
        if (a.GoalsFor != b.GoalsFor) return a.GoalsFor.CompareTo(b.GoalsFor);
        if (a.GoalsAgainst != b.GoalsAgainst) return a.GoalsAgainst.CompareTo(b.GoalsAgainst);

        return a.FullName.CompareTo(b.FullName);
    }
}

